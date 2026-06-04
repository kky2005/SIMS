using System;
using System.Data;
using SIMS.BLL;

namespace SIMS.Student
{
    public partial class CourseDetails : System.Web.UI.Page
    {
        private StudentEnrolledCourseBLL courseBLL = new StudentEnrolledCourseBLL();

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
                LoadCourseDetails();
            }
        }

        private void LoadCourseDetails()
        {
            if (Request.QueryString["CourseId"] == null)
            {
                lblMessage.Text = "No course selected.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            int studentId = Convert.ToInt32(Session["StudentId"]);
            int courseId = Convert.ToInt32(Request.QueryString["CourseId"]);

            DataTable courseTable = courseBLL.GetCourseDetails(studentId, courseId);

            if (courseTable.Rows.Count == 0)
            {
                lblMessage.Text = "Course not found or you are not enrolled in this course.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            DataRow row = courseTable.Rows[0];

            lblCourseTitle.Text = row["CourseCode"].ToString() + " - " + row["CourseName"].ToString();
            lblCourseCode.Text = row["CourseCode"].ToString();
            lblCourseName.Text = row["CourseName"].ToString();
            lblCreditHours.Text = row["CreditHours"].ToString();
            lblYearSemester.Text = row["AcademicYear"].ToString() + " / Sem " + row["Semester"].ToString();

            LoadCourseMaterials(studentId, courseId);
        }

        private void LoadCourseMaterials(int studentId, int courseId)
        {
            DataTable materials = courseBLL.GetCourseMaterials(studentId, courseId);

            gvMaterials.DataSource = materials;
            gvMaterials.DataBind();
        }
    }
}