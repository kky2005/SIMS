using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace SIMS.HeadOfProgramme
{
    public partial class HOPManageEnrolments : HOPBase
    {
        private readonly string connStr = ConfigurationManager.ConnectionStrings["SIMS_DB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            EnsureAuthenticated();

            if (!IsPostBack)
            {
                LoadCourseFilter();
                LoadAllTables();
            }
        }

        private void LoadCourseFilter()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"
                    SELECT CourseId, CourseCode + ' - ' + CourseName AS CourseDisplay
                    FROM Courses
                    ORDER BY CourseCode";

                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ddlFilterCourse.DataSource = dt;
                ddlFilterCourse.DataTextField = "CourseDisplay";
                ddlFilterCourse.DataValueField = "CourseId";
                ddlFilterCourse.DataBind();
                ddlFilterCourse.Items.Insert(0, new ListItem("All Courses", ""));
            }
        }

        private void LoadAllTables()
        {
            LoadPendingRequests();
            LoadApprovedRequests();
            LoadRejectedRequests();
        }

        private void LoadPendingRequests()
        {
            DataTable dt = LoadRequestsByStatus("Pending");
            gvPending.DataSource = dt;
            gvPending.DataBind();
            lblPendingCount.Text = "(" + dt.Rows.Count + ")";
        }

        private void LoadApprovedRequests()
        {
            DataTable dt = LoadRequestsByStatus("Approved");
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
                        e.EnrolmentId,
                        ISNULL(s.StudentNo, '-') AS StudentNo,
                        u.FullName AS StudentName,
                        c.CourseCode,
                        c.CourseName,
                        e.AcademicYear,
                        e.Semester,
                        e.Status,
                        e.RequestedAt,
                        e.EnrolledAt,
                        e.DroppedAt,
                        ISNULL(lastLog.Action, '-') AS LastAction,
                        ISNULL(adminUser.FullName, '-') AS LastActionBy,
                        lastLog.ActionDate AS LastActionDate
                    FROM Enrolments e
                    INNER JOIN Students s ON s.StudentId = e.StudentId
                    INNER JOIN Users u ON u.UserId = s.UserId
                    INNER JOIN Courses c ON c.CourseId = e.CourseId
                    OUTER APPLY
                    (
                        SELECT TOP 1 al.UserId, al.Action, al.ActionDate
                        FROM AuditLogs al
                        WHERE al.TableAffected = 'Enrolments'
                          AND al.RecordId = e.EnrolmentId
                        ORDER BY al.ActionDate DESC
                    ) lastLog
                    LEFT JOIN Users adminUser ON adminUser.UserId = lastLog.UserId
                    WHERE 1 = 1";

                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                if (status == "Rejected")
                {
                    sql += " AND e.Status IN ('Rejected', 'Dropped')";
                }
                else
                {
                    sql += " AND e.Status = @Status";
                    cmd.Parameters.AddWithValue("@Status", status);
                }

                AddCommonFilters(ref sql, cmd);

                if (status == "Pending")
                    sql += " ORDER BY e.RequestedAt DESC";
                else if (status == "Approved")
                    sql += " ORDER BY e.EnrolledAt DESC, e.RequestedAt DESC";
                else
                    sql += " ORDER BY e.DroppedAt DESC, e.RequestedAt DESC";

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
                            OR ISNULL(u.FullName, '') LIKE @StudentSearch
                          )";
                cmd.Parameters.AddWithValue("@StudentSearch", "%" + txtSearchStudent.Text.Trim() + "%");
            }

            if (!string.IsNullOrEmpty(ddlFilterCourse.SelectedValue))
            {
                sql += " AND e.CourseId = @CourseId";
                cmd.Parameters.AddWithValue("@CourseId", ddlFilterCourse.SelectedValue);
            }

            if (!string.IsNullOrWhiteSpace(txtFromDate.Text))
            {
                DateTime fromDate;
                if (DateTime.TryParse(txtFromDate.Text, out fromDate))
                {
                    sql += " AND CAST(e.RequestedAt AS date) >= @FromDate";
                    cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                }
            }

            if (!string.IsNullOrWhiteSpace(txtToDate.Text))
            {
                DateTime toDate;
                if (DateTime.TryParse(txtToDate.Text, out toDate))
                {
                    sql += " AND CAST(e.RequestedAt AS date) <= @ToDate";
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
            ddlFilterCourse.SelectedIndex = 0;
            txtFromDate.Text = "";
            txtToDate.Text = "";
            LoadAllTables();
        }

        private List<int> GetSelectedIds(GridView grid, string checkboxId)
        {
            List<int> selectedIds = new List<int>();

            foreach (GridViewRow row in grid.Rows)
            {
                CheckBox chk = row.FindControl(checkboxId) as CheckBox;

                if (chk != null && chk.Checked)
                {
                    int enrolmentId = Convert.ToInt32(grid.DataKeys[row.RowIndex].Value);
                    selectedIds.Add(enrolmentId);
                }
            }

            return selectedIds;
        }

        protected void btnArchiveSelected_Click(object sender, EventArgs e)
        {
            List<int> selectedIds = GetSelectedIds(gvApproved, "chkSelectApproved");

            if (selectedIds.Count == 0)
            {
                ShowMessage("Please select at least one approved enrolment to archive.", false);
                return;
            }

            int successCount = 0;
            List<string> errors = new List<string>();

            foreach (int id in selectedIds)
            {
                string error;
                if (TryArchiveEnrolment(id, out error))
                    successCount++;
                else
                    errors.Add("ID " + id + ": " + error);
            }

            LoadAllTables();

            if (errors.Count == 0)
                ShowMessage(successCount + " selected enrolment(s) archived successfully.", true);
            else
                ShowMessage(successCount + " archived. Some records failed: " + string.Join(" | ", errors), false);
        }

        protected void btnDeleteSelected_Click(object sender, EventArgs e)
        {
            List<int> selectedIds = GetSelectedIds(gvRejected, "chkSelectRejected");

            if (selectedIds.Count == 0)
            {
                ShowMessage("Please select at least one rejected/dropped enrolment to delete.", false);
                return;
            }

            int successCount = 0;
            List<string> errors = new List<string>();

            foreach (int id in selectedIds)
            {
                string error;
                if (TryDeleteEnrolment(id, out error))
                    successCount++;
                else
                    errors.Add("ID " + id + ": " + error);
            }

            LoadAllTables();

            if (errors.Count == 0)
                ShowMessage(successCount + " selected enrolment(s) deleted successfully.", true);
            else
                ShowMessage(successCount + " deleted. Some records failed: " + string.Join(" | ", errors), false);
        }

        protected void gvPending_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int enrolmentId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out enrolmentId))
                return;

            if (e.CommandName == "ApproveEnrolment")
                ApproveEnrolment(enrolmentId);
            else if (e.CommandName == "RejectEnrolment")
                RejectEnrolment(enrolmentId);
            else
                return;

            LoadAllTables();
        }

        protected void gvProcessed_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int enrolmentId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out enrolmentId))
                return;

            if (e.CommandName == "ArchiveEnrolment")
                ArchiveEnrolment(enrolmentId);
            else if (e.CommandName == "DeleteEnrolment")
                DeleteEnrolment(enrolmentId);
            else
                return;

            LoadAllTables();
        }

        private void ApproveEnrolment(int enrolmentId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();

                try
                {
                    EnrolmentInfo info = GetEnrolmentInfo(conn, tran, enrolmentId);

                    if (info == null)
                        throw new Exception("Enrolment request not found.");

                    if (info.Status != "Pending")
                        throw new Exception("Only pending enrolment requests can be approved.");

                    if (HasApprovedDuplicate(conn, tran, info))
                        throw new Exception("This student is already approved for this course in the same academic year and semester.");

                    string updateSql = @"
                        UPDATE Enrolments
                        SET Status = 'Approved',
                            EnrolledAt = SYSUTCDATETIME(),
                            DroppedAt = NULL
                        WHERE EnrolmentId = @EnrolmentId
                          AND Status = 'Pending'";

                    using (SqlCommand updateCmd = new SqlCommand(updateSql, conn, tran))
                    {
                        updateCmd.Parameters.AddWithValue("@EnrolmentId", enrolmentId);
                        int affected = updateCmd.ExecuteNonQuery();

                        if (affected == 0)
                            throw new Exception("Unable to approve. The request may already have been processed.");
                    }

                    InsertAuditLog(
                        conn,
                        tran,
                        CurrentUserId,
                        "Approved enrolment request",
                        enrolmentId,
                        "Status=Pending; EnrolledAt=NULL; DroppedAt=NULL",
                        "Status=Approved; Student=" + info.StudentName + "; Course=" + info.CourseCode
                    );

                    tran.Commit();
                    ShowMessage("Enrolment request approved successfully.", true);
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    ShowMessage("Error approving enrolment: " + ex.Message, false);
                }
            }
        }

        private void RejectEnrolment(int enrolmentId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();

                try
                {
                    EnrolmentInfo info = GetEnrolmentInfo(conn, tran, enrolmentId);

                    if (info == null)
                        throw new Exception("Enrolment request not found.");

                    if (info.Status != "Pending")
                        throw new Exception("Only pending enrolment requests can be rejected.");

                    string updateSql = @"
                        UPDATE Enrolments
                        SET Status = 'Rejected',
                            EnrolledAt = NULL,
                            DroppedAt = SYSUTCDATETIME()
                        WHERE EnrolmentId = @EnrolmentId
                          AND Status = 'Pending'";

                    using (SqlCommand updateCmd = new SqlCommand(updateSql, conn, tran))
                    {
                        updateCmd.Parameters.AddWithValue("@EnrolmentId", enrolmentId);
                        int affected = updateCmd.ExecuteNonQuery();

                        if (affected == 0)
                            throw new Exception("Unable to reject. The request may already have been processed.");
                    }

                    InsertAuditLog(
                        conn,
                        tran,
                        CurrentUserId,
                        "Rejected enrolment request",
                        enrolmentId,
                        "Status=Pending; EnrolledAt=NULL; DroppedAt=NULL",
                        "Status=Rejected; Student=" + info.StudentName + "; Course=" + info.CourseCode
                    );

                    tran.Commit();
                    ShowMessage("Enrolment request rejected successfully.", true);
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    ShowMessage("Error rejecting enrolment: " + ex.Message, false);
                }
            }
        }

        private void ArchiveEnrolment(int enrolmentId)
        {
            string error;
            if (TryArchiveEnrolment(enrolmentId, out error))
                ShowMessage("Enrolment archived successfully. You can view it from the archived enrolments page.", true);
            else
                ShowMessage("Error archiving enrolment: " + error, false);
        }

        private bool TryArchiveEnrolment(int enrolmentId, out string error)
        {
            error = "";

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();

                try
                {
                    EnrolmentInfo info = GetEnrolmentInfo(conn, tran, enrolmentId);

                    if (info == null)
                        throw new Exception("Enrolment record not found.");

                    if (info.Status != "Approved")
                        throw new Exception("Only approved enrolments can be archived.");

                    string updateSql = @"
                        UPDATE Enrolments
                        SET Status = 'Archived'
                        WHERE EnrolmentId = @EnrolmentId
                          AND Status = 'Approved'";

                    using (SqlCommand updateCmd = new SqlCommand(updateSql, conn, tran))
                    {
                        updateCmd.Parameters.AddWithValue("@EnrolmentId", enrolmentId);
                        int affected = updateCmd.ExecuteNonQuery();

                        if (affected == 0)
                            throw new Exception("Unable to archive. The enrolment may already have been updated.");
                    }

                    InsertAuditLog(
                        conn,
                        tran,
                        CurrentUserId,
                        "Archived enrolment record",
                        enrolmentId,
                        "Status=Approved; Student=" + info.StudentName + "; Course=" + info.CourseCode,
                        "Status=Archived; Record moved to archived enrolments"
                    );

                    tran.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    error = ex.Message;
                    return false;
                }
            }
        }

        private void DeleteEnrolment(int enrolmentId)
        {
            string error;
            if (TryDeleteEnrolment(enrolmentId, out error))
                ShowMessage("Enrolment record deleted successfully.", true);
            else
                ShowMessage("Error deleting enrolment: " + error, false);
        }

        private bool TryDeleteEnrolment(int enrolmentId, out string error)
        {
            error = "";

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();

                try
                {
                    EnrolmentInfo info = GetEnrolmentInfo(conn, tran, enrolmentId);

                    if (info == null)
                        throw new Exception("Enrolment record not found.");

                    if (info.Status == "Pending")
                        throw new Exception("Pending requests cannot be deleted here. Approve or reject them first.");

                    if (info.Status != "Rejected" && info.Status != "Dropped")
                        throw new Exception("Only rejected or dropped enrolments can be deleted from this section.");

                    string checkSql = "SELECT COUNT(*) FROM Attendance WHERE EnrolmentId = @EnrolmentId";
                    using (SqlCommand checkCmd = new SqlCommand(checkSql, conn, tran))
                    {
                        checkCmd.Parameters.AddWithValue("@EnrolmentId", enrolmentId);
                        int attendanceCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (attendanceCount > 0)
                            throw new Exception("This enrolment cannot be deleted because it has attendance records. Archive or keep the record instead.");
                    }

                    InsertAuditLog(
                        conn,
                        tran,
                        CurrentUserId,
                        "Deleted enrolment record",
                        enrolmentId,
                        "Status=" + info.Status + "; Student=" + info.StudentName + "; Course=" + info.CourseCode,
                        "Record deleted from Enrolments"
                    );


                    using (SqlCommand deleteCmd = new SqlCommand("DELETE FROM Enrolments WHERE EnrolmentId = @EnrolmentId", conn, tran))
                    {
                        deleteCmd.Parameters.AddWithValue("@EnrolmentId", enrolmentId);
                        deleteCmd.ExecuteNonQuery();
                    }

                    tran.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    error = ex.Message;
                    return false;
                }
            }
        }

        private EnrolmentInfo GetEnrolmentInfo(SqlConnection conn, SqlTransaction tran, int enrolmentId)
        {
            string sql = @"
                SELECT
                    e.EnrolmentId,
                    e.StudentId,
                    e.CourseId,
                    e.AcademicYear,
                    e.Semester,
                    e.Status,
                    u.FullName AS StudentName,
                    c.CourseCode,
                    c.CourseName
                FROM Enrolments e
                INNER JOIN Students s ON s.StudentId = e.StudentId
                INNER JOIN Users u ON u.UserId = s.UserId
                INNER JOIN Courses c ON c.CourseId = e.CourseId
                WHERE e.EnrolmentId = @EnrolmentId";

            using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
            {
                cmd.Parameters.AddWithValue("@EnrolmentId", enrolmentId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    return new EnrolmentInfo
                    {
                        EnrolmentId = Convert.ToInt32(reader["EnrolmentId"]),
                        StudentId = Convert.ToInt32(reader["StudentId"]),
                        CourseId = Convert.ToInt32(reader["CourseId"]),
                        AcademicYear = Convert.ToInt16(reader["AcademicYear"]),
                        Semester = Convert.ToByte(reader["Semester"]),
                        Status = reader["Status"].ToString(),
                        StudentName = reader["StudentName"].ToString(),
                        CourseCode = reader["CourseCode"].ToString(),
                        CourseName = reader["CourseName"].ToString()
                    };
                }
            }
        }

        private bool HasApprovedDuplicate(SqlConnection conn, SqlTransaction tran, EnrolmentInfo info)
        {
            string sql = @"
                SELECT COUNT(*)
                FROM Enrolments
                WHERE StudentId = @StudentId
                  AND CourseId = @CourseId
                  AND AcademicYear = @AcademicYear
                  AND Semester = @Semester
                  AND Status = 'Approved'
                  AND EnrolmentId <> @EnrolmentId";

            using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
            {
                cmd.Parameters.AddWithValue("@StudentId", info.StudentId);
                cmd.Parameters.AddWithValue("@CourseId", info.CourseId);
                cmd.Parameters.AddWithValue("@AcademicYear", info.AcademicYear);
                cmd.Parameters.AddWithValue("@Semester", info.Semester);
                cmd.Parameters.AddWithValue("@EnrolmentId", info.EnrolmentId);

                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        private void InsertAuditLog(SqlConnection conn, SqlTransaction tran, int userId, string action, int enrolmentId, string oldValue, string newValue)
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
                    'Enrolments',
                    @RecordId,
                    @OldValue,
                    @NewValue,
                    SYSUTCDATETIME()
                )";

            using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@Action", action);
                cmd.Parameters.AddWithValue("@RecordId", enrolmentId);
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
            else if (status == "Approved")
                lblStatus.CssClass += " status-approved";
            else if (status == "Rejected" || status == "Dropped")
                lblStatus.CssClass += " status-rejected";
            else if (status == "Archived")
                lblStatus.CssClass += " status-archived";
        }

        private void ShowMessage(string message, bool success)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = success
                ? "alert alert-success d-block message-box"
                : "alert alert-danger d-block message-box";
        }

        private class EnrolmentInfo
        {
            public int EnrolmentId { get; set; }
            public int StudentId { get; set; }
            public int CourseId { get; set; }
            public short AcademicYear { get; set; }
            public byte Semester { get; set; }
            public string Status { get; set; }
            public string StudentName { get; set; }
            public string CourseCode { get; set; }
            public string CourseName { get; set; }
        }
    }
}
