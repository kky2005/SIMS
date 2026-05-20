using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SIMS.Lecturer
{
    public partial class LecturerAttendance : System.Web.UI.Page
    {
        string connStr = ConfigurationManager
            .ConnectionStrings["SIMS_DB"]
            .ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["LecturerID"] == null)
            {
                Response.Redirect("LecturerLogin.aspx");
                return;
            }

            if (!IsPostBack)
            {
                if (string.IsNullOrEmpty(Request.QueryString["CourseID"]))
                {
                    Response.Redirect("LecturerCourses.aspx");
                    return;
                }

                int courseId = int.Parse(Request.QueryString["CourseID"]);
                hidCourseId.Value = courseId.ToString();

                LoadCourseInfo(courseId);
                LoadStudentAttendance(courseId, DateTime.Now.ToString("yyyy-MM-dd"), "");

                // Set today's date as default
                txtAttendanceDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
            }
        }

        void LoadCourseInfo(int courseId)
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

        void LoadStudentAttendance(int courseId, string attendanceDate, string statusFilter)
        {
            int lecturerId = (int)Session["LecturerID"];
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

        protected void rptAttendance_ItemDataBound(object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e)
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
            int courseId = int.Parse(hidCourseId.Value);
            int lecturerId = (int)Session["LecturerID"];
            string attendanceDate = txtAttendanceDate.Text;

            if (string.IsNullOrEmpty(attendanceDate))
            {
                attendanceDate = DateTime.Now.ToString("yyyy-MM-dd");
            }

            SqlConnection conn = new SqlConnection(connStr);

            // First, delete existing attendance records for this date
            string deleteSql = @"
                DELETE FROM Attendance
                WHERE EnrolmentId IN (
                    SELECT e.EnrolmentId FROM Enrolments e
                    INNER JOIN CourseAssignments ca 
                        ON ca.CourseId = e.CourseId
                    WHERE e.CourseId = @CourseId
                      AND ca.LecturerId = @LecturerId
                      AND CAST(AttendanceDate AS DATE) = @AttendanceDate
                )";

            SqlCommand deleteCmd = new SqlCommand(deleteSql, conn);
            deleteCmd.Parameters.AddWithValue("@CourseId", courseId);
            deleteCmd.Parameters.AddWithValue("@LecturerId", lecturerId);
            deleteCmd.Parameters.AddWithValue("@AttendanceDate", attendanceDate);
            conn.Open();
            deleteCmd.ExecuteNonQuery();
            conn.Close();

            // Get selected enrolment IDs from the checkboxes
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
                        insertCmd.Parameters.AddWithValue("@RecordedBy", lecturerId);
                        insertCmd.Parameters.AddWithValue("@RecordedAt", DateTime.Now);

                        conn.Open();
                        insertCmd.ExecuteNonQuery();
                        conn.Close();
                    }
                }
            }

            pnlSuccess.Visible = true;
            litSuccessMsg.Text = "Attendance saved successfully!";

            // Reload the attendance data
            LoadStudentAttendance(courseId, attendanceDate, ddlStatusFilter.SelectedValue);
        }

        protected void btnMarkAllPresent_Click(object sender, EventArgs e)
        {
            // Mark all visible checkboxes
            System.Web.UI.HtmlControls.HtmlInputCheckBox chk;
            foreach (System.Web.UI.WebControls.RepeaterItem item in rptAttendance.Items)
            {
                chk = item.FindControl("chkAttendance") as System.Web.UI.HtmlControls.HtmlInputCheckBox;
                if (chk != null)
                {
                    chk.Checked = true;
                }
            }
        }

        protected void btnMarkAllAbsent_Click(object sender, EventArgs e)
        {
            // Unmark all checkboxes
            System.Web.UI.HtmlControls.HtmlInputCheckBox chk;
            foreach (System.Web.UI.WebControls.RepeaterItem item in rptAttendance.Items)
            {
                chk = item.FindControl("chkAttendance") as System.Web.UI.HtmlControls.HtmlInputCheckBox;
                if (chk != null)
                {
                    chk.Checked = false;
                }
            }
        }

        int GetCurrentSemester()
        {
            int month = DateTime.Now.Month;
            if (month <= 4) return 1;
            if (month <= 8) return 2;
            return 3;
        }
    }
}
