using SIMS.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SIMS.Lecturer
{
    public partial class LecturerAttendance : LecturerBase
    {
        string connStr = ConfigurationManager.ConnectionStrings["SIMS_DB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            EnsureAuthenticated();

            if (!IsPostBack)
            {
                if (string.IsNullOrEmpty(Request.QueryString["CourseID"]) ||
                    !int.TryParse(Request.QueryString["CourseID"], out int courseId))
                {
                    Response.Redirect("LecturerCourses.aspx");
                    return;
                }

                var assignment = GetMostRecentAssignment(courseId);
                if (assignment.year <= 0 || assignment.semester <= 0)
                {
                    ShowError("No course assignment found. Please contact administrator.");
                    return;
                }

                hidCourseId.Value = courseId.ToString();
                hidAcademicYear.Value = assignment.year.ToString();
                hidSemester.Value = assignment.semester.ToString();

                LoadCourseHeader(courseId);
                txtAttendanceDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                LoadStudents(courseId, assignment.year, assignment.semester, DateTime.Now.ToString("yyyy-MM-dd"), "");
            }
        }

        private (int year, int semester) GetMostRecentAssignment(int courseId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = @"
                        SELECT TOP 1 AcademicYear, Semester
                        FROM CourseAssignments
                        WHERE CourseId = @CourseId
                          AND LecturerId = @LecturerId
                        ORDER BY AcademicYear DESC, Semester DESC, AssignedDate DESC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CourseId", courseId);
                        cmd.Parameters.AddWithValue("@LecturerId", CurrentLecturerId);

                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return (Convert.ToInt32(reader["AcademicYear"]), Convert.ToInt32(reader["Semester"]));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting assignment: {ex.Message}");
            }
            return (0, 0);
        }

        private void LoadCourseHeader(int courseId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = "SELECT CourseCode, CourseName FROM Courses WHERE CourseId = @CourseId";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CourseId", courseId);
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string code = reader["CourseCode"].ToString();
                                string name = reader["CourseName"].ToString();
                                litCourseName.Text = $"{code} - {name}";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading course header: {ex.Message}");
            }
        }

        private void LoadStudents(int courseId, int academicYear, int semester, string attendanceDate, string statusFilter)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = @"
                        SELECT
                            e.EnrolmentId,
                            s.StudentId,
                            s.StudentNo,
                            u.FullName,
                            u.Email,
                            p.ProgrammeName,
                            ISNULL(att.Status, 'Absent') AS Status
                        FROM Enrolments e
                        INNER JOIN Students s ON s.StudentId = e.StudentId
                        INNER JOIN Users u ON u.UserId = s.UserId
                        INNER JOIN Programmes p ON p.ProgrammeId = s.ProgrammeId
                        LEFT JOIN Attendance att 
                            ON att.EnrolmentId = e.EnrolmentId
                            AND CAST(att.AttendanceDate AS DATE) = @AttendanceDate
                        WHERE e.CourseId = @CourseId
                          AND e.AcademicYear = @AcademicYear
                          AND e.Semester = @Semester
                          AND e.Status = 'Active'";

                    if (!string.IsNullOrEmpty(statusFilter))
                    {
                        sql += " AND ISNULL(att.Status, 'Absent') = @StatusFilter";
                    }

                    sql += " ORDER BY s.StudentNo ASC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CourseId", courseId);
                        cmd.Parameters.AddWithValue("@AcademicYear", academicYear);
                        cmd.Parameters.AddWithValue("@Semester", semester);
                        cmd.Parameters.AddWithValue("@AttendanceDate", attendanceDate);

                        if (!string.IsNullOrEmpty(statusFilter))
                        {
                            cmd.Parameters.AddWithValue("@StatusFilter", statusFilter);
                        }

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            rptAttendance.DataSource = dt;
                            rptAttendance.DataBind();
                            pnlNoData.Visible = false;
                            CalculateAndDisplayStats(dt);
                        }
                        else
                        {
                            pnlNoData.Visible = true;
                            litPresentCount.Text = "0";
                            litAbsentCount.Text = "0";
                            litTotalCount.Text = "0";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading students: {ex.Message}");
                ShowError("Error loading student data: " + ex.Message);
                pnlNoData.Visible = true;
            }
        }

        private void CalculateAndDisplayStats(DataTable dt)
        {
            int present = 0;
            int absent = 0;

            foreach (DataRow row in dt.Rows)
            {
                if (row["Status"].ToString() == "Present")
                    present++;
                else
                    absent++;
            }

            litPresentCount.Text = present.ToString();
            litAbsentCount.Text = absent.ToString();
            litTotalCount.Text = dt.Rows.Count.ToString();
        }

        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            int courseId = int.Parse(hidCourseId.Value);
            int year = int.Parse(hidAcademicYear.Value);
            int semester = int.Parse(hidSemester.Value);
            string date = txtAttendanceDate.Text;
            string filter = ddlStatusFilter.SelectedValue;

            if (string.IsNullOrEmpty(date))
                date = DateTime.Now.ToString("yyyy-MM-dd");

            LoadStudents(courseId, year, semester, date, filter);
        }

        protected void btnMarkAllPresent_Click(object sender, EventArgs e)
        {
            Page.ClientScript.RegisterStartupScript(
                this.GetType(),
                "markAllPresent",
                @"
                var chkboxes = document.querySelectorAll('.attendance-checkbox');
                chkboxes.forEach(function (cb) { cb.checked = true; });
                document.getElementById('chkSelectAll').checked = true;
                ",
                true);
        }

        protected void btnMarkAllAbsent_Click(object sender, EventArgs e)
        {
            Page.ClientScript.RegisterStartupScript(
                this.GetType(),
                "markAllAbsent",
                @"
                var chkboxes = document.querySelectorAll('.attendance-checkbox');
                chkboxes.forEach(function (cb) { cb.checked = false; });
                document.getElementById('chkSelectAll').checked = false;
                ",
                true);
        }

        protected void btnSaveAttendance_Click(object sender, EventArgs e)
        {
            try
            {
                int courseId = int.Parse(hidCourseId.Value);
                int year = int.Parse(hidAcademicYear.Value);
                int semester = int.Parse(hidSemester.Value);
                string attendanceDate = txtAttendanceDate.Text;

                if (string.IsNullOrEmpty(attendanceDate))
                    attendanceDate = DateTime.Now.ToString("yyyy-MM-dd");

                var studentsRecorded = new List<int>();

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string deleteSql = @"
                        DELETE FROM Attendance
                        WHERE EnrolmentId IN (
                            SELECT e.EnrolmentId FROM Enrolments e
                            WHERE e.CourseId = @CourseId
                              AND e.AcademicYear = @AcademicYear
                              AND e.Semester = @Semester
                        )
                          AND CAST(AttendanceDate AS DATE) = @AttendanceDate";

                    using (SqlCommand deleteCmd = new SqlCommand(deleteSql, conn))
                    {
                        deleteCmd.Parameters.AddWithValue("@CourseId", courseId);
                        deleteCmd.Parameters.AddWithValue("@AcademicYear", year);
                        deleteCmd.Parameters.AddWithValue("@Semester", semester);
                        deleteCmd.Parameters.AddWithValue("@AttendanceDate", attendanceDate);

                        conn.Open();
                        deleteCmd.ExecuteNonQuery();
                        conn.Close();
                    }

                    if (Request.Form["chkAttendance"] != null)
                    {
                        string[] selectedIds = Request.Form["chkAttendance"].Split(',');

                        foreach (string enrolmentIdStr in selectedIds)
                        {
                            if (int.TryParse(enrolmentIdStr.Trim(), out int enrolmentId) && enrolmentId > 0)
                            {
                                string insertSql = @"
                                    INSERT INTO Attendance (EnrolmentId, AttendanceDate, Status, RecordedBy, RecordedAt)
                                    VALUES (@EnrolmentId, @AttendanceDate, 'Present', @RecordedBy, GETDATE())";

                                using (SqlCommand insertCmd = new SqlCommand(insertSql, conn))
                                {
                                    insertCmd.Parameters.AddWithValue("@EnrolmentId", enrolmentId);
                                    insertCmd.Parameters.AddWithValue("@AttendanceDate", attendanceDate);
                                    insertCmd.Parameters.AddWithValue("@RecordedBy", CurrentUserId);

                                    conn.Open();
                                    insertCmd.ExecuteNonQuery();
                                    conn.Close();

                                    using (SqlCommand getStudentCmd = new SqlCommand(
                                        "SELECT StudentId FROM Enrolments WHERE EnrolmentId = @EnrolmentId", conn))
                                    {
                                        getStudentCmd.Parameters.AddWithValue("@EnrolmentId", enrolmentId);
                                        conn.Open();
                                        object studentIdObj = getStudentCmd.ExecuteScalar();
                                        if (studentIdObj != null)
                                        {
                                            studentsRecorded.Add(Convert.ToInt32(studentIdObj));
                                        }
                                        conn.Close();
                                    }
                                }
                            }
                        }
                    }
                }

                if (studentsRecorded.Count > 0)
                {
                    try
                    {
                        var progressService = new AcademicProgressService(connStr);
                        var warningsTriggered = progressService.OnAttendanceRecorded(
                            courseId,
                            year,
                            semester,
                            CurrentLecturerId,
                            studentsRecorded.ToArray()
                        );

                        if (warningsTriggered.Count > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"Academic Progress Service: {warningsTriggered.Count} warnings triggered after attendance recorded");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error triggering academic progress analysis: {ex.Message}");
                    }
                }

                ShowSuccess("Attendance saved successfully!");
                LoadStudents(courseId, year, semester, attendanceDate, ddlStatusFilter.SelectedValue);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving attendance: {ex.Message}");
                ShowError("Error saving attendance: " + ex.Message);
            }
        }

        protected void btnExportCSV_Click(object sender, EventArgs e)
        {
            int courseId = int.Parse(hidCourseId.Value);
            int year = int.Parse(hidAcademicYear.Value);
            int semester = int.Parse(hidSemester.Value);
            string statusFilter = ddlStatusFilter.SelectedValue;

            DateTime attendanceDate;
            if (!DateTime.TryParse(txtAttendanceDate.Text, out attendanceDate))
            {
                attendanceDate = DateTime.Now;
            }

            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    // FIXED: Table name changed to Enrolments, joined with Users to access FullName and Email
                    string sql = @"
                SELECT 
                    s.StudentNo AS [Student ID],
                    u.FullName AS [Student Name],
                    u.Email AS [Email],
                    ISNULL(a.Status, 'Absent') AS [Attendance Status],
                    @AttendanceDate AS [Record Date]
                FROM Enrolments e
                INNER JOIN Students s ON e.StudentId = s.StudentId
                INNER JOIN Users u ON s.UserId = u.UserId
                LEFT JOIN Attendance a ON e.EnrolmentId = a.EnrolmentId 
                    AND CAST(a.AttendanceDate AS DATE) = CAST(@AttendanceDate AS DATE)
                WHERE e.CourseId = @CourseId 
                  AND e.AcademicYear = @AcademicYear 
                  AND e.Semester = @Semester
                  AND e.Status = 'Active'";

                    if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
                    {
                        if (statusFilter == "Absent")
                        {
                            sql += " AND (a.Status IS NULL OR a.Status = 'Absent')";
                        }
                        else
                        {
                            sql += " AND a.Status = @StatusFilter";
                        }
                    }

                    sql += " ORDER BY s.StudentNo ASC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CourseId", courseId);
                        cmd.Parameters.AddWithValue("@AcademicYear", year);
                        cmd.Parameters.AddWithValue("@Semester", semester);
                        cmd.Parameters.AddWithValue("@AttendanceDate", attendanceDate.Date);

                        if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All" && statusFilter != "Absent")
                        {
                            cmd.Parameters.AddWithValue("@StatusFilter", statusFilter);
                        }

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }

                if (dt.Rows.Count > 0)
                {
                    Response.Clear();
                    Response.Buffer = true;
                    Response.AddHeader("content-disposition", "attachment;filename=AttendanceReport.csv");
                    Response.Charset = "";
                    Response.ContentType = "application/text";

                    StringBuilder sb = new StringBuilder();

                    // Add headers
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        sb.Append(dt.Columns[i].ColumnName + (i == dt.Columns.Count - 1 ? "" : ","));
                    }
                    sb.Append("\r\n");

                    // Add rows
                    foreach (DataRow row in dt.Rows)
                    {
                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            string cellValue = row[i].ToString().Replace(",", ";").Replace("\r", "").Replace("\n", " ");
                            sb.Append(cellValue + (i == dt.Columns.Count - 1 ? "" : ","));
                        }
                        sb.Append("\r\n");
                    }

                    Response.Output.Write(sb.ToString());
                    Response.Flush();
                    Response.End();
                }
                else
                {
                    ShowError("No records available to export for the selected filters.");
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
                // Caught safely to prevent unexpected app standard stack dumps during Response.End()
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error exporting attendance CSV: {ex.Message}");
                ShowError("Error generating report dataset: " + ex.Message);
            }
        }

        protected void rptAttendance_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
        }

        private void ShowSuccess(string message)
        {
            pnlSuccess.Visible = true;
            litSuccessMsg.Text = message;
            pnlError.Visible = false;
        }

        private void ShowError(string message)
        {
            pnlError.Visible = true;
            litErrorMsg.Text = message;
            pnlSuccess.Visible = false;
        }
    }
}