using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
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


        protected void btnPreviewAdmissionCsv_Click(object sender, EventArgs e)
        {
            try
            {
                if (!fuAdmissionCsv.HasFile)
                {
                    ShowMessage(lblMessage, "Please choose an admitted admissions CSV file first.", false);
                    return;
                }

                string ext = Path.GetExtension(fuAdmissionCsv.FileName).ToLower();
                if (ext != ".csv")
                {
                    ShowMessage(lblMessage, "Only CSV files are supported.", false);
                    return;
                }

                DataTable preview = BuildImportPreviewTable();

                using (StreamReader reader = new StreamReader(fuAdmissionCsv.FileContent, Encoding.UTF8, true))
                {
                    string csvText = reader.ReadToEnd();
                    DataTable csvTable = ReadCsvToTable(csvText);

                    if (csvTable.Rows.Count == 0)
                    {
                        ShowMessage(lblMessage, "The CSV file has no records.", false);
                        return;
                    }

                    foreach (DataRow row in csvTable.Rows)
                    {
                        string status = GetCsvValue(row, "Status");
                        if (!status.Equals("Admitted", StringComparison.OrdinalIgnoreCase))
                            continue;

                        DataRow newRow = preview.NewRow();
                        newRow["AdmissionId"] = GetCsvValue(row, "AdmissionId");
                        newRow["StudentNo"] = CleanStudentNo(GetCsvValue(row, "StudentNo"));
                        newRow["StudentName"] = GetCsvValue(row, "StudentName");
                        newRow["ApplicantEmail"] = GetCsvValue(row, "ApplicantEmail");
                        newRow["PhoneNumber"] = GetCsvValue(row, "PhoneNumber");
                        newRow["ProgrammeName"] = GetCsvValue(row, "ProgrammeName");
                        newRow["IntakeYear"] = GetCsvValue(row, "IntakeYear");
                        newRow["IntakeSemester"] = GetCsvValue(row, "IntakeSemester");
                        newRow["Status"] = "Admitted";
                        newRow["ImportRemark"] = ValidateImportPreviewRow(newRow);
                        preview.Rows.Add(newRow);
                    }
                }

                if (preview.Rows.Count == 0)
                {
                    ShowMessage(lblMessage, "No admitted admission records were found in the CSV.", false);
                    return;
                }

                Session["AdmissionImportPreview"] = preview;
                gvImportPreview.DataSource = preview;
                gvImportPreview.DataBind();
                pnlImportPreview.Visible = true;
                btnConfirmImportStudents.Visible = true;
                btnCancelImport.Visible = true;

                ShowMessage(lblMessage, preview.Rows.Count + " admitted record(s) loaded for preview. Please check them before confirming.", true);
            }
            catch (Exception ex)
            {
                ShowMessage(lblMessage, "CSV preview failed. " + ex.Message, false);
            }
        }

        protected void btnConfirmImportStudents_Click(object sender, EventArgs e)
        {
            DataTable preview = Session["AdmissionImportPreview"] as DataTable;

            if (preview == null || preview.Rows.Count == 0)
            {
                ShowMessage(lblMessage, "No CSV preview found. Please upload and preview the file again.", false);
                return;
            }

            int imported = 0;
            int skipped = 0;
            List<string> errors = new List<string>();

            using (SqlConnection con = new SqlConnection(ConnStr))
            {
                con.Open();

                foreach (DataRow row in preview.Rows)
                {
                    SqlTransaction tx = con.BeginTransaction();

                    try
                    {
                        string validation = ValidateImportPreviewRow(row);
                        if (validation != "Ready")
                        {
                            skipped++;
                            tx.Rollback();
                            errors.Add(row["StudentName"] + ": " + validation);
                            continue;
                        }

                        int admissionId = Convert.ToInt32(row["AdmissionId"]);
                        string fullName = Convert.ToString(row["StudentName"]).Trim();
                        string email = Convert.ToString(row["ApplicantEmail"]).Trim();
                        string phone = Convert.ToString(row["PhoneNumber"]).Trim();
                        string programmeName = Convert.ToString(row["ProgrammeName"]).Trim();
                        int intakeYear = Convert.ToInt32(row["IntakeYear"]);
                        int intakeSemester = Convert.ToInt32(row["IntakeSemester"]);
                        int programmeId = GetProgrammeIdByName(con, tx, programmeName);

                        if (programmeId == 0)
                            throw new Exception("Programme not found: " + programmeName);

                        if (EmailExists(con, tx, email))
                            throw new Exception("Email already exists in Users.");

                        string studentNo = Convert.ToString(row["StudentNo"]).Trim();
                        if (string.IsNullOrWhiteSpace(studentNo) || studentNo == "-")
                            studentNo = GenerateStudentNo(con, tx, intakeYear);

                        if (StudentNoExists(con, tx, studentNo))
                            throw new Exception("Student No already exists: " + studentNo);

                        string nationalId = GetNationalIdByAdmissionId(con, tx, admissionId);

                        if (string.IsNullOrWhiteSpace(nationalId))
                            throw new Exception("National ID not found for Admission ID " + admissionId);

                        SqlCommand userCmd = new SqlCommand(
                            "INSERT INTO Users(RoleId,FullName,Email,PasswordHash,Phone,IsActive) OUTPUT INSERTED.UserId VALUES(@Role,@Name,@Email,@Pass,@Phone,1)",
                            con,
                            tx
                        );
                        userCmd.Parameters.AddWithValue("@Role", GetRoleId("Student"));
                        userCmd.Parameters.AddWithValue("@Name", fullName);
                        userCmd.Parameters.AddWithValue("@Email", email);
                        userCmd.Parameters.AddWithValue("@Pass", HashPassword(nationalId));
                        userCmd.Parameters.AddWithValue("@Phone", phone);
                        int userId = Convert.ToInt32(userCmd.ExecuteScalar());

                        SqlCommand studentCmd = new SqlCommand(
                            "INSERT INTO Students(UserId,ProgrammeId,StudentNo,IntakeYear,IntakeSemester,AdmissionDate,CurrentSemester,Status) OUTPUT INSERTED.StudentId VALUES(@UserId,@ProgrammeId,@StudentNo,@IntakeYear,@IntakeSemester,SYSUTCDATETIME(),1,'Active')",
                            con,
                            tx
                        );
                        studentCmd.Parameters.AddWithValue("@UserId", userId);
                        studentCmd.Parameters.AddWithValue("@ProgrammeId", programmeId);
                        studentCmd.Parameters.AddWithValue("@StudentNo", studentNo);
                        studentCmd.Parameters.AddWithValue("@IntakeYear", intakeYear);
                        studentCmd.Parameters.AddWithValue("@IntakeSemester", intakeSemester);
                        int studentId = Convert.ToInt32(studentCmd.ExecuteScalar());

                        SqlCommand updateAdmissionCmd = new SqlCommand(
                            "UPDATE Admissions SET StudentId=@StudentId WHERE AdmissionId=@AdmissionId AND Status='Admitted'",
                            con,
                            tx
                        );
                        updateAdmissionCmd.Parameters.AddWithValue("@StudentId", studentId);
                        updateAdmissionCmd.Parameters.AddWithValue("@AdmissionId", admissionId);
                        updateAdmissionCmd.ExecuteNonQuery();

                        InsertAuditLog(
                            con,
                            tx,
                            "Imported admitted admission as student",
                            "Students",
                            studentId,
                            "Imported from Admissions CSV AdmissionId=" + admissionId,
                            "Name=" + fullName + "; Email=" + email + "; StudentNo=" + studentNo + "; ProgrammeId=" + programmeId + "; UserId=" + userId + "; DefaultPassword=NationalId"
                        );

                        tx.Commit();
                        imported++;
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        skipped++;
                        errors.Add(row["StudentName"] + ": " + ex.Message);
                    }
                }
            }

            Session["AdmissionImportPreview"] = null;
            pnlImportPreview.Visible = false;
            btnConfirmImportStudents.Visible = false;
            btnCancelImport.Visible = false;
            BindGrid();

            string message = imported + " student(s) imported successfully. Default password is the student's National ID.";
            if (skipped > 0)
                message += " " + skipped + " record(s) skipped. " + string.Join(" | ", errors.ToArray());

            ShowMessage(lblMessage, message, imported > 0);
        }

        protected void btnCancelImport_Click(object sender, EventArgs e)
        {
            Session["AdmissionImportPreview"] = null;
            pnlImportPreview.Visible = false;
            btnConfirmImportStudents.Visible = false;
            btnCancelImport.Visible = false;
            gvImportPreview.DataSource = null;
            gvImportPreview.DataBind();
            ShowMessage(lblMessage, "CSV import preview cancelled.", true);
        }

        private DataTable BuildImportPreviewTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("AdmissionId");
            dt.Columns.Add("StudentNo");
            dt.Columns.Add("StudentName");
            dt.Columns.Add("ApplicantEmail");
            dt.Columns.Add("PhoneNumber");
            dt.Columns.Add("ProgrammeName");
            dt.Columns.Add("IntakeYear");
            dt.Columns.Add("IntakeSemester");
            dt.Columns.Add("Status");
            dt.Columns.Add("ImportRemark");
            return dt;
        }

        private DataTable ReadCsvToTable(string csvText)
        {
            DataTable dt = new DataTable();
            List<List<string>> rows = ParseCsv(csvText);

            if (rows.Count == 0)
                return dt;

            foreach (string header in rows[0])
                dt.Columns.Add(header.Trim());

            for (int i = 1; i < rows.Count; i++)
            {
                if (rows[i].Count == 1 && string.IsNullOrWhiteSpace(rows[i][0]))
                    continue;

                DataRow dr = dt.NewRow();

                for (int c = 0; c < dt.Columns.Count; c++)
                {
                    dr[c] = c < rows[i].Count ? rows[i][c] : "";
                }

                dt.Rows.Add(dr);
            }

            return dt;
        }

        private List<List<string>> ParseCsv(string csvText)
        {
            List<List<string>> rows = new List<List<string>>();
            List<string> row = new List<string>();
            StringBuilder value = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < csvText.Length; i++)
            {
                char ch = csvText[i];

                if (ch == '"')
                {
                    if (inQuotes && i + 1 < csvText.Length && csvText[i + 1] == '"')
                    {
                        value.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (ch == ',' && !inQuotes)
                {
                    row.Add(value.ToString());
                    value.Length = 0;
                }
                else if ((ch == '\r' || ch == '\n') && !inQuotes)
                {
                    if (ch == '\r' && i + 1 < csvText.Length && csvText[i + 1] == '\n')
                        i++;

                    row.Add(value.ToString());
                    rows.Add(row);
                    row = new List<string>();
                    value.Length = 0;
                }
                else
                {
                    value.Append(ch);
                }
            }

            if (value.Length > 0 || row.Count > 0)
            {
                row.Add(value.ToString());
                rows.Add(row);
            }

            if (rows.Count > 0 && rows[0].Count > 0)
                rows[0][0] = rows[0][0].TrimStart('\uFEFF');

            return rows;
        }

        private string GetCsvValue(DataRow row, string column)
        {
            if (!row.Table.Columns.Contains(column) || row[column] == DBNull.Value)
                return "";

            return Convert.ToString(row[column]).Trim();
        }

        private string CleanStudentNo(string studentNo)
        {
            if (string.IsNullOrWhiteSpace(studentNo) || studentNo.Trim() == "-")
                return "";

            return studentNo.Trim();
        }

        private string ValidateImportPreviewRow(DataRow row)
        {
            int tempInt;

            if (!int.TryParse(Convert.ToString(row["AdmissionId"]), out tempInt))
                return "Invalid AdmissionId";

            if (string.IsNullOrWhiteSpace(Convert.ToString(row["StudentName"])))
                return "Missing name";

            if (string.IsNullOrWhiteSpace(Convert.ToString(row["ApplicantEmail"])))
                return "Missing email";

            if (string.IsNullOrWhiteSpace(Convert.ToString(row["ProgrammeName"])))
                return "Missing programme";

            if (!int.TryParse(Convert.ToString(row["IntakeYear"]), out tempInt))
                return "Invalid intake year";

            if (!int.TryParse(Convert.ToString(row["IntakeSemester"]), out tempInt))
                return "Invalid intake semester";

            return "Ready";
        }

        private int GetProgrammeIdByName(SqlConnection con, SqlTransaction tx, string programmeName)
        {
            SqlCommand cmd = new SqlCommand("SELECT ProgrammeId FROM Programmes WHERE ProgrammeName=@ProgrammeName", con, tx);
            cmd.Parameters.AddWithValue("@ProgrammeName", programmeName);
            object result = cmd.ExecuteScalar();
            return result == null ? 0 : Convert.ToInt32(result);
        }

        private string GetNationalIdByAdmissionId(SqlConnection con, SqlTransaction tx, int admissionId)
        {
            SqlCommand cmd = new SqlCommand(@"
        SELECT NationalId
        FROM Admissions
        WHERE AdmissionId = @AdmissionId",
                con,
                tx);

            cmd.Parameters.AddWithValue("@AdmissionId", admissionId);

            object result = cmd.ExecuteScalar();

            return result == null ? "" : result.ToString().Trim();
        }

        private bool EmailExists(SqlConnection con, SqlTransaction tx, string email)
        {
            SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Email=@Email", con, tx);
            cmd.Parameters.AddWithValue("@Email", email);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        private bool StudentNoExists(SqlConnection con, SqlTransaction tx, string studentNo)
        {
            SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Students WHERE StudentNo=@StudentNo", con, tx);
            cmd.Parameters.AddWithValue("@StudentNo", studentNo);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        private string GenerateStudentNo(SqlConnection con, SqlTransaction tx, int intakeYear)
        {
            string prefix = "S" + intakeYear.ToString().Substring(2);
            SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Students WHERE StudentNo LIKE @Prefix", con, tx);
            cmd.Parameters.AddWithValue("@Prefix", prefix + "%");
            int count = Convert.ToInt32(cmd.ExecuteScalar()) + 1;

            string studentNo;
            do
            {
                studentNo = prefix + count.ToString("0000");
                count++;
            }
            while (StudentNoExists(con, tx, studentNo));

            return studentNo;
        }


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
