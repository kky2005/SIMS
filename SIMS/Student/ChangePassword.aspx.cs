using System;
using SIMS.BLL;

namespace SIMS.Student
{
    public partial class ChangePassword : System.Web.UI.Page
    {
        private StudentAccountBLL accountBLL = new StudentAccountBLL();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null ||
                Session["UserRole"] == null ||
                Session["UserRole"].ToString().ToLower() != "student")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }
        }

        protected void btnChangePassword_Click(object sender, EventArgs e)
        {
            int userId = Convert.ToInt32(Session["UserId"]);

            string currentPassword = txtCurrentPassword.Text.Trim();
            string newPassword = txtNewPassword.Text.Trim();
            string confirmPassword = txtConfirmPassword.Text.Trim();

            string message;

            bool success = accountBLL.ChangePassword(
                userId,
                currentPassword,
                newPassword,
                confirmPassword,
                out message
            );

            lblMessage.Text = message;

            if (success)
            {
                lblMessage.ForeColor = System.Drawing.Color.Green;

                txtCurrentPassword.Text = "";
                txtNewPassword.Text = "";
                txtConfirmPassword.Text = "";
            }
            else
            {
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("Profile.aspx");
        }
    }
}