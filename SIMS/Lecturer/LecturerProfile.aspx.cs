using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace SIMS.Lecturer
{
    public partial class LecturerProfile : LecturerBase
    {
        string connStr = ConfigurationManager
            .ConnectionStrings["SIMS_DB"]
            .ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Ensure user is authenticated
            EnsureAuthenticated();

            if (!IsPostBack)
            {
                LoadProfileData();
            }
        }

        /// <summary>
        /// Loads the lecturer's profile information from the database.
        /// </summary>
        private void LoadProfileData()
        {
            try
            {
                int lecturerId = CurrentLecturerId;

                SqlConnection conn = new SqlConnection(connStr);

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

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@LecturerId", lecturerId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    // Set view mode literals
                    litFullName.Text = reader["FullName"].ToString();
                    litEmail.Text = reader["Email"].ToString();
                    litStaffNo.Text = reader["StaffNo"].ToString();
                    litDepartment.Text = reader["Department"].ToString();
                    
                    string specialisation = reader["Specialisation"].ToString();
                    if (!string.IsNullOrEmpty(specialisation))
                    {
                        litSpecialisation.Text = specialisation;
                    }

                    string phone = reader["Phone"].ToString();
                    if (!string.IsNullOrEmpty(phone))
                    {
                        litPhone.Text = phone;
                    }

                    litEmploymentStatus.Text = reader["EmploymentStatus"].ToString();

                    // Format dates
                    DateTime createdAt = Convert.ToDateTime(reader["CreatedAt"]);
                    litMemberSince.Text = createdAt.ToString("MMMM dd, yyyy");

                    object lastLogin = reader["LastLoginAt"];
                    if (lastLogin != DBNull.Value)
                    {
                        DateTime lastLoginDate = Convert.ToDateTime(lastLogin);
                        litLastLogin.Text = lastLoginDate.ToString("MMMM dd, yyyy h:mm tt");
                    }

                    // Photo status
                    string photoUrl = reader["PhotoUrl"].ToString();
                    if (!string.IsNullOrEmpty(photoUrl))
                    {
                        litPhotoStatus.Text = "Profile picture uploaded";
                    }

                    // Set edit mode textboxes
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

                conn.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading profile: {ex.Message}");
                ShowError("An error occurred while loading your profile. Please try again.");
            }
        }

        /// <summary>
        /// Handles the Edit button click.
        /// </summary>
        protected void btnEdit_Click(object sender, EventArgs e)
        {
            // Toggle is handled by JavaScript on the client side
            // Page will postback and script will toggle the UI
        }

        /// <summary>
        /// Handles the Save button click.
        /// Validates and updates the profile information.
        /// </summary>
        protected void btnSave_Click(object sender, EventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                ShowError("Full name is required.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                ShowError("Email is required.");
                return;
            }

            // Validate email format
            try
            {
                var addr = new System.Net.Mail.MailAddress(txtEmail.Text);
                if (addr.Address != txtEmail.Text)
                {
                    ShowError("Invalid email format.");
                    return;
                }
            }
            catch
            {
                ShowError("Invalid email format.");
                return;
            }

            try
            {
                int userId = CurrentUserId;
                int lecturerId = CurrentLecturerId;

                SqlConnection conn = new SqlConnection(connStr);

                // Check if email is already in use by another user
                string checkEmailSql = "SELECT COUNT(*) FROM Users WHERE Email = @Email AND UserId != @UserId";
                SqlCommand checkCmd = new SqlCommand(checkEmailSql, conn);
                checkCmd.Parameters.AddWithValue("@Email", txtEmail.Text);
                checkCmd.Parameters.AddWithValue("@UserId", userId);

                conn.Open();
                int emailCount = (int)checkCmd.ExecuteScalar();
                conn.Close();

                if (emailCount > 0)
                {
                    ShowError("This email is already in use by another account.");
                    return;
                }

                // Update Users table
                string updateUserSql = @"
                    UPDATE Users
                    SET FullName = @FullName,
                        Email = @Email,
                        Phone = @Phone
                    WHERE UserId = @UserId";

                SqlCommand updateUserCmd = new SqlCommand(updateUserSql, conn);
                updateUserCmd.Parameters.AddWithValue("@FullName", txtFullName.Text);
                updateUserCmd.Parameters.AddWithValue("@Email", txtEmail.Text);
                updateUserCmd.Parameters.AddWithValue("@Phone", string.IsNullOrWhiteSpace(txtPhone.Text) ? (object)DBNull.Value : txtPhone.Text);
                updateUserCmd.Parameters.AddWithValue("@UserId", userId);

                conn.Open();
                updateUserCmd.ExecuteNonQuery();
                conn.Close();

                // Update Lecturers table
                string updateLecturerSql = @"
                    UPDATE Lecturers
                    SET Specialisation = @Specialisation
                    WHERE LecturerId = @LecturerId";

                SqlCommand updateLecturerCmd = new SqlCommand(updateLecturerSql, conn);
                updateLecturerCmd.Parameters.AddWithValue("@Specialisation", string.IsNullOrWhiteSpace(txtSpecialisation.Text) ? (object)DBNull.Value : txtSpecialisation.Text);
                updateLecturerCmd.Parameters.AddWithValue("@LecturerId", lecturerId);

                conn.Open();
                updateLecturerCmd.ExecuteNonQuery();
                conn.Close();

                // Update session variables
                Session["FullName"] = txtFullName.Text;
                Session["Email"] = txtEmail.Text;

                ShowSuccess("Profile updated successfully!");

                // Reload profile data
                LoadProfileData();

                // Use ClientScript to toggle back to view mode after save
                Page.ClientScript.RegisterStartupScript(
                    this.GetType(),
                    "toggleBack",
                    "setTimeout(function() { toggleEditMode(); }, 500);",
                    true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating profile: {ex.Message}");
                ShowError("An error occurred while updating your profile. Please try again.");
            }
        }

        /// <summary>
        /// Handles the Cancel button click.
        /// </summary>
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            // Reload profile data to discard changes
            LoadProfileData();

            // Switch back to view mode via JavaScript
            Page.ClientScript.RegisterStartupScript(
                this.GetType(),
                "toggleCancel",
                "toggleEditMode();",
                true);
        }

        /// <summary>
        /// Displays success message to the user.
        /// </summary>
        private void ShowSuccess(string message)
        {
            pnlSuccess.Visible = true;
            litSuccessMsg.Text = message;
            pnlError.Visible = false;
        }

        /// <summary>
        /// Displays error message to the user.
        /// </summary>
        private void ShowError(string message)
        {
            pnlError.Visible = true;
            litErrorMsg.Text = message;
            pnlSuccess.Visible = false;
        }
    }
}