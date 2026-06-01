using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace SIMS.HeadOfProgramme
{
    public partial class HOPManageAdmissions : HOPCrudBase
    {
        protected void Page_Load(object sender, EventArgs e){ EnsureAuthenticated(); if(!IsPostBack){ BindProgrammes(); BindGrid(); } }
        private void BindProgrammes(){ BindDropDown(ddlProgramme,"SELECT ProgrammeId,ProgrammeName FROM Programmes ORDER BY ProgrammeName","ProgrammeName","ProgrammeId"); }
        private void BindGrid(){ gvAdmissions.DataSource=GetData(@"SELECT s.*,u.FullName,p.ProgrammeName FROM Students s INNER JOIN Users u ON s.UserId=u.UserId INNER JOIN Programmes p ON s.ProgrammeId=p.ProgrammeId ORDER BY s.StudentId DESC"); gvAdmissions.DataBind(); }
        protected void gvAdmissions_RowCommand(object sender,GridViewCommandEventArgs e){ int id=Convert.ToInt32(e.CommandArgument); DataTable dt=GetData(@"SELECT s.*,u.FullName FROM Students s INNER JOIN Users u ON s.UserId=u.UserId WHERE s.StudentId=@Id",new SqlParameter("@Id",id)); if(dt.Rows.Count==0)return; DataRow r=dt.Rows[0]; hfStudentId.Value=id.ToString(); txtStudent.Text=r["StudentNo"]+" - "+r["FullName"]; ddlProgramme.SelectedValue=r["ProgrammeId"].ToString(); txtIntakeYear.Text=r["IntakeYear"].ToString(); txtIntakeSemester.Text=r["IntakeSemester"].ToString(); txtCurrentSemester.Text=r["CurrentSemester"].ToString(); txtAdmissionDate.Text=r["AdmissionDate"]==DBNull.Value?"":Convert.ToDateTime(r["AdmissionDate"]).ToString("yyyy-MM-dd"); ddlStatus.SelectedValue=r["Status"].ToString(); }
        protected void btnSave_Click(object sender,EventArgs e){ try{ if(string.IsNullOrEmpty(hfStudentId.Value)){ShowMessage(lblMessage,"Please select a student first.",false);return;} Execute(@"UPDATE Students SET ProgrammeId=@P,IntakeYear=@Year,IntakeSemester=@ISem,AdmissionDate=@Date,CurrentSemester=@CSem,Status=@Status WHERE StudentId=@Id",new SqlParameter("@P",ddlProgramme.SelectedValue),new SqlParameter("@Year",txtIntakeYear.Text),new SqlParameter("@ISem",txtIntakeSemester.Text),new SqlParameter("@Date",string.IsNullOrEmpty(txtAdmissionDate.Text)?(object)DBNull.Value:txtAdmissionDate.Text),new SqlParameter("@CSem",txtCurrentSemester.Text),new SqlParameter("@Status",ddlStatus.SelectedValue),new SqlParameter("@Id",hfStudentId.Value)); ClearForm(); BindGrid(); ShowMessage(lblMessage,"Admission updated.",true);}catch(Exception ex){ShowMessage(lblMessage,ex.Message,false);} }
        protected void btnClear_Click(object sender,EventArgs e){ ClearForm(); }
        private void ClearForm(){ hfStudentId.Value=""; txtStudent.Text=""; txtIntakeYear.Text=""; txtIntakeSemester.Text=""; txtCurrentSemester.Text=""; txtAdmissionDate.Text=""; ddlStatus.SelectedValue="Active"; if(ddlProgramme.Items.Count>0)ddlProgramme.SelectedIndex=0; }
    }
}
