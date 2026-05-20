using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SIMS.Lecturer
{
    public partial class LecturerCourses : System.Web.UI.Page
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
                LoadCourses(0);
                btnFilterAll.CssClass = "filter-badge active";
            }
        }

        void LoadCourses(int semester)
        {
            int lecturerId = (int)Session["LecturerID"];
            int currentYear = DateTime.Now.Year;

            SqlConnection conn = new SqlConnection(connStr);

            string sql = @"
                SELECT
                    c.CourseId,
                    c.CourseCode,
                    c.CourseName,
                    c.CreditHours,
                    c.Semester,
                    ca.AcademicYear,
                    COUNT(e.EnrolmentId) AS TotalStudents
                FROM CourseAssignments ca
                INNER JOIN Courses c ON c.CourseId = ca.CourseId
                LEFT JOIN Enrolments e
                    ON e.CourseId = c.CourseId
                    AND e.AcademicYear = ca.AcademicYear
                    AND e.Semester = ca.Semester
                    AND e.Status = 'Active'
                WHERE ca.LecturerId = @LecturerId
                  AND ca.AcademicYear = @Year";

            if (semester > 0)
            {
                sql += " AND c.Semester = @Semester";
            }

            sql += @" GROUP BY c.CourseId, c.CourseCode, c.CourseName, 
                     c.CreditHours, c.Semester, ca.AcademicYear
                     ORDER BY c.Semester ASC, c.CourseCode ASC";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@LecturerId", lecturerId);
            cmd.Parameters.AddWithValue("@Year", currentYear);
            if (semester > 0)
            {
                cmd.Parameters.AddWithValue("@Semester", semester);
            }

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            if (dt.Rows.Count > 0)
            {
                rptCourses.DataSource = dt;
                rptCourses.DataBind();
                pnlNoCourses.Visible = false;
            }
            else
            {
                pnlNoCourses.Visible = true;
            }
        }

        protected void FilterCourses_Click(object sender, EventArgs e)
        {
            // Reset all buttons to inactive
            btnFilterAll.CssClass = "filter-badge";
            btnFilterSem1.CssClass = "filter-badge";
            btnFilterSem2.CssClass = "filter-badge";
            btnFilterSem3.CssClass = "filter-badge";

            // Get the clicked button
            System.Web.UI.WebControls.LinkButton btn = (System.Web.UI.WebControls.LinkButton)sender;
            int semester = int.Parse(btn.CommandArgument);

            // Set clicked button to active
            btn.CssClass = "filter-badge active";

            // Load courses with filter
            LoadCourses(semester);
        }

        protected void rptCourses_ItemDataBound(object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e)
        {
            // Any additional logic for repeater items can be added here
        }
    }
}
