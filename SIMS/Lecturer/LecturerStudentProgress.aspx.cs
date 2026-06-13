using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SIMS.Lecturer
{
    public partial class LecturerStudentProgress : LecturerBase
    {
        private readonly string connStr = ConfigurationManager.ConnectionStrings["SIMS_DB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            EnsureAuthenticated();

            if (!IsPostBack)
            {
                LoadCourses();
                ExecuteSearch();
                LoadReportHistoryLogs();
            }
        }

        private void LoadCourses()
        {
            try
            {
                int lecturerId = CurrentLecturerId;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = @"
                        SELECT DISTINCT
                            c.CourseId,
                            c.CourseCode,
                            c.CourseName,
                            ca.AcademicYear,
                            ca.Semester
                        FROM CourseAssignments ca
                        INNER JOIN Courses c ON c.CourseId = ca.CourseId
                        WHERE ca.LecturerId = @LecturerId
                        ORDER BY ca.AcademicYear DESC, ca.Semester DESC, c.CourseCode ASC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@LecturerId", lecturerId);

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            dt.Columns.Add("DisplayTitle", typeof(string));
                            foreach (DataRow row in dt.Rows)
                            {
                                row["DisplayTitle"] = $"{row["CourseCode"]} - {row["CourseName"]} (Yr {row["AcademicYear"]} / Sem {row["Semester"]})";
                            }

                            ddlCourse.DataSource = dt;
                            ddlCourse.DataTextField = "DisplayTitle";
                            ddlCourse.DataValueField = "CourseId";
                            ddlCourse.DataBind();

                            ddlReportCourse.DataSource = dt;
                            ddlReportCourse.DataTextField = "DisplayTitle";
                            ddlReportCourse.DataValueField = "CourseId";
                            ddlReportCourse.DataBind();
                        }
                    }
                }

                ddlCourse.Items.Insert(0, new ListItem("-- All Assigned Courses --", "0"));
                ddlReportCourse.Items.Insert(0, new ListItem("-- Entire Course Workloads --", "0"));
            }
            catch (Exception ex)
            {
                ShowSystemFeedback($"Error loading associated courses: {ex.Message}", true);
            }
        }

        protected void SwitchTab_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int targetView = int.Parse(btn.CommandArgument);
            mvProgressViews.ActiveViewIndex = targetView;

            btnTabTracker.CssClass = "tab-btn" + (targetView == 0 ? " active-tab" : "");
            btnTabReports.CssClass = "tab-btn" + (targetView == 1 ? " active-tab" : "");

            if (targetView == 1)
            {
                ClearReportPreview();
                LoadReportHistoryLogs();
            }
            else
            {
                ExecuteSearch();
            }
        }

        #region TAB 1: STUDENT TRACKER METRICS VIEW

        private DataTable GetProgressDataMetrics(int courseId)
        {
            DataTable dt = new DataTable();
            int lecturerId = CurrentLecturerId;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // ADDED TotalMarksObtained TO THE SELECT COLUMNS AND CALCULATIONS WITHIN CTE
                string sql = @"
                WITH StudentBaseMetrics AS (
                    SELECT 
                        s.StudentId,
                        s.StudentNo,
                        u.FullName,
                        u.Email,
                        c.CourseCode,
                        c.CourseId,
                        e.AcademicYear,
                        e.Semester,
                        e.EnrolmentId,
                        
                        ISNULL((
                            SELECT 100.0 * SUM(CASE WHEN att.Status = 'Present' THEN 1 ELSE 0 END) / NULLIF(COUNT(*), 0)
                            FROM Attendance att WHERE att.EnrolmentId = e.EnrolmentId
                        ), 100.0) AS AttendancePercent,

                        ISNULL((
                            SELECT 100.0 * SUM(sm.MarksObtained) / NULLIF(SUM(a.MaxMark), 0)
                            FROM StudentMarks sm
                            INNER JOIN Assessments a ON sm.AssessmentId = a.AssessmentId
                            WHERE sm.StudentId = s.StudentId AND a.CourseId = e.CourseId AND a.AcademicYear = e.AcademicYear AND a.Semester = e.Semester
                        ), 0.0) AS AssessmentMarkPercent,

                        ISNULL((
                            SELECT COUNT(DISTINCT sm.AssessmentId)
                            FROM StudentMarks sm
                            INNER JOIN Assessments a ON sm.AssessmentId = a.AssessmentId
                            WHERE sm.StudentId = s.StudentId 
                                AND a.CourseId = e.CourseId 
                                AND a.AcademicYear = e.AcademicYear 
                                AND a.Semester = e.Semester
                        ), 0) AS CompletedSubmissions,

                        ISNULL((
                            SELECT (100.0 * SUM(sm.MarksObtained) / NULLIF(SUM(a.MaxMark), 0))
                            FROM StudentMarks sm
                            INNER JOIN Assessments a ON sm.AssessmentId = a.AssessmentId
                            WHERE sm.StudentId = s.StudentId 
                                AND a.CourseId = e.CourseId 
                                AND a.AcademicYear = e.AcademicYear 
                                AND a.Semester = e.Semester
                        ), 0.0) AS TotalMarksObtained

                    FROM Enrolments e
                    INNER JOIN Students s ON e.StudentId = s.StudentId
                    INNER JOIN Users u ON s.UserId = u.UserId
                    INNER JOIN Courses c ON e.CourseId = c.CourseId
                    INNER JOIN CourseAssignments ca ON c.CourseId = ca.CourseId AND e.AcademicYear = ca.AcademicYear AND e.Semester = ca.Semester
                    WHERE ca.LecturerId = @LecturerId 
                      AND (@CourseId = 0 OR e.CourseId = @CourseId)
                      AND e.Status = 'Active'
                ),
                MetricsWithGPA AS (
                    SELECT 
                        m.*,
                        ISNULL((
                            SELECT TOP 1 gs.GradePoint FROM GradeScale gs 
                            WHERE m.AssessmentMarkPercent >= gs.MinMark AND m.AssessmentMarkPercent <= gs.MaxMark
                            ORDER BY gs.MinMark DESC
                        ), 0.00) AS CurrentGPA
                    FROM StudentBaseMetrics m
                )
                SELECT 
                    StudentId, StudentNo, FullName, Email, CourseCode, CourseId, AcademicYear, Semester, CurrentGPA, TotalMarksObtained, AttendancePercent, CompletedSubmissions,
                    CASE 
                        WHEN AttendancePercent < 80.0 OR (CompletedSubmissions > 0 AND CurrentGPA < 2.00) THEN 'High'
                        WHEN AttendancePercent < 90.0 OR (CompletedSubmissions > 0 AND CurrentGPA < 2.75) THEN 'Medium'
                        ELSE 'Low'
                    END AS RiskLevel,
                    CASE 
                        WHEN AttendancePercent < 80.0 AND CompletedSubmissions > 0 AND CurrentGPA < 2.00 THEN 'High Risk: Attendance is below 80% and Course GPA is below 2.00'
                        WHEN AttendancePercent < 80.0 THEN 'High Risk: Poor Attendance record (< 80%)'
                        WHEN CompletedSubmissions > 0 AND CurrentGPA < 2.00 THEN 'High Risk: Low Academic Grade Failings (GPA < 2.00)'
                        WHEN AttendancePercent < 90.0 AND CompletedSubmissions > 0 AND CurrentGPA < 2.75 THEN 'Medium Risk: Borderline Attendance (< 90%) and GPA (< 2.75)'
                        WHEN AttendancePercent < 90.0 THEN 'Medium Risk: Suboptimal Class Attendance (< 90%)'
                        WHEN CompletedSubmissions > 0 AND CurrentGPA < 2.75 THEN 'Medium Risk: Modest Academic Assessment Average (GPA < 2.75)'
                        ELSE 'Low Risk: Satisfactory performance metrics maintained.'
                    END AS RiskReason
                FROM MetricsWithGPA
                ORDER BY FullName ASC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@LecturerId", lecturerId);
                    cmd.Parameters.AddWithValue("@CourseId", courseId);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            return dt;
        }

        public void ExecuteSearch()
        {
            try
            {
                int courseId = int.Parse(ddlCourse.SelectedValue ?? "0");
                string riskFilter = ddlRiskLevel.SelectedValue;

                DataTable dt = GetProgressDataMetrics(courseId);
                DataView dv = dt.DefaultView;

                if (!string.IsNullOrEmpty(riskFilter))
                {
                    dv.RowFilter = $"RiskLevel = '{riskFilter}'";
                }

                rptStudentProgress.DataSource = dv;
                rptStudentProgress.DataBind();
                pnlNoData.Visible = (dv.Count == 0);
            }
            catch (Exception ex)
            {
                ShowSystemFeedback($"Error evaluating criteria profiles: {ex.Message}", true);
            }
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            ExecuteSearch();
        }

        #endregion

        #region TAB 2: REPORT ENGINE WITH SECURE STREAMING

        protected void btnGenerateReport_Click(object sender, EventArgs e)
        {
            try
            {
                int courseId = int.Parse(ddlReportCourse.SelectedValue ?? "0");
                string riskFilter = ddlReportRisk.SelectedValue;

                DataTable dt = GetProgressDataMetrics(courseId);
                DataView dv = dt.DefaultView;

                if (!string.IsNullOrEmpty(riskFilter) && riskFilter != "All")
                {
                    dv.RowFilter = $"RiskLevel = '{riskFilter}'";
                }

                DataTable reportData = dv.ToTable();

                DataTable clientTable = new DataTable();
                clientTable.Columns.Add("Student No");
                clientTable.Columns.Add("Full Name");
                clientTable.Columns.Add("Email");
                clientTable.Columns.Add("Course Code");
                clientTable.Columns.Add("Attendance Rate");
                clientTable.Columns.Add("Projected GPA");
                clientTable.Columns.Add("Total Marks"); // ADDED DATAFIELD COLUMN
                clientTable.Columns.Add("Risk Level");

                foreach (DataRow row in reportData.Rows)
                {
                    clientTable.Rows.Add(
                        row["StudentNo"],
                        row["FullName"],
                        row["Email"],
                        row["CourseCode"],
                        Convert.ToDouble(row["AttendancePercent"]).ToString("F1") + "%",
                        Convert.ToDouble(row["CurrentGPA"]).ToString("F2"),
                        Convert.ToDouble(row["TotalMarksObtained"]).ToString("F2"), // INJECTED CORRESPONDING DATAROW SCALAR VALUE
                        row["RiskLevel"]
                    );
                }

                litReportTitle.Text = "Student Performance Exceptions Preview Analysis";
                gvReportPreview.DataSource = clientTable;
                gvReportPreview.DataBind();

                ViewState["LecturerReportBuffer"] = reportData;
                ViewState["LecturerReportTitle"] = "Student Progress Summary";
                ViewState["LecturerReportScope"] = $"Course ID: {courseId} | Filter: {riskFilter}";

                pnlReportWorkspace.Visible = true;
                lblReportFeedback.Visible = false;

                if (clientTable.Rows.Count == 0)
                {
                    lblReportFeedback.Text = "No records matched your specified filter configuration.";
                    lblReportFeedback.Style["background-color"] = "#fffbeb";
                    lblReportFeedback.Style["color"] = "#b45309";
                    lblReportFeedback.Style["border"] = "1px solid #fef3c7";
                    lblReportFeedback.Visible = true;
                }
            }
            catch (Exception ex)
            {
                ShowSystemFeedback($"Generation Pipeline Interrupted: {ex.Message}", true);
            }
        }

        protected void btnCompileCSVReport_Click(object sender, EventArgs e)
        {
            DataTable reportData = ViewState["LecturerReportBuffer"] as DataTable;
            string reportClassification = Convert.ToString(ViewState["LecturerReportTitle"]);
            string appliedRestrictions = Convert.ToString(ViewState["LecturerReportScope"]);

            if (reportData == null || reportData.Rows.Count == 0)
            {
                lblReportFeedback.Text = "Please click 'Generate Workload Preview' before executing file compile downstreams.";
                lblReportFeedback.Style["background-color"] = "#fee2e2";
                lblReportFeedback.Style["color"] = "#991b1b";
                lblReportFeedback.Style["border"] = "1px solid #fca5a5";
                lblReportFeedback.Visible = true;
                return;
            }

            try
            {
                string targetFolder = Server.MapPath("~/ReportExports/");
                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileBaseName = $"StudentProgressReport_{timestamp}.csv";
                string physicalWritePath = Path.Combine(targetFolder, fileBaseName);
                string applicationVirtualPath = "~/ReportExports/" + fileBaseName;

                StringBuilder sb = new StringBuilder();
                // INJECTED Total Marks INTO CSV STREAM HEADER
                sb.AppendLine("Student No,Full Name,Email,Course Code,Attendance %,Projected GPA,Total Marks,Risk Level,Details / Reasons");

                foreach (DataRow row in reportData.Rows)
                {
                    string escapedName = row["FullName"].ToString().Replace("\"", "\"\"");
                    string escapedReason = row["RiskReason"].ToString().Replace("\"", "\"\"");

                    // MATCHED EXPLICIT FORMAT PLACEMENT WRITER STRINGS
                    sb.AppendLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4:F1}\",\"{5:F2}\",\"{6:F2}\",\"{7}\",\"{8}\"",
                        row["StudentNo"], escapedName, row["Email"], row["CourseCode"],
                        row["AttendancePercent"], row["CurrentGPA"], row["TotalMarksObtained"], row["RiskLevel"], escapedReason
                    ));
                }

                File.WriteAllText(physicalWritePath, sb.ToString(), Encoding.UTF8);

                int currentUserId = CurrentUserId;
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    int reportId = 0;
                    string insertReportSql = @"
                        INSERT INTO [dbo].[Reports] (ReportType, AcademicYear, Semester, FilterCriteria, GeneratedBy, GeneratedAt)
                        VALUES (@ReportType, @Year, @Sem, @Criteria, @User, SYSUTCDATETIME());
                        SELECT CAST(SCOPE_IDENTITY() as int);";

                    int month = DateTime.Now.Month;
                    int calculatedSemester = (month <= 4) ? 1 : (month <= 8) ? 2 : 3;

                    using (SqlCommand cmd = new SqlCommand(insertReportSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ReportType", reportClassification);
                        cmd.Parameters.AddWithValue("@Year", DateTime.Now.Year);
                        cmd.Parameters.AddWithValue("@Sem", calculatedSemester);
                        cmd.Parameters.AddWithValue("@Criteria", appliedRestrictions);
                        cmd.Parameters.AddWithValue("@User", currentUserId);
                        reportId = (int)cmd.ExecuteScalar();
                    }

                    if (reportId > 0)
                    {
                        string insertExportSql = @"
                            INSERT INTO [dbo].[ReportExports] (ReportId, ExportFormat, FilePath, ExportedAt)
                            VALUES (@ReportId, 'CSV', @Path, SYSUTCDATETIME());";

                        using (SqlCommand cmdExport = new SqlCommand(insertExportSql, conn))
                        {
                            cmdExport.Parameters.AddWithValue("@ReportId", reportId);
                            cmdExport.Parameters.AddWithValue("@Path", applicationVirtualPath);
                            cmdExport.ExecuteNonQuery();
                        }
                    }
                }

                LoadReportHistoryLogs();
                TransmitFileStreamSecurely(physicalWritePath, fileBaseName);
            }
            catch (Exception ex)
            {
                lblReportFeedback.Text = $"Compilation Exception Disrupted: {ex.Message}";
                lblReportFeedback.Style["background-color"] = "#fee2e2";
                lblReportFeedback.Style["color"] = "#991b1b";
                lblReportFeedback.Style["border"] = "1px solid #fca5a5";
                lblReportFeedback.Visible = true;
            }
        }

        private void TransmitFileStreamSecurely(string physicalPath, string userVisibleName)
        {
            try
            {
                // 1. Read the raw text elements from the historical CSV logs repository safely
                string[] fileLines = File.ReadAllLines(physicalPath);
                if (fileLines.Length == 0)
                {
                    ShowSystemFeedback("The selected tracking logs profile data file appears to be empty.", true);
                    return;
                }

                // Parse file variables to derive dynamic metadata contexts
                string reportTitle = "STUDENT PERFORMANCE PROGRESS MONITORING REPORT";
                string cleanTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm tt");

                // Transform user visible file name to target modern excel extension metrics (.xls)
                string fixedDownloadFileName = Path.GetFileNameWithoutExtension(userVisibleName) + "_Formatted.xls";

                // 2. Initialize Professional Spreadsheet Stream Settings
                Response.Clear();
                Response.Buffer = true;
                Response.ContentType = "application/vnd.ms-excel";
                Response.AddHeader("content-disposition", "attachment;filename=" + fixedDownloadFileName);
                Response.Charset = "utf-8";
                Response.ContentEncoding = Encoding.UTF8;

                StringBuilder sb = new StringBuilder();

                // 3. Document Structure XML Definitions
                sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                sb.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
                sb.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");
                sb.AppendLine(" xmlns:o=\"urn:schemas-microsoft-com:office:office\"");
                sb.AppendLine(" xmlns:x=\"urn:schemas-microsoft-com:office:excel\"");
                sb.AppendLine(" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"");
                sb.AppendLine(" xmlns:html=\"http://www.w3.org/TR/REC-html40\">");

                // 4. Premium Theme Visual Styles (Matching your dashboard's signature deep red accent palette)
                sb.AppendLine(" <Styles>");
                sb.AppendLine("  <Style ss:ID=\"Default\" ss:Name=\"Normal\">");
                sb.AppendLine("   <Font ss:FontName=\"Segoe UI\" x:CharSet=\"1\" ss:Size=\"11\" ss:Color=\"#1E293B\"/>");
                sb.AppendLine("  </Style>");
                sb.AppendLine("  <Style ss:ID=\"ReportHeader\">");
                sb.AppendLine("   <Font ss:FontName=\"Segoe UI\" ss:Size=\"14\" ss:Bold=\"1\" ss:Color=\"#DC2626\"/>"); // Professional deep red brand match
                sb.AppendLine("  </Style>");
                sb.AppendLine("  <Style ss:ID=\"MetadataLabel\">");
                sb.AppendLine("   <Font ss:FontName=\"Segoe UI\" ss:Size=\"10\" ss:Bold=\"1\" ss:Color=\"#64748B\"/>");
                sb.AppendLine("  </Style>");
                sb.AppendLine("  <Style ss:ID=\"MetadataValue\">");
                sb.AppendLine("   <Font ss:FontName=\"Segoe UI\" ss:Size=\"10\" ss:Color=\"#1E293B\"/>");
                sb.AppendLine("  </Style>");
                sb.AppendLine("  <Style ss:ID=\"TableHeader\">");
                sb.AppendLine("   <Interior ss:Color=\"#FFF5F5\" ss:Pattern=\"Solid\"/>"); // Light tinted background
                sb.AppendLine("   <Borders>");
                sb.AppendLine("    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"2\" ss:Color=\"#FCA5A5\"/>");
                sb.AppendLine("   </Borders>");
                sb.AppendLine("   <Font ss:FontName=\"Segoe UI\" ss:Size=\"11\" ss:Bold=\"1\" ss:Color=\"#991B1B\"/>");
                sb.AppendLine("  </Style>");
                sb.AppendLine("  <Style ss:ID=\"DataCell\">");
                sb.AppendLine("   <Borders>");
                sb.AppendLine("    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#F3F4F6\"/>");
                sb.AppendLine("   </Borders>");
                sb.AppendLine("  </Style>");
                sb.AppendLine(" </Styles>");

                // 5. Open Worksheet Segment Workspace
                sb.AppendLine(" <Worksheet ss:Name=\"Academic Progress Monitoring\">");
                sb.AppendLine("  <Table>");

                // CRITICAL ENHANCEMENT: Directs Excel to parse content tracking text lines and layout widths instantly
                sb.AppendLine("   <Column ss:AutoFitWidth=\"1\" ss:Min=\"1\" ss:Max=\"15\"/>");

                // 6. Corporate Metadata Information Block
                sb.AppendLine("   <Row ss:Height=\"25\">");
                sb.AppendLine("    <Cell ss:StyleID=\"ReportHeader\"><Data ss:Type=\"String\">" + reportTitle + "</Data></Cell>");
                sb.AppendLine("   </Row>");

                sb.AppendLine("   <Row>");
                sb.AppendLine("    <Cell ss:StyleID=\"MetadataLabel\"><Data ss:Type=\"String\">File Identity:</Data></Cell>");
                sb.AppendLine("    <Cell ss:StyleID=\"MetadataValue\"><Data ss:Type=\"String\">" + ProgressSecurityEscape(userVisibleName) + "</Data></Cell>");
                sb.AppendLine("   </Row>");

                sb.AppendLine("   <Row>");
                sb.AppendLine("    <Cell ss:StyleID=\"MetadataLabel\"><Data ss:Type=\"String\">Processed Date:</Data></Cell>");
                sb.AppendLine("    <Cell ss:StyleID=\"MetadataValue\"><Data ss:Type=\"String\">" + cleanTimestamp + "</Data></Cell>");
                sb.AppendLine("   </Row>");

                sb.AppendLine("   <Row>");
                sb.AppendLine("    <Cell ss:StyleID=\"MetadataLabel\"><Data ss:Type=\"String\">Classification:</Data></Cell>");
                sb.AppendLine("    <Cell ss:StyleID=\"MetadataValue\"><Data ss:Type=\"String\">Restricted Academic Evaluation Record</Data></Cell>");
                sb.AppendLine("   </Row>");

                sb.AppendLine("   <Row ss:Height=\"15\"></Row>"); // Visual whitespace separator row

                // 7. Parse Data Grid Elements Loop
                bool isFirstDataRow = true;
                foreach (string fileLine in fileLines)
                {
                    if (string.IsNullOrWhiteSpace(fileLine)) continue;

                    // Split file items by default comma matrices
                    string[] cellContents = fileLine.Split(',');

                    // Determine if the line is the grid's tracking layout header or regular student cells
                    string dynamicRowStyleId = isFirstDataRow ? "ss:StyleID=\"TableHeader\" ss:Height=\"22\"" : "ss:StyleID=\"DataCell\" ss:Height=\"20\"";
                    sb.AppendLine("   <Row " + dynamicRowStyleId + ">");

                    foreach (string directValue in cellContents)
                    {
                        // Unquote values safely to clean string metrics
                        string balancedTextToken = directValue.Trim(' ', '"');
                        string structuredCleanString = ProgressSecurityEscape(balancedTextToken);

                        sb.AppendLine("    <Cell><Data ss:Type=\"String\">" + structuredCleanString + "</Data></Cell>");
                    }

                    sb.AppendLine("   </Row>");
                    isFirstDataRow = false; // Transition parsing focus to regular row layouts
                }

                // 8. Close Spreadsheet Document Tree Structures
                sb.AppendLine("  </Table>");
                sb.AppendLine("  <WorksheetOptions xmlns=\"urn:schemas-microsoft-com:office:excel\">");
                sb.AppendLine("   <Selected/>");
                sb.AppendLine("   <ProtectObjects>False</ProtectObjects>");
                sb.AppendLine("   <ProtectScenarios>False</ProtectScenarios>");
                sb.AppendLine("  </WorksheetOptions>");
                sb.AppendLine(" </Worksheet>");
                sb.AppendLine("</Workbook>");

                Response.Write(sb.ToString());
                Response.Flush();
                Response.End();
            }
            catch (System.Threading.ThreadAbortException)
            {
                // Caught cleanly to bypass standard system stack dumps on Response.End() termination
            }
            catch (Exception ex)
            {
                ShowSystemFeedback("Transmission Pipeline Exception Interrupted: " + ex.Message, true);
            }
        }

        // Escapes special symbols to maintain structural validation of the spreadsheet layout engine
        private string ProgressSecurityEscape(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return input.Replace("&", "&amp;")
                        .Replace("<", "&lt;")
                        .Replace(">", "&gt;")
                        .Replace("\"", "&quot;")
                        .Replace("'", "&apos;");
        }

        private void LoadReportHistoryLogs()
        {
            try
            {
                int currentUserId = CurrentUserId;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string sql = @"
                        SELECT 
                            e.[ExportId],
                            r.[ReportType] AS [Classification],
                            r.[FilterCriteria] AS [ScopeFilters],
                            e.[ExportedAt] AS [ExportedAt],
                            e.[FilePath] AS [FilePath],
                            'Completed' AS [Status]
                        FROM [dbo].[ReportExports] e
                        INNER JOIN [dbo].[Reports] r ON e.[ReportId] = r.[ReportId]
                        WHERE r.[GeneratedBy] = @GeneratedBy
                        ORDER BY e.[ExportedAt] DESC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@GeneratedBy", currentUserId);
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);
                            gvReportHistory.DataSource = dt;
                            gvReportHistory.DataBind();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Audit Log Fetch Failed: " + ex.Message);
            }
        }

        protected void gvReportHistory_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DownloadReportFile")
            {
                string virtualPath = Convert.ToString(e.CommandArgument);

                if (string.IsNullOrEmpty(virtualPath))
                {
                    ShowSystemFeedback("The targeted file path information is missing inside the audit database records.", true);
                    return;
                }

                try
                {
                    string physicalPath = Server.MapPath(virtualPath);

                    if (File.Exists(physicalPath))
                    {
                        string userVisibleName = Path.GetFileName(physicalPath);
                        TransmitFileStreamSecurely(physicalPath, userVisibleName);
                    }
                    else
                    {
                        ShowSystemFeedback("The requested CSV asset can no longer be located on the hosting filesystem storage.", true);
                    }
                }
                catch (Exception ex)
                {
                    ShowSystemFeedback($"File Retrieval Engine Interrupted: {ex.Message}", true);
                }
            }
        }

        private void ClearReportPreview()
        {
            pnlReportWorkspace.Visible = false;
            gvReportPreview.DataSource = null;
            gvReportPreview.DataBind();
            lblReportFeedback.Visible = false;
        }

        #endregion

        private void ShowSystemFeedback(string txt, bool isError)
        {
            lblStatusMessage.Visible = true;
            lblStatusMessage.Text = txt;
            lblStatusMessage.Style["background-color"] = isError ? "#fee2e2" : "#dcfce7";
            lblStatusMessage.Style["color"] = isError ? "#991b1b" : "#166534";
            lblStatusMessage.Style["border"] = isError ? "1px solid #fca5a5" : "1px solid #86efac";
        }
    }
}