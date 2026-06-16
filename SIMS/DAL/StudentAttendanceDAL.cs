using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SIMS.DAL
{
    public class StudentAttendanceDAL
    {
        private string connStr = ConfigurationManager.ConnectionStrings["SIMS_DB"].ConnectionString;

        public DataTable GetOverallAttendanceStats(int studentId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT
                        COUNT(DISTINCT e.EnrolmentId) AS TotalCourses,
                        COUNT(a.AttendanceId) AS TotalClasses,
                        SUM(CASE WHEN a.Status IN ('Present', 'Late', 'Excused') THEN 1 ELSE 0 END) AS AttendedClasses,
                        SUM(CASE WHEN a.Status = 'Absent' THEN 1 ELSE 0 END) AS AbsentClasses,
                        CAST(
                            CASE 
                                WHEN COUNT(a.AttendanceId) = 0 THEN 0
                                ELSE 
                                    (SUM(CASE WHEN a.Status IN ('Present', 'Late', 'Excused') THEN 1 ELSE 0 END) * 100.0)
                                    / COUNT(a.AttendanceId)
                            END
                        AS DECIMAL(5,2)) AS OverallAttendancePercentage
                    FROM Enrolments e
                    INNER JOIN Courses c 
                        ON e.CourseId = c.CourseId
                    LEFT JOIN Attendance a 
                        ON e.EnrolmentId = a.EnrolmentId
                    WHERE e.StudentId = @StudentId
                      AND e.Status IN ('Active', 'Completed')";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StudentId", studentId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        public DataTable GetAttendanceCourseFilter(int studentId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
            SELECT
                e.EnrolmentId,
                c.CourseCode + ' - ' + c.CourseName + ' (Sem ' + CAST(e.Semester AS NVARCHAR) + ')' AS CourseDisplay
            FROM Enrolments e
            INNER JOIN Courses c
                ON e.CourseId = c.CourseId
            WHERE e.StudentId = @StudentId
              AND e.Status IN ('Active', 'Completed')
            ORDER BY e.AcademicYear, e.Semester, c.CourseCode";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StudentId", studentId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        public DataTable GetCourseAttendanceSummary(int studentId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT
                        e.EnrolmentId,
                        c.CourseCode,
                        c.CourseName,
                        e.AcademicYear,
                        e.Semester,
                        COUNT(a.AttendanceId) AS TotalClasses,
                        SUM(CASE WHEN a.Status = 'Present' THEN 1 ELSE 0 END) AS PresentCount,
                        SUM(CASE WHEN a.Status = 'Late' THEN 1 ELSE 0 END) AS LateCount,
                        SUM(CASE WHEN a.Status = 'Absent' THEN 1 ELSE 0 END) AS AbsentCount,
                        SUM(CASE WHEN a.Status = 'Excused' THEN 1 ELSE 0 END) AS ExcusedCount,
                        CAST(
                            CASE 
                                WHEN COUNT(a.AttendanceId) = 0 THEN 0
                                ELSE 
                                    (SUM(CASE WHEN a.Status IN ('Present', 'Late', 'Excused') THEN 1 ELSE 0 END) * 100.0)
                                    / COUNT(a.AttendanceId)
                            END
                        AS DECIMAL(5,2)) AS AttendancePercentage
                    FROM Enrolments e
                    INNER JOIN Courses c 
                        ON e.CourseId = c.CourseId
                    LEFT JOIN Attendance a 
                        ON e.EnrolmentId = a.EnrolmentId
                    WHERE e.StudentId = @StudentId
                      AND e.Status IN ('Active', 'Completed')
                    GROUP BY
                        e.EnrolmentId,
                        c.CourseCode,
                        c.CourseName,
                        e.AcademicYear,
                        e.Semester
                    ORDER BY e.AcademicYear, e.Semester, c.CourseCode";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StudentId", studentId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        public DataTable GetAttendanceDetails(int studentId, int enrolmentId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
            SELECT
                c.CourseCode,
                c.CourseName,
                e.AcademicYear,
                e.Semester,
                a.AttendanceDate,
                a.Status,
                a.Remarks,
                u.FullName AS RecordedBy,
                a.RecordedAt
            FROM Attendance a
            INNER JOIN Enrolments e
                ON a.EnrolmentId = e.EnrolmentId
            INNER JOIN Courses c
                ON e.CourseId = c.CourseId
            INNER JOIN Users u
                ON a.RecordedBy = u.UserId
            WHERE e.StudentId = @StudentId
              AND e.Status IN ('Active', 'Completed')
              AND (@EnrolmentId = 0 OR e.EnrolmentId = @EnrolmentId)
            ORDER BY a.AttendanceDate DESC, c.CourseCode";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StudentId", studentId);
                cmd.Parameters.AddWithValue("@EnrolmentId", enrolmentId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }


    }
}