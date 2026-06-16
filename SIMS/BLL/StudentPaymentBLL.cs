using System.Data;
using SIMS.DAL;

namespace SIMS.BLL
{
    public class StudentPaymentBLL
    {
        private StudentPaymentDAL paymentDAL = new StudentPaymentDAL();

        public DataTable GetStudentPayments(int studentId)
        {
            return paymentDAL.GetStudentPayments(studentId);
        }

        public bool PayStudentFee(int paymentId, int studentId)
        {
            return paymentDAL.PayStudentFee(paymentId, studentId);
        }
        public void EnsureStudentPayments(int studentId)
        {
            paymentDAL.EnsureStudentPayments(studentId);
        }
    }
}