using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;

namespace SIMS.HeadOfProgramme
{
    public partial class Dashboard : HOPBase
    {
        private readonly string connStr = ConfigurationManager
            .ConnectionStrings["SIMS_DB"]
            .ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            EnsureAuthenticated();

            if (!IsPostBack)
            {
                BindFilterLists();
                LoadDashboardAnalytics();
            }
        }

        protected void btnApplyFilter_Click(object sender, EventArgs e)
        {
            LoadDashboardAnalytics();
            ClearGeneratedReport();
        }

        protected void btnEnrolmentReport_Click(object sender, EventArgs e)
        {
            LoadDashboardAnalytics();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                LoadEnrolmentReport(conn);
            }
        }

        protected void btnPerformanceReport_Click(object sender, EventArgs e)
        {
            LoadDashboardAnalytics();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                LoadPerformanceReport(conn);
            }
        }

        protected void btnAttendanceReport_Click(object sender, EventArgs e)
        {
            LoadDashboardAnalytics();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                LoadAttendanceReport(conn);
            }
        }

        private int? SelectedAcademicYear
        {
            get
            {
                int year;
                return int.TryParse(ddlAcademicYear.SelectedValue, out year) ? (int?)year : null;
            }
        }

        private int? SelectedSemester
        {
            get
            {
                int semester;
                return int.TryParse(ddlSemester.SelectedValue, out semester) ? (int?)semester : null;
            }
        }

        private void BindFilterLists()
        {
            ddlAcademicYear.Items.Clear();
            ddlAcademicYear.Items.Add(new System.Web.UI.WebControls.ListItem("All Academic Years", ""));

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    DataTable years = GetData(conn, @"
                        SELECT DISTINCT AcademicYear
                        FROM (
                            SELECT AcademicYear FROM Enrolments WHERE AcademicYear IS NOT NULL
                            UNION
                            SELECT AcademicYear FROM GPARecords WHERE AcademicYear IS NOT NULL
                            UNION
                            SELECT AcademicYear FROM AcademicCalendar WHERE AcademicYear IS NOT NULL
                        ) y
                        ORDER BY AcademicYear DESC");

                    foreach (DataRow row in years.Rows)
                    {
                        string year = row["AcademicYear"].ToString();
                        ddlAcademicYear.Items.Add(new System.Web.UI.WebControls.ListItem(year, year));
                    }
                }
            }
            catch
            {
                ddlAcademicYear.Items.Add(new System.Web.UI.WebControls.ListItem("2025", "2025"));
            }
        }

        private void LoadDashboardAnalytics()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    LoadMainCounts(conn);
                    LoadProgrammeChart(conn);
                    LoadCourseEnrolmentChart(conn);
                    LoadAttendanceSummary(conn);
                    LoadPerformanceSummary(conn);
                    LoadProgrammeReport(conn);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading dashboard analytics: " + ex.Message);
                SetSafeDefaults();
            }
        }

        private string EnrolmentFilterSql(string aliasPrefix)
        {
            string prefix = string.IsNullOrEmpty(aliasPrefix) ? "" : aliasPrefix + ".";
            List<string> filters = new List<string>();

            if (SelectedAcademicYear.HasValue) filters.Add(prefix + "AcademicYear = @AcademicYear");
            if (SelectedSemester.HasValue) filters.Add(prefix + "Semester = @Semester");

            return filters.Count == 0 ? "" : " WHERE " + string.Join(" AND ", filters);
        }

        private string GpaFilterSql(string aliasPrefix)
        {
            string prefix = string.IsNullOrEmpty(aliasPrefix) ? "" : aliasPrefix + ".";
            List<string> filters = new List<string>();

            if (SelectedAcademicYear.HasValue) filters.Add(prefix + "AcademicYear = @AcademicYear");
            if (SelectedSemester.HasValue) filters.Add(prefix + "Semester = @Semester");

            return filters.Count == 0 ? "" : " WHERE " + string.Join(" AND ", filters);
        }

        private void AddFilterParams(SqlCommand cmd)
        {
            if (SelectedAcademicYear.HasValue)
                cmd.Parameters.AddWithValue("@AcademicYear", SelectedAcademicYear.Value);

            if (SelectedSemester.HasValue)
                cmd.Parameters.AddWithValue("@Semester", SelectedSemester.Value);
        }

        private void LoadMainCounts(SqlConnection conn)
        {
            litProgrammesCount.Text = SafeCount(conn, "Programmes").ToString();
            litCoursesCount.Text = SafeCount(conn, "Courses").ToString();
            litStudentsCount.Text = SafeCount(conn, "Students").ToString();
            litLecturersCount.Text = SafeCount(conn, "Lecturers").ToString();
            litEnrolmentsCount.Text = GetScalarInt(conn, "SELECT COUNT(*) FROM Enrolments" + EnrolmentFilterSql("")).ToString();

            litActiveStudentsCount.Text = GetFilteredActiveStudents(conn).ToString();

            litStudentsAtRisk.Text = GetStudentsAtRisk(conn).ToString();
        }

        private int GetFilteredActiveStudents(SqlConnection conn)
        {
            if (!TableExists(conn, "Students")) return 0;

            bool hasStatus = ColumnExists(conn, "Students", "Status");

            // When no filter is selected, show the overall active student count.
            if (!SelectedAcademicYear.HasValue && !SelectedSemester.HasValue)
            {
                if (hasStatus)
                    return GetScalarInt(conn, "SELECT COUNT(*) FROM Students WHERE Status = 'Active'");

                return SafeCount(conn, "Students");
            }

            // When academic year/semester is selected, count only active students
            // who have enrolments in the selected academic period.
            if (!TableExists(conn, "Enrolments")) return 0;

            string statusFilter = hasStatus ? " AND s.Status = 'Active'" : "";

            string sql = @"
                SELECT COUNT(DISTINCT s.StudentId)
                FROM Students s
                INNER JOIN Enrolments e ON s.StudentId = e.StudentId" +
                EnrolmentFilterSql("e") + statusFilter;

            return GetScalarInt(conn, sql);
        }

        private int GetStudentsAtRisk(SqlConnection conn)
        {
            if (!TableExists(conn, "GPARecords") || !ColumnExists(conn, "GPARecords", "CGPA")) return 0;

            string sql = @"
                SELECT COUNT(*)
                FROM (
                    SELECT StudentId, MAX(CGPA) AS LatestCGPA
                    FROM GPARecords" + GpaFilterSql("") + @"
                    GROUP BY StudentId
                ) g
                WHERE g.LatestCGPA < 2.50";

            return GetScalarInt(conn, sql);
        }

        private void LoadProgrammeChart(SqlConnection conn)
        {
            DataTable dt;

            if (TableExists(conn, "Students") && TableExists(conn, "Programmes") &&
                ColumnExists(conn, "Students", "ProgrammeId") && ColumnExists(conn, "Programmes", "ProgrammeId"))
            {
                if (SelectedAcademicYear.HasValue || SelectedSemester.HasValue)
                {
                    dt = GetData(conn, @"
                        SELECT TOP 10 p.ProgrammeName, COUNT(DISTINCT e.StudentId) AS TotalStudents
                        FROM Programmes p
                        LEFT JOIN Students s ON p.ProgrammeId = s.ProgrammeId
                        LEFT JOIN Enrolments e ON s.StudentId = e.StudentId" +
                        EnrolmentFilterSql("e") + @"
                        GROUP BY p.ProgrammeName
                        ORDER BY TotalStudents DESC, p.ProgrammeName");
                }
                else
                {
                    dt = GetData(conn, @"
                        SELECT TOP 10 p.ProgrammeName, COUNT(s.StudentId) AS TotalStudents
                        FROM Programmes p
                        LEFT JOIN Students s ON p.ProgrammeId = s.ProgrammeId
                        GROUP BY p.ProgrammeName
                        ORDER BY TotalStudents DESC, p.ProgrammeName");
                }
            }
            else
            {
                dt = CreateSimpleData("ProgrammeName", "TotalStudents", "No Data", 0);
            }

            hfProgrammeLabelsJson.Value = ToJsonArray(dt, "ProgrammeName", true);
            hfProgrammeDataJson.Value = ToJsonArray(dt, "TotalStudents", false);
        }

        private void LoadCourseEnrolmentChart(SqlConnection conn)
        {
            DataTable dt;

            if (TableExists(conn, "Enrolments") && TableExists(conn, "Courses") &&
                ColumnExists(conn, "Enrolments", "CourseId") && ColumnExists(conn, "Courses", "CourseId"))
            {
                dt = GetData(conn, @"
                    SELECT TOP 8 
                        c.CourseCode + ' - ' + c.CourseName AS CourseLabel,
                        COUNT(e.EnrolmentId) AS Total
                    FROM Enrolments e
                    INNER JOIN Courses c ON e.CourseId = c.CourseId" + EnrolmentFilterSql("e") + @"
                    GROUP BY c.CourseCode, c.CourseName
                    ORDER BY Total DESC, c.CourseCode");
            }
            else
            {
                dt = CreateSimpleData("CourseLabel", "Total", "No Enrolment Data", 0);
            }

            hfStatusLabelsJson.Value = ToJsonArray(dt, "CourseLabel", true);
            hfStatusDataJson.Value = ToJsonArray(dt, "Total", false);
        }

        private void LoadAttendanceSummary(SqlConnection conn)
        {
            if (!TableExists(conn, "Attendance") || !ColumnExists(conn, "Attendance", "Status"))
            {
                litAttendanceRate.Text = "N/A";
                hfAttendanceLabelsJson.Value = "[\"No Attendance Data\"]";
                hfAttendanceDataJson.Value = "[0]";
                pnlAttendanceNote.Visible = true;
                return;
            }

            string joinFilter = EnrolmentFilterSql("e");
            string sql = @"
                SELECT ISNULL(a.Status, 'Unknown') AS Status, COUNT(*) AS Total
                FROM Attendance a
                INNER JOIN Enrolments e ON a.EnrolmentId = e.EnrolmentId" + joinFilter + @"
                GROUP BY a.Status
                ORDER BY Total DESC";

            DataTable dt = GetData(conn, sql);

            hfAttendanceLabelsJson.Value = ToJsonArray(dt, "Status", true);
            hfAttendanceDataJson.Value = ToJsonArray(dt, "Total", false);

            int total = GetScalarInt(conn, @"
                SELECT COUNT(*)
                FROM Attendance a
                INNER JOIN Enrolments e ON a.EnrolmentId = e.EnrolmentId" + joinFilter);

            string presentFilter = joinFilter;
            if (string.IsNullOrEmpty(presentFilter))
                presentFilter = " WHERE a.Status IN ('Present', 'P')";
            else
                presentFilter += " AND a.Status IN ('Present', 'P')";

            int present = GetScalarInt(conn, @"
                SELECT COUNT(*)
                FROM Attendance a
                INNER JOIN Enrolments e ON a.EnrolmentId = e.EnrolmentId" + presentFilter);

            litAttendanceRate.Text = total == 0 ? "0%" : Math.Round((present * 100.0) / total, 1) + "%";
            pnlAttendanceNote.Visible = total == 0;
        }

        private void LoadPerformanceSummary(SqlConnection conn)
        {
            if (TableExists(conn, "GPARecords") && ColumnExists(conn, "GPARecords", "CGPA"))
            {
                double avg = GetScalarDouble(conn, "SELECT AVG(CAST(CGPA AS FLOAT)) FROM GPARecords" + GpaFilterSql(""));
                litAveragePerformance.Text = avg <= 0 ? "N/A" : Math.Round(avg, 2).ToString("0.00");

                DataTable dt = GetData(conn, @"
                    SELECT 
                        CASE 
                            WHEN CGPA >= 3.50 THEN '3.50 - 4.00'
                            WHEN CGPA >= 3.00 THEN '3.00 - 3.49'
                            WHEN CGPA >= 2.50 THEN '2.50 - 2.99'
                            WHEN CGPA >= 2.00 THEN '2.00 - 2.49'
                            ELSE 'Below 2.00'
                        END AS RangeName,
                        COUNT(*) AS Total
                    FROM GPARecords" + GpaFilterSql("") + @"
                    GROUP BY 
                        CASE 
                            WHEN CGPA >= 3.50 THEN '3.50 - 4.00'
                            WHEN CGPA >= 3.00 THEN '3.00 - 3.49'
                            WHEN CGPA >= 2.50 THEN '2.50 - 2.99'
                            WHEN CGPA >= 2.00 THEN '2.00 - 2.49'
                            ELSE 'Below 2.00'
                        END
                    ORDER BY MIN(CGPA)");

                hfPerformanceLabelsJson.Value = ToJsonArray(dt, "RangeName", true);
                hfPerformanceDataJson.Value = ToJsonArray(dt, "Total", false);
                pnlPerformanceNote.Visible = false;
                return;
            }

            litAveragePerformance.Text = "N/A";
            hfPerformanceLabelsJson.Value = "[\"No Performance Data\"]";
            hfPerformanceDataJson.Value = "[0]";
            pnlPerformanceNote.Visible = true;
        }

        private void LoadProgrammeReport(SqlConnection conn)
        {
            if (TableExists(conn, "Students") && TableExists(conn, "Programmes") &&
                ColumnExists(conn, "Students", "ProgrammeId") && ColumnExists(conn, "Students", "Status"))
            {
                gvProgrammeReport.DataSource = GetData(conn, @"
                    SELECT TOP 10 
                        p.ProgrammeName,
                        COUNT(s.StudentId) AS TotalStudents,
                        SUM(CASE WHEN s.Status = 'Active' THEN 1 ELSE 0 END) AS ActiveStudents
                    FROM Programmes p
                    LEFT JOIN Students s ON p.ProgrammeId = s.ProgrammeId
                    GROUP BY p.ProgrammeName
                    ORDER BY TotalStudents DESC, p.ProgrammeName");
            }
            else
            {
                gvProgrammeReport.DataSource = null;
            }

            gvProgrammeReport.DataBind();
        }

        private void LoadEnrolmentReport(SqlConnection conn)
        {
            litGeneratedReportTitle.Text = "Generated Enrolment Statistics Report";
            gvGeneratedReport.EmptyDataText = "No enrolment records found for the selected academic year and semester.";
            DataTable dt = GetData(conn, @"
                SELECT 
                    p.ProgrammeName AS Programme,
                    c.CourseCode,
                    c.CourseName,
                    COUNT(e.EnrolmentId) AS TotalEnrolments,
                    COUNT(DISTINCT e.StudentId) AS UniqueStudents
                FROM Enrolments e
                INNER JOIN Courses c ON e.CourseId = c.CourseId
                INNER JOIN Programmes p ON c.ProgrammeId = p.ProgrammeId" + EnrolmentFilterSql("e") + @"
                GROUP BY p.ProgrammeName, c.CourseCode, c.CourseName
                ORDER BY p.ProgrammeName, c.CourseCode");
            gvGeneratedReport.DataSource = dt;
            gvGeneratedReport.DataBind();
            ShowReportMessage(dt.Rows.Count == 0, "No enrolment records found for the selected filter.");
        }

        private void LoadPerformanceReport(SqlConnection conn)
        {
            litGeneratedReportTitle.Text = "Generated Student Performance Report";
            gvGeneratedReport.EmptyDataText = "No performance records found for the selected academic year and semester.";
            DataTable dt = GetData(conn, @"
                SELECT 
                    s.StudentNo,
                    u.FullName,
                    p.ProgrammeName,
                    g.AcademicYear,
                    g.Semester,
                    g.GPA,
                    g.CGPA,
                    CASE WHEN g.CGPA < 2.50 THEN 'At Risk' ELSE 'Good Standing' END AS PerformanceStatus
                FROM GPARecords g
                INNER JOIN Students s ON g.StudentId = s.StudentId
                INNER JOIN Users u ON s.UserId = u.UserId
                INNER JOIN Programmes p ON s.ProgrammeId = p.ProgrammeId" + GpaFilterSql("g") + @"
                ORDER BY g.CGPA DESC, u.FullName");
            gvGeneratedReport.DataSource = dt;
            gvGeneratedReport.DataBind();
            ShowReportMessage(dt.Rows.Count == 0, "No student performance records found for the selected filter.");
        }

        private void LoadAttendanceReport(SqlConnection conn)
        {
            litGeneratedReportTitle.Text = "Generated Attendance Summary Report";
            gvGeneratedReport.EmptyDataText = "No attendance records found for the selected academic year and semester.";
            DataTable dt = GetData(conn, @"
                SELECT 
                    s.StudentNo,
                    u.FullName,
                    c.CourseCode,
                    c.CourseName,
                    a.AttendanceDate,
                    a.Status,
                    a.Remarks
                FROM Attendance a
                INNER JOIN Enrolments e ON a.EnrolmentId = e.EnrolmentId
                INNER JOIN Students s ON e.StudentId = s.StudentId
                INNER JOIN Users u ON s.UserId = u.UserId
                INNER JOIN Courses c ON e.CourseId = c.CourseId" + EnrolmentFilterSql("e") + @"
                ORDER BY a.AttendanceDate DESC, u.FullName");
            gvGeneratedReport.DataSource = dt;
            gvGeneratedReport.DataBind();
            ShowReportMessage(dt.Rows.Count == 0, "No attendance records found for the selected filter.");
        }

        private void ClearGeneratedReport()
        {
            litGeneratedReportTitle.Text = "Generated Report";
            litGeneratedReportMessage.Text = "Choose a report above to view institutional data for the selected filter.";
            litGeneratedReportMessage.Visible = true;
            gvGeneratedReport.EmptyDataText = "Click a report button above to generate a report.";
            gvGeneratedReport.DataSource = null;
            gvGeneratedReport.DataBind();
        }

        private void ShowReportMessage(bool showEmptyMessage, string emptyMessage)
        {
            if (showEmptyMessage)
            {
                litGeneratedReportMessage.Text = emptyMessage;
                litGeneratedReportMessage.Visible = true;
            }
            else
            {
                litGeneratedReportMessage.Text = "Report generated successfully.";
                litGeneratedReportMessage.Visible = true;
            }
        }

        private int SafeCount(SqlConnection conn, string tableName)
        {
            if (!TableExists(conn, tableName)) return 0;
            return GetScalarInt(conn, "SELECT COUNT(*) FROM [" + tableName + "]");
        }

        private bool TableExists(SqlConnection conn, string tableName)
        {
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = @TableName", conn))
            {
                cmd.Parameters.AddWithValue("@TableName", tableName);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private bool ColumnExists(SqlConnection conn, string tableName, string columnName)
        {
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT COUNT(*)
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = 'dbo' 
                  AND TABLE_NAME = @TableName 
                  AND COLUMN_NAME = @ColumnName", conn))
            {
                cmd.Parameters.AddWithValue("@TableName", tableName);
                cmd.Parameters.AddWithValue("@ColumnName", columnName);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private int GetScalarInt(SqlConnection conn, string sql)
        {
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                AddFilterParams(cmd);
                object result = cmd.ExecuteScalar();
                return result == DBNull.Value || result == null ? 0 : Convert.ToInt32(result);
            }
        }

        private double GetScalarDouble(SqlConnection conn, string sql)
        {
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                AddFilterParams(cmd);
                object result = cmd.ExecuteScalar();
                return result == DBNull.Value || result == null ? 0 : Convert.ToDouble(result);
            }
        }

        private DataTable GetData(SqlConnection conn, string sql)
        {
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                AddFilterParams(cmd);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        private DataTable CreateSimpleData(string labelColumn, string valueColumn, string label, int value)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(labelColumn);
            dt.Columns.Add(valueColumn, typeof(int));
            dt.Rows.Add(label, value);
            return dt;
        }

        private string ToJsonArray(DataTable dt, string columnName, bool quoteString)
        {
            if (dt == null || dt.Rows.Count == 0) return "[]";

            List<string> values = new List<string>();

            foreach (DataRow row in dt.Rows)
            {
                object raw = row[columnName];

                if (quoteString)
                {
                    values.Add("\"" + JavaScriptStringEncode(Convert.ToString(raw)) + "\"");
                }
                else
                {
                    int number = 0;
                    int.TryParse(Convert.ToString(raw), out number);
                    values.Add(number.ToString());
                }
            }

            return "[" + string.Join(",", values) + "]";
        }

        private string JavaScriptStringEncode(string value)
        {
            if (value == null) return "";

            StringBuilder sb = new StringBuilder();

            foreach (char c in value)
            {
                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '"': sb.Append("\\\""); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default: sb.Append(c); break;
                }
            }

            return sb.ToString();
        }

        private void SetSafeDefaults()
        {
            litProgrammesCount.Text = "0";
            litCoursesCount.Text = "0";
            litStudentsCount.Text = "0";
            litLecturersCount.Text = "0";
            litEnrolmentsCount.Text = "0";
            litActiveStudentsCount.Text = "0";
            litStudentsAtRisk.Text = "0";
            litAttendanceRate.Text = "N/A";
            litAveragePerformance.Text = "N/A";

            hfProgrammeLabelsJson.Value = "[\"No Data\"]";
            hfProgrammeDataJson.Value = "[0]";
            hfStatusLabelsJson.Value = "[\"No Data\"]";
            hfStatusDataJson.Value = "[0]";
            hfAttendanceLabelsJson.Value = "[\"No Data\"]";
            hfAttendanceDataJson.Value = "[0]";
            hfPerformanceLabelsJson.Value = "[\"No Data\"]";
            hfPerformanceDataJson.Value = "[0]";

            pnlAttendanceNote.Visible = true;
            pnlPerformanceNote.Visible = true;
            ClearGeneratedReport();
        }
    }
}
