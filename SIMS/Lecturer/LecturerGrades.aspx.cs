using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using SIMS.Services;

namespace SIMS.Lecturer
{
    public partial class LecturerGrades : LecturerBase
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

                var assignment = GetMostRecentAssignment(courseId);
                if (assignment.year <= 0 || assignment.semester <= 0)
                {
                    ShowError("No course assignment found for this lecturer.");
                    return;
                }

                hidCourseId.Value = courseId.ToString();
                LoadCourseInfo(courseId);
                LoadAcademicYears(assignment.year);
                LoadSemesters(assignment.semester);
                btnLoadAssessments_Click(null, null);
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
                        WHERE CourseId = @CourseId AND LecturerId = @LecturerId
                        ORDER BY AcademicYear DESC, Semester DESC, AssignedDate DESC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CourseId", courseId);
                        cmd.Parameters.AddWithValue("@LecturerId", CurrentLecturerId);
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                                return (Convert.ToInt32(reader["AcademicYear"]), Convert.ToInt32(reader["Semester"]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting assignment: {ex.Message}");
            }
            return (0, 0);
        }

        private void LoadCourseInfo(int courseId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT CourseCode, CourseName FROM Courses WHERE CourseId = @CourseId", conn))
                    {
                        cmd.Parameters.AddWithValue("@CourseId", courseId);
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                                litCourseName.Text = $"{reader["CourseCode"]} - {reader["CourseName"]}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading course info: {ex.Message}");
            }
        }

        private void LoadAcademicYears(int selectedYear)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT DISTINCT AcademicYear FROM CourseAssignments WHERE LecturerId = @LecturerId ORDER BY AcademicYear DESC", conn))
                    {
                        cmd.Parameters.AddWithValue("@LecturerId", CurrentLecturerId);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        ddlAcademicYear.Items.Clear();
                        foreach (DataRow row in dt.Rows)
                        {
                            int year = Convert.ToInt32(row["AcademicYear"]);
                            ddlAcademicYear.Items.Add(new ListItem(year.ToString(), year.ToString()));
                        }

                        if (ddlAcademicYear.Items.FindByValue(selectedYear.ToString()) != null)
                            ddlAcademicYear.SelectedValue = selectedYear.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading years: {ex.Message}");
            }
        }

        private void LoadSemesters(int selectedSemester)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT DISTINCT Semester FROM CourseAssignments WHERE LecturerId = @LecturerId AND Semester IS NOT NULL ORDER BY Semester ASC", conn))
                    {
                        cmd.Parameters.AddWithValue("@LecturerId", CurrentLecturerId);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        ddlSemester.Items.Clear();
                        foreach (DataRow row in dt.Rows)
                        {
                            if (row["Semester"] != DBNull.Value)
                            {
                                int sem = Convert.ToInt32(row["Semester"]);
                                ddlSemester.Items.Add(new ListItem($"Semester {sem}", sem.ToString()));
                            }
                        }

                        if (selectedSemester > 0 && ddlSemester.Items.FindByValue(selectedSemester.ToString()) != null)
                            ddlSemester.SelectedValue = selectedSemester.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading semesters: {ex.Message}");
            }
        }

        protected void btnLoadAssessments_Click(object sender, EventArgs e)
        {
            try
            {
                LoadAssessmentsForGrading();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                ShowError("Error loading assessments.");
            }
        }

        private void LoadAssessmentsForGrading()
        {
            int courseId = int.Parse(hidCourseId.Value);
            int year = int.Parse(ddlAcademicYear.SelectedValue);
            int semester = int.Parse(ddlSemester.SelectedValue);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT AssessmentId, AssessmentName, MaxMark, Weightage, IsPublished FROM Assessments WHERE CourseId = @CourseId AND AcademicYear = @Year AND Semester = @Semester ORDER BY AssessmentName ASC", conn))
                {
                    cmd.Parameters.AddWithValue("@CourseId", courseId);
                    cmd.Parameters.AddWithValue("@Year", year);
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

        private void LoadStudentMarksForAssessment(int assessmentId, Repeater rpt)
        {
            int courseId = int.Parse(hidCourseId.Value);
            int year = int.Parse(ddlAcademicYear.SelectedValue);
            int semester = int.Parse(ddlSemester.SelectedValue);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"
                    SELECT
                        s.StudentId, 
                        s.StudentNo, 
                        u.FullName, 
                        u.Email, 
                        a.MaxMark,
                        sm.MarksObtained,
                        ISNULL(asub.SubmissionId, 0) AS SubmissionId,
                        ISNULL(asub.FileName, '') AS FileName,
                        ISNULL(asub.FileUrl, '') AS FileUrl,
                        ISNULL(asub.SubmittedAt, '') AS SubmittedAt,
                        ISNULL(asub.Status, 'Not Submitted') AS SubmissionStatus
                    FROM Enrolments e
                    INNER JOIN Students s ON s.StudentId = e.StudentId
                    INNER JOIN Users u ON u.UserId = s.UserId
                    INNER JOIN Assessments a ON a.AssessmentId = @AssessmentId
                    LEFT JOIN StudentMarks sm ON sm.AssessmentId = @AssessmentId AND sm.StudentId = s.StudentId
                    LEFT JOIN (
                        SELECT SubmissionId, StudentId, AssessmentId, FileName, FileUrl, SubmittedAt, Status
                        FROM AssessmentSubmissions
                        WHERE IsLatest = 1
                    ) asub ON asub.AssessmentId = @AssessmentId AND asub.StudentId = s.StudentId
                    WHERE e.CourseId = @CourseId 
                      AND e.AcademicYear = @Year 
                      AND e.Semester = @Semester 
                      AND e.Status = 'Active'
                    ORDER BY s.StudentNo ASC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@AssessmentId", assessmentId);
                    cmd.Parameters.AddWithValue("@CourseId", courseId);
                    cmd.Parameters.AddWithValue("@Year", year);
                    cmd.Parameters.AddWithValue("@Semester", semester);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // Add MaxMark column to DataTable for the input max attribute
                    if (!dt.Columns.Contains("MaxMark"))
                    {
                        dt.Columns.Add("MaxMark", typeof(decimal));
                        foreach (DataRow row in dt.Rows)
                        {
                            row["MaxMark"] = 100; // Default value
                        }
                    }

                    rpt.DataSource = dt;
                    rpt.DataBind();
                }
            }
        }

        protected void btnSaveAllMarks_Click(object sender, EventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                int assessmentId = int.Parse(btn.CommandArgument);
                int courseId = int.Parse(hidCourseId.Value);
                int year = int.Parse(ddlAcademicYear.SelectedValue);
                int semester = int.Parse(ddlSemester.SelectedValue);

                int marksSaved = 0;
                int marksUpdated = 0;

                decimal maxMark = 100;
                decimal weightage = 0;
                bool isAssessmentPublished = false;

                // 1. Fetch parameters from parent Assessment record
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string assessmentSql = "SELECT MaxMark, Weightage, IsPublished FROM Assessments WHERE AssessmentId = @AssessmentId";
                    using (SqlCommand cmd = new SqlCommand(assessmentSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@AssessmentId", assessmentId);
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                maxMark = reader["MaxMark"] != DBNull.Value ? Convert.ToDecimal(reader["MaxMark"]) : 100;
                                weightage = reader["Weightage"] != DBNull.Value ? Convert.ToDecimal(reader["Weightage"]) : 0;
                                isAssessmentPublished = reader["IsPublished"] != DBNull.Value && Convert.ToBoolean(reader["IsPublished"]);
                            }
                        }
                    }
                }

                // Get Active Enrolled Students
                DataTable studentList = new DataTable();
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT DISTINCT s.StudentId FROM Enrolments e INNER JOIN Students s ON s.StudentId = e.StudentId WHERE e.CourseId = @CourseId AND e.AcademicYear = @Year AND e.Semester = @Semester AND e.Status = 'Active'", conn))
                    {
                        cmd.Parameters.AddWithValue("@CourseId", courseId);
                        cmd.Parameters.AddWithValue("@Year", year);
                        cmd.Parameters.AddWithValue("@Semester", semester);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(studentList);
                    }
                }

                var studentsUpdated = new List<int>();

                foreach (DataRow row in studentList.Rows)
                {
                    int studentId = Convert.ToInt32(row["StudentId"]);
                    string fieldName = $"txtMark_{assessmentId}_{studentId}";
                    string markValue = Request.Form[fieldName];

                    if (!string.IsNullOrEmpty(markValue) && decimal.TryParse(markValue, out decimal marks) && marks >= 0)
                    {
                        // Validate mark doesn't exceed max mark
                        if (marks > maxMark)
                        {
                            ShowError($"Mark {marks} exceeds maximum mark {maxMark} for a student. Please correct and try again.");
                            return;
                        }

                        // 2. Proportional Calculations for WeightedMark and Grade Mapping
                        decimal weightedMark = maxMark > 0 ? (marks / maxMark) * weightage : 0;
                        decimal percentageScore = maxMark > 0 ? (marks / maxMark) * 100 : 0;

                        object gradeScaleIdObj = DBNull.Value;

                        // 3. Match GradeScale database rules
                        using (SqlConnection conn = new SqlConnection(connStr))
                        {
                            string scaleSql = "SELECT TOP 1 GradeScaleId FROM GradeScale WHERE @Percentage >= MinMark AND @Percentage <= MaxMark ORDER BY MinMark DESC";
                            using (SqlCommand scaleCmd = new SqlCommand(scaleSql, conn))
                            {
                                scaleCmd.Parameters.AddWithValue("@Percentage", percentageScore);
                                conn.Open();
                                object res = scaleCmd.ExecuteScalar();
                                if (res != null) gradeScaleIdObj = res;
                            }
                        }

                        using (SqlConnection conn = new SqlConnection(connStr))
                        {
                            conn.Open();

                            int markId = 0;
                            using (SqlCommand checkCmd = new SqlCommand(
                                "SELECT MarkId FROM StudentMarks WHERE AssessmentId = @AssessmentId AND StudentId = @StudentId", conn))
                            {
                                checkCmd.Parameters.AddWithValue("@AssessmentId", assessmentId);
                                checkCmd.Parameters.AddWithValue("@StudentId", studentId);
                                object result = checkCmd.ExecuteScalar();
                                if (result != null) markId = Convert.ToInt32(result);
                            }

                            if (markId > 0)
                            {
                                string updateSql = @"
                                    UPDATE StudentMarks 
                                    SET MarksObtained = @Marks, 
                                        WeightedMark = @WeightedMark,
                                        GradeScaleId = @GradeScaleId,
                                        IsPublished = @IsPublished,
                                        GradedBy = @GradedBy, 
                                        GradedAt = GETDATE() 
                                    WHERE MarkId = @MarkId";

                                using (SqlCommand updateCmd = new SqlCommand(updateSql, conn))
                                {
                                    updateCmd.Parameters.AddWithValue("@Marks", marks);
                                    updateCmd.Parameters.AddWithValue("@WeightedMark", weightedMark);
                                    updateCmd.Parameters.AddWithValue("@GradeScaleId", gradeScaleIdObj);
                                    updateCmd.Parameters.AddWithValue("@IsPublished", isAssessmentPublished ? 1 : 0);
                                    updateCmd.Parameters.AddWithValue("@GradedBy", CurrentUserId);
                                    updateCmd.Parameters.AddWithValue("@MarkId", markId);
                                    updateCmd.ExecuteNonQuery();
                                    marksUpdated++;
                                    studentsUpdated.Add(studentId);
                                }
                            }
                            else
                            {
                                string insertSql = @"
                                    INSERT INTO StudentMarks 
                                        (AssessmentId, StudentId, GradeScaleId, MarksObtained, WeightedMark, GradedBy, GradedAt, IsPublished) 
                                    VALUES 
                                        (@AssessmentId, @StudentId, @GradeScaleId, @Marks, @WeightedMark, @GradedBy, GETDATE(), @IsPublished)";

                                using (SqlCommand insertCmd = new SqlCommand(insertSql, conn))
                                {
                                    insertCmd.Parameters.AddWithValue("@AssessmentId", assessmentId);
                                    insertCmd.Parameters.AddWithValue("@StudentId", studentId);
                                    insertCmd.Parameters.AddWithValue("@GradeScaleId", gradeScaleIdObj);
                                    insertCmd.Parameters.AddWithValue("@Marks", marks);
                                    insertCmd.Parameters.AddWithValue("@WeightedMark", weightedMark);
                                    insertCmd.Parameters.AddWithValue("@GradedBy", CurrentUserId);
                                    insertCmd.Parameters.AddWithValue("@IsPublished", isAssessmentPublished ? 1 : 0);
                                    insertCmd.ExecuteNonQuery();
                                    marksSaved++;
                                    studentsUpdated.Add(studentId);
                                }
                            }
                        }
                    }
                }

                if (studentsUpdated.Count > 0)
                {
                    try
                    {
                        var progressService = new AcademicProgressService(connStr);
                        progressService.OnGradesSaved(courseId, year, semester, CurrentLecturerId, studentsUpdated.ToArray());
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"AcademicProgressService trigger failed: {ex.Message}");
                    }
                }

                ShowSuccess($"Marks saved successfully! New: {marksSaved}, Updated: {marksUpdated}");
                LoadAssessmentsForGrading(); // Refresh the view
            }
            catch (Exception ex)
            {
                ShowError("Error saving marks: " + ex.Message);
            }
        }

        protected void btnTogglePublish_Click(object sender, EventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                int assessmentId = int.Parse(btn.CommandArgument);
                bool newPublishStatus = false;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    // Get current publish status
                    string getStatusSql = "SELECT IsPublished FROM Assessments WHERE AssessmentId = @AssessmentId";
                    using (SqlCommand getCmd = new SqlCommand(getStatusSql, conn))
                    {
                        getCmd.Parameters.AddWithValue("@AssessmentId", assessmentId);
                        object result = getCmd.ExecuteScalar();
                        bool currentStatus = result != null && Convert.ToBoolean(result);
                        newPublishStatus = !currentStatus;
                    }

                    // Toggle the assessment publish status
                    string toggleAssessmentSql = "UPDATE Assessments SET IsPublished = @IsPublished WHERE AssessmentId = @AssessmentId";
                    using (SqlCommand cmd = new SqlCommand(toggleAssessmentSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@AssessmentId", assessmentId);
                        cmd.Parameters.AddWithValue("@IsPublished", newPublishStatus);
                        cmd.ExecuteNonQuery();
                    }

                    // Sync marks publish status
                    string syncMarksSql = @"
                        UPDATE StudentMarks 
                        SET IsPublished = (SELECT IsPublished FROM Assessments WHERE AssessmentId = @AssessmentId)
                        WHERE AssessmentId = @AssessmentId";
                    using (SqlCommand syncCmd = new SqlCommand(syncMarksSql, conn))
                    {
                        syncCmd.Parameters.AddWithValue("@AssessmentId", assessmentId);
                        syncCmd.ExecuteNonQuery();
                    }
                }

                string statusText = newPublishStatus ? "published" : "unpublished";
                ShowSuccess($"Assessment has been {statusText} successfully.");
                LoadAssessmentsForGrading(); // Refresh the view
            }
            catch (Exception ex)
            {
                ShowError("Error updating publish status: " + ex.Message);
            }
        }

        public string GetGradeLetter(object marksObj)
        {
            if (marksObj == null || marksObj == DBNull.Value || string.IsNullOrEmpty(marksObj.ToString()))
                return "N/A";

            if (!decimal.TryParse(marksObj.ToString(), out decimal marks))
                return "N/A";

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT TOP 1 GradeLetter FROM GradeScale WHERE @Marks >= MinMark AND @Marks <= MaxMark ORDER BY MinMark DESC", conn))
                    {
                        cmd.Parameters.AddWithValue("@Marks", marks);
                        conn.Open();
                        object result = cmd.ExecuteScalar();
                        return result != null ? result.ToString() : "N/A";
                    }
                }
            }
            catch { return "N/A"; }
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