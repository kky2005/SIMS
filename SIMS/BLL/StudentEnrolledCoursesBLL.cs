using System.Data;
using SIMS.DAL;

namespace SIMS.BLL
{
    public class StudentEnrolledCourseBLL
    {
        private StudentEnrolledCourseDAL courseDAL = new StudentEnrolledCourseDAL();

        public DataTable GetCurrentEnrolledCourses(int studentId)
        {
            return courseDAL.GetCurrentEnrolledCourses(studentId);
        }

        public DataTable GetCourseDetails(int studentId, int courseId)
        {
            return courseDAL.GetCourseDetails(studentId, courseId);
        }

        public DataTable GetCourseMaterials(int studentId, int courseId)
        {
            return courseDAL.GetCourseMaterials(studentId, courseId);
        }
    }
}