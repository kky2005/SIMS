using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SIMS.Lecturer
{
    public partial class LecturerDashboard : System.Web.UI.Page
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
                litName.Text    = Session["FullName"].ToString();
                litDept.Text    = Session["Department"].ToString();
                litStaffNo.Text = Session["StaffNo"].ToString();
                litDate.Text    = DateTime.Now.ToString("dddd, dd MMMM yyyy");

                LoadDashboardStats();
                LoadDashboardCourses();
                LoadAtRiskStudents();
            }
        }

        void LoadDashboardStats()
        {
            int lecturerId = (int)Session["LecturerID"];

            SqlConnection conn = new SqlConnection(connStr);

            // Current academic year and semester (you may read from AcademicCalendar)
            int currentYear = DateTime.Now.Year;
            int currentSemester = GetCurrentSemester();

            string sql = @"
                SELECT
                    COUNT(DISTINCT ca.CourseId)  AS TotalCourses,
                    COUNT(DISTINCT e.StudentId)  AS TotalStudents
                FROM CourseAssignments ca
                INNER JOIN Enrolments e
                    ON e.CourseId     = ca.CourseId
                    AND e.AcademicYear = ca.AcademicYear
                    AND e.Semester     = ca.Semester
                    AND e.Status       = 'Active'
                WHERE ca.LecturerId   = @LecturerId
                  AND ca.AcademicYear = @Year
                  AND ca.Semester     = @Semester";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@LecturerId", lecturerId);
            cmd.Parameters.AddWithValue("@Year", currentYear);
            cmd.Parameters.AddWithValue("@Semester", currentSemester);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            if (dt.Rows.Count > 0)
            {
                litTotalCourses.Text  = dt.Rows[0]["TotalCourses"].ToString();
                litTotalStudents.Text = dt.Rows[0]["TotalStudents"].ToString();
            }

            // Count at-risk (attendance < 80%)
            int atRisk = CountAtRiskStudents(lecturerId, currentYear, currentSemester);
            litAtRisk.Text      = atRisk.ToString();
            litAtRiskBadge.Text = atRisk.ToString();

            // Count unpublished assessments
            litPendingMarks.Text = CountPendingAssessments(
                lecturerId, currentYear, currentSemester).ToString();
        }

        int CountAtRiskStudents(int lecturerId, int year, int semester)
        {
            SqlConnection conn = new SqlConnection(connStr);
            string sql = @"
                SELECT COUNT(DISTINCT e.StudentId) FROM Enrolments e
                INNER JOIN CourseAssignments ca
                    ON ca.CourseId = e.CourseId
                    AND ca.AcademicYear = e.AcademicYear
                    AND ca.Semester = e.Semester
                LEFT JOIN (
                    SELECT EnrolmentId,
                           100.0 * SUM(CASE WHEN Status='Present' THEN 1 ELSE 0 END)
                                 / NULLIF(COUNT(*),0) AS Pct
                    FROM Attendance
                    GROUP BY EnrolmentId
                ) att ON att.EnrolmentId = e.EnrolmentId
                WHERE ca.LecturerId = @LecturerId
                  AND ca.AcademicYear = @Year
                  AND ca.Semester = @Semester
                  AND e.Status = 'Active'
                  AND (att.Pct < 80 OR att.Pct IS NULL)";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@LecturerId", lecturerId);
            cmd.Parameters.AddWithValue("@Year", year);
            cmd.Parameters.AddWithValue("@Semester", semester);
            conn.Open();
            int count = (int)cmd.ExecuteScalar();
            conn.Close();
            return count;
        }

        int CountPendingAssessments(int lecturerId, int year, int semester)
        {
            SqlConnection conn = new SqlConnection(connStr);
            string sql = @"
                SELECT COUNT(*) FROM Assessments a
                INNER JOIN CourseAssignments ca
                    ON ca.CourseId = a.CourseId
                    AND ca.AcademicYear = a.AcademicYear
                    AND ca.Semester = a.Semester
                WHERE ca.LecturerId = @LecturerId
                  AND ca.AcademicYear = @Year
                  AND ca.Semester = @Semester
                  AND a.IsPublished = 0";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@LecturerId", lecturerId);
            cmd.Parameters.AddWithValue("@Year", year);
            cmd.Parameters.AddWithValue("@Semester", semester);
            conn.Open();
            int count = (int)cmd.ExecuteScalar();
            conn.Close();
            return count;
        }

        void LoadDashboardCourses()
        {
            int lecturerId = (int)Session["LecturerID"];
            int currentYear = DateTime.Now.Year;
            int currentSemester = GetCurrentSemester();

            SqlConnection conn = new SqlConnection(connStr);

            string sql = @"
                SELECT
                    c.CourseId,
                    c.CourseCode,
                    c.CourseName,
                    c.CreditHours,
                    COUNT(e.EnrolmentId) AS TotalStudents
                FROM CourseAssignments ca
                INNER JOIN Courses c ON c.CourseId = ca.CourseId
                LEFT JOIN Enrolments e
                    ON e.CourseId = c.CourseId
                    AND e.AcademicYear = ca.AcademicYear
                    AND e.Semester = ca.Semester
                    AND e.Status = 'Active'
                WHERE ca.LecturerId   = @LecturerId
                  AND ca.AcademicYear = @Year
                  AND ca.Semester     = @Semester
                GROUP BY c.CourseId, c.CourseCode, c.CourseName, c.CreditHours";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@LecturerId", lecturerId);
            cmd.Parameters.AddWithValue("@Year", currentYear);
            cmd.Parameters.AddWithValue("@Semester", currentSemester);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            gvDashboardCourses.DataSource = dt;
            gvDashboardCourses.DataBind();
        }

        void LoadAtRiskStudents()
        {
            int lecturerId = (int)Session["LecturerID"];
            int currentYear = DateTime.Now.Year;
            int currentSemester = GetCurrentSemester();

            SqlConnection conn = new SqlConnection(connStr);

            string sql = @"
                SELECT TOP 5
                    s.StudentNo,
                    u.FullName,
                    c.CourseName,
                    ISNULL(CAST(ROUND(att.Pct,1) AS NVARCHAR(10)), 'No data') AS AttendancePct
                FROM Enrolments e
                INNER JOIN Students s   ON s.StudentId = e.StudentId
                INNER JOIN Users u      ON u.UserId    = s.UserId
                INNER JOIN Courses c    ON c.CourseId  = e.CourseId
                INNER JOIN CourseAssignments ca
                    ON ca.CourseId = e.CourseId
                    AND ca.AcademicYear = e.AcademicYear
                    AND ca.Semester = e.Semester
                LEFT JOIN (
                    SELECT EnrolmentId,
                           100.0 * SUM(CASE WHEN Status='Present' THEN 1 ELSE 0 END)
                                 / NULLIF(COUNT(*),0) AS Pct
                    FROM Attendance
                    GROUP BY EnrolmentId
                ) att ON att.EnrolmentId = e.EnrolmentId
                WHERE ca.LecturerId   = @LecturerId
                  AND ca.AcademicYear = @Year
                  AND ca.Semester     = @Semester
                  AND e.Status        = 'Active'
                  AND (att.Pct < 80 OR att.Pct IS NULL)
                ORDER BY att.Pct ASC";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@LecturerId", lecturerId);
            cmd.Parameters.AddWithValue("@Year", currentYear);
            cmd.Parameters.AddWithValue("@Semester", currentSemester);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            gvAtRisk.DataSource = dt;
            gvAtRisk.DataBind();
        }

        // Simple helper: semester 1 = Jan-Apr, 2 = May-Aug, 3 = Sep-Dec
        int GetCurrentSemester()
        {
            int month = DateTime.Now.Month;
            if (month <= 4) return 1;
            if (month <= 8) return 2;
            return 3;
        }
    }
}