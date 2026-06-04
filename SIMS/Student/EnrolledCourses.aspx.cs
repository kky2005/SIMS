using System;
using System.Data;
using System.Web.UI.WebControls;
using SIMS.BLL;

namespace SIMS.Student
{
    public partial class EnrolledCourses : System.Web.UI.Page
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
                LoadEnrolledCourses();
            }
        }

        private void LoadEnrolledCourses()
        {
            int studentId = Convert.ToInt32(Session["StudentId"]);

            DataTable dt = courseBLL.GetCurrentEnrolledCourses(studentId);

            gvEnrolledCourses.DataSource = dt;
            gvEnrolledCourses.DataBind();

            if (dt.Rows.Count == 0)
            {
                lblMessage.Text = "You are not currently enrolled in any active courses.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void gvEnrolledCourses_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ViewCourse")
            {
                int courseId = Convert.ToInt32(e.CommandArgument);
                Response.Redirect("CourseDetails.aspx?CourseId=" + courseId);
            }
        }
    }
}