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
                conn.Open();

                // 1. My Courses Breakdown & Total Aggregations
                string coursesSql = @"
                    SELECT c.CourseCode, c.CourseName, 
                           ('Y' + CAST(ca.AcademicYear AS VARCHAR) + 'S' + CAST(ca.Semester AS VARCHAR)) as SemesterName
                    FROM CourseAssignments ca
                    INNER JOIN Courses c ON c.CourseId = ca.CourseId
                    WHERE ca.LecturerId = @LecturerId
                    ORDER BY ca.AcademicYear DESC, ca.Semester DESC";

                using (SqlCommand cmd = new SqlCommand(coursesSql, conn))
                {
                    cmd.Parameters.AddWithValue("@LecturerId", lecturerId);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dtCourses = new DataTable();
                    da.Fill(dtCourses);

                    litTotalCourses.Text = dtCourses.Rows.Count.ToString();
                    rptCoursesDetail.DataSource = dtCourses;
                    rptCoursesDetail.DataBind();
                }

                // 2. Total Students By Assigned Course Groupings
                string studentsSql = @"
                    SELECT c.CourseCode, c.CourseName, COUNT(e.StudentId) AS StudentCount
                    FROM CourseAssignments ca
                    INNER JOIN Courses c ON c.CourseId = ca.CourseId
                    LEFT JOIN Enrolments e ON e.CourseId = ca.CourseId 
                        AND e.AcademicYear = ca.AcademicYear 
                        AND e.Semester = ca.Semester 
                        AND e.Status = 'Active'
                    WHERE ca.LecturerId = @LecturerId
                    GROUP BY c.CourseCode, c.CourseName";

                using (SqlCommand cmd = new SqlCommand(studentsSql, conn))
                {
                    cmd.Parameters.AddWithValue("@LecturerId", lecturerId);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dtStudents = new DataTable();
                    da.Fill(dtStudents);

                    int totalStudentsCount = 0;
                    foreach (DataRow row in dtStudents.Rows)
                    {
                        totalStudentsCount += Convert.ToInt32(row["StudentCount"]);
                    }

                    litTotalStudents.Text = totalStudentsCount.ToString();
                    rptStudentsDetail.DataSource = dtStudents;
                    rptStudentsDetail.DataBind();
                }

                // 3. At Risk Students Breakdown by Course
                string riskSql = @"
                    SELECT c.CourseCode, c.CourseName, COUNT(DISTINCT e.StudentId) AS RiskCount
                    FROM Enrolments e
                    INNER JOIN CourseAssignments ca ON ca.CourseId = e.CourseId 
                        AND ca.AcademicYear = e.AcademicYear AND ca.Semester = e.Semester
                    INNER JOIN Courses c ON c.CourseId = e.CourseId
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
                    ) acad ON acad.StudentId = e.StudentId AND acad.CourseId = e.CourseId 
                          AND acad.AcademicYear = e.AcademicYear AND acad.Semester = e.Semester
                    WHERE ca.LecturerId = @LecturerId AND e.Status = 'Active'
                      AND ((att.Pct IS NOT NULL AND att.Pct < 80) OR (acad.AvgPct IS NOT NULL AND acad.AvgPct < 50))
                    GROUP BY c.CourseCode, c.CourseName";

                using (SqlCommand cmd = new SqlCommand(riskSql, conn))
                {
                    cmd.Parameters.AddWithValue("@LecturerId", lecturerId);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dtRisk = new DataTable();
                    da.Fill(dtRisk);

                    int totalRiskCount = 0;
                    foreach (DataRow row in dtRisk.Rows)
                    {
                        totalRiskCount += Convert.ToInt32(row["RiskCount"]);
                    }

                    litAtRisk.Text = totalRiskCount.ToString();
                    litAtRiskBadge.Text = totalRiskCount.ToString();
                    rptAtRiskDetail.DataSource = dtRisk;
                    rptAtRiskDetail.DataBind();
                }

                // 4. Pending Marks Evaluation Breakdown by Course
                string pendingSql = @"
                    SELECT c.CourseCode, c.CourseName, COUNT(*) AS PendingCount
                    FROM Enrolments e
                    INNER JOIN CourseAssignments ca ON ca.CourseId = e.CourseId 
                        AND ca.AcademicYear = e.AcademicYear AND ca.Semester = e.Semester
                    INNER JOIN Courses c ON c.CourseId = ca.CourseId
                    INNER JOIN Assessments a ON a.CourseId = ca.CourseId 
                        AND a.AcademicYear = ca.AcademicYear AND a.Semester = ca.Semester
                    LEFT JOIN StudentMarks sm ON sm.StudentId = e.StudentId AND sm.AssessmentId = a.AssessmentId
                    WHERE ca.LecturerId = @LecturerId AND e.Status = 'Active' AND (sm.MarksObtained IS NULL)
                    GROUP BY c.CourseCode, c.CourseName";

                using (SqlCommand cmd = new SqlCommand(pendingSql, conn))
                {
                    cmd.Parameters.AddWithValue("@LecturerId", lecturerId);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dtPending = new DataTable();
                    da.Fill(dtPending);

                    int totalPendingCount = 0;
                    foreach (DataRow row in dtPending.Rows)
                    {
                        totalPendingCount += Convert.ToInt32(row["PendingCount"]);
                    }

                    litPendingMarks.Text = totalPendingCount.ToString();
                    rptPendingDetail.DataSource = dtPending;
                    rptPendingDetail.DataBind();
                }
            }
        }

        // Keep core features running safely on background pipelines
        void LoadDashboardCourses()
        {
            int lecturerId = CurrentLecturerId;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"
                    SELECT c.CourseId, c.CourseCode, 
                           c.CourseName + ' (Yr ' + CAST(ca.AcademicYear AS VARCHAR) + ' / Sem ' + CAST(ca.Semester AS VARCHAR) + ')' AS CourseName, 
                           c.CreditHours, COUNT(e.EnrolmentId) AS TotalStudents
                    FROM CourseAssignments ca
                    INNER JOIN Courses c ON c.CourseId = ca.CourseId
                    LEFT JOIN Enrolments e ON e.CourseId = c.CourseId AND e.AcademicYear = ca.AcademicYear AND e.Semester = ca.Semester AND e.Status = 'Active'
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
                string sql = @"
                    SELECT TOP 10 s.StudentNo, u.FullName, c.CourseName + ' (Sem ' + CAST(e.Semester AS VARCHAR) + ')' AS CourseName,
                        ISNULL(FORMAT(att.Pct, 'N1'), 'No data') AS AttendancePct, ISNULL(FORMAT(acad.AvgPct, 'N1'), 'No marks') AS AcademicAvg,
                        CASE 
                            WHEN (att.Pct IS NOT NULL AND att.Pct < 80) AND acad.AvgPct < 50 THEN 'Critical (Attendance & Marks)'
                            WHEN (att.Pct IS NOT NULL AND att.Pct < 80) THEN 'Low Attendance (<80%)'
                            WHEN acad.AvgPct < 50 THEN 'Low Assessment Marks (<50%)'
                            ELSE 'Normal'
                        END AS RiskReason
                    FROM Enrolments e
                    INNER JOIN Students s ON s.StudentId = e.StudentId
                    INNER JOIN Users u ON u.UserId = s.UserId
                    INNER JOIN Courses c ON c.CourseId = e.CourseId
                    INNER JOIN CourseAssignments ca ON ca.CourseId = e.CourseId AND ca.AcademicYear = e.AcademicYear AND ca.Semester = e.Semester
                    LEFT JOIN (
                        SELECT EnrolmentId, 100.0 * SUM(CASE WHEN Status='Present' THEN 1 ELSE 0 END) / NULLIF(COUNT(*),0) AS Pct
                        FROM Attendance GROUP BY EnrolmentId
                    ) att ON att.EnrolmentId = e.EnrolmentId
                    LEFT JOIN (
                        SELECT sm.StudentId, a.CourseId, a.AcademicYear, a.Semester, 100.0 * SUM(sm.MarksObtained) / NULLIF(SUM(a.MaxMark), 0) AS AvgPct
                        FROM StudentMarks sm INNER JOIN Assessments a ON sm.AssessmentId = a.AssessmentId
                        GROUP BY sm.StudentId, a.CourseId, a.AcademicYear, a.Semester
                    ) acad ON acad.StudentId = e.StudentId AND acad.CourseId = e.CourseId AND acad.AcademicYear = e.AcademicYear AND acad.Semester = e.Semester
                    WHERE ca.LecturerId = @LecturerId AND e.Status = 'Active' AND (att.Pct IS NOT NULL AND att.Pct < 80 OR acad.AvgPct < 50)
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

                string riskSql = @"
                    SELECT 
                        SUM(CASE WHEN (att.Pct IS NOT NULL AND att.Pct < 80) AND (acad.AvgPct >= 50 OR acad.AvgPct IS NULL) THEN 1 ELSE 0 END) AS AttendanceRisk,
                        SUM(CASE WHEN (att.Pct >= 80) AND (acad.AvgPct < 50) THEN 1 ELSE 0 END) AS AcademicRisk,
                        SUM(CASE WHEN (att.Pct IS NOT NULL AND att.Pct < 80) AND (acad.AvgPct < 50) THEN 1 ELSE 0 END) AS CriticalRisk
                    FROM Enrolments e
                    INNER JOIN CourseAssignments ca ON ca.CourseId = e.CourseId AND ca.AcademicYear = e.AcademicYear AND ca.Semester = e.Semester
                    LEFT JOIN (
                        SELECT EnrolmentId, 100.0 * SUM(CASE WHEN Status='Present' THEN 1 ELSE 0 END) / NULLIF(COUNT(*),0) AS Pct
                        FROM Attendance GROUP BY EnrolmentId
                    ) att ON att.EnrolmentId = e.EnrolmentId
                    LEFT JOIN (
                        SELECT sm.StudentId, a.CourseId, a.AcademicYear, a.Semester, 100.0 * SUM(sm.MarksObtained) / NULLIF(SUM(a.MaxMark), 0) AS AvgPct
                        FROM StudentMarks sm INNER JOIN Assessments a ON sm.AssessmentId = a.AssessmentId
                        GROUP BY sm.StudentId, a.CourseId, a.AcademicYear, a.Semester
                    ) acad ON acad.StudentId = e.StudentId AND acad.CourseId = e.CourseId AND acad.AcademicYear = e.AcademicYear AND acad.Semester = e.Semester
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