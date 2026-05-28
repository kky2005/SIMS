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
        string connStr = ConfigurationManager.ConnectionStrings["SIMS_DB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] != null)
            {
                RedirectToDashboard(Session["UserRole"].ToString());
                return;
            }

            if (!IsPostBack && Request.Cookies["SIMS_Email"] != null)
            {
                txtEmail.Text = Request.Cookies["SIMS_Email"].Value;
                chkRememberMe.Checked = true;
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;

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

            LoginResult result = AuthenticateUserAnyRole(email, password);

            if (result.IsAuthenticated)
            {
                Session["UserId"] = result.UserId;
                Session["UserRole"] = result.UserRole;
                Session["FullName"] = result.FullName;
                Session["Email"] = result.Email;
                Session["RoleId"] = result.RoleId;

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

                Session.Timeout = 30;

                if (chkRememberMe.Checked)
                {
                    HttpCookie emailCookie = new HttpCookie("SIMS_Email", email);
                    emailCookie.Expires = DateTime.Now.AddDays(30);
                    Response.Cookies.Add(emailCookie);
                }
                else
                {
                    ClearRememberMeCookies();
                }

                UpdateLastLoginTime(result.UserId);
                LogLoginAttempt(result.UserId, email, true);
                RedirectToDashboard(result.UserRole);
            }
            else
            {
                ShowError(result.ErrorMessage);
                LogLoginAttempt(0, email, false);
            }
        }

        //TEst push

        private LoginResult AuthenticateUserAnyRole(string emailOrId, string password)
        {
            // 1. HOP from config
            var hop = AuthenticateHOPFromConfig(emailOrId, password);
            if (hop.IsAuthenticated) return hop;

            // 2. Lecturer DB
            var result = AuthenticateAsRole(emailOrId, password, "Lecturer");
            if (result.IsAuthenticated) return result;

            // 3. Student DB
            result = AuthenticateAsRole(emailOrId, password, "Student");
            if (result.IsAuthenticated) return result;

            return new LoginResult
            {
                IsAuthenticated = false,
                ErrorMessage = "Invalid email/ID or password."
            };
        }

        private LoginResult AuthenticateHOPFromConfig(string email, string password)
        {
            string hopUsername = ConfigurationManager.AppSettings["HOP_Username"];
            string hopPassword = ConfigurationManager.AppSettings["HOP_Password"];

            if (email == hopUsername && password == hopPassword)
            {
                return new LoginResult
                {
                    IsAuthenticated = true,
                    UserId = 1,
                    RoleId = 1,
                    UserRole = "HeadOfProgramme",
                    FullName = "System Administrator",
                    Email = hopUsername,
                    RoleSpecificId = 1
                };
            }

            return new LoginResult { IsAuthenticated = false };
        }

        private LoginResult AuthenticateAsRole(string emailOrId, string password, string role)
        {
            try
            {
                SqlConnection conn = new SqlConnection(connStr);
                string roleId = GetRoleId(role);

                if (string.IsNullOrEmpty(roleId))
                {
                    return new LoginResult { IsAuthenticated = false, ErrorMessage = "Invalid role selected." };
                }

                string sql = "";

                if (role.Equals("HeadOfProgramme", StringComparison.OrdinalIgnoreCase))
                {
                    sql = @"
                        SELECT
                            u.UserId,
                            u.RoleId,
                            u.FullName,
                            u.Email,
                            u.PasswordHash,
                            u.IsActive
                        FROM Users u
                        WHERE u.Email = @EmailOrId
                          AND u.RoleId = @RoleId
                          AND u.IsActive = 1";
                }
                else if (role.Equals("Lecturer", StringComparison.OrdinalIgnoreCase))
                {
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
                    conn.Close();
                }
                else
                {
                    conn.Close();
                }

                return new LoginResult { IsAuthenticated = false, ErrorMessage = "" };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Auth as {role} error: {ex.Message}");
                return new LoginResult { IsAuthenticated = false, ErrorMessage = "" };
            }
        }

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
            catch { return ""; }
        }

        private string HashPasswordHex(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToUpper();
            }
        }

        private string HashPasswordBase64(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPasswordWithFallback(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(password)) return false;

            try
            {
                string hashHex = HashPasswordHex(password);
                if (hashHex.Equals(storedHash.ToUpper())) return true;

                string hashBase64 = HashPasswordBase64(password);
                if (hashBase64.Equals(storedHash)) return true;

                if (storedHash.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    string hashWithoutPrefix = storedHash.Substring(2).ToUpper();
                    if (hashHex.Equals(hashWithoutPrefix)) return true;
                }

                return false;
            }
            catch { return false; }
        }

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
            catch { }
        }

        private void LogLoginAttempt(int userId, string email, bool success)
        {
            try
            {
                SqlConnection conn = new SqlConnection(connStr);
                string sql = "INSERT INTO LoginAttempts (UserId, Email, IsSuccessful, AttemptDate) VALUES (@UserId, @Email, @IsSuccessful, GETDATE())";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserId", userId > 0 ? (object)userId : DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@IsSuccessful", success ? 1 : 0);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch { }
        }

        private void RedirectToDashboard(string role)
        {
            switch (role.ToLower())
            {
                case "headofprogramme":
                    Response.Redirect("~/HeadOfProgramme/HOPDashboard.aspx");
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

        private void ClearRememberMeCookies()
        {
            if (Request.Cookies["SIMS_Email"] != null)
            {
                HttpCookie cookie = new HttpCookie("SIMS_Email");
                cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(cookie);
            }
        }

        private void ShowError(string message)
        {
            pnlError.Visible = true;
            litErrorMsg.Text = message;
        }
    }

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