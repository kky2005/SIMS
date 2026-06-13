using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SIMS.DAL
{
    public class StudentPaymentDAL
    {
        private string connStr = ConfigurationManager.ConnectionStrings["SIMS_DB"].ConnectionString;

        public void EnsureStudentPayments(int studentId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
            DECLARE @FeePerCreditHour DECIMAL(10,2) = 100.00;

            DECLARE @SemesterFees TABLE
            (
                StudentId INT,
                AcademicYear SMALLINT,
                Semester TINYINT,
                TotalCreditHours INT,
                Amount DECIMAL(10,2)
            );

            INSERT INTO @SemesterFees
            (StudentId, AcademicYear, Semester, TotalCreditHours, Amount)
            SELECT
                e.StudentId,
                e.AcademicYear,
                e.Semester,
                SUM(c.CreditHours) AS TotalCreditHours,
                SUM(c.CreditHours) * @FeePerCreditHour AS Amount
            FROM Enrolments e
            INNER JOIN Courses c 
                ON e.CourseId = c.CourseId
            WHERE e.StudentId = @StudentId
              AND e.Status IN ('Active', 'Completed')
            GROUP BY 
                e.StudentId, 
                e.AcademicYear, 
                e.Semester;

            -- Update existing payment amount
            UPDATE sp
            SET sp.Amount = sf.Amount
            FROM StudentPayments sp
            INNER JOIN @SemesterFees sf
                ON sp.StudentId = sf.StudentId
               AND sp.AcademicYear = sf.AcademicYear
               AND sp.Semester = sf.Semester;

            -- For semester 1 students, mark semester 1 payment as already paid
            UPDATE sp
            SET sp.PaymentStatus = 'Paid',
                sp.PaidAt = ISNULL(sp.PaidAt, SYSUTCDATETIME())
            FROM StudentPayments sp
            INNER JOIN Students s 
                ON sp.StudentId = s.StudentId
            WHERE sp.StudentId = @StudentId
              AND s.CurrentSemester = 1
              AND sp.Semester = 1
              AND sp.PaymentStatus = 'Pending';

            -- Insert missing payment records
            INSERT INTO StudentPayments
            (StudentId, AcademicYear, Semester, Amount, PaymentStatus, CreatedAt, PaidAt)
            SELECT
                sf.StudentId,
                sf.AcademicYear,
                sf.Semester,
                sf.Amount,
                CASE 
                    WHEN s.CurrentSemester = 1 AND sf.Semester = 1 THEN 'Paid'
                    WHEN sf.Semester < s.CurrentSemester THEN 'Paid'
                    ELSE 'Pending'
                END AS PaymentStatus,
                SYSUTCDATETIME(),
                CASE 
                    WHEN s.CurrentSemester = 1 AND sf.Semester = 1 THEN SYSUTCDATETIME()
                    WHEN sf.Semester < s.CurrentSemester THEN SYSUTCDATETIME()
                    ELSE NULL
                END AS PaidAt
            FROM @SemesterFees sf
            INNER JOIN Students s 
                ON sf.StudentId = s.StudentId
            WHERE NOT EXISTS (
                SELECT 1
                FROM StudentPayments sp
                WHERE sp.StudentId = sf.StudentId
                  AND sp.AcademicYear = sf.AcademicYear
                  AND sp.Semester = sf.Semester
            );";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StudentId", studentId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public DataTable GetStudentPayments(int studentId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT
                        p.PaymentId,
                        p.AcademicYear,
                        p.Semester,
                        ISNULL(ch.TotalCreditHours, 0) AS TotalCreditHours,
                        CAST(100.00 AS DECIMAL(10,2)) AS FeePerCreditHour,
                        p.Amount,
                        p.PaymentStatus,
                        p.CreatedAt,
                        p.PaidAt
                    FROM StudentPayments p
                    LEFT JOIN (
                        SELECT
                            e.StudentId,
                            e.AcademicYear,
                            e.Semester,
                            SUM(c.CreditHours) AS TotalCreditHours
                        FROM Enrolments e
                        INNER JOIN Courses c 
                            ON e.CourseId = c.CourseId
                        WHERE e.Status IN ('Active', 'Completed')
                        GROUP BY 
                            e.StudentId, 
                            e.AcademicYear, 
                            e.Semester
                    ) ch
                        ON p.StudentId = ch.StudentId
                       AND p.AcademicYear = ch.AcademicYear
                       AND p.Semester = ch.Semester
                    WHERE p.StudentId = @StudentId
                    ORDER BY 
                        p.AcademicYear DESC, 
                        p.Semester DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StudentId", studentId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        public bool PayStudentFee(int paymentId, int studentId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    UPDATE StudentPayments
                    SET PaymentStatus = 'Paid',
                        PaidAt = SYSUTCDATETIME()
                    WHERE PaymentId = @PaymentId
                      AND StudentId = @StudentId
                      AND PaymentStatus = 'Pending'";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@PaymentId", paymentId);
                cmd.Parameters.AddWithValue("@StudentId", studentId);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();

                return rowsAffected > 0;
            }
        }
    }
}