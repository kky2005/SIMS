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
                LoadPeriodProgrammeDropdown();
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
            LoadRegistrationPeriods();
            LoadPendingRequests();
            LoadApprovedEnrolments();
            LoadDroppedEnrolments();
        }



        private void LoadPeriodProgrammeDropdown()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"
                    SELECT ProgrammeId, ProgrammeCode + ' - ' + ProgrammeName AS ProgrammeDisplay
                    FROM Programmes
                    ORDER BY ProgrammeCode";

                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ddlPeriodProgramme.DataSource = dt;
                ddlPeriodProgramme.DataTextField = "ProgrammeDisplay";
                ddlPeriodProgramme.DataValueField = "ProgrammeId";
                ddlPeriodProgramme.DataBind();
                ddlPeriodProgramme.Items.Insert(0, new ListItem("Select Programme", ""));
            }
        }

        private void LoadRegistrationPeriods()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"
                    SELECT
                        rp.PeriodId,
                        rp.ProgrammeId,
                        p.ProgrammeCode + ' - ' + p.ProgrammeName AS ProgrammeName,
                        rp.AcademicYear,
                        rp.Semester,
                        rp.PeriodType,
                        rp.StartDate,
                        rp.EndDate,
                        ISNULL(rp.IsActive, 0) AS IsActive
                    FROM RegistrationPeriods rp
                    INNER JOIN Programmes p ON p.ProgrammeId = rp.ProgrammeId
                    ORDER BY rp.AcademicYear DESC, rp.Semester DESC, p.ProgrammeCode, rp.PeriodType";

                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvRegistrationPeriods.DataSource = dt;
                gvRegistrationPeriods.DataBind();
                lblPeriodCount.Text = "(" + dt.Rows.Count + ")";
            }
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
            DataTable dt = LoadEnrolmentsByStatus("Active");
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

                if (status == "Active")
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



        protected void btnSavePeriod_Click(object sender, EventArgs e)
        {
            int programmeId;
            short academicYear;
            byte semester;
            DateTime startDate;
            DateTime endDate;

            if (!int.TryParse(ddlPeriodProgramme.SelectedValue, out programmeId))
            {
                ShowMessage("Please select a programme for the registration period.", false);
                return;
            }

            if (!short.TryParse(txtPeriodAcademicYear.Text.Trim(), out academicYear) || academicYear < 2000)
            {
                ShowMessage("Please enter a valid academic year.", false);
                return;
            }

            if (!byte.TryParse(ddlPeriodSemester.SelectedValue, out semester))
            {
                ShowMessage("Please select a valid semester.", false);
                return;
            }

            if (!DateTime.TryParse(txtPeriodStartDate.Text, out startDate) || !DateTime.TryParse(txtPeriodEndDate.Text, out endDate))
            {
                ShowMessage("Please enter both start date and end date.", false);
                return;
            }

            if (endDate.Date < startDate.Date)
            {
                ShowMessage("End date cannot be earlier than start date.", false);
                return;
            }

            string periodType = ddlPeriodType.SelectedValue;
            int periodId;
            bool isEdit = int.TryParse(hfPeriodId.Value, out periodId) && periodId > 0;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();

                try
                {
                    string duplicateSql = @"
                        SELECT COUNT(*)
                        FROM RegistrationPeriods
                        WHERE ProgrammeId = @ProgrammeId
                          AND AcademicYear = @AcademicYear
                          AND Semester = @Semester
                          AND PeriodType = @PeriodType
                          AND PeriodId <> @PeriodId";

                    using (SqlCommand duplicateCmd = new SqlCommand(duplicateSql, conn, tran))
                    {
                        duplicateCmd.Parameters.AddWithValue("@ProgrammeId", programmeId);
                        duplicateCmd.Parameters.AddWithValue("@AcademicYear", academicYear);
                        duplicateCmd.Parameters.AddWithValue("@Semester", semester);
                        duplicateCmd.Parameters.AddWithValue("@PeriodType", periodType);
                        duplicateCmd.Parameters.AddWithValue("@PeriodId", isEdit ? periodId : 0);

                        int duplicateCount = Convert.ToInt32(duplicateCmd.ExecuteScalar());
                        if (duplicateCount > 0)
                            throw new Exception("A registration period already exists for this programme, year, semester and type.");
                    }

                    string sql;
                    if (isEdit)
                    {
                        sql = @"
                            UPDATE RegistrationPeriods
                            SET ProgrammeId = @ProgrammeId,
                                AcademicYear = @AcademicYear,
                                Semester = @Semester,
                                PeriodType = @PeriodType,
                                StartDate = @StartDate,
                                EndDate = @EndDate,
                                IsActive = @IsActive
                            WHERE PeriodId = @PeriodId";
                    }
                    else
                    {
                        sql = @"
                            INSERT INTO RegistrationPeriods
                            (ProgrammeId, AcademicYear, Semester, PeriodType, StartDate, EndDate, IsActive)
                            VALUES
                            (@ProgrammeId, @AcademicYear, @Semester, @PeriodType, @StartDate, @EndDate, @IsActive);
                            SELECT CAST(SCOPE_IDENTITY() AS INT);";
                    }

                    using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@ProgrammeId", programmeId);
                        cmd.Parameters.AddWithValue("@AcademicYear", academicYear);
                        cmd.Parameters.AddWithValue("@Semester", semester);
                        cmd.Parameters.AddWithValue("@PeriodType", periodType);
                        cmd.Parameters.AddWithValue("@StartDate", startDate.Date);
                        cmd.Parameters.AddWithValue("@EndDate", endDate.Date);
                        cmd.Parameters.AddWithValue("@IsActive", chkPeriodIsActive.Checked);

                        if (isEdit)
                        {
                            cmd.Parameters.AddWithValue("@PeriodId", periodId);
                            cmd.ExecuteNonQuery();
                        }
                        else
                        {
                            periodId = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                    }

                    InsertAuditLog(
                        conn,
                        tran,
                        CurrentUserId,
                        isEdit ? "Updated registration period" : "Created registration period",
                        "RegistrationPeriods",
                        periodId,
                        isEdit ? "Registration period edited" : "New registration period",
                        "ProgrammeId=" + programmeId + "; Year=" + academicYear + "; Semester=" + semester + "; Type=" + periodType + "; Start=" + startDate.ToString("yyyy-MM-dd") + "; End=" + endDate.ToString("yyyy-MM-dd") + "; Active=" + chkPeriodIsActive.Checked
                    );

                    tran.Commit();
                    ClearPeriodForm();
                    LoadRegistrationPeriods();
                    ShowMessage(isEdit ? "Registration period updated successfully." : "Registration period added successfully.", true);
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    ShowMessage("Error saving registration period: " + ex.Message, false);
                }
            }
        }

        protected void btnClearPeriod_Click(object sender, EventArgs e)
        {
            ClearPeriodForm();
        }

        private void ClearPeriodForm()
        {
            hfPeriodId.Value = "";
            if (ddlPeriodProgramme.Items.Count > 0)
                ddlPeriodProgramme.SelectedIndex = 0;
            txtPeriodAcademicYear.Text = "";
            ddlPeriodSemester.SelectedValue = "1";
            ddlPeriodType.SelectedValue = "Registration";
            txtPeriodStartDate.Text = "";
            txtPeriodEndDate.Text = "";
            chkPeriodIsActive.Checked = true;
            btnSavePeriod.Text = "Add Period";
        }

        protected void gvRegistrationPeriods_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int periodId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out periodId))
                return;

            if (e.CommandName == "EditPeriod")
                LoadPeriodForEdit(periodId);
            else if (e.CommandName == "TogglePeriod")
                ToggleRegistrationPeriod(periodId);
        }

        private void LoadPeriodForEdit(int periodId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"
                    SELECT PeriodId, ProgrammeId, AcademicYear, Semester, PeriodType, StartDate, EndDate, ISNULL(IsActive, 0) AS IsActive
                    FROM RegistrationPeriods
                    WHERE PeriodId = @PeriodId";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@PeriodId", periodId);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            ShowMessage("Registration period not found.", false);
                            return;
                        }

                        hfPeriodId.Value = reader["PeriodId"].ToString();
                        ddlPeriodProgramme.SelectedValue = reader["ProgrammeId"].ToString();
                        txtPeriodAcademicYear.Text = reader["AcademicYear"].ToString();
                        ddlPeriodSemester.SelectedValue = reader["Semester"].ToString();
                        ddlPeriodType.SelectedValue = reader["PeriodType"].ToString();
                        txtPeriodStartDate.Text = Convert.ToDateTime(reader["StartDate"]).ToString("yyyy-MM-dd");
                        txtPeriodEndDate.Text = Convert.ToDateTime(reader["EndDate"]).ToString("yyyy-MM-dd");
                        chkPeriodIsActive.Checked = Convert.ToBoolean(reader["IsActive"]);
                        btnSavePeriod.Text = "Update Period";
                    }
                }
            }
        }

        private void ToggleRegistrationPeriod(int periodId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();

                try
                {
                    string sql = @"
                        UPDATE RegistrationPeriods
                        SET IsActive = CASE WHEN ISNULL(IsActive, 0) = 1 THEN 0 ELSE 1 END
                        WHERE PeriodId = @PeriodId";

                    using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@PeriodId", periodId);
                        int affected = cmd.ExecuteNonQuery();
                        if (affected == 0)
                            throw new Exception("Registration period not found.");
                    }

                    InsertAuditLog(
                        conn,
                        tran,
                        CurrentUserId,
                        "Toggled registration period status",
                        "RegistrationPeriods",
                        periodId,
                        "Status changed",
                        "IsActive toggled"
                    );

                    tran.Commit();
                    ClearPeriodForm();
                    LoadRegistrationPeriods();
                    ShowMessage("Registration period status updated successfully.", true);
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    ShowMessage("Error updating registration period: " + ex.Message, false);
                }
            }
        }

        protected void gvRegistrationPeriods_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;

            Label lblStatus = e.Row.FindControl("lblPeriodStatus") as Label;
            if (lblStatus == null)
                return;

            lblStatus.CssClass = "status-badge";
            if (lblStatus.Text == "Active")
                lblStatus.CssClass += " status-approved";
            else
                lblStatus.CssClass += " status-archived";
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

        protected void btnCleanupRejected_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"
                    DELETE FROM CourseRegistrationRequests
                    WHERE Status = 'Rejected'
                      AND RejectedAt IS NOT NULL
                      AND RejectedAt < DATEADD(DAY, -7, DATEADD(HOUR, 8, SYSUTCDATETIME()))";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    int deleted = cmd.ExecuteNonQuery();

                    ShowMessage(deleted + " old rejected request(s) cleaned up successfully.", true);
                }
            }

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
                ShowMessage("Please select at least one active enrolment to complete.", false);
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
                ShowMessage(successCount + " selected enrolment(s) completed successfully.", true);
            else
                ShowMessage(successCount + " completed. Some records failed: " + string.Join(" | ", errors), false);
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
                    else if (string.Equals(request.RequestType, "Register", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(request.RequestType, "Add", StringComparison.OrdinalIgnoreCase))
                        ApproveAddRequest(conn, tran, request);
                    else
                        throw new Exception("Invalid request type. Only Register and Drop are allowed.");

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
                throw new Exception("This student already has an active or completed enrolment for this course.");

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
                    'Active',
                    DATEADD(HOUR, 8, SYSUTCDATETIME()),
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
                "Activated registration request",
                "Enrolments",
                enrolmentId,
                "CourseRegistrationRequests.RequestId=" + request.RequestId + "; Status=Pending; Type=Register",
                "Status=Active; Student=" + request.StudentName + "; Course=" + request.CourseCode
            );
        }

        private void ApproveDropRequest(SqlConnection conn, SqlTransaction tran, CourseRequestInfo request)
        {
            EnrolmentInfo enrolment = GetApprovedEnrolment(conn, tran, request.StudentId, request.CourseId);

            if (enrolment == null)
                throw new Exception("This student does not have an active or completed enrolment for this course to drop.");

            string updateSql = @"
                UPDATE Enrolments
                SET Status = 'Dropped',
                    DroppedAt = DATEADD(HOUR, 8, SYSUTCDATETIME())
                WHERE EnrolmentId = @EnrolmentId
                  AND Status IN ('Active', 'Completed')";

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

                    string updateSql = @"
                        UPDATE CourseRegistrationRequests
                        SET Status = 'Rejected',
                            RejectedAt = DATEADD(HOUR, 8, SYSUTCDATETIME())
                        WHERE RequestId = @RequestId
                          AND Status = 'Pending'";

                    using (SqlCommand updateCmd = new SqlCommand(updateSql, conn, tran))
                    {
                        updateCmd.Parameters.AddWithValue("@RequestId", requestId);
                        int affected = updateCmd.ExecuteNonQuery();

                        if (affected == 0)
                            throw new Exception("Unable to reject. The request may already have been processed.");
                    }

                    InsertAuditLog(
                        conn,
                        tran,
                        CurrentUserId,
                        "Rejected course request",
                        "CourseRegistrationRequests",
                        request.RequestId,
                        "Status=Pending; Type=" + request.RequestType + "; Student=" + request.StudentName + "; Course=" + request.CourseCode,
                        "Status=Rejected; Request kept in CourseRegistrationRequests"
                    );

                    tran.Commit();
                    ShowMessage("Course request rejected successfully. The request is kept with Rejected status.", true);
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
                ShowMessage("Enrolment completed successfully. You can view it from the completed enrolments page.", true);
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

                    if (info.Status != "Active")
                        throw new Exception("Only active enrolments can be completed.");

                    string updateSql = @"
                        UPDATE Enrolments
                        SET Status = 'Completed'
                        WHERE EnrolmentId = @EnrolmentId
                          AND Status = 'Active'";

                    using (SqlCommand updateCmd = new SqlCommand(updateSql, conn, tran))
                    {
                        updateCmd.Parameters.AddWithValue("@EnrolmentId", enrolmentId);
                        int affected = updateCmd.ExecuteNonQuery();

                        if (affected == 0)
                            throw new Exception("Unable to complete. The enrolment may already have been updated.");
                    }

                    InsertAuditLog(
                        conn,
                        tran,
                        CurrentUserId,
                        "Completed enrolment record",
                        "Enrolments",
                        enrolmentId,
                        "Status=Active; Student=" + info.StudentName + "; Course=" + info.CourseCode,
                        "Status=Completed; Record moved to completed enrolments"
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
                            throw new Exception("This enrolment cannot be deleted because it has attendance records. Complete or keep the record instead.");
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
                  AND e.Status IN ('Active', 'Completed')
                ORDER BY
                    CASE WHEN e.Status = 'Active' THEN 0 ELSE 1 END,
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
                  AND Status IN ('Active', 'Completed')";

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
                    DATEADD(HOUR, 8, SYSUTCDATETIME())
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
            else if (status == "Active")
                lblStatus.CssClass += " status-approved";
            else if (status == "Rejected" || status == "Dropped")
                lblStatus.CssClass += " status-rejected";
            else if (status == "Completed")
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
