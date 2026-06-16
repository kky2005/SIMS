using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Text.RegularExpressions;

namespace SIMS
{
    public partial class AdmissionForm : Page
    {
        // ── Connection string from Web.config ──────────────────────────────────
        private readonly string _connStr =
            ConfigurationManager.ConnectionStrings["SIMS_DB"].ConnectionString;

        // ── Page load ──────────────────────────────────────────────────────────
        protected void Page_Load(object sender, EventArgs e)
        {
            // Require authenticated admission users to access the form
            if (!AuthenticationHelper.IsAuthenticated())
            {
                Response.Redirect("AdmissionLogin.aspx");
                return;
            }
            if (!IsPostBack)
            {
                // Populate courses dropdown from Programmes/Programmes table
                PopulateCourses();
                PopulateIntakeYears();
                pnlSuccess.Visible = false;
                pnlError.Visible = false;
                // show approximate requested time (DB will set canonical RequestedAt)
                if (lblRequestedAt != null) lblRequestedAt.Text = DateTime.UtcNow.ToString("u");
            }
        }

        protected void btnBackToDashboard_Click(object sender, EventArgs e)
        {
            // Do not depend on AdmissionId in session for navigation; use authenticated user session only
            int userId = AuthenticationHelper.GetCurrentUserId();
            string email = AuthenticationHelper.GetCurrentUserEmail();

            if (userId != 0 && !string.IsNullOrEmpty(email))
            {
                Response.Redirect("AdmissionDashboard.aspx");
            }
            else
            {
                Response.Redirect("AdmissionLogin.aspx");
            }
        }

        private void PopulateIntakeYears()
        {
            int currentYear = DateTime.UtcNow.Year;
            ddlIntakeYear.Items.Clear();
            ddlIntakeYear.Items.Add(new System.Web.UI.WebControls.ListItem("— Select year —", ""));
            for (int y = currentYear; y <= currentYear + 2; y++)
                ddlIntakeYear.Items.Add(new System.Web.UI.WebControls.ListItem(y.ToString(), y.ToString()));
        }

