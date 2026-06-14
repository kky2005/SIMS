using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI.WebControls;

namespace SIMS.HeadOfProgramme
{
    public partial class HOPManageCourses : HOPCrudBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            EnsureAuthenticated();
            if (!IsPostBack)
            {
                BindProgrammes();
                BindFilterProgrammes();
                BindLecturers();
                txtAcademicYear.Text = DateTime.Now.Year.ToString();
                BindGrid();
            }
        }

        private void BindProgrammes()
        {
            BindDropDown(ddlProgramme, "SELECT ProgrammeId, ProgrammeName FROM Programmes ORDER BY ProgrammeName", "ProgrammeName", "ProgrammeId");
        }

        private void BindFilterProgrammes()
        {
            BindDropDown(ddlFilterProgramme, "SELECT ProgrammeId, ProgrammeName FROM Programmes ORDER BY ProgrammeName", "ProgrammeName", "ProgrammeId");
            ddlFilterProgramme.Items.Insert(0, new ListItem("All Programmes", ""));
        }

        private void BindLecturers()
        {
            lbLecturers.DataSource = GetData(@"
                SELECT l.LecturerId,
                       u.FullName + ' (' + l.StaffNo + ')' AS LecturerDisplay
                FROM Lecturers l
                INNER JOIN Users u ON u.UserId = l.UserId
                WHERE ISNULL(u.IsActive, 1) = 1
                  AND UPPER(LTRIM(RTRIM(ISNULL(l.EmploymentStatus, 'Active')))) = 'ACTIVE'
                ORDER BY u.FullName");

            lbLecturers.DataTextField = "LecturerDisplay";
            lbLecturers.DataValueField = "LecturerId";
            lbLecturers.DataBind();
        }        private void BindGrid()
        {
            string sql = @"
                SELECT c.*, p.ProgrammeName,
                    CASE WHEN c.IsActive = 1 THEN 'Yes' ELSE 'No' END AS IsActiveText,
                    ISNULL(lecturers.LecturerNames, '-') AS LecturerNames,
                    ISNULL(assignments.AssignmentYears, '-') AS AssignmentYears
                FROM Courses c
                INNER JOIN Programmes p ON c.ProgrammeId = p.ProgrammeId
                OUTER APPLY
                (
                    SELECT STUFF((SELECT DISTINCT ', ' + u.FullName FROM CourseAssignments ca INNER JOIN Lecturers l ON l.LecturerId = ca.LecturerId INNER JOIN Users u ON u.UserId = l.UserId WHERE ca.CourseId = c.CourseId ORDER BY ', ' + u.FullName FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS LecturerNames
                ) lecturers
                OUTER APPLY
                (
                    SELECT STUFF((SELECT DISTINCT ', ' + CAST(ca.AcademicYear AS NVARCHAR(10)) + ' S' + CAST(ca.Semester AS NVARCHAR(10)) FROM CourseAssignments ca WHERE ca.CourseId = c.CourseId ORDER BY ', ' + CAST(ca.AcademicYear AS NVARCHAR(10)) + ' S' + CAST(ca.Semester AS NVARCHAR(10)) FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS AssignmentYears
                ) assignments
                WHERE 1 = 1";

            System.Collections.Generic.List<SqlParameter> parameters = new System.Collections.Generic.List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(txtFilterCourse.Text))
            {
                sql += " AND (c.CourseCode LIKE @Search OR c.CourseName LIKE @Search OR ISNULL(lecturers.LecturerNames, '') LIKE @Search)";
                parameters.Add(new SqlParameter("@Search", "%" + txtFilterCourse.Text.Trim() + "%"));
            }
            if (!string.IsNullOrEmpty(ddlFilterProgramme.SelectedValue))
            {
                sql += " AND c.ProgrammeId = @ProgrammeId";
                parameters.Add(new SqlParameter("@ProgrammeId", ddlFilterProgramme.SelectedValue));
            }
            if (!string.IsNullOrEmpty(ddlFilterActive.SelectedValue))
            {
                sql += " AND c.IsActive = @IsActive";
                parameters.Add(new SqlParameter("@IsActive", ddlFilterActive.SelectedValue));
            }

            sql += " ORDER BY c.CourseId DESC";
            gvCourses.DataSource = GetData(sql, parameters.ToArray());
            gvCourses.DataBind();
        }


        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtCourseCode.Text) || string.IsNullOrWhiteSpace(txtCourseName.Text))
                {
                    ShowMessage(lblMessage, "Code and name required.", false);
                    return;
                }

                short academicYear;
                byte semester;

                if (!short.TryParse(txtAcademicYear.Text.Trim(), out academicYear))
                {
                    ShowMessage(lblMessage, "Please enter a valid academic year, for example 2026.", false);
                    return;
                }

                if (!byte.TryParse(txtSemester.Text.Trim(), out semester))
                {
                    ShowMessage(lblMessage, "Please enter a valid semester.", false);
                    return;
                }

                string selectedLecturers = GetSelectedLecturerAuditValue();
                string newValue = "ProgrammeId=" + ddlProgramme.SelectedValue +
                                  "; Code=" + txtCourseCode.Text.Trim() +
                                  "; Name=" + txtCourseName.Text.Trim() +
                                  "; CreditHours=" + txtCreditHours.Text.Trim() +
                                  "; CourseSemester=" + txtSemester.Text.Trim() +
                                  "; IsActive=" + ddlIsActive.SelectedValue +
                                  "; AssignmentAcademicYear=" + academicYear +
                                  "; AssignmentSemester=" + semester +
                                  "; Lecturers=" + selectedLecturers;

                using (SqlConnection con = new SqlConnection(ConnStr))
                {
                    con.Open();
                    SqlTransaction tx = con.BeginTransaction();

                    try
                    {
                        int courseId;

                        if (string.IsNullOrEmpty(hfCourseId.Value))
                        {
                            using (SqlCommand cmd = new SqlCommand(@"
                                INSERT INTO Courses(ProgrammeId, CourseCode, CourseName, CreditHours, Semester, IsActive)
                                OUTPUT INSERTED.CourseId
                                VALUES(@P, @Code, @Name, @Credit, @Sem, @Active)", con, tx))
                            {
                                cmd.Parameters.AddWithValue("@P", ddlProgramme.SelectedValue);
                                cmd.Parameters.AddWithValue("@Code", txtCourseCode.Text.Trim());
                                cmd.Parameters.AddWithValue("@Name", txtCourseName.Text.Trim());
                                cmd.Parameters.AddWithValue("@Credit", txtCreditHours.Text.Trim());
                                cmd.Parameters.AddWithValue("@Sem", semester);
                                cmd.Parameters.AddWithValue("@Active", ddlIsActive.SelectedValue);

                                courseId = Convert.ToInt32(cmd.ExecuteScalar());
                            }

                            SaveCourseAssignments(con, tx, courseId, academicYear, semester);
                            InsertAuditLog(con, tx, "Created course", "Courses", courseId, "New course record", newValue);
                        }
                        else
                        {
                            courseId = Convert.ToInt32(hfCourseId.Value);
                            string oldValue = GetCourseAuditValue(con, tx, courseId);

                            using (SqlCommand cmd = new SqlCommand(@"
                                UPDATE Courses
                                SET ProgrammeId = @P,
                                    CourseCode = @Code,
                                    CourseName = @Name,
                                    CreditHours = @Credit,
                                    Semester = @Sem,
                                    IsActive = @Active
                                WHERE CourseId = @Id", con, tx))
                            {
                                cmd.Parameters.AddWithValue("@P", ddlProgramme.SelectedValue);
                                cmd.Parameters.AddWithValue("@Code", txtCourseCode.Text.Trim());
                                cmd.Parameters.AddWithValue("@Name", txtCourseName.Text.Trim());
                                cmd.Parameters.AddWithValue("@Credit", txtCreditHours.Text.Trim());
                                cmd.Parameters.AddWithValue("@Sem", semester);
                                cmd.Parameters.AddWithValue("@Active", ddlIsActive.SelectedValue);
                                cmd.Parameters.AddWithValue("@Id", courseId);
                                cmd.ExecuteNonQuery();
                            }

                            SaveCourseAssignments(con, tx, courseId, academicYear, semester);
                            InsertAuditLog(con, tx, "Updated course", "Courses", courseId, oldValue, newValue);
                        }

                        tx.Commit();
                        BindLecturers();
                        ClearForm();
                        BindGrid();
                        ShowMessage(lblMessage, "Course saved successfully.", true);
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage(lblMessage, ex.Message, false);
            }
        }

        protected void gvCourses_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                int id = Convert.ToInt32(e.CommandArgument);

                if (e.CommandName == "EditCourse")
                {
                    DataTable dt = GetData("SELECT * FROM Courses WHERE CourseId=@Id", new SqlParameter("@Id", id));
                    if (dt.Rows.Count == 0) return;

                    DataRow r = dt.Rows[0];

                    hfCourseId.Value = id.ToString();
                    ddlProgramme.SelectedValue = r["ProgrammeId"].ToString();
                    txtCourseCode.Text = r["CourseCode"].ToString();
                    txtCourseName.Text = r["CourseName"].ToString();
                    txtCreditHours.Text = r["CreditHours"].ToString();
                    txtSemester.Text = r["Semester"].ToString();
                    ddlIsActive.SelectedValue = Convert.ToBoolean(r["IsActive"]) ? "1" : "0";

                    short academicYear = GetLatestAssignmentAcademicYear(id);
                    txtAcademicYear.Text = academicYear == 0 ? DateTime.Now.Year.ToString() : academicYear.ToString();

                    LoadSelectedLecturers(id, Convert.ToInt16(txtAcademicYear.Text), Convert.ToByte(txtSemester.Text));
                }
                else if (e.CommandName == "DeleteCourse")
                {
                    using (SqlConnection con = new SqlConnection(ConnStr))
                    {
                        con.Open();
                        SqlTransaction tx = con.BeginTransaction();

                        try
                        {
                            string oldValue = GetCourseAuditValue(con, tx, id);

                            using (SqlCommand cmd = new SqlCommand(@"
                                UPDATE Courses
                                SET IsActive = 0
                                WHERE CourseId = @Id", con, tx))
                            {
                                cmd.Parameters.AddWithValue("@Id", id);
                                cmd.ExecuteNonQuery();
                            }

                            InsertAuditLog(con, tx, "Deactivated course", "Courses", id, oldValue, "IsActive=False; Course kept safely instead of deleted");
                            tx.Commit();
                        }
                        catch
                        {
                            tx.Rollback();
                            throw;
                        }
                    }

                    BindGrid();
                    ShowMessage(lblMessage, "Course deactivated successfully.", true);
                }
            }
            catch (Exception ex)
            {
                ShowMessage(lblMessage, "Deactivate failed. " + ex.Message, false);
            }
        }

        private void SaveCourseAssignments(SqlConnection con, SqlTransaction tx, int courseId, short academicYear, byte semester)
        {
            using (SqlCommand deleteCmd = new SqlCommand(@"
                DELETE FROM CourseAssignments
                WHERE CourseId = @CourseId
                  AND AcademicYear = @AcademicYear
                  AND Semester = @Semester", con, tx))
            {
                deleteCmd.Parameters.AddWithValue("@CourseId", courseId);
                deleteCmd.Parameters.AddWithValue("@AcademicYear", academicYear);
                deleteCmd.Parameters.AddWithValue("@Semester", semester);
                deleteCmd.ExecuteNonQuery();
            }

            foreach (ListItem item in lbLecturers.Items)
            {
                if (!item.Selected) continue;

                using (SqlCommand insertCmd = new SqlCommand(@"
                    INSERT INTO CourseAssignments(CourseId, LecturerId, AcademicYear, Semester, AssignedDate)
                    VALUES(@CourseId, @LecturerId, @AcademicYear, @Semester, CONVERT(date, GETDATE()))", con, tx))
                {
                    insertCmd.Parameters.AddWithValue("@CourseId", courseId);
                    insertCmd.Parameters.AddWithValue("@LecturerId", item.Value);
                    insertCmd.Parameters.AddWithValue("@AcademicYear", academicYear);
                    insertCmd.Parameters.AddWithValue("@Semester", semester);
                    insertCmd.ExecuteNonQuery();
                }
            }
        }

        private void LoadSelectedLecturers(int courseId, short academicYear, byte semester)
        {
            foreach (ListItem item in lbLecturers.Items)
                item.Selected = false;

            DataTable dt = GetData(@"
                SELECT LecturerId
                FROM CourseAssignments
                WHERE CourseId = @CourseId
                  AND AcademicYear = @AcademicYear
                  AND Semester = @Semester",
                new SqlParameter("@CourseId", courseId),
                new SqlParameter("@AcademicYear", academicYear),
                new SqlParameter("@Semester", semester));

            foreach (DataRow row in dt.Rows)
            {
                ListItem item = lbLecturers.Items.FindByValue(row["LecturerId"].ToString());
                if (item != null) item.Selected = true;
            }
        }

        private short GetLatestAssignmentAcademicYear(int courseId)
        {
            object value = ExecuteScalar(@"
                SELECT TOP 1 AcademicYear
                FROM CourseAssignments
                WHERE CourseId = @CourseId
                ORDER BY AcademicYear DESC, AssignedDate DESC",
                new SqlParameter("@CourseId", courseId));

            if (value == null || value == DBNull.Value)
                return 0;

            return Convert.ToInt16(value);
        }

        private string GetSelectedLecturerAuditValue()
        {
            StringBuilder sb = new StringBuilder();

            foreach (ListItem item in lbLecturers.Items)
            {
                if (item.Selected)
                {
                    if (sb.Length > 0) sb.Append(", ");
                    sb.Append(item.Text);
                }
            }

            return sb.Length == 0 ? "None" : sb.ToString();
        }

        private string GetCourseAuditValue(SqlConnection con, SqlTransaction tx, int id)
        {
            string sql = @"
                SELECT
                    c.ProgrammeId,
                    c.CourseCode,
                    c.CourseName,
                    c.CreditHours,
                    c.Semester,
                    c.IsActive,
                    ISNULL(lecturers.LecturerNames, 'None') AS LecturerNames
                FROM Courses c
                OUTER APPLY
                (
                    SELECT STUFF((
                        SELECT ', ' + u.FullName + ' (' + CAST(ca.AcademicYear AS NVARCHAR(10)) + ' S' + CAST(ca.Semester AS NVARCHAR(10)) + ')'
                        FROM CourseAssignments ca
                        INNER JOIN Lecturers l ON l.LecturerId = ca.LecturerId
                        INNER JOIN Users u ON u.UserId = l.UserId
                        WHERE ca.CourseId = c.CourseId
                        ORDER BY u.FullName
                        FOR XML PATH(''), TYPE
                    ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS LecturerNames
                ) lecturers
                WHERE c.CourseId = @Id";

            using (SqlCommand cmd = new SqlCommand(sql, con, tx))
            {
                cmd.Parameters.AddWithValue("@Id", id);

                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    if (!r.Read()) return "Record not found";

                    return "ProgrammeId=" + r["ProgrammeId"] +
                           "; Code=" + r["CourseCode"] +
                           "; Name=" + r["CourseName"] +
                           "; CreditHours=" + r["CreditHours"] +
                           "; CourseSemester=" + r["Semester"] +
                           "; IsActive=" + r["IsActive"] +
                           "; Lecturers=" + r["LecturerNames"];
                }
            }
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

        private void InsertAuditLog(SqlConnection con, SqlTransaction tx, string action, string tableAffected, int recordId, string oldValue, string newValue)
        {
            using (SqlCommand cmd = new SqlCommand(@"
                INSERT INTO AuditLogs(UserId, Action, TableAffected, RecordId, OldValue, NewValue, ActionDate)
                VALUES(@UserId, @Action, @TableAffected, @RecordId, @OldValue, @NewValue, SYSUTCDATETIME())", con, tx))
            {
                cmd.Parameters.AddWithValue("@UserId", CurrentUserId);
                cmd.Parameters.AddWithValue("@Action", action);
                cmd.Parameters.AddWithValue("@TableAffected", tableAffected);
                cmd.Parameters.AddWithValue("@RecordId", recordId);
                cmd.Parameters.AddWithValue("@OldValue", oldValue);
                cmd.Parameters.AddWithValue("@NewValue", newValue);
                cmd.ExecuteNonQuery();
            }
        }

        
        protected void btnFilter_Click(object sender, EventArgs e)
        {
            BindGrid();
        }

        protected void btnResetFilter_Click(object sender, EventArgs e)
        {
            txtFilterCourse.Text = "";
            ddlFilterProgramme.SelectedValue = "";
            ddlFilterActive.SelectedValue = "";
            BindGrid();
        }


        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            hfCourseId.Value = "";
            txtCourseCode.Text = "";
            txtCourseName.Text = "";
            txtCreditHours.Text = "3";
            txtSemester.Text = "";
            txtAcademicYear.Text = DateTime.Now.Year.ToString();
            ddlIsActive.SelectedValue = "1";

            if (ddlProgramme.Items.Count > 0)
                ddlProgramme.SelectedIndex = 0;

            foreach (ListItem item in lbLecturers.Items)
                item.Selected = false;
        }
    }
}
