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
                        ), 0) AS CompletedSubmissions

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
                    StudentId, StudentNo, FullName, Email, CourseCode, CourseId, AcademicYear, Semester, CurrentGPA, AttendancePercent, CompletedSubmissions,
                    CASE 
                        -- Only penalize GPA if they have actually completed submissions/assessments
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
                sb.AppendLine("Student No,Full Name,Email,Course Code,Attendance %,Projected GPA,Risk Level,Details / Reasons");

                foreach (DataRow row in reportData.Rows)
                {
                    string escapedName = row["FullName"].ToString().Replace("\"", "\"\"");
                    string escapedReason = row["RiskReason"].ToString().Replace("\"", "\"\"");

                    sb.AppendLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4:F1}\",\"{5:F2}\",\"{6}\",\"{7}\"",
                        row["StudentNo"], escapedName, row["Email"], row["CourseCode"],
                        row["AttendancePercent"], row["CurrentGPA"], row["RiskLevel"], escapedReason
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

        private void TransmitFileStreamSecurely(string exactFilePath, string userVisibleName)
        {
            HttpResponse response = HttpContext.Current.Response;
            response.Clear();
            response.ClearHeaders();
            response.ClearContent();

            response.Buffer = true;
            response.ContentType = "text/csv";
            response.AddHeader("Content-Length", new FileInfo(exactFilePath).Length.ToString());
            response.AddHeader("Content-Disposition", $"attachment; filename=\"{userVisibleName}\"");
            response.Charset = "utf-8";

            response.TransmitFile(exactFilePath);
            response.Flush();

            HttpContext.Current.ApplicationInstance.CompleteRequest();
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