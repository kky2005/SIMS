using System.Data;
using SIMS.DAL;

namespace SIMS.BLL
{
    public class StudentMarksDetailBLL
    {
        private StudentMarksDetailDAL marksDAL = new StudentMarksDetailDAL();

        public DataTable GetSemesterMarksDetails(int studentId, int academicYear, int semester)
        {
            return marksDAL.GetSemesterMarksDetails(studentId, academicYear, semester);
        }

        public DataTable GetCourseTotals(int studentId, int academicYear, int semester)
        {
            return marksDAL.GetCourseTotals(studentId, academicYear, semester);
        }

        public DataTable GetSemesterInfo(int studentId, int academicYear, int semester)
        {
            return marksDAL.GetSemesterInfo(studentId, academicYear, semester);
        }
    }
}