using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SIMS.Lecturer
{
    public partial class LecturerCourses : LecturerBase
    {
        string connStr = ConfigurationManager
            .ConnectionStrings["SIMS_DB"]
            .ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Ensure user is authenticated
            EnsureAuthenticated();

            if (!IsPostBack)
            {
                // Load available semesters from database
                LoadAvailableSemesters();
                
                // Load all courses
                LoadCourses(0);
                btnFilterAll.CssClass = "filter-badge active";
            }
        }

        /// <summary>
        /// Dynamically loads available semesters from the database for the current lecturer's courses.
        /// </summary>
        private void LoadAvailableSemesters()
        {
            try
            {
                int lecturerId = CurrentLecturerId;
                int currentYear = DateTime.Now.Year;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = @"
                        SELECT DISTINCT c.Semester
                        FROM CourseAssignments ca
                        INNER JOIN Courses c ON c.CourseId = ca.CourseId
                        WHERE ca.LecturerId = @LecturerId
                          AND ca.AcademicYear = @Year
                        ORDER BY c.Semester ASC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@LecturerId", lecturerId);
                        cmd.Parameters.AddWithValue("@Year", currentYear);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        // Create filter buttons for each available semester
                        foreach (DataRow row in dt.Rows)
                        {
                            int semester = (int)row["Semester"];
                            
                            LinkButton btnSemester = new LinkButton
                            {
                                ID = $"btnFilterSem{semester}",
                                Text = $"Semester {semester}",
                                CssClass = "filter-badge",
                                CommandArgument = semester.ToString()
                            };
                            btnSemester.Click += FilterCourses_Click;

                            phSemesterFilters.Controls.Add(btnSemester);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading available semesters: {ex.Message}");
            }
        }

        void LoadCourses(int semester)
        {
            try
            {
                int lecturerId = CurrentLecturerId;
                int currentYear = DateTime.Now.Year;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
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

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
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
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading courses: {ex.Message}");
                pnlNoCourses.Visible = true;
            }
        }

        protected void FilterCourses_Click(object sender, EventArgs e)
        {
            // Reset all filter buttons to inactive
            btnFilterAll.CssClass = "filter-badge";
            
            foreach (Control ctrl in phSemesterFilters.Controls)
            {
                if (ctrl is LinkButton btn)
                {
                    btn.CssClass = "filter-badge";
                }
            }

            // Get the clicked button
            LinkButton clickedBtn = (LinkButton)sender;
            int semester = int.Parse(clickedBtn.CommandArgument);

            // Set clicked button to active
            clickedBtn.CssClass = "filter-badge active";

            // Load courses with filter
            LoadCourses(semester);
        }

        protected void rptCourses_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            // Additional logic for repeater items can be added here
        }
    }
}
