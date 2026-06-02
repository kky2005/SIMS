using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SIMS.DAL
{
    public class StudentResultDAL
    {
        private string connStr = ConfigurationManager.ConnectionStrings["SIMS_DB"].ConnectionString;

        public DataTable GetGPASummary(int studentId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT TOP 1
                        AcademicYear,
                        Semester,
                        GPA,
                        CGPA,
                        TotalCreditHours,
                        CalculatedAt
                    FROM GPARecords
                    WHERE StudentId = @StudentId
                    ORDER BY AcademicYear DESC, Semester DESC, CalculatedAt DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StudentId", studentId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        public DataTable GetCourseResults(int studentId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    WITH CourseTotals AS
                    (
                        SELECT
                            e.AcademicYear,
                            e.Semester,
                            c.CourseId,
                            c.CourseCode,
                            c.CourseName,
                            c.CreditHours,
                            e.Status AS EnrolmentStatus,

                            COUNT(a.AssessmentId) AS TotalAssessments,
                            COUNT(sm.MarkId) AS PublishedMarks,
                            CAST(SUM(ISNULL(sm.WeightedMark, 0)) AS DECIMAL(6,2)) AS TotalMark
                        FROM Enrolments e
                        INNER JOIN Courses c 
                            ON e.CourseId = c.CourseId
                        LEFT JOIN Assessments a 
                            ON c.CourseId = a.CourseId
                           AND e.AcademicYear = a.AcademicYear
                           AND e.Semester = a.Semester
                           AND a.IsPublished = 1
                        LEFT JOIN StudentMarks sm 
                            ON a.AssessmentId = sm.AssessmentId
                           AND e.StudentId = sm.StudentId
                           AND sm.IsPublished = 1
                        WHERE e.StudentId = @StudentId
                          AND e.Status IN ('Active', 'Completed')
                        GROUP BY
                            e.AcademicYear,
                            e.Semester,
                            c.CourseId,
                            c.CourseCode,
                            c.CourseName,
                            c.CreditHours,
                            e.Status
                    )
                    SELECT
                        ct.AcademicYear,
                        ct.Semester,
                        ct.CourseCode,
                        ct.CourseName,
                        ct.CreditHours,
                        ct.EnrolmentStatus,

                        CASE
                            WHEN ct.TotalAssessments = 0 THEN '-'
                            WHEN ct.PublishedMarks < ct.TotalAssessments THEN '-'
                            ELSE CAST(ct.TotalMark AS VARCHAR(20))
                        END AS TotalWeightedMark,

                        CASE
                            WHEN ct.TotalAssessments = 0 THEN '-'
                            WHEN ct.PublishedMarks < ct.TotalAssessments THEN '-'
                            ELSE ISNULL(gs.GradeLetter, '-')
                        END AS Grade,

                        CASE
                            WHEN ct.TotalAssessments = 0 THEN '-'
                            WHEN ct.PublishedMarks < ct.TotalAssessments THEN '-'
                            ELSE CAST(gs.GradePoint AS VARCHAR(20))
                        END AS GradePoint,

                        CASE
                            WHEN ct.TotalAssessments = 0 THEN 'In Progress'
                            WHEN ct.PublishedMarks < ct.TotalAssessments THEN 'In Progress'
                            ELSE 'Published'
                        END AS ResultStatus
                    FROM CourseTotals ct
                    OUTER APPLY
                    (
                        SELECT TOP 1
                            GradeLetter,
                            GradePoint
                        FROM GradeScale
                        WHERE ct.TotalMark BETWEEN MinMark AND MaxMark
                        ORDER BY MinMark DESC
                    ) gs
                    ORDER BY ct.AcademicYear DESC, ct.Semester DESC, ct.CourseCode";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StudentId", studentId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        public DataTable GetAssessmentBreakdown(int studentId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT
                        e.AcademicYear,
                        e.Semester,
                        c.CourseCode,
                        c.CourseName,
                        a.AssessmentName,
                        a.MaxMark,
                        a.Weightage,

                        CASE
                            WHEN sm.MarkId IS NULL THEN '-'
                            ELSE CAST(sm.MarksObtained AS VARCHAR(20))
                        END AS MarksObtained,

                        CASE
                            WHEN sm.MarkId IS NULL THEN '-'
                            ELSE CAST(sm.WeightedMark AS VARCHAR(20))
                        END AS WeightedMark,

                        CASE
                            WHEN sm.MarkId IS NULL THEN 'Not Published'
                            ELSE 'Published'
                        END AS MarkStatus
                    FROM Enrolments e
                    INNER JOIN Courses c 
                        ON e.CourseId = c.CourseId
                    INNER JOIN Assessments a 
                        ON c.CourseId = a.CourseId
                       AND e.AcademicYear = a.AcademicYear
                       AND e.Semester = a.Semester
                       AND a.IsPublished = 1
                    LEFT JOIN StudentMarks sm 
                        ON a.AssessmentId = sm.AssessmentId
                       AND e.StudentId = sm.StudentId
                       AND sm.IsPublished = 1
                    WHERE e.StudentId = @StudentId
                      AND e.Status IN ('Active', 'Completed')
                    ORDER BY e.AcademicYear DESC, e.Semester DESC, c.CourseCode, a.AssessmentId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StudentId", studentId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        public DataTable GetResultSemesters(int studentId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
            SELECT DISTINCT
                e.AcademicYear,
                e.Semester
            FROM Enrolments e
            WHERE e.StudentId = @StudentId
              AND e.Status IN ('Active', 'Completed')
            ORDER BY e.AcademicYear, e.Semester";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StudentId", studentId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        public DataTable GetCourseResultsBySemester(int studentId, int academicYear, int semester)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
            WITH CourseTotals AS
            (
                SELECT
                    e.AcademicYear,
                    e.Semester,
                    c.CourseId,
                    c.CourseCode,
                    c.CourseName,
                    c.CreditHours,
                    e.Status AS EnrolmentStatus,

                    COUNT(a.AssessmentId) AS TotalAssessments,
                    COUNT(sm.MarkId) AS PublishedMarks,
                    CAST(SUM(ISNULL(sm.WeightedMark, 0)) AS DECIMAL(6,2)) AS TotalMark
                FROM Enrolments e
                INNER JOIN Courses c 
                    ON e.CourseId = c.CourseId
                LEFT JOIN Assessments a 
                    ON c.CourseId = a.CourseId
                   AND e.AcademicYear = a.AcademicYear
                   AND e.Semester = a.Semester
                   AND a.IsPublished = 1
                LEFT JOIN StudentMarks sm 
                    ON a.AssessmentId = sm.AssessmentId
                   AND e.StudentId = sm.StudentId
                   AND sm.IsPublished = 1
                WHERE e.StudentId = @StudentId
                  AND e.AcademicYear = @AcademicYear
                  AND e.Semester = @Semester
                  AND e.Status IN ('Active', 'Completed')
                GROUP BY
                    e.AcademicYear,
                    e.Semester,
                    c.CourseId,
                    c.CourseCode,
                    c.CourseName,
                    c.CreditHours,
                    e.Status
            )
            SELECT
                ct.AcademicYear,
                ct.Semester,
                ct.CourseCode,
                ct.CourseName,
                ct.CreditHours,
                ct.EnrolmentStatus,

                CASE
                    WHEN ct.TotalAssessments = 0 THEN '-'
                    WHEN ct.PublishedMarks < ct.TotalAssessments THEN '-'
                    ELSE CAST(ct.TotalMark AS VARCHAR(20))
                END AS TotalWeightedMark,

                CASE
                    WHEN ct.TotalAssessments = 0 THEN '-'
                    WHEN ct.PublishedMarks < ct.TotalAssessments THEN '-'
                    ELSE ISNULL(gs.GradeLetter, '-')
                END AS Grade,

                CASE
                    WHEN ct.TotalAssessments = 0 THEN '-'
                    WHEN ct.PublishedMarks < ct.TotalAssessments THEN '-'
                    ELSE CAST(gs.GradePoint AS VARCHAR(20))
                END AS GradePoint,

                CASE
                    WHEN ct.TotalAssessments = 0 THEN 'In Progress'
                    WHEN ct.PublishedMarks < ct.TotalAssessments THEN 'In Progress'
                    ELSE 'Published'
                END AS ResultStatus
            FROM CourseTotals ct
            OUTER APPLY
            (
                SELECT TOP 1
                    GradeLetter,
                    GradePoint
                FROM GradeScale
                WHERE ct.TotalMark BETWEEN MinMark AND MaxMark
                ORDER BY MinMark DESC
            ) gs
            ORDER BY ct.CourseCode";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StudentId", studentId);
                cmd.Parameters.AddWithValue("@AcademicYear", academicYear);
                cmd.Parameters.AddWithValue("@Semester", semester);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        public DataTable GetGPASummaryBySemester(int studentId, int academicYear, int semester)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
            SELECT TOP 1
                AcademicYear,
                Semester,
                GPA,
                CGPA,
                TotalCreditHours,
                CalculatedAt
            FROM GPARecords
            WHERE StudentId = @StudentId
              AND AcademicYear = @AcademicYear
              AND Semester = @Semester
            ORDER BY CalculatedAt DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StudentId", studentId);
                cmd.Parameters.AddWithValue("@AcademicYear", academicYear);
                cmd.Parameters.AddWithValue("@Semester", semester);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }
    }
}