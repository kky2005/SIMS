using System;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using SIMS.BLL;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace SIMS.Student
{
    public partial class AcademicResults : System.Web.UI.Page
    {
        private StudentResultBLL resultBLL = new StudentResultBLL();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null ||
                Session["UserRole"] == null ||
                Session["UserRole"].ToString().ToLower() != "student")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadAcademicResults();
            }
        }

        private void LoadAcademicResults()
        {
            int studentId = Convert.ToInt32(Session["StudentId"]);

            LoadGPASummary(studentId);
            LoadSemesterCards(studentId);
        }

        private void LoadGPASummary(int studentId)
        {
            DataTable dt = resultBLL.GetGPASummary(studentId);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];

                lblGPA.Text = row["GPA"].ToString();
                lblCGPA.Text = row["CGPA"].ToString();
                lblCreditHours.Text = row["TotalCreditHours"].ToString();
                lblLatestSemester.Text = row["AcademicYear"].ToString() + " / Sem " + row["Semester"].ToString();
            }
            else
            {
                lblGPA.Text = "-";
                lblCGPA.Text = "-";
                lblCreditHours.Text = "-";
                lblLatestSemester.Text = "-";
            }
        }

        private void LoadSemesterCards(int studentId)
        {
            DataTable semesters = resultBLL.GetResultSemesters(studentId);

            rptSemesters.DataSource = semesters;
            rptSemesters.DataBind();

            if (semesters.Rows.Count == 0)
            {
                lblMessage.Text = "No academic result records found.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void rptSemesters_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item ||
                e.Item.ItemType == ListItemType.AlternatingItem)
            {
                DataRowView row = (DataRowView)e.Item.DataItem;

                int studentId = Convert.ToInt32(Session["StudentId"]);
                int academicYear = Convert.ToInt32(row["AcademicYear"]);
                int semester = Convert.ToInt32(row["Semester"]);

                GridView gvSemesterResults = (GridView)e.Item.FindControl("gvSemesterResults");

                if (gvSemesterResults != null)
                {
                    gvSemesterResults.DataSource = resultBLL.GetCourseResultsBySemester(studentId, academicYear, semester);
                    gvSemesterResults.DataBind();
                }
            }
        }

        protected void rptSemesters_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "GenerateReport")
            {
                string[] values = e.CommandArgument.ToString().Split('|');

                int academicYear = Convert.ToInt32(values[0]);
                int semester = Convert.ToInt32(values[1]);

                GenerateSemesterReport(academicYear, semester);
            }
        }

        private void GenerateSemesterReport(int academicYear, int semester)
        {
            int studentId = Convert.ToInt32(Session["StudentId"]);

            DataTable resultTable = resultBLL.GetCourseResultsBySemester(studentId, academicYear, semester);
            DataTable gpaTable = resultBLL.GetGPASummaryBySemester(studentId, academicYear, semester);

            ViewState["GeneratedAcademicYear"] = academicYear;
            ViewState["GeneratedSemester"] = semester;

            lblReportName.Text = Session["FullName"].ToString();
            lblReportStudentNo.Text = Session["StudentNo"].ToString();
            lblReportAcademicYear.Text = academicYear.ToString();
            lblReportSemester.Text = semester.ToString();

            gvGeneratedReport.DataSource = resultTable;
            gvGeneratedReport.DataBind();

            if (gpaTable.Rows.Count > 0)
            {
                DataRow row = gpaTable.Rows[0];

                lblReportCreditHours.Text = row["TotalCreditHours"].ToString();
                lblReportGPA.Text = row["GPA"].ToString();
                lblReportCGPA.Text = row["CGPA"].ToString();
            }
            else
            {
                lblReportCreditHours.Text = "-";
                lblReportGPA.Text = "-";
                lblReportCGPA.Text = "-";
            }

            pnlGeneratedReport.Visible = true;

            lblMessage.Text = "Academic result slip generated successfully. You may download it as PDF.";
            lblMessage.ForeColor = System.Drawing.Color.Green;
        }

        private void DownloadSemesterReport(int academicYear, int semester)
        {
            int studentId = Convert.ToInt32(Session["StudentId"]);
            string studentNo = Session["StudentNo"].ToString();

            DataTable dt = resultBLL.GetCourseResultsBySemester(studentId, academicYear, semester);

            StringBuilder csv = new StringBuilder();

            csv.AppendLine("Student No," + EscapeCsv(studentNo));
            csv.AppendLine("Academic Year," + academicYear);
            csv.AppendLine("Semester," + semester);
            csv.AppendLine();

            csv.AppendLine("Course Code,Course Name,Credit Hours,Total Mark,Grade,Grade Point,Result Status");

            foreach (DataRow row in dt.Rows)
            {
                csv.AppendLine(
                    EscapeCsv(row["CourseCode"].ToString()) + "," +
                    EscapeCsv(row["CourseName"].ToString()) + "," +
                    EscapeCsv(row["CreditHours"].ToString()) + "," +
                    EscapeCsv(row["TotalWeightedMark"].ToString()) + "," +
                    EscapeCsv(row["Grade"].ToString()) + "," +
                    EscapeCsv(row["GradePoint"].ToString()) + "," +
                    EscapeCsv(row["ResultStatus"].ToString())
                );
            }

            string fileName = studentNo + "_AcademicYear_" + academicYear + "_Semester_" + semester + "_Results.csv";

            Response.Clear();
            Response.Buffer = true;
            Response.ContentType = "text/csv";
            Response.AddHeader("content-disposition", "attachment;filename=" + fileName);
            Response.Charset = "";
            Response.Output.Write(csv.ToString());
            Response.Flush();
            Response.End();
        }

        protected void btnDownloadPdf_Click(object sender, EventArgs e)
        {
            if (ViewState["GeneratedAcademicYear"] == null || ViewState["GeneratedSemester"] == null)
            {
                lblMessage.Text = "Please generate a semester report before downloading.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            int studentId = Convert.ToInt32(Session["StudentId"]);
            string studentNo = Session["StudentNo"].ToString();
            string fullName = Session["FullName"].ToString();

            int academicYear = Convert.ToInt32(ViewState["GeneratedAcademicYear"]);
            int semester = Convert.ToInt32(ViewState["GeneratedSemester"]);

            DataTable resultTable = resultBLL.GetCourseResultsBySemester(studentId, academicYear, semester);
            DataTable gpaTable = resultBLL.GetGPASummaryBySemester(studentId, academicYear, semester);

            decimal gpa = 0;
            decimal cgpa = 0;
            int totalCreditHours = 0;

            if (gpaTable.Rows.Count > 0)
            {
                DataRow gpaRow = gpaTable.Rows[0];

                gpa = Convert.ToDecimal(gpaRow["GPA"]);
                cgpa = Convert.ToDecimal(gpaRow["CGPA"]);
                totalCreditHours = Convert.ToInt32(gpaRow["TotalCreditHours"]);
            }

            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 40, 40, 35, 35);
                PdfWriter.GetInstance(document, ms);

                document.Open();

                Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 15);
                Font subTitleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);
                Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                Font boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
                Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.WHITE);

                Paragraph title = new Paragraph("SIMS ACADEMIC RESULT SLIP", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 5;
                document.Add(title);

                Paragraph subtitle = new Paragraph("Student Information Management System", normalFont);
                subtitle.Alignment = Element.ALIGN_CENTER;
                subtitle.SpacingAfter = 18;
                document.Add(subtitle);

                PdfPTable infoTable = new PdfPTable(4);
                infoTable.WidthPercentage = 100;
                infoTable.SetWidths(new float[] { 18f, 32f, 18f, 32f });
                infoTable.SpacingAfter = 15;

                AddInfoCell(infoTable, "Name", boldFont);
                AddInfoCell(infoTable, fullName, normalFont);
                AddInfoCell(infoTable, "Student No", boldFont);
                AddInfoCell(infoTable, studentNo, normalFont);

                AddInfoCell(infoTable, "Academic Year", boldFont);
                AddInfoCell(infoTable, academicYear.ToString(), normalFont);
                AddInfoCell(infoTable, "Semester", boldFont);
                AddInfoCell(infoTable, semester.ToString(), normalFont);

                document.Add(infoTable);

                Paragraph tableTitle = new Paragraph("Semester Result", subTitleFont);
                tableTitle.SpacingAfter = 8;
                document.Add(tableTitle);

                PdfPTable resultPdfTable = new PdfPTable(6);
                resultPdfTable.WidthPercentage = 100;
                resultPdfTable.SetWidths(new float[] { 15f, 38f, 12f, 12f, 10f, 13f });
                resultPdfTable.SpacingAfter = 15;

                AddHeaderCell(resultPdfTable, "Code", headerFont);
                AddHeaderCell(resultPdfTable, "Subject Name", headerFont);
                AddHeaderCell(resultPdfTable, "Credit", headerFont);
                AddHeaderCell(resultPdfTable, "Marks", headerFont);
                AddHeaderCell(resultPdfTable, "Grade", headerFont);
                AddHeaderCell(resultPdfTable, "Point", headerFont);

                foreach (DataRow row in resultTable.Rows)
                {
                    AddBodyCell(resultPdfTable, row["CourseCode"].ToString(), normalFont);
                    AddBodyCell(resultPdfTable, row["CourseName"].ToString(), normalFont);
                    AddBodyCell(resultPdfTable, row["CreditHours"].ToString(), normalFont);
                    AddBodyCell(resultPdfTable, row["TotalWeightedMark"].ToString(), normalFont);
                    AddBodyCell(resultPdfTable, row["Grade"].ToString(), normalFont);
                    AddBodyCell(resultPdfTable, row["GradePoint"].ToString(), normalFont);
                }

                document.Add(resultPdfTable);

                PdfPTable summaryTable = new PdfPTable(3);
                summaryTable.WidthPercentage = 100;
                summaryTable.SetWidths(new float[] { 33f, 33f, 34f });
                summaryTable.SpacingBefore = 5;

                AddSummaryCell(summaryTable, "Total Credit Hours: " + totalCreditHours, boldFont);
                AddSummaryCell(summaryTable, "GPA: " + gpa.ToString("0.00"), boldFont);
                AddSummaryCell(summaryTable, "CGPA: " + cgpa.ToString("0.00"), boldFont);

                document.Add(summaryTable);

                Paragraph footer = new Paragraph(
                    "\nThis result slip is computer generated by SIMS.",
                    normalFont
                );
                footer.SpacingBefore = 20;
                document.Add(footer);

                document.Close();

                string fileName = studentNo + "_Year_" + academicYear + "_Sem_" + semester + "_ResultSlip.pdf";

                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "attachment;filename=" + fileName);
                Response.BinaryWrite(ms.ToArray());
                Response.Flush();
                Response.End();
            }
        }

        private void AddInfoCell(PdfPTable table, string text, Font font)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.Border = Rectangle.NO_BORDER;
            cell.Padding = 4;
            table.AddCell(cell);
        }

        private void AddHeaderCell(PdfPTable table, string text, Font font)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.BackgroundColor = new BaseColor(30, 64, 105);
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.Padding = 6;
            table.AddCell(cell);
        }

        private void AddBodyCell(PdfPTable table, string text, Font font)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.Padding = 5;
            table.AddCell(cell);
        }

        private void AddSummaryCell(PdfPTable table, string text, Font font)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.Padding = 7;
            cell.BackgroundColor = new BaseColor(241, 245, 249);
            table.AddCell(cell);
        }

        private string EscapeCsv(string value)
        {
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                value = value.Replace("\"", "\"\"");
                return "\"" + value + "\"";
            }

            return value;
        }
    }
}