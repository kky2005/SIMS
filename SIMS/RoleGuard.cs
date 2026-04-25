// ============================================================
//  RoleGuard.cs  –  App_Code/RoleGuard.cs
//  ============================================================
//  HOW TEAMMATES USE THIS FILE
//  ───────────────────────────
//  Add ONE line at the very top of every page's Page_Load:
//
//      RoleGuard.Require(this, "Admin");           // Admin-only page
//      RoleGuard.Require(this, "Lecturer");        // Lecturer-only page
//      RoleGuard.Require(this, "Student");         // Student-only page
//      RoleGuard.Require(this, "Admin","Lecturer");// Multiple roles allowed
//      RoleGuard.RequireAny(this);                 // Just needs to be logged in
//
//  That single call will:
//    1. Verify the user is authenticated; redirect to Login if not.
//    2. Verify the user's role matches; redirect to their own dashboard if not.
//    3. Expose RoleGuard.CurrentUser to the page for convenience.
//
// ============================================================

using System;
using System.Web;
using System.Web.UI;

namespace SIMS.App_Code
{
    // ── Strongly-typed session key constants ─────────────────────────────────
    public static class SessionKeys
    {
        public const string UserId = "SIMS_UserId";
        public const string UserName = "SIMS_UserName";
        public const string UserEmail = "SIMS_UserEmail";
        public const string UserRole = "SIMS_UserRole";
        public const string ProgrammeId = "SIMS_ProgrammeId";
    }

    // ── Lightweight current-user wrapper ─────────────────────────────────────
    public class CurrentUserInfo
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }   // "Admin" | "Lecturer" | "Student"
        public int? ProgrammeId { get; set; }

        public bool IsAdmin => Role == "Admin";
        public bool IsLecturer => Role == "Lecturer";
        public bool IsStudent => Role == "Student";
    }

    // ── Role-based redirect helper ────────────────────────────────────────────
    public static class RoleRedirectHelper
    {
        /// <summary>Redirect based on a role string (used right after login).</summary>
        public static void RedirectByRole(HttpResponse response, string role = null)
        {
            if (role == null)
                role = HttpContext.Current.Session[SessionKeys.UserRole] as string ?? string.Empty;

            switch (role)
            {
                case "Admin":
                    response.Redirect("~/Admin/Dashboard.aspx", true);
                    break;
                case "Lecturer":
                    response.Redirect("~/Lecturer/Dashboard.aspx", true);
                    break;
                case "Student":
                    response.Redirect("~/Student/Dashboard.aspx", true);
                    break;
                default:
                    // Unknown role – go back to login for safety
                    response.Redirect("~/Login.aspx", true);
                    break;
            }
        }
    }

    // ── Main guard class ─────────────────────────────────────────────────────
    /// <summary>
    /// Drop-in role enforcement.  Call RoleGuard.Require(this, "Admin") in Page_Load.
    /// </summary>
    public static class RoleGuard
    {
        // ── Public surface ────────────────────────────────────────────────────

        /// <summary>
        /// Require the user to be logged in AND have one of the specified roles.
        /// Call at the very top of Page_Load, before any other logic.
        /// </summary>
        /// <param name="page">Pass 'this' from the code-behind.</param>
        /// <param name="allowedRoles">One or more of "Admin", "Lecturer", "Student".</param>
        public static CurrentUserInfo Require(Page page, params string[] allowedRoles)
        {
            CurrentUserInfo user = GetCurrentUser(page);

            // 1. Not logged in → Login
            if (user == null)
            {
                RedirectToLogin(page);
                return null;            // unreachable after redirect, satisfies compiler
            }

            // 2. Wrong role → their own dashboard
            if (allowedRoles != null && allowedRoles.Length > 0)
            {
                bool allowed = false;
                foreach (string r in allowedRoles)
                    if (r == user.Role) { allowed = true; break; }

                if (!allowed)
                {
                    RoleRedirectHelper.RedirectByRole(page.Response, user.Role);
                    return null;
                }
            }

            return user;
        }

        /// <summary>
        /// Require the user to be logged in (any role accepted).
        /// </summary>
        public static CurrentUserInfo RequireAny(Page page)
        {
            return Require(page);   // no role restriction
        }

        /// <summary>
        /// Returns the current user from Session, or null if not logged in.
        /// </summary>
        public static CurrentUserInfo GetCurrentUser(Page page)
        {
            var session = page.Session;

            object idObj = session[SessionKeys.UserId];
            if (idObj == null) return null;

            return new CurrentUserInfo
            {
                UserId      = (int)idObj,
                FullName    = session[SessionKeys.UserName]    as string ?? string.Empty,
                Email       = session[SessionKeys.UserEmail]   as string ?? string.Empty,
                Role        = session[SessionKeys.UserRole]    as string ?? string.Empty,
                ProgrammeId = session[SessionKeys.ProgrammeId] as int?
            };
        }

        // ── Internal helpers ──────────────────────────────────────────────────

        private static void RedirectToLogin(Page page)
        {
            string returnUrl = HttpUtility.UrlEncode(page.Request.RawUrl);
            page.Response.Redirect("~/Login.aspx?ReturnUrl=" + returnUrl, true);
        }
    }
}