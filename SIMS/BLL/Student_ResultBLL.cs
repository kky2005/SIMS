using System.Data;
using SIMS.DAL;

namespace SIMS.BLL
{
    public class StudentResultBLL
    {
        private StudentResultDAL resultDAL = new StudentResultDAL();

        public DataTable GetGPASummary(int studentId)
        {
            return resultDAL.GetGPASummary(studentId);
        }

        public DataTable GetCourseResults(int studentId)
        {
            return resultDAL.GetCourseResults(studentId);
        }

        public DataTable GetAssessmentBreakdown(int studentId)
        {
            return resultDAL.GetAssessmentBreakdown(studentId);
        }

        public DataTable GetResultSemesters(int studentId)
        {
            return resultDAL.GetResultSemesters(studentId);
        }

        public DataTable GetCourseResultsBySemester(int studentId, int academicYear, int semester)
        {
            return resultDAL.GetCourseResultsBySemester(studentId, academicYear, semester);
        }
        public DataTable GetGPASummaryBySemester(int studentId, int academicYear, int semester)
        {
            return resultDAL.GetGPASummaryBySemester(studentId, academicYear, semester);
        }
    }
}