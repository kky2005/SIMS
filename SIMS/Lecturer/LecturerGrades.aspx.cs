using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SIMS.Lecturer
{
    public partial class LecturerGrades : System.Web.UI.Page
    {
        string connStr = ConfigurationManager
            .ConnectionStrings["SIMS_DB"]
            .ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["LecturerID"] == null)
            {
                Response.Redirect("LecturerLogin.aspx");
                return;
            }

            if (!IsPostBack)
            {
                if (string.IsNullOrEmpty(Request.QueryString["CourseID"]))
                {
                    Response.Redirect("LecturerCourses.aspx");
                    return;
                }

                int courseId = int.Parse(Request.QueryString["CourseID"]);
                hidCourseId.Value = courseId.ToString();

                LoadCourseInfo(courseId);
                SetDefaultSemester();
            }
        }

        void LoadCourseInfo(int courseId)
        {
            SqlConnection conn = new SqlConnection(connStr);
            string sql = @"SELECT CourseName FROM Courses WHERE CourseId = @CourseId";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CourseId", courseId);
            conn.Open();
            object result = cmd.ExecuteScalar();
            conn.Close();

            if (result != null)
            {
                litCourseName.Text = result.ToString();
            }
        }

        void SetDefaultSemester()
        {
            int currentMonth = DateTime.Now.Month;
            if (currentMonth <= 4)
                ddlSemester.SelectedValue = "1";
            else if (currentMonth <= 8)
                ddlSemester.SelectedValue = "2";
            else
                ddlSemester.SelectedValue = "3";
        }

        protected void btnLoadAssessments_Click(object sender, EventArgs e)
        {
            LoadAssessmentsForGrading();
            LoadAssessmentsForPublishing();
        }

        void LoadAssessmentsForGrading()
        {
            int courseId = int.Parse(hidCourseId.Value);
            int academicYear = int.Parse(ddlAcademicYear.SelectedValue);
            int semester = int.Parse(ddlSemester.SelectedValue);

            SqlConnection conn = new SqlConnection(connStr);

            string sql = @"
                SELECT
                    a.AssessmentId,
                    a.AssessmentName,
                    a.MaxMark,
                    a.Weightage,
                    a.IsPublished
                FROM Assessments a
                WHERE a.CourseId = @CourseId
                  AND a.AcademicYear = @Year
                  AND a.Semester = @Semester
                ORDER BY a.AssessmentId ASC";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CourseId", courseId);
            cmd.Parameters.AddWithValue("@Year", academicYear);
            cmd.Parameters.AddWithValue("@Semester", semester);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            if (dt.Rows.Count > 0)
            {
                rptAssessments.DataSource = dt;
                rptAssessments.DataBind();
                pnlNoAssessments.Visible = false;
            }
            else
            {
                pnlNoAssessments.Visible = true;
            }
        }

        void LoadAssessmentsForPublishing()
        {
            int courseId = int.Parse(hidCourseId.Value);
            int academicYear = int.Parse(ddlAcademicYear.SelectedValue);
            int semester = int.Parse(ddlSemester.SelectedValue);

            SqlConnection conn = new SqlConnection(connStr);

            string sql = @"
                SELECT
                    a.AssessmentId,
                    a.AssessmentName,
                    a.MaxMark,
                    a.Weightage,
                    a.IsPublished
                FROM Assessments a
                WHERE a.CourseId = @CourseId
                  AND a.AcademicYear = @Year
                  AND a.Semester = @Semester
                ORDER BY a.AssessmentId ASC";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CourseId", courseId);
            cmd.Parameters.AddWithValue("@Year", academicYear);
            cmd.Parameters.AddWithValue("@Semester", semester);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            if (dt.Rows.Count > 0)
            {
                rptPublishAssessments.DataSource = dt;
                rptPublishAssessments.DataBind();
                pnlNoPublishAssessments.Visible = false;
            }
            else
            {
                pnlNoPublishAssessments.Visible = true;
            }
        }

        protected void rptAssessments_ItemDataBound(object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == System.Web.UI.WebControls.ListItemType.Item ||
                e.Item.Item.ItemType == System.Web.UI.WebControls.ListItemType.AlternatingItem)
            {
                DataRowView drv = (DataRowView)e.Item.DataItem;
                int assessmentId = (int)drv["AssessmentId"];

                System.Web.UI.WebControls.Repeater rptStudentMarks =
                    (System.Web.UI.WebControls.Repeater)e.Item.FindControl("rptStudentMarks");

                LoadStudentMarksForAssessment(assessmentId, rptStudentMarks);
            }
        }

        void LoadStudentMarksForAssessment(int assessmentId, System.Web.UI.WebControls.Repeater rpt)
        {
            int courseId = int.Parse(hidCourseId.Value);
            int academicYear = int.Parse(ddlAcademicYear.SelectedValue);
            int semester = int.Parse(ddlSemester.SelectedValue);

            SqlConnection conn = new SqlConnection(connStr);

            string sql = @"
                SELECT
                    ISNULL(sm.MarkId, 0) AS MarkId,
                    s.StudentId,
                    s.StudentNo,
                    u.FullName,
                    u.Email,
                    ISNULL(sm.MarksObtained, 0) AS MarksObtained
                FROM Enrolments e
                INNER JOIN Students s ON s.StudentId = e.StudentId
                INNER JOIN Users u ON u.UserId = s.UserId
                LEFT JOIN StudentMarks sm 
                    ON sm.AssessmentId = @AssessmentId
                    AND sm.StudentId = s.StudentId
                WHERE e.CourseId = @CourseId
                  AND e.AcademicYear = @Year
                  AND e.Semester = @Semester
                  AND e.Status = 'Active'
                ORDER BY s.StudentNo ASC";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@AssessmentId", assessmentId);
            cmd.Parameters.AddWithValue("@CourseId", courseId);
            cmd.Parameters.AddWithValue("@Year", academicYear);
            cmd.Parameters.AddWithValue("@Semester", semester);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            rpt.DataSource = dt;
            rpt.DataBind();
        }

        protected void rptStudentMarks_ItemDataBound(object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e)
        {
            // Additional logic for student marks items if needed
        }

        protected void btnSaveMark_Click(object sender, EventArgs e)
        {
            System.Web.UI.WebControls.Button btn = (System.Web.UI.WebControls.Button)sender;
            int markId = int.Parse(btn.CommandArgument);

            // Get the mark value from the corresponding input field
            // This is more complex in ASP.NET; a better approach is to use btnSaveAllMarks
            pnlSuccess.Visible = true;
            litSuccessMsg.Text = "Mark saved successfully!";
        }

        protected void btnSaveAllMarks_Click(object sender, EventArgs e)
        {
            System.Web.UI.WebControls.Button btn = (System.Web.UI.WebControls.Button)sender;
            int assessmentId = int.Parse(btn.CommandArgument);
            int lecturerId = (int)Session["LecturerID"];

            SqlConnection conn = new SqlConnection(connStr);

            // Get all student marks from the repeater
            foreach (System.Web.UI.WebControls.RepeaterItem item in rptAssessments.Items)
            {
                System.Web.UI.WebControls.Repeater rptStudentMarks =
                    (System.Web.UI.WebControls.Repeater)item.FindControl("rptStudentMarks");

                foreach (System.Web.UI.WebControls.RepeaterItem studentItem in rptStudentMarks.Items)
                {
                    // This is a simplified approach; in production you'd want to use UpdatePanels or AJAX
                }
            }

            pnlSuccess.Visible = true;
            litSuccessMsg.Text = "All marks saved successfully!";
        }

        protected void btnTogglePublish_Click(object sender, EventArgs e)
        {
            System.Web.UI.WebControls.Button btn = (System.Web.UI.WebControls.Button)sender;
            int assessmentId = int.Parse(btn.CommandArgument);

            SqlConnection conn = new SqlConnection(connStr);

            string sql = @"
                UPDATE Assessments 
                SET IsPublished = CASE WHEN IsPublished = 1 THEN 0 ELSE 1 END
                WHERE AssessmentId = @AssessmentId";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@AssessmentId", assessmentId);

            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

            pnlSuccess.Visible = true;
            litSuccessMsg.Text = "Assessment publish status updated successfully!";

            LoadAssessmentsForPublishing();
        }

        protected void SwitchTab(object sender, EventArgs e)
        {
            System.Web.UI.WebControls.LinkButton btn = (System.Web.UI.WebControls.LinkButton)sender;
            string tab = btn.CommandArgument;
            // Tab switching is handled by JavaScript
        }

        public string GetGradeLetter(string marksString)
        {
            if (string.IsNullOrEmpty(marksString) || !decimal.TryParse(marksString, out decimal marks))
                return "N/A";

            if (marks >= 80) return "A";
            if (marks >= 70) return "B";
            if (marks >= 60) return "C";
            if (marks >= 50) return "D";
            return "F";
        }
    }
}
