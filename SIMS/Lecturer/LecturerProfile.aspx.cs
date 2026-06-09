using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;

namespace SIMS.Lecturer
{
    public partial class LecturerProfile : LecturerBase
    {
        string connStr = ConfigurationManager.ConnectionStrings["SIMS_DB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            EnsureAuthenticated();

            if (!IsPostBack)
            {
                LoadProfileData();
            }
        }

        /// <summary>
        /// Encrypts plain text strings into SHA-256 hex hashes to match database standards.
        /// </summary>
        private string ComputeSha256Hash(string rawData)
        {
            if (string.IsNullOrEmpty(rawData)) return string.Empty;

            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private void LoadProfileData()
        {
            try
            {
                int lecturerId = CurrentLecturerId;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = @"
                        SELECT
                            u.UserId,
                            u.FullName,
                            u.Email,
                            u.Phone,
                            u.PhotoUrl,
                            u.CreatedAt,
                            u.LastLoginAt,
                            l.LecturerId,
                            l.StaffNo,
                            l.Department,
                            l.Specialisation,
                            l.EmploymentStatus
                        FROM Users u
                        INNER JOIN Lecturers l ON l.UserId = u.UserId
                        WHERE l.LecturerId = @LecturerId";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@LecturerId", lecturerId);
                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                litFullName.Text = reader["FullName"].ToString();
                                litEmail.Text = reader["Email"].ToString();
                                litStaffNo.Text = reader["StaffNo"].ToString();
                                litDepartment.Text = reader["Department"].ToString();

                                string specialisation = reader["Specialisation"].ToString();
                                litSpecialisation.Text = !string.IsNullOrEmpty(specialisation) ? specialisation : "Not specified";

                                string phone = reader["Phone"].ToString();
                                litPhone.Text = !string.IsNullOrEmpty(phone) ? phone : "Not provided";

                                litEmploymentStatus.Text = reader["EmploymentStatus"].ToString();

                                DateTime createdAt = Convert.ToDateTime(reader["CreatedAt"]);
                                litMemberSince.Text = createdAt.ToString("MMMM dd, yyyy");

                                object lastLogin = reader["LastLoginAt"];
                                if (lastLogin != DBNull.Value)
                                {
                                    DateTime lastLoginDate = Convert.ToDateTime(lastLogin);
                                    litLastLogin.Text = lastLoginDate.ToString("MMMM dd, yyyy h:mm tt");
                                }

                                string photoUrl = reader["PhotoUrl"].ToString();
                                litPhotoStatus.Text = !string.IsNullOrEmpty(photoUrl) ? "Profile picture uploaded" : "No profile picture";

                                txtFullName.Text = reader["FullName"].ToString();
                                txtEmail.Text = reader["Email"].ToString();
                                txtPhone.Text = phone;
                                txtSpecialisation.Text = specialisation;
                                txtStaffNo.Text = reader["StaffNo"].ToString();
                                txtDepartment.Text = reader["Department"].ToString();
                                txtEmploymentStatus.Text = reader["EmploymentStatus"].ToString();
                            }
                            else
                            {
                                ShowError("Profile information not found.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading profile: {ex.Message}");
                ShowError("An error occurred while loading your profile. Please try again.");
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                ShowError("Full name is required.");
                MaintainEditMode();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                ShowError("Email is required.");
                MaintainEditMode();
                return;
            }

            try
            {
                var addr = new System.Net.Mail.MailAddress(txtEmail.Text);
                if (addr.Address != txtEmail.Text)
                {
                    ShowError("Invalid email format.");
                    MaintainEditMode();
                    return;
                }
            }
            catch
            {
                ShowError("Invalid email format.");
                MaintainEditMode();
                return;
            }

            bool isChangingPassword = !string.IsNullOrEmpty(txtCurrentPassword.Text) ||
                                      !string.IsNullOrEmpty(txtNewPassword.Text) ||
                                      !string.IsNullOrEmpty(txtConfirmPassword.Text);

            if (isChangingPassword)
            {
                if (string.IsNullOrEmpty(txtCurrentPassword.Text))
                {
                    ShowError("You must enter your current password to set a new one.");
                    MaintainEditMode();
                    return;
                }
                if (string.IsNullOrEmpty(txtNewPassword.Text))
                {
                    ShowError("New password field cannot be blank.");
                    MaintainEditMode();
                    return;
                }
                if (txtNewPassword.Text != txtConfirmPassword.Text)
                {
                    ShowError("The new password and confirmation password do not match.");
                    MaintainEditMode();
                    return;
                }
            }

            try
            {
                int userId = CurrentUserId;
                int lecturerId = CurrentLecturerId;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string checkEmailSql = "SELECT COUNT(*) FROM Users WHERE Email = @Email AND UserId != @UserId";
                    using (SqlCommand checkCmd = new SqlCommand(checkEmailSql, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                        checkCmd.Parameters.AddWithValue("@UserId", userId);
                        int emailCount = (int)checkCmd.ExecuteScalar();

                        if (emailCount > 0)
                        {
                            ShowError("This email is already in use by another account.");
                            MaintainEditMode();
                            return;
                        }
                    }

                    if (isChangingPassword)
                    {
                        string verifyPassSql = "SELECT PasswordHash FROM Users WHERE UserId = @UserId";
                        string currentDbPasswordHash = "";

                        using (SqlCommand verifyCmd = new SqlCommand(verifyPassSql, conn))
                        {
                            verifyCmd.Parameters.AddWithValue("@UserId", userId);
                            object result = verifyCmd.ExecuteScalar();
                            currentDbPasswordHash = result != null ? result.ToString().Trim() : "";
                        }

                        string inputCurrentPasswordHash = ComputeSha256Hash(txtCurrentPassword.Text);

                        if (!string.Equals(currentDbPasswordHash, inputCurrentPasswordHash, StringComparison.OrdinalIgnoreCase))
                        {
                            ShowError("The current password you entered is incorrect.");
                            MaintainEditMode();
                            return;
                        }

                        string encryptedNewPassword = ComputeSha256Hash(txtNewPassword.Text);

                        string updatePassSql = "UPDATE Users SET PasswordHash = @NewPassword WHERE UserId = @UserId";
                        using (SqlCommand updatePassCmd = new SqlCommand(updatePassSql, conn))
                        {
                            updatePassCmd.Parameters.AddWithValue("@NewPassword", encryptedNewPassword);
                            updatePassCmd.Parameters.AddWithValue("@UserId", userId);
                            updatePassCmd.ExecuteNonQuery();
                        }

                        // AUDIT LOG IMPLEMENTATION: Records credential manipulation history safely
                        string auditSql = @"
                            INSERT INTO [dbo].[AuditLogs] 
                                ([UserId], [Action], [TableAffected], [RecordId], [OldValue], [NewValue], [ActionDate])
                            VALUES 
                                (@UserId, @Action, @TableAffected, @RecordId, @OldValue, @NewValue, SYSUTCDATETIME())";

                        using (SqlCommand auditCmd = new SqlCommand(auditSql, conn))
                        {
                            auditCmd.Parameters.AddWithValue("@UserId", userId);
                            auditCmd.Parameters.AddWithValue("@Action", "Password Changed");
                            auditCmd.Parameters.AddWithValue("@TableAffected", "Users");
                            auditCmd.Parameters.AddWithValue("@RecordId", userId);
                            auditCmd.Parameters.AddWithValue("@OldValue", "******");
                            auditCmd.Parameters.AddWithValue("@NewValue", "******");
                            auditCmd.ExecuteNonQuery();
                        }
                    }

                    string updateUserSql = @"
                        UPDATE Users
                        SET FullName = @FullName,
                            Email = @Email,
                            Phone = @Phone
                        WHERE UserId = @UserId";

                    using (SqlCommand updateUserCmd = new SqlCommand(updateUserSql, conn))
                    {
                        updateUserCmd.Parameters.AddWithValue("@FullName", txtFullName.Text.Trim());
                        updateUserCmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                        updateUserCmd.Parameters.AddWithValue("@Phone", string.IsNullOrWhiteSpace(txtPhone.Text) ? (object)DBNull.Value : txtPhone.Text.Trim());
                        updateUserCmd.Parameters.AddWithValue("@UserId", userId);
                        updateUserCmd.ExecuteNonQuery();
                    }

                    string updateLecturerSql = @"
                        UPDATE Lecturers
                        SET Specialisation = @Specialisation
                        WHERE LecturerId = @LecturerId";

                    using (SqlCommand updateLecturerCmd = new SqlCommand(updateLecturerSql, conn))
                    {
                        updateLecturerCmd.Parameters.AddWithValue("@Specialisation", string.IsNullOrWhiteSpace(txtSpecialisation.Text) ? (object)DBNull.Value : txtSpecialisation.Text.Trim());
                        updateLecturerCmd.Parameters.AddWithValue("@LecturerId", lecturerId);
                        updateLecturerCmd.ExecuteNonQuery();
                    }
                }

                Session["FullName"] = txtFullName.Text.Trim();
                Session["Email"] = txtEmail.Text.Trim();

                ShowSuccess("Profile updated successfully!");

                txtCurrentPassword.Text = "";
                txtNewPassword.Text = "";
                txtConfirmPassword.Text = "";

                LoadProfileData();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating profile: {ex.Message}");
                ShowError("An error occurred while updating your profile. Please try again.");
                MaintainEditMode();
            }
        }

        private void MaintainEditMode()
        {
            Page.ClientScript.RegisterStartupScript(
                this.GetType(),
                "maintainEdit",
                "setInterfaceMode(true);",
                true);
        }

        private void ShowSuccess(string message)
        {
            pnlSuccess.Visible = true;
            litSuccessMsg.Text = message;
            pnlError.Visible = false;
        }

        private void ShowError(string message)
        {
            pnlError.Visible = true;
            litErrorMsg.Text = message;
            pnlSuccess.Visible = false;
        }
    }
}