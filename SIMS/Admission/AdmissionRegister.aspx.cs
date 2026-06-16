using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace SIMS
{
    public partial class AdmissionRegister : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            // userId is no longer collected; generate or leave null and use Email for login
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;
            string confirm = txtConfirmPassword.Text;

            if (string.IsNullOrWhiteSpace(email))
            {
                ShowError("Please enter your email.");
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowError("Please enter a password.");
                return;
            }

            if (password != confirm)
            {
                ShowError("Passwords do not match.");
                return;
            }

            // Hash password using SHA256 (Base64)
            string passwordHash = HashPasswordBase64(password);

            // Server-side validation of validators
            if (!Page.IsValid)
            {
                // Let the validators show their messages
                return;
            }

            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["SIMS_DB"]?.ConnectionString;
                if (string.IsNullOrEmpty(connStr))
                {
                    ShowError("Configuration error: database connection not found. Please contact support.");
                    System.Diagnostics.Debug.WriteLine("Missing SIMS_DB connection string.");
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    // Only insert Email and PasswordHash; let the ApplicantId identity and other defaults be set by the DB
                    cmd.CommandText = @"INSERT INTO dbo.Applicants (Email, PasswordHash, IsActive, CreatedAt)
                                        VALUES (@Email, @PasswordHash, 1, SYSUTCDATETIME())";

                    // Use explicit parameter types/sizes to avoid surprises (truncation, type mismatch)
                    cmd.Parameters.Add("@Email", System.Data.SqlDbType.NVarChar, 255).Value = email;
                    cmd.Parameters.Add("@PasswordHash", System.Data.SqlDbType.NVarChar, 256).Value = passwordHash;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                // Redirect to login after successful registration
                Response.Redirect("AdmissionLogin.aspx?registered=1", false);
            }
            catch (SqlException sqlEx)
            {
                System.Diagnostics.Debug.WriteLine($"Registration SQL error #{sqlEx.Number}: {sqlEx.Message}");
                if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                {
                    ShowError("User ID or Email already exists. Please choose a different User ID or use another email.");
                }
                else if (sqlEx.Number == 544) // Cannot insert explicit value for identity column
                {
                    // Informative message for schema mismatch / accidental identity insert
                    ShowError("Registration failed: database schema prevents inserting an explicit value for an identity column. Please check the Applicants table schema.");
                    System.Diagnostics.Debug.WriteLine("Hint: Ensure the INSERT lists only non-identity columns (or remove the explicit value for the identity column).");
                }
                else if (sqlEx.Number == 8152) // string or binary data would be truncated
                {
                    ShowError("Input value too long for a database field. Please shorten some fields.");
                }
                else if (sqlEx.Number == 515) // cannot insert the value NULL
                {
                    ShowError("A required field was missing. Please ensure all required fields are provided.");
                }
                else if (sqlEx.Number == 208) // invalid object name
                {
                    ShowError("Database table not found. Please contact support.");
                }
                else
                {
                    ShowError("Database error occurred while registering. Please contact support.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Registration error: {ex.Message}");
                ShowError("An unexpected error occurred while registering.");
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

        // (Using SHA256 Base64 hashing only)

        private void ShowError(string message)
        {
            pnlError.Visible = true;
            litErrorMsg.Text = message;
        }
    }
}
