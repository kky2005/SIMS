using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace SIMS.Services
{
    /// <summary>
    /// AcademicProgressService: Centralized service for tracking and analyzing student academic progress
    /// Links Grades, Attendance, and Student Progress Tracker pages
    /// Manages Academic Warnings and performance metrics
    /// </summary>
    public class AcademicProgressService
    {
        private readonly string _connectionString;

        public AcademicProgressService(string connectionString = null)
        {
            _connectionString = connectionString ?? ConfigurationManager.ConnectionStrings["SIMS_DB"].ConnectionString;
        }

        #region ============ Student Performance Metrics ============

        /// <summary>
        /// Gets comprehensive performance metrics for a single student in a specific course
        /// </summary>
        public StudentPerformanceMetrics GetStudentPerformanceMetrics(int studentId, int courseId, int academicYear, int semester)
        {
            var metrics = new StudentPerformanceMetrics
            {
                StudentId = studentId,
                CourseId = courseId,
                AcademicYear = academicYear,
                Semester = semester
            };

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // FIXED: Attendance joins and calculations are structurally bound to the contextual active EnrolmentId
                // matching the core metrics pipeline inside the Lecturer Dashboard script.
                string sql = @"
                    SELECT
                        s.StudentNo,
                        u.FullName,
                        u.Email,
                        c.CourseCode,
                        c.CourseName,

                        -- FIXED: Attendance Tracking aligned with Dashboard Enrolment calculations
                        ISNULL(att.AttendancePercent, 100.0) AS AttendancePercent,
                        ISNULL(att.TotalSessions, 0) AS TotalSessions,
                        ISNULL(att.SessionsAttended, 0) AS SessionsAttended,

                        -- Assessment Metrics
                        ISNULL(agg.TotalAssessments, 0) AS TotalAssessments,
                        ISNULL(agg.CompletedAssessments, 0) AS CompletedAssessments,
                        ISNULL(agg.AverageMarks, 0.0) AS AverageMarks,
                        ISNULL(agg.MaxMarks, 0.0) AS MaxMarks,

                        -- Submission Metrics
                        ISNULL(sub.SubmittedAssignments, 0) AS SubmittedAssignments,
                        ISNULL(sub.LateSubmissions, 0) AS LateSubmissions,

                        -- Dynamic GPA Calculation (Aligned with Dashboard performance logic)
                        ISNULL((
                            SELECT TOP 1 gs.GradePoint 
                            FROM GradeScale gs 
                            WHERE ISNULL((
                                SELECT 100.0 * SUM(sm2.MarksObtained) / NULLIF(SUM(a2.MaxMark), 0)
                                FROM StudentMarks sm2
                                INNER JOIN Assessments a2 ON sm2.AssessmentId = a2.AssessmentId
                                WHERE sm2.StudentId = s.StudentId 
                                  AND a2.CourseId = e.CourseId
                                  AND a2.AcademicYear = e.AcademicYear
                                  AND a2.Semester = e.Semester
                            ), 0.00) >= gs.MinMark 
                            AND ISNULL((
                                SELECT 100.0 * SUM(sm2.MarksObtained) / NULLIF(SUM(a2.MaxMark), 0)
                                FROM StudentMarks sm2
                                INNER JOIN Assessments a2 ON sm2.AssessmentId = a2.AssessmentId
                                WHERE sm2.StudentId = s.StudentId 
                                  AND a2.CourseId = e.CourseId
                                  AND a2.AcademicYear = e.AcademicYear
                                  AND a2.Semester = e.Semester
                            ), 0.00) <= gs.MaxMark
                            ORDER BY gs.MinMark DESC
                        ), 0.00) AS CurrentGPA,

                        ISNULL(gpa.CGPA, 0.0) AS CGPA,
                        gpa.CalculatedAt AS LastGPAUpdate

                    FROM Students s
                    INNER JOIN Users u ON u.UserId = s.UserId
                    INNER JOIN Enrolments e ON e.StudentId = s.StudentId
                    INNER JOIN Courses c ON c.CourseId = e.CourseId
                    LEFT JOIN (
                        SELECT 
                            EnrolmentId,
                            100.0 * SUM(CASE WHEN Status = 'Present' THEN 1 ELSE 0 END) / NULLIF(COUNT(*), 0) AS AttendancePercent,
                            COUNT(*) AS TotalSessions,
                            SUM(CASE WHEN Status = 'Present' THEN 1 ELSE 0 END) AS SessionsAttended
                        FROM Attendance
                        GROUP BY EnrolmentId
                    ) att ON att.EnrolmentId = e.EnrolmentId
                    LEFT JOIN (
                        SELECT 
                            sm.StudentId,
                            COUNT(*) AS TotalAssessments,
                            COUNT(CASE WHEN sm.MarksObtained > 0 THEN 1 END) AS CompletedAssessments,
                            AVG(CASE WHEN sm.MarksObtained > 0 THEN sm.MarksObtained ELSE NULL END) AS AverageMarks,
                            SUM(a.MaxMark) AS MaxMarks
                        FROM StudentMarks sm
                        INNER JOIN Assessments a ON a.AssessmentId = sm.AssessmentId
                        WHERE a.CourseId = @CourseId 
                          AND a.AcademicYear = @AcademicYear
                          AND a.Semester = @Semester
                        GROUP BY sm.StudentId
                    ) agg ON agg.StudentId = s.StudentId
                    LEFT JOIN (
                        SELECT 
                            asub.StudentId,
                            COUNT(*) AS SubmittedAssignments,
                            SUM(CASE WHEN DATEDIFF(day, a.DueDate, asub.SubmittedAt) > 0 THEN 1 ELSE 0 END) AS LateSubmissions
                        FROM AssessmentSubmissions asub
                        INNER JOIN Assessments a ON a.AssessmentId = asub.AssessmentId
                        WHERE a.CourseId = @CourseId 
                          AND a.AcademicYear = @AcademicYear
                          AND a.Semester = @Semester
                          AND asub.IsLatest = 1
                        GROUP BY asub.StudentId
                    ) sub ON sub.StudentId = s.StudentId
                    LEFT JOIN (
                        SELECT StudentId, CGPA, CalculatedAt,
                               ROW_NUMBER() OVER (PARTITION BY StudentId ORDER BY CalculatedAt DESC) as rn
                        FROM GPARecords
                    ) gpa ON gpa.StudentId = s.StudentId AND gpa.rn = 1
                    WHERE s.StudentId = @StudentId
                      AND e.CourseId = @CourseId
                      AND e.AcademicYear = @AcademicYear
                      AND e.Semester = @Semester
                      AND e.Status = 'Active'";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@StudentId", studentId);
                    cmd.Parameters.AddWithValue("@CourseId", courseId);
                    cmd.Parameters.AddWithValue("@AcademicYear", academicYear);
                    cmd.Parameters.AddWithValue("@Semester", semester);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            metrics.StudentNo = reader["StudentNo"].ToString();
                            metrics.FullName = reader["FullName"].ToString();
                            metrics.Email = reader["Email"].ToString();
                            metrics.CourseCode = reader["CourseCode"].ToString();
                            metrics.CourseName = reader["CourseName"].ToString();

                            metrics.AttendancePercent = Convert.ToDouble(reader["AttendancePercent"]);
                            metrics.TotalSessions = Convert.ToInt32(reader["TotalSessions"]);
                            metrics.SessionsAttended = Convert.ToInt32(reader["SessionsAttended"]);

                            metrics.TotalAssessments = Convert.ToInt32(reader["TotalAssessments"]);
                            metrics.CompletedAssessments = Convert.ToInt32(reader["CompletedAssessments"]);
                            metrics.AverageMarks = Convert.ToDouble(reader["AverageMarks"]);
                            metrics.MaxMarks = Convert.ToDouble(reader["MaxMarks"]);

                            metrics.SubmittedAssignments = Convert.ToInt32(reader["SubmittedAssignments"]);
                            metrics.LateSubmissions = Convert.ToInt32(reader["LateSubmissions"]);

                            metrics.CurrentGPA = Convert.ToDouble(reader["CurrentGPA"]);
                            metrics.CGPA = Convert.ToDouble(reader["CGPA"]);

                            if (reader["LastGPAUpdate"] != DBNull.Value)
                                metrics.LastGPAUpdate = Convert.ToDateTime(reader["LastGPAUpdate"]);

                            metrics.RiskLevel = CalculateRiskLevel(metrics);
                        }
                    }
                }
            }

            return metrics;
        }

        /// <summary>
        /// Gets batch performance metrics for all students in a course
        /// </summary>
        public DataTable GetCourseStudentMetrics(int courseId, int academicYear, int semester, int lecturerId = 0)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // FIXED: Aggregated AttendancePercent is extracted purely via the matching EnrolmentId grouped framework
                string sql = @"
                    SELECT
                        s.StudentId,
                        s.StudentNo,
                        u.FullName,
                        u.Email,
                        c.CourseCode,

                        ISNULL(att.AttendancePercent, 100.0) AS AttendancePercent,
                        ISNULL(agg.AverageMarks, 0.0) AS AverageMarks,
                        
                        -- Dynamic Current GPA Calculation
                        ISNULL((
                            SELECT TOP 1 gs.GradePoint 
                            FROM GradeScale gs 
                            WHERE ISNULL((
                                SELECT 100.0 * SUM(sm2.MarksObtained) / NULLIF(SUM(a2.MaxMark), 0)
                                FROM StudentMarks sm2
                                INNER JOIN Assessments a2 ON sm2.AssessmentId = a2.AssessmentId
                                WHERE sm2.StudentId = s.StudentId 
                                  AND a2.CourseId = e.CourseId
                                  AND a2.AcademicYear = e.AcademicYear
                                  AND a2.Semester = e.Semester
                            ), 0.00) >= gs.MinMark 
                            AND ISNULL((
                                SELECT 100.0 * SUM(sm2.MarksObtained) / NULLIF(SUM(a2.MaxMark), 0)
                                FROM StudentMarks sm2
                                INNER JOIN Assessments a2 ON sm2.AssessmentId = a2.AssessmentId
                                WHERE sm2.StudentId = s.StudentId 
                                  AND a2.CourseId = e.CourseId
                                  AND a2.AcademicYear = e.AcademicYear
                                  AND a2.Semester = e.Semester
                            ), 0.00) <= gs.MaxMark
                            ORDER BY gs.MinMark DESC
                        ), 0.00) AS CurrentGPA,

                        ISNULL(gpa.CGPA, 0.0) AS CGPA,

                        -- Evaluate Risk Level using dashboard matched variables (att.AttendancePercent < 80)
                        CASE 
                            WHEN ISNULL(att.AttendancePercent, 100.0) < 80.0 OR ISNULL((
                                SELECT TOP 1 gs.GradePoint FROM GradeScale gs WHERE ISNULL((
                                    SELECT 100.0 * SUM(sm2.MarksObtained) / NULLIF(SUM(a2.MaxMark), 0)
                                    FROM StudentMarks sm2 INNER JOIN Assessments a2 ON sm2.AssessmentId = a2.AssessmentId
                                    WHERE sm2.StudentId = s.StudentId AND a2.CourseId = e.CourseId AND a2.AcademicYear = e.AcademicYear AND a2.Semester = e.Semester
                                ), 0.00) >= gs.MinMark AND ISNULL((
                                    SELECT 100.0 * SUM(sm2.MarksObtained) / NULLIF(SUM(a2.MaxMark), 0)
                                    FROM StudentMarks sm2 INNER JOIN Assessments a2 ON sm2.AssessmentId = a2.AssessmentId
                                    WHERE sm2.StudentId = s.StudentId AND a2.CourseId = e.CourseId AND a2.AcademicYear = e.AcademicYear AND a2.Semester = e.Semester
                                ), 0.00) <= gs.MaxMark ORDER BY gs.MinMark DESC
                            ), 0.00) < 2.00 THEN 'High'
                            
                            WHEN ISNULL(att.AttendancePercent, 100.0) < 90.0 OR ISNULL((
                                SELECT TOP 1 gs.GradePoint FROM GradeScale gs WHERE ISNULL((
                                    SELECT 100.0 * SUM(sm2.MarksObtained) / NULLIF(SUM(a2.MaxMark), 0)
                                    FROM StudentMarks sm2 INNER JOIN Assessments a2 ON sm2.AssessmentId = a2.AssessmentId
                                    WHERE sm2.StudentId = s.StudentId AND a2.CourseId = e.CourseId AND a2.AcademicYear = e.AcademicYear AND a2.Semester = e.Semester
                                ), 0.00) >= gs.MinMark AND ISNULL((
                                    SELECT 100.0 * SUM(sm2.MarksObtained) / NULLIF(SUM(a2.MaxMark), 0)
                                    FROM StudentMarks sm2 INNER JOIN Assessments a2 ON sm2.AssessmentId = a2.AssessmentId
                                    WHERE sm2.StudentId = s.StudentId AND a2.CourseId = e.CourseId AND a2.AcademicYear = e.AcademicYear AND a2.Semester = e.Semester
                                ), 0.00) <= gs.MaxMark ORDER BY gs.MinMark DESC
                            ), 0.00) < 2.75 THEN 'Medium'
                            ELSE 'Low'
                        END AS RiskLevel,

                        ISNULL(sub.SubmittedAssignments, 0) AS SubmittedAssignments,
                        ISNULL(sub.LateSubmissions, 0) AS LateSubmissions
                    FROM Enrolments e
                    INNER JOIN Students s ON s.StudentId = e.StudentId
                    INNER JOIN Users u ON u.UserId = s.UserId
                    INNER JOIN Courses c ON c.CourseId = e.CourseId
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
                            AVG(CASE WHEN sm.MarksObtained > 0 THEN sm.MarksObtained ELSE NULL END) AS AverageMarks
                        FROM StudentMarks sm
                        INNER JOIN Assessments a ON a.AssessmentId = sm.AssessmentId
                        WHERE a.CourseId = @CourseId 
                          AND a.AcademicYear = @AcademicYear
                          AND a.Semester = @Semester
                        GROUP BY sm.StudentId
                    ) agg ON agg.StudentId = s.StudentId
                    LEFT JOIN (
                        SELECT StudentId, CGPA,
                               ROW_NUMBER() OVER (PARTITION BY StudentId ORDER BY CalculatedAt DESC) as rn
                        FROM GPARecords
                    ) gpa ON gpa.StudentId = s.StudentId AND gpa.rn = 1
                    LEFT JOIN (
                        SELECT 
                            asub.StudentId,
                            COUNT(*) AS SubmittedAssignments,
                            SUM(CASE WHEN DATEDIFF(day, a.DueDate, asub.SubmittedAt) > 0 THEN 1 ELSE 0 END) AS LateSubmissions
                        FROM AssessmentSubmissions asub
                        INNER JOIN Assessments a ON a.AssessmentId = asub.AssessmentId
                        WHERE a.CourseId = @CourseId 
                          AND a.AcademicYear = @AcademicYear
                          AND a.Semester = @Semester
                          AND asub.IsLatest = 1
                        GROUP BY asub.StudentId
                    ) sub ON sub.StudentId = s.StudentId
                    WHERE e.CourseId = @CourseId
                      AND e.AcademicYear = @AcademicYear
                      AND e.Semester = @Semester
                      AND e.Status = 'Active'";

                if (lecturerId > 0)
                {
                    sql += @" AND EXISTS (
                        SELECT 1 FROM CourseAssignments ca
                        WHERE ca.CourseId = e.CourseId 
                          AND ca.AcademicYear = e.AcademicYear
                          AND ca.Semester = e.Semester
                          AND ca.LecturerId = @LecturerId
                    )";
                }

                sql += " ORDER BY s.StudentNo ASC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CourseId", courseId);
                    cmd.Parameters.AddWithValue("@AcademicYear", academicYear);
                    cmd.Parameters.AddWithValue("@Semester", semester);
                    if (lecturerId > 0)
                        cmd.Parameters.AddWithValue("@LecturerId", lecturerId);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        #endregion

        #region ============ Risk Level Analysis ============

        public RiskLevel CalculateRiskLevel(StudentPerformanceMetrics metrics)
        {
            int riskScore = 0;

            if (metrics.AttendancePercent < 60)
                riskScore += 40;
            else if (metrics.AttendancePercent < 75)
                riskScore += 25;
            else if (metrics.AttendancePercent < 80)
                riskScore += 10;

            if (metrics.AverageMarks < 40)
                riskScore += 30;
            else if (metrics.AverageMarks < 50)
                riskScore += 20;
            else if (metrics.AverageMarks < 60)
                riskScore += 10;

            if (metrics.CurrentGPA < 1.5)
                riskScore += 20;
            else if (metrics.CurrentGPA < 2.0)
                riskScore += 12;
            else if (metrics.CurrentGPA < 2.5)
                riskScore += 5;

            if (metrics.SubmittedAssignments > 0 && metrics.LateSubmissions > metrics.SubmittedAssignments * 0.5)
                riskScore += 10;

            if (riskScore >= 70)
                return RiskLevel.High;
            else if (riskScore >= 40)
                return RiskLevel.Medium;
            else
                return RiskLevel.Low;
        }

        #endregion

        #region ============ Academic Warnings Management ============

        public int CreateAcademicWarning(int studentId, int courseId, string warningType, string reason, string severity, int issuedBy)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    INSERT INTO AcademicWarnings (StudentId, CourseId, WarningType, Reason, Severity, Status, IssuedBy, IssuedAt)
                    OUTPUT INSERTED.WarningId
                    VALUES (@StudentId, @CourseId, @WarningType, @Reason, @Severity, 'Active', @IssuedBy, SYSUTCDATETIME())";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@StudentId", studentId);
                    cmd.Parameters.AddWithValue("@CourseId", courseId);
                    cmd.Parameters.AddWithValue("@WarningType", warningType);
                    cmd.Parameters.AddWithValue("@Reason", reason ?? "");
                    cmd.Parameters.AddWithValue("@Severity", severity);
                    cmd.Parameters.AddWithValue("@IssuedBy", issuedBy);

                    conn.Open();
                    int warningId = (int)cmd.ExecuteScalar();
                    return warningId;
                }
            }
        }

        public DataTable GetActiveWarnings(int studentId, int courseId = 0)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT WarningId, StudentId, CourseId, WarningType, Reason, Severity, Status, IssuedBy, IssuedAt
                    FROM AcademicWarnings WHERE StudentId = @StudentId AND Status = 'Active'";

                if (courseId > 0)
                    sql += " AND CourseId = @CourseId";

                sql += " ORDER BY IssuedAt DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@StudentId", studentId);
                    if (courseId > 0)
                        cmd.Parameters.AddWithValue("@CourseId", courseId);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        public List<int> TriggerAutomaticWarnings(int studentId, int courseId, int academicYear, int semester, int lecturerId)
        {
            var warningsCreated = new List<int>();
            var metrics = GetStudentPerformanceMetrics(studentId, courseId, academicYear, semester);
            var existingWarnings = GetActiveWarnings(studentId, courseId);

            if (metrics.AttendancePercent < 80 && !HasWarningOfType(existingWarnings, "Low Attendance"))
            {
                int warningId = CreateAcademicWarning(
                    studentId, courseId, "Low Attendance",
                    $"Attendance percentage ({metrics.AttendancePercent:F1}%) is below Dashboard tracking threshold.",
                    "High", lecturerId
                );
                warningsCreated.Add(warningId);
            }

            if (metrics.AverageMarks < 50 && metrics.CompletedAssessments > 0 && !HasWarningOfType(existingWarnings, "Low Academic Performance"))
            {
                int warningId = CreateAcademicWarning(
                    studentId, courseId, "Low Academic Performance",
                    $"Average marks ({metrics.AverageMarks:F2}) are below 50.",
                    "High", lecturerId
                );
                warningsCreated.Add(warningId);
            }

            if (metrics.CurrentGPA < 2.0 && !HasWarningOfType(existingWarnings, "Low GPA"))
            {
                int warningId = CreateAcademicWarning(
                    studentId, courseId, "Low GPA",
                    $"Current Course GPA ({metrics.CurrentGPA:F2}) is below 2.0 threshold.",
                    "Medium", lecturerId
                );
                warningsCreated.Add(warningId);
            }

            return warningsCreated;
        }

        private bool HasWarningOfType(DataTable warnings, string warningType)
        {
            foreach (DataRow row in warnings.Rows)
            {
                if (row["WarningType"].ToString() == warningType)
                    return true;
            }
            return false;
        }

        public void ResolveWarning(int warningId, string resolutionNotes = null)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    UPDATE AcademicWarnings SET Status = 'Resolved', ResolutionNotes = @ResolutionNotes, ResolvedAt = SYSUTCDATETIME()
                    WHERE WarningId = @WarningId";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@WarningId", warningId);
                    cmd.Parameters.AddWithValue("@ResolutionNotes", resolutionNotes ?? "");
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        #endregion

        #region ============ Trigger Points from Grades & Attendance ============

        public List<int> OnGradesSaved(int courseId, int academicYear, int semester, int lecturerId, int[] studentIds = null)
        {
            var warningsCreated = new List<int>();
            try
            {
                if (studentIds == null || studentIds.Length == 0)
                    studentIds = GetCourseStudentIds(courseId, academicYear, semester);

                foreach (int studentId in studentIds)
                {
                    var warnings = TriggerAutomaticWarnings(studentId, courseId, academicYear, semester, lecturerId);
                    warningsCreated.AddRange(warnings);
                }
            }
            catch { }
            return warningsCreated;
        }

        public List<int> OnAttendanceRecorded(int courseId, int academicYear, int semester, int lecturerId, int[] studentIds = null)
        {
            var warningsCreated = new List<int>();
            try
            {
                if (studentIds == null || studentIds.Length == 0)
                    studentIds = GetCourseStudentIds(courseId, academicYear, semester);

                foreach (int studentId in studentIds)
                {
                    var warnings = TriggerAutomaticWarnings(studentId, courseId, academicYear, semester, lecturerId);
                    warningsCreated.AddRange(warnings);
                }
            }
            catch { }
            return warningsCreated;
        }

        private int[] GetCourseStudentIds(int courseId, int academicYear, int semester)
        {
            var studentIds = new List<int>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT DISTINCT s.StudentId FROM Enrolments e
                    INNER JOIN Students s ON s.StudentId = e.StudentId
                    WHERE e.CourseId = @CourseId AND e.AcademicYear = @AcademicYear AND e.Semester = @Semester AND e.Status = 'Active'";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CourseId", courseId);
                    cmd.Parameters.AddWithValue("@AcademicYear", academicYear);
                    cmd.Parameters.AddWithValue("@Semester", semester);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            studentIds.Add(Convert.ToInt32(reader["StudentId"]));
                        }
                    }
                }
            }
            return studentIds.ToArray();
        }

        #endregion

        #region ============ Reporting & Analytics ============

        public CourseSummaryMetrics GetCourseSummary(int courseId, int academicYear, int semester)
        {
            var summary = new CourseSummaryMetrics
            {
                CourseId = courseId,
                AcademicYear = academicYear,
                Semester = semester
            };

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT
                        COUNT(DISTINCT e.StudentId) AS TotalStudents,
                        AVG(ISNULL(att.AttendancePercent, 100.0)) AS AvgAttendance,
                        AVG(ISNULL(agg.AverageMarks, 0.0)) AS AvgMarks,
                        
                        SUM(CASE WHEN ISNULL(att.AttendancePercent, 100.0) < 80.0 OR ISNULL((
                            SELECT TOP 1 gs.GradePoint FROM GradeScale gs WHERE ISNULL((
                                SELECT 100.0 * SUM(sm2.MarksObtained) / NULLIF(SUM(a2.MaxMark), 0)
                                FROM StudentMarks sm2 INNER JOIN Assessments a2 ON sm2.AssessmentId = a2.AssessmentId
                                WHERE sm2.StudentId = e.StudentId AND a2.CourseId = e.CourseId AND a2.AcademicYear = e.AcademicYear AND a2.Semester = e.Semester
                            ), 0.00) >= gs.MinMark AND ISNULL((
                                SELECT 100.0 * SUM(sm2.MarksObtained) / NULLIF(SUM(a2.MaxMark), 0)
                                FROM StudentMarks sm2 INNER JOIN Assessments a2 ON sm2.AssessmentId = a2.AssessmentId
                                WHERE sm2.StudentId = e.StudentId AND a2.CourseId = e.CourseId AND a2.AcademicYear = e.AcademicYear AND a2.Semester = e.Semester
                            ), 0.00) <= gs.MaxMark ORDER BY gs.MinMark DESC
                        ), 0.00) < 2.00 THEN 1 ELSE 0 END) AS HighRiskCount,

                        SUM(CASE WHEN (ISNULL(att.AttendancePercent, 100.0) >= 80.0 AND ISNULL(att.AttendancePercent, 100.0) < 90.0) OR (ISNULL((
                            SELECT TOP 1 gs.GradePoint FROM GradeScale gs WHERE ISNULL((
                                SELECT 100.0 * SUM(sm2.MarksObtained) / NULLIF(SUM(a2.MaxMark), 0)
                                FROM StudentMarks sm2 INNER JOIN Assessments a2 ON sm2.AssessmentId = a2.AssessmentId
                                WHERE sm2.StudentId = e.StudentId AND a2.CourseId = e.CourseId AND a2.AcademicYear = e.AcademicYear AND a2.Semester = e.Semester
                            ), 0.00) >= gs.MinMark AND ISNULL((
                                SELECT 100.0 * SUM(sm2.MarksObtained) / NULLIF(SUM(a2.MaxMark), 0)
                                FROM StudentMarks sm2 INNER JOIN Assessments a2 ON sm2.AssessmentId = a2.AssessmentId
                                WHERE sm2.StudentId = e.StudentId AND a2.CourseId = e.CourseId AND a2.AcademicYear = e.AcademicYear AND a2.Semester = e.Semester
                            ), 0.00) <= gs.MaxMark ORDER BY gs.MinMark DESC
                        ), 0.00) >= 2.00 AND ISNULL((
                            SELECT TOP 1 gs.GradePoint FROM GradeScale gs WHERE ISNULL((
                                SELECT 100.0 * SUM(sm2.MarksObtained) / NULLIF(SUM(a2.MaxMark), 0)
                                FROM StudentMarks sm2 INNER JOIN Assessments a2 ON sm2.AssessmentId = a2.AssessmentId
                                WHERE sm2.StudentId = e.StudentId AND a2.CourseId = e.CourseId AND a2.AcademicYear = e.AcademicYear AND a2.Semester = e.Semester
                            ), 0.00) >= gs.MinMark AND ISNULL((
                                SELECT 100.0 * SUM(sm2.MarksObtained) / NULLIF(SUM(a2.MaxMark), 0)
                                FROM StudentMarks sm2 INNER JOIN Assessments a2 ON sm2.AssessmentId = a2.AssessmentId
                                WHERE sm2.StudentId = e.StudentId AND a2.CourseId = e.CourseId AND a2.AcademicYear = e.AcademicYear AND a2.Semester = e.Semester
                            ), 0.00) <= gs.MaxMark ORDER BY gs.MinMark DESC
                        ), 0.00) < 2.75) THEN 1 ELSE 0 END) AS MediumRiskCount

                    FROM Enrolments e
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
                            AVG(CASE WHEN sm.MarksObtained > 0 THEN sm.MarksObtained ELSE NULL END) AS AverageMarks
                        FROM StudentMarks sm
                        INNER JOIN Assessments a ON a.AssessmentId = sm.AssessmentId
                        WHERE a.CourseId = @CourseId AND a.AcademicYear = @AcademicYear AND a.Semester = @Semester
                        GROUP BY sm.StudentId
                    ) agg ON agg.StudentId = e.StudentId
                    WHERE e.CourseId = @CourseId AND e.AcademicYear = @AcademicYear AND e.Semester = @Semester AND e.Status = 'Active'";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CourseId", courseId);
                    cmd.Parameters.AddWithValue("@AcademicYear", academicYear);
                    cmd.Parameters.AddWithValue("@Semester", semester);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            summary.TotalStudents = Convert.ToInt32(reader["TotalStudents"]);
                            summary.AverageAttendance = Convert.ToDouble(reader["AvgAttendance"]);
                            summary.AverageMarks = Convert.ToDouble(reader["AvgMarks"]);
                            summary.HighRiskStudents = Convert.ToInt32(reader["HighRiskCount"]);
                            summary.MediumRiskStudents = Convert.ToInt32(reader["MediumRiskCount"]);
                        }
                    }
                }
            }

            return summary;
        }

        #endregion
    }

    #region ============ Supporting Models ============

    public class StudentPerformanceMetrics
    {
        public int StudentId { get; set; }
        public string StudentNo { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public int AcademicYear { get; set; }
        public int Semester { get; set; }

        public double AttendancePercent { get; set; }
        public int TotalSessions { get; set; }
        public int SessionsAttended { get; set; }

        public int TotalAssessments { get; set; }
        public int CompletedAssessments { get; set; }
        public double AverageMarks { get; set; }
        public double MaxMarks { get; set; }

        public int SubmittedAssignments { get; set; }
        public int LateSubmissions { get; set; }

        public double CurrentGPA { get; set; }
        public double CGPA { get; set; }
        public DateTime? LastGPAUpdate { get; set; }

        public RiskLevel RiskLevel { get; set; }
    }

    public class CourseSummaryMetrics
    {
        public int CourseId { get; set; }
        public int AcademicYear { get; set; }
        public int Semester { get; set; }
        public int TotalStudents { get; set; }
        public double AverageAttendance { get; set; }
        public double AverageMarks { get; set; }
        public int HighRiskStudents { get; set; }
        public int MediumRiskStudents { get; set; }
    }

    public enum RiskLevel
    {
        Low,
        Medium,
        High
    }

    #endregion
}