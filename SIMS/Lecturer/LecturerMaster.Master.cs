using System;
using System.Web.UI;

namespace SIMS.Lecturer
{
    public partial class LecturerMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Ensure user is authenticated and is a Lecturer
            if (Session["UserId"] == null || Session["UserRole"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            // Verify user role is Lecturer
            if (!Session["UserRole"].ToString().Equals("Lecturer", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            // Set sidebar and topbar names
            string fullName = Session["FullName"] != null
                ? Session["FullName"].ToString()
                : "Lecturer";

            litSidebarName.Text = fullName;
            litTopbarName.Text = fullName;
        }

        /// <summary>
        /// Returns "active" CSS class if the current page matches the provided page name.
        /// Used by navigation links in the master page to highlight the current page.
        /// </summary>
        public string GetActiveClass(string pageName)
        {
            string currentPage = System.IO.Path.GetFileNameWithoutExtension(Request.FilePath);
            return currentPage.Equals(pageName, StringComparison.OrdinalIgnoreCase) 
                ? "active" 
                : "";
        }

        /// <summary>
        /// Handles the logout button click event.
        /// Clears the session and redirects to the login page.
        /// </summary>
        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Login.aspx");
        }
    }
}