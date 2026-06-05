using System;
using System.Data;
using System.Web.UI.WebControls;
using SIMS.BLL;

namespace SIMS.Student
{
    public partial class AttendanceRecord : System.Web.UI.Page
    {
        private StudentAttendanceBLL attendanceBLL = new StudentAttendanceBLL();

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
                LoadAttendanceRecord();
            }
        }

        private void LoadAttendanceRecord()
        {
            int studentId = Convert.ToInt32(Session["StudentId"]);

            LoadOverallStats(studentId);
            LoadCourseSummary(studentId);
            LoadCourseFilter(studentId);
            LoadAttendanceDetails(studentId, 0);
        }

        private void LoadOverallStats(int studentId)
        {
            DataTable stats = attendanceBLL.GetOverallAttendanceStats(studentId);

            if (stats.Rows.Count > 0)
            {
                DataRow row = stats.Rows[0];

                lblTotalCourses.Text = row["TotalCourses"].ToString();
                lblTotalClasses.Text = row["TotalClasses"].ToString();
                lblAttendedClasses.Text = row["AttendedClasses"] == DBNull.Value ? "0" : row["AttendedClasses"].ToString();

                decimal percentage = row["OverallAttendancePercentage"] == DBNull.Value
                    ? 0
                    : Convert.ToDecimal(row["OverallAttendancePercentage"]);

                lblOverallAttendance.Text = percentage.ToString("0.00") + "%";
            }
        }

        private void LoadCourseSummary(int studentId)
        {
            DataTable summary = attendanceBLL.GetCourseAttendanceSummary(studentId);

            gvAttendanceSummary.DataSource = summary;
            gvAttendanceSummary.DataBind();

            if (summary.Rows.Count == 0)
            {
                lblMessage.Text = "No attendance records available yet.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void LoadAttendanceDetails(int studentId, int enrolmentId)
        {
            DataTable details = attendanceBLL.GetAttendanceDetails(studentId, enrolmentId);

            gvAttendanceDetails.DataSource = details;
            gvAttendanceDetails.DataBind();
        }

        protected void gvAttendanceDetails_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                Label lblStatus = (Label)e.Row.FindControl("lblStatus");

                if (lblStatus != null)
                {
                    string status = lblStatus.Text.ToLower();

                    if (status == "present")
                    {
                        lblStatus.CssClass = "status-present";
                    }
                    else if (status == "late")
                    {
                        lblStatus.CssClass = "status-late";
                    }
                    else if (status == "absent")
                    {
                        lblStatus.CssClass = "status-absent";
                    }
                }
            }
        }

        private void LoadCourseFilter(int studentId)
        {
            DataTable courses = attendanceBLL.GetAttendanceCourseFilter(studentId);

            ddlCourseFilter.DataSource = courses;
            ddlCourseFilter.DataTextField = "CourseDisplay";
            ddlCourseFilter.DataValueField = "EnrolmentId";
            ddlCourseFilter.DataBind();

            ddlCourseFilter.Items.Insert(0, new ListItem("All Courses", "0"));
        }
        protected void ddlCourseFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            int studentId = Convert.ToInt32(Session["StudentId"]);
            int enrolmentId = Convert.ToInt32(ddlCourseFilter.SelectedValue);

            LoadAttendanceDetails(studentId, enrolmentId);
        }
    }
}