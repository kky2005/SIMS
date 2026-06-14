using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace SIMS.HeadOfProgramme
{
    public partial class HOPRegisterStudent : HOPCrudBase
    {
        protected void Page_Load(object sender, EventArgs e) { EnsureAuthenticated(); if (!IsPostBack) { BindProgrammes(); BindFilterProgrammes(); BindGrid(); } }
        private void BindProgrammes() { BindDropDown(ddlProgramme, "SELECT ProgrammeId,ProgrammeName FROM Programmes ORDER BY ProgrammeName", "ProgrammeName", "ProgrammeId"); }        private void BindGrid()
        {
            string sql = @"SELECT s.*, u.FullName, u.Email, p.ProgrammeName
                           FROM Students s
                           INNER JOIN Users u ON s.UserId = u.UserId
                           INNER JOIN Programmes p ON s.ProgrammeId = p.ProgrammeId
                           WHERE 1 = 1";
            System.Collections.Generic.List<SqlParameter> parameters = new System.Collections.Generic.List<SqlParameter>();
            if (!string.IsNullOrWhiteSpace(txtFilterStudent.Text))
            {
                sql += " AND (s.StudentNo LIKE @Search OR u.FullName LIKE @Search OR u.Email LIKE @Search)";
                parameters.Add(new SqlParameter("@Search", "%" + txtFilterStudent.Text.Trim() + "%"));
            }
            if (!string.IsNullOrEmpty(ddlFilterProgramme.SelectedValue))
            {
                sql += " AND s.ProgrammeId = @ProgrammeId";
                parameters.Add(new SqlParameter("@ProgrammeId", ddlFilterProgramme.SelectedValue));
            }
            if (!string.IsNullOrEmpty(ddlFilterStatus.SelectedValue))
            {
                sql += " AND s.Status = @Status";
                parameters.Add(new SqlParameter("@Status", ddlFilterStatus.SelectedValue));
            }
            sql += " ORDER BY s.StudentId DESC";
            gvStudents.DataSource = GetData(sql, parameters.ToArray());
            gvStudents.DataBind();
        }


        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtFullName.Text) || string.IsNullOrWhiteSpace(txtEmail.Text) || string.IsNullOrWhiteSpace(txtStudentNo.Text)) { ShowMessage(lblMessage, "Name, email and student no required.", false); return; }
                using (SqlConnection con = new SqlConnection(ConnStr))
                {
                    con.Open(); SqlTransaction tx = con.BeginTransaction();
                    try
                    {
                        int userId; int studentId;
                        string newValue = BuildStudentAuditValue();
                        if (string.IsNullOrEmpty(hfStudentId.Value))
                        {
                            if (string.IsNullOrWhiteSpace(txtPassword.Text)) { ShowMessage(lblMessage, "Password required for new student.", false); tx.Rollback(); return; }
                            SqlCommand u = new SqlCommand(@"INSERT INTO Users(RoleId,FullName,Email,PasswordHash,Phone,IsActive) OUTPUT INSERTED.UserId VALUES(@Role,@Name,@Email,@Pass,@Phone,1)", con, tx);
                            u.Parameters.AddWithValue("@Role", GetRoleId("Student")); u.Parameters.AddWithValue("@Name", txtFullName.Text.Trim()); u.Parameters.AddWithValue("@Email", txtEmail.Text.Trim()); u.Parameters.AddWithValue("@Pass", HashPassword(txtPassword.Text)); u.Parameters.AddWithValue("@Phone", txtPhone.Text.Trim()); userId = (int)u.ExecuteScalar();
                            SqlCommand s = new SqlCommand(@"INSERT INTO Students(UserId,ProgrammeId,StudentNo,IntakeYear,IntakeSemester,AdmissionDate,CurrentSemester,Status) OUTPUT INSERTED.StudentId VALUES(@UserId,@P,@No,@Year,@ISem,@Date,@CSem,@Status)", con, tx);
                            AddStudentParams(s, userId); studentId = (int)s.ExecuteScalar();
                            InsertAuditLog(con, tx, "Registered student", "Students", studentId, "New student record", newValue + "; UserId=" + userId);
                        }
                        else
                        {
                            studentId = Convert.ToInt32(hfStudentId.Value); userId = Convert.ToInt32(hfUserId.Value);
                            string oldValue = GetStudentAuditValue(con, tx, studentId);
                            string userSql = string.IsNullOrWhiteSpace(txtPassword.Text) ? @"UPDATE Users SET FullName=@Name,Email=@Email,Phone=@Phone WHERE UserId=@UserId" : @"UPDATE Users SET FullName=@Name,Email=@Email,Phone=@Phone,PasswordHash=@Pass WHERE UserId=@UserId";
                            SqlCommand u = new SqlCommand(userSql, con, tx); u.Parameters.AddWithValue("@Name", txtFullName.Text.Trim()); u.Parameters.AddWithValue("@Email", txtEmail.Text.Trim()); u.Parameters.AddWithValue("@Phone", txtPhone.Text.Trim()); u.Parameters.AddWithValue("@UserId", userId); if (!string.IsNullOrWhiteSpace(txtPassword.Text)) u.Parameters.AddWithValue("@Pass", HashPassword(txtPassword.Text)); u.ExecuteNonQuery();
                            SqlCommand s = new SqlCommand(@"UPDATE Students SET ProgrammeId=@P,StudentNo=@No,IntakeYear=@Year,IntakeSemester=@ISem,AdmissionDate=@Date,CurrentSemester=@CSem,Status=@Status WHERE StudentId=@Id", con, tx);
                            AddStudentParams(s, userId); s.Parameters.AddWithValue("@Id", studentId); s.ExecuteNonQuery();
                            InsertAuditLog(con, tx, "Updated student", "Students", studentId, oldValue, newValue + "; UserId=" + userId);
                        }
                        tx.Commit(); ClearForm(); BindGrid(); ShowMessage(lblMessage, "Student saved successfully.", true);
                    }
                    catch { tx.Rollback(); throw; }
                }
            }
            catch (Exception ex) { ShowMessage(lblMessage, ex.Message, false); }
        }
        private void BindFilterProgrammes() { BindDropDown(ddlFilterProgramme, "SELECT ProgrammeId,ProgrammeName FROM Programmes ORDER BY ProgrammeName", "ProgrammeName", "ProgrammeId"); ddlFilterProgramme.Items.Insert(0, new ListItem("All Programmes", "")); }
        private void AddStudentParams(SqlCommand s, int userId) { s.Parameters.AddWithValue("@UserId", userId); s.Parameters.AddWithValue("@P", ddlProgramme.SelectedValue); s.Parameters.AddWithValue("@No", txtStudentNo.Text.Trim()); s.Parameters.AddWithValue("@Year", txtIntakeYear.Text); s.Parameters.AddWithValue("@ISem", txtIntakeSemester.Text); s.Parameters.AddWithValue("@Date", string.IsNullOrEmpty(txtAdmissionDate.Text) ? (object)DBNull.Value : txtAdmissionDate.Text); s.Parameters.AddWithValue("@CSem", txtCurrentSemester.Text); s.Parameters.AddWithValue("@Status", ddlStatus.SelectedValue); }

        protected void gvStudents_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                int id = Convert.ToInt32(e.CommandArgument);
                if (e.CommandName == "EditStudent")
                {
                    DataTable dt = GetData(@"SELECT s.*,u.FullName,u.Email,u.Phone FROM Students s INNER JOIN Users u ON s.UserId=u.UserId WHERE s.StudentId=@Id", new SqlParameter("@Id", id)); if (dt.Rows.Count == 0) return; DataRow r = dt.Rows[0];
                    hfStudentId.Value = id.ToString(); hfUserId.Value = r["UserId"].ToString(); txtFullName.Text = r["FullName"].ToString(); txtEmail.Text = r["Email"].ToString(); txtPhone.Text = r["Phone"].ToString(); txtStudentNo.Text = r["StudentNo"].ToString(); ddlProgramme.SelectedValue = r["ProgrammeId"].ToString(); txtIntakeYear.Text = r["IntakeYear"].ToString(); txtIntakeSemester.Text = r["IntakeSemester"].ToString(); txtCurrentSemester.Text = r["CurrentSemester"].ToString(); txtAdmissionDate.Text = r["AdmissionDate"] == DBNull.Value ? "" : Convert.ToDateTime(r["AdmissionDate"]).ToString("yyyy-MM-dd"); ddlStatus.SelectedValue = r["Status"].ToString();
                }
                else if (e.CommandName == "DeleteStudent")
                {
                    using (SqlConnection con = new SqlConnection(ConnStr))
                    {
                        con.Open(); SqlTransaction tx = con.BeginTransaction();
                        try
                        {
                            string oldValue = GetStudentAuditValue(con, tx, id);
                            SqlCommand getUser = new SqlCommand("SELECT UserId FROM Students WHERE StudentId=@Id", con, tx); getUser.Parameters.AddWithValue("@Id", id);
                            object userObj = getUser.ExecuteScalar(); if (userObj == null) { tx.Rollback(); return; }
                            int uid = Convert.ToInt32(userObj);

                            SqlCommand d1 = new SqlCommand("UPDATE Students SET Status='Inactive' WHERE StudentId=@Id", con, tx);
                            d1.Parameters.AddWithValue("@Id", id);
                            d1.ExecuteNonQuery();

                            SqlCommand d2 = new SqlCommand("UPDATE Users SET IsActive=0 WHERE UserId=@Uid", con, tx);
                            d2.Parameters.AddWithValue("@Uid", uid);
                            d2.ExecuteNonQuery();

                            InsertAuditLog(con, tx, "Deactivated student", "Students", id, oldValue, "Status=Inactive; User IsActive=0; Student kept safely instead of deleted");
                            tx.Commit(); BindGrid(); ShowMessage(lblMessage, "Student deactivated successfully.", true);
                        }
                        catch { tx.Rollback(); throw; }
                    }
                }
            }
            catch (Exception ex) { ShowMessage(lblMessage, "Deactivate failed. " + ex.Message, false); }
        }

        private string BuildStudentAuditValue()
        {
            return "Name=" + txtFullName.Text.Trim() + "; Email=" + txtEmail.Text.Trim() + "; StudentNo=" + txtStudentNo.Text.Trim() + "; ProgrammeId=" + ddlProgramme.SelectedValue + "; IntakeYear=" + txtIntakeYear.Text.Trim() + "; IntakeSemester=" + txtIntakeSemester.Text.Trim() + "; CurrentSemester=" + txtCurrentSemester.Text.Trim() + "; Status=" + ddlStatus.SelectedValue;
        }
        private string GetStudentAuditValue(SqlConnection con, SqlTransaction tx, int studentId)
        {
            SqlCommand cmd = new SqlCommand(@"SELECT s.UserId, s.ProgrammeId, s.StudentNo, s.IntakeYear, s.IntakeSemester, s.CurrentSemester, s.Status, u.FullName, u.Email FROM Students s INNER JOIN Users u ON s.UserId=u.UserId WHERE s.StudentId=@Id", con, tx);
            cmd.Parameters.AddWithValue("@Id", studentId);
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                if (!r.Read()) return "Record not found";
                return "Name=" + r["FullName"] + "; Email=" + r["Email"] + "; StudentNo=" + r["StudentNo"] + "; ProgrammeId=" + r["ProgrammeId"] + "; IntakeYear=" + r["IntakeYear"] + "; IntakeSemester=" + r["IntakeSemester"] + "; CurrentSemester=" + r["CurrentSemester"] + "; Status=" + r["Status"] + "; UserId=" + r["UserId"];
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
            txtFilterStudent.Text = "";
            ddlFilterProgramme.SelectedValue = "";
            ddlFilterStatus.SelectedValue = "";
            BindGrid();
        }


        protected void btnClear_Click(object sender, EventArgs e) { ClearForm(); }
        private void ClearForm() { hfStudentId.Value = ""; hfUserId.Value = ""; txtFullName.Text = ""; txtEmail.Text = ""; txtPhone.Text = ""; txtPassword.Text = ""; txtStudentNo.Text = ""; txtIntakeYear.Text = DateTime.Now.Year.ToString(); txtIntakeSemester.Text = "1"; txtCurrentSemester.Text = "1"; txtAdmissionDate.Text = DateTime.Now.ToString("yyyy-MM-dd"); ddlStatus.SelectedValue = "Active"; if (ddlProgramme.Items.Count > 0) ddlProgramme.SelectedIndex = 0; }
    }
}
