using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace SIMS.HeadOfProgramme
{
    public partial class HOPAnnouncements : HOPBase
    {
        string connStr = ConfigurationManager
            .ConnectionStrings["SIMS_DB"]
            .ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            EnsureAuthenticated();

            // Automatically remove announcements that have already expired
            DeleteExpiredAnnouncements();

            if (!IsPostBack)
            {
                LoadCourses();
                LoadFilterAuthors();
                LoadAnnouncements();
            }
        }

        void DeleteExpiredAnnouncements()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = @"
                        DELETE FROM Announcements
                        WHERE ExpiresAt IS NOT NULL
                          AND ExpiresAt < GETDATE()";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error deleting expired announcements: " + ex.Message);
            }
        }

        void LoadCourses()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = @"
                        SELECT CourseId, CourseCode, CourseName
                        FROM Courses
                        ORDER BY CourseCode";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        ddlCourse.DataSource = dt;
                        ddlCourse.DataTextField = "CourseName";
                        ddlCourse.DataValueField = "CourseId";
                        ddlCourse.DataBind();

                        ddlCourse.Items.Insert(0, new ListItem("-- General Announcement --", ""));

                        ddlFilterCourse.DataSource = dt.Copy();
                        ddlFilterCourse.DataTextField = "CourseName";
                        ddlFilterCourse.DataValueField = "CourseId";
                        ddlFilterCourse.DataBind();
                        ddlFilterCourse.Items.Insert(0, new ListItem("General Announcements", "GENERAL"));
                        ddlFilterCourse.Items.Insert(0, new ListItem("-- All Courses --", ""));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading courses: " + ex.Message);
                ShowAlert("Error loading courses: " + ex.Message, "error");
            }
        }

        void LoadFilterAuthors()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = @"
                        SELECT DISTINCT
                            u.UserId,
                            ISNULL(u.FullName, 'Unknown User') AS FullName
                        FROM Announcements a
                        INNER JOIN Users u ON u.UserId = a.AuthorUserId
                        ORDER BY FullName";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        ddlFilterAuthor.DataSource = dt;
                        ddlFilterAuthor.DataTextField = "FullName";
                        ddlFilterAuthor.DataValueField = "UserId";
                        ddlFilterAuthor.DataBind();

                        ddlFilterAuthor.Items.Insert(0, new ListItem("-- All Authors --", ""));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading filter authors: " + ex.Message);
            }
        }

        void LoadAnnouncements()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = @"
                        SELECT
                            a.AnnouncementId,
                            a.Title,
                            a.Body,
                            a.Audience,
                            a.PublishedAt,
                            a.ExpiresAt,
                            c.CourseCode,
                            c.CourseName,
                            ISNULL(u.FullName, 'Unknown User') AS AuthorName
                        FROM Announcements a
                        LEFT JOIN Courses c ON c.CourseId = a.CourseId
                        LEFT JOIN Users u ON u.UserId = a.AuthorUserId
                        WHERE 1 = 1";

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = conn;

                        if (!string.IsNullOrWhiteSpace(txtFilterTitle.Text))
                        {
                            sql += " AND a.Title LIKE @Title";
                            cmd.Parameters.AddWithValue("@Title", "%" + txtFilterTitle.Text.Trim() + "%");
                        }

                        if (!string.IsNullOrEmpty(ddlFilterAuthor.SelectedValue))
                        {
                            sql += " AND a.AuthorUserId = @AuthorUserId";
                            cmd.Parameters.AddWithValue("@AuthorUserId", int.Parse(ddlFilterAuthor.SelectedValue));
                        }

                        if (!string.IsNullOrEmpty(ddlFilterCourse.SelectedValue))
                        {
                            if (ddlFilterCourse.SelectedValue == "GENERAL")
                            {
                                sql += " AND a.CourseId IS NULL";
                            }
                            else
                            {
                                sql += " AND a.CourseId = @CourseId";
                                cmd.Parameters.AddWithValue("@CourseId", int.Parse(ddlFilterCourse.SelectedValue));
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(txtFilterPostedDate.Text))
                        {
                            if (DateTime.TryParse(txtFilterPostedDate.Text, out DateTime postedDate))
                            {
                                sql += " AND CAST(a.PublishedAt AS DATE) = @PostedDate";
                                cmd.Parameters.AddWithValue("@PostedDate", postedDate.Date);
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(txtFilterExpiryDate.Text))
                        {
                            if (DateTime.TryParse(txtFilterExpiryDate.Text, out DateTime expiryDate))
                            {
                                sql += " AND a.ExpiresAt IS NOT NULL AND CAST(a.ExpiresAt AS DATE) = @ExpiryDate";
                                cmd.Parameters.AddWithValue("@ExpiryDate", expiryDate.Date);
                            }
                        }

                        sql += " ORDER BY a.PublishedAt DESC";
                        cmd.CommandText = sql;
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
                ShowAlert("Error loading announcements: " + ex.Message, "error");
            }
        }

        protected void btnPublish_Click(object sender, EventArgs e)
        {
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

                if (!string.IsNullOrEmpty(ddlCourse.SelectedValue))
                {
                    courseId = int.Parse(ddlCourse.SelectedValue);
                }

                string audience = ddlAudience.SelectedValue;

                if (courseId.HasValue && audience == "AllStudents")
                {
                    audience = "CourseStudents";
                }

                DateTime? expiresAt = null;

                if (!string.IsNullOrWhiteSpace(txtExpiryDays.Text))
                {
                    if (!int.TryParse(txtExpiryDays.Text.Trim(), out int expiryDays))
                    {
                        ShowAlert("Please enter a valid number of days.", "error");
                        return;
                    }

                    if (expiryDays < 1)
                    {
                        ShowAlert("Expiry days must be at least 1 day.", "error");
                        return;
                    }

                    expiresAt = DateTime.Now.AddDays(expiryDays);
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
                        cmd.Parameters.AddWithValue("@AuthorUserId", userId);
                        cmd.Parameters.AddWithValue("@CourseId", courseId.HasValue ? (object)courseId.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@Title", txtTitle.Text.Trim());
                        cmd.Parameters.AddWithValue("@Body", txtBody.Text.Trim());
                        cmd.Parameters.AddWithValue("@Audience", audience);
                        cmd.Parameters.AddWithValue("@ExpiresAt", expiresAt.HasValue ? (object)expiresAt.Value : DBNull.Value);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowAlert("Announcement posted successfully!", "success");
                ClearAnnouncementForm();
                LoadAnnouncements();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error posting announcement: " + ex.Message);
                ShowAlert("Error posting announcement: " + ex.Message, "error");
            }
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                Button btn = (Button)sender;

                if (int.TryParse(btn.CommandArgument, out int announcementId))
                {
                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        string sql = "DELETE FROM Announcements WHERE AnnouncementId = @AnnouncementId";

                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@AnnouncementId", announcementId);

                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }

                    ShowAlert("Announcement deleted successfully!", "success");
                    LoadAnnouncements();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error deleting announcement: " + ex.Message);
                ShowAlert("Error deleting announcement: " + ex.Message, "error");
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearAnnouncementForm();
        }

        protected void btnApplyFilter_Click(object sender, EventArgs e)
        {
            LoadAnnouncements();
        }

        protected void btnClearFilter_Click(object sender, EventArgs e)
        {
            txtFilterTitle.Text = "";
            ddlFilterAuthor.SelectedIndex = 0;
            ddlFilterCourse.SelectedIndex = 0;
            txtFilterPostedDate.Text = "";
            txtFilterExpiryDate.Text = "";
            LoadAnnouncements();
        }

        void ClearAnnouncementForm()
        {
            txtTitle.Text = "";
            txtBody.Text = "";
            txtExpiryDays.Text = "";
            ddlCourse.SelectedIndex = 0;
            ddlAudience.SelectedIndex = 0;
        }

        protected string FormatAudience(object audienceObj, object courseCodeObj)
        {
            string audience = audienceObj == null ? "" : audienceObj.ToString();
            string courseCode = courseCodeObj == DBNull.Value || courseCodeObj == null ? "" : courseCodeObj.ToString();

            if (!string.IsNullOrEmpty(courseCode))
            {
                return "Course Students - " + courseCode;
            }

            switch (audience)
            {
                case "AllStudents":
                    return "All Students";
                case "CourseStudents":
                    return "Course Students";
                case "AllLecturers":
                    return "All Lecturers";
                case "Everyone":
                    return "Everyone";
                default:
                    return audience;
            }
        }

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
