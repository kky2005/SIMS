using System.Data;
using SIMS.DAL;

namespace SIMS.BLL
{
    public class StudentNotificationBLL
    {
        private StudentNotificationDAL notificationDAL = new StudentNotificationDAL();

        public DataTable GetStudentNotifications(int userId)
        {
            return notificationDAL.GetStudentNotifications(userId);
        }

        public int GetUnreadNotificationCount(int userId)
        {
            return notificationDAL.GetUnreadNotificationCount(userId);
        }

        public bool MarkNotificationAsRead(int notificationId, int userId)
        {
            return notificationDAL.MarkNotificationAsRead(notificationId, userId);
        }

        public bool MarkAllNotificationsAsRead(int userId)
        {
            return notificationDAL.MarkAllNotificationsAsRead(userId);
        }

        public string GetNotificationLinkUrl(int notificationId, int userId)
        {
            return notificationDAL.GetNotificationLinkUrl(notificationId, userId);
        }
        public bool DeleteNotification(int notificationId, int userId)
        {
            return notificationDAL.DeleteNotification(notificationId, userId);
        }
    }
}