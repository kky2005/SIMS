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
    public partial class AdmissionLogin : Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["SIMS_DB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (AuthenticationHelper.IsAuthenticated())
            {
                // Applicants do not have a role column in the DB; always redirect authenticated admission users
                RedirectToDashboard("Admission");
                return;
            }

            if (!IsPostBack && Request.Cookies["SIMS_Email"] != null)
            {
                var txtEmail = FindControlRecursive(this, "txtEmail") as System.Web.UI.WebControls.TextBox;
                var chkRememberMe = FindControlRecursive(this, "chkRememberMe") as System.Web.UI.WebControls.CheckBox;
                if (txtEmail != null) txtEmail.Text = Request.Cookies["SIMS_Email"].Value;
                if (chkRememberMe != null) chkRememberMe.Checked = true;
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            var txtEmailCtrl = FindControlRecursive(this, "txtEmail") as System.Web.UI.WebControls.TextBox;
            var txtPasswordCtrl = FindControlRecursive(this, "txtPassword") as System.Web.UI.WebControls.TextBox;

            string email = txtEmailCtrl?.Text.Trim() ?? string.Empty;
            string password = txtPasswordCtrl?.Text ?? string.Empty;

            if (string.IsNullOrWhiteSpace(email))
            {
                ShowError("Please enter your email.");
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowError("Please enter your password.");
                return;
            }

            AdmissionLoginResult result = AuthenticateApplicant(email, password);

            if (result.IsAuthenticated)
            {
                // Only store the minimal applicant authentication details: UserId, Email, AdmissionId
                Session[AuthenticationHelper.SESSION_USER_ID] = result.UserId;
                Session[AuthenticationHelper.SESSION_EMAIL] = result.Email;
                Session[AuthenticationHelper.SESSION_ADMISSION_ID] = result.AdmissionId;

                Session.Timeout = 30;

                var chkRememberMeCtrl = FindControlRecursive(this, "chkRememberMe") as System.Web.UI.WebControls.CheckBox;
                if (chkRememberMeCtrl != null && chkRememberMeCtrl.Checked)
                {
                    HttpCookie emailCookie = new HttpCookie("SIMS_Email", email);
                    emailCookie.Expires = DateTime.Now.AddDays(30);
                    Response.Cookies.Add(emailCookie);
                }
                else
                {
                    ClearRememberMeCookies();
                }

                UpdateLastLoginTimeApplicant(result.UserId);
                LogLoginAttempt(result.UserId, email, true);
                RedirectToDashboard(result.UserRole);
            }
            else
            {
                ShowError(result.ErrorMessage);
                LogLoginAttempt(0, email, false);
            }
        }

        // Authenticate against the Applicants table for this page
        private AdmissionLoginResult AuthenticateApplicant(string emailOrId, string password)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = @"
                        SELECT
                            ap.UserId,
                            ap.Email,
                            ap.PasswordHash,
                            ap.IsActive,
                            ap.AdmissionId
                        FROM dbo.Applicants ap
                    WHERE ap.Email = @EmailOrId
                          AND ap.IsActive = 1";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@EmailOrId", emailOrId);

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string storedPasswordHash = reader["PasswordHash"].ToString();

                        // Verify using existing supported formats (hex, base64, or 0x-prefixed hex)
                        bool verified = VerifyPasswordWithFallback(password, storedPasswordHash);

                        if (verified)
                        {
                        // Primary key in Applicants table is UserId (identity)
                            int userId = Convert.ToInt32(reader["UserId"]);
                            // AdmissionId column may be null; do NOT treat UserId as a substitute for AdmissionId.
                            // Only use AdmissionId when it is present in the Applicants row.
                            int admissionId = 0;
                            if (reader["AdmissionId"] != DBNull.Value)
                            {
                                int tmp;
                                if (int.TryParse(reader["AdmissionId"].ToString(), out tmp)) admissionId = tmp;
                            }
                            string userEmail = reader["Email"].ToString();

                            return new AdmissionLoginResult
                            {
                                IsAuthenticated = true,
                                UserId = userId,
                                AdmissionId = admissionId,
                                FullName = userEmail,
                                Email = userEmail,
                                AdmissionNo = admissionId > 0 ? admissionId.ToString() : string.Empty,
                                UserRole = "Admission",
                                ErrorMessage = string.Empty
                            };
                        }
                    }
                }

                return new AdmissionLoginResult { IsAuthenticated = false, ErrorMessage = "Invalid email/ID or password." };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Admission auth error: {ex.Message}");
                return new AdmissionLoginResult { IsAuthenticated = false, ErrorMessage = "An error occurred during login." };
            }
        }

        private void UpdateLastLoginTimeApplicant(int userId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    // Applicants table primary key is UserId
                    cmd.CommandText = "UPDATE dbo.Applicants SET LastLoginAt = GETDATE() WHERE UserId = @UserId";
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch { }
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

        private void UpdateLastLoginTimeAdmission(int admissionId)
        {
            try
            {
                SqlConnection conn = new SqlConnection(connStr);
                string sql = "UPDATE Admissions SET LastLoginAt = GETDATE() WHERE AdmissionId = @AdmissionId";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@AdmissionId", admissionId);
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
                case "admission":
                    Response.Redirect("~/AdmissionDashboard.aspx");
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
            var pnlError = FindControlRecursive(this, "pnlError") as System.Web.UI.WebControls.Panel;
            var litErrorMsg = FindControlRecursive(this, "litErrorMsg") as System.Web.UI.WebControls.Literal;
            if (pnlError != null) pnlError.Visible = true;
            if (litErrorMsg != null) litErrorMsg.Text = message;
        }

        private System.Web.UI.Control FindControlRecursive(System.Web.UI.Control root, string id)
        {
            if (root == null) return null;
            var c = root.FindControl(id);
            if (c != null) return c;
            foreach (System.Web.UI.Control child in root.Controls)
            {
                var found = FindControlRecursive(child, id);
                if (found != null) return found;
            }
            return null;
        }
    }

    // Result class for admissions-only login
    public class AdmissionLoginResult
    {
        public bool IsAuthenticated { get; set; }
        public int UserId { get; set; }
        public int AdmissionId { get; set; }
        public string AdmissionNo { get; set; }
        public string UserRole { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string ErrorMessage { get; set; }
    }
}

/* Ignore this message*/