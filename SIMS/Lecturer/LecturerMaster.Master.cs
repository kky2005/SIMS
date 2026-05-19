using System;
using System.Web.UI;

namespace SIMS.Lecturer
{
    public partial class LecturerMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Guard: must be logged in as Lecturer
            if (Session["LecturerID"] == null)
            {
                Response.Redirect("~/Lecturer/LecturerLogin.aspx");
                return;
            }

            string fullName = Session["FullName"] != null
                ? Session["FullName"].ToString()
                : "Lecturer";

            litSidebarName.Text = fullName;
            litTopbarName.Text  = fullName;
        }

        // Returns "active" css class for the current page link
        public string GetActiveClass(string pageName)
        {
            string currentPage = System.IO.Path.GetFileNameWithoutExtension(
                Request.FilePath);
            return currentPage.Equals(pageName,
                StringComparison.OrdinalIgnoreCase) ? "active" : "";
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Lecturer/LecturerLogin.aspx");
        }
    }
}