using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;

namespace SIMS
{
    public partial class Login : Page
    {
        string connStr = ConfigurationManager
            .ConnectionStrings["SIMS_DB"]
            .ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // If already logged in, redirect to appropriate dashboard
            if (Session["UserId"] != null)
            {
                RedirectToDashboard(Session["UserRole"].ToString());
                return;
            }

            if (!IsPostBack)
            {
                // Check if there's a remember-me cookie
                if (Request.Cookies["SIMS_Email"] != null)
                {
                    txtEmail.Text = Request.Cookies["SIMS_Email"].Value;
                }
                
                if (Request.Cookies["SIMS_Role"] != null)
                {
                    ddlRole.SelectedValue = Request.Cookies["SIMS_Role"].Value;
                    chkRememberMe.Checked = true;
                }
            }
        }

        /// <summary>
        /// Handles the login button click event.
        /// Validates credentials for all three user roles and creates appropriate sessions.
        /// </summary>
        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;
            string role = ddlRole.SelectedValue;

            // Validation
            if (string.IsNullOrWhiteSpace(role))
            {
                ShowError("Please select your role.");
                return;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                ShowError("Please enter your email or ID number.");
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowError("Please enter your password.");
                return;
            }

            // Authenticate user based on role
            LoginResult result = AuthenticateUser(email, password, role);

