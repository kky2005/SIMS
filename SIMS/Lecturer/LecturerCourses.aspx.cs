using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SIMS.Lecturer
{
    public partial class LecturerCourses : LecturerBase
    {
        string connStr = ConfigurationManager.ConnectionStrings["SIMS_DB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            EnsureAuthenticated();

            // CRITICAL FIX: Dynamic controls MUST be re-created on every postback
            LoadAvailableSemesters();

            if (!IsPostBack)
            {
                LoadCourses(0);
                btnFilterAll.CssClass = "filter-badge active";
            }
        }

        private void LoadAvailableSemesters()
        {
            try
            {
                int lecturerId = CurrentLecturerId;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = @"
                        SELECT DISTINCT c.Semester
                        FROM CourseAssignments ca
                        INNER JOIN Courses c ON c.CourseId = ca.CourseId
                        WHERE ca.LecturerId = @LecturerId
                        ORDER BY c.Semester ASC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@LecturerId", lecturerId);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        // Clear placeholder before re-adding controls to prevent duplicate sets
                        phSemesterFilters.Controls.Clear();

                        foreach (DataRow row in dt.Rows)
                        {
                            int semester = Convert.ToInt32(row["Semester"]);

                            LinkButton btnSemester = new LinkButton
                            {
                                ID = $"btnFilterSem{semester}",
                                Text = $"Semester {semester}",
                                CssClass = "filter-badge",
                                CommandArgument = semester.ToString()
                            };
                            btnSemester.Click += FilterCourses_Click;

                            phSemesterFilters.Controls.Add(btnSemester);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading semesters: {ex.Message}");
            }
        }

        void LoadCourses(int semester)
        {
            try
            {
                int lecturerId = CurrentLecturerId;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = @"
                        SELECT
                            c.CourseId,
                            c.CourseCode,
                            c.CourseName,
                            c.CreditHours,
                            c.Semester,
                            ca.AcademicYear,
                            COUNT(DISTINCT e.EnrolmentId) AS TotalStudents,
ca.Semester AS AssignmentSemester
                        FROM CourseAssignments ca
                        INNER JOIN Courses c ON c.CourseId = ca.CourseId
                        LEFT JOIN Enrolments e
                            ON e.CourseId = c.CourseId
                            AND e.AcademicYear = ca.AcademicYear
                            AND e.Semester = ca.Semester
                            AND e.Status = 'Active'
                        WHERE ca.LecturerId = @LecturerId";

                    if (semester > 0)
                    {
                        sql += " AND c.Semester = @Semester";
                    }

                    sql += @" GROUP BY
                                c.CourseId,
                                c.CourseCode,
                                c.CourseName,
                                c.CreditHours,
                                c.Semester,
                                ca.Semester,
                                ca.AcademicYear
                             ORDER BY ca.AcademicYear DESC, c.Semester ASC, c.CourseCode ASC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@LecturerId", lecturerId);
                        if (semester > 0) cmd.Parameters.AddWithValue("@Semester", semester);

                        conn.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        conn.Close();

                        if (dt.Rows.Count > 0)
                        {
                            rptCourses.DataSource = dt;
                            rptCourses.DataBind();
                            pnlNoCourses.Visible = false;
                        }
                        else
                        {
                            // CRITICAL FIX: Explicitly clear the layout control if no items match
                            rptCourses.DataSource = null;
                            rptCourses.DataBind();
                            pnlNoCourses.Visible = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading courses: {ex.Message}");
                rptCourses.DataSource = null;
                rptCourses.DataBind();
                pnlNoCourses.Visible = true;
            }
        }

        private void LoadStudents(
    int courseId,
    int academicYear,
    int semester)
        {
            using (SqlConnection conn =
                   new SqlConnection(connStr))
            {
                string sql = @"

SELECT
    s.StudentNo,
    u.FullName,
    u.Email,
    p.ProgrammeName

FROM Enrolments e

INNER JOIN Students s
ON e.StudentId = s.StudentId

INNER JOIN Users u
ON s.UserId = u.UserId

INNER JOIN Programmes p
ON s.ProgrammeId = p.ProgrammeId

WHERE
e.CourseId = @CourseId
AND e.AcademicYear = @AcademicYear
AND e.Semester = @Semester
AND e.Status = 'Active'

ORDER BY u.FullName";

                SqlCommand cmd =
                    new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@CourseId", courseId);
                cmd.Parameters.AddWithValue("@AcademicYear", academicYear);
                cmd.Parameters.AddWithValue("@Semester", semester);

                SqlDataAdapter da =
                    new SqlDataAdapter(cmd);

                DataTable dt =
                    new DataTable();

                da.Fill(dt);

                CurrentStudentData = dt;

                gvStudents.DataSource = dt;
                gvStudents.DataBind();
            }
        }

        protected void FilterCourses_Click(object sender, EventArgs e)
        {
            try
            {
                LinkButton clickedBtn = (LinkButton)sender;
                int semester = int.Parse(clickedBtn.CommandArgument);

                // Reset all filter visual classes
                btnFilterAll.CssClass = "filter-badge";
                foreach (Control ctrl in phSemesterFilters.Controls)
                {
                    if (ctrl is LinkButton btn)
                    {
                        btn.CssClass = "filter-badge";
                    }
                }

                // Apply active class to current selection
                clickedBtn.CssClass = "filter-badge active";

                LoadCourses(semester);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in FilterCourses_Click: {ex.Message}");
            }
        }

        protected void btnFilterAll_Click(object sender, EventArgs e)
        {
            try
            {
                btnFilterAll.CssClass = "filter-badge active";
                foreach (Control ctrl in phSemesterFilters.Controls)
                {
                    if (ctrl is LinkButton btn)
                    {
                        btn.CssClass = "filter-badge";
                    }
                }

                LoadCourses(0);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in btnFilterAll_Click: {ex.Message}");
            }
        }

        protected void rptCourses_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
        }

        private DataTable CurrentStudentData
        {
            get
            {
                return ViewState["CurrentStudentData"] as DataTable;
            }
            set
            {
                ViewState["CurrentStudentData"] = value;
            }
        }
        protected void btnViewStudents_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;
            string[] data = btn.CommandArgument.Split('|');

            int courseId = Convert.ToInt32(data[0]);
            int academicYear = Convert.ToInt32(data[1]);
            int semester = Convert.ToInt32(data[2]);

            // FIX: Grab the course name from the repeater item container to pass to the CSV metadata report
            RepeaterItem item = (RepeaterItem)btn.NamingContainer;
            Literal litCourseName = item.FindControl("litCourseName") as Literal;

            // Fallback if control lookup isn't used: read text or derive cleanly
            string courseName = data.Length > 3 ? data[3] : "Assigned Course Track";
            ViewState["ReportActiveMeta"] = courseName;

            LoadStudents(courseId, academicYear, semester);

            pnlStudentModal.Visible = true;
            pnlStudentModal.CssClass = "modal-overlay show";
        }

        protected void btnCloseModal_Click(
    object sender,
    EventArgs e)
        {
            pnlStudentModal.Visible = false;
        }

        protected void btnExportCsv_Click(object sender, EventArgs e)
        {
            // Retrieve the active dataset from your existing class properties
            DataTable dt = CurrentStudentData;
            if (dt == null || dt.Rows.Count == 0) return;

            string associatedCourse = Convert.ToString(ViewState["ReportActiveMeta"] ?? "Assigned Course System Track");
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm tt");

            Response.Clear();
            Response.Buffer = true;

            // Set content headers to let Excel know it is reading an XML Spreadsheet representation
            Response.ContentType = "application/vnd.ms-excel";
            Response.AddHeader("content-disposition", "attachment;filename=Enrolment_Roster_" + DateTime.Now.ToString("yyyyMMdd") + ".xls");
            Response.Charset = "utf-8";
            Response.ContentEncoding = Encoding.UTF8;

            StringBuilder sb = new StringBuilder();

            // 1. Core Spreadsheet Document Setup
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
            sb.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");
            sb.AppendLine(" xmlns:o=\"urn:schemas-microsoft-com:office:office\"");
            sb.AppendLine(" xmlns:x=\"urn:schemas-microsoft-com:office:excel\"");
            sb.AppendLine(" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"");
            sb.AppendLine(" xmlns:html=\"http://www.w3.org/TR/REC-html40\">");

            // 2. Dedicated UI Styling Panel Definitions
            sb.AppendLine(" <Styles>");
            sb.AppendLine("  <Style ss:ID=\"Default\" ss:Name=\"Normal\">");
            sb.AppendLine("   <Font ss:FontName=\"Segoe UI\" x:CharSet=\"1\" ss:Size=\"11\" ss:Color=\"#1E293B\"/>");
            sb.AppendLine("  </Style>");
            sb.AppendLine("  <Style ss:ID=\"ReportHeader\">");
            sb.AppendLine("   <Font ss:FontName=\"Segoe UI\" ss:Size=\"14\" ss:Bold=\"1\" ss:Color=\"#0D6EFD\"/>");
            sb.AppendLine("  </Style>");
            sb.AppendLine("  <Style ss:ID=\"MetadataLabel\">");
            sb.AppendLine("   <Font ss:FontName=\"Segoe UI\" ss:Size=\"10\" ss:Bold=\"1\" ss:Color=\"#64748B\"/>");
            sb.AppendLine("  </Style>");
            sb.AppendLine("  <Style ss:ID=\"MetadataValue\">");
            sb.AppendLine("   <Font ss:FontName=\"Segoe UI\" ss:Size=\"10\" ss:Color=\"#1E293B\"/>");
            sb.AppendLine("  </Style>");
            sb.AppendLine("  <Style ss:ID=\"TableHeader\">");
            sb.AppendLine("   <Interior ss:Color=\"#F8FAFC\" ss:Pattern=\"Solid\"/>");
            sb.AppendLine("   <Borders>");
            sb.AppendLine("    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"2\" ss:Color=\"#CBD5E1\"/>");
            sb.AppendLine("   </Borders>");
            sb.AppendLine("   <Font ss:FontName=\"Segoe UI\" ss:Size=\"11\" ss:Bold=\"1\" ss:Color=\"#1E293B\"/>");
            sb.AppendLine("  </Style>");
            sb.AppendLine("  <Style ss:ID=\"DataCell\">");
            sb.AppendLine("   <Borders>");
            sb.AppendLine("    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E2E8F0\"/>");
            sb.AppendLine("   </Borders>");
            sb.AppendLine("  </Style>");
            sb.AppendLine(" </Styles>");

            // 3. Document Worksheet Layout Block
            sb.AppendLine(" <Worksheet ss:Name=\"Enrolment Roster\">");
            sb.AppendLine("  <Table>");

            // CRITICAL ENHANCEMENT: Instructs Excel to evaluate the longest string per grid column and automatically resize it instantly!
            sb.AppendLine("   <Column ss:AutoFitWidth=\"1\" ss:Min=\"1\" ss:Max=\"4\"/>");

            // 4. Report Header Block Data Entries
            sb.AppendLine("   <Row ss:Height=\"25\">");
            sb.AppendLine("    <Cell ss:StyleID=\"ReportHeader\"><Data ss:Type=\"String\">STUDENT ENROLMENT ROSTER MANAGEMENT REPORT</Data></Cell>");
            sb.AppendLine("   </Row>");

            sb.AppendLine("   <Row>");
            sb.AppendLine("    <Cell ss:StyleID=\"MetadataLabel\"><Data ss:Type=\"String\">Course Track:</Data></Cell>");
            sb.AppendLine("    <Cell ss:StyleID=\"MetadataValue\"><Data ss:Type=\"String\">" + SecurityXmlConvert(associatedCourse) + "</Data></Cell>");
            sb.AppendLine("   </Row>");

            sb.AppendLine("   <Row>");
            sb.AppendLine("    <Cell ss:StyleID=\"MetadataLabel\"><Data ss:Type=\"String\">Compile Date:</Data></Cell>");
            sb.AppendLine("    <Cell ss:StyleID=\"MetadataValue\"><Data ss:Type=\"String\">" + SecurityXmlConvert(timestamp) + "</Data></Cell>");
            sb.AppendLine("   </Row>");

            sb.AppendLine("   <Row>");
            sb.AppendLine("    <Cell ss:StyleID=\"MetadataLabel\"><Data ss:Type=\"String\">Classification:</Data></Cell>");
            sb.AppendLine("    <Cell ss:StyleID=\"MetadataValue\"><Data ss:Type=\"String\">Official Institutional Record</Data></Cell>");
            sb.AppendLine("   </Row>");

            sb.AppendLine("   <Row ss:Height=\"15\"></Row>"); // Visual whitespace padding

            // 5. Data Matrix Grid Table Column Header
            sb.AppendLine("   <Row ss:Height=\"22\" ss:StyleID=\"TableHeader\">");
            sb.AppendLine("    <Cell><Data ss:Type=\"String\">Student No</Data></Cell>");
            sb.AppendLine("    <Cell><Data ss:Type=\"String\">Student Name</Data></Cell>");
            sb.AppendLine("    <Cell><Data ss:Type=\"String\">Email Address</Data></Cell>");
            sb.AppendLine("    <Cell><Data ss:Type=\"String\">Academic Programme Enrollment</Data></Cell>");
            sb.AppendLine("   </Row>");

            // 6. Data Matrix Value Iterations
            foreach (DataRow row in dt.Rows)
            {
                string cleanNo = SecurityXmlConvert(row["StudentNo"].ToString());
                string cleanName = SecurityXmlConvert(row["FullName"].ToString());
                string cleanEmail = SecurityXmlConvert(row["Email"].ToString());
                string cleanProg = SecurityXmlConvert(row["ProgrammeName"].ToString());

                sb.AppendLine("   <Row ss:Height=\"20\" ss:StyleID=\"DataCell\">");
                sb.AppendLine("    <Cell><Data ss:Type=\"String\">" + cleanNo + "</Data></Cell>");
                sb.AppendLine("    <Cell><Data ss:Type=\"String\">" + cleanName + "</Data></Cell>");
                sb.AppendLine("    <Cell><Data ss:Type=\"String\">" + cleanEmail + "</Data></Cell>");
                sb.AppendLine("    <Cell><Data ss:Type=\"String\">" + cleanProg + "</Data></Cell>");
                sb.AppendLine("   </Row>");
            }

            // 7. Structural Closures
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

        // Escapes special characters so they don't corrupt the structural layout of the spreadsheet engine
        private string SecurityXmlConvert(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return input.Replace("&", "&amp;")
                        .Replace("<", "&lt;")
                        .Replace(">", "&gt;")
                        .Replace("\"", "&quot;")
                        .Replace("'", "&apos;");
        }
    }
}