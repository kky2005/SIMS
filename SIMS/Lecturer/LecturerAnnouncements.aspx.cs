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
            EnsureAuthenticated();

            if (!IsPostBack)
            {
                // Require CourseID (same UX as Attendance/Grades)
                if (string.IsNullOrEmpty(Request.QueryString["CourseID"]) ||
                    !int.TryParse(Request.QueryString["CourseID"], out int courseId))
                {
                    Response.Redirect("LecturerCourses.aspx");
                    return;
                }

                // Verify lecturer teaches this course
                if (!LecturerTeachesCourse(courseId))
                {
                    Response.Redirect("LecturerCourses.aspx");
                    return;
                }

                hidCourseId.Value = courseId.ToString();

                LoadCourseHeader(courseId);
                LoadAnnouncements(courseId);
            }
        }

        private bool LecturerTeachesCourse(int courseId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = "SELECT COUNT(1) FROM CourseAssignments WHERE CourseId = @CourseId AND LecturerId = @LecturerId";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CourseId", courseId);
                        cmd.Parameters.AddWithValue("@LecturerId", CurrentLecturerId);
                        conn.Open();
                        int c = Convert.ToInt32(cmd.ExecuteScalar());
                        conn.Close();
                        return c > 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        private void LoadCourseHeader(int courseId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = "SELECT CourseCode, CourseName FROM Courses WHERE CourseId = @CourseId";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CourseId", courseId);
                        conn.Open();
                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                string code = r["CourseCode"].ToString();
                                string name = r["CourseName"].ToString();
                                litCourseName.Text = $"{code} - {name}";
                                litCourseHeader.Text = $"{code} - {name} (Announcements)";
                            }
                        }
                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading course header: " + ex.Message);
            }
        }

        private void LoadAnnouncements(int courseId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = @"
                        SELECT AnnouncementId, Title, Body, Audience, PublishedAt, ExpiresAt
                        FROM Announcements
                        WHERE CourseId = @CourseId
                          AND AuthorUserId = @AuthorUserId
                        ORDER BY PublishedAt DESC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CourseId", courseId);
                        cmd.Parameters.AddWithValue("@AuthorUserId", CurrentUserId);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        rptAnnouncements.DataSource = dt;
                        rptAnnouncements.DataBind();

                        pnlNoAnnouncements.Visible = (dt.Rows.Count == 0);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading announcements: " + ex.Message);
                pnlNoAnnouncements.Visible = true;
            }
        }

        protected void btnPublish_Click(object sender, EventArgs e)
        {
            try
            {
                int courseId = int.Parse(hidCourseId.Value);

                if (string.IsNullOrWhiteSpace(txtTitle.Text))
                {
                    ShowError("Please enter an announcement title.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtBody.Text))
                {
                    ShowError("Please enter announcement content.");
                    return;
                }

                DateTime? expiresAt = null;
                if (!string.IsNullOrWhiteSpace(txtExpiresAt.Text))
                {
                    if (DateTime.TryParse(txtExpiresAt.Text, out DateTime dt))
                        expiresAt = dt;
                    else
                    {
                        ShowError("Invalid expiration date.");
                        return;
                    }
                }

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = @"
                        INSERT INTO Announcements
                        (AuthorUserId, CourseId, Title, Body, Audience, PublishedAt, ExpiresAt)
                        VALUES
                        (@AuthorUserId, @CourseId, @Title, @Body, @Audience, GETDATE(), @ExpiresAt)";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@AuthorUserId", CurrentUserId);
                        cmd.Parameters.AddWithValue("@CourseId", courseId);
                        cmd.Parameters.AddWithValue("@Title", txtTitle.Text.Trim());
                        cmd.Parameters.AddWithValue("@Body", txtBody.Text.Trim());
                        cmd.Parameters.AddWithValue("@Audience", "CourseStudents");
                        cmd.Parameters.AddWithValue("@ExpiresAt", expiresAt.HasValue ? (object)expiresAt.Value : DBNull.Value);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                }

                litSuccessMsg.Text = "Announcement posted successfully!";
                pnlSuccess.Visible = true;
                pnlError.Visible = false;

                txtTitle.Text = "";
                txtBody.Text = "";
                txtExpiresAt.Text = "";

                LoadAnnouncements(courseId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error posting announcement: " + ex.Message);
                ShowError("Error posting announcement: " + ex.Message);
            }
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (!int.TryParse(((Button)sender).CommandArgument, out int announcementId))
                    return;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    // Ensure the current user is the author of this announcement
                    string checkSql = "SELECT COUNT(1) FROM Announcements WHERE AnnouncementId = @Id AND AuthorUserId = @AuthorUserId";
                    using (SqlCommand checkCmd = new SqlCommand(checkSql, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@Id", announcementId);
                        checkCmd.Parameters.AddWithValue("@AuthorUserId", CurrentUserId);
                        conn.Open();
                        int exists = Convert.ToInt32(checkCmd.ExecuteScalar());
                        conn.Close();

                        if (exists == 0)
                        {
                            ShowError("You are not authorized to delete this announcement.");
                            return;
                        }
                    }

                    string delSql = "DELETE FROM Announcements WHERE AnnouncementId = @Id";
                    using (SqlCommand delCmd = new SqlCommand(delSql, conn))
                    {
                        delCmd.Parameters.AddWithValue("@Id", announcementId);
                        conn.Open();
                        delCmd.ExecuteNonQuery();
                        conn.Close();
                    }
                }

                litSuccessMsg.Text = "Announcement deleted.";
                pnlSuccess.Visible = true;
                pnlError.Visible = false;

                LoadAnnouncements(int.Parse(hidCourseId.Value));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error deleting announcement: " + ex.Message);
                ShowError("Error deleting announcement: " + ex.Message);
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtTitle.Text = "";
            txtBody.Text = "";
            txtExpiresAt.Text = "";
            pnlSuccess.Visible = false;
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