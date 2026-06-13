using System;
using System.Configuration;
using System.Data.SqlClient;

namespace SIMS.DAL
{
    public class StudentAccountDAL
    {
        private string connStr = ConfigurationManager.ConnectionStrings["SIMS_DB"].ConnectionString;

        public bool ChangePassword(int userId, string currentPassword, string newPassword)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    UPDATE Users
                    SET PasswordHash = CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CONVERT(VARCHAR(255), @NewPassword)), 2)
                    WHERE UserId = @UserId
                    AND PasswordHash = CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CONVERT(VARCHAR(255), @CurrentPassword)), 2)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@CurrentPassword", currentPassword);
                cmd.Parameters.AddWithValue("@NewPassword", newPassword);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();

                return rowsAffected > 0;
            }
        }
    }
}