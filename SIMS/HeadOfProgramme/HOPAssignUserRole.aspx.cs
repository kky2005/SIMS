using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace SIMS.HeadOfProgramme
{
    public partial class HOPAssignUserRole : HOPCrudBase
    {
        protected void Page_Load(object sender, EventArgs e) { EnsureAuthenticated(); if (!IsPostBack) { BindRoles(); BindFilterRoles(); BindGrid(); } }
        private void BindRoles() { BindDropDown(ddlRole, "SELECT RoleId,RoleName FROM Roles ORDER BY RoleName", "RoleName", "RoleId"); }        private void BindGrid()
        {
            string sql = @"SELECT u.UserId, u.FullName, u.Email, r.RoleName, CASE WHEN u.IsActive=1 THEN 'Yes' ELSE 'No' END AS IsActiveText
                           FROM Users u
                           INNER JOIN Roles r ON u.RoleId = r.RoleId
                           WHERE 1 = 1";
            System.Collections.Generic.List<SqlParameter> parameters = new System.Collections.Generic.List<SqlParameter>();
            if (!string.IsNullOrWhiteSpace(txtFilterUser.Text))
            {
                sql += " AND (u.FullName LIKE @Search OR u.Email LIKE @Search)";
                parameters.Add(new SqlParameter("@Search", "%" + txtFilterUser.Text.Trim() + "%"));
            }
            if (!string.IsNullOrEmpty(ddlFilterRole.SelectedValue))
            {
                sql += " AND u.RoleId = @RoleId";
                parameters.Add(new SqlParameter("@RoleId", ddlFilterRole.SelectedValue));
            }
            if (!string.IsNullOrEmpty(ddlFilterUserActive.SelectedValue))
            {
                sql += " AND u.IsActive = @IsActive";
                parameters.Add(new SqlParameter("@IsActive", ddlFilterUserActive.SelectedValue));
            }
            sql += " ORDER BY u.UserId";
            gvUsers.DataSource = GetData(sql, parameters.ToArray());
            gvUsers.DataBind();
        }

        protected void gvUsers_RowCommand(object sender, GridViewCommandEventArgs e) { int id = Convert.ToInt32(e.CommandArgument); DataTable dt = GetData("SELECT * FROM Users WHERE UserId=@Id", new SqlParameter("@Id", id)); if (dt.Rows.Count == 0) return; DataRow r = dt.Rows[0]; hfUserId.Value = id.ToString(); txtSelectedUser.Text = r["FullName"] + " (" + r["Email"] + ")"; ddlRole.SelectedValue = r["RoleId"].ToString(); ddlIsActive.SelectedValue = Convert.ToBoolean(r["IsActive"]) ? "1" : "0"; }
        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(hfUserId.Value)) { ShowMessage(lblMessage, "Please select a user first.", false); return; }
                int id = Convert.ToInt32(hfUserId.Value);
                string oldValue = GetUserRoleAuditValue(id);
                Execute("UPDATE Users SET RoleId=@Role,IsActive=@Active WHERE UserId=@Id", new SqlParameter("@Role", ddlRole.SelectedValue), new SqlParameter("@Active", ddlIsActive.SelectedValue), new SqlParameter("@Id", id));
                string newValue = GetUserRoleAuditValue(id);
                InsertAuditLog("Updated user role", "Users", id, oldValue, newValue);
                ClearForm(); BindGrid(); ShowMessage(lblMessage, "User role updated.", true);
            }
            catch (Exception ex) { ShowMessage(lblMessage, ex.Message, false); }
        }
        private void BindFilterRoles() { BindDropDown(ddlFilterRole, "SELECT RoleId,RoleName FROM Roles ORDER BY RoleName", "RoleName", "RoleId"); ddlFilterRole.Items.Insert(0, new ListItem("All Roles", "")); }
        private string GetUserRoleAuditValue(int id)
        {
            DataTable dt = GetData(@"SELECT u.FullName, u.Email, u.RoleId, r.RoleName, u.IsActive FROM Users u LEFT JOIN Roles r ON r.RoleId=u.RoleId WHERE u.UserId=@Id", new SqlParameter("@Id", id));
            if (dt.Rows.Count == 0) return "Record not found";
            DataRow r = dt.Rows[0];
            return "Name=" + r["FullName"] + "; Email=" + r["Email"] + "; RoleId=" + r["RoleId"] + "; Role=" + r["RoleName"] + "; IsActive=" + r["IsActive"];
        }
        private void InsertAuditLog(string action, string tableAffected, int recordId, string oldValue, string newValue)
        {
            Execute(@"INSERT INTO AuditLogs(UserId, Action, TableAffected, RecordId, OldValue, NewValue, ActionDate) VALUES(@UserId,@Action,@TableAffected,@RecordId,@OldValue,@NewValue,SYSUTCDATETIME())",
                new SqlParameter("@UserId", CurrentUserId), new SqlParameter("@Action", action), new SqlParameter("@TableAffected", tableAffected), new SqlParameter("@RecordId", recordId), new SqlParameter("@OldValue", oldValue), new SqlParameter("@NewValue", newValue));
        }
        
        protected void btnFilter_Click(object sender, EventArgs e)
        {
            BindGrid();
        }

        protected void btnResetFilter_Click(object sender, EventArgs e)
        {
            txtFilterUser.Text = "";
            ddlFilterRole.SelectedValue = "";
            ddlFilterUserActive.SelectedValue = "";
            BindGrid();
        }


        protected void btnClear_Click(object sender, EventArgs e) { ClearForm(); }
        private void ClearForm() { hfUserId.Value = ""; txtSelectedUser.Text = ""; if (ddlRole.Items.Count > 0) ddlRole.SelectedIndex = 0; ddlIsActive.SelectedValue = "1"; }
    }
}
