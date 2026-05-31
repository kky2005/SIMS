using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace SIMS.HeadOfProgramme
{
    public partial class HOPManageCourses : HOPCrudBase
    {
        protected void Page_Load(object sender, EventArgs e) { EnsureAuthenticated(); if (!IsPostBack) { BindProgrammes(); BindGrid(); } }
        private void BindProgrammes() { BindDropDown(ddlProgramme, "SELECT ProgrammeId, ProgrammeName FROM Programmes ORDER BY ProgrammeName", "ProgrammeName", "ProgrammeId"); }
        private void BindGrid()
        {
            gvCourses.DataSource = GetData(@"SELECT c.*, p.ProgrammeName, CASE WHEN c.IsActive=1 THEN 'Yes' ELSE 'No' END AS IsActiveText
                FROM Courses c INNER JOIN Programmes p ON c.ProgrammeId=p.ProgrammeId ORDER BY c.CourseId DESC");
            gvCourses.DataBind();
        }
        protected void btnSave_Click(object sender, EventArgs e)
        {
            try {
                if (string.IsNullOrWhiteSpace(txtCourseCode.Text) || string.IsNullOrWhiteSpace(txtCourseName.Text)) { ShowMessage(lblMessage,"Code and name required.",false); return; }
                if (string.IsNullOrEmpty(hfCourseId.Value))
                    Execute(@"INSERT INTO Courses(ProgrammeId,CourseCode,CourseName,CreditHours,Semester,IsActive) VALUES(@P,@Code,@Name,@Credit,@Sem,@Active)",
                        new SqlParameter("@P", ddlProgramme.SelectedValue), new SqlParameter("@Code", txtCourseCode.Text.Trim()), new SqlParameter("@Name", txtCourseName.Text.Trim()),
                        new SqlParameter("@Credit", txtCreditHours.Text), new SqlParameter("@Sem", txtSemester.Text), new SqlParameter("@Active", ddlIsActive.SelectedValue));
                else
                    Execute(@"UPDATE Courses SET ProgrammeId=@P,CourseCode=@Code,CourseName=@Name,CreditHours=@Credit,Semester=@Sem,IsActive=@Active WHERE CourseId=@Id",
                        new SqlParameter("@P", ddlProgramme.SelectedValue), new SqlParameter("@Code", txtCourseCode.Text.Trim()), new SqlParameter("@Name", txtCourseName.Text.Trim()),
                        new SqlParameter("@Credit", txtCreditHours.Text), new SqlParameter("@Sem", txtSemester.Text), new SqlParameter("@Active", ddlIsActive.SelectedValue), new SqlParameter("@Id", hfCourseId.Value));
                ClearForm(); BindGrid(); ShowMessage(lblMessage,"Course saved successfully.",true);
            } catch (Exception ex) { ShowMessage(lblMessage, ex.Message, false); }
        }
        protected void gvCourses_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try {
                int id=Convert.ToInt32(e.CommandArgument);
                if(e.CommandName=="EditCourse") {
                    DataTable dt=GetData("SELECT * FROM Courses WHERE CourseId=@Id", new SqlParameter("@Id",id));
                    if(dt.Rows.Count==0)return; DataRow r=dt.Rows[0];
                    hfCourseId.Value=id.ToString(); ddlProgramme.SelectedValue=r["ProgrammeId"].ToString(); txtCourseCode.Text=r["CourseCode"].ToString(); txtCourseName.Text=r["CourseName"].ToString();
                    txtCreditHours.Text=r["CreditHours"].ToString(); txtSemester.Text=r["Semester"].ToString(); ddlIsActive.SelectedValue=Convert.ToBoolean(r["IsActive"])?"1":"0";
                } else if(e.CommandName=="DeleteCourse") {
                    Execute("DELETE FROM Courses WHERE CourseId=@Id", new SqlParameter("@Id",id)); BindGrid(); ShowMessage(lblMessage,"Course deleted.",true);
                }
            } catch(Exception ex){ ShowMessage(lblMessage,"Delete failed. This course may be used by enrolments/assessments. " + ex.Message,false); }
        }
        protected void btnClear_Click(object sender, EventArgs e){ ClearForm(); }
        private void ClearForm(){ hfCourseId.Value=""; txtCourseCode.Text=""; txtCourseName.Text=""; txtCreditHours.Text="3"; txtSemester.Text=""; ddlIsActive.SelectedValue="1"; if(ddlProgramme.Items.Count>0) ddlProgramme.SelectedIndex=0; }
    }
}
