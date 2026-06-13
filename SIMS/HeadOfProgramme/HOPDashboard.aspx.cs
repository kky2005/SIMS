using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Web.UI;
using iTextSharp.text;
using iTextSharp.text.pdf;

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

        protected void btnExportReport_Click(object sender, EventArgs e)
        {
            DataTable reportData = ViewState["CurrentGeneratedReport"] as DataTable;
            string reportTitle = Convert.ToString(ViewState["CurrentGeneratedReportTitle"]);
            string reportType = Convert.ToString(ViewState["CurrentGeneratedReportType"]);
            string exportFormat = ddlExportFormat.SelectedValue.ToUpper();

            if (reportData == null || reportData.Rows.Count == 0)
            {
                lblExportMessage.Text = "Please generate a report with data before exporting.";
                lblExportMessage.Visible = true;
                return;
            }

            string folderPath = Server.MapPath("~/ReportExports/");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string safeTitle = MakeSafeFileName(reportTitle);
            string extension = exportFormat == "EXCEL" ? ".xls" : exportFormat == "PDF" ? ".pdf" : ".csv";
            string fileName = safeTitle + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + extension;
            string physicalPath = Path.Combine(folderPath, fileName);
            string virtualPath = "~/ReportExports/" + fileName;

            if (exportFormat == "CSV")
                WriteCsvFile(reportData, physicalPath);
            else if (exportFormat == "EXCEL")
                WriteExcelFile(reportData, physicalPath, reportTitle);
            else if (exportFormat == "PDF")
                WritePdfFile(reportData, physicalPath, reportTitle);
            else
            {
                lblExportMessage.Text = "Invalid export format selected.";
                lblExportMessage.Visible = true;
                return;
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                int reportId = GetOrCreateReportId(conn, reportTitle, reportType);
                int exportId = InsertReportExport(conn, reportId, exportFormat, virtualPath);
                InsertAuditLog(conn, "EXPORT_REPORT", "Exported " + reportTitle + " as " + exportFormat + ". ExportId: " + exportId, reportId);
            }

            DownloadFile(physicalPath, fileName, exportFormat);
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
                    LoadPendingTasks(conn);
                    LoadRecentActivity(conn);
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

        private void LoadPendingTasks(SqlConnection conn)
        {
            litPendingAdmissions.Text = TableExists(conn, "Admissions")
                ? GetScalarInt(conn, "SELECT COUNT(*) FROM Admissions WHERE Status = 'Pending'").ToString()
                : "0";

            litPendingEnrolments.Text = TableExists(conn, "Enrolments")
                ? GetScalarInt(conn, "SELECT COUNT(*) FROM Enrolments WHERE Status = 'Pending'").ToString()
                : "0";

            litArchivedEnrolments.Text = TableExists(conn, "Enrolments")
                ? GetScalarInt(conn, "SELECT COUNT(*) FROM Enrolments WHERE Status = 'Archived'").ToString()
                : "0";
        }

        private void LoadRecentActivity(SqlConnection conn)
        {
            if (!TableExists(conn, "AuditLogs"))
            {
                gvRecentActivity.DataSource = null;
                gvRecentActivity.DataBind();
                return;
            }

            gvRecentActivity.DataSource = GetData(conn, @"
                SELECT TOP 8
                    ISNULL(a.Action, '-') AS Action,
                    ISNULL(u.FullName, 'Unknown User') AS FullName,
                    a.ActionDate
                FROM AuditLogs a
                LEFT JOIN Users u ON u.UserId = a.UserId
                ORDER BY a.ActionDate DESC, a.LogId DESC");
            gvRecentActivity.DataBind();
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
            SetCurrentGeneratedReport(dt, litGeneratedReportTitle.Text, "Enrolment");
            BindGeneratedReport(dt);
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
            SetCurrentGeneratedReport(dt, litGeneratedReportTitle.Text, "Performance");
            BindGeneratedReport(dt);
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
            SetCurrentGeneratedReport(dt, litGeneratedReportTitle.Text, "Attendance");
            BindGeneratedReport(dt);
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
            ViewState["CurrentGeneratedReport"] = null;
            ViewState["CurrentGeneratedReportTitle"] = null;
            ViewState["CurrentGeneratedReportType"] = null;
            ViewState["GeneratedReportSortExpression"] = null;
            ViewState["GeneratedReportSortDirection"] = null;
            lblExportMessage.Visible = false;
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

        private void SetCurrentGeneratedReport(DataTable dt, string title, string reportType)
        {
            ViewState["CurrentGeneratedReport"] = dt;
            ViewState["CurrentGeneratedReportTitle"] = title;
            ViewState["CurrentGeneratedReportType"] = reportType;
            ViewState["GeneratedReportSortExpression"] = null;
            ViewState["GeneratedReportSortDirection"] = null;
            lblExportMessage.Visible = false;
        }

        private void BindGeneratedReport(DataTable dt)
        {
            gvGeneratedReport.DataSource = dt;
            gvGeneratedReport.DataBind();
        }

        protected void gvGeneratedReport_Sorting(object sender, System.Web.UI.WebControls.GridViewSortEventArgs e)
        {
            DataTable reportData = ViewState["CurrentGeneratedReport"] as DataTable;

            if (reportData == null || reportData.Rows.Count == 0)
                return;

            string sortExpression = e.SortExpression;
            string sortDirection = GetReportSortDirection(sortExpression);

            DataView view = reportData.DefaultView;
            view.Sort = "[" + sortExpression.Replace("]", "]]" ) + "] " + sortDirection;

            DataTable sortedData = view.ToTable();
            ViewState["CurrentGeneratedReport"] = sortedData;
            BindGeneratedReport(sortedData);

            litGeneratedReportMessage.Text = "Sorted by " + GetFriendlyReportHeader(sortExpression) + (sortDirection == "ASC" ? " ▲" : " ▼");
            litGeneratedReportMessage.Visible = true;
        }

        private string GetReportSortDirection(string sortExpression)
        {
            string lastExpression = Convert.ToString(ViewState["GeneratedReportSortExpression"]);
            string lastDirection = Convert.ToString(ViewState["GeneratedReportSortDirection"]);

            string newDirection = "ASC";

            if (lastExpression == sortExpression && lastDirection == "ASC")
                newDirection = "DESC";

            ViewState["GeneratedReportSortExpression"] = sortExpression;
            ViewState["GeneratedReportSortDirection"] = newDirection;

            return newDirection;
        }


        protected void gvGeneratedReport_RowCreated(object sender, System.Web.UI.WebControls.GridViewRowEventArgs e)
        {
            if (e.Row.RowType != System.Web.UI.WebControls.DataControlRowType.Header)
                return;

            string currentSort = Convert.ToString(ViewState["GeneratedReportSortExpression"]);
            string currentDirection = Convert.ToString(ViewState["GeneratedReportSortDirection"]);

            foreach (System.Web.UI.WebControls.TableCell cell in e.Row.Cells)
            {
                if (cell.Controls.Count == 0)
                    continue;

                System.Web.UI.WebControls.LinkButton headerLink = cell.Controls[0] as System.Web.UI.WebControls.LinkButton;
                if (headerLink == null)
                    continue;

                string sortExpression = headerLink.CommandArgument;
                string friendlyHeader = GetFriendlyReportHeader(sortExpression);

                string arrow = "↕";
                if (currentSort == sortExpression)
                {
                    arrow = currentDirection == "ASC" ? "▲" : "▼";
                    cell.CssClass = (cell.CssClass + " sorted-column").Trim();
                }

                headerLink.Text = friendlyHeader + " <span class='sort-arrow'>" + arrow + "</span>";
            }
        }

        private string GetFriendlyReportHeader(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                return "";

            switch (columnName)
            {
                case "StudentNo": return "Student No";
                case "FullName": return "Full Name";
                case "ProgrammeName": return "Programme";
                case "AcademicYear": return "Academic Year";
                case "CourseCode": return "Course Code";
                case "CourseName": return "Course Name";
                case "TotalEnrolments": return "Total Enrolments";
                case "UniqueStudents": return "Unique Students";
                case "PerformanceStatus": return "Performance Status";
                case "AttendanceDate": return "Attendance Date";
                default:
                    return SplitPascalCase(columnName);
            }
        }

        private string SplitPascalCase(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            StringBuilder result = new StringBuilder();
            result.Append(value[0]);

            for (int i = 1; i < value.Length; i++)
            {
                char current = value[i];
                char previous = value[i - 1];

                if (char.IsUpper(current) && !char.IsUpper(previous))
                    result.Append(' ');

                result.Append(current);
            }

            return result.ToString();
        }

        private void WriteCsvFile(DataTable dt, string physicalPath)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append(EscapeCsv(dt.Columns[i].ColumnName));
            }
            sb.AppendLine();

            foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (i > 0) sb.Append(",");
                    sb.Append(EscapeCsv(Convert.ToString(row[i])));
                }
                sb.AppendLine();
            }

            File.WriteAllText(physicalPath, sb.ToString(), Encoding.UTF8);
        }

        private void WriteExcelFile(DataTable dt, string physicalPath, string reportTitle)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<html><head><meta charset='utf-8'></head><body>");
            sb.Append("<h2>").Append(HttpUtility.HtmlEncode(reportTitle)).Append("</h2>");
            sb.Append("<table border='1'><tr>");

            foreach (DataColumn col in dt.Columns)
                sb.Append("<th>").Append(HttpUtility.HtmlEncode(col.ColumnName)).Append("</th>");

            sb.Append("</tr>");

            foreach (DataRow row in dt.Rows)
            {
                sb.Append("<tr>");
                foreach (DataColumn col in dt.Columns)
                    sb.Append("<td>").Append(HttpUtility.HtmlEncode(Convert.ToString(row[col]))).Append("</td>");
                sb.Append("</tr>");
            }

            sb.Append("</table></body></html>");
            File.WriteAllText(physicalPath, sb.ToString(), Encoding.UTF8);
        }

        private void WritePdfFile(DataTable dt, string physicalPath, string reportTitle)
        {
            Document document = new Document(PageSize.A4.Rotate(), 20f, 20f, 20f, 20f);
            PdfWriter.GetInstance(document, new FileStream(physicalPath, FileMode.Create));
            document.Open();

            Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
            Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
            Font bodyFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);

            document.Add(new Paragraph(reportTitle, titleFont));
            document.Add(new Paragraph("Exported at: " + DateTime.Now.ToString("dd MMM yyyy HH:mm")));
            document.Add(new Paragraph(" "));

            PdfPTable table = new PdfPTable(dt.Columns.Count);
            table.WidthPercentage = 100;

            foreach (DataColumn col in dt.Columns)
                table.AddCell(new Phrase(col.ColumnName, headerFont));

            foreach (DataRow row in dt.Rows)
            {
                foreach (DataColumn col in dt.Columns)
                    table.AddCell(new Phrase(Convert.ToString(row[col]), bodyFont));
            }

            document.Add(table);
            document.Close();
        }

        private string EscapeCsv(string value)
        {
            if (value == null) value = "";
            value = value.Replace("\"", "\"\"");
            return "\"" + value + "\"";
        }

        private string MakeSafeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) value = "Report";
            foreach (char c in Path.GetInvalidFileNameChars())
                value = value.Replace(c, '_');
            return value.Replace(" ", "_");
        }

        private void DownloadFile(string physicalPath, string fileName, string exportFormat)
        {
            string contentType = "text/csv";
            if (exportFormat == "EXCEL") contentType = "application/vnd.ms-excel";
            if (exportFormat == "PDF") contentType = "application/pdf";

            Response.Clear();
            Response.ContentType = contentType;
            Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
            Response.TransmitFile(physicalPath);
            Response.Flush();
            Response.End();
        }

        private int GetOrCreateReportId(SqlConnection conn, string reportTitle, string reportType)
        {
            if (!TableExists(conn, "Reports"))
                throw new Exception("Reports table was not found. ReportExports needs a valid ReportId.");

            string nameColumn = GetFirstExistingColumn(conn, "Reports", new string[] { "ReportName", "ReportTitle", "Title", "Name" });

            if (!string.IsNullOrEmpty(nameColumn))
            {
                using (SqlCommand findCmd = new SqlCommand("SELECT TOP 1 ReportId FROM Reports WHERE " + nameColumn + " = @ReportTitle ORDER BY ReportId DESC", conn))
                {
                    findCmd.Parameters.AddWithValue("@ReportTitle", reportTitle);
                    object existing = findCmd.ExecuteScalar();
                    if (existing != null && existing != DBNull.Value)
                        return Convert.ToInt32(existing);
                }
            }

            List<string> columns = new List<string>();
            List<string> values = new List<string>();

            if (!string.IsNullOrEmpty(nameColumn))
            {
                columns.Add(nameColumn);
                values.Add("@ReportTitle");
            }

            string typeColumn = GetFirstExistingColumn(conn, "Reports", new string[] { "ReportType", "Type", "Category" });
            if (!string.IsNullOrEmpty(typeColumn))
            {
                columns.Add(typeColumn);
                values.Add("@ReportType");
            }

            string userColumn = GetFirstExistingColumn(conn, "Reports", new string[] { "GeneratedBy", "CreatedBy", "UserId" });
            if (!string.IsNullOrEmpty(userColumn))
            {
                columns.Add(userColumn);
                values.Add("@UserId");
            }

            string dateColumn = GetFirstExistingColumn(conn, "Reports", new string[] { "GeneratedAt", "CreatedAt", "ReportDate" });
            if (!string.IsNullOrEmpty(dateColumn))
            {
                columns.Add(dateColumn);
                values.Add("SYSUTCDATETIME()");
            }

            if (columns.Count == 0)
            {
                using (SqlCommand fallback = new SqlCommand("SELECT TOP 1 ReportId FROM Reports ORDER BY ReportId DESC", conn))
                {
                    object result = fallback.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                        return Convert.ToInt32(result);
                }

                throw new Exception("Reports table has no supported report title columns. Add ReportName, ReportTitle, Title, or Name.");
            }

            string sql = "INSERT INTO Reports (" + string.Join(",", columns) + ") VALUES (" + string.Join(",", values) + "); SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@ReportTitle", reportTitle);
                cmd.Parameters.AddWithValue("@ReportType", reportType);
                cmd.Parameters.AddWithValue("@UserId", GetCurrentUserId());
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private int InsertReportExport(SqlConnection conn, int reportId, string exportFormat, string filePath)
        {
            using (SqlCommand cmd = new SqlCommand(@"
                INSERT INTO ReportExports (ReportId, ExportFormat, FilePath)
                VALUES (@ReportId, @ExportFormat, @FilePath);
                SELECT CAST(SCOPE_IDENTITY() AS INT);", conn))
            {
                cmd.Parameters.AddWithValue("@ReportId", reportId);
                cmd.Parameters.AddWithValue("@ExportFormat", exportFormat);
                cmd.Parameters.AddWithValue("@FilePath", filePath);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private void InsertAuditLog(SqlConnection conn, string action, string description, int reportId)
        {
            string auditTable = TableExists(conn, "AuditLogs") ? "AuditLogs" : TableExists(conn, "AuditLog") ? "AuditLog" : null;
            if (auditTable == null) return;

            List<string> columns = new List<string>();
            List<string> values = new List<string>();

            AddAuditColumn(conn, auditTable, columns, values, "UserId", "@UserId");
            AddAuditColumn(conn, auditTable, columns, values, "Action", "@Action");
            AddAuditColumn(conn, auditTable, columns, values, "ActionType", "@Action");
            AddAuditColumn(conn, auditTable, columns, values, "Description", "@Description");
            AddAuditColumn(conn, auditTable, columns, values, "Details", "@Description");
            AddAuditColumn(conn, auditTable, columns, values, "EntityName", "@EntityName");
            AddAuditColumn(conn, auditTable, columns, values, "EntityId", "@EntityId");
            AddAuditColumn(conn, auditTable, columns, values, "IpAddress", "@IpAddress");
            AddAuditColumn(conn, auditTable, columns, values, "CreatedAt", "SYSUTCDATETIME()");
            AddAuditColumn(conn, auditTable, columns, values, "AuditDate", "SYSUTCDATETIME()");

            if (columns.Count == 0) return;

            string sql = "INSERT INTO " + auditTable + " (" + string.Join(",", columns) + ") VALUES (" + string.Join(",", values) + ")";

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", GetCurrentUserId());
                cmd.Parameters.AddWithValue("@Action", action);
                cmd.Parameters.AddWithValue("@Description", description);
                cmd.Parameters.AddWithValue("@EntityName", "Reports");
                cmd.Parameters.AddWithValue("@EntityId", reportId);
                cmd.Parameters.AddWithValue("@IpAddress", Request.UserHostAddress ?? "");
                cmd.ExecuteNonQuery();
            }
        }

        private void AddAuditColumn(SqlConnection conn, string tableName, List<string> columns, List<string> values, string columnName, string valueExpression)
        {
            if (ColumnExists(conn, tableName, columnName))
            {
                columns.Add(columnName);
                values.Add(valueExpression);
            }
        }

        private string GetFirstExistingColumn(SqlConnection conn, string tableName, string[] columnNames)
        {
            foreach (string columnName in columnNames)
            {
                if (ColumnExists(conn, tableName, columnName))
                    return columnName;
            }
            return null;
        }

        private int GetCurrentUserId()
        {
            int userId;
            return Session["UserId"] != null && int.TryParse(Session["UserId"].ToString(), out userId) ? userId : 0;
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
            litPendingAdmissions.Text = "0";
            litPendingEnrolments.Text = "0";
            litArchivedEnrolments.Text = "0";
            if (gvRecentActivity != null)
            {
                gvRecentActivity.DataSource = null;
                gvRecentActivity.DataBind();
            }
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
