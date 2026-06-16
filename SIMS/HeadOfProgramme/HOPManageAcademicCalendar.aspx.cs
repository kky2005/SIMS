using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace SIMS.HeadOfProgramme
{
    public partial class HOPManageAcademicCalendar : HOPCrudBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            EnsureAuthenticated();
            if (!IsPostBack) BindGrid();
        }        private void BindGrid()
        {
            string sql = @"SELECT CalendarId, EventName, EventType, StartDate, EndDate, AcademicYear, Semester, Description
                           FROM AcademicCalendar
                           WHERE 1 = 1";
            System.Collections.Generic.List<SqlParameter> parameters = new System.Collections.Generic.List<SqlParameter>();
            if (!string.IsNullOrWhiteSpace(txtFilterEvent.Text))
            {
                sql += " AND (EventName LIKE @Search OR ISNULL(Description, '') LIKE @Search)";
                parameters.Add(new SqlParameter("@Search", "%" + txtFilterEvent.Text.Trim() + "%"));
            }
            if (!string.IsNullOrEmpty(ddlFilterEventType.SelectedValue))
            {
                sql += " AND EventType = @EventType";
                parameters.Add(new SqlParameter("@EventType", ddlFilterEventType.SelectedValue));
            }
            if (!string.IsNullOrWhiteSpace(txtFilterAcademicYear.Text))
            {
                short year;
                if (short.TryParse(txtFilterAcademicYear.Text.Trim(), out year))
                {
                    sql += " AND AcademicYear = @AcademicYear";
                    parameters.Add(new SqlParameter("@AcademicYear", year));
                }
            }
            sql += " ORDER BY StartDate DESC";
            gvCalendar.DataSource = GetData(sql, parameters.ToArray());
            gvCalendar.DataBind();
        }


        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtEventName.Text) || string.IsNullOrWhiteSpace(txtStartDate.Text) || string.IsNullOrWhiteSpace(txtEndDate.Text) || string.IsNullOrWhiteSpace(txtAcademicYear.Text))
                { ShowMessage(lblMessage, "Please fill in Event Name, Start Date, End Date, and Academic Year.", false); return; }

                DateTime startDate, endDate; short academicYear; byte semester;
                if (!DateTime.TryParse(txtStartDate.Text, out startDate) || !DateTime.TryParse(txtEndDate.Text, out endDate)) { ShowMessage(lblMessage, "Please enter valid start and end dates.", false); return; }
                if (endDate < startDate) { ShowMessage(lblMessage, "End Date cannot be earlier than Start Date.", false); return; }
                if (!short.TryParse(txtAcademicYear.Text, out academicYear)) { ShowMessage(lblMessage, "Please enter a valid academic year, for example 2026.", false); return; }

                object semesterValue = DBNull.Value;
                if (!string.IsNullOrWhiteSpace(txtSemester.Text))
                {
                    if (!byte.TryParse(txtSemester.Text, out semester)) { ShowMessage(lblMessage, "Please enter a valid semester number.", false); return; }
                    semesterValue = semester;
                }

                string newValue = "EventName=" + txtEventName.Text.Trim() + "; EventType=" + ddlEventType.SelectedValue + "; StartDate=" + startDate.ToString("yyyy-MM-dd") + "; EndDate=" + endDate.ToString("yyyy-MM-dd") + "; AcademicYear=" + academicYear + "; Semester=" + (semesterValue == DBNull.Value ? "NULL" : semesterValue.ToString());

                if (string.IsNullOrEmpty(hfCalendarId.Value))
                {
                    int newId = Convert.ToInt32(ExecuteScalar(@"INSERT INTO AcademicCalendar(EventName, EventType, StartDate, EndDate, AcademicYear, Semester, Description)
                        OUTPUT INSERTED.CalendarId VALUES(@EventName, @EventType, @StartDate, @EndDate, @AcademicYear, @Semester, @Description)",
                        new SqlParameter("@EventName", txtEventName.Text.Trim()), new SqlParameter("@EventType", ddlEventType.SelectedValue),
                        new SqlParameter("@StartDate", startDate), new SqlParameter("@EndDate", endDate), new SqlParameter("@AcademicYear", academicYear),
                        new SqlParameter("@Semester", semesterValue), new SqlParameter("@Description", string.IsNullOrWhiteSpace(txtDescription.Text) ? (object)DBNull.Value : txtDescription.Text.Trim())));
                    InsertAuditLog("Created academic calendar event", "AcademicCalendar", newId, "New academic calendar record", newValue);
                    ShowMessage(lblMessage, "Academic calendar event added successfully.", true);
                }
                else
                {
                    int id = Convert.ToInt32(hfCalendarId.Value);
                    string oldValue = GetCalendarAuditValue(id);
                    Execute(@"UPDATE AcademicCalendar SET EventName=@EventName, EventType=@EventType, StartDate=@StartDate, EndDate=@EndDate, AcademicYear=@AcademicYear, Semester=@Semester, Description=@Description WHERE CalendarId=@CalendarId",
                        new SqlParameter("@EventName", txtEventName.Text.Trim()), new SqlParameter("@EventType", ddlEventType.SelectedValue),
                        new SqlParameter("@StartDate", startDate), new SqlParameter("@EndDate", endDate), new SqlParameter("@AcademicYear", academicYear),
                        new SqlParameter("@Semester", semesterValue), new SqlParameter("@Description", string.IsNullOrWhiteSpace(txtDescription.Text) ? (object)DBNull.Value : txtDescription.Text.Trim()),
                        new SqlParameter("@CalendarId", id));
                    InsertAuditLog("Updated academic calendar event", "AcademicCalendar", id, oldValue, newValue);
                    ShowMessage(lblMessage, "Academic calendar event updated successfully.", true);
                }

                ClearForm(); BindGrid();
            }
            catch (Exception ex) { ShowMessage(lblMessage, ex.Message, false); }
        }

        protected void gvCalendar_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id = Convert.ToInt32(e.CommandArgument);
            if (e.CommandName == "EditRow")
            {
                DataTable dt = GetData("SELECT * FROM AcademicCalendar WHERE CalendarId=@CalendarId", new SqlParameter("@CalendarId", id));
                if (dt.Rows.Count == 0) return;
                DataRow r = dt.Rows[0];
                hfCalendarId.Value = r["CalendarId"].ToString(); txtEventName.Text = r["EventName"].ToString(); ddlEventType.SelectedValue = r["EventType"].ToString();
                txtStartDate.Text = Convert.ToDateTime(r["StartDate"]).ToString("yyyy-MM-dd"); txtEndDate.Text = Convert.ToDateTime(r["EndDate"]).ToString("yyyy-MM-dd");
                txtAcademicYear.Text = r["AcademicYear"].ToString(); txtSemester.Text = r["Semester"] == DBNull.Value ? "" : r["Semester"].ToString(); txtDescription.Text = r["Description"] == DBNull.Value ? "" : r["Description"].ToString();
            }
            else if (e.CommandName == "DeleteRow")
            {
                string oldValue = GetCalendarAuditValue(id);
                Execute("DELETE FROM AcademicCalendar WHERE CalendarId=@CalendarId", new SqlParameter("@CalendarId", id));
                InsertAuditLog("Deleted academic calendar event", "AcademicCalendar", id, oldValue, "Record deleted from AcademicCalendar");
                ClearForm(); BindGrid(); ShowMessage(lblMessage, "Academic calendar event deleted successfully.", true);
            }
        }

        private string GetCalendarAuditValue(int id)
        {
            DataTable dt = GetData("SELECT EventName, EventType, StartDate, EndDate, AcademicYear, Semester FROM AcademicCalendar WHERE CalendarId=@Id", new SqlParameter("@Id", id));
            if (dt.Rows.Count == 0) return "Record not found";
            DataRow r = dt.Rows[0];
            return "EventName=" + r["EventName"] + "; EventType=" + r["EventType"] + "; StartDate=" + Convert.ToDateTime(r["StartDate"]).ToString("yyyy-MM-dd") + "; EndDate=" + Convert.ToDateTime(r["EndDate"]).ToString("yyyy-MM-dd") + "; AcademicYear=" + r["AcademicYear"] + "; Semester=" + (r["Semester"] == DBNull.Value ? "NULL" : r["Semester"].ToString());
        }
        private object ExecuteScalar(string sql, params SqlParameter[] parameters) { using (SqlConnection con = new SqlConnection(ConnStr)) using (SqlCommand cmd = new SqlCommand(sql, con)) { if (parameters != null) cmd.Parameters.AddRange(parameters); con.Open(); return cmd.ExecuteScalar(); } }
        private void InsertAuditLog(string action, string tableAffected, int recordId, string oldValue, string newValue) { Execute(@"INSERT INTO AuditLogs(UserId, Action, TableAffected, RecordId, OldValue, NewValue, ActionDate) VALUES(@UserId,@Action,@TableAffected,@RecordId,@OldValue,@NewValue,SYSUTCDATETIME())", new SqlParameter("@UserId", CurrentUserId), new SqlParameter("@Action", action), new SqlParameter("@TableAffected", tableAffected), new SqlParameter("@RecordId", recordId), new SqlParameter("@OldValue", oldValue), new SqlParameter("@NewValue", newValue)); }
        
        protected void btnFilter_Click(object sender, EventArgs e)
        {
            BindGrid();
        }

        protected void btnResetFilter_Click(object sender, EventArgs e)
        {
            txtFilterEvent.Text = "";
            ddlFilterEventType.SelectedValue = "";
            txtFilterAcademicYear.Text = "";
            BindGrid();
        }


        protected void btnClear_Click(object sender, EventArgs e) { ClearForm(); }
        private void ClearForm() { hfCalendarId.Value = ""; txtEventName.Text = ""; ddlEventType.SelectedIndex = 0; txtStartDate.Text = ""; txtEndDate.Text = ""; txtAcademicYear.Text = ""; txtSemester.Text = ""; txtDescription.Text = ""; }
    }
}
