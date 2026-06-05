using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SIMS.Lecturer
{
    public partial class LecturerStudentProgress : LecturerBase
    {
        private string connStr = ConfigurationManager.ConnectionStrings["SIMS_DB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            EnsureAuthenticated();

            if (!IsPostBack)
            {
                LoadCourses();
                ExecuteSearch();
            }
        }

        void LoadCourses()
        {
            try
            {
                int lecturerId = CurrentLecturerId;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    // REMOVED rigid current-day/month constraints so past, current, and future assigned courses show up dynamically
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

                            // Add a descriptive display field combining Code, Name and Academic Period
                            dt.Columns.Add("DisplayTitle", typeof(string));
                            foreach (DataRow row in dt.Rows)
                            {
                                row["DisplayTitle"] = $"{row["CourseCode"]} - {row["CourseName"]} (Yr {row["AcademicYear"]} / Sem {row["Semester"]})";
                            }

                            ddlCourse.DataSource = dt;
                            ddlCourse.DataTextField = "DisplayTitle";
                            ddlCourse.DataValueField = "CourseId";
                            ddlCourse.DataBind();
                        }
                    }
                }

                ddlCourse.Items.Insert(0, new ListItem("-- All Assigned Courses --", "0"));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading courses: {ex.Message}");
                ShowSystemFeedback($"Error loading associated courses: {ex.Message}", true);
            }
        }

        private DataTable GetProgressDataMetrics(int courseId)
        {
            int lecturerId = CurrentLecturerId;
            int currentYear = DateTime.Now.Year;
            int currentSemester = GetCurrentSemester();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Expanded query calculating Risk dynamically across explicit structural points: Attendance and Cumulative CGPA
                string sql = @"
    SELECT
        s.StudentId,
        s.StudentNo,
        u.FullName,
        u.Email,
        c.CourseCode, -- Added to identify the student's current course
        ISNULL(att.AttendancePercent, 0.0) AS AttendancePercent,
        ISNULL(gpa.CGPA, 0.00) AS CurrentGPA,
        CASE 
            WHEN ISNULL(att.AttendancePercent, 100) < 55 OR ISNULL(gpa.CGPA, 4.0) < 2.00 THEN 'High'
            WHEN ISNULL(att.AttendancePercent, 100) < 65 OR ISNULL(gpa.CGPA, 4.0) < 2.50 THEN 'Medium'
            ELSE 'Low'
        END AS RiskLevel,
        COUNT(DISTINCT a.AssessmentId) AS AssignmentStatus
    FROM Enrolments e
    INNER JOIN Students s ON s.StudentId = e.StudentId
    INNER JOIN Users u ON u.UserId = s.UserId
    INNER JOIN Courses c ON c.CourseId = e.CourseId
    LEFT JOIN (
        SELECT EnrolmentId,
               100.0 * SUM(CASE WHEN Status = 'Present' THEN 1 ELSE 0 END) / NULLIF(COUNT(*), 0) AS AttendancePercent
        FROM Attendance
        GROUP BY EnrolmentId
    ) att ON att.EnrolmentId = e.EnrolmentId
    LEFT JOIN (
        SELECT StudentId, GPA, CGPA,
               ROW_NUMBER() OVER (PARTITION BY StudentId ORDER BY CalculatedAt DESC) as rn
        FROM GPARecords
    ) gpa ON gpa.StudentId = s.StudentId AND gpa.rn = 1
    LEFT JOIN Assessments a ON a.CourseId = e.CourseId
    INNER JOIN CourseAssignments ca ON ca.CourseId = e.CourseId 
                                   AND ca.AcademicYear = e.AcademicYear 
                                   AND ca.Semester = e.Semester
    WHERE ca.LecturerId = @LecturerId
      AND e.Status = 'Active'";

                if (courseId > 0)
                {
                    sql += " AND e.CourseId = @CourseId";
                }

                // Added c.CourseCode to the GROUP BY clause
                sql += " GROUP BY s.StudentId, s.StudentNo, u.FullName, u.Email, c.CourseCode, att.AttendancePercent, gpa.CGPA ORDER BY s.StudentNo";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@LecturerId", lecturerId);
                    cmd.Parameters.AddWithValue("@Year", currentYear);
                    cmd.Parameters.AddWithValue("@Semester", currentSemester);
                    if (courseId > 0) cmd.Parameters.AddWithValue("@CourseId", courseId);

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        private void ExecuteSearch()
        {
            try
            {
                int courseId = int.Parse(ddlCourse.SelectedValue ?? "0");
                string riskFilter = ddlRiskLevel.SelectedValue;

                DataTable dt = GetProgressDataMetrics(courseId);

                if (!string.IsNullOrEmpty(riskFilter) && dt.Rows.Count > 0)
                {
                    DataView dv = dt.DefaultView;
                    dv.RowFilter = $"RiskLevel = '{riskFilter}'";
                    rptStudentProgress.DataSource = dv;
                    pnlNoData.Visible = (dv.Count == 0);
                }
                else
                {
                    rptStudentProgress.DataSource = dt;
                    pnlNoData.Visible = (dt.Rows.Count == 0);
                }

                rptStudentProgress.DataBind();
            }
            catch (Exception ex)
            {
                ShowSystemFeedback($"Error collecting metrics pipeline: {ex.Message}", true);
            }
        }

        protected void btnApplyFilter_Click(object sender, EventArgs e)
        {
            ExecuteSearch();
        }

        protected void btnExportReport_Click(object sender, EventArgs e)
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

                DataTable filteredRecords = dv.ToTable();

                // Structural Feature: Dynamic database persistence into the Reports table schema
                int loggedReportId = LogReportGenerationToDatabase(courseId, riskFilter);

                // Compile CSV Memory Document
                StringBuilder sb = new StringBuilder();
                // 1. Update the CSV column headers text string line to include Course Code:
                sb.AppendLine("Report Audit ID,Student Number,Full Name,Course Code,Email Address,Attendance %,CGPA,Risk Designation,Assessments Enrolled");

                // 2. Update loop iteration values statement block mapping:
                foreach (DataRow row in filteredRecords.Rows)
                {
                    double attVal = Convert.ToDouble(row["AttendancePercent"]);
                    double gpaVal = Convert.ToDouble(row["CurrentGPA"]);

                    sb.AppendLine($"\"{loggedReportId}\",\"{row["StudentNo"]}\",\"{row["FullName"]}\",\"{row["CourseCode"]}\",\"{row["Email"]}\",\"{attVal:F1}%\",\"{gpaVal:F2}\",\"{row["RiskLevel"]}\",\"{row["AssignmentStatus"]}\"");
                }

                // Clear downstream response pipelines to emit dynamic file download binary contents directly
                HttpResponse response = HttpContext.Current.Response;
                response.Clear();
                response.ContentType = "text/csv";
                response.AddHeader("content-disposition", $"attachment;filename=Student_Progress_Report_Sem{GetCurrentSemester()}_{DateTime.Now:yyyyMMdd}.csv");
                response.Buffer = true;
                response.Write(sb.ToString());
                response.Flush();
                response.End();
            }
            catch (System.Threading.ThreadAbortException)
            {
                // Catch standard safely-thrown inner redirect system architecture exceptions on End() operations
            }
            catch (Exception ex)
            {
                ShowSystemFeedback($"Failed to compile structural download: {ex.Message}", true);
            }
        }

        private int LogReportGenerationToDatabase(int courseId, string riskCriteria)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Inserts a tracker log inside your explicit 'Reports' table architecture
                string sql = @"
                    INSERT INTO Reports (GeneratedBy, ReportType, AcademicYear, Semester, FilterCriteria, GeneratedAt)
                    OUTPUT INSERTED.ReportId
                    VALUES (@User, 'Lecturer Student Progress Tracker Flag Output', @Year, @Sem, @Criteria, SYSUTCDATETIME());";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    // Assuming basic authentication framework context exposes global identity references directly
                    cmd.Parameters.AddWithValue("@User", CurrentLecturerId);
                    cmd.Parameters.AddWithValue("@Year", DateTime.Now.Year);
                    cmd.Parameters.AddWithValue("@Sem", GetCurrentSemester());
                    cmd.Parameters.AddWithValue("@Criteria", $"Course ID Target: {courseId} | Risk Selection Filter Constraints: {riskCriteria}");

                    conn.Open();
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        protected void rptStudentProgress_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "IssueWarning")
            {
                int targetStudentId = Convert.ToInt32(e.CommandArgument);
                int currentCourseId = int.Parse(ddlCourse.SelectedValue == "0" ? "1" : ddlCourse.SelectedValue); // Safely map to an active structural class identifier

                try
                {
                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        // Operational feature tracking straight into the 'AcademicWarnings' table framework
                        string sql = @"
                            INSERT INTO AcademicWarnings (StudentId, CourseId, WarningType, Reason, Severity, Status, IssuedBy, IssuedAt)
                            VALUES (@StudentId, @CourseId, 'Performance Risk Warning', 'Automated flag issued due to automated threshold system analytics drop on Attendance/Marks matrices.', 'Medium', 'Active', @IssuedBy, SYSUTCDATETIME());";

                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@StudentId", targetStudentId);
                            cmd.Parameters.AddWithValue("@CourseId", currentCourseId);
                            cmd.Parameters.AddWithValue("@IssuedBy", CurrentLecturerId);

                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }

                    ShowSystemFeedback("Academic warning flag successfully generated and preserved in tracking tables.", false);
                    ExecuteSearch(); // Clear down and reload view representation models
                }
                catch (Exception ex)
                {
                    ShowSystemFeedback($"Failed tracking historical action context warning logs: {ex.Message}", true);
                }
            }
        }

        private void ShowSystemFeedback(string txt, bool isError)
        {
            lblStatusMessage.Visible = true;
            lblStatusMessage.Text = txt;
            lblStatusMessage.Style["background-color"] = isError ? "#fee2e2" : "#dcfce7";
            lblStatusMessage.Style["color"] = isError ? "#991b1b" : "#166534";
            lblStatusMessage.Style["border"] = isError ? "1px solid #fca5a5" : "1px solid #86efac";
        }

        private int GetCurrentSemester()
        {
            int month = DateTime.Now.Month;
            if (month <= 4) return 1;
            if (month <= 8) return 2;
            return 3;
        }
    }
}