        // ── Populate Programme dropdown from DB ────────────────────────────────
        private void PopulateCourses()
        {
            try
            {
                ddlCourse.Items.Clear();
                using (var conn = new SqlConnection(_connStr))
                using (var cmd = new SqlCommand("SELECT ProgrammeId, ProgrammeName FROM dbo.Programmes ORDER BY ProgrammeName", conn))
                {
                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            ddlCourse.Items.Add(new System.Web.UI.WebControls.ListItem(dr["ProgrammeName"].ToString(), dr["ProgrammeId"].ToString()));
                        }
                    }
                }
                ddlCourse.Items.Insert(0, new System.Web.UI.WebControls.ListItem("— Select a course —", ""));
            }
            catch (Exception ex)
            {
                ShowError("Could not load courses: " + ex.Message);
            }
        }



        // ── Submit handler ─────────────────────────────────────────────────────
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            // Ensure the applicant is logged in (Admissions.UserId FK)
            int userId = AuthenticationHelper.GetCurrentUserId();
            if (userId == 0)
            {
                ShowError("Please log in or register as an applicant before submitting an application.");
                return;
            }

            int programmeId = 0;
            if (!int.TryParse(ddlCourse.SelectedValue, out programmeId) || programmeId <= 0)
            {
                ShowError("Please select a valid course.");
                return;
            }

            if (!short.TryParse(ddlIntakeYear.SelectedValue, out short intakeYear))
            {
                ShowError("Please select an intake year.");
                return;
            }

            if (!byte.TryParse(ddlIntakeSemester.SelectedValue, out byte intakeSemester))
            {
                ShowError("Please select an intake semester.");
                return;
            }

            try
            {
                string status = "Pending";

                // If the applicant already has an application in the Admissions table, do not allow another
                try
                {
                    using (var conn = new SqlConnection(_connStr))
                    using (var cmd = new SqlCommand("SELECT TOP 1 AdmissionId FROM dbo.Admissions WHERE UserId = @UserId", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        conn.Open();
                        var existingObj = cmd.ExecuteScalar();
                        if (existingObj != null && existingObj != DBNull.Value)
                        {
                            int existingAdmission = Convert.ToInt32(existingObj);
                            ClearForm();
                            ShowError($"You have already submitted an application (Admission ID #{existingAdmission}). You cannot submit another application.");
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // If checking fails, log and continue to allow submission (or you may choose to block)
                    System.Diagnostics.Debug.WriteLine("Error checking existing admission: " + ex.Message);
                }

                // Read and validate personal fields
                string fullName = txtFullName.Text.Trim();
                if (string.IsNullOrEmpty(fullName)) { ShowError("Full name is required."); return; }
                if (fullName.Length > 100) { ShowError("Full name must be 100 characters or fewer."); return; }

                if (!DateTime.TryParse(txtDateOfBirth.Text, out DateTime dob)) { ShowError("Please enter a valid date of birth."); return; }
                // DOB must be in the past and age at least 16
                if (dob >= DateTime.UtcNow.Date) { ShowError("Date of birth must be in the past."); return; }
                int age = DateTime.UtcNow.Year - dob.Year; if (dob > DateTime.UtcNow.AddYears(-age)) age--;
                if (age < 16) { ShowError("Applicant must be at least 16 years old."); return; }

                string gender = ddlGender.SelectedValue ?? string.Empty;
                if (string.IsNullOrEmpty(gender)) { ShowError("Please select gender."); return; }

                string nationalId = txtNationalId.Text.Trim();
                if (string.IsNullOrEmpty(nationalId)) { ShowError("National ID is required."); return; }
                if (nationalId.Length > 20) { ShowError("National ID must be 20 characters or fewer."); return; }

                string nationality = txtNationality.Text.Trim();
                if (string.IsNullOrEmpty(nationality)) { ShowError("Nationality is required."); return; }

                string phone = txtPhoneNumber.Text.Trim();
                if (string.IsNullOrEmpty(phone)) { ShowError("Phone number is required."); return; }
                // basic phone validation: digits, spaces, +, -, ()
                if (!Regex.IsMatch(phone, @"^[0-9()+\-\s]+$")) { ShowError("Phone number contains invalid characters."); return; }
                if (phone.Length > 20) { ShowError("Phone number must be 20 characters or fewer."); return; }

                string previousInstitution = txtPreviousInstitution.Text.Trim();
                if (string.IsNullOrEmpty(previousInstitution)) { ShowError("Previous institution is required."); return; }
                if (previousInstitution.Length > 150) { ShowError("Previous institution must be 150 characters or fewer."); return; }

                string highestQualification = txtHighestQualification.Text.Trim();
                if (string.IsNullOrEmpty(highestQualification)) { ShowError("Highest qualification is required."); return; }
                if (highestQualification.Length > 50) { ShowError("Highest qualification must be 50 characters or fewer."); return; }

                decimal? previousCgpa = null;
                if (!string.IsNullOrWhiteSpace(txtPreviousCGPA.Text))
                {
                    if (decimal.TryParse(txtPreviousCGPA.Text.Trim(), out decimal cgpa))
                    {
                        if (cgpa < 0m || cgpa > 4.0m) { ShowError("Previous CGPA must be between 0.00 and 4.00."); return; }
                        previousCgpa = Math.Round(cgpa, 2);
                    }
                    else { ShowError("Previous CGPA must be a number, e.g. 3.20"); return; }
                }

                // admitted/rejected timestamps and rejection reason are set by admissions staff
                int newAdmissionId = InsertAdmission(userId, programmeId, intakeYear, intakeSemester, fullName, dob, gender, nationalId, nationality, phone, previousInstitution, highestQualification, previousCgpa, status, null, null, null);
                // Update Applicants table to reference this admission id (if Applicants table contains AdmissionId)
                try
                {
                    using (var conn = new SqlConnection(_connStr))
                    using (var cmd = new SqlCommand("UPDATE dbo.Applicants SET AdmissionId = @AdmissionId WHERE UserId = @UserId", conn))
                    {
                        cmd.Parameters.AddWithValue("@AdmissionId", newAdmissionId);
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                catch { }
                // Keep user session but do not set an AdmissionId session value here
                Session[AuthenticationHelper.SESSION_USER_ID] = userId;
                // Ensure email is present in session (login should have set this). Do not store full name here.
                if (string.IsNullOrEmpty(AuthenticationHelper.GetCurrentUserEmail()))
                {
                    // If email not available in session for some reason, try to read from Applicants table and set it.
                    Session[AuthenticationHelper.SESSION_EMAIL] = string.Empty;
                }

                ShowSuccess(newAdmissionId);
                ClearForm();
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 2601 || sqlEx.Number == 2627) ShowError("An application already exists."); else ShowError("Database error: " + sqlEx.Message);
            }
            catch (Exception ex)
            {
                ShowError("An unexpected error occurred: " + ex.Message);
            }
        }

        // ── DB insert ──────────────────────────────────────────────────────────
        private int InsertAdmission(int userId, int programmeId, short intakeYear, byte intakeSemester, string fullName, DateTime dob, string gender, string nationalId, string nationality, string phoneNumber, string previousInstitution, string highestQualification, decimal? previousCgpa, string status, DateTime? admittedAt, DateTime? rejectedAt, string rejectionReason)
        {
            const string sql = @"
                INSERT INTO dbo.Admissions
                    (UserId, ProgrammeId, IntakeYear, IntakeSemester, FullName, DateOfBirth, Gender, NationalId, Nationality, PhoneNumber, PreviousInstitution, HighestQualification, PreviousCGPA, Status, RequestedAt, AdmittedAt, RejectedAt)
                OUTPUT INSERTED.AdmissionId
                VALUES
                    (@UserId, @ProgrammeId, @IntakeYear, @IntakeSemester, @FullName, @DateOfBirth, @Gender, @NationalId, @Nationality, @PhoneNumber, @PreviousInstitution, @HighestQualification, @PreviousCGPA, @Status, SYSUTCDATETIME(), @AdmittedAt, @RejectedAt)";

            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

                cmd.Parameters.Add("@ProgrammeId", SqlDbType.Int).Value = programmeId;
                cmd.Parameters.Add("@IntakeYear", SqlDbType.SmallInt).Value = intakeYear;
                cmd.Parameters.Add("@IntakeSemester", SqlDbType.TinyInt).Value = intakeSemester;

                cmd.Parameters.Add("@FullName", SqlDbType.NVarChar, 100).Value = fullName;
                cmd.Parameters.Add("@DateOfBirth", SqlDbType.Date).Value = dob;
                cmd.Parameters.Add("@Gender", SqlDbType.NVarChar, 10).Value = gender;
                cmd.Parameters.Add("@NationalId", SqlDbType.NVarChar, 20).Value = nationalId;
                cmd.Parameters.Add("@Nationality", SqlDbType.NVarChar, 50).Value = nationality;
                cmd.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar, 20).Value = phoneNumber;
                cmd.Parameters.Add("@PreviousInstitution", SqlDbType.NVarChar, 150).Value = previousInstitution;
                cmd.Parameters.Add("@HighestQualification", SqlDbType.NVarChar, 50).Value = highestQualification;

                if (previousCgpa.HasValue)
                    cmd.Parameters.Add("@PreviousCGPA", SqlDbType.Decimal).Value = previousCgpa.Value;
                else
                    cmd.Parameters.Add("@PreviousCGPA", SqlDbType.Decimal).Value = DBNull.Value;

                cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = string.IsNullOrEmpty(status) ? "Pending" : status;
                cmd.Parameters.Add("@AdmittedAt", SqlDbType.DateTime2).Value = (object)admittedAt ?? DBNull.Value;
                cmd.Parameters.Add("@RejectedAt", SqlDbType.DateTime2).Value = (object)rejectedAt ?? DBNull.Value;
                cmd.Parameters.Add("@RejectionReason", SqlDbType.NVarChar, 255).Value = string.IsNullOrEmpty(rejectionReason) ? (object)DBNull.Value : rejectionReason;

                conn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        // ── Helpers ────────────────────────────────────────────────────────────
        private void ShowSuccess(int admissionId)
        {
            pnlError.Visible = false;
            pnlSuccess.Visible = true;

            lblSuccess.Text = $"Application submitted successfully! Your Admission ID is <strong>#{admissionId}</strong>. You will be notified once it is reviewed.";
        }

        private void ShowError(string message)
        {
            pnlSuccess.Visible = false;
            pnlError.Visible = true;
            pnlError.Style["display"] = "flex";
            // message may contain markup (we only encode where appropriate). Use Literal to render HTML safely if needed.
            lblError.Text = message;
        }

        private void ClearForm()
        {
            ddlCourse.SelectedIndex = 0;
            ddlIntakeYear.SelectedIndex = 0;
            ddlIntakeSemester.SelectedIndex = 0;
                // status not selectable by applicant
            // admitted/rejected not editable by applicant
            txtFullName.Text = string.Empty;
            txtDateOfBirth.Text = string.Empty;
            ddlGender.SelectedIndex = 0;
            txtNationalId.Text = string.Empty;
            txtNationality.Text = string.Empty;
            txtPhoneNumber.Text = string.Empty;
            txtPreviousInstitution.Text = string.Empty;
            txtHighestQualification.Text = string.Empty;
            txtPreviousCGPA.Text = string.Empty;
            // rejection reason not editable by applicant
        }
    }
}
