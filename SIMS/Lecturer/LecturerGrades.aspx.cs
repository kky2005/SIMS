using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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

                if (!LecturerTeachesCourse(courseId))
                {
                    Response.Redirect("LecturerCourses.aspx");
                    return;
                }

                hidCourseId.Value = courseId.ToString();
                LoadCourseInfo(courseId);

                var assignment = GetMostRecentAssignment(courseId);
                int academicYear = assignment.year > 0 ? assignment.year : DateTime.Now.Year;
                int semester = assignment.semester > 0 ? assignment.semester : GetCurrentSemester();

                hidAcademicYear.Value = academicYear.ToString();
                hidSemester.Value = semester.ToString();

                litAcademicYear.Text = academicYear.ToString();
                litSemester.Text = $"Semester {semester}";

                // Load data directly without requiring button click
                LoadAssessmentsForGrading();
                LoadCourseSummaryMatrix();
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

        private int GetCurrentSemester()
        {
            int m = DateTime.Now.Month;
            if (m <= 4) return 1;
            if (m <= 8) return 2;
            return 3;
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

        private void LoadAssessmentsForGrading()
        {
            int courseId = int.Parse(hidCourseId.Value);
            int year = int.Parse(hidAcademicYear.Value);
            int semester = int.Parse(hidSemester.Value);

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
            int year = int.Parse(hidAcademicYear.Value);
            int semester = int.Parse(hidSemester.Value);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"
                    SELECT
                        s.StudentId,
                        s.StudentNo,
                        u.FullName,
                        u.Email,
                        a.MaxMark,
                        sm.MarksObtained
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

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@AssessmentId", assessmentId);
                    cmd.Parameters.AddWithValue("@CourseId", courseId);
                    cmd.Parameters.AddWithValue("@Year", year);
                    cmd.Parameters.AddWithValue("@Semester", semester);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (!dt.Columns.Contains("MaxMark"))
                    {
                        dt.Columns.Add("MaxMark", typeof(decimal));
                        foreach (DataRow row in dt.Rows)
                        {
                            row["MaxMark"] = 100;
                        }
                    }

                    rpt.DataSource = dt;
                    rpt.DataBind();
                }
            }
        }

        private void LoadCourseSummaryMatrix()
        {
            int courseId = int.Parse(hidCourseId.Value);
            int year = int.Parse(hidAcademicYear.Value);
            int semester = int.Parse(hidSemester.Value);

            StringBuilder sb = new StringBuilder();

            // 1. Load active evaluation checkpoints for the chosen syllabus criteria
            DataTable dtAssessments = new DataTable();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sqlAssessments = @"
                    SELECT AssessmentId, AssessmentName, MaxMark, Weightage 
                    FROM Assessments 
                    WHERE CourseId = @CourseId AND AcademicYear = @Year AND Semester = @Semester 
                    ORDER BY AssessmentName ASC";
                using (SqlCommand cmd = new SqlCommand(sqlAssessments, conn))
                {
                    cmd.Parameters.AddWithValue("@CourseId", courseId);
                    cmd.Parameters.AddWithValue("@Year", year);
                    cmd.Parameters.AddWithValue("@Semester", semester);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dtAssessments);
                }
            }

            // 2. Load all matching student records and marks for processing
            DataTable dtMarksData = new DataTable();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sqlMarks = @"
                    SELECT 
                        s.StudentId,
                        s.StudentNo,
                        u.FullName,
                        sm.AssessmentId,
                        sm.MarksObtained
                    FROM Enrolments e
                    INNER JOIN Students s ON e.StudentId = s.StudentId
                    INNER JOIN Users u ON s.UserId = u.UserId
                    LEFT JOIN StudentMarks sm ON sm.StudentId = s.StudentId 
                        AND sm.AssessmentId IN (
                            SELECT AssessmentId FROM Assessments 
                            WHERE CourseId = @CourseId AND AcademicYear = @Year AND Semester = @Semester
                        )
                    WHERE e.CourseId = @CourseId 
                      AND e.AcademicYear = @Year 
                      AND e.Semester = @Semester 
                      AND e.Status = 'Active'
                    ORDER BY s.StudentNo ASC";
                using (SqlCommand cmd = new SqlCommand(sqlMarks, conn))
                {
                    cmd.Parameters.AddWithValue("@CourseId", courseId);
                    cmd.Parameters.AddWithValue("@Year", year);
                    cmd.Parameters.AddWithValue("@Semester", semester);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dtMarksData);
                }
            }

            if (dtMarksData.Rows.Count == 0)
            {
                litSummaryContainer.Text = "<div class='no-data'><i class='fa fa-users'></i><p>No active enrolled students found for the current course configuration filters.</p></div>";
                return;
            }

            // 3. Assemble dynamic HTML spreadsheet view model mapping output elements
            sb.Append("<div class='grades-table-wrapper'>");
            sb.Append("<table class='table-sims'>");
            sb.Append("<thead><tr>");
            sb.Append("<th>Student No</th>");
            sb.Append("<th>Student Full Name</th>");

            foreach (DataRow assRow in dtAssessments.Rows)
            {
                sb.AppendFormat("<th>{0}<br/><small style='color:#64748b;'>Max: {1} ({2}%)</small></th>",
                    Server.HtmlEncode(assRow["AssessmentName"].ToString()),
                    Convert.ToDecimal(assRow["MaxMark"]).ToString("G29"),
                    Convert.ToDecimal(assRow["Weightage"]).ToString("G29"));
            }

            // Explicitly embed the mathematical formula algorithm representation beside the Total Mark header
            sb.Append("<th>Total Marks <span class='formula-box' title='Total Marks Calculation Rules Engine'>Algorithm: (&Sigma; Marks Obtained / &Sigma; Max Mark) &times; 100</span></th>");
            sb.Append("<th>GPA</th>");
            sb.Append("<th>Final Grade</th>");

            sb.Append("</tr></thead>");
            sb.Append("<tbody>");

            // Perform pivot rendering grouping logic in memory using LINQ
            var studentGroups = from DataRow r in dtMarksData.Rows
                                group r by new
                                {
                                    StudentId = Convert.ToInt32(r["StudentId"]),
                                    StudentNo = r["StudentNo"].ToString(),
                                    FullName = r["FullName"].ToString()
                                } into g
                                select g;

            foreach (var student in studentGroups)
            {
                sb.Append("<tr>");
                sb.AppendFormat("<td>{0}</td>", Server.HtmlEncode(student.Key.StudentNo));
                sb.AppendFormat("<td class='student-name'>{0}</td>", Server.HtmlEncode(student.Key.FullName));

                decimal totalMarksObtained = 0;
                decimal totalMaxMarks = 0;

                foreach (DataRow assRow in dtAssessments.Rows)
                {
                    int currentAssId = Convert.ToInt32(assRow["AssessmentId"]);
                    decimal maxMark = Convert.ToDecimal(assRow["MaxMark"]);

                    var markRow = student.FirstOrDefault(r => r["AssessmentId"] != DBNull.Value && Convert.ToInt32(r["AssessmentId"]) == currentAssId);
                    if (markRow != null && markRow["MarksObtained"] != DBNull.Value)
                    {
                        decimal marksObtained = Convert.ToDecimal(markRow["MarksObtained"]);
                        sb.AppendFormat("<td>{0}</td>", marksObtained.ToString("F2"));

                        totalMarksObtained += marksObtained;
                        totalMaxMarks += maxMark;
                    }
                    else
                    {
                        sb.Append("<td style='color: #cbd5e1;'>N/A</td>");
                    }
                }

                // Compute percentage based on the sum of obtained / sum of max marks
                decimal aggregateCourseScore = 0;
                if (totalMaxMarks > 0)
                {
                    aggregateCourseScore = (totalMarksObtained / totalMaxMarks) * 100;
                }

                // Process the cumulative score output through the contextual grade database rules mapper
                string evaluatedLetter = GetGradeLetter(aggregateCourseScore);
                string evaluatedGPA = GetGradePoint(aggregateCourseScore);
                string gradeBadgeClass = "grade-n-a";
                if (!string.IsNullOrEmpty(evaluatedLetter) && evaluatedLetter != "N/A")
                {
                    gradeBadgeClass = "grade-" + evaluatedLetter.Substring(0, 1).ToLower();
                }

                sb.AppendFormat("<td style='font-weight: bold; color: #047857;'>{0} / 100.00</td>", aggregateCourseScore.ToString("F2"));
                sb.AppendFormat("<td>{0}</td>", evaluatedGPA);
                sb.AppendFormat("<td><span class='grade-badge {0}'>{1}</span></td>", gradeBadgeClass, evaluatedLetter);
                sb.Append("</tr>");
            }

            sb.Append("</tbody></table></div>");
            litSummaryContainer.Text = sb.ToString();
        }

        public string GetGradePoint(decimal marks)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT TOP 1 GradePoint FROM GradeScale WHERE @Marks >= MinMark AND @Marks <= MaxMark ORDER BY MinMark DESC", conn))
                    {
                        cmd.Parameters.AddWithValue("@Marks", marks);
                        conn.Open();
                        object result = cmd.ExecuteScalar();
                        return result != null ? Convert.ToDecimal(result).ToString("F2") : "0.00";
                    }
                }
            }
            catch { return "0.00"; }
        }

        protected void btnSaveAllMarks_Click(object sender, EventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                int assessmentId = int.Parse(btn.CommandArgument);
                int courseId = int.Parse(hidCourseId.Value);
                int year = int.Parse(hidAcademicYear.Value);
                int semester = int.Parse(hidSemester.Value);

                int marksSaved = 0;
                int marksUpdated = 0;

                decimal maxMark = 100;
                decimal weightage = 0;
                bool isAssessmentPublished = false;

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
                        if (marks > maxMark)
                        {
                            ShowError($"Mark {marks} exceeds maximum mark {maxMark} for a student. Please correct and try again.");
                            return;
                        }

                        decimal weightedMark = maxMark > 0 ? (marks / maxMark) * weightage : 0;
                        decimal percentageScore = maxMark > 0 ? (marks / maxMark) * 100 : 0;

                        object gradeScaleIdObj = DBNull.Value;

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

                // Refresh full layout dashboards on completion
                LoadAssessmentsForGrading();
                LoadCourseSummaryMatrix();
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

                    string getStatusSql = "SELECT IsPublished FROM Assessments WHERE AssessmentId = @AssessmentId";
                    using (SqlCommand getCmd = new SqlCommand(getStatusSql, conn))
                    {
                        getCmd.Parameters.AddWithValue("@AssessmentId", assessmentId);
                        object result = getCmd.ExecuteScalar();
                        bool currentStatus = result != null && Convert.ToBoolean(result);
                        newPublishStatus = !currentStatus;
                    }

                    string toggleAssessmentSql = "UPDATE Assessments SET IsPublished = @IsPublished WHERE AssessmentId = @AssessmentId";
                    using (SqlCommand cmd = new SqlCommand(toggleAssessmentSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@AssessmentId", assessmentId);
                        cmd.Parameters.AddWithValue("@IsPublished", newPublishStatus);
                        cmd.ExecuteNonQuery();
                    }

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

                // Refresh full layout dashboards on completion
                LoadAssessmentsForGrading();
                LoadCourseSummaryMatrix();
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