using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;

namespace SIMS.HeadOfProgramme
{
    public partial class HOPArchivedEnrolments : HOPBase
    {
        private readonly string connStr = ConfigurationManager.ConnectionStrings["SIMS_DB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            EnsureAuthenticated();

            if (!IsPostBack)
            {
                LoadCourseFilter();
                LoadArchivedEnrolments();
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

        private void LoadArchivedEnrolments()
        {
            DataTable dt = GetArchivedEnrolmentsData();
            gvArchived.DataSource = dt;
            gvArchived.DataBind();
            lblArchivedCount.Text = "(" + dt.Rows.Count + ")";
        }

        private DataTable GetArchivedEnrolmentsData()
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
                    WHERE e.Status = 'Completed'";

                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                AddCommonFilters(ref sql, cmd);

                sql += " ORDER BY lastLog.ActionDate DESC, e.EnrolledAt DESC, e.RequestedAt DESC";
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
            LoadArchivedEnrolments();
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtSearchStudent.Text = "";
            ddlFilterCourse.SelectedIndex = 0;
            txtFromDate.Text = "";
            txtToDate.Text = "";
            LoadArchivedEnrolments();
        }

        protected void gvArchived_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "RestoreEnrolment")
                return;

            int enrolmentId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out enrolmentId))
                return;

            RestoreEnrolment(enrolmentId);
            LoadArchivedEnrolments();
        }

        protected void btnRestoreSelected_Click(object sender, EventArgs e)
        {
            List<int> selectedIds = GetSelectedIds(gvArchived, "chkSelectArchived");

            if (selectedIds.Count == 0)
            {
                ShowMessage("Please select at least one completed enrolment to restore.", false);
                return;
            }

            int restoredCount = 0;

            foreach (int id in selectedIds)
            {
                if (RestoreEnrolment(id, false))
                    restoredCount++;
            }

            LoadArchivedEnrolments();
            ShowMessage(restoredCount + " completed enrolment(s) restored to active successfully.", true);
        }


        protected void btnExportArchived_Click(object sender, EventArgs e)
        {
            ExportArchivedEnrolments();
        }

        private void ExportArchivedEnrolments()
        {
            DataTable dt = GetArchivedEnrolmentsData();

            if (dt.Rows.Count == 0)
            {
                ShowMessage("No completed enrolment records found to export.", false);
                return;
            }

            string fileName = "Completed_Enrolments_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";

            StringBuilder csv = new StringBuilder();
            string[] exportColumns = new string[]
            {
                "EnrolmentId", "StudentNo", "StudentName", "CourseCode", "CourseName",
                "AcademicYear", "Semester", "Status", "RequestedAt", "EnrolledAt",
                "DroppedAt", "LastAction", "LastActionBy", "LastActionDate"
            };

            csv.AppendLine(string.Join(",", exportColumns));

            foreach (DataRow row in dt.Rows)
            {
                List<string> values = new List<string>();

                foreach (string column in exportColumns)
                {
                    object value = dt.Columns.Contains(column) ? row[column] : "";
                    values.Add(ToCsvValue(value));
                }

                csv.AppendLine(string.Join(",", values));
            }

            Response.Clear();
            Response.Buffer = true;
            Response.ClearContent();
            Response.ClearHeaders();
            Response.ContentType = "text/csv";
            Response.ContentEncoding = Encoding.UTF8;
            Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
            Response.Write("\uFEFF");
            Response.Write(csv.ToString());
            Response.End();
        }

        private string ToCsvValue(object value)
        {
            if (value == null || value == DBNull.Value)
                return "\"\"";

            string text = Convert.ToString(value);
            text = text.Replace("\"", "\"\"");
            return "\"" + text + "\"";
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

        private bool RestoreEnrolment(int enrolmentId, bool showResultMessage = true)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();

                try
                {
                    EnrolmentInfo info = GetEnrolmentInfo(conn, tran, enrolmentId);

                    if (info == null)
                        throw new Exception("Completed enrolment record not found.");

                    if (info.Status != "Completed")
                        throw new Exception("Only completed enrolments can be restored.");

                    if (HasApprovedDuplicate(conn, tran, info))
                        throw new Exception("This student already has an active enrolment for this course in the same academic year and semester.");

                    string updateSql = @"
                        UPDATE Enrolments
                        SET Status = 'Active'
                        WHERE EnrolmentId = @EnrolmentId
                          AND Status = 'Completed'";

                    using (SqlCommand updateCmd = new SqlCommand(updateSql, conn, tran))
                    {
                        updateCmd.Parameters.AddWithValue("@EnrolmentId", enrolmentId);
                        int affected = updateCmd.ExecuteNonQuery();

                        if (affected == 0)
                            throw new Exception("Unable to restore. The enrolment may already have been updated.");
                    }

                    InsertAuditLog(
                        conn,
                        tran,
                        CurrentUserId,
                        "Restored completed enrolment record",
                        enrolmentId,
                        "Status=Completed; Student=" + info.StudentName + "; Course=" + info.CourseCode,
                        "Status=Active; Record restored from completed enrolments"
                    );

                    tran.Commit();

                    if (showResultMessage)
                        ShowMessage("Enrolment restored successfully.", true);

                    return true;
                }
                catch (Exception ex)
                {
                    tran.Rollback();

                    if (showResultMessage)
                        ShowMessage("Error restoring enrolment: " + ex.Message, false);

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
                  AND Status = 'Active'
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
