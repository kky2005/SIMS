using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SIMS.DAL
{
    public class StudentCourseDAL
    {
        private string connStr = ConfigurationManager.ConnectionStrings["SIMS_DB"].ConnectionString;

        public int GetCurrentSemester(int studentId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT CurrentSemester
                    FROM Students
                    WHERE StudentId = @StudentId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StudentId", studentId);

                conn.Open();
                object result = cmd.ExecuteScalar();

                if (result != null)
                {
                    return Convert.ToInt32(result);
                }

                return 0;
            }
        }

        public DataTable GetAvailableCourses(int studentId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT 
                        c.CourseId,
                        c.CourseCode,
                        c.CourseName,
                        c.CreditHours,
                        c.Semester
                    FROM Courses c
                    INNER JOIN Students s ON c.ProgrammeId = s.ProgrammeId
                    WHERE s.StudentId = @StudentId
                      AND c.IsActive = 1
                      AND c.Semester = s.CurrentSemester
                      AND c.CourseId NOT IN (
                            SELECT CourseId
                            FROM Enrolments
                            WHERE StudentId = @StudentId
                              AND Status = 'Active'
                      )
                      AND c.CourseId NOT IN (
                            SELECT CourseId
                            FROM CourseRegistrationRequests
                            WHERE StudentId = @StudentId
                              AND RequestType = 'Register'
                              AND Status = 'Pending'
                      )
                    ORDER BY c.CourseCode";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StudentId", studentId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        public DataTable GetEnrolledCourses(int studentId)
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
            INNER JOIN Courses c ON e.CourseId = c.CourseId
            INNER JOIN Students s ON e.StudentId = s.StudentId
            WHERE e.StudentId = @StudentId
              AND e.Status = 'Active'
              AND e.Semester = s.CurrentSemester
            ORDER BY e.AcademicYear DESC, e.Semester DESC, c.CourseCode";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StudentId", studentId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        public DataTable GetCourseRequests(int studentId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT 
                        r.RequestId,
                        c.CourseCode,
                        c.CourseName,
                        r.RequestType,
                        r.Status,
                        r.RequestedAt,
                        ISNULL(r.AdminRemarks, '-') AS AdminRemarks
                    FROM CourseRegistrationRequests r
                    INNER JOIN Courses c ON r.CourseId = c.CourseId
                    WHERE r.StudentId = @StudentId
                    ORDER BY r.RequestedAt DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StudentId", studentId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        public bool HasPendingRequest(int studentId, int courseId, string requestType)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT COUNT(*)
                    FROM CourseRegistrationRequests
                    WHERE StudentId = @StudentId
                      AND CourseId = @CourseId
                      AND RequestType = @RequestType
                      AND Status = 'Pending'";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StudentId", studentId);
                cmd.Parameters.AddWithValue("@CourseId", courseId);
                cmd.Parameters.AddWithValue("@RequestType", requestType);

                conn.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());

                return count > 0;
            }
        }

        public bool IsCurrentSemesterActiveEnrolment(int studentId, int courseId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
            SELECT COUNT(*)
            FROM Enrolments e
            INNER JOIN Students s ON e.StudentId = s.StudentId
            WHERE e.StudentId = @StudentId
              AND e.CourseId = @CourseId
              AND e.Status = 'Active'
              AND e.Semester = s.CurrentSemester";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StudentId", studentId);
                cmd.Parameters.AddWithValue("@CourseId", courseId);

                conn.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());

                return count > 0;
            }
        }
        public bool IsWithinRegistrationPeriod(int studentId, string periodType)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
            SELECT COUNT(*)
            FROM RegistrationPeriods rp
            INNER JOIN Students s ON rp.ProgrammeId = s.ProgrammeId
            WHERE s.StudentId = @StudentId
              AND rp.Semester = s.CurrentSemester
              AND rp.PeriodType = @PeriodType
              AND rp.IsActive = 1
              AND CAST(GETDATE() AS DATE) BETWEEN rp.StartDate AND rp.EndDate";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StudentId", studentId);
                cmd.Parameters.AddWithValue("@PeriodType", periodType);

                conn.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());

                return count > 0;
            }
        }
        public bool InsertCourseRequest(int studentId, int courseId, string requestType)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    INSERT INTO CourseRegistrationRequests
                    (StudentId, CourseId, RequestType, Status, RequestedAt)
                    VALUES
                    (@StudentId, @CourseId, @RequestType, 'Pending', SYSUTCDATETIME())";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StudentId", studentId);
                cmd.Parameters.AddWithValue("@CourseId", courseId);
                cmd.Parameters.AddWithValue("@RequestType", requestType);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();

                return rows > 0;
            }
        }

        public DataTable GetRegistrationPeriods(int studentId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
            SELECT 
                rp.PeriodType,
                CONVERT(VARCHAR(11), rp.StartDate, 106) AS StartDateText,
                CONVERT(VARCHAR(11), rp.EndDate, 106) AS EndDateText,
                CASE 
                    WHEN rp.IsActive = 1 
                     AND CAST(GETDATE() AS DATE) BETWEEN rp.StartDate AND rp.EndDate
                    THEN 'Open'
                    ELSE 'Closed'
                END AS PeriodStatus
            FROM RegistrationPeriods rp
            INNER JOIN Students s ON rp.ProgrammeId = s.ProgrammeId
            WHERE s.StudentId = @StudentId
              AND rp.Semester = s.CurrentSemester
            ORDER BY 
                CASE 
                    WHEN rp.PeriodType = 'Registration' THEN 1
                    WHEN rp.PeriodType = 'Drop' THEN 2
                    ELSE 3
                END";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StudentId", studentId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }
    }
}