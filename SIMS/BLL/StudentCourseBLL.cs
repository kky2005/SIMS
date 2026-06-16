using System.Data;
using SIMS.DAL;

namespace SIMS.BLL
{
    public class StudentCourseBLL
    {
        private StudentCourseDAL courseDAL = new StudentCourseDAL();

        public int GetCurrentSemester(int studentId)
        {
            return courseDAL.GetCurrentSemester(studentId);
        }

        public DataTable GetAvailableCourses(int studentId)
        {
            return courseDAL.GetAvailableCourses(studentId);
        }

        public DataTable GetEnrolledCourses(int studentId)
        {
            return courseDAL.GetEnrolledCourses(studentId);
        }

        public DataTable GetCourseRequests(int studentId)
        {
            return courseDAL.GetCourseRequests(studentId);
        }

        public bool IsRegistrationOpen(int studentId)
        {
            return courseDAL.IsWithinRegistrationPeriod(studentId, "Registration");
        }

        public bool IsDropOpen(int studentId)
        {
            return courseDAL.IsWithinRegistrationPeriod(studentId, "Drop");
        }
        public string SubmitRegisterRequest(int studentId, int courseId)
        {
            int currentSemester = courseDAL.GetCurrentSemester(studentId);

            if (currentSemester <= 0)
            {
                return "Student semester information could not be found.";
            }

            if (currentSemester == 1)
            {
                return "First semester course registration is managed by Admin. You cannot register courses yourself.";
            }

            if (!courseDAL.IsWithinRegistrationPeriod(studentId, "Registration"))
            {
                return "Course registration is currently closed. Please register during the allowed registration period.";
            }

            if (courseDAL.HasPendingRequest(studentId, courseId, "Register"))
            {
                return "You already have a pending registration request for this course.";
            }

            bool success = courseDAL.InsertCourseRequest(studentId, courseId, "Register");

            if (success)
            {
                return "Course registration request submitted successfully. Please wait for Admin approval.";
            }

            return "Course registration request failed. Please try again.";
        }

        public string SubmitDropRequest(int studentId, int courseId)
        {
            if (!courseDAL.IsWithinRegistrationPeriod(studentId, "Drop"))
            {
                return "Course drop is currently closed. Please drop courses during the allowed drop period.";
            }

            if (!courseDAL.IsCurrentSemesterActiveEnrolment(studentId, courseId))
            {
                return "You can only request to drop courses from your current semester.";
            }

            if (courseDAL.HasPendingRequest(studentId, courseId, "Drop"))
            {
                return "You already have a pending drop request for this course.";
            }

            bool success = courseDAL.InsertCourseRequest(studentId, courseId, "Drop");

            if (success)
            {
                return "Course drop request submitted successfully. Please wait for Admin approval.";
            }

            return "Course drop request failed. Please try again.";
        }
        public DataTable GetRegistrationPeriods(int studentId)
        {
            return courseDAL.GetRegistrationPeriods(studentId);
        }
    }
}