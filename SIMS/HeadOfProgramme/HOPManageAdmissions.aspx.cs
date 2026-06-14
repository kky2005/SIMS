using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace SIMS.HeadOfProgramme
{
    public partial class HOPManageAdmissions : HOPBase
    {
        private readonly string connStr = ConfigurationManager.ConnectionStrings["SIMS_DB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            EnsureAuthenticated();

            if (!IsPostBack)
            {
                LoadProgrammeFilter();
                LoadAllTables();
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

        private void LoadAllTables()
        {
            LoadPendingRequests();
            LoadAdmittedRequests();
            LoadRejectedRequests();
        }

        private void LoadPendingRequests()
        {
            DataTable dt = LoadRequestsByStatus("Pending");
            gvPending.DataSource = dt;
            gvPending.DataBind();
            lblPendingCount.Text = "(" + dt.Rows.Count + ")";
        }

        private void LoadAdmittedRequests()
        {
            DataTable dt = LoadRequestsByStatus("Admitted");
            gvApproved.DataSource = dt;
            gvApproved.DataBind();
            lblApprovedCount.Text = "(" + dt.Rows.Count + ")";
        }

        private void LoadRejectedRequests()
        {
            DataTable dt = LoadRequestsByStatus("Rejected");
            gvRejected.DataSource = dt;
            gvRejected.DataBind();
            lblRejectedCount.Text = "(" + dt.Rows.Count + ")";
        }

        private DataTable LoadRequestsByStatus(string status)
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
                        a.Status,
                        a.RequestedAt,
                        a.AdmittedAt,
                        a.RejectedAt,
                        a.RejectionReason,
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
                    WHERE 1 = 1";

                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                sql += " AND a.Status = @Status";
                cmd.Parameters.AddWithValue("@Status", status);

                // New table has no Archived status. Archived records are hidden by the latest archive audit log.
                if (status == "Admitted")
                    sql += " AND (archiveLog.Action IS NULL OR archiveLog.Action <> 'Archived admission record')";

                AddCommonFilters(ref sql, cmd);

                if (status == "Pending")
                    sql += " ORDER BY a.RequestedAt DESC";
                else if (status == "Admitted")
                    sql += " ORDER BY a.AdmittedAt DESC, a.RequestedAt DESC";
                else
                    sql += " ORDER BY a.RejectedAt DESC, a.RequestedAt DESC";

                cmd.CommandText = sql;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
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
            LoadAllTables();
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtSearchStudent.Text = "";
            ddlFilterProgramme.SelectedIndex = 0;
            txtFromDate.Text = "";
            txtToDate.Text = "";
            LoadAllTables();
        }

        protected void gvPending_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int admissionId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out admissionId))
                return;

            if (e.CommandName == "ApproveAdmission")
                AdmitAdmission(admissionId);
            else if (e.CommandName == "RejectAdmission")
                RejectAdmission(admissionId);
            else
                return;

            LoadAllTables();
        }

        protected void gvProcessed_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int admissionId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out admissionId))
                return;

            if (e.CommandName == "ArchiveAdmission")
                ArchiveAdmission(admissionId);
            else if (e.CommandName == "DeleteAdmission")
                DeleteAdmission(admissionId);
            else
                return;

            LoadAllTables();
        }

        protected void btnArchiveSelectedApproved_Click(object sender, EventArgs e)
        {
            List<int> selectedIds = GetSelectedIds(gvApproved, "chkSelectApproved");

            if (selectedIds.Count == 0)
            {
                ShowMessage("Please select at least one admitted admission to archive.", false);
                return;
            }

            int archivedCount = 0;
            foreach (int admissionId in selectedIds)
            {
                ArchiveAdmission(admissionId);
                archivedCount++;
            }

            LoadAllTables();
            ShowMessage(archivedCount + " selected admission record(s) archived successfully.", true);
        }

        protected void btnDeleteSelectedRejected_Click(object sender, EventArgs e)
        {
            List<int> selectedIds = GetSelectedIds(gvRejected, "chkSelectRejected");

            if (selectedIds.Count == 0)
            {
                ShowMessage("Please select at least one rejected admission to delete.", false);
                return;
            }

            int deletedCount = 0;
            foreach (int admissionId in selectedIds)
            {
                DeleteAdmission(admissionId);
                deletedCount++;
            }

            LoadAllTables();
            ShowMessage(deletedCount + " selected admission record(s) deleted successfully.", true);
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

        private void AdmitAdmission(int admissionId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();

                try
                {
                    AdmissionInfo info = GetAdmissionInfo(conn, tran, admissionId);

                    if (info == null)
                        throw new Exception("Admission request not found.");

                    if (info.Status != "Pending")
                        throw new Exception("Only pending admission requests can be admitted.");

                    if (HasAdmittedDuplicate(conn, tran, info))
                        throw new Exception("This applicant already has an admitted admission for the same programme, intake year and semester.");

                    string updateAdmissionSql = @"
                        UPDATE Admissions
                        SET Status = 'Admitted',
                            AdmittedAt = SYSUTCDATETIME(),
                            RejectedAt = NULL,
                            RejectionReason = NULL
                        WHERE AdmissionId = @AdmissionId
                          AND Status = 'Pending'";

                    using (SqlCommand updateCmd = new SqlCommand(updateAdmissionSql, conn, tran))
                    {
                        updateCmd.Parameters.AddWithValue("@AdmissionId", admissionId);
                        int affected = updateCmd.ExecuteNonQuery();

                        if (affected == 0)
                            throw new Exception("Unable to admit. The request may already have been processed.");
                    }

                    if (info.StudentId.HasValue)
                    {
                        string updateStudentSql = @"
                            UPDATE Students
                            SET ProgrammeId = @ProgrammeId,
                                IntakeYear = @IntakeYear,
                                IntakeSemester = @IntakeSemester,
                                CurrentSemester = 1,
                                AdmissionDate = SYSUTCDATETIME(),
                                Status = 'Active'
                            WHERE StudentId = @StudentId";

                        using (SqlCommand studentCmd = new SqlCommand(updateStudentSql, conn, tran))
                        {
                            studentCmd.Parameters.AddWithValue("@ProgrammeId", info.ProgrammeId);
                            studentCmd.Parameters.AddWithValue("@IntakeYear", info.IntakeYear);
                            studentCmd.Parameters.AddWithValue("@IntakeSemester", info.IntakeSemester);
                            studentCmd.Parameters.AddWithValue("@StudentId", info.StudentId.Value);
                            studentCmd.ExecuteNonQuery();
                        }
                    }

                    InsertAuditLog(
                        conn,
                        tran,
                        CurrentUserId,
                        "Admitted admission request",
                        admissionId,
                        "Status=Pending; AdmittedAt=NULL; RejectedAt=NULL",
                        "Status=Admitted; Applicant=" + info.StudentName + "; Programme=" + info.ProgrammeName
                    );

                    tran.Commit();
                    ShowMessage("Admission request admitted successfully.", true);
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    ShowMessage("Error admitting admission: " + ex.Message, false);
                }
            }
        }

        private void RejectAdmission(int admissionId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();

                try
                {
                    AdmissionInfo info = GetAdmissionInfo(conn, tran, admissionId);

                    if (info == null)
                        throw new Exception("Admission request not found.");

                    if (info.Status != "Pending")
                        throw new Exception("Only pending admission requests can be rejected.");

                    string updateSql = @"
                        UPDATE Admissions
                        SET Status = 'Rejected',
                            AdmittedAt = NULL,
                            RejectedAt = SYSUTCDATETIME(),
                            RejectionReason = ISNULL(RejectionReason, 'Rejected by Head of Programme')
                        WHERE AdmissionId = @AdmissionId
                          AND Status = 'Pending'";

                    using (SqlCommand updateCmd = new SqlCommand(updateSql, conn, tran))
                    {
                        updateCmd.Parameters.AddWithValue("@AdmissionId", admissionId);
                        int affected = updateCmd.ExecuteNonQuery();

                        if (affected == 0)
                            throw new Exception("Unable to reject. The request may already have been processed.");
                    }

                    InsertAuditLog(
                        conn,
                        tran,
                        CurrentUserId,
                        "Rejected admission request",
                        admissionId,
                        "Status=Pending; AdmittedAt=NULL; RejectedAt=NULL",
                        "Status=Rejected; Applicant=" + info.StudentName + "; Programme=" + info.ProgrammeName
                    );

                    tran.Commit();
                    ShowMessage("Admission request rejected successfully.", true);
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    ShowMessage("Error rejecting admission: " + ex.Message, false);
                }
            }
        }

        private void ArchiveAdmission(int admissionId)
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
                        throw new Exception("Only admitted admissions can be archived.");

                    InsertAuditLog(
                        conn,
                        tran,
                        CurrentUserId,
                        "Archived admission record",
                        admissionId,
                        "Status=Admitted; Applicant=" + info.StudentName + "; Programme=" + info.ProgrammeName,
                        "Record hidden from admitted admissions list"
                    );

                    tran.Commit();
                    ShowMessage("Admission archived successfully. You can view it from the archived admissions page.", true);
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    ShowMessage("Error archiving admission: " + ex.Message, false);
                }
            }
        }

        private void DeleteAdmission(int admissionId)
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

                    if (info.Status == "Pending")
                        throw new Exception("Pending requests cannot be deleted here. Admit or reject them first.");

                    if (info.Status == "Admitted")
                        throw new Exception("Admitted admissions should not be deleted here. Archive admitted records instead.");

                    InsertAuditLog(
                        conn,
                        tran,
                        CurrentUserId,
                        "Deleted admission record",
                        admissionId,
                        "Status=" + info.Status + "; Applicant=" + info.StudentName + "; Programme=" + info.ProgrammeName,
                        "Record deleted from Admissions"
                    );

                    using (SqlCommand unlinkApplicantCmd = new SqlCommand("UPDATE Applicants SET AdmissionId = NULL WHERE AdmissionId = @AdmissionId", conn, tran))
                    {
                        unlinkApplicantCmd.Parameters.AddWithValue("@AdmissionId", admissionId);
                        unlinkApplicantCmd.ExecuteNonQuery();
                    }

                    using (SqlCommand deleteCmd = new SqlCommand("DELETE FROM Admissions WHERE AdmissionId = @AdmissionId", conn, tran))
                    {
                        deleteCmd.Parameters.AddWithValue("@AdmissionId", admissionId);
                        deleteCmd.ExecuteNonQuery();
                    }

                    tran.Commit();
                    ShowMessage("Admission record deleted successfully.", true);
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    ShowMessage("Error deleting admission: " + ex.Message, false);
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
                WHERE a.AdmissionId = @AdmissionId";

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

        private bool HasAdmittedDuplicate(SqlConnection conn, SqlTransaction tran, AdmissionInfo info)
        {
            string sql = @"
                SELECT COUNT(*)
                FROM Admissions
                WHERE UserId = @UserId
                  AND ProgrammeId = @ProgrammeId
                  AND IntakeYear = @IntakeYear
                  AND IntakeSemester = @IntakeSemester
                  AND Status = 'Admitted'
                  AND AdmissionId <> @AdmissionId";

            using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
            {
                cmd.Parameters.AddWithValue("@UserId", info.UserId);
                cmd.Parameters.AddWithValue("@ProgrammeId", info.ProgrammeId);
                cmd.Parameters.AddWithValue("@IntakeYear", info.IntakeYear);
                cmd.Parameters.AddWithValue("@IntakeSemester", info.IntakeSemester);
                cmd.Parameters.AddWithValue("@AdmissionId", info.AdmissionId);

                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
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

            string status = lblStatus.Text;
            lblStatus.CssClass = "status-badge";

            if (status == "Pending")
                lblStatus.CssClass += " status-pending";
            else if (status == "Admitted")
                lblStatus.CssClass += " status-approved";
            else if (status == "Rejected")
                lblStatus.CssClass += " status-rejected";
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
