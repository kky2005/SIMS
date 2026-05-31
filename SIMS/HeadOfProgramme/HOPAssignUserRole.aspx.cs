using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace SIMS.HeadOfProgramme
{
    public partial class HOPAssignUserRole : HOPCrudBase
    {
        protected void Page_Load(object sender, EventArgs e){ EnsureAuthenticated(); if(!IsPostBack){ BindRoles(); BindGrid(); } }
        private void BindRoles(){ BindDropDown(ddlRole,"SELECT RoleId,RoleName FROM Roles ORDER BY RoleName","RoleName","RoleId"); }
        private void BindGrid(){ gvUsers.DataSource=GetData(@"SELECT u.UserId,u.FullName,u.Email,r.RoleName,CASE WHEN u.IsActive=1 THEN 'Yes' ELSE 'No' END AS IsActiveText FROM Users u INNER JOIN Roles r ON u.RoleId=r.RoleId ORDER BY u.UserId"); gvUsers.DataBind(); }
        protected void gvUsers_RowCommand(object sender,GridViewCommandEventArgs e){ int id=Convert.ToInt32(e.CommandArgument); DataTable dt=GetData("SELECT * FROM Users WHERE UserId=@Id",new SqlParameter("@Id",id)); if(dt.Rows.Count==0)return; DataRow r=dt.Rows[0]; hfUserId.Value=id.ToString(); txtSelectedUser.Text=r["FullName"]+" ("+r["Email"]+")"; ddlRole.SelectedValue=r["RoleId"].ToString(); ddlIsActive.SelectedValue=Convert.ToBoolean(r["IsActive"])?"1":"0"; }
        protected void btnSave_Click(object sender,EventArgs e){ try{ if(string.IsNullOrEmpty(hfUserId.Value)){ShowMessage(lblMessage,"Please select a user first.",false);return;} Execute("UPDATE Users SET RoleId=@Role,IsActive=@Active WHERE UserId=@Id",new SqlParameter("@Role",ddlRole.SelectedValue),new SqlParameter("@Active",ddlIsActive.SelectedValue),new SqlParameter("@Id",hfUserId.Value)); ClearForm(); BindGrid(); ShowMessage(lblMessage,"User role updated.",true);}catch(Exception ex){ShowMessage(lblMessage,ex.Message,false);} }
        protected void btnClear_Click(object sender,EventArgs e){ ClearForm(); }
        private void ClearForm(){ hfUserId.Value=""; txtSelectedUser.Text=""; if(ddlRole.Items.Count>0)ddlRole.SelectedIndex=0; ddlIsActive.SelectedValue="1"; }
    }
}
