using System;
using System.Configuration;
using System.Data.SqlClient;
using SIMS.BLL;

namespace SIMS.Student
{
    public partial class Dashboard : System.Web.UI.Page
    {
        private string connStr = ConfigurationManager.ConnectionStrings["SIMS_DB"].ConnectionString;
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
                lblWelcome.Text = "Welcome, " + Session["FullName"].ToString();
                lblStudentNo.Text = "Student No: " + Session["StudentNo"].ToString();

                LoadDashboardSummary();
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
            int studentId = Convert.ToInt32(Session["StudentId"]);
            int userId = Convert.ToInt32(Session["UserId"]);

            // Recalculate GPA/CGPA before showing dashboard value
            resultBLL.RecalculateGPARecords(studentId);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // =========================
                // Current Semester
                // =========================
                string semesterQuery = @"
                    SELECT CurrentSemester
                    FROM Students
                    WHERE StudentId = @StudentId";

                SqlCommand semesterCmd = new SqlCommand(semesterQuery, conn);
                semesterCmd.Parameters.AddWithValue("@StudentId", studentId);

                object semesterResult = semesterCmd.ExecuteScalar();

                if (semesterResult != null && semesterResult != DBNull.Value)
                {
                    lblCurrentSemester.Text = semesterResult.ToString();
                }
                else
                {
                    lblCurrentSemester.Text = "-";
                }

                // =========================
                // Current Enrolled Courses
                // =========================
                string enrolledQuery = @"
                    SELECT COUNT(*)
                    FROM Enrolments e
                    INNER JOIN Students s 
                        ON e.StudentId = s.StudentId
                    WHERE e.StudentId = @StudentId
                      AND e.Semester = s.CurrentSemester
                      AND e.Status = 'Active'";

                SqlCommand enrolledCmd = new SqlCommand(enrolledQuery, conn);
                enrolledCmd.Parameters.AddWithValue("@StudentId", studentId);

                object enrolledResult = enrolledCmd.ExecuteScalar();
                lblEnrolledCourses.Text = enrolledResult != null ? enrolledResult.ToString() : "0";

                // =========================
                // Latest CGPA
                // =========================
                string cgpaQuery = @"
                    SELECT TOP 1 CGPA
                    FROM GPARecords
                    WHERE StudentId = @StudentId
                    ORDER BY AcademicYear DESC, Semester DESC, CalculatedAt DESC";

                SqlCommand cgpaCmd = new SqlCommand(cgpaQuery, conn);
                cgpaCmd.Parameters.AddWithValue("@StudentId", studentId);

                object cgpaResult = cgpaCmd.ExecuteScalar();

                if (cgpaResult != null && cgpaResult != DBNull.Value)
                {
                    lblCGPA.Text = Convert.ToDecimal(cgpaResult).ToString("0.00");
                }
                else
                {
                    lblCGPA.Text = "-";
                }

                // =========================
                // Notifications Count
                // =========================
                string notificationQuery = @"
                    SELECT COUNT(*)
                    FROM Notifications
                    WHERE UserId = @UserId";

                SqlCommand notificationCmd = new SqlCommand(notificationQuery, conn);
                notificationCmd.Parameters.AddWithValue("@UserId", userId);

                object notificationResult = notificationCmd.ExecuteScalar();
                lblNotifications.Text = notificationResult != null ? notificationResult.ToString() : "0";
            }
        }
    }
}