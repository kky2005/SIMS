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
            LoadApprovedEnrolments();
            LoadDroppedEnrolments();
        }

        private void LoadPendingRequests()
        {
            DataTable dt = LoadPendingCourseRequests();
            gvPending.DataSource = dt;
            gvPending.DataBind();
            lblPendingCount.Text = "(" + dt.Rows.Count + ")";
        }

        private void LoadApprovedEnrolments()
        {
            DataTable dt = LoadEnrolmentsByStatus("Approved");
            gvApproved.DataSource = dt;
            gvApproved.DataBind();
            lblApprovedCount.Text = "(" + dt.Rows.Count + ")";
        }

        private void LoadDroppedEnrolments()
        {
            DataTable dt = LoadEnrolmentsByStatus("Dropped");
            gvRejected.DataSource = dt;
            gvRejected.DataBind();
            lblRejectedCount.Text = "(" + dt.Rows.Count + ")";
        }

        private DataTable LoadPendingCourseRequests()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"
                    SELECT
                        r.RequestId,
                        r.RequestType,
                        ISNULL(s.StudentNo, '-') AS StudentNo,
                        u.FullName AS StudentName,
                        c.CourseCode,
                        c.CourseName,
                        COALESCE(s.IntakeYear, YEAR(GETDATE())) AS AcademicYear,
                        COALESCE(s.CurrentSemester, 1) AS Semester,
                        r.Status,
                        r.RequestedAt,
                        CAST(NULL AS DATETIME2) AS EnrolledAt,
                        CAST(NULL AS DATETIME2) AS DroppedAt,
                        '-' AS LastAction,
                        '-' AS LastActionBy,
                        CAST(NULL AS DATETIME2) AS LastActionDate
                    FROM CourseRegistrationRequests r
                    INNER JOIN Students s ON s.StudentId = r.StudentId
                    INNER JOIN Users u ON u.UserId = s.UserId
                    INNER JOIN Courses c ON c.CourseId = r.CourseId
                    WHERE r.Status = 'Pending'";

                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                AddRequestFilters(ref sql, cmd);
                sql += " ORDER BY r.RequestedAt DESC";
                cmd.CommandText = sql;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        private DataTable LoadEnrolmentsByStatus(string status)
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
                    WHERE e.Status = @Status";

                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.Parameters.AddWithValue("@Status", status);

                AddEnrolmentFilters(ref sql, cmd);

                if (status == "Approved")
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

        private void AddRequestFilters(ref string sql, SqlCommand cmd)
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
                sql += " AND r.CourseId = @CourseId";
                cmd.Parameters.AddWithValue("@CourseId", ddlFilterCourse.SelectedValue);
            }

            if (!string.IsNullOrEmpty(ddlFilterRequestType.SelectedValue))
            {
                sql += " AND r.RequestType = @RequestType";
                cmd.Parameters.AddWithValue("@RequestType", ddlFilterRequestType.SelectedValue);
            }

            AddDateFilters(ref sql, cmd, "r.RequestedAt");
        }

        private void AddEnrolmentFilters(ref string sql, SqlCommand cmd)
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

            AddDateFilters(ref sql, cmd, "e.RequestedAt");
        }

        private void AddDateFilters(ref string sql, SqlCommand cmd, string dateColumn)
        {
            if (!string.IsNullOrWhiteSpace(txtFromDate.Text))
            {
                DateTime fromDate;
                if (DateTime.TryParse(txtFromDate.Text, out fromDate))
                {
                    sql += " AND CAST(" + dateColumn + " AS date) >= @FromDate";
                    cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                }
            }

            if (!string.IsNullOrWhiteSpace(txtToDate.Text))
            {
                DateTime toDate;
                if (DateTime.TryParse(txtToDate.Text, out toDate))
                {
                    sql += " AND CAST(" + dateColumn + " AS date) <= @ToDate";
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
            ddlFilterRequestType.SelectedIndex = 0;
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
                ShowMessage("Please select at least one dropped enrolment to delete.", false);
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
                ShowMessage(successCount + " selected dropped enrolment(s) deleted successfully.", true);
            else
                ShowMessage(successCount + " deleted. Some records failed: " + string.Join(" | ", errors), false);
        }

        protected void gvPending_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int requestId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out requestId))
                return;

            if (e.CommandName == "ApproveRequest")
                ApproveCourseRequest(requestId);
            else if (e.CommandName == "RejectRequest")
                RejectCourseRequest(requestId);
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

        private void ApproveCourseRequest(int requestId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();

                try
                {
                    CourseRequestInfo request = GetCourseRequestInfo(conn, tran, requestId);

                    if (request == null)
                        throw new Exception("Pending course request not found.");

                    if (request.Status != "Pending")
                        throw new Exception("Only pending course requests can be approved.");

                    if (string.Equals(request.RequestType, "Drop", StringComparison.OrdinalIgnoreCase))
                        ApproveDropRequest(conn, tran, request);
                    else
                        ApproveAddRequest(conn, tran, request);

                    DeleteCourseRequest(conn, tran, requestId);

                    tran.Commit();
                    ShowMessage("Course request approved successfully.", true);
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    ShowMessage("Error approving course request: " + ex.Message, false);
                }
            }
        }

        private void ApproveAddRequest(SqlConnection conn, SqlTransaction tran, CourseRequestInfo request)
        {
            if (HasActiveEnrolment(conn, tran, request.StudentId, request.CourseId))
                throw new Exception("This student already has an approved or archived enrolment for this course.");

            string insertSql = @"
                INSERT INTO Enrolments
                (
                    StudentId,
                    CourseId,
                    AcademicYear,
                    Semester,
                    Status,
                    EnrolledAt,
                    DroppedAt,
                    RequestedAt
                )
                VALUES
                (
                    @StudentId,
                    @CourseId,
                    @AcademicYear,
                    @Semester,
                    'Approved',
                    SYSUTCDATETIME(),
                    NULL,
                    @RequestedAt
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            int enrolmentId;
            using (SqlCommand cmd = new SqlCommand(insertSql, conn, tran))
            {
                cmd.Parameters.AddWithValue("@StudentId", request.StudentId);
                cmd.Parameters.AddWithValue("@CourseId", request.CourseId);
                cmd.Parameters.AddWithValue("@AcademicYear", request.AcademicYear);
                cmd.Parameters.AddWithValue("@Semester", request.Semester);
                cmd.Parameters.AddWithValue("@RequestedAt", (object)request.RequestedAt ?? DBNull.Value);
                enrolmentId = Convert.ToInt32(cmd.ExecuteScalar());
            }

            InsertAuditLog(
                conn,
                tran,
                CurrentUserId,
                "Approved add course request",
                "Enrolments",
                enrolmentId,
                "CourseRegistrationRequests.RequestId=" + request.RequestId + "; Status=Pending; Type=Add",
                "Status=Approved; Student=" + request.StudentName + "; Course=" + request.CourseCode
            );
        }

        private void ApproveDropRequest(SqlConnection conn, SqlTransaction tran, CourseRequestInfo request)
        {
            EnrolmentInfo enrolment = GetApprovedEnrolment(conn, tran, request.StudentId, request.CourseId);

            if (enrolment == null)
                throw new Exception("This student does not have an approved or archived enrolment for this course to drop.");

            string updateSql = @"
                UPDATE Enrolments
                SET Status = 'Dropped',
                    DroppedAt = SYSUTCDATETIME()
                WHERE EnrolmentId = @EnrolmentId
                  AND Status IN ('Approved', 'Archived')";

            using (SqlCommand cmd = new SqlCommand(updateSql, conn, tran))
            {
                cmd.Parameters.AddWithValue("@EnrolmentId", enrolment.EnrolmentId);
                int affected = cmd.ExecuteNonQuery();

                if (affected == 0)
                    throw new Exception("Unable to drop. The enrolment may already have been updated.");
            }

            InsertAuditLog(
                conn,
                tran,
                CurrentUserId,
                "Approved drop course request",
                "Enrolments",
                enrolment.EnrolmentId,
                "Status=" + enrolment.Status + "; CourseRegistrationRequests.RequestId=" + request.RequestId,
                "Status=Dropped; Student=" + request.StudentName + "; Course=" + request.CourseCode
            );
        }

        private void RejectCourseRequest(int requestId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();

                try
                {
                    CourseRequestInfo request = GetCourseRequestInfo(conn, tran, requestId);

                    if (request == null)
                        throw new Exception("Pending course request not found.");

                    if (request.Status != "Pending")
                        throw new Exception("Only pending course requests can be rejected.");

                    InsertAuditLog(
                        conn,
                        tran,
                        CurrentUserId,
                        "Rejected course request",
                        "CourseRegistrationRequests",
                        request.RequestId,
                        "Status=Pending; Type=" + request.RequestType + "; Student=" + request.StudentName + "; Course=" + request.CourseCode,
                        "Request deleted; no enrolment record created/changed"
                    );

                    DeleteCourseRequest(conn, tran, requestId);

                    tran.Commit();
                    ShowMessage("Course request rejected and removed successfully.", true);
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    ShowMessage("Error rejecting course request: " + ex.Message, false);
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
                        "Enrolments",
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
                ShowMessage("Dropped enrolment record deleted successfully.", true);
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

                    if (info.Status != "Dropped")
                        throw new Exception("Only dropped enrolments can be deleted from this section.");

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
                        "Deleted dropped enrolment record",
                        "Enrolments",
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

        private CourseRequestInfo GetCourseRequestInfo(SqlConnection conn, SqlTransaction tran, int requestId)
        {
            string sql = @"
                SELECT
                    r.RequestId,
                    r.StudentId,
                    r.CourseId,
                    r.RequestType,
                    r.Status,
                    r.RequestedAt,
                    COALESCE(s.IntakeYear, YEAR(GETDATE())) AS AcademicYear,
                    COALESCE(s.CurrentSemester, 1) AS Semester,
                    u.FullName AS StudentName,
                    c.CourseCode,
                    c.CourseName
                FROM CourseRegistrationRequests r
                INNER JOIN Students s ON s.StudentId = r.StudentId
                INNER JOIN Users u ON u.UserId = s.UserId
                INNER JOIN Courses c ON c.CourseId = r.CourseId
                WHERE r.RequestId = @RequestId";

            using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
            {
                cmd.Parameters.AddWithValue("@RequestId", requestId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    return new CourseRequestInfo
                    {
                        RequestId = Convert.ToInt32(reader["RequestId"]),
                        StudentId = Convert.ToInt32(reader["StudentId"]),
                        CourseId = Convert.ToInt32(reader["CourseId"]),
                        RequestType = reader["RequestType"].ToString(),
                        Status = reader["Status"].ToString(),
                        RequestedAt = reader["RequestedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["RequestedAt"]),
                        AcademicYear = Convert.ToInt16(reader["AcademicYear"]),
                        Semester = Convert.ToByte(reader["Semester"]),
                        StudentName = reader["StudentName"].ToString(),
                        CourseCode = reader["CourseCode"].ToString(),
                        CourseName = reader["CourseName"].ToString()
                    };
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

        private EnrolmentInfo GetApprovedEnrolment(SqlConnection conn, SqlTransaction tran, int studentId, int courseId)
        {
            string sql = @"
                SELECT TOP 1
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
                WHERE e.StudentId = @StudentId
                  AND e.CourseId = @CourseId
                  AND e.Status IN ('Approved', 'Archived')
                ORDER BY
                    CASE WHEN e.Status = 'Approved' THEN 0 ELSE 1 END,
                    e.EnrolledAt DESC,
                    e.EnrolmentId DESC";

            using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
            {
                cmd.Parameters.AddWithValue("@StudentId", studentId);
                cmd.Parameters.AddWithValue("@CourseId", courseId);

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

        private bool HasActiveEnrolment(SqlConnection conn, SqlTransaction tran, int studentId, int courseId)
        {
            string sql = @"
                SELECT COUNT(*)
                FROM Enrolments
                WHERE StudentId = @StudentId
                  AND CourseId = @CourseId
                  AND Status IN ('Approved', 'Archived')";

            using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
            {
                cmd.Parameters.AddWithValue("@StudentId", studentId);
                cmd.Parameters.AddWithValue("@CourseId", courseId);

                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        private void DeleteCourseRequest(SqlConnection conn, SqlTransaction tran, int requestId)
        {
            using (SqlCommand cmd = new SqlCommand("DELETE FROM CourseRegistrationRequests WHERE RequestId = @RequestId", conn, tran))
            {
                cmd.Parameters.AddWithValue("@RequestId", requestId);
                cmd.ExecuteNonQuery();
            }
        }

        private void InsertAuditLog(SqlConnection conn, SqlTransaction tran, int userId, string action, string tableAffected, int recordId, string oldValue, string newValue)
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
                    @TableAffected,
                    @RecordId,
                    @OldValue,
                    @NewValue,
                    SYSUTCDATETIME()
                )";

            using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@Action", action);
                cmd.Parameters.AddWithValue("@TableAffected", tableAffected);
                cmd.Parameters.AddWithValue("@RecordId", recordId);
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

        private class CourseRequestInfo
        {
            public int RequestId { get; set; }
            public int StudentId { get; set; }
            public int CourseId { get; set; }
            public string RequestType { get; set; }
            public string Status { get; set; }
            public DateTime? RequestedAt { get; set; }
            public short AcademicYear { get; set; }
            public byte Semester { get; set; }
            public string StudentName { get; set; }
            public string CourseCode { get; set; }
            public string CourseName { get; set; }
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
