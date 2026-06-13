using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SIMS.DAL
{
    public class StudentMarksDetailDAL
    {
        private string connStr = ConfigurationManager.ConnectionStrings["SIMS_DB"].ConnectionString;

        public DataTable GetSemesterMarksDetails(int studentId, int academicYear, int semester)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT
                        c.CourseId,
                        c.CourseCode,
                        c.CourseName,
                        c.CreditHours,
                        a.AssessmentName,
                        a.MaxMark,
                        a.Weightage,
                        sm.MarksObtained,
                        sm.WeightedMark,
                        gs.GradeLetter,
                        gs.GradePoint,
                        sm.IsPublished,
                        a.IsPublished AS AssessmentPublished,
                        sm.GradedAt
                    FROM Enrolments e
                    INNER JOIN Courses c
                        ON e.CourseId = c.CourseId
                    INNER JOIN Assessments a
                        ON c.CourseId = a.CourseId
                       AND e.AcademicYear = a.AcademicYear
                       AND e.Semester = a.Semester
                    LEFT JOIN StudentMarks sm
                        ON a.AssessmentId = sm.AssessmentId
                       AND sm.StudentId = e.StudentId
                    LEFT JOIN GradeScale gs
                        ON sm.GradeScaleId = gs.GradeScaleId
                    WHERE e.StudentId = @StudentId
                      AND e.AcademicYear = @AcademicYear
                      AND e.Semester = @Semester
                      AND e.Status IN ('Active', 'Completed')
                    ORDER BY c.CourseCode, a.AssessmentId";

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

        public DataTable GetCourseTotals(int studentId, int academicYear, int semester)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
            ;WITH CourseTotalMarks AS
            (
                SELECT
                    c.CourseId,
                    c.CourseCode,
                    c.CourseName,
                    c.CreditHours,
                    CAST(SUM(ISNULL(sm.WeightedMark, 0)) AS DECIMAL(6,2)) AS TotalMark,
                    COUNT(a.AssessmentId) AS TotalAssessments,
                    COUNT(sm.MarkId) AS PublishedMarks
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
                   AND sm.StudentId = e.StudentId
                   AND sm.IsPublished = 1
                WHERE e.StudentId = @StudentId
                  AND e.AcademicYear = @AcademicYear
                  AND e.Semester = @Semester
                  AND e.Status IN ('Active', 'Completed')
                GROUP BY
                    c.CourseId,
                    c.CourseCode,
                    c.CourseName,
                    c.CreditHours
            )
            SELECT
                ctm.CourseId,
                ctm.CourseCode,
                ctm.CourseName,
                ctm.CreditHours,
                ctm.TotalMark,
                gs.GradeLetter,
                gs.GradePoint,
                CASE
                    WHEN ctm.TotalAssessments = ctm.PublishedMarks
                         AND ctm.TotalAssessments > 0
                    THEN 'Published'
                    ELSE 'In Progress'
                END AS ResultStatus
            FROM CourseTotalMarks ctm
            OUTER APPLY
            (
                SELECT TOP 1
                    GradeLetter,
                    GradePoint
                FROM GradeScale
                WHERE ctm.TotalMark BETWEEN MinMark AND MaxMark
                ORDER BY MinMark DESC
            ) gs
            ORDER BY ctm.CourseCode";

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

        public DataTable GetSemesterInfo(int studentId, int academicYear, int semester)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT TOP 1
                        u.FullName,
                        s.StudentNo,
                        e.AcademicYear,
                        e.Semester
                    FROM Enrolments e
                    INNER JOIN Students s
                        ON e.StudentId = s.StudentId
                    INNER JOIN Users u
                        ON s.UserId = u.UserId
                    WHERE e.StudentId = @StudentId
                      AND e.AcademicYear = @AcademicYear
                      AND e.Semester = @Semester";

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