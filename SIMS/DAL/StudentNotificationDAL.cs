using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SIMS.DAL
{
    public class StudentNotificationDAL
    {
        private string connStr =
            ConfigurationManager.ConnectionStrings["SIMS_DB"].ConnectionString;

        // GET ALL NOTIFICATIONS FOR THE LOGGED-IN USER
        public DataTable GetStudentNotifications(int userId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT
                        NotificationId,
                        Title,
                        Message,
                        NotificationType,
                        IsRead,
                        CreatedAt,
                        LinkUrl,
                        CASE
                            WHEN IsRead = 1 THEN 'Read'
                            ELSE 'Unread'
                        END AS ReadStatus
                    FROM Notifications
                    WHERE UserId = @UserId
                    ORDER BY IsRead ASC, CreatedAt DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();

                da.Fill(dt);

                return dt;
            }
        }

        // GET NUMBER OF UNREAD NOTIFICATIONS
        public int GetUnreadNotificationCount(int userId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT COUNT(*)
                    FROM Notifications
                    WHERE UserId = @UserId
                      AND IsRead = 0";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);

                conn.Open();

                object result = cmd.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt32(result);
                }

                return 0;
            }
        }

        // MARK ONE NOTIFICATION AS READ
        public bool MarkNotificationAsRead(int notificationId, int userId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    UPDATE Notifications
                    SET IsRead = 1
                    WHERE NotificationId = @NotificationId
                      AND UserId = @UserId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@NotificationId", notificationId);
                cmd.Parameters.AddWithValue("@UserId", userId);

                conn.Open();

                int rowsAffected = cmd.ExecuteNonQuery();

                return rowsAffected > 0;
            }
        }

        // MARK ALL USER NOTIFICATIONS AS READ
        public bool MarkAllNotificationsAsRead(int userId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    UPDATE Notifications
                    SET IsRead = 1
                    WHERE UserId = @UserId
                      AND IsRead = 0";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);

                conn.Open();

                int rowsAffected = cmd.ExecuteNonQuery();

                return rowsAffected > 0;
            }
        }

        // GET THE PAGE LINK STORED FOR A NOTIFICATION
        public string GetNotificationLinkUrl(int notificationId, int userId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT LinkUrl
                    FROM Notifications
                    WHERE NotificationId = @NotificationId
                      AND UserId = @UserId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@NotificationId", notificationId);
                cmd.Parameters.AddWithValue("@UserId", userId);

                conn.Open();

                object result = cmd.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    return result.ToString();
                }

                return "";
            }
        }

        // DELETE ONE NOTIFICATION
        public bool DeleteNotification(int notificationId, int userId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    DELETE FROM Notifications
                    WHERE NotificationId = @NotificationId
                      AND UserId = @UserId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@NotificationId", notificationId);
                cmd.Parameters.AddWithValue("@UserId", userId);

                conn.Open();

                int rowsAffected = cmd.ExecuteNonQuery();

                return rowsAffected > 0;
            }
        }
    }
}