            if (result.IsAuthenticated)
            {
                // Set session variables
                Session["UserId"] = result.UserId;
                Session["UserRole"] = result.UserRole;
                Session["FullName"] = result.FullName;
                Session["Email"] = result.Email;
                Session["RoleId"] = result.RoleId;

                // Role-specific session variables
                switch (result.UserRole.ToLower())
                {
                    case "headofprogramme":
                        Session["HeadOfProgrammeId"] = result.RoleSpecificId;
                        break;
                    case "lecturer":
                        Session["LecturerId"] = result.RoleSpecificId;
                        Session["StaffNo"] = result.StaffNo;
                        Session["Department"] = result.Department;
                        break;
                    case "student":
                        Session["StudentId"] = result.RoleSpecificId;
                        Session["StudentNo"] = result.StaffNo;
                        Session["ProgrammeId"] = result.ProgrammeId;
                        break;
                }

                // Set session timeout (30 minutes)
                Session.Timeout = 30;

                // Handle remember me
                if (chkRememberMe.Checked)
                {
                    HttpCookie emailCookie = new HttpCookie("SIMS_Email", email);
                    emailCookie.Expires = DateTime.Now.AddDays(30);
                    Response.Cookies.Add(emailCookie);

                    HttpCookie roleCookie = new HttpCookie("SIMS_Role", role);
                    roleCookie.Expires = DateTime.Now.AddDays(30);
                    Response.Cookies.Add(roleCookie);
                }
                else
                {
                    // Clear remember me cookies if unchecked
                    ClearRememberMeCookies();
                }

                // Update last login time
                UpdateLastLoginTime(result.UserId);

                // Log login attempt
                LogLoginAttempt(result.UserId, email, true);

                // Redirect to appropriate dashboard
                RedirectToDashboard(result.UserRole);
            }
            else
            {
                ShowError(result.ErrorMessage);
                LogLoginAttempt(0, email, false);
            }
        }

        /// <summary>
        /// Authenticates a user based on their role.
        /// Supports login by email or role-specific ID number.
        /// </summary>
        private LoginResult AuthenticateUser(string emailOrId, string password, string role)
        {
            try
            {
                SqlConnection conn = new SqlConnection(connStr);

                string sql = "";
                string roleId = GetRoleId(role);

                if (string.IsNullOrEmpty(roleId))
                {
                    return new LoginResult
                    {
                        IsAuthenticated = false,
                        ErrorMessage = "Invalid role selected."
                    };
                }

                // Query based on role
                if (role.Equals("HeadOfProgramme", StringComparison.OrdinalIgnoreCase))
                {
                    // Head of Programme authentication
                    sql = @"
                        SELECT
                            u.UserId,
                            u.RoleId,
                            u.FullName,
                            u.Email,
                            u.PasswordHash,
                            u.IsActive,
                            u.LastLoginAt
                        FROM Users u
                        WHERE (u.Email = @EmailOrId)
                          AND u.RoleId = @RoleId
                          AND u.IsActive = 1";
                }
                else if (role.Equals("Lecturer", StringComparison.OrdinalIgnoreCase))
                {
                    // Lecturer authentication
                    sql = @"
                        SELECT
                            u.UserId,
                            u.RoleId,
                            u.FullName,
                            u.Email,
                            u.PasswordHash,
                            u.IsActive,
                            l.LecturerId,
                            l.StaffNo,
                            l.Department,
                            l.EmploymentStatus
                        FROM Users u
                        INNER JOIN Lecturers l ON l.UserId = u.UserId
                        WHERE (u.Email = @EmailOrId OR l.StaffNo = @EmailOrId)
                          AND u.RoleId = @RoleId
                          AND u.IsActive = 1
                          AND l.EmploymentStatus = 'Active'";
                }
                else if (role.Equals("Student", StringComparison.OrdinalIgnoreCase))
                {
                    // Student authentication
                    sql = @"
                        SELECT
                            u.UserId,
                            u.RoleId,
                            u.FullName,
                            u.Email,
                            u.PasswordHash,
                            u.IsActive,
                            s.StudentId,
                            s.StudentNo,
                            s.ProgrammeId,
                            s.Status
                        FROM Users u
                        INNER JOIN Students s ON s.UserId = u.UserId
                        WHERE (u.Email = @EmailOrId OR s.StudentNo = @EmailOrId)
                          AND u.RoleId = @RoleId
                          AND u.IsActive = 1
                          AND s.Status = 'Active'";
                }

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@EmailOrId", emailOrId);
                cmd.Parameters.AddWithValue("@RoleId", roleId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    string storedPasswordHash = reader["PasswordHash"].ToString();

                    // Verify password using multiple hashing methods for compatibility
                    if (VerifyPasswordWithFallback(password, storedPasswordHash))
                    {
                        int userId = Convert.ToInt32(reader["UserId"]);
                        int userRoleId = Convert.ToInt32(reader["RoleId"]);
                        string fullName = reader["FullName"].ToString();
                        string userEmail = reader["Email"].ToString();

                        int roleSpecificId = 0;
                        string staffNo = "";
                        string department = "";
                        int programmeId = 0;

                        if (role.Equals("Lecturer", StringComparison.OrdinalIgnoreCase))
                        {
                            roleSpecificId = Convert.ToInt32(reader["LecturerId"]);
                            staffNo = reader["StaffNo"].ToString();
                            department = reader["Department"].ToString();
                        }
                        else if (role.Equals("Student", StringComparison.OrdinalIgnoreCase))
                        {
                            roleSpecificId = Convert.ToInt32(reader["StudentId"]);
                            staffNo = reader["StudentNo"].ToString();
                            programmeId = Convert.ToInt32(reader["ProgrammeId"]);
                        }

                        conn.Close();

                        return new LoginResult
                        {
                            IsAuthenticated = true,
                            UserId = userId,
                            RoleId = userRoleId,
                            UserRole = role,
                            FullName = fullName,
                            Email = userEmail,
                            RoleSpecificId = roleSpecificId,
                            StaffNo = staffNo,
                            Department = department,
                            ProgrammeId = programmeId,
                            ErrorMessage = ""
                        };
                    }
                    else
                    {
                        conn.Close();
                        return new LoginResult
                        {
                            IsAuthenticated = false,
                            ErrorMessage = "Invalid email/ID or password."
                        };
                    }
                }
                else
                {
                    conn.Close();
                    return new LoginResult
                    {
                        IsAuthenticated = false,
                        ErrorMessage = "Invalid email/ID or password."
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
                return new LoginResult
                {
                    IsAuthenticated = false,
                    ErrorMessage = "An error occurred during login. Please try again."
                };
            }
        }

        /// <summary>
        /// Gets the RoleId from the database based on role name.
        /// </summary>
        private string GetRoleId(string roleName)
        {
            try
            {
                SqlConnection conn = new SqlConnection(connStr);
                string sql = "SELECT RoleId FROM Roles WHERE RoleName = @RoleName";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@RoleName", roleName);

                conn.Open();
                object result = cmd.ExecuteScalar();
                conn.Close();

                return result != null ? result.ToString() : "";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Hashes a password using SHA256 and returns it in hexadecimal format
        /// (matches SQL Server's HASHBYTES('SHA2_256') output).
        /// </summary>
        private string HashPasswordHex(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                // Convert to hexadecimal string to match SQL Server's format
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToUpper();
            }
        }

        /// <summary>
        /// Hashes a password using SHA256 and returns it in Base64 format
        /// (legacy C# format for backward compatibility).
        /// </summary>
        private string HashPasswordBase64(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        /// <summary>
        /// Verifies password against stored hash using multiple methods for compatibility.
        /// Tries hexadecimal format first (SQL Server HASHBYTES), then Base64 (legacy format).
        /// Returns true if either method matches.
        /// </summary>
        private bool VerifyPasswordWithFallback(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(password))
                return false;

            try
            {
                // Method 1: Try SQL Server hexadecimal format (SHA2_256)
                string hashHex = HashPasswordHex(password);
                if (hashHex.Equals(storedHash.ToUpper()))
                {
                    System.Diagnostics.Debug.WriteLine("Password verified using HEX format (SQL Server HASHBYTES)");
                    return true;
                }

                // Method 2: Try Base64 format (legacy C# format)
                string hashBase64 = HashPasswordBase64(password);
                if (hashBase64.Equals(storedHash))
                {
                    System.Diagnostics.Debug.WriteLine("Password verified using BASE64 format (legacy)");
                    return true;
                }

                // Method 3: Handle case where stored hash might have "0x" prefix from SQL Server
                if (storedHash.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    string hashWithoutPrefix = storedHash.Substring(2).ToUpper();
                    if (hashHex.Equals(hashWithoutPrefix))
                    {
                        System.Diagnostics.Debug.WriteLine("Password verified using HEX format with 0x prefix");
                        return true;
                    }
                }

                // Log failed attempts for debugging
                System.Diagnostics.Debug.WriteLine($"Password verification failed.");
                System.Diagnostics.Debug.WriteLine($"Stored hash: {storedHash}");
                System.Diagnostics.Debug.WriteLine($"HEX hash: {hashHex}");
                System.Diagnostics.Debug.WriteLine($"BASE64 hash: {hashBase64}");

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Password verification error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Updates the last login timestamp for the user.
        /// </summary>
        private void UpdateLastLoginTime(int userId)
        {
            try
            {
                SqlConnection conn = new SqlConnection(connStr);
                string sql = "UPDATE Users SET LastLoginAt = GETDATE() WHERE UserId = @UserId";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch
            {
                // Silently fail - this is not critical
            }
        }

        /// <summary>
        /// Logs login attempts for audit purposes.
        /// </summary>
        private void LogLoginAttempt(int userId, string email, bool success)
        {
            try
            {
                SqlConnection conn = new SqlConnection(connStr);

                string sql = @"
                    INSERT INTO LoginAttempts (UserId, Email, IsSuccessful, AttemptDate)
                    VALUES (@UserId, @Email, @IsSuccessful, GETDATE())";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserId", userId > 0 ? (object)userId : DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@IsSuccessful", success ? 1 : 0);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch
            {
                // Silently fail - logging should not break the application
            }
        }

        /// <summary>
        /// Redirects the user to their role-specific dashboard.
        /// </summary>
        private void RedirectToDashboard(string role)
        {
            switch (role.ToLower())
            {
                case "headofprogramme":
                    Response.Redirect("~/HeadOfProgramme/Dashboard.aspx");
                    break;
                case "lecturer":
                    Response.Redirect("~/Lecturer/LecturerDashboard.aspx");
                    break;
                case "student":
                    Response.Redirect("~/Student/Dashboard.aspx");
                    break;
                default:
                    Response.Redirect("~/Login.aspx");
                    break;
            }
        }

        /// <summary>
        /// Clears remember-me cookies.
        /// </summary>
        private void ClearRememberMeCookies()
        {
            if (Request.Cookies["SIMS_Email"] != null)
            {
                HttpCookie cookie = new HttpCookie("SIMS_Email");
                cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(cookie);
            }

            if (Request.Cookies["SIMS_Role"] != null)
            {
                HttpCookie cookie = new HttpCookie("SIMS_Role");
                cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(cookie);
            }
        }

        /// <summary>
        /// Displays error message to the user.
        /// </summary>
        private void ShowError(string message)
        {
            pnlError.Visible = true;
            litErrorMsg.Text = message;
            pnlWarning.Visible = false;
        }

        /// <summary>
        /// Displays warning message to the user.
        /// </summary>
        private void ShowWarning(string message)
        {
            pnlWarning.Visible = true;
            litWarningMsg.Text = message;
            pnlError.Visible = false;
        }
    }

    /// <summary>
    /// Result class for login authentication.
    /// Contains all necessary information after successful authentication.
    /// </summary>
    public class LoginResult
    {
        public bool IsAuthenticated { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string UserRole { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int RoleSpecificId { get; set; }
        public string StaffNo { get; set; }
        public string Department { get; set; }
        public int ProgrammeId { get; set; }
        public string ErrorMessage { get; set; }
    }
}