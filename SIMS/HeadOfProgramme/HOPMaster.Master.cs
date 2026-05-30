using System;
using System.Web.UI;

namespace SIMS.HeadOfProgramme
{
    public partial class HOPMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null || Session["UserRole"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!Session["UserRole"].ToString().Equals("HeadOfProgramme", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string fullName = Session["FullName"] != null
                ? Session["FullName"].ToString()
                : "Head Of Programme";

            litSidebarName.Text = fullName;
            litTopbarName.Text = fullName;
        }

        public string GetActiveClass(string pageName)
        {
            string currentPage = System.IO.Path.GetFileNameWithoutExtension(Request.FilePath);

            return currentPage.Equals(pageName, StringComparison.OrdinalIgnoreCase)
                ? "active"
                : "";
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Login.aspx");
        }
    }
}