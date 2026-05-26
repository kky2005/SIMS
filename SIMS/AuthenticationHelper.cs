using System;
using System.Web;

namespace SIMS
{
    /// <summary>
    /// Global authentication helper class for session management across all roles.
    /// </summary>
    public static class AuthenticationHelper
    {
        public const string SESSION_USER_ID = "UserId";
        public const string SESSION_USER_ROLE = "UserRole";
        public const string SESSION_FULL_NAME = "FullName";
        public const string SESSION_EMAIL = "Email";

        // Lecturer-specific
        public const string SESSION_LECTURER_ID = "LecturerId";
        public const string SESSION_STAFF_NO = "StaffNo";
        public const string SESSION_DEPARTMENT = "Department";

        // Student-specific
        public const string SESSION_STUDENT_ID = "StudentId";
        public const string SESSION_STUDENT_NO = "StudentNo";
        public const string SESSION_PROGRAMME_ID = "ProgrammeId";

        // Head of Programme-specific
        public const string SESSION_HOP_ID = "HeadOfProgrammeId";

        /// <summary>
        /// Checks if user is authenticated.
        /// </summary>
        public static bool IsAuthenticated()
        {
            return HttpContext.Current.Session != null && 
                   HttpContext.Current.Session[SESSION_USER_ID] != null;
        }

        /// <summary>
        /// Gets the current user's ID.
        /// </summary>
        public static int GetCurrentUserId()
        {
            object userId = HttpContext.Current.Session?[SESSION_USER_ID];
            return (userId != null && int.TryParse(userId.ToString(), out int id)) ? id : 0;
        }

        /// <summary>
        /// Gets the current user's role.
        /// </summary>
        public static string GetCurrentUserRole()
        {
            return HttpContext.Current.Session?[SESSION_USER_ROLE]?.ToString() ?? "";
        }

        /// <summary>
        /// Gets the current user's full name.
        /// </summary>
        public static string GetCurrentUserFullName()
        {
            return HttpContext.Current.Session?[SESSION_FULL_NAME]?.ToString() ?? "User";
        }

        /// <summary>
        /// Checks if current user is a Lecturer.
        /// </summary>
        public static bool IsLecturer()
        {
            return GetCurrentUserRole().Equals("Lecturer", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if current user is a Student.
        /// </summary>
        public static bool IsStudent()
        {
            return GetCurrentUserRole().Equals("Student", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if current user is Head of Programme.
        /// </summary>
        public static bool IsHeadOfProgramme()
        {
            return GetCurrentUserRole().Equals("HeadOfProgramme", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the Lecturer ID (if user is a lecturer).
        /// </summary>
        public static int GetLecturerId()
        {
            object lecturerId = HttpContext.Current.Session?[SESSION_LECTURER_ID];
            return (lecturerId != null && int.TryParse(lecturerId.ToString(), out int id)) ? id : 0;
        }

        /// <summary>
        /// Gets the Student ID (if user is a student).
        /// </summary>
        public static int GetStudentId()
        {
            object studentId = HttpContext.Current.Session?[SESSION_STUDENT_ID];
            return (studentId != null && int.TryParse(studentId.ToString(), out int id)) ? id : 0;
        }
    }
}