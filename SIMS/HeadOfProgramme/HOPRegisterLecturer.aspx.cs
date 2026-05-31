using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace SIMS.HeadOfProgramme
{
    public partial class HOPRegisterLecturer : HOPCrudBase
    {
        protected void Page_Load(object sender, EventArgs e){ EnsureAuthenticated(); if(!IsPostBack) BindGrid(); }
        private void BindGrid(){ gvLecturers.DataSource=GetData(@"SELECT l.*, u.FullName,u.Email,u.Phone FROM Lecturers l INNER JOIN Users u ON l.UserId=u.UserId ORDER BY l.LecturerId DESC"); gvLecturers.DataBind(); }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try {
                if(string.IsNullOrWhiteSpace(txtFullName.Text)||string.IsNullOrWhiteSpace(txtEmail.Text)||string.IsNullOrWhiteSpace(txtStaffNo.Text)){ ShowMessage(lblMessage,"Name, email and staff no required.",false); return; }
                using(SqlConnection con=new SqlConnection(ConnStr)) {
                    con.Open(); SqlTransaction tx=con.BeginTransaction();
                    try {
                        int userId;
                        if(string.IsNullOrEmpty(hfLecturerId.Value)) {
                            if(string.IsNullOrWhiteSpace(txtPassword.Text)){ ShowMessage(lblMessage,"Password required for new lecturer.",false); return; }
                            SqlCommand u=new SqlCommand(@"INSERT INTO Users(RoleId,FullName,Email,PasswordHash,Phone,IsActive) OUTPUT INSERTED.UserId VALUES(@Role,@Name,@Email,@Pass,@Phone,1)",con,tx);
                            u.Parameters.AddWithValue("@Role",GetRoleId("Lecturer")); u.Parameters.AddWithValue("@Name",txtFullName.Text.Trim()); u.Parameters.AddWithValue("@Email",txtEmail.Text.Trim()); u.Parameters.AddWithValue("@Pass",HashPassword(txtPassword.Text)); u.Parameters.AddWithValue("@Phone",txtPhone.Text.Trim());
                            userId=(int)u.ExecuteScalar();
                            SqlCommand l=new SqlCommand(@"INSERT INTO Lecturers(UserId,StaffNo,Department,Specialisation,EmploymentStatus) VALUES(@UserId,@Staff,@Dept,@Spec,@Status)",con,tx);
                            l.Parameters.AddWithValue("@UserId",userId); l.Parameters.AddWithValue("@Staff",txtStaffNo.Text.Trim()); l.Parameters.AddWithValue("@Dept",txtDepartment.Text.Trim()); l.Parameters.AddWithValue("@Spec",txtSpecialisation.Text.Trim()); l.Parameters.AddWithValue("@Status",ddlEmploymentStatus.SelectedValue); l.ExecuteNonQuery();
                        } else {
                            userId=Convert.ToInt32(hfUserId.Value);
                            string userSql=string.IsNullOrWhiteSpace(txtPassword.Text) ? @"UPDATE Users SET FullName=@Name,Email=@Email,Phone=@Phone WHERE UserId=@UserId" : @"UPDATE Users SET FullName=@Name,Email=@Email,Phone=@Phone,PasswordHash=@Pass WHERE UserId=@UserId";
                            SqlCommand u=new SqlCommand(userSql,con,tx); u.Parameters.AddWithValue("@Name",txtFullName.Text.Trim()); u.Parameters.AddWithValue("@Email",txtEmail.Text.Trim()); u.Parameters.AddWithValue("@Phone",txtPhone.Text.Trim()); u.Parameters.AddWithValue("@UserId",userId); if(!string.IsNullOrWhiteSpace(txtPassword.Text)) u.Parameters.AddWithValue("@Pass",HashPassword(txtPassword.Text)); u.ExecuteNonQuery();
                            SqlCommand l=new SqlCommand(@"UPDATE Lecturers SET StaffNo=@Staff,Department=@Dept,Specialisation=@Spec,EmploymentStatus=@Status WHERE LecturerId=@Id",con,tx);
                            l.Parameters.AddWithValue("@Staff",txtStaffNo.Text.Trim()); l.Parameters.AddWithValue("@Dept",txtDepartment.Text.Trim()); l.Parameters.AddWithValue("@Spec",txtSpecialisation.Text.Trim()); l.Parameters.AddWithValue("@Status",ddlEmploymentStatus.SelectedValue); l.Parameters.AddWithValue("@Id",hfLecturerId.Value); l.ExecuteNonQuery();
                        }
                        tx.Commit(); ClearForm(); BindGrid(); ShowMessage(lblMessage,"Lecturer saved successfully.",true);
                    } catch { tx.Rollback(); throw; }
                }
            } catch(Exception ex){ ShowMessage(lblMessage,ex.Message,false); }
        }

        protected void gvLecturers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try {
                int id=Convert.ToInt32(e.CommandArgument);
                if(e.CommandName=="EditLecturer") {
                    DataTable dt=GetData(@"SELECT l.*,u.FullName,u.Email,u.Phone FROM Lecturers l INNER JOIN Users u ON l.UserId=u.UserId WHERE l.LecturerId=@Id",new SqlParameter("@Id",id));
                    if(dt.Rows.Count==0)return; DataRow r=dt.Rows[0]; hfLecturerId.Value=id.ToString(); hfUserId.Value=r["UserId"].ToString();
                    txtFullName.Text=r["FullName"].ToString(); txtEmail.Text=r["Email"].ToString(); txtPhone.Text=r["Phone"].ToString(); txtStaffNo.Text=r["StaffNo"].ToString(); txtDepartment.Text=r["Department"].ToString(); txtSpecialisation.Text=r["Specialisation"].ToString(); ddlEmploymentStatus.SelectedValue=r["EmploymentStatus"].ToString();
                } else if(e.CommandName=="DeleteLecturer") {
                    DataTable dt=GetData("SELECT UserId FROM Lecturers WHERE LecturerId=@Id",new SqlParameter("@Id",id)); if(dt.Rows.Count==0)return; int uid=Convert.ToInt32(dt.Rows[0]["UserId"]);
                    Execute("DELETE FROM Lecturers WHERE LecturerId=@Id",new SqlParameter("@Id",id)); Execute("DELETE FROM Users WHERE UserId=@Uid",new SqlParameter("@Uid",uid)); BindGrid(); ShowMessage(lblMessage,"Lecturer deleted.",true);
                }
            } catch(Exception ex){ ShowMessage(lblMessage,"Delete failed. Lecturer may be assigned to courses. "+ex.Message,false); }
        }
        protected void btnClear_Click(object sender,EventArgs e){ ClearForm(); }
        private void ClearForm(){ hfLecturerId.Value=""; hfUserId.Value=""; txtFullName.Text=""; txtEmail.Text=""; txtPhone.Text=""; txtPassword.Text=""; txtStaffNo.Text=""; txtDepartment.Text=""; txtSpecialisation.Text=""; ddlEmploymentStatus.SelectedValue="Active"; }
    }
}
