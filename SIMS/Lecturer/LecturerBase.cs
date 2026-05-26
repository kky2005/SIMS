using System;
using System.Web.UI;

namespace SIMS.Lecturer
{
    /// <summary>
    /// Base page class for all Lecturer pages.
    /// Provides centralized authentication and session management.
    /// All Lecturer pages should inherit from this class instead of Page.
    /// </summary>
    public class LecturerBase : Page
    {
        public const string LOGIN_PAGE = "~/Login.aspx";
        public const string SESSION_USER_ID = "UserId";
        public const string SESSION_USER_ROLE = "UserRole";
        public const string SESSION_LECTURER_ID = "LecturerId";
        public const string SESSION_FULL_NAME = "FullName";
        public const string SESSION_STAFF_NO = "StaffNo";
        public const string SESSION_DEPARTMENT = "Department";
        public const string SESSION_EMAIL = "Email";

        /// <summary>
        /// Gets the current logged-in User ID.
        /// </summary>
        public int CurrentUserId
        {
            get
            {
                object id = Session[SESSION_USER_ID];
                return (id != null && int.TryParse(id.ToString(), out int userId))
                    ? userId
                    : 0;
            }
        }

        /// <summary>
        /// Gets the current logged-in Lecturer ID.
        /// </summary>
        public int CurrentLecturerId
        {
            get
            {
                object id = Session[SESSION_LECTURER_ID];
                return (id != null && int.TryParse(id.ToString(), out int lecturerId))
                    ? lecturerId
                    : 0;
            }
        }

        /// <summary>
        /// Gets the full name from session.
        /// </summary>
        public string CurrentFullName
        {
            get
            {
                return Session[SESSION_FULL_NAME] as string ?? "Lecturer";
            }
        }

        /// <summary>
        /// Gets the department from session.
        /// </summary>
        public string CurrentDepartment
        {
            get
            {
                return Session[SESSION_DEPARTMENT] as string ?? "Unknown Department";
            }
        }

        /// <summary>
        /// Gets the staff number from session.
        /// </summary>
        public string CurrentStaffNo
        {
            get
            {
                return Session[SESSION_STAFF_NO] as string ?? "N/A";
            }
        }

        /// <summary>
        /// Gets the email from session.
        /// </summary>
        public string CurrentEmail
        {
            get
            {
                return Session[SESSION_EMAIL] as string ?? "N/A";
            }
        }

        /// <summary>
        /// Checks if the lecturer is authenticated.
        /// Redirects to login page if not authenticated or role is incorrect.
        /// Call this in your Page_Load method before any other logic.
        /// </summary>
        protected void EnsureAuthenticated()
        {
            if (Session[SESSION_USER_ID] == null)
            {
                Response.Redirect(LOGIN_PAGE);
                return;
            }

            // Verify user role is Lecturer
            object userRole = Session[SESSION_USER_ROLE];
            if (userRole == null || !userRole.ToString().Equals("Lecturer", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect(LOGIN_PAGE);
                return;
            }
        }

        /// <summary>
        /// Helper method to get the current semester (1, 2, or 3).
        /// Semester 1 = Jan-Apr, 2 = May-Aug, 3 = Sep-Dec
        /// </summary>
        protected int GetCurrentSemester()
        {
            int month = DateTime.Now.Month;
            if (month <= 4) return 1;
            if (month <= 8) return 2;
            return 3;
        }
    }
}