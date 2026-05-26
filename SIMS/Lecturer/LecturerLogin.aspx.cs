using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;

namespace SIMS.Lecturer
{
    public partial class LecturerLogin : Page
    {
        string connStr = ConfigurationManager
            .ConnectionStrings["SIMS_DB"]
            .ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // If already logged in, redirect to dashboard
            if (Session["LecturerID"] != null)
            {
                Response.Redirect("~/Lecturer/LecturerDashboard.aspx");
                return;
            }

            if (!IsPostBack)
            {
                // Check if there's a remember-me cookie
                if (Request.Cookies["LecturerEmail"] != null)
                {
                    txtEmail.Text = Request.Cookies["LecturerEmail"].Value;
                    chkRememberMe.Checked = true;
                }
            }
        }

        /// <summary>
        /// Handles the login button click event.
        /// Validates credentials and creates a session if successful.
        /// </summary>
        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;

            // Validation
            if (string.IsNullOrWhiteSpace(email))
            {
                ShowError("Please enter your email or staff number.");
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowError("Please enter your password.");
                return;
            }

            // Authenticate user
            LecturerLoginResult result = AuthenticateLecturer(email, password);

            if (result.IsAuthenticated)
            {
                // Set session variables
                Session["LecturerID"] = result.LecturerId;
                Session["UserID"] = result.UserId;
                Session["FullName"] = result.FullName;
                Session["Department"] = result.Department;
                Session["StaffNo"] = result.StaffNo;
                Session["Email"] = result.Email;

                // Set session timeout (30 minutes)
                Session.Timeout = 30;

                // Handle remember me
                if (chkRememberMe.Checked)
                {
                    HttpCookie cookie = new HttpCookie("LecturerEmail", email);
                    cookie.Expires = DateTime.Now.AddDays(30);
                    Response.Cookies.Add(cookie);
                }
                else
                {
                    // Clear remember me cookie if unchecked
                    if (Request.Cookies["LecturerEmail"] != null)
                    {
                        HttpCookie cookie = new HttpCookie("LecturerEmail");
                        cookie.Expires = DateTime.Now.AddDays(-1);
                        Response.Cookies.Add(cookie);
                    }
                }

                // Redirect to dashboard
                Response.Redirect("~/Lecturer/LecturerDashboard.aspx");
            }
            else
            {
                ShowError(result.ErrorMessage);
            }
        }

        /// <summary>
        /// Authenticates a lecturer by checking credentials against the database.
        /// Supports login by email or staff number.
        /// </summary>
        private LecturerLoginResult AuthenticateLecturer(string emailOrStaffNo, string password)
        {
            try
            {
                SqlConnection conn = new SqlConnection(connStr);

                // Query to find lecturer by email or staff number
                string sql = @"
                    SELECT
                        l.LecturerId,
                        u.UserId,
                        u.FullName,
                        u.Email,
                        u.PasswordHash,
                        l.Department,
                        l.StaffNo,
                        l.IsActive
                    FROM Lecturers l
                    INNER JOIN Users u ON u.UserId = l.UserId
                    WHERE (u.Email = @EmailOrStaffNo OR l.StaffNo = @EmailOrStaffNo)
                      AND l.IsActive = 1
                      AND u.IsActive = 1";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@EmailOrStaffNo", emailOrStaffNo);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    string storedPasswordHash = reader["PasswordHash"].ToString();
                    string passwordToCheck = HashPassword(password);

                    // Verify password
                    if (VerifyPassword(password, storedPasswordHash))
                    {
                        int lecturerId = Convert.ToInt32(reader["LecturerId"]);
                        int userId = Convert.ToInt32(reader["UserId"]);
                        string fullName = reader["FullName"].ToString();
                        string email = reader["Email"].ToString();
                        string department = reader["Department"].ToString();
                        string staffNo = reader["StaffNo"].ToString();

                        conn.Close();

                        // Log login activity
                        LogLoginActivity(lecturerId, true);

                        return new LecturerLoginResult
                        {
                            IsAuthenticated = true,
                            LecturerId = lecturerId,
                            UserId = userId,
                            FullName = fullName,
                            Email = email,
                            Department = department,
                            StaffNo = staffNo,
                            ErrorMessage = ""
                        };
                    }
                    else
                    {
                        conn.Close();
                        LogLoginActivity(0, false, emailOrStaffNo, "Invalid password");
                        return new LecturerLoginResult
                        {
                            IsAuthenticated = false,
                            ErrorMessage = "Invalid email/staff number or password."
                        };
                    }
                }
                else
                {
                    conn.Close();
                    LogLoginActivity(0, false, emailOrStaffNo, "User not found");
                    return new LecturerLoginResult
                    {
                        IsAuthenticated = false,
                        ErrorMessage = "Invalid email/staff number or password."
                    };
                }
            }
            catch (Exception ex)
            {
                // Log error but don't expose details to user
                System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
                return new LecturerLoginResult
                {
                    IsAuthenticated = false,
                    ErrorMessage = "An error occurred during login. Please try again."
                };
            }
        }

        /// <summary>
        /// Hashes a password using PBKDF2 (if you're using this method).
        /// Alternatively, use bcrypt or ASP.NET Identity for production.
        /// </summary>
        private string HashPassword(string password)
        {
            // For production, consider using BCrypt or ASP.NET Identity
            // This is a simplified example using SHA256
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        /// <summary>
        /// Verifies if the provided password matches the stored hash.
        /// This is a simplified verification - for production, use bcrypt or ASP.NET Identity.
        /// </summary>
        private bool VerifyPassword(string password, string hash)
        {
            string hashOfInput = HashPassword(password);
            return hashOfInput.Equals(hash);
        }

        /// <summary>
        /// Logs login activities for audit purposes.
        /// </summary>
        private void LogLoginActivity(int lecturerId, bool success, string emailOrStaffNo = "", string remarks = "")
        {
            try
            {
                SqlConnection conn = new SqlConnection(connStr);

                string sql = @"
                    INSERT INTO LoginLogs (LecturerId, LoginAttempt, Success, IPAddress, Timestamp, Remarks)
                    VALUES (@LecturerId, @LoginAttempt, @Success, @IPAddress, GETDATE(), @Remarks)";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@LecturerId", lecturerId > 0 ? lecturerId : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@LoginAttempt", emailOrStaffNo);
                cmd.Parameters.AddWithValue("@Success", success ? 1 : 0);
                cmd.Parameters.AddWithValue("@IPAddress", GetClientIP());
                cmd.Parameters.AddWithValue("@Remarks", string.IsNullOrEmpty(remarks) ? (object)DBNull.Value : remarks);

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
        /// Gets the client's IP address.
        /// </summary>
        private string GetClientIP()
        {
            string clientIP = Request.ServerVariables["HTTP_CF_CONNECTING_IP"];
            if (string.IsNullOrEmpty(clientIP))
            {
                clientIP = Request.ServerVariables["REMOTE_ADDR"];
            }
            return clientIP ?? "Unknown";
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
    /// </summary>
    public class LecturerLoginResult
    {
        public bool IsAuthenticated { get; set; }
        public int LecturerId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public string StaffNo { get; set; }
        public string ErrorMessage { get; set; }
    }
}