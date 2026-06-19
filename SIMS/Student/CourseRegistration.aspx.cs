using System;
using System.Drawing;
using System.Web.UI.WebControls;
using SIMS.BLL;

namespace SIMS.Student
{
    public partial class CourseRegistration : System.Web.UI.Page
    {
        private readonly StudentCourseBLL courseBLL =
            new StudentCourseBLL();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null ||
                Session["UserRole"] == null ||
                Session["StudentId"] == null ||
                !Session["UserRole"].ToString()
                    .Equals("student", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                LoadCourseRegistrationPage();
                DisplayFlashMessage();
            }
        }

        private void LoadCourseRegistrationPage()
        {
            int studentId = Convert.ToInt32(Session["StudentId"]);

            int currentSemester =
                courseBLL.GetCurrentSemester(studentId);

            bool registrationOpen =
                courseBLL.IsRegistrationOpen(studentId);

            bool dropOpen =
                courseBLL.IsDropOpen(studentId);

            lblSemester.Text =
                "Current Semester: " + currentSemester;

            if (currentSemester == 1)
            {
                lblMessage.Text =
                    "First semester course registration is managed by Admin. " +
                    "You cannot self-register courses.";

                lblMessage.ForeColor = Color.DarkOrange;
            }
            else if (!registrationOpen && !dropOpen)
            {
                lblMessage.Text =
                    "Course registration and drop periods are currently closed.";

                lblMessage.ForeColor = Color.Red;
            }
            else if (registrationOpen && dropOpen)
            {
                lblMessage.Text =
                    "Course registration and drop periods are currently open. " +
                    "All requests require approval.";

                lblMessage.ForeColor = Color.Green;
            }
            else if (registrationOpen)
            {
                lblMessage.Text =
                    "Course registration is currently open. " +
                    "Course drop is currently closed.";

                lblMessage.ForeColor = Color.Green;
            }
            else
            {
                lblMessage.Text =
                    "Course drop is currently open. " +
                    "Course registration is currently closed.";

                lblMessage.ForeColor = Color.DarkOrange;
            }

            gvRegistrationPeriods.DataSource =
                courseBLL.GetRegistrationPeriods(studentId);

            gvRegistrationPeriods.DataBind();

            gvAvailableCourses.DataSource =
                courseBLL.GetAvailableCourses(studentId);

            gvAvailableCourses.DataBind();

            gvEnrolledCourses.DataSource =
                courseBLL.GetEnrolledCourses(studentId);

            gvEnrolledCourses.DataBind();

            gvCourseRequests.DataSource =
                courseBLL.GetCourseRequests(studentId);

            gvCourseRequests.DataBind();

            pnlAvailableCourses.Visible =
                registrationOpen && currentSemester > 1;

            // Drop action is available only during an open drop period.
            if (gvEnrolledCourses.Columns.Count > 6)
            {
                gvEnrolledCourses.Columns[6].Visible = dropOpen;
            }
        }

        protected void gvAvailableCourses_RowCommand(
            object sender,
            GridViewCommandEventArgs e)
        {
            if (!e.CommandName.Equals(
                    "RegisterCourse",
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            int studentId =
                Convert.ToInt32(Session["StudentId"]);

            int courseId =
                Convert.ToInt32(e.CommandArgument);

            string message =
                courseBLL.SubmitRegisterRequest(
                    studentId,
                    courseId);

            bool success =
                message.IndexOf(
                    "successfully",
                    StringComparison.OrdinalIgnoreCase) >= 0;

            RedirectAfterRequest(message, success);
        }

        protected void gvEnrolledCourses_RowCommand(
            object sender,
            GridViewCommandEventArgs e)
        {
            if (!e.CommandName.Equals(
                    "DropCourse",
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            int studentId =
                Convert.ToInt32(Session["StudentId"]);

            int courseId =
                Convert.ToInt32(e.CommandArgument);

            string message =
                courseBLL.SubmitDropRequest(
                    studentId,
                    courseId);

            bool success =
                message.IndexOf(
                    "successfully",
                    StringComparison.OrdinalIgnoreCase) >= 0;

            RedirectAfterRequest(message, success);
        }

        private void RedirectAfterRequest(
            string message,
            bool success)
        {
            /*
             * Store the message temporarily because redirecting creates
             * a new HTTP request.
             */
            Session["CourseRegistrationMessage"] = message;
            Session["CourseRegistrationMessageType"] =
                success ? "Success" : "Error";

            /*
             * Post/Redirect/Get:
             * The original POST request is replaced by a normal GET request.
             * Browser refresh will therefore not insert the request again.
             */
            Response.Redirect(
                ResolveUrl("~/Student/CourseRegistration.aspx"),
                false);

            Context.ApplicationInstance.CompleteRequest();
        }

        private void DisplayFlashMessage()
        {
            if (Session["CourseRegistrationMessage"] == null)
            {
                return;
            }

            lblMessage.Text =
                Session["CourseRegistrationMessage"].ToString();

            string messageType =
                Convert.ToString(
                    Session["CourseRegistrationMessageType"]);

            lblMessage.ForeColor =
                messageType == "Success"
                    ? Color.Green
                    : Color.Red;

            Session.Remove("CourseRegistrationMessage");
            Session.Remove("CourseRegistrationMessageType");
        }
    }
}