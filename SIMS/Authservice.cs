// ============================================================
//  AuthService.cs  –  App_Code/AuthService.cs
//  ============================================================
//  Handles credential verification against SQL Server.
//  Uses parameterised queries; passwords stored as bcrypt hashes.
//
//  Required NuGet: BCrypt.Net-Next
//  Connection string name (Web.config): "SimsDB"
// ============================================================

using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SIMS.App_Code
{
    // ── Return type from ValidateUser ─────────────────────────────────────────
    public class UserLoginResult
    {
        public bool IsSuccess { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }   // "Admin" | "Lecturer" | "Student"
        public int? ProgrammeId { get; set; }
    }

    // ── Service ──────────────────────────────────────────────────────────────
    public static class AuthService
    {
        private static string ConnectionString =>
            ConfigurationManager.ConnectionStrings["SimsDB"].ConnectionString;

        // ── Validate credentials ──────────────────────────────────────────────
        /// <summary>
        /// Looks up the user by e-mail, verifies the bcrypt password hash,
        /// and returns a populated result object.
        /// </summary>
        public static UserLoginResult ValidateUser(string email, string plainPassword)
        {
            var result = new UserLoginResult { IsSuccess = false };

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    const string sql = @"
                        SELECT u.UserId,
                               u.FullName,
                               u.Email,
                               u.PasswordHash,
                               u.IsActive,
                               r.RoleName,
                               u.ProgrammeId
                        FROM   Users  u
                        JOIN   Roles  r ON r.RoleId = u.RoleId
                        WHERE  u.Email = @Email";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 150).Value = email;

                        using (SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            if (!rdr.Read())
                                return result;                  // email not found

                            bool isActive = (bool)rdr["IsActive"];
                            if (!isActive)
                                return result;                  // account disabled

                            string storedHash = rdr["PasswordHash"].ToString();

                            // bcrypt verification
                            bool passwordOk = BCrypt.Net.BCrypt.Verify(plainPassword, storedHash);
                            if (!passwordOk)
                                return result;                  // wrong password

                            result.IsSuccess   = true;
                            result.UserId      = (int)rdr["UserId"];
                            result.FullName    = rdr["FullName"].ToString();
                            result.Email       = rdr["Email"].ToString();
                            result.Role        = rdr["RoleName"].ToString();
                            result.ProgrammeId = rdr["ProgrammeId"] == DBNull.Value
                                                    ? (int?)null
                                                    : (int)rdr["ProgrammeId"];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception using your preferred logger, then return failure
                System.Diagnostics.Debug.WriteLine("[AuthService] " + ex.Message);
            }

            return result;
        }

        // ── Hash a plain-text password (used when creating / updating users) ──
        /// <summary>Returns a bcrypt hash suitable for storing in the database.</summary>
        public static string HashPassword(string plainPassword)
        {
            return BCrypt.Net.BCrypt.HashPassword(plainPassword, workFactor: 12);
        }
    }
}