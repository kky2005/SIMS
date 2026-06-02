using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SIMS.Lecturer
{
    public partial class LecturerStudentProgress : LecturerBase
    {
        string connStr = ConfigurationManager.ConnectionStrings["SIMS_DB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            EnsureAuthenticated();

            if (!IsPostBack)
            {
                LoadCourses();
                LoadStudentProgress(0, "");
            }
        }

        void LoadCourses()
        {
            try
            {
                int lecturerId = CurrentLecturerId;
                int currentYear = DateTime.Now.Year;
                int currentSemester = GetCurrentSemester();

                SqlConnection conn = new SqlConnection(connStr);

                string sql = @"
                    SELECT DISTINCT
                        c.CourseId,
                        c.CourseCode,
                        c.CourseName
                    FROM CourseAssignments ca
                    INNER JOIN Courses c ON c.CourseId = ca.CourseId
                    WHERE ca.LecturerId = @LecturerId
                      AND ca.AcademicYear = @Year
                      AND ca.Semester = @Semester
                    ORDER BY c.CourseCode";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@LecturerId", lecturerId);
                cmd.Parameters.AddWithValue("@Year", currentYear);
                cmd.Parameters.AddWithValue("@Semester", currentSemester);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ddlCourse.DataSource = dt;
                ddlCourse.DataTextField = "CourseName";
                ddlCourse.DataValueField = "CourseId";
                ddlCourse.DataBind();

                ddlCourse.Items.Insert(0, new ListItem("-- All Courses --", "0"));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading courses: {ex.Message}");
            }
        }

        void LoadStudentProgress(int courseId, string riskLevel)
        {
            try
            {
                int lecturerId = CurrentLecturerId;
                int currentYear = DateTime.Now.Year;
                int currentSemester = GetCurrentSemester();

                SqlConnection conn = new SqlConnection(connStr);

                string sql = @"
                    SELECT
                        s.StudentId,
                        s.StudentNo,
                        u.FullName,
                        u.Email,
                        ISNULL(CAST(ROUND(att.AttendancePercent, 1) AS NVARCHAR(10)), '0') AS AttendancePercent,
                        ISNULL(CAST(ROUND(gpa.AverageGPA, 2) AS NVARCHAR(10)), '0.00') AS CurrentGPA,
                        CASE 
                            WHEN ISNULL(att.AttendancePercent, 0) < 75 THEN 'High'
                            WHEN ISNULL(att.AttendancePercent, 0) < 85 THEN 'Medium'
                            ELSE 'Low'
                        END AS RiskLevel,
                        COUNT(DISTINCT a.AssessmentId) AS AssignmentStatus
                    FROM Enrolments e
                    INNER JOIN Students s ON s.StudentId = e.StudentId
                    INNER JOIN Users u ON u.UserId = s.UserId
                    INNER JOIN CourseAssignments ca 
                        ON ca.CourseId = e.CourseId
                        AND ca.AcademicYear = e.AcademicYear
                        AND ca.Semester = e.Semester
                    LEFT JOIN (
                        SELECT 
                            EnrolmentId,
                            100.0 * SUM(CASE WHEN Status = 'Present' THEN 1 ELSE 0 END) / NULLIF(COUNT(*), 0) AS AttendancePercent
                        FROM Attendance
                        GROUP BY EnrolmentId
                    ) att ON att.EnrolmentId = e.EnrolmentId
                    LEFT JOIN (
                        SELECT 
                            sm.StudentId,
                            AVG(CAST(sm.MarksObtained AS FLOAT) / NULLIF(a.MaxMark, 0)) * 100 / 100 AS AverageGPA
                        FROM StudentMarks sm
                        INNER JOIN Assessments a ON a.AssessmentId = sm.AssessmentId
                        GROUP BY sm.StudentId
                    ) gpa ON gpa.StudentId = s.StudentId
                    LEFT JOIN Assessments a ON a.CourseId = e.CourseId
                    WHERE ca.LecturerId = @LecturerId
                      AND ca.AcademicYear = @Year
                      AND ca.Semester = @Semester
                      AND e.Status = 'Active'";

                if (courseId > 0)
                {
                    sql += " AND e.CourseId = @CourseId";
                }

                sql += " GROUP BY s.StudentId, s.StudentNo, u.FullName, u.Email, att.AttendancePercent, gpa.AverageGPA ORDER BY s.StudentNo";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@LecturerId", lecturerId);
                cmd.Parameters.AddWithValue("@Year", currentYear);
                cmd.Parameters.AddWithValue("@Semester", currentSemester);
                if (courseId > 0)
                {
                    cmd.Parameters.AddWithValue("@CourseId", courseId);
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // Apply risk level filter if specified
                if (!string.IsNullOrEmpty(riskLevel) && dt.Rows.Count > 0)
                {
                    DataView dv = dt.DefaultView;
                    dv.RowFilter = $"RiskLevel = '{riskLevel}'";
                    rptStudentProgress.DataSource = dv;
                }
                else
                {
                    rptStudentProgress.DataSource = dt;
                }

                rptStudentProgress.DataBind();
                pnlNoData.Visible = (dt.Rows.Count == 0);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading student progress: {ex.Message}");
                pnlNoData.Visible = true;
            }
        }

        protected void btnApplyFilter_Click(object sender, EventArgs e)
        {
            int courseId = 0;
            if (int.TryParse(ddlCourse.SelectedValue, out courseId))
            {
                LoadStudentProgress(courseId, ddlRiskLevel.SelectedValue);
            }
        }

        private int GetCurrentSemester()
        {
            int month = DateTime.Now.Month;
            if (month <= 4) return 1;
            if (month <= 8) return 2;
            return 3;
        }
    }
}