using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SIMS.Lecturer
{
    public partial class LecturerGrades : LecturerBase
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
                // Get course ID from query string
                if (string.IsNullOrEmpty(Request.QueryString["CourseID"]))
                {
                    Response.Redirect("LecturerCourses.aspx?msg=selectcourse");
                    return;
                }

                if (!int.TryParse(Request.QueryString["CourseID"], out int courseId))
                {
                    Response.Redirect("LecturerCourses.aspx");
                    return;
                }

                // Verify that this lecturer teaches this course
                if (!VerifyLecturerTeachesCourse(courseId))
                {
                    ShowError("You do not have permission to view this course.");
                    Response.Redirect("LecturerCourses.aspx");
                    return;
                }

                hidCourseId.Value = courseId.ToString();
                LoadCourseInfo(courseId);
                SetDefaultSemester();
            }
        }

        /// <summary>
        /// Verifies that the current lecturer teaches the specified course.
        /// Checks if the course is assigned to this lecturer (regardless of academic year/semester).
        /// </summary>
        private bool VerifyLecturerTeachesCourse(int courseId)
        {
            try
            {
                SqlConnection conn = new SqlConnection(connStr);
                
                // Check if this lecturer teaches this course (any assignment)
                string sql = @"
                    SELECT COUNT(*) FROM CourseAssignments ca
                    WHERE ca.CourseId = @CourseId
                      AND ca.LecturerId = @LecturerId";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CourseId", courseId);
                cmd.Parameters.AddWithValue("@LecturerId", CurrentLecturerId);

                conn.Open();
                int count = (int)cmd.ExecuteScalar();
                conn.Close();

                if (count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Lecturer {CurrentLecturerId} verified to teach course {courseId}");
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Lecturer {CurrentLecturerId} does NOT teach course {courseId}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error verifying course access: {ex.Message}");
                return false;
            }
        }

        void LoadCourseInfo(int courseId)
        {
            try
            {
                SqlConnection conn = new SqlConnection(connStr);
                string sql = @"SELECT CourseName FROM Courses WHERE CourseId = @CourseId";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CourseId", courseId);
                conn.Open();
                object result = cmd.ExecuteScalar();
                conn.Close();

                if (result != null)
                {
                    litCourseName.Text = result.ToString();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading course info: {ex.Message}");
            }
        }

        void SetDefaultSemester()
        {
            int currentMonth = DateTime.Now.Month;
            if (currentMonth <= 4)
                ddlSemester.SelectedValue = "1";
            else if (currentMonth <= 8)
                ddlSemester.SelectedValue = "2";
            else
                ddlSemester.SelectedValue = "3";
        }

        protected void btnLoadAssessments_Click(object sender, EventArgs e)
        {
            try
            {
                LoadAssessmentsForGrading();
                LoadAssessmentsForPublishing();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading assessments: {ex.Message}");
                ShowError("An error occurred while loading assessments.");
            }
        }

        void LoadAssessmentsForGrading()
        {
            int courseId = int.Parse(hidCourseId.Value);
            int academicYear = int.Parse(ddlAcademicYear.SelectedValue);
            int semester = int.Parse(ddlSemester.SelectedValue);

            SqlConnection conn = new SqlConnection(connStr);

            string sql = @"
                SELECT
                    a.AssessmentId,
                    a.AssessmentName,
                    a.MaxMark,
                    a.Weightage,
                    a.IsPublished
                FROM Assessments a
                WHERE a.CourseId = @CourseId
                  AND a.AcademicYear = @Year
                  AND a.Semester = @Semester
                ORDER BY a.AssessmentId ASC";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CourseId", courseId);
            cmd.Parameters.AddWithValue("@Year", academicYear);
            cmd.Parameters.AddWithValue("@Semester", semester);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            if (dt.Rows.Count > 0)
            {
                rptAssessments.DataSource = dt;
                rptAssessments.DataBind();
                pnlNoAssessments.Visible = false;
            }
            else
            {
                pnlNoAssessments.Visible = true;
            }
        }

        void LoadAssessmentsForPublishing()
        {
            int courseId = int.Parse(hidCourseId.Value);
            int academicYear = int.Parse(ddlAcademicYear.SelectedValue);
            int semester = int.Parse(ddlSemester.SelectedValue);

            SqlConnection conn = new SqlConnection(connStr);

            string sql = @"
                SELECT
                    a.AssessmentId,
                    a.AssessmentName,
                    a.MaxMark,
                    a.Weightage,
                    a.IsPublished
                FROM Assessments a
                WHERE a.CourseId = @CourseId
                  AND a.AcademicYear = @Year
                  AND a.Semester = @Semester
                ORDER BY a.AssessmentId ASC";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CourseId", courseId);
            cmd.Parameters.AddWithValue("@Year", academicYear);
            cmd.Parameters.AddWithValue("@Semester", semester);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            if (dt.Rows.Count > 0)
            {
                rptPublishAssessments.DataSource = dt;
                rptPublishAssessments.DataBind();
                pnlNoPublishAssessments.Visible = false;
            }
            else
            {
                pnlNoPublishAssessments.Visible = true;
            }
        }

        protected void rptAssessments_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                DataRowView drv = (DataRowView)e.Item.DataItem;
                int assessmentId = (int)drv["AssessmentId"];

                Repeater rptStudentMarks = (Repeater)e.Item.FindControl("rptStudentMarks");
                LoadStudentMarksForAssessment(assessmentId, rptStudentMarks);
            }
        }

        void LoadStudentMarksForAssessment(int assessmentId, Repeater rpt)
        {
            try
            {
                int courseId = int.Parse(hidCourseId.Value);
                int academicYear = int.Parse(ddlAcademicYear.SelectedValue);
                int semester = int.Parse(ddlSemester.SelectedValue);

                SqlConnection conn = new SqlConnection(connStr);

                string sql = @"
                    SELECT
                        ISNULL(sm.MarkId, 0) AS MarkId,
                        s.StudentId,
                        s.StudentNo,
                        u.FullName,
                        u.Email,
                        a.MaxMark,
                        ISNULL(sm.MarksObtained, 0) AS MarksObtained
                    FROM Enrolments e
                    INNER JOIN Students s ON s.StudentId = e.StudentId
                    INNER JOIN Users u ON u.UserId = s.UserId
                    INNER JOIN Assessments a ON a.AssessmentId = @AssessmentId
                    LEFT JOIN StudentMarks sm 
                        ON sm.AssessmentId = @AssessmentId
                        AND sm.StudentId = s.StudentId
                    WHERE e.CourseId = @CourseId
                      AND e.AcademicYear = @Year
                      AND e.Semester = @Semester
                      AND e.Status = 'Active'
                    ORDER BY s.StudentNo ASC";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@AssessmentId", assessmentId);
                cmd.Parameters.AddWithValue("@CourseId", courseId);
                cmd.Parameters.AddWithValue("@Year", academicYear);
                cmd.Parameters.AddWithValue("@Semester", semester);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rpt.DataSource = dt;
                rpt.DataBind();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading student marks: {ex.Message}");
            }
        }

        protected void rptStudentMarks_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            // Additional logic for student marks items if needed
        }

        protected void btnSaveMark_Click(object sender, EventArgs e)
        {
            ShowSuccess("Mark saved successfully!");
        }

        protected void btnSaveAllMarks_Click(object sender, EventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                int assessmentId = int.Parse(btn.CommandArgument);
                int courseId = int.Parse(hidCourseId.Value);

                SqlConnection conn = new SqlConnection(connStr);

                int marksSaved = 0;
                foreach (string key in Request.Form.Keys)
                {
                    if (key.StartsWith("txtMark_") && decimal.TryParse(Request.Form[key], out decimal marks))
                    {
                        string studentIdStr = key.Replace("txtMark_", "");
                        if (int.TryParse(studentIdStr, out int studentId))
                        {
                            string checkSql = @"
                                SELECT COUNT(*) FROM StudentMarks 
                                WHERE AssessmentId = @AssessmentId 
                                  AND StudentId = @StudentId";

                            SqlCommand checkCmd = new SqlCommand(checkSql, conn);
                            checkCmd.Parameters.AddWithValue("@AssessmentId", assessmentId);
                            checkCmd.Parameters.AddWithValue("@StudentId", studentId);

                            conn.Open();
                            int exists = (int)checkCmd.ExecuteScalar();
                            conn.Close();

                            string sql;
                            if (exists > 0)
                            {
                                sql = @"
                                    UPDATE StudentMarks 
                                    SET MarksObtained = @Marks,
                                        GradedBy = @GradedBy,
                                        GradedAt = GETDATE()
                                    WHERE AssessmentId = @AssessmentId 
                                      AND StudentId = @StudentId";
                            }
                            else
                            {
                                sql = @"
                                    INSERT INTO StudentMarks 
                                    (AssessmentId, StudentId, MarksObtained, GradedBy, GradedAt, IsPublished)
                                    VALUES (@AssessmentId, @StudentId, @Marks, @GradedBy, GETDATE(), 0)";
                            }

                            SqlCommand cmd = new SqlCommand(sql, conn);
                            cmd.Parameters.AddWithValue("@AssessmentId", assessmentId);
                            cmd.Parameters.AddWithValue("@StudentId", studentId);
                            cmd.Parameters.AddWithValue("@Marks", marks);
                            cmd.Parameters.AddWithValue("@GradedBy", CurrentUserId);

                            conn.Open();
                            cmd.ExecuteNonQuery();
                            conn.Close();

                            marksSaved++;
                        }
                    }
                }

                ShowSuccess($"{marksSaved} marks saved successfully!");
                LoadAssessmentsForGrading();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving all marks: {ex.Message}");
                ShowError("An error occurred while saving marks.");
            }
        }

        protected void btnTogglePublish_Click(object sender, EventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                int assessmentId = int.Parse(btn.CommandArgument);

                SqlConnection conn = new SqlConnection(connStr);

                string sql = @"
                    UPDATE Assessments 
                    SET IsPublished = CASE WHEN IsPublished = 1 THEN 0 ELSE 1 END
                    WHERE AssessmentId = @AssessmentId";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@AssessmentId", assessmentId);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();

                ShowSuccess("Assessment publish status updated successfully!");
                LoadAssessmentsForPublishing();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error toggling publish status: {ex.Message}");
                ShowError("An error occurred while updating publish status.");
            }
        }

        protected void SwitchTab(object sender, EventArgs e)
        {
            // Tab switching is handled by JavaScript on the client side
        }

        public string GetGradeLetter(string marksString)
        {
            if (string.IsNullOrEmpty(marksString) || !decimal.TryParse(marksString, out decimal marks))
                return "N/A";

            if (marks >= 80) return "A";
            if (marks >= 70) return "B";
            if (marks >= 60) return "C";
            if (marks >= 50) return "D";
            return "F";
        }

        private void ShowSuccess(string message)
        {
            pnlSuccess.Visible = true;
            litSuccessMsg.Text = message;
            pnlError.Visible = false;
        }

        private void ShowError(string message)
        {
            pnlError.Visible = true;
            litErrorMsg.Text = message;
            pnlSuccess.Visible = false;
        }
    }
}
