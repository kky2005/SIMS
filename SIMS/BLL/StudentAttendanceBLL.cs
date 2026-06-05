using System.Data;
using SIMS.DAL;

namespace SIMS.BLL
{
    public class StudentAttendanceBLL
    {
        private StudentAttendanceDAL attendanceDAL = new StudentAttendanceDAL();

        public DataTable GetOverallAttendanceStats(int studentId)
        {
            return attendanceDAL.GetOverallAttendanceStats(studentId);
        }

        public DataTable GetCourseAttendanceSummary(int studentId)
        {
            return attendanceDAL.GetCourseAttendanceSummary(studentId);
        }

        public DataTable GetAttendanceDetails(int studentId, int enrolmentId)
        {
            return attendanceDAL.GetAttendanceDetails(studentId, enrolmentId);
        }
        public DataTable GetAttendanceCourseFilter(int studentId)
        {
            return attendanceDAL.GetAttendanceCourseFilter(studentId);
        }
    }
}