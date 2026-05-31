using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace SIMS.HeadOfProgramme
{
    public partial class HOPRegisterStudent : HOPCrudBase
    {
        protected void Page_Load(object sender, EventArgs e){ EnsureAuthenticated(); if(!IsPostBack){ BindProgrammes(); BindGrid(); } }
        private void BindProgrammes(){ BindDropDown(ddlProgramme,"SELECT ProgrammeId,ProgrammeName FROM Programmes ORDER BY ProgrammeName","ProgrammeName","ProgrammeId"); }
        private void BindGrid(){ gvStudents.DataSource=GetData(@"SELECT s.*,u.FullName,u.Email,p.ProgrammeName FROM Students s INNER JOIN Users u ON s.UserId=u.UserId INNER JOIN Programmes p ON s.ProgrammeId=p.ProgrammeId ORDER BY s.StudentId DESC"); gvStudents.DataBind(); }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try {
                if(string.IsNullOrWhiteSpace(txtFullName.Text)||string.IsNullOrWhiteSpace(txtEmail.Text)||string.IsNullOrWhiteSpace(txtStudentNo.Text)){ ShowMessage(lblMessage,"Name, email and student no required.",false); return; }
                using(SqlConnection con=new SqlConnection(ConnStr)){ con.Open(); SqlTransaction tx=con.BeginTransaction();
                    try{
                        int userId;
                        if(string.IsNullOrEmpty(hfStudentId.Value)){
                            if(string.IsNullOrWhiteSpace(txtPassword.Text)){ ShowMessage(lblMessage,"Password required for new student.",false); return; }
                            SqlCommand u=new SqlCommand(@"INSERT INTO Users(RoleId,FullName,Email,PasswordHash,Phone,IsActive) OUTPUT INSERTED.UserId VALUES(@Role,@Name,@Email,@Pass,@Phone,1)",con,tx);
                            u.Parameters.AddWithValue("@Role",GetRoleId("Student")); u.Parameters.AddWithValue("@Name",txtFullName.Text.Trim()); u.Parameters.AddWithValue("@Email",txtEmail.Text.Trim()); u.Parameters.AddWithValue("@Pass",HashPassword(txtPassword.Text)); u.Parameters.AddWithValue("@Phone",txtPhone.Text.Trim()); userId=(int)u.ExecuteScalar();
                            SqlCommand s=new SqlCommand(@"INSERT INTO Students(UserId,ProgrammeId,StudentNo,IntakeYear,IntakeSemester,AdmissionDate,CurrentSemester,Status) VALUES(@UserId,@P,@No,@Year,@ISem,@Date,@CSem,@Status)",con,tx);
                            AddStudentParams(s,userId); s.ExecuteNonQuery();
                        } else {
                            userId=Convert.ToInt32(hfUserId.Value);
                            string userSql=string.IsNullOrWhiteSpace(txtPassword.Text) ? @"UPDATE Users SET FullName=@Name,Email=@Email,Phone=@Phone WHERE UserId=@UserId" : @"UPDATE Users SET FullName=@Name,Email=@Email,Phone=@Phone,PasswordHash=@Pass WHERE UserId=@UserId";
                            SqlCommand u=new SqlCommand(userSql,con,tx); u.Parameters.AddWithValue("@Name",txtFullName.Text.Trim()); u.Parameters.AddWithValue("@Email",txtEmail.Text.Trim()); u.Parameters.AddWithValue("@Phone",txtPhone.Text.Trim()); u.Parameters.AddWithValue("@UserId",userId); if(!string.IsNullOrWhiteSpace(txtPassword.Text)) u.Parameters.AddWithValue("@Pass",HashPassword(txtPassword.Text)); u.ExecuteNonQuery();
                            SqlCommand s=new SqlCommand(@"UPDATE Students SET ProgrammeId=@P,StudentNo=@No,IntakeYear=@Year,IntakeSemester=@ISem,AdmissionDate=@Date,CurrentSemester=@CSem,Status=@Status WHERE StudentId=@Id",con,tx);
                            AddStudentParams(s,userId); s.Parameters.AddWithValue("@Id",hfStudentId.Value); s.ExecuteNonQuery();
                        }
                        tx.Commit(); ClearForm(); BindGrid(); ShowMessage(lblMessage,"Student saved successfully.",true);
                    } catch{ tx.Rollback(); throw; }
                }
            } catch(Exception ex){ ShowMessage(lblMessage,ex.Message,false); }
        }
        private void AddStudentParams(SqlCommand s,int userId){ s.Parameters.AddWithValue("@UserId",userId); s.Parameters.AddWithValue("@P",ddlProgramme.SelectedValue); s.Parameters.AddWithValue("@No",txtStudentNo.Text.Trim()); s.Parameters.AddWithValue("@Year",txtIntakeYear.Text); s.Parameters.AddWithValue("@ISem",txtIntakeSemester.Text); s.Parameters.AddWithValue("@Date",string.IsNullOrEmpty(txtAdmissionDate.Text)?(object)DBNull.Value:txtAdmissionDate.Text); s.Parameters.AddWithValue("@CSem",txtCurrentSemester.Text); s.Parameters.AddWithValue("@Status",ddlStatus.SelectedValue); }

        protected void gvStudents_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try{
                int id=Convert.ToInt32(e.CommandArgument);
                if(e.CommandName=="EditStudent"){ DataTable dt=GetData(@"SELECT s.*,u.FullName,u.Email,u.Phone FROM Students s INNER JOIN Users u ON s.UserId=u.UserId WHERE s.StudentId=@Id",new SqlParameter("@Id",id)); if(dt.Rows.Count==0)return; DataRow r=dt.Rows[0];
                    hfStudentId.Value=id.ToString(); hfUserId.Value=r["UserId"].ToString(); txtFullName.Text=r["FullName"].ToString(); txtEmail.Text=r["Email"].ToString(); txtPhone.Text=r["Phone"].ToString(); txtStudentNo.Text=r["StudentNo"].ToString(); ddlProgramme.SelectedValue=r["ProgrammeId"].ToString(); txtIntakeYear.Text=r["IntakeYear"].ToString(); txtIntakeSemester.Text=r["IntakeSemester"].ToString(); txtCurrentSemester.Text=r["CurrentSemester"].ToString(); txtAdmissionDate.Text=r["AdmissionDate"]==DBNull.Value?"":Convert.ToDateTime(r["AdmissionDate"]).ToString("yyyy-MM-dd"); ddlStatus.SelectedValue=r["Status"].ToString();
                } else if(e.CommandName=="DeleteStudent"){ DataTable dt=GetData("SELECT UserId FROM Students WHERE StudentId=@Id",new SqlParameter("@Id",id)); if(dt.Rows.Count==0)return; int uid=Convert.ToInt32(dt.Rows[0]["UserId"]); Execute("DELETE FROM Students WHERE StudentId=@Id",new SqlParameter("@Id",id)); Execute("DELETE FROM Users WHERE UserId=@Uid",new SqlParameter("@Uid",uid)); BindGrid(); ShowMessage(lblMessage,"Student deleted.",true); }
            } catch(Exception ex){ ShowMessage(lblMessage,"Delete failed. Student may have enrolments/marks/fees. "+ex.Message,false); }
        }
        protected void btnClear_Click(object sender,EventArgs e){ ClearForm(); }
        private void ClearForm(){ hfStudentId.Value=""; hfUserId.Value=""; txtFullName.Text=""; txtEmail.Text=""; txtPhone.Text=""; txtPassword.Text=""; txtStudentNo.Text=""; txtIntakeYear.Text=DateTime.Now.Year.ToString(); txtIntakeSemester.Text="1"; txtCurrentSemester.Text="1"; txtAdmissionDate.Text=DateTime.Now.ToString("yyyy-MM-dd"); ddlStatus.SelectedValue="Active"; if(ddlProgramme.Items.Count>0) ddlProgramme.SelectedIndex=0; }
    }
}
