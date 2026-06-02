using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace SIMS.HeadOfProgramme
{
    public partial class HOPManageEnrolments : HOPCrudBase
    {
        protected void Page_Load(object sender, EventArgs e){ EnsureAuthenticated(); if(!IsPostBack){ BindStudents(); BindCourses(); BindGrid(); txtAcademicYear.Text=DateTime.Now.Year.ToString(); txtSemester.Text=GetCurrentSemester().ToString(); } }
        private void BindStudents(){ BindDropDown(ddlStudent,@"SELECT s.StudentId, s.StudentNo + ' - ' + u.FullName AS StudentName FROM Students s INNER JOIN Users u ON s.UserId=u.UserId ORDER BY s.StudentNo","StudentName","StudentId"); }
        private void BindCourses(){ BindDropDown(ddlCourse,@"SELECT CourseId, CourseCode + ' - ' + CourseName AS CourseTitle FROM Courses WHERE IsActive=1 ORDER BY CourseCode","CourseTitle","CourseId"); }
        private void BindGrid(){ gvEnrolments.DataSource=GetData(@"SELECT e.*,s.StudentNo,u.FullName,c.CourseCode,c.CourseName FROM Enrolments e INNER JOIN Students s ON e.StudentId=s.StudentId INNER JOIN Users u ON s.UserId=u.UserId INNER JOIN Courses c ON e.CourseId=c.CourseId ORDER BY e.EnrolmentId DESC"); gvEnrolments.DataBind(); }
        protected void btnSave_Click(object sender,EventArgs e){ try{ if(string.IsNullOrEmpty(hfEnrolmentId.Value)) Execute(@"INSERT INTO Enrolments(StudentId,CourseId,AcademicYear,Semester,Status,EnrolledAt,DroppedAt) VALUES(@S,@C,@Y,@Sem,@Status,SYSUTCDATETIME(),CASE WHEN @Status='Dropped' THEN SYSUTCDATETIME() ELSE NULL END)",Params(null)); else Execute(@"UPDATE Enrolments SET StudentId=@S,CourseId=@C,AcademicYear=@Y,Semester=@Sem,Status=@Status,DroppedAt=CASE WHEN @Status='Dropped' THEN ISNULL(DroppedAt,SYSUTCDATETIME()) ELSE NULL END WHERE EnrolmentId=@Id",Params(hfEnrolmentId.Value)); ClearForm(); BindGrid(); ShowMessage(lblMessage,"Enrolment saved.",true);}catch(Exception ex){ShowMessage(lblMessage,ex.Message,false);} }
        private SqlParameter[] Params(string id){ var list=new System.Collections.Generic.List<SqlParameter>{new SqlParameter("@S",ddlStudent.SelectedValue),new SqlParameter("@C",ddlCourse.SelectedValue),new SqlParameter("@Y",txtAcademicYear.Text),new SqlParameter("@Sem",txtSemester.Text),new SqlParameter("@Status",ddlStatus.SelectedValue)}; if(id!=null)list.Add(new SqlParameter("@Id",id)); return list.ToArray(); }
        protected void gvEnrolments_RowCommand(object sender,GridViewCommandEventArgs e){ try{ int id=Convert.ToInt32(e.CommandArgument); if(e.CommandName=="EditEnrolment"){ DataTable dt=GetData("SELECT * FROM Enrolments WHERE EnrolmentId=@Id",new SqlParameter("@Id",id)); if(dt.Rows.Count==0)return; DataRow r=dt.Rows[0]; hfEnrolmentId.Value=id.ToString(); ddlStudent.SelectedValue=r["StudentId"].ToString(); ddlCourse.SelectedValue=r["CourseId"].ToString(); txtAcademicYear.Text=r["AcademicYear"].ToString(); txtSemester.Text=r["Semester"].ToString(); ddlStatus.SelectedValue=r["Status"].ToString(); } else if(e.CommandName=="DropEnrolment"){ Execute("UPDATE Enrolments SET Status='Dropped',DroppedAt=SYSUTCDATETIME() WHERE EnrolmentId=@Id",new SqlParameter("@Id",id)); BindGrid(); ShowMessage(lblMessage,"Enrolment dropped.",true); } else if(e.CommandName=="DeleteEnrolment"){ Execute("DELETE FROM Enrolments WHERE EnrolmentId=@Id",new SqlParameter("@Id",id)); BindGrid(); ShowMessage(lblMessage,"Enrolment deleted.",true); } }catch(Exception ex){ShowMessage(lblMessage,ex.Message,false);} }
        protected void btnClear_Click(object sender,EventArgs e){ ClearForm(); }
        private void ClearForm(){ hfEnrolmentId.Value=""; if(ddlStudent.Items.Count>0)ddlStudent.SelectedIndex=0; if(ddlCourse.Items.Count>0)ddlCourse.SelectedIndex=0; txtAcademicYear.Text=DateTime.Now.Year.ToString(); txtSemester.Text=GetCurrentSemester().ToString(); ddlStatus.SelectedValue="Active"; }
    }
}
