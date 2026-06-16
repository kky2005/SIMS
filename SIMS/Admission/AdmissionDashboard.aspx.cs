using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SIMS
{
    public partial class AdmissionDashboard : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Authenticate applicant based on user session (do not rely on AdmissionId session)
            int userId = AuthenticationHelper.GetCurrentUserId();
            string email = AuthenticationHelper.GetCurrentUserEmail();

            if (userId == 0 || string.IsNullOrEmpty(email))
            {
                // Not an authenticated applicant — send to admission login
                Response.Redirect("~/AdmissionLogin.aspx");
                return;
            }

            if (!IsPostBack)
            {
                // admissionId is available and valid here
            }
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            // Clear applicant session keys and sign out (do not depend on AdmissionId session)
            Session.Remove(AuthenticationHelper.SESSION_USER_ID);
            Session.Remove(AuthenticationHelper.SESSION_EMAIL);
            Session.Abandon();
            Response.Redirect("~/AdmissionLogin.aspx");
        }
    }
}