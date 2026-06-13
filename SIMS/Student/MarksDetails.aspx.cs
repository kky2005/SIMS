using System;
using System.Data;
using System.Web.UI.WebControls;
using SIMS.BLL;

namespace SIMS.Student
{
    public partial class MarksDetails : System.Web.UI.Page
    {
        private StudentMarksDetailBLL marksBLL = new StudentMarksDetailBLL();

        private DataTable allAssessmentMarks;

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
                LoadMarksDetails();
            }
        }

        private void LoadMarksDetails()
        {
            if (Request.QueryString["AcademicYear"] == null ||
                Request.QueryString["Semester"] == null)
            {
                lblMessage.Text = "Academic year or semester was not selected.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            int studentId = Convert.ToInt32(Session["StudentId"]);
            int academicYear = Convert.ToInt32(Request.QueryString["AcademicYear"]);
            int semester = Convert.ToInt32(Request.QueryString["Semester"]);

            LoadSemesterInfo(studentId, academicYear, semester);

            allAssessmentMarks = marksBLL.GetSemesterMarksDetails(studentId, academicYear, semester);
            DataTable courseTotals = marksBLL.GetCourseTotals(studentId, academicYear, semester);

            if (courseTotals.Rows.Count == 0)
            {
                lblMessage.Text = "No marks details found for the selected semester.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            rptCourses.DataSource = courseTotals;
            rptCourses.DataBind();
        }

        private void LoadSemesterInfo(int studentId, int academicYear, int semester)
        {
            DataTable infoTable = marksBLL.GetSemesterInfo(studentId, academicYear, semester);

            if (infoTable.Rows.Count > 0)
            {
                DataRow row = infoTable.Rows[0];

                lblStudentName.Text = row["FullName"].ToString();
                lblStudentNo.Text = row["StudentNo"].ToString();
                lblAcademicYear.Text = row["AcademicYear"].ToString();
                lblSemester.Text = row["Semester"].ToString();
            }
            else
            {
                lblStudentName.Text = Session["FullName"] != null ? Session["FullName"].ToString() : "-";
                lblStudentNo.Text = Session["StudentNo"] != null ? Session["StudentNo"].ToString() : "-";
                lblAcademicYear.Text = academicYear.ToString();
                lblSemester.Text = semester.ToString();
            }
        }

        protected void rptCourses_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item ||
                e.Item.ItemType == ListItemType.AlternatingItem)
            {
                HiddenField hfCourseId = (HiddenField)e.Item.FindControl("hfCourseId");
                GridView gvAssessments = (GridView)e.Item.FindControl("gvAssessments");

                if (hfCourseId == null || gvAssessments == null || allAssessmentMarks == null)
                {
                    return;
                }

                int courseId = Convert.ToInt32(hfCourseId.Value);

                DataView view = new DataView(allAssessmentMarks);
                view.RowFilter = "CourseId = " + courseId;

                DataTable filteredTable = view.ToTable();

                AddDisplayColumns(filteredTable);

                gvAssessments.DataSource = filteredTable;
                gvAssessments.DataBind();
            }
        }

        private void AddDisplayColumns(DataTable table)
        {
            if (!table.Columns.Contains("MarksObtainedDisplay"))
            {
                table.Columns.Add("MarksObtainedDisplay", typeof(string));
            }

            if (!table.Columns.Contains("WeightedMarkDisplay"))
            {
                table.Columns.Add("WeightedMarkDisplay", typeof(string));
            }

            if (!table.Columns.Contains("StatusDisplay"))
            {
                table.Columns.Add("StatusDisplay", typeof(string));
            }

            if (!table.Columns.Contains("GradedAtDisplay"))
            {
                table.Columns.Add("GradedAtDisplay", typeof(string));
            }

            foreach (DataRow row in table.Rows)
            {
                row["MarksObtainedDisplay"] =
                    row["MarksObtained"] == DBNull.Value
                        ? "-"
                        : Convert.ToDecimal(row["MarksObtained"]).ToString("0.00");

                row["WeightedMarkDisplay"] =
                    row["WeightedMark"] == DBNull.Value
                        ? "-"
                        : Convert.ToDecimal(row["WeightedMark"]).ToString("0.00");

                bool markPublished =
                    row["IsPublished"] != DBNull.Value &&
                    Convert.ToBoolean(row["IsPublished"]);

                bool assessmentPublished =
                    row["AssessmentPublished"] != DBNull.Value &&
                    Convert.ToBoolean(row["AssessmentPublished"]);

                row["StatusDisplay"] =
                    markPublished && assessmentPublished
                        ? "Published"
                        : "In Progress";

                row["GradedAtDisplay"] =
                    row["GradedAt"] == DBNull.Value
                        ? "-"
                        : Convert.ToDateTime(row["GradedAt"]).ToString("dd MMM yyyy hh:mm tt");
            }
        }
    }
}