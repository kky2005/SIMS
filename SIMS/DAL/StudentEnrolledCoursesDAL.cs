using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SIMS.DAL
{
    public class StudentEnrolledCourseDAL
    {
        private string connStr = ConfigurationManager.ConnectionStrings["SIMS_DB"].ConnectionString;

        public DataTable GetCurrentEnrolledCourses(int studentId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT
                        e.EnrolmentId,
                        c.CourseId,
                        c.CourseCode,
                        c.CourseName,
                        c.CreditHours,
                        e.AcademicYear,
                        e.Semester,
                        e.Status
                    FROM Enrolments e
                    INNER JOIN Courses c 
                        ON e.CourseId = c.CourseId
                    WHERE e.StudentId = @StudentId
                      AND e.Status = 'Active'
                    ORDER BY c.CourseCode";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StudentId", studentId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        public DataTable GetCourseDetails(int studentId, int courseId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT
                        c.CourseId,
                        c.CourseCode,
                        c.CourseName,
                        c.CreditHours,
                        e.AcademicYear,
                        e.Semester,
                        e.Status
                    FROM Enrolments e
                    INNER JOIN Courses c 
                        ON e.CourseId = c.CourseId
                    WHERE e.StudentId = @StudentId
                      AND e.CourseId = @CourseId
                      AND e.Status = 'Active'";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StudentId", studentId);
                cmd.Parameters.AddWithValue("@CourseId", courseId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        public DataTable GetCourseMaterials(int studentId, int courseId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT
                        cm.MaterialId,
                        cm.CourseId,
                        cm.Title,
                        cm.Description,
                        cm.FileUrl,
                        cm.FileType,
                        cm.FileSizeKB,
                        cm.AcademicYear,
                        cm.Semester,
                        cm.UploadedAt
                    FROM CourseMaterials cm
                    INNER JOIN Enrolments e
                        ON cm.CourseId = e.CourseId
                       AND cm.AcademicYear = e.AcademicYear
                       AND cm.Semester = e.Semester
                    WHERE e.StudentId = @StudentId
                      AND e.CourseId = @CourseId
                      AND e.Status = 'Active'
                      AND cm.IsVisible = 1
                    ORDER BY cm.UploadedAt DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StudentId", studentId);
                cmd.Parameters.AddWithValue("@CourseId", courseId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }
    }
}