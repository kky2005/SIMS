using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace SIMS.HeadOfProgramme
{
    public partial class HOPRegisterLecturer : HOPCrudBase
    {
        protected void Page_Load(object sender, EventArgs e) { EnsureAuthenticated(); if (!IsPostBack) BindGrid(); }        private void BindGrid()
        {
            string sql = @"SELECT l.*, u.FullName, u.Email, u.Phone
                           FROM Lecturers l
                           INNER JOIN Users u ON l.UserId = u.UserId
                           WHERE 1 = 1";
            System.Collections.Generic.List<SqlParameter> parameters = new System.Collections.Generic.List<SqlParameter>();
            if (!string.IsNullOrWhiteSpace(txtFilterLecturer.Text))
            {
                sql += " AND (u.FullName LIKE @Search OR u.Email LIKE @Search OR l.StaffNo LIKE @Search)";
                parameters.Add(new SqlParameter("@Search", "%" + txtFilterLecturer.Text.Trim() + "%"));
            }
            if (!string.IsNullOrWhiteSpace(txtFilterDepartment.Text))
            {
                sql += " AND ISNULL(l.Department, '') LIKE @Department";
                parameters.Add(new SqlParameter("@Department", "%" + txtFilterDepartment.Text.Trim() + "%"));
            }
            if (!string.IsNullOrEmpty(ddlFilterEmploymentStatus.SelectedValue))
            {
                sql += " AND l.EmploymentStatus = @Status";
                parameters.Add(new SqlParameter("@Status", ddlFilterEmploymentStatus.SelectedValue));
            }
            sql += " ORDER BY l.LecturerId DESC";
            gvLecturers.DataSource = GetData(sql, parameters.ToArray());
            gvLecturers.DataBind();
        }


        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtFullName.Text) || string.IsNullOrWhiteSpace(txtEmail.Text) || string.IsNullOrWhiteSpace(txtStaffNo.Text)) { ShowMessage(lblMessage, "Name, email and staff no required.", false); return; }
                using (SqlConnection con = new SqlConnection(ConnStr))
                {
                    con.Open(); SqlTransaction tx = con.BeginTransaction();
                    try
                    {
                        int userId; int lecturerId;
                        string newValue = "Name=" + txtFullName.Text.Trim() + "; Email=" + txtEmail.Text.Trim() + "; StaffNo=" + txtStaffNo.Text.Trim() + "; Department=" + txtDepartment.Text.Trim() + "; Status=" + ddlEmploymentStatus.SelectedValue;
                        if (string.IsNullOrEmpty(hfLecturerId.Value))
                        {
                            if (string.IsNullOrWhiteSpace(txtPassword.Text)) { ShowMessage(lblMessage, "Password required for new lecturer.", false); tx.Rollback(); return; }
                            SqlCommand u = new SqlCommand(@"INSERT INTO Users(RoleId,FullName,Email,PasswordHash,Phone,IsActive) OUTPUT INSERTED.UserId VALUES(@Role,@Name,@Email,@Pass,@Phone,1)", con, tx);
                            u.Parameters.AddWithValue("@Role", GetRoleId("Lecturer")); u.Parameters.AddWithValue("@Name", txtFullName.Text.Trim()); u.Parameters.AddWithValue("@Email", txtEmail.Text.Trim()); u.Parameters.AddWithValue("@Pass", HashPassword(txtPassword.Text)); u.Parameters.AddWithValue("@Phone", txtPhone.Text.Trim());
                            userId = (int)u.ExecuteScalar();
                            SqlCommand l = new SqlCommand(@"INSERT INTO Lecturers(UserId,StaffNo,Department,Specialisation,EmploymentStatus) OUTPUT INSERTED.LecturerId VALUES(@UserId,@Staff,@Dept,@Spec,@Status)", con, tx);
                            l.Parameters.AddWithValue("@UserId", userId); l.Parameters.AddWithValue("@Staff", txtStaffNo.Text.Trim()); l.Parameters.AddWithValue("@Dept", txtDepartment.Text.Trim()); l.Parameters.AddWithValue("@Spec", txtSpecialisation.Text.Trim()); l.Parameters.AddWithValue("@Status", ddlEmploymentStatus.SelectedValue);
                            lecturerId = (int)l.ExecuteScalar();
                            InsertAuditLog(con, tx, "Registered lecturer", "Lecturers", lecturerId, "New lecturer record", newValue + "; UserId=" + userId);
                        }
                        else
                        {
                            lecturerId = Convert.ToInt32(hfLecturerId.Value); userId = Convert.ToInt32(hfUserId.Value);
                            string oldValue = GetLecturerAuditValue(con, tx, lecturerId);
                            string userSql = string.IsNullOrWhiteSpace(txtPassword.Text) ? @"UPDATE Users SET FullName=@Name,Email=@Email,Phone=@Phone WHERE UserId=@UserId" : @"UPDATE Users SET FullName=@Name,Email=@Email,Phone=@Phone,PasswordHash=@Pass WHERE UserId=@UserId";
                            SqlCommand u = new SqlCommand(userSql, con, tx); u.Parameters.AddWithValue("@Name", txtFullName.Text.Trim()); u.Parameters.AddWithValue("@Email", txtEmail.Text.Trim()); u.Parameters.AddWithValue("@Phone", txtPhone.Text.Trim()); u.Parameters.AddWithValue("@UserId", userId); if (!string.IsNullOrWhiteSpace(txtPassword.Text)) u.Parameters.AddWithValue("@Pass", HashPassword(txtPassword.Text)); u.ExecuteNonQuery();
                            SqlCommand l = new SqlCommand(@"UPDATE Lecturers SET StaffNo=@Staff,Department=@Dept,Specialisation=@Spec,EmploymentStatus=@Status WHERE LecturerId=@Id", con, tx);
                            l.Parameters.AddWithValue("@Staff", txtStaffNo.Text.Trim()); l.Parameters.AddWithValue("@Dept", txtDepartment.Text.Trim()); l.Parameters.AddWithValue("@Spec", txtSpecialisation.Text.Trim()); l.Parameters.AddWithValue("@Status", ddlEmploymentStatus.SelectedValue); l.Parameters.AddWithValue("@Id", lecturerId); l.ExecuteNonQuery();
                            InsertAuditLog(con, tx, "Updated lecturer", "Lecturers", lecturerId, oldValue, newValue + "; UserId=" + userId);
                        }
                        tx.Commit(); ClearForm(); BindGrid(); ShowMessage(lblMessage, "Lecturer saved successfully.", true);
                    }
                    catch { tx.Rollback(); throw; }
                }
            }
            catch (Exception ex) { ShowMessage(lblMessage, ex.Message, false); }
        }

        protected void gvLecturers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                int id = Convert.ToInt32(e.CommandArgument);
                if (e.CommandName == "EditLecturer")
                {
                    DataTable dt = GetData(@"SELECT l.*,u.FullName,u.Email,u.Phone FROM Lecturers l INNER JOIN Users u ON l.UserId=u.UserId WHERE l.LecturerId=@Id", new SqlParameter("@Id", id));
                    if (dt.Rows.Count == 0) return; DataRow r = dt.Rows[0]; hfLecturerId.Value = id.ToString(); hfUserId.Value = r["UserId"].ToString();
                    txtFullName.Text = r["FullName"].ToString(); txtEmail.Text = r["Email"].ToString(); txtPhone.Text = r["Phone"].ToString(); txtStaffNo.Text = r["StaffNo"].ToString(); txtDepartment.Text = r["Department"].ToString(); txtSpecialisation.Text = r["Specialisation"].ToString(); ddlEmploymentStatus.SelectedValue = r["EmploymentStatus"].ToString();
                }
                else if (e.CommandName == "DeleteLecturer")
                {
                    using (SqlConnection con = new SqlConnection(ConnStr))
                    {
                        con.Open(); SqlTransaction tx = con.BeginTransaction();
                        try
                        {
                            string oldValue = GetLecturerAuditValue(con, tx, id);
                            SqlCommand getUser = new SqlCommand("SELECT UserId FROM Lecturers WHERE LecturerId=@Id", con, tx); getUser.Parameters.AddWithValue("@Id", id);
                            object userObj = getUser.ExecuteScalar(); if (userObj == null) { tx.Rollback(); return; }
                            int uid = Convert.ToInt32(userObj);

                            SqlCommand d1 = new SqlCommand("UPDATE Lecturers SET EmploymentStatus='Inactive' WHERE LecturerId=@Id", con, tx);
                            d1.Parameters.AddWithValue("@Id", id);
                            d1.ExecuteNonQuery();

                            SqlCommand d2 = new SqlCommand("UPDATE Users SET IsActive=0 WHERE UserId=@Uid", con, tx);
                            d2.Parameters.AddWithValue("@Uid", uid);
                            d2.ExecuteNonQuery();

                            InsertAuditLog(con, tx, "Deactivated lecturer", "Lecturers", id, oldValue, "EmploymentStatus=Inactive; User IsActive=0; Lecturer kept safely instead of deleted");
                            tx.Commit(); BindGrid(); ShowMessage(lblMessage, "Lecturer deactivated successfully.", true);
                        }
                        catch { tx.Rollback(); throw; }
                    }
                }
            }
            catch (Exception ex) { ShowMessage(lblMessage, "Deactivate failed. " + ex.Message, false); }
        }

        private string GetLecturerAuditValue(SqlConnection con, SqlTransaction tx, int lecturerId)
        {
            SqlCommand cmd = new SqlCommand(@"SELECT l.UserId, l.StaffNo, l.Department, l.Specialisation, l.EmploymentStatus, u.FullName, u.Email FROM Lecturers l INNER JOIN Users u ON l.UserId=u.UserId WHERE l.LecturerId=@Id", con, tx);
            cmd.Parameters.AddWithValue("@Id", lecturerId);
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                if (!r.Read()) return "Record not found";
                return "Name=" + r["FullName"] + "; Email=" + r["Email"] + "; StaffNo=" + r["StaffNo"] + "; Department=" + r["Department"] + "; Status=" + r["EmploymentStatus"] + "; UserId=" + r["UserId"];
            }
        }
        private void InsertAuditLog(SqlConnection con, SqlTransaction tx, string action, string tableAffected, int recordId, string oldValue, string newValue)
        {
            SqlCommand cmd = new SqlCommand(@"INSERT INTO AuditLogs(UserId,Action,TableAffected,RecordId,OldValue,NewValue,ActionDate) VALUES(@UserId,@Action,@TableAffected,@RecordId,@OldValue,@NewValue,SYSUTCDATETIME())", con, tx);
            cmd.Parameters.AddWithValue("@UserId", CurrentUserId); cmd.Parameters.AddWithValue("@Action", action); cmd.Parameters.AddWithValue("@TableAffected", tableAffected); cmd.Parameters.AddWithValue("@RecordId", recordId); cmd.Parameters.AddWithValue("@OldValue", oldValue); cmd.Parameters.AddWithValue("@NewValue", newValue); cmd.ExecuteNonQuery();
        }
        
        protected void btnFilter_Click(object sender, EventArgs e)
        {
            BindGrid();
        }

        protected void btnResetFilter_Click(object sender, EventArgs e)
        {
            txtFilterLecturer.Text = "";
            txtFilterDepartment.Text = "";
            ddlFilterEmploymentStatus.SelectedValue = "";
            BindGrid();
        }


        protected void btnClear_Click(object sender, EventArgs e) { ClearForm(); }
        private void ClearForm() { hfLecturerId.Value = ""; hfUserId.Value = ""; txtFullName.Text = ""; txtEmail.Text = ""; txtPhone.Text = ""; txtPassword.Text = ""; txtStaffNo.Text = ""; txtDepartment.Text = ""; txtSpecialisation.Text = ""; ddlEmploymentStatus.SelectedValue = "Active"; }
    }
}
