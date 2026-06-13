using SIMS.BLL;
using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SIMS.Student
{
    public partial class Notifications : System.Web.UI.Page
    {
        private StudentNotificationBLL notificationBLL = new StudentNotificationBLL();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null ||
                Session["UserRole"] == null ||
                Session["UserRole"].ToString().ToLower() != "student")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadNotifications();
            }
        }

        private void LoadNotifications()
        {
            int userId = Convert.ToInt32(Session["UserId"]);

            DataTable dt = notificationBLL.GetStudentNotifications(userId);

            gvNotifications.DataSource = dt;
            gvNotifications.DataBind();

            if (dt.Rows.Count == 0)
            {
                lblMessage.Text = "You have no notifications.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
            else
            {
                int unreadCount = notificationBLL.GetUnreadNotificationCount(userId);

                if (unreadCount > 0)
                {
                    lblMessage.Text = "You have " + unreadCount + " unread notification(s).";
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                }
                else
                {
                    lblMessage.Text = "All notifications have been read.";
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                }
            }
        }

        protected void gvNotifications_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int userId = Convert.ToInt32(Session["UserId"]);

            if (e.CommandName == "OpenNotification")
            {
                int notificationId = Convert.ToInt32(e.CommandArgument);

                notificationBLL.MarkNotificationAsRead(notificationId, userId);

                string linkUrl = notificationBLL.GetNotificationLinkUrl(notificationId, userId);

                if (!string.IsNullOrEmpty(linkUrl))
                {
                    Response.Redirect(linkUrl);
                }
                else
                {
                    LoadNotifications();
                }
            }

            if (e.CommandName == "MarkRead")
            {
                int notificationId = Convert.ToInt32(e.CommandArgument);

                bool success = notificationBLL.MarkNotificationAsRead(notificationId, userId);

                if (success)
                {
                    lblMessage.Text = "Notification marked as read.";
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                }
                else
                {
                    lblMessage.Text = "Unable to update notification.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }

                LoadNotifications();
            }

            if (e.CommandName == "DeleteNotification")
            {
                int notificationId = Convert.ToInt32(e.CommandArgument);

                bool success = notificationBLL.DeleteNotification(notificationId, userId);

                if (success)
                {
                    lblMessage.Text = "Notification deleted successfully.";
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                }
                else
                {
                    lblMessage.Text = "Unable to delete notification.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }

                LoadNotifications();
            }
        }

        protected void gvNotifications_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                bool isRead = Convert.ToBoolean(DataBinder.Eval(e.Row.DataItem, "IsRead"));

                Button btnMarkRead = (Button)e.Row.FindControl("btnMarkRead");

                if (btnMarkRead != null)
                {
                    btnMarkRead.Visible = !isRead;
                }

                if (!isRead)
                {
                    e.Row.Font.Bold = true;
                }
            }
        }

        protected void btnMarkAllRead_Click(object sender, EventArgs e)
        {
            int userId = Convert.ToInt32(Session["UserId"]);

            bool success = notificationBLL.MarkAllNotificationsAsRead(userId);

            if (success)
            {
                lblMessage.Text = "All notifications marked as read.";
                lblMessage.ForeColor = System.Drawing.Color.Green;
            }
            else
            {
                lblMessage.Text = "There are no unread notifications to update.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }

            LoadNotifications();
        }
    }
}