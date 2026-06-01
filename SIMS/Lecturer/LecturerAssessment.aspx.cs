using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SIMS.Lecturer
{
    public partial class LecturerAssessments : LecturerBase
    {
        string connStr = ConfigurationManager.ConnectionStrings["SIMS_DB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            EnsureAuthenticated();

            if (!IsPostBack)
            {
                if (string.IsNullOrEmpty(Request.QueryString["CourseID"]) ||
                    !int.TryParse(Request.QueryString["CourseID"], out int courseId))
                {
                    Response.Redirect("LecturerCourses.aspx");
                    return;
                }

                if (!LecturerTeachesCourse(courseId))
                {
                    Response.Redirect("LecturerCourses.aspx");
                    return;
                }

                hidCourseId.Value = courseId.ToString();

                LoadCourseHeader(courseId);
                var assigned = GetMostRecentAssignment(courseId);
                int academicYear = assigned.year > 0 ? assigned.year : DateTime.Now.Year;
                int semester = assigned.semester > 0 ? assigned.semester : GetCurrentSemester();

                hidAcademicYear.Value = academicYear.ToString();
                litAcademicYear.Text = academicYear.ToString();
                ddlSemester.SelectedValue = semester.ToString();

                LoadAssessments();
            }
        }

        private bool LecturerTeachesCourse(int courseId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = "SELECT COUNT(1) FROM CourseAssignments WHERE CourseId = @CourseId AND LecturerId = @LecturerId";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CourseId", courseId);
                        cmd.Parameters.AddWithValue("@LecturerId", CurrentLecturerId);
                        conn.Open();
                        int c = Convert.ToInt32(cmd.ExecuteScalar());
                        conn.Close();
                        return c > 0;
                    }
                }
            }
            catch { return false; }
        }

        private void LoadCourseHeader(int courseId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = "SELECT CourseCode, CourseName FROM Courses WHERE CourseId = @CourseId";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CourseId", courseId);
                        conn.Open();
                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                string code = r["CourseCode"].ToString();
                                string name = r["CourseName"].ToString();
                                litCourseName.Text = $"{code} - {name}";
                                litCourseHeader.Text = $"{code} - {name} (Assessments)";
                            }
                        }
                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading course header: {ex.Message}");
            }
        }

        private (int year, int semester) GetMostRecentAssignment(int courseId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = @"
                        SELECT TOP 1 AcademicYear, Semester
                        FROM CourseAssignments
                        WHERE CourseId = @CourseId
                          AND LecturerId = @LecturerId
                        ORDER BY AcademicYear DESC, Semester DESC, AssignedDate DESC";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CourseId", courseId);
                        cmd.Parameters.AddWithValue("@LecturerId", CurrentLecturerId);
                        conn.Open();
                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                int y = r["AcademicYear"] != DBNull.Value ? Convert.ToInt32(r["AcademicYear"]) : 0;
                                int s = r["Semester"] != DBNull.Value ? Convert.ToInt32(r["Semester"]) : 0;
                                return (y, s);
                            }
                        }
                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting assignment: {ex.Message}");
            }
            return (0, 0);
        }

        void LoadAssessments()
        {
            try
            {
                int courseId = int.Parse(hidCourseId.Value);
                int year = int.Parse(hidAcademicYear.Value);
                int semester = int.Parse(ddlSemester.SelectedValue);

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = @"
                        SELECT AssessmentId, AssessmentName, MaxMark, Weightage, Semester, IsPublished
                        FROM Assessments
                        WHERE CourseId = @CourseId
                          AND AcademicYear = @Year
                          AND Semester = @Semester
                        ORDER BY AssessmentName ASC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CourseId", courseId);
                        cmd.Parameters.AddWithValue("@Year", year);
                        cmd.Parameters.AddWithValue("@Semester", semester);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        rptAssessments.DataSource = dt;
                        rptAssessments.DataBind();

                        pnlNoAssessments.Visible = (dt.Rows.Count == 0);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading assessments: {ex.Message}");
                pnlNoAssessments.Visible = true;
            }
        }

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            try
            {
                int courseId = int.Parse(hidCourseId.Value);
                int year = int.Parse(hidAcademicYear.Value);
                int semester = int.Parse(ddlSemester.SelectedValue);

                if (string.IsNullOrWhiteSpace(txtAssessmentName.Text))
                {
                    ShowError("Please enter an assessment name.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtMaxMark.Text) || !int.TryParse(txtMaxMark.Text, out int maxMark))
                {
                    ShowError("Please enter a valid max mark.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtWeightage.Text) || !decimal.TryParse(txtWeightage.Text, out decimal weightage))
                {
                    ShowError("Please enter a valid weightage.");
                    return;
                }

                if (maxMark <= 0)
                {
                    ShowError("Max mark must be greater than 0.");
                    return;
                }

                if (weightage < 0 || weightage > 100)
                {
                    ShowError("Weightage must be between 0 and 100.");
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = @"
                        INSERT INTO Assessments
                        (CourseId, AcademicYear, Semester, AssessmentName, MaxMark, Weightage, IsPublished)
                        VALUES (@CourseId, @AcademicYear, @Semester, @AssessmentName, @MaxMark, @Weightage, 0)";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CourseId", courseId);
                        cmd.Parameters.AddWithValue("@AcademicYear", year);
                        cmd.Parameters.AddWithValue("@Semester", semester);
                        cmd.Parameters.AddWithValue("@AssessmentName", txtAssessmentName.Text.Trim());
                        cmd.Parameters.AddWithValue("@MaxMark", maxMark);
                        cmd.Parameters.AddWithValue("@Weightage", weightage);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                }

                ShowSuccess("Assessment created successfully.");
                txtAssessmentName.Text = "";
                txtMaxMark.Text = "";
                txtWeightage.Text = "";
                LoadAssessments();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating assessment: {ex.Message}");
                ShowError("Error creating assessment: " + ex.Message);
            }
        }

        protected void btnTogglePublish_Click(object sender, EventArgs e)
        {
            try
            {
                if (!int.TryParse(((Button)sender).CommandArgument, out int assessmentId))
                    return;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = @"
                        UPDATE Assessments 
                        SET IsPublished = CASE WHEN IsPublished = 1 THEN 0 ELSE 1 END
                        WHERE AssessmentId = @AssessmentId";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@AssessmentId", assessmentId);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                }

                ShowSuccess("Assessment publish status updated.");
                LoadAssessments();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error toggling publish: {ex.Message}");
                ShowError("Error updating assessment: " + ex.Message);
            }
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (!int.TryParse(((Button)sender).CommandArgument, out int assessmentId))
                    return;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    // Delete associated student marks first
                    string delMarks = "DELETE FROM StudentMarks WHERE AssessmentId = @AssessmentId";
                    using (SqlCommand cmd = new SqlCommand(delMarks, conn))
                    {
                        cmd.Parameters.AddWithValue("@AssessmentId", assessmentId);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }

                    // Delete assessment
                    string delAssessment = "DELETE FROM Assessments WHERE AssessmentId = @AssessmentId";
                    using (SqlCommand cmd = new SqlCommand(delAssessment, conn))
                    {
                        cmd.Parameters.AddWithValue("@AssessmentId", assessmentId);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                }

                ShowSuccess("Assessment deleted.");
                LoadAssessments();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting assessment: {ex.Message}");
                ShowError("Error deleting assessment: " + ex.Message);
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtAssessmentName.Text = "";
            txtMaxMark.Text = "";
            txtWeightage.Text = "";
        }

        private void ShowSuccess(string msg)
        {
            pnlSuccess.Visible = true;
            litSuccessMsg.Text = msg;
            pnlError.Visible = false;
        }

        private void ShowError(string msg)
        {
            pnlError.Visible = true;
            litErrorMsg.Text = msg;
            pnlSuccess.Visible = false;
        }

        private int GetCurrentSemester()
        {
            int m = DateTime.Now.Month;
            if (m <= 4) return 1;
            if (m <= 8) return 2;
            return 3;
        }
    }
}