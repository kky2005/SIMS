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

                // Get the operational term context (Database-driven fallback pattern)
                var operationalTerm = GetOperationalTermContext();
                int targetYear = operationalTerm.year;
                int targetSemester = operationalTerm.semester;

                LoadDashboardStats(targetYear, targetSemester);
                LoadDashboardCourses(targetYear, targetSemester);
                LoadAtRiskStudents(targetYear, targetSemester);
                LoadAnalyticsCharts(targetYear, targetSemester);
            }
        }

        private (int year, int semester) GetOperationalTermContext()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    // Query the database to find the latest active term records exist for this specific lecturer
                    string sql = @"
                        SELECT TOP 1 AcademicYear, Semester
                        FROM CourseAssignments
                        WHERE LecturerId = @LecturerId
                        ORDER BY AcademicYear DESC, Semester DESC, AssignedDate DESC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@LecturerId", CurrentLecturerId);
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return (Convert.ToInt32(reader["AcademicYear"]), Convert.ToInt32(reader["Semester"]));
                            }
                        }
                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving operational term: {ex.Message}");
            }

            // Safety net: Fallback to current calendar clock values if no historical data mapping is detected
            return (DateTime.Now.Year, GetCurrentSemester());
        }

        void LoadDashboardStats(int year, int semester)
        {
            int lecturerId = CurrentLecturerId;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
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

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@LecturerId", lecturerId);
                    cmd.Parameters.AddWithValue("@Year", year);
                    cmd.Parameters.AddWithValue("@Semester", semester);

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

            int atRisk = CountAtRiskStudents(lecturerId, year, semester);
            litAtRisk.Text = atRisk.ToString();
            litAtRiskBadge.Text = atRisk.ToString();

            litPendingMarks.Text = CountPendingAssessments(lecturerId, year, semester).ToString();
        }

        int CountAtRiskStudents(int lecturerId, int year, int semester)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
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
                        SELECT sm.StudentId, a.CourseId,
                               100.0 * SUM(sm.MarksObtained) / NULLIF(SUM(a.MaxMark), 0) AS AvgPct
                        FROM StudentMarks sm
                        INNER JOIN Assessments a ON sm.AssessmentId = a.AssessmentId
                        GROUP BY sm.StudentId, a.CourseId
                    ) acad ON acad.StudentId = e.StudentId AND acad.CourseId = e.CourseId
                    WHERE ca.LecturerId = @LecturerId
                      AND ca.AcademicYear = @Year
                      AND ca.Semester = @Semester
                      AND e.Status = 'Active'
                      AND (att.Pct < 80 OR att.Pct IS NULL OR acad.AvgPct < 50)";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@LecturerId", lecturerId);
                    cmd.Parameters.AddWithValue("@Year", year);
                    cmd.Parameters.AddWithValue("@Semester", semester);
                    conn.Open();
                    int count = (int)cmd.ExecuteScalar();
                    return count;
                }
            }
        }

        int CountPendingAssessments(int lecturerId, int year, int semester)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"
                    SELECT COUNT(*) FROM Assessments a
                    INNER JOIN CourseAssignments ca
                        ON ca.CourseId = a.CourseId AND ca.AcademicYear = a.AcademicYear AND ca.Semester = a.Semester
                    WHERE ca.LecturerId = @LecturerId
                      AND ca.AcademicYear = @Year
                      AND ca.Semester = @Semester
                      AND a.IsPublished = 0";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@LecturerId", lecturerId);
                    cmd.Parameters.AddWithValue("@Year", year);
                    cmd.Parameters.AddWithValue("@Semester", semester);
                    conn.Open();
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        void LoadDashboardCourses(int year, int semester)
        {
            int lecturerId = CurrentLecturerId;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"
                    SELECT
                        c.CourseId, c.CourseCode, c.CourseName, c.CreditHours,
                        COUNT(e.EnrolmentId) AS TotalStudents
                    FROM CourseAssignments ca
                    INNER JOIN Courses c ON c.CourseId = ca.CourseId
                    LEFT JOIN Enrolments e
                        ON e.CourseId = c.CourseId AND e.AcademicYear = ca.AcademicYear AND e.Semester = ca.Semester AND e.Status = 'Active'
                    WHERE ca.LecturerId   = @LecturerId
                      AND ca.AcademicYear = @Year
                      AND ca.Semester     = @Semester
                    GROUP BY c.CourseId, c.CourseCode, c.CourseName, c.CreditHours";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@LecturerId", lecturerId);
                    cmd.Parameters.AddWithValue("@Year", year);
                    cmd.Parameters.AddWithValue("@Semester", semester);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvDashboardCourses.DataSource = dt;
                    gvDashboardCourses.DataBind();
                }
            }
        }

        void LoadAtRiskStudents(int year, int semester)
        {
            int lecturerId = CurrentLecturerId;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"
                    SELECT TOP 10
                        s.StudentNo, u.FullName, c.CourseName,
                        ISNULL(CAST(ROUND(att.Pct,1) AS NVARCHAR(10)), 'No data') AS AttendancePct,
                        ISNULL(CAST(ROUND(acad.AvgPct,1) AS NVARCHAR(10)), 'No marks') AS AcademicAvg,
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
                        SELECT sm.StudentId, a.CourseId,
                               100.0 * SUM(sm.MarksObtained) / NULLIF(SUM(a.MaxMark), 0) AS AvgPct
                        FROM StudentMarks sm
                        INNER JOIN Assessments a ON sm.AssessmentId = a.AssessmentId
                        GROUP BY sm.StudentId, a.CourseId
                    ) acad ON acad.StudentId = e.StudentId AND acad.CourseId = e.CourseId
                    WHERE ca.LecturerId   = @LecturerId
                      AND ca.AcademicYear = @Year
                      AND ca.Semester     = @Semester
                      AND e.Status         = 'Active'
                      AND (att.Pct < 80 OR att.Pct IS NULL OR acad.AvgPct < 50)
                    ORDER BY CASE WHEN att.Pct IS NULL THEN 1 ELSE 0 END DESC, att.Pct ASC, acad.AvgPct ASC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@LecturerId", lecturerId);
                    cmd.Parameters.AddWithValue("@Year", year);
                    cmd.Parameters.AddWithValue("@Semester", semester);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvAtRisk.DataSource = dt;
                    gvAtRisk.DataBind();
                }
            }
        }

        void LoadAnalyticsCharts(int year, int semester)
        {
            int lecturerId = CurrentLecturerId;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Query 1: Calculate Course Performance Trends (Course Code vs Total Grade Percentage Average)
                string perfSql = @"
                    SELECT c.CourseCode, 
                           ISNULL(ROUND(100.0 * SUM(sm.MarksObtained) / NULLIF(SUM(a.MaxMark), 0), 1), 0) AS AvgMarkPct
                    FROM CourseAssignments ca
                    INNER JOIN Courses c ON c.CourseId = ca.CourseId
                    INNER JOIN Assessments a ON a.CourseId = ca.CourseId AND a.AcademicYear = ca.AcademicYear AND a.Semester = ca.Semester
                    INNER JOIN StudentMarks sm ON sm.AssessmentId = a.AssessmentId
                    WHERE ca.LecturerId = @LecturerId AND ca.AcademicYear = @Year AND ca.Semester = @Semester
                    GROUP BY c.CourseCode";

                using (SqlCommand cmd = new SqlCommand(perfSql, conn))
                {
                    cmd.Parameters.AddWithValue("@LecturerId", lecturerId);
                    cmd.Parameters.AddWithValue("@Year", year);
                    cmd.Parameters.AddWithValue("@Semester", semester);

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

                // Query 2: Segment students across different risk combinations
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
                        SELECT sm.StudentId, a.CourseId, 100.0 * SUM(sm.MarksObtained) / NULLIF(SUM(a.MaxMark), 0) AS AvgPct
                        FROM StudentMarks sm
                        INNER JOIN Assessments a ON sm.AssessmentId = a.AssessmentId
                        GROUP BY sm.StudentId, a.CourseId
                    ) acad ON acad.StudentId = e.StudentId AND acad.CourseId = e.CourseId
                    WHERE ca.LecturerId = @LecturerId AND ca.AcademicYear = @Year AND ca.Semester = @Semester AND e.Status = 'Active'";

                using (SqlCommand cmd = new SqlCommand(riskSql, conn))
                {
                    cmd.Parameters.AddWithValue("@LecturerId", lecturerId);
                    cmd.Parameters.AddWithValue("@Year", year);
                    cmd.Parameters.AddWithValue("@Semester", semester);

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

        int GetCurrentSemester()
        {
            int month = DateTime.Now.Month;
            if (month <= 4) return 1;
            if (month <= 8) return 2;
            return 3;
        }
    }
}