using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SIMS.Lecturer
{
    public partial class LecturerAttendance : LecturerBase
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
                // Get course ID from query string
                if (string.IsNullOrEmpty(Request.QueryString["CourseID"]))
                {
                    Response.Redirect("LecturerCourses.aspx?msg=selectcourse");
                    return;
                }

                if (!int.TryParse(Request.QueryString["CourseID"], out int courseId))
                {
                    Response.Redirect("LecturerCourses.aspx");
                    return;
                }

                // Verify that this lecturer teaches this course
                if (!VerifyLecturerTeachesCourse(courseId))
                {
                    ShowError("You do not have permission to view this course.");
                    Response.Redirect("LecturerCourses.aspx");
                    return;
                }

                hidCourseId.Value = courseId.ToString();

                LoadCourseInfo(courseId);
                LoadStudentAttendance(courseId, DateTime.Now.ToString("yyyy-MM-dd"), "");
                txtAttendanceDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
            }
        }

        /// <summary>
        /// Verifies that the current lecturer teaches the specified course.
        /// Checks if the course is assigned to this lecturer (regardless of academic year/semester).
        /// </summary>
        private bool VerifyLecturerTeachesCourse(int courseId)
        {
            try
            {
                SqlConnection conn = new SqlConnection(connStr);
                
                // Check if this lecturer teaches this course (any assignment)
                string sql = @"
                    SELECT COUNT(*) FROM CourseAssignments ca
                    WHERE ca.CourseId = @CourseId
                      AND ca.LecturerId = @LecturerId";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CourseId", courseId);
                cmd.Parameters.AddWithValue("@LecturerId", CurrentLecturerId);

                conn.Open();
                int count = (int)cmd.ExecuteScalar();
                conn.Close();

                if (count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Lecturer {CurrentLecturerId} verified to teach course {courseId}");
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Lecturer {CurrentLecturerId} does NOT teach course {courseId}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error verifying course access: {ex.Message}");
                return false;
            }
        }

        void LoadCourseInfo(int courseId)
        {
            try
            {
                SqlConnection conn = new SqlConnection(connStr);
                string sql = @"SELECT CourseName FROM Courses WHERE CourseId = @CourseId";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CourseId", courseId);
                conn.Open();
                object result = cmd.ExecuteScalar();
                conn.Close();

                if (result != null)
                {
                    litCourseName.Text = result.ToString();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading course info: {ex.Message}");
            }
        }

        void LoadStudentAttendance(int courseId, string attendanceDate, string statusFilter)
        {
            try
            {
                int lecturerId = CurrentLecturerId;
                int currentYear = DateTime.Now.Year;
                int currentSemester = GetCurrentSemester();

                SqlConnection conn = new SqlConnection(connStr);

                string sql = @"
                    SELECT
                        e.EnrolmentId,
                        s.StudentNo,
                        u.FullName,
                        u.Email,
                        p.ProgrammeName,
                        ISNULL(att.Status, 'Absent') AS Status
                    FROM Enrolments e
                    INNER JOIN Students s ON s.StudentId = e.StudentId
                    INNER JOIN Users u ON u.UserId = s.UserId
                    INNER JOIN Programmes p ON p.ProgrammeId = s.ProgrammeId
                    INNER JOIN CourseAssignments ca 
                        ON ca.CourseId = e.CourseId
                        AND ca.AcademicYear = e.AcademicYear
                        AND ca.Semester = e.Semester
                    LEFT JOIN Attendance att 
                        ON att.EnrolmentId = e.EnrolmentId
                        AND CAST(att.AttendanceDate AS DATE) = @AttendanceDate
                    WHERE e.CourseId = @CourseId
                      AND ca.LecturerId = @LecturerId
                      AND ca.AcademicYear = @Year
                      AND ca.Semester = @Semester
                      AND e.Status = 'Active'";

                if (!string.IsNullOrEmpty(statusFilter))
                {
                    sql += " AND ISNULL(att.Status, 'Absent') = @StatusFilter";
                }

                sql += " ORDER BY s.StudentNo ASC";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CourseId", courseId);
                cmd.Parameters.AddWithValue("@LecturerId", lecturerId);
                cmd.Parameters.AddWithValue("@Year", currentYear);
                cmd.Parameters.AddWithValue("@Semester", currentSemester);
                cmd.Parameters.AddWithValue("@AttendanceDate", attendanceDate);
                if (!string.IsNullOrEmpty(statusFilter))
                {
                    cmd.Parameters.AddWithValue("@StatusFilter", statusFilter);
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    rptAttendance.DataSource = dt;
                    rptAttendance.DataBind();
                    pnlNoData.Visible = false;
                    CalculateStats(dt);
                }
                else
                {
                    pnlNoData.Visible = true;
                    litPresentCount.Text = "0";
                    litAbsentCount.Text = "0";
                    litTotalCount.Text = "0";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading attendance: {ex.Message}");
                ShowError("An error occurred while loading attendance data.");
            }
        }

        void CalculateStats(DataTable dt)
        {
            int present = 0;
            int absent = 0;

            foreach (DataRow row in dt.Rows)
            {
                if (row["Status"].ToString() == "Present")
                    present++;
                else
                    absent++;
            }

            litPresentCount.Text = present.ToString();
            litAbsentCount.Text = absent.ToString();
            litTotalCount.Text = dt.Rows.Count.ToString();
        }

        protected void rptAttendance_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            // Additional logic for repeater items if needed
        }

        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtAttendanceDate.Text))
            {
                txtAttendanceDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
            }

            int courseId = int.Parse(hidCourseId.Value);
            LoadStudentAttendance(courseId, txtAttendanceDate.Text, ddlStatusFilter.SelectedValue);
        }

        protected void btnSaveAttendance_Click(object sender, EventArgs e)
        {
            try
            {
                int courseId = int.Parse(hidCourseId.Value);
                int lecturerId = CurrentLecturerId;
                string attendanceDate = txtAttendanceDate.Text;

                if (string.IsNullOrEmpty(attendanceDate))
                {
                    attendanceDate = DateTime.Now.ToString("yyyy-MM-dd");
                }

                SqlConnection conn = new SqlConnection(connStr);

                // Delete existing attendance records for this date and course
                string deleteSql = @"
                    DELETE FROM Attendance
                    WHERE EnrolmentId IN (
                        SELECT e.EnrolmentId FROM Enrolments e
                        INNER JOIN CourseAssignments ca 
                            ON ca.CourseId = e.CourseId
                        WHERE e.CourseId = @CourseId
                          AND ca.LecturerId = @LecturerId
                          AND CAST(Attendance.AttendanceDate AS DATE) = @AttendanceDate
                    )";

                SqlCommand deleteCmd = new SqlCommand(deleteSql, conn);
                deleteCmd.Parameters.AddWithValue("@CourseId", courseId);
                deleteCmd.Parameters.AddWithValue("@LecturerId", lecturerId);
                deleteCmd.Parameters.AddWithValue("@AttendanceDate", attendanceDate);

                conn.Open();
                deleteCmd.ExecuteNonQuery();
                conn.Close();

                // Insert new attendance records for selected students
                if (Request.Form["chkAttendance"] != null)
                {
                    string[] selectedIds = Request.Form["chkAttendance"].Split(',');

                    foreach (string enrolmentIdStr in selectedIds)
                    {
                        if (int.TryParse(enrolmentIdStr, out int enrolmentId))
                        {
                            string insertSql = @"
                                INSERT INTO Attendance (EnrolmentId, AttendanceDate, Status, RecordedBy, RecordedAt)
                                VALUES (@EnrolmentId, @AttendanceDate, @Status, @RecordedBy, @RecordedAt)";

                            SqlCommand insertCmd = new SqlCommand(insertSql, conn);
                            insertCmd.Parameters.AddWithValue("@EnrolmentId", enrolmentId);
                            insertCmd.Parameters.AddWithValue("@AttendanceDate", attendanceDate);
                            insertCmd.Parameters.AddWithValue("@Status", "Present");
                            insertCmd.Parameters.AddWithValue("@RecordedBy", CurrentUserId);
                            insertCmd.Parameters.AddWithValue("@RecordedAt", DateTime.Now);

                            conn.Open();
                            insertCmd.ExecuteNonQuery();
                            conn.Close();
                        }
                    }
                }

                ShowSuccess("Attendance saved successfully!");
                LoadStudentAttendance(courseId, attendanceDate, ddlStatusFilter.SelectedValue);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving attendance: {ex.Message}");
                ShowError("An error occurred while saving attendance.");
            }
        }

        protected void btnMarkAllPresent_Click(object sender, EventArgs e)
        {
            Page.ClientScript.RegisterStartupScript(
                this.GetType(),
                "markAllPresent",
                @"
                var chkboxes = document.querySelectorAll('.attendance-checkbox');
                chkboxes.forEach(function (checkbox) {
                    checkbox.checked = true;
                });
                ",
                true);
        }

        protected void btnMarkAllAbsent_Click(object sender, EventArgs e)
        {
            Page.ClientScript.RegisterStartupScript(
                this.GetType(),
                "markAllAbsent",
                @"
                var chkboxes = document.querySelectorAll('.attendance-checkbox');
                chkboxes.forEach(function (checkbox) {
                    checkbox.checked = false;
                });
                ",
                true);
        }

        private void ShowSuccess(string message)
        {
            pnlSuccess.Visible = true;
            litSuccessMsg.Text = message;
        }

        private void ShowError(string message)
        {
            Page.ClientScript.RegisterStartupScript(
                this.GetType(),
                "showError",
                $"alert('{message.Replace("'", "\\'")}');",
                true);
        }
    }
}
