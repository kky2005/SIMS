using SIMS.DAL;

namespace SIMS.BLL
{
    public class StudentAccountBLL
    {
        private StudentAccountDAL accountDAL = new StudentAccountDAL();

        public bool ChangePassword(int userId, string currentPassword, string newPassword, string confirmPassword, out string message)
        {
            if (string.IsNullOrWhiteSpace(currentPassword))
            {
                message = "Please enter your current password.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                message = "Please enter your new password.";
                return false;
            }

            if (newPassword.Length < 6)
            {
                message = "New password must be at least 6 characters.";
                return false;
            }

            if (newPassword != confirmPassword)
            {
                message = "New password and confirm password do not match.";
                return false;
            }

            if (currentPassword == newPassword)
            {
                message = "New password cannot be the same as the current password.";
                return false;
            }

            bool success = accountDAL.ChangePassword(userId, currentPassword, newPassword);

            if (success)
            {
                message = "Password changed successfully.";
                return true;
            }

            message = "Current password is incorrect.";
            return false;
        }
    }
}