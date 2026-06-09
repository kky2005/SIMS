using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI.WebControls;

namespace SIMS.Lecturer
{
    public partial class LecturerDashboard : LecturerBase
    {
        string connStr = ConfigurationManager.ConnectionStrings["SIMS_DB"].ConnectionString;

        // Public properties to pass structured metrics down to front-end JavaScript handlers safely
        public string PerformanceJsonData { get; set; } = "[]";
        public string RiskJsonData { get; set; } = "{ AttendanceRisk: 0, AcademicRisk: 0, CriticalRisk: 0 }";

        protected void Page_Load(object sender, EventArgs e)
        {
            EnsureAuthenticated();

            if (!IsPostBack)
            {
                litName.Text = CurrentFullName;
                litDept.Text = CurrentDepartment;
                litStaffNo.Text = CurrentStaffNo;
                litDate.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy");

                // Execute all metrics pipelines globally bounded by all courses assigned to this lecturer
                LoadDashboardStats();
                LoadDashboardCourses();
                LoadAtRiskStudents();
                LoadAnalyticsCharts();
            }
        }

        void LoadDashboardStats()
        {
            int lecturerId = CurrentLecturerId;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Updated to calculate totals across all active course terms assigned to this lecturer
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
                    WHERE ca.LecturerId   = @LecturerId";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@LecturerId", lecturerId);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        litTotalCourses.Text = dt.Rows[0]["TotalCourses"].ToString();
                        litTotalStudents.Text = dt.Rows[0]["TotalStudents"].ToString();
                    }
                }
            }

            int atRisk = CountAtRiskStudents(lecturerId);
            litAtRisk.Text = atRisk.ToString();
            litAtRiskBadge.Text = atRisk.ToString();

            litPendingMarks.Text = CountPendingAssessments(lecturerId).ToString();
        }

        int CountAtRiskStudents(int lecturerId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Evaluates attendance and marks matching the exact courses and semesters assigned to this lecturer
                string sql = @"
                    SELECT COUNT(DISTINCT e.StudentId) 
                    FROM Enrolments e
                    INNER JOIN CourseAssignments ca
                        ON ca.CourseId = e.CourseId AND ca.AcademicYear = e.AcademicYear AND ca.Semester = e.Semester
                    LEFT JOIN (
                        SELECT EnrolmentId,
                               100.0 * SUM(CASE WHEN Status='Present' THEN 1 ELSE 0 END) / NULLIF(COUNT(*),0) AS Pct
                        FROM Attendance GROUP BY EnrolmentId
                    ) att ON att.EnrolmentId = e.EnrolmentId
                    LEFT JOIN (
                        SELECT sm.StudentId, a.CourseId, a.AcademicYear, a.Semester,
                               100.0 * SUM(sm.MarksObtained) / NULLIF(SUM(a.MaxMark), 0) AS AvgPct
                        FROM StudentMarks sm
                        INNER JOIN Assessments a ON sm.AssessmentId = a.AssessmentId
                        GROUP BY sm.StudentId, a.CourseId, a.AcademicYear, a.Semester
                    ) acad ON acad.StudentId = e.StudentId 
                          AND acad.CourseId = e.CourseId 
                          AND acad.AcademicYear = e.AcademicYear 
                          AND acad.Semester = e.Semester
                    WHERE ca.LecturerId = @LecturerId
                      AND e.Status = 'Active'
                      AND (att.Pct < 80 OR att.Pct IS NULL OR acad.AvgPct < 50)";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@LecturerId", lecturerId);
                    conn.Open();
                    int count = (int)cmd.ExecuteScalar();
                    return count;
                }
            }
        }

        int CountPendingAssessments(int lecturerId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Counts total student assessments that are missing marks or ungraded
                // across all active enrolments for this lecturer's assigned courses.
                string sql = @"
            SELECT COUNT(*) 
            FROM Enrolments e
            INNER JOIN CourseAssignments ca
                ON ca.CourseId = e.CourseId 
               AND ca.AcademicYear = e.AcademicYear 
               AND ca.Semester = e.Semester
            INNER JOIN Assessments a 
                ON a.CourseId = ca.CourseId 
               AND a.AcademicYear = ca.AcademicYear 
               AND a.Semester = ca.Semester
            LEFT JOIN StudentMarks sm 
                ON sm.StudentId = e.StudentId 
               AND sm.AssessmentId = a.AssessmentId
            WHERE ca.LecturerId = @LecturerId
              AND e.Status = 'Active'
              AND (sm.MarksObtained IS NULL)"; // Triggers when no mark record exists or mark is explicitly null

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@LecturerId", lecturerId);
                    conn.Open();
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        void LoadDashboardCourses()
        {
            int lecturerId = CurrentLecturerId;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Displays all assigned courses regardless of semester, showing their respective term properties
                string sql = @"
                    SELECT
                        c.CourseId, 
                        c.CourseCode, 
                        c.CourseName + ' (Yr ' + CAST(ca.AcademicYear AS VARCHAR) + ' / Sem ' + CAST(ca.Semester AS VARCHAR) + ')' AS CourseName, 
                        c.CreditHours,
                        COUNT(e.EnrolmentId) AS TotalStudents
                    FROM CourseAssignments ca
                    INNER JOIN Courses c ON c.CourseId = ca.CourseId
                    LEFT JOIN Enrolments e
                        ON e.CourseId = c.CourseId AND e.AcademicYear = ca.AcademicYear AND e.Semester = ca.Semester AND e.Status = 'Active'
                    WHERE ca.LecturerId = @LecturerId
                    GROUP BY c.CourseId, c.CourseCode, c.CourseName, c.CreditHours, ca.AcademicYear, ca.Semester
                    ORDER BY ca.AcademicYear DESC, ca.Semester DESC, c.CourseCode ASC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@LecturerId", lecturerId);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvDashboardCourses.DataSource = dt;
                    gvDashboardCourses.DataBind();
                }
            }
        }

        void LoadAtRiskStudents()
        {
            int lecturerId = CurrentLecturerId;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Returns top 10 at risk students across all active courses assigned to the lecturer
                string sql = @"
                    SELECT TOP 10
                        s.StudentNo, u.FullName, 
                        c.CourseName + ' (Sem ' + CAST(e.Semester AS VARCHAR) + ')' AS CourseName,
                        ISNULL(FORMAT(att.Pct, 'N1'), 'No data') AS AttendancePct,
                        ISNULL(FORMAT(acad.AvgPct, 'N1'), 'No marks') AS AcademicAvg,
                        CASE 
                            WHEN (att.Pct < 80 OR att.Pct IS NULL) AND acad.AvgPct < 50 THEN 'Critical (Attendance & Marks)'
                            WHEN (att.Pct < 80 OR att.Pct IS NULL) THEN 'Low Attendance (<80%)'
                            WHEN acad.AvgPct < 50 THEN 'Low Assessment Marks (<50%)'
                            ELSE 'Normal'
                        END AS RiskReason
                    FROM Enrolments e
                    INNER JOIN Students s   ON s.StudentId = e.StudentId
                    INNER JOIN Users u      ON u.UserId    = s.UserId
                    INNER JOIN Courses c    ON c.CourseId  = e.CourseId
                    INNER JOIN CourseAssignments ca
                        ON ca.CourseId = e.CourseId AND ca.AcademicYear = e.AcademicYear AND ca.Semester = e.Semester
                    LEFT JOIN (
                        SELECT EnrolmentId,
                               100.0 * SUM(CASE WHEN Status='Present' THEN 1 ELSE 0 END) / NULLIF(COUNT(*),0) AS Pct
                        FROM Attendance GROUP BY EnrolmentId
                    ) att ON att.EnrolmentId = e.EnrolmentId
                    LEFT JOIN (
                        SELECT sm.StudentId, a.CourseId, a.AcademicYear, a.Semester,
                               100.0 * SUM(sm.MarksObtained) / NULLIF(SUM(a.MaxMark), 0) AS AvgPct
                        FROM StudentMarks sm
                        INNER JOIN Assessments a ON sm.AssessmentId = a.AssessmentId
                        GROUP BY sm.StudentId, a.CourseId, a.AcademicYear, a.Semester
                    ) acad ON acad.StudentId = e.StudentId 
                          AND acad.CourseId = e.CourseId 
                          AND acad.AcademicYear = e.AcademicYear 
                          AND acad.Semester = e.Semester
                    WHERE ca.LecturerId   = @LecturerId
                      AND e.Status         = 'Active'
                      AND (att.Pct < 80 OR att.Pct IS NULL OR acad.AvgPct < 50)
                    ORDER BY CASE WHEN att.Pct IS NULL THEN 1 ELSE 0 END DESC, att.Pct ASC, acad.AvgPct ASC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@LecturerId", lecturerId);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvAtRisk.DataSource = dt;
                    gvAtRisk.DataBind();
                }
            }
        }

        void LoadAnalyticsCharts()
        {
            int lecturerId = CurrentLecturerId;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Query 1: Calculate Course Performance Trends with distinct Course Code and Semester markers
                string perfSql = @"
                    SELECT c.CourseCode + ' (S' + CAST(ca.Semester AS VARCHAR) + ')' AS CourseCode, 
                           ISNULL(ROUND(100.0 * SUM(sm.MarksObtained) / NULLIF(SUM(a.MaxMark), 0), 1), 0) AS AvgMarkPct
                    FROM CourseAssignments ca
                    INNER JOIN Courses c ON c.CourseId = ca.CourseId
                    INNER JOIN Assessments a ON a.CourseId = ca.CourseId AND a.AcademicYear = ca.AcademicYear AND a.Semester = ca.Semester
                    INNER JOIN StudentMarks sm ON sm.AssessmentId = a.AssessmentId
                    WHERE ca.LecturerId = @LecturerId
                    GROUP BY c.CourseCode, ca.Semester";

                using (SqlCommand cmd = new SqlCommand(perfSql, conn))
                {
                    cmd.Parameters.AddWithValue("@LecturerId", lecturerId);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    StringBuilder sb = new StringBuilder();
                    sb.Append("[");
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        sb.Append("{");
                        sb.AppendFormat("\"CourseCode\":\"{0}\",\"AvgMarkPct\":{1}", dt.Rows[i]["CourseCode"], dt.Rows[i]["AvgMarkPct"]);
                        sb.Append("}");
                        if (i < dt.Rows.Count - 1) sb.Append(",");
                    }
                    sb.Append("]");
                    PerformanceJsonData = sb.ToString();
                }

                // Query 2: Segment students across different risk combinations globally across assigned courses
                string riskSql = @"
                    SELECT 
                        SUM(CASE WHEN (att.Pct < 80 OR att.Pct IS NULL) AND (acad.AvgPct >= 50 OR acad.AvgPct IS NULL) THEN 1 ELSE 0 END) AS AttendanceRisk,
                        SUM(CASE WHEN (att.Pct >= 80) AND (acad.AvgPct < 50) THEN 1 ELSE 0 END) AS AcademicRisk,
                        SUM(CASE WHEN (att.Pct < 80 OR att.Pct IS NULL) AND (acad.AvgPct < 50) THEN 1 ELSE 0 END) AS CriticalRisk
                    FROM Enrolments e
                    INNER JOIN CourseAssignments ca ON ca.CourseId = e.CourseId AND ca.AcademicYear = e.AcademicYear AND ca.Semester = e.Semester
                    LEFT JOIN (
                        SELECT EnrolmentId, 100.0 * SUM(CASE WHEN Status='Present' THEN 1 ELSE 0 END) / NULLIF(COUNT(*),0) AS Pct
                        FROM Attendance GROUP BY EnrolmentId
                    ) att ON att.EnrolmentId = e.EnrolmentId
                    LEFT JOIN (
                        SELECT sm.StudentId, a.CourseId, a.AcademicYear, a.Semester, 
                               100.0 * SUM(sm.MarksObtained) / NULLIF(SUM(a.MaxMark), 0) AS AvgPct
                        FROM StudentMarks sm
                        INNER JOIN Assessments a ON sm.AssessmentId = a.AssessmentId
                        GROUP BY sm.StudentId, a.CourseId, a.AcademicYear, a.Semester
                    ) acad ON acad.StudentId = e.StudentId 
                          AND acad.CourseId = e.CourseId 
                          AND acad.AcademicYear = e.AcademicYear 
                          AND acad.Semester = e.Semester
                    WHERE ca.LecturerId = @LecturerId AND e.Status = 'Active'";

                using (SqlCommand cmd = new SqlCommand(riskSql, conn))
                {
                    cmd.Parameters.AddWithValue("@LecturerId", lecturerId);

                    conn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            int attRisk = rdr["AttendanceRisk"] != DBNull.Value ? Convert.ToInt32(rdr["AttendanceRisk"]) : 0;
                            int acadRisk = rdr["AcademicRisk"] != DBNull.Value ? Convert.ToInt32(rdr["AcademicRisk"]) : 0;
                            int critRisk = rdr["CriticalRisk"] != DBNull.Value ? Convert.ToInt32(rdr["CriticalRisk"]) : 0;

                            RiskJsonData = string.Format("{{ \"AttendanceRisk\": {0}, \"AcademicRisk\": {1}, \"CriticalRisk\": {2} }}", attRisk, acadRisk, critRisk);
                        }
                    }
                }
            }
        }

        protected string GetRiskBadgeClass(string reason)
        {
            if (reason.Contains("Critical")) return "badge bg-danger text-white";
            if (reason.Contains("Attendance")) return "badge bg-warning text-dark";
            return "badge bg-info text-white";
        }
    }
}