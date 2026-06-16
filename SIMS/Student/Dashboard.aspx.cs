using System;
using System.Configuration;
using System.Data.SqlClient;
using SIMS.BLL;

namespace SIMS.Student
{
    public partial class Dashboard : System.Web.UI.Page
    {
        private string connStr =
            ConfigurationManager.ConnectionStrings["SIMS_DB"].ConnectionString;

        private StudentResultBLL resultBLL =
            new StudentResultBLL();

        private StudentNotificationBLL notificationBLL =
            new StudentNotificationBLL();

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
                lblWelcome.Text =
                    "Welcome, " + Session["FullName"].ToString();

                lblStudentNo.Text =
                    "Student No: " + Session["StudentNo"].ToString();

                LoadDashboardSummary();
                LoadUnreadNotificationCount();
            }
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();

            Response.Redirect("../Login.aspx");
        }

        private void LoadDashboardSummary()
        {
            int studentId =
                Convert.ToInt32(Session["StudentId"]);

            // Recalculate GPA and CGPA before displaying dashboard values
            resultBLL.RecalculateGPARecords(studentId);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // =========================
                // CURRENT SEMESTER
                // =========================

                string semesterQuery = @"
                    SELECT CurrentSemester
                    FROM Students
                    WHERE StudentId = @StudentId";

                SqlCommand semesterCmd =
                    new SqlCommand(semesterQuery, conn);

                semesterCmd.Parameters.AddWithValue(
                    "@StudentId",
                    studentId
                );

                object semesterResult =
                    semesterCmd.ExecuteScalar();

                if (semesterResult != null &&
                    semesterResult != DBNull.Value)
                {
                    lblCurrentSemester.Text =
                        semesterResult.ToString();
                }
                else
                {
                    lblCurrentSemester.Text = "-";
                }

                // =========================
                // CURRENT ENROLLED COURSES
                // =========================

                string enrolledQuery = @"
                    SELECT COUNT(*)
                    FROM Enrolments e
                    INNER JOIN Students s
                        ON e.StudentId = s.StudentId
                    WHERE e.StudentId = @StudentId
                      AND e.Semester = s.CurrentSemester
                      AND e.Status = 'Active'";

                SqlCommand enrolledCmd =
                    new SqlCommand(enrolledQuery, conn);

                enrolledCmd.Parameters.AddWithValue(
                    "@StudentId",
                    studentId
                );

                object enrolledResult =
                    enrolledCmd.ExecuteScalar();

                if (enrolledResult != null &&
                    enrolledResult != DBNull.Value)
                {
                    lblEnrolledCourses.Text =
                        enrolledResult.ToString();
                }
                else
                {
                    lblEnrolledCourses.Text = "0";
                }

                // =========================
                // LATEST CGPA
                // =========================

                string cgpaQuery = @"
                    SELECT TOP 1 CGPA
                    FROM GPARecords
                    WHERE StudentId = @StudentId
                    ORDER BY
                        AcademicYear DESC,
                        Semester DESC,
                        CalculatedAt DESC";

                SqlCommand cgpaCmd =
                    new SqlCommand(cgpaQuery, conn);

                cgpaCmd.Parameters.AddWithValue(
                    "@StudentId",
                    studentId
                );

                object cgpaResult =
                    cgpaCmd.ExecuteScalar();

                if (cgpaResult != null &&
                    cgpaResult != DBNull.Value)
                {
                    lblCGPA.Text =
                        Convert.ToDecimal(cgpaResult).ToString("0.00");
                }
                else
                {
                    lblCGPA.Text = "-";
                }
            }
        }

        private void LoadUnreadNotificationCount()
        {
            int userId =
                Convert.ToInt32(Session["UserId"]);

            int unreadCount =
                notificationBLL.GetUnreadNotificationCount(userId);

            string badgeText =
                unreadCount > 99
                    ? "99+"
                    : unreadCount.ToString();

            // Dashboard notification summary card
            lblNotifications.Text = unreadCount.ToString();

            if (unreadCount > 0)
            {
                // Top-right bell badge
                lblDashboardUnreadBadge.Text = badgeText;
                lblDashboardUnreadBadge.Visible = true;

                // Sidebar notification badge
                lblSidebarUnreadBadge.Text = badgeText;
                lblSidebarUnreadBadge.Visible = true;
            }
            else
            {
                lblDashboardUnreadBadge.Visible = false;
                lblSidebarUnreadBadge.Visible = false;
            }
        }
    }
}