using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace SIMS.HeadOfProgramme
{
    public partial class HOPArchivedAdmissions : HOPBase
    {
        private readonly string connStr = ConfigurationManager.ConnectionStrings["SIMS_DB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            EnsureAuthenticated();

            if (!IsPostBack)
            {
                LoadProgrammeFilter();
                LoadArchivedAdmissions();
            }
        }

        private void LoadProgrammeFilter()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"
                    SELECT ProgrammeId, ProgrammeName
                    FROM Programmes
                    ORDER BY ProgrammeName";

                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ddlFilterProgramme.DataSource = dt;
                ddlFilterProgramme.DataTextField = "ProgrammeName";
                ddlFilterProgramme.DataValueField = "ProgrammeId";
                ddlFilterProgramme.DataBind();
                ddlFilterProgramme.Items.Insert(0, new ListItem("All Programmes", ""));
            }
        }

        private void LoadArchivedAdmissions()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"
                    SELECT
                        a.AdmissionId,
                        ISNULL(s.StudentNo, '-') AS StudentNo,
                        a.FullName AS StudentName,
                        a.FullName,
                        a.NationalId,
                        a.Nationality,
                        a.PhoneNumber,
                        a.PreviousInstitution,
                        a.HighestQualification,
                        a.PreviousCGPA,
                        ap.Email AS ApplicantEmail,
                        p.ProgrammeName,
                        a.IntakeYear,
                        a.IntakeSemester,
                        'Archived' AS Status,
                        a.RequestedAt,
                        a.AdmittedAt,
                        a.RejectedAt,
                        ISNULL(lastLog.Action, '-') AS LastAction,
                        ISNULL(adminUser.FullName, '-') AS LastActionBy,
                        lastLog.ActionDate AS LastActionDate
                    FROM Admissions a
                    LEFT JOIN Applicants ap ON ap.UserId = a.UserId
                    LEFT JOIN Students s ON s.StudentId = a.StudentId
                    INNER JOIN Programmes p ON p.ProgrammeId = a.ProgrammeId
                    OUTER APPLY
                    (
                        SELECT TOP 1 al.UserId, al.Action, al.ActionDate
                        FROM AuditLogs al
                        WHERE al.TableAffected = 'Admissions'
                          AND al.RecordId = a.AdmissionId
                        ORDER BY al.ActionDate DESC
                    ) lastLog
                    OUTER APPLY
                    (
                        SELECT TOP 1 al.Action, al.ActionDate
                        FROM AuditLogs al
                        WHERE al.TableAffected = 'Admissions'
                          AND al.RecordId = a.AdmissionId
                          AND al.Action IN ('Archived admission record', 'Restored archived admission record')
                        ORDER BY al.ActionDate DESC
                    ) archiveLog
                    LEFT JOIN Users adminUser ON adminUser.UserId = lastLog.UserId
                    WHERE a.Status = 'Admitted'
                      AND archiveLog.Action = 'Archived admission record'";

                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                AddCommonFilters(ref sql, cmd);
                sql += " ORDER BY archiveLog.ActionDate DESC, a.AdmittedAt DESC, a.RequestedAt DESC";
                cmd.CommandText = sql;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvArchived.DataSource = dt;
                gvArchived.DataBind();
                lblArchivedCount.Text = "(" + dt.Rows.Count + ")";
            }
        }

        private void AddCommonFilters(ref string sql, SqlCommand cmd)
        {
            if (!string.IsNullOrWhiteSpace(txtSearchStudent.Text))
            {
                sql += @" AND (
                            ISNULL(s.StudentNo, '') LIKE @StudentSearch
                            OR ISNULL(a.FullName, '') LIKE @StudentSearch
                            OR ISNULL(a.NationalId, '') LIKE @StudentSearch
                            OR ISNULL(ap.Email, '') LIKE @StudentSearch
                          )";
                cmd.Parameters.AddWithValue("@StudentSearch", "%" + txtSearchStudent.Text.Trim() + "%");
            }

            if (!string.IsNullOrEmpty(ddlFilterProgramme.SelectedValue))
            {
                sql += " AND a.ProgrammeId = @ProgrammeId";
                cmd.Parameters.AddWithValue("@ProgrammeId", ddlFilterProgramme.SelectedValue);
            }

            if (!string.IsNullOrWhiteSpace(txtFromDate.Text))
            {
                DateTime fromDate;
                if (DateTime.TryParse(txtFromDate.Text, out fromDate))
                {
                    sql += " AND CAST(a.RequestedAt AS date) >= @FromDate";
                    cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                }
            }

            if (!string.IsNullOrWhiteSpace(txtToDate.Text))
            {
                DateTime toDate;
                if (DateTime.TryParse(txtToDate.Text, out toDate))
                {
                    sql += " AND CAST(a.RequestedAt AS date) <= @ToDate";
                    cmd.Parameters.AddWithValue("@ToDate", toDate.Date);
                }
            }
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            LoadArchivedAdmissions();
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtSearchStudent.Text = "";
            ddlFilterProgramme.SelectedIndex = 0;
            txtFromDate.Text = "";
            txtToDate.Text = "";
            LoadArchivedAdmissions();
        }

        protected void gvArchived_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "RestoreAdmission")
                return;

            int admissionId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out admissionId))
                return;

            RestoreAdmission(admissionId);
            LoadArchivedAdmissions();
        }

        protected void btnRestoreSelected_Click(object sender, EventArgs e)
        {
            List<int> selectedIds = GetSelectedIds(gvArchived, "chkSelectArchived");

            if (selectedIds.Count == 0)
            {
                ShowMessage("Please select at least one archived admission to restore.", false);
                return;
            }

            int restoredCount = 0;
            foreach (int admissionId in selectedIds)
            {
                RestoreAdmission(admissionId);
                restoredCount++;
            }

            LoadArchivedAdmissions();
            ShowMessage(restoredCount + " selected admission record(s) restored successfully.", true);
        }

        private List<int> GetSelectedIds(GridView grid, string checkboxId)
        {
            List<int> selectedIds = new List<int>();

            foreach (GridViewRow row in grid.Rows)
            {
                CheckBox chk = row.FindControl(checkboxId) as CheckBox;
                if (chk != null && chk.Checked)
                {
                    int admissionId = Convert.ToInt32(grid.DataKeys[row.RowIndex].Value);
                    selectedIds.Add(admissionId);
                }
            }

            return selectedIds;
        }

        private void RestoreAdmission(int admissionId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();

                try
                {
                    AdmissionInfo info = GetAdmissionInfo(conn, tran, admissionId);

                    if (info == null)
                        throw new Exception("Admission record not found.");

                    if (info.Status != "Admitted")
                        throw new Exception("Only archived admitted admissions can be restored.");

                    InsertAuditLog(
                        conn,
                        tran,
                        CurrentUserId,
                        "Restored archived admission record",
                        admissionId,
                        "Archived=True; Applicant=" + info.StudentName + "; Programme=" + info.ProgrammeName,
                        "Archived=False; Record restored to admitted admissions"
                    );

                    tran.Commit();
                    ShowMessage("Admission restored successfully.", true);
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    ShowMessage("Error restoring admission: " + ex.Message, false);
                }
            }
        }

        private AdmissionInfo GetAdmissionInfo(SqlConnection conn, SqlTransaction tran, int admissionId)
        {
            string sql = @"
                SELECT
                    a.AdmissionId,
                    a.UserId,
                    a.StudentId,
                    a.ProgrammeId,
                    a.IntakeYear,
                    a.IntakeSemester,
                    a.Status,
                    a.FullName AS StudentName,
                    p.ProgrammeName
                FROM Admissions a
                INNER JOIN Programmes p ON p.ProgrammeId = a.ProgrammeId
                OUTER APPLY
                (
                    SELECT TOP 1 al.Action, al.ActionDate
                    FROM AuditLogs al
                    WHERE al.TableAffected = 'Admissions'
                      AND al.RecordId = a.AdmissionId
                      AND al.Action IN ('Archived admission record', 'Restored archived admission record')
                    ORDER BY al.ActionDate DESC
                ) archiveLog
                WHERE a.AdmissionId = @AdmissionId
                  AND a.Status = 'Admitted'
                  AND archiveLog.Action = 'Archived admission record'";

            using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
            {
                cmd.Parameters.AddWithValue("@AdmissionId", admissionId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    return new AdmissionInfo
                    {
                        AdmissionId = Convert.ToInt32(reader["AdmissionId"]),
                        UserId = Convert.ToInt32(reader["UserId"]),
                        StudentId = reader["StudentId"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["StudentId"]),
                        ProgrammeId = Convert.ToInt32(reader["ProgrammeId"]),
                        IntakeYear = Convert.ToInt16(reader["IntakeYear"]),
                        IntakeSemester = Convert.ToByte(reader["IntakeSemester"]),
                        Status = reader["Status"].ToString(),
                        StudentName = reader["StudentName"].ToString(),
                        ProgrammeName = reader["ProgrammeName"].ToString()
                    };
                }
            }
        }

        private void InsertAuditLog(SqlConnection conn, SqlTransaction tran, int userId, string action, int admissionId, string oldValue, string newValue)
        {
            string sql = @"
                INSERT INTO AuditLogs
                (
                    UserId,
                    Action,
                    TableAffected,
                    RecordId,
                    OldValue,
                    NewValue,
                    ActionDate
                )
                VALUES
                (
                    @UserId,
                    @Action,
                    'Admissions',
                    @RecordId,
                    @OldValue,
                    @NewValue,
                    SYSUTCDATETIME()
                )";

            using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@Action", action);
                cmd.Parameters.AddWithValue("@RecordId", admissionId);
                cmd.Parameters.AddWithValue("@OldValue", oldValue);
                cmd.Parameters.AddWithValue("@NewValue", newValue);
                cmd.ExecuteNonQuery();
            }
        }

        protected void gvStatus_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;

            Label lblStatus = e.Row.FindControl("lblStatus") as Label;
            if (lblStatus == null)
                return;

            lblStatus.CssClass = "status-badge status-archived";
        }

        private void ShowMessage(string message, bool success)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = success
                ? "alert alert-success d-block message-box"
                : "alert alert-danger d-block message-box";
        }

        private class AdmissionInfo
        {
            public int AdmissionId { get; set; }
            public int UserId { get; set; }
            public int? StudentId { get; set; }
            public int ProgrammeId { get; set; }
            public short IntakeYear { get; set; }
            public byte IntakeSemester { get; set; }
            public string Status { get; set; }
            public string StudentName { get; set; }
            public string ProgrammeName { get; set; }
        }
    }
}
