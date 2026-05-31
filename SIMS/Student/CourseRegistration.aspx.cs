using System;
using SIMS.BLL;

namespace SIMS.Student
{
    public partial class CourseRegistration : System.Web.UI.Page
    {
        private StudentCourseBLL courseBLL = new StudentCourseBLL();

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
                LoadCourseRegistrationPage();
            }
        }

        private void LoadCourseRegistrationPage()
        {
            int studentId = Convert.ToInt32(Session["StudentId"]);

            int currentSemester = courseBLL.GetCurrentSemester(studentId);
            bool registrationOpen = courseBLL.IsRegistrationOpen(studentId);
            bool dropOpen = courseBLL.IsDropOpen(studentId);

            lblSemester.Text = "Current Semester: " + currentSemester;

            if (currentSemester == 1)
            {
                lblMessage.Text = "First semester course registration is managed by Admin. You cannot self-register courses.";
                lblMessage.ForeColor = System.Drawing.Color.DarkOrange;
            }
            else if (!registrationOpen && !dropOpen)
            {
                lblMessage.Text = "Course registration and drop periods are currently closed.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
            else if (registrationOpen && dropOpen)
            {
                lblMessage.Text = "Course registration and drop periods are currently open. All requests require Admin approval.";
                lblMessage.ForeColor = System.Drawing.Color.Green;
            }
            else if (registrationOpen)
            {
                lblMessage.Text = "Course registration is currently open. Course drop is currently closed.";
                lblMessage.ForeColor = System.Drawing.Color.Green;
            }
            else if (dropOpen)
            {
                lblMessage.Text = "Course drop is currently open. Course registration is currently closed.";
                lblMessage.ForeColor = System.Drawing.Color.DarkOrange;
            }

            gvRegistrationPeriods.DataSource = courseBLL.GetRegistrationPeriods(studentId);
            gvRegistrationPeriods.DataBind();

            gvAvailableCourses.DataSource = courseBLL.GetAvailableCourses(studentId);
            gvAvailableCourses.DataBind();

            gvEnrolledCourses.DataSource = courseBLL.GetEnrolledCourses(studentId);
            gvEnrolledCourses.DataBind();

            gvCourseRequests.DataSource = courseBLL.GetCourseRequests(studentId);
            gvCourseRequests.DataBind();
            pnlAvailableCourses.Visible = registrationOpen && currentSemester > 1;

            // Hide register button column if registration is closed or student is semester 1
            //gvAvailableCourses.Columns[4].Visible = registrationOpen && currentSemester > 1;

            // Hide drop button column if drop period is closed
            gvEnrolledCourses.Columns[6].Visible = dropOpen;
        }

        protected void gvAvailableCourses_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            if (e.CommandName == "RegisterCourse")
            {
                int studentId = Convert.ToInt32(Session["StudentId"]);
                int courseId = Convert.ToInt32(e.CommandArgument);

                string message = courseBLL.SubmitRegisterRequest(studentId, courseId);

                lblMessage.Text = message;

                if (message.Contains("successfully"))
                {
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                }
                else
                {
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }

                LoadCourseRegistrationPage();
            }
        }

        protected void gvEnrolledCourses_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DropCourse")
            {
                int studentId = Convert.ToInt32(Session["StudentId"]);
                int courseId = Convert.ToInt32(e.CommandArgument);

                string message = courseBLL.SubmitDropRequest(studentId, courseId);

                lblMessage.Text = message;

                if (message.Contains("successfully"))
                {
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                }
                else
                {
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }

                LoadCourseRegistrationPage();
            }
        }
    }
}