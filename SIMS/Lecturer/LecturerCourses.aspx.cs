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

            // CRITICAL FIX: Dynamic controls MUST be re-created on every postback
            LoadAvailableSemesters();

            if (!IsPostBack)
            {
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

                        // Clear placeholder before re-adding controls to prevent duplicate sets
                        phSemesterFilters.Controls.Clear();

                        foreach (DataRow row in dt.Rows)
                        {
                            int semester = Convert.ToInt32(row["Semester"]);

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
                            COUNT(DISTINCT e.EnrolmentId) AS TotalStudents,
ca.Semester AS AssignmentSemester
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

                    sql += @" GROUP BY
                                c.CourseId,
                                c.CourseCode,
                                c.CourseName,
                                c.CreditHours,
                                c.Semester,
                                ca.Semester,
                                ca.AcademicYear
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
                            // CRITICAL FIX: Explicitly clear the layout control if no items match
                            rptCourses.DataSource = null;
                            rptCourses.DataBind();
                            pnlNoCourses.Visible = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading courses: {ex.Message}");
                rptCourses.DataSource = null;
                rptCourses.DataBind();
                pnlNoCourses.Visible = true;
            }
        }

        private void LoadStudents(
    int courseId,
    int academicYear,
    int semester)
        {
            using (SqlConnection conn =
                   new SqlConnection(connStr))
            {
                string sql = @"

SELECT
    s.StudentNo,
    u.FullName,
    u.Email,
    p.ProgrammeName

FROM Enrolments e

INNER JOIN Students s
ON e.StudentId = s.StudentId

INNER JOIN Users u
ON s.UserId = u.UserId

INNER JOIN Programmes p
ON s.ProgrammeId = p.ProgrammeId

WHERE
e.CourseId = @CourseId
AND e.AcademicYear = @AcademicYear
AND e.Semester = @Semester
AND e.Status = 'Active'

ORDER BY u.FullName";

                SqlCommand cmd =
                    new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@CourseId", courseId);
                cmd.Parameters.AddWithValue("@AcademicYear", academicYear);
                cmd.Parameters.AddWithValue("@Semester", semester);

                SqlDataAdapter da =
                    new SqlDataAdapter(cmd);

                DataTable dt =
                    new DataTable();

                da.Fill(dt);

                CurrentStudentData = dt;

                gvStudents.DataSource = dt;
                gvStudents.DataBind();
            }
        }

        protected void FilterCourses_Click(object sender, EventArgs e)
        {
            try
            {
                LinkButton clickedBtn = (LinkButton)sender;
                int semester = int.Parse(clickedBtn.CommandArgument);

                // Reset all filter visual classes
                btnFilterAll.CssClass = "filter-badge";
                foreach (Control ctrl in phSemesterFilters.Controls)
                {
                    if (ctrl is LinkButton btn)
                    {
                        btn.CssClass = "filter-badge";
                    }
                }

                // Apply active class to current selection
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
                {
                    if (ctrl is LinkButton btn)
                    {
                        btn.CssClass = "filter-badge";
                    }
                }

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

        private DataTable CurrentStudentData
        {
            get
            {
                return ViewState["CurrentStudentData"] as DataTable;
            }
            set
            {
                ViewState["CurrentStudentData"] = value;
            }
        }
        protected void btnViewStudents_Click(
    object sender,
    EventArgs e)
        {
            LinkButton btn =
                (LinkButton)sender;

            string[] data =
                btn.CommandArgument.Split('|');

            int courseId =
                Convert.ToInt32(data[0]);

            int academicYear =
                Convert.ToInt32(data[1]);

            int semester =
                Convert.ToInt32(data[2]);

            LoadStudents(
                courseId,
                academicYear,
                semester);

            pnlStudentModal.Visible = true;
            pnlStudentModal.CssClass = "modal-overlay show";
        }

        protected void btnCloseModal_Click(
    object sender,
    EventArgs e)
        {
            pnlStudentModal.Visible = false;
        }

        protected void btnExportCsv_Click(
    object sender,
    EventArgs e)
        {
            DataTable dt =
                CurrentStudentData;

            if (dt == null || dt.Rows.Count == 0)
                return;

            Response.Clear();

            Response.ContentType = "text/csv";

            Response.AddHeader(
                "content-disposition",
                "attachment;filename=StudentList.csv");

            Response.Write(
                "Student No,Student Name,Email,Programme\r\n");

            foreach (DataRow row in dt.Rows)
            {
                Response.Write(
                    "\"" + row["StudentNo"] + "\","
                    + "\"" + row["FullName"] + "\","
                    + "\"" + row["Email"] + "\","
                    + "\"" + row["ProgrammeName"] + "\"\r\n");
            }

            Response.End();
        }
    }
}