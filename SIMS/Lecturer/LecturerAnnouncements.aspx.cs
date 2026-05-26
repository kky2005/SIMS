using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SIMS.Lecturer
{
    public partial class LecturerAnnouncements : LecturerBase
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
                LoadCourses();
                LoadAnnouncements();
            }
        }

        /// <summary>
        /// Load lecturer's courses into dropdown
        /// </summary>
        void LoadCourses()
        {
            try
            {
                int lecturerId = CurrentLecturerId;
                int currentYear = DateTime.Now.Year;
                int currentSemester = GetCurrentSemester();

                SqlConnection conn = new SqlConnection(connStr);

                string sql = @"
                    SELECT DISTINCT
                        c.CourseId,
                        c.CourseCode,
                        c.CourseName
                    FROM CourseAssignments ca
                    INNER JOIN Courses c ON c.CourseId = ca.CourseId
                    WHERE ca.LecturerId = @LecturerId
                      AND ca.AcademicYear = @Year
                      AND ca.Semester = @Semester
                    ORDER BY c.CourseCode";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@LecturerId", lecturerId);
                cmd.Parameters.AddWithValue("@Year", currentYear);
                cmd.Parameters.AddWithValue("@Semester", currentSemester);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ddlCourse.DataSource = dt;
                ddlCourse.DataTextField = "CourseName";
                ddlCourse.DataValueField = "CourseId";
                ddlCourse.DataBind();

                ddlCourse.Items.Insert(0, new ListItem("-- Select Course --", ""));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading courses: {ex.Message}");
            }
        }

        /// <summary>
        /// Load all announcements posted by this lecturer
        /// </summary>
        void LoadAnnouncements()
        {
            try
            {
                int userId = CurrentUserId;

                SqlConnection conn = new SqlConnection(connStr);

                string sql = @"
                    SELECT
                        a.AnnouncementId,
                        a.Title,
                        a.Body,
                        CASE
                            WHEN a.CourseId IS NULL THEN 'AllStudents'
                            ELSE 'CourseStudents'
                        END AS Audience,
                        a.PublishedAt,
                        CASE
                            WHEN a.ExpiresAt IS NULL THEN 'No expiry'
                            WHEN a.ExpiresAt > GETDATE() THEN 'Active'
                            ELSE 'Expired'
                        END AS Status,
                        a.ExpiresAt
                    FROM Announcements a
                    WHERE a.AuthorUserId = @UserId
                    ORDER BY a.PublishedAt DESC";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptAnnouncements.DataSource = dt;
                rptAnnouncements.DataBind();

                // Show message if no announcements
                pnlNoAnnouncements.Visible = (dt.Rows.Count == 0);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading announcements: {ex.Message}");
                pnlNoAnnouncements.Visible = true;
            }
        }

        /// <summary>
        /// Publish a new announcement
        /// </summary>
        protected void btnPublish_Click(object sender, EventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                ShowAlert("Please enter an announcement title.", "error");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtBody.Text))
            {
                ShowAlert("Please enter announcement content.", "error");
                return;
            }

            if (txtBody.Text.Length > 5000)
            {
                ShowAlert("Announcement content cannot exceed 5000 characters.", "error");
                return;
            }

            try
            {
                int userId = CurrentUserId;
                int? courseId = null;

                // Check if a specific course is selected
                if (!string.IsNullOrEmpty(ddlCourse.SelectedValue) && ddlCourse.SelectedValue != "")
                {
                    courseId = int.Parse(ddlCourse.SelectedValue);
                }

                DateTime? expiresAt = null;
                if (!string.IsNullOrWhiteSpace(txtExpiresAt.Text))
                {
                    if (DateTime.TryParse(txtExpiresAt.Text, out DateTime expDate))
                    {
                        expiresAt = expDate;
                    }
                }

                SqlConnection conn = new SqlConnection(connStr);

                string sql = @"
                    INSERT INTO Announcements
                    (AuthorUserId, CourseId, Title, Body, Audience, PublishedAt, ExpiresAt)
                    VALUES
                    (@AuthorUserId, @CourseId, @Title, @Body, @Audience, GETDATE(), @ExpiresAt)";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@AuthorUserId", userId);
                cmd.Parameters.AddWithValue("@CourseId", courseId.HasValue ? (object)courseId : DBNull.Value);
                cmd.Parameters.AddWithValue("@Title", txtTitle.Text);
                cmd.Parameters.AddWithValue("@Body", txtBody.Text);
                cmd.Parameters.AddWithValue("@Audience", courseId.HasValue ? "CourseStudents" : "AllStudents");
                cmd.Parameters.AddWithValue("@ExpiresAt", expiresAt.HasValue ? (object)expiresAt : DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();

                ShowAlert("Announcement posted successfully!", "success");
                ClearAnnouncementForm();
                LoadAnnouncements();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error posting announcement: {ex.Message}");
                ShowAlert("Error posting announcement: " + ex.Message, "error");
            }
        }

        /// <summary>
        /// Delete announcement
        /// </summary>
        protected void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (int.TryParse(((Button)sender).CommandArgument, out int announcementId))
                {
                    SqlConnection conn = new SqlConnection(connStr);

                    string sql = "DELETE FROM Announcements WHERE AnnouncementId = @AnnouncementId";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@AnnouncementId", announcementId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();

                    ShowAlert("Announcement deleted successfully!", "success");
                    LoadAnnouncements();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting announcement: {ex.Message}");
                ShowAlert("Error deleting announcement: " + ex.Message, "error");
            }
        }

        /// <summary>
        /// Edit announcement (placeholder - implement as needed)
        /// </summary>
        protected void btnEdit_Click(object sender, EventArgs e)
        {
            try
            {
                if (int.TryParse(((Button)sender).CommandArgument, out int announcementId))
                {
                    // TODO: Implement edit functionality
                    ShowAlert("Edit functionality coming soon.", "error");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error editing announcement: {ex.Message}");
                ShowAlert("Error: " + ex.Message, "error");
            }
        }

        /// <summary>
        /// Clear form button
        /// </summary>
        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearAnnouncementForm();
        }

        /// <summary>
        /// Repeater item data bound event
        /// </summary>
        protected void rptAnnouncements_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            // Custom binding logic can be added here if needed
        }

        /// <summary>
        /// Clear form fields
        /// </summary>
        void ClearAnnouncementForm()
        {
            txtTitle.Text = "";
            txtBody.Text = "";
            ddlCourse.SelectedIndex = 0;
            txtExpiresAt.Text = "";
        }

        /// <summary>
        /// Show alert messages using Panel controls
        /// </summary>
        void ShowAlert(string message, string type)
        {
            if (type == "success")
            {
                litSuccessMsg.Text = message;
                pnlSuccess.Visible = true;
                pnlError.Visible = false;
            }
            else
            {
                litErrorMsg.Text = message;
                pnlError.Visible = true;
                pnlSuccess.Visible = false;
            }
        }
    }
}