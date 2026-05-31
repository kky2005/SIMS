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
        string connStr = ConfigurationManager.ConnectionStrings["SIMS_DB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            EnsureAuthenticated();

            if (!IsPostBack)
            {
                LoadAvailableSemesters();
                LoadCourses(0);
                btnFilterAll.CssClass = "filter-badge active";
            }
        }

        private void LoadAvailableSemesters()
        {
            try
            {
                int lecturerId = CurrentLecturerId;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = @"
                        SELECT DISTINCT c.Semester
                        FROM CourseAssignments ca
                        INNER JOIN Courses c ON c.CourseId = ca.CourseId
                        WHERE ca.LecturerId = @LecturerId
                        ORDER BY c.Semester ASC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@LecturerId", lecturerId);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

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
                System.Diagnostics.Debug.WriteLine($"Error loading semesters: {ex.Message}");
            }
        }

        void LoadCourses(int semester)
        {
            try
            {
                int lecturerId = CurrentLecturerId;

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
                            COUNT(DISTINCT e.EnrolmentId) AS TotalStudents
                        FROM CourseAssignments ca
                        INNER JOIN Courses c ON c.CourseId = ca.CourseId
                        LEFT JOIN Enrolments e
                            ON e.CourseId = c.CourseId
                            AND e.AcademicYear = ca.AcademicYear
                            AND e.Semester = ca.Semester
                            AND e.Status = 'Active'
                        WHERE ca.LecturerId = @LecturerId";

                    if (semester > 0)
                    {
                        sql += " AND c.Semester = @Semester";
                    }

                    sql += @" GROUP BY c.CourseId, c.CourseCode, c.CourseName, 
                             c.CreditHours, c.Semester, ca.AcademicYear
                             ORDER BY ca.AcademicYear DESC, c.Semester ASC, c.CourseCode ASC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@LecturerId", lecturerId);
                        if (semester > 0) cmd.Parameters.AddWithValue("@Semester", semester);

                        conn.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        conn.Close();

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
            try
            {
                btnFilterAll.CssClass = "filter-badge";
                foreach (Control ctrl in phSemesterFilters.Controls)
                    if (ctrl is LinkButton btn) btn.CssClass = "filter-badge";

                LinkButton clickedBtn = (LinkButton)sender;
                int semester = int.Parse(clickedBtn.CommandArgument);
                clickedBtn.CssClass = "filter-badge active";

                LoadCourses(semester);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in FilterCourses_Click: {ex.Message}");
            }
        }

        protected void btnFilterAll_Click(object sender, EventArgs e)
        {
            try
            {
                btnFilterAll.CssClass = "filter-badge active";
                foreach (Control ctrl in phSemesterFilters.Controls)
                    if (ctrl is LinkButton btn) btn.CssClass = "filter-badge";

                LoadCourses(0);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in btnFilterAll_Click: {ex.Message}");
            }
        }

        protected void rptCourses_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
        }
    }
}
