using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace SIMS.HeadOfProgramme
{
    public partial class HOPManageProgrammes : HOPCrudBase
    {
        protected void Page_Load(object sender, EventArgs e) { EnsureAuthenticated(); if (!IsPostBack) BindGrid(); }        private void BindGrid()
        {
            string sql = @"
                SELECT
                    p.ProgrammeId, p.ProgrammeCode, p.ProgrammeName, p.DurationYears, p.Description, p.IsActive,
                    CASE WHEN p.IsActive = 1 THEN 'Yes' ELSE 'No' END AS IsActiveText,
                    ISNULL(courseStats.CourseCount, 0) AS CourseCount,
                    ISNULL(courseStats.CoursesOffered, '-') AS CoursesOffered,
                    ISNULL(lecturerStats.LecturerCount, 0) AS LecturerCount,
                    ISNULL(lecturerStats.TeachingLecturers, '-') AS TeachingLecturers,
                    ISNULL(studentStats.StudentCount, 0) AS StudentCount,
                    ISNULL(studentStats.ActiveStudentCount, 0) AS ActiveStudentCount
                FROM Programmes p
                OUTER APPLY
                (
                    SELECT COUNT(*) AS CourseCount,
                        STUFF((SELECT ', ' + c2.CourseCode FROM Courses c2 WHERE c2.ProgrammeId = p.ProgrammeId ORDER BY c2.CourseCode FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS CoursesOffered
                    FROM Courses c WHERE c.ProgrammeId = p.ProgrammeId
                ) courseStats
                OUTER APPLY
                (
                    SELECT COUNT(DISTINCT l.LecturerId) AS LecturerCount,
                        STUFF((SELECT DISTINCT ', ' + u2.FullName FROM Courses c2 INNER JOIN CourseAssignments ca2 ON ca2.CourseId = c2.CourseId INNER JOIN Lecturers l2 ON l2.LecturerId = ca2.LecturerId INNER JOIN Users u2 ON u2.UserId = l2.UserId WHERE c2.ProgrammeId = p.ProgrammeId FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS TeachingLecturers
                    FROM Courses c INNER JOIN CourseAssignments ca ON ca.CourseId = c.CourseId INNER JOIN Lecturers l ON l.LecturerId = ca.LecturerId WHERE c.ProgrammeId = p.ProgrammeId
                ) lecturerStats
                OUTER APPLY
                (
                    SELECT COUNT(*) AS StudentCount, SUM(CASE WHEN s.Status = 'Active' THEN 1 ELSE 0 END) AS ActiveStudentCount
                    FROM Students s WHERE s.ProgrammeId = p.ProgrammeId
                ) studentStats
                WHERE 1 = 1";

            System.Collections.Generic.List<SqlParameter> parameters = new System.Collections.Generic.List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(txtFilterProgramme.Text))
            {
                sql += " AND (p.ProgrammeCode LIKE @Search OR p.ProgrammeName LIKE @Search OR ISNULL(p.Description, '') LIKE @Search)";
                parameters.Add(new SqlParameter("@Search", "%" + txtFilterProgramme.Text.Trim() + "%"));
            }

            if (!string.IsNullOrEmpty(ddlFilterActive.SelectedValue))
            {
                sql += " AND p.IsActive = @IsActive";
                parameters.Add(new SqlParameter("@IsActive", ddlFilterActive.SelectedValue));
            }

            sql += " ORDER BY p.ProgrammeId DESC";
            gvProgrammes.DataSource = GetData(sql, parameters.ToArray());
            gvProgrammes.DataBind();
        }


        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtProgrammeCode.Text) || string.IsNullOrWhiteSpace(txtProgrammeName.Text)) { ShowMessage(lblMessage, "Code and name required.", false); return; }

                string newValue = "Code=" + txtProgrammeCode.Text.Trim() + "; Name=" + txtProgrammeName.Text.Trim() + "; DurationYears=" + txtDurationYears.Text.Trim() + "; IsActive=" + ddlIsActive.SelectedValue;

                if (string.IsNullOrEmpty(hfProgrammeId.Value))
                {
                    int newId = Convert.ToInt32(ExecuteScalar(@"INSERT INTO Programmes(ProgrammeCode,ProgrammeName,DurationYears,Description,IsActive)
                        OUTPUT INSERTED.ProgrammeId VALUES(@Code,@Name,@Years,@Desc,@Active)",
                        new SqlParameter("@Code", txtProgrammeCode.Text.Trim()), new SqlParameter("@Name", txtProgrammeName.Text.Trim()),
                        new SqlParameter("@Years", txtDurationYears.Text), new SqlParameter("@Desc", txtDescription.Text.Trim()),
                        new SqlParameter("@Active", ddlIsActive.SelectedValue)));

                    InsertAuditLog("Created programme", "Programmes", newId, "New programme record", newValue);
                }
                else
                {
                    int id = Convert.ToInt32(hfProgrammeId.Value);
                    string oldValue = GetProgrammeAuditValue(id);

                    Execute(@"UPDATE Programmes SET ProgrammeCode=@Code,ProgrammeName=@Name,DurationYears=@Years,Description=@Desc,IsActive=@Active WHERE ProgrammeId=@Id",
                        new SqlParameter("@Code", txtProgrammeCode.Text.Trim()), new SqlParameter("@Name", txtProgrammeName.Text.Trim()),
                        new SqlParameter("@Years", txtDurationYears.Text), new SqlParameter("@Desc", txtDescription.Text.Trim()),
                        new SqlParameter("@Active", ddlIsActive.SelectedValue), new SqlParameter("@Id", id));

                    InsertAuditLog("Updated programme", "Programmes", id, oldValue, newValue);
                }

                ClearForm(); BindGrid(); ShowMessage(lblMessage, "Programme saved successfully.", true);
            }
            catch (Exception ex) { ShowMessage(lblMessage, ex.Message, false); }
        }

        protected void gvProgrammes_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                int id = Convert.ToInt32(e.CommandArgument);
                if (e.CommandName == "EditProgramme")
                {
                    DataTable dt = GetData("SELECT * FROM Programmes WHERE ProgrammeId=@Id", new SqlParameter("@Id", id));
                    if (dt.Rows.Count == 0) return; DataRow r = dt.Rows[0];
                    hfProgrammeId.Value = id.ToString(); txtProgrammeCode.Text = r["ProgrammeCode"].ToString(); txtProgrammeName.Text = r["ProgrammeName"].ToString();
                    txtDurationYears.Text = r["DurationYears"].ToString(); txtDescription.Text = r["Description"].ToString(); ddlIsActive.SelectedValue = Convert.ToBoolean(r["IsActive"]) ? "1" : "0";
                }
                else if (e.CommandName == "DeleteProgramme")
                {
                    string oldValue = GetProgrammeAuditValue(id);

                    // Soft delete: keep the programme record because Courses, Students,
                    // Admissions and other tables may still reference ProgrammeId.
                    Execute("UPDATE Programmes SET IsActive = 0 WHERE ProgrammeId = @Id", new SqlParameter("@Id", id));

                    InsertAuditLog(
                        "Deactivated programme",
                        "Programmes",
                        id,
                        oldValue,
                        "IsActive=False; Programme hidden/deactivated instead of deleted because related records may exist"
                    );

                    BindGrid();
                    ShowMessage(lblMessage, "Programme deactivated successfully. Existing courses and students are kept safely.", true);
                }
            }
            catch (Exception ex) { ShowMessage(lblMessage, "Action failed. " + ex.Message, false); }
        }

        private string GetProgrammeAuditValue(int id)
        {
            DataTable dt = GetData("SELECT ProgrammeCode, ProgrammeName, DurationYears, IsActive FROM Programmes WHERE ProgrammeId=@Id", new SqlParameter("@Id", id));
            if (dt.Rows.Count == 0) return "Record not found";
            DataRow r = dt.Rows[0];
            return "Code=" + r["ProgrammeCode"] + "; Name=" + r["ProgrammeName"] + "; DurationYears=" + r["DurationYears"] + "; IsActive=" + r["IsActive"];
        }

        private object ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection con = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                con.Open();
                return cmd.ExecuteScalar();
            }
        }

        private void InsertAuditLog(string action, string tableAffected, int recordId, string oldValue, string newValue)
        {
            Execute(@"INSERT INTO AuditLogs(UserId, Action, TableAffected, RecordId, OldValue, NewValue, ActionDate)
                      VALUES(@UserId, @Action, @TableAffected, @RecordId, @OldValue, @NewValue, SYSUTCDATETIME())",
                new SqlParameter("@UserId", CurrentUserId), new SqlParameter("@Action", action), new SqlParameter("@TableAffected", tableAffected),
                new SqlParameter("@RecordId", recordId), new SqlParameter("@OldValue", oldValue), new SqlParameter("@NewValue", newValue));
        }

        
        protected void btnFilter_Click(object sender, EventArgs e)
        {
            BindGrid();
        }

        protected void btnResetFilter_Click(object sender, EventArgs e)
        {
            txtFilterProgramme.Text = "";
            ddlFilterActive.SelectedValue = "";
            BindGrid();
        }


        protected void btnClear_Click(object sender, EventArgs e) { ClearForm(); }
        private void ClearForm() { hfProgrammeId.Value = ""; txtProgrammeCode.Text = ""; txtProgrammeName.Text = ""; txtDurationYears.Text = "3"; txtDescription.Text = ""; ddlIsActive.SelectedValue = "1"; }
    }
}
