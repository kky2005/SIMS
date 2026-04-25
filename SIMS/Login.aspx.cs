using System;
using System.Web;
using System.Web.Security;
using SIMS.App_Code;

namespace SIMS
{
    public partial class Login : System.Web.UI.Page
    {
        // ── Properties consumed by the ASPX template ──────────────────────────
        protected bool ShowAlert { get; private set; } = false;
        protected string AlertMessage { get; private set; } = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            // If already authenticated, redirect immediately
            if (!IsPostBack && Request.IsAuthenticated)
                RoleRedirectHelper.RedirectByRole(Response);
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Please enter both your email address and password.");
                return;
            }

            // ── Authenticate against the database ─────────────────────────────
            UserLoginResult result = AuthService.ValidateUser(email, password);

            if (!result.IsSuccess)
            {
                ShowError("Invalid email or password. Please try again.");
                return;
            }

            // ── Persist session data ───────────────────────────────────────────
            Session[SessionKeys.UserId]       = result.UserId;
            Session[SessionKeys.UserName]     = result.FullName;
            Session[SessionKeys.UserEmail]    = result.Email;
            Session[SessionKeys.UserRole]     = result.Role;          // "Admin" | "Lecturer" | "Student"
            Session[SessionKeys.ProgrammeId]  = result.ProgrammeId;  // nullable

            // ── Forms authentication ticket (used by RoleGuard) ───────────────
            if (chkRemember.Checked)
            {
                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                    version: 1,
                    name: result.Email,
                    issueDate: DateTime.Now,
                    expiration: DateTime.Now.AddDays(14),
                    isPersistent: true,
                    userData: result.Role
                );
                string encrypted = FormsAuthentication.Encrypt(ticket);
                HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted)
                {
                    Expires  = ticket.Expiration,
                    HttpOnly = true,
                    Secure   = Request.IsSecureConnection
                };
                Response.Cookies.Add(cookie);
            }
            else
            {
                FormsAuthentication.SetAuthCookie(result.Email, false);
            }

            // ── Redirect to the correct dashboard ─────────────────────────────
            RoleRedirectHelper.RedirectByRole(Response, result.Role);
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private void ShowError(string message)
        {
            ShowAlert    = true;
            AlertMessage = message;
        }
    }
}