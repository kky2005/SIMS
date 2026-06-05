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

            if (!IsPostBack)
            {
                BindGrid();
            }
        }

        private void BindGrid()
        {
            gvCalendar.DataSource = GetData(@"
                SELECT CalendarId, EventName, EventType, StartDate, EndDate, AcademicYear, Semester, Description
                FROM AcademicCalendar
                ORDER BY StartDate DESC
            ");

            gvCalendar.DataBind();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtEventName.Text) ||
                    string.IsNullOrWhiteSpace(txtStartDate.Text) ||
                    string.IsNullOrWhiteSpace(txtEndDate.Text) ||
                    string.IsNullOrWhiteSpace(txtAcademicYear.Text))
                {
                    ShowMessage(lblMessage, "Please fill in Event Name, Start Date, End Date, and Academic Year.", false);
                    return;
                }

                DateTime startDate;
                DateTime endDate;
                short academicYear;
                byte semester;

                if (!DateTime.TryParse(txtStartDate.Text, out startDate) || !DateTime.TryParse(txtEndDate.Text, out endDate))
                {
                    ShowMessage(lblMessage, "Please enter valid start and end dates.", false);
                    return;
                }

                if (endDate < startDate)
                {
                    ShowMessage(lblMessage, "End Date cannot be earlier than Start Date.", false);
                    return;
                }

                if (!short.TryParse(txtAcademicYear.Text, out academicYear))
                {
                    ShowMessage(lblMessage, "Please enter a valid academic year, for example 2026.", false);
                    return;
                }

                object semesterValue = DBNull.Value;
                if (!string.IsNullOrWhiteSpace(txtSemester.Text))
                {
                    if (!byte.TryParse(txtSemester.Text, out semester))
                    {
                        ShowMessage(lblMessage, "Please enter a valid semester number.", false);
                        return;
                    }

                    semesterValue = semester;
                }

                if (string.IsNullOrEmpty(hfCalendarId.Value))
                {
                    Execute(@"
                        INSERT INTO AcademicCalendar
                        (EventName, EventType, StartDate, EndDate, AcademicYear, Semester, Description)
                        VALUES
                        (@EventName, @EventType, @StartDate, @EndDate, @AcademicYear, @Semester, @Description)
                    ",
                    new SqlParameter("@EventName", txtEventName.Text.Trim()),
                    new SqlParameter("@EventType", ddlEventType.SelectedValue),
                    new SqlParameter("@StartDate", startDate),
                    new SqlParameter("@EndDate", endDate),
                    new SqlParameter("@AcademicYear", academicYear),
                    new SqlParameter("@Semester", semesterValue),
                    new SqlParameter("@Description", string.IsNullOrWhiteSpace(txtDescription.Text) ? (object)DBNull.Value : txtDescription.Text.Trim()));

                    ShowMessage(lblMessage, "Academic calendar event added successfully.", true);
                }
                else
                {
                    Execute(@"
                        UPDATE AcademicCalendar
                        SET EventName = @EventName,
                            EventType = @EventType,
                            StartDate = @StartDate,
                            EndDate = @EndDate,
                            AcademicYear = @AcademicYear,
                            Semester = @Semester,
                            Description = @Description
                        WHERE CalendarId = @CalendarId
                    ",
                    new SqlParameter("@EventName", txtEventName.Text.Trim()),
                    new SqlParameter("@EventType", ddlEventType.SelectedValue),
                    new SqlParameter("@StartDate", startDate),
                    new SqlParameter("@EndDate", endDate),
                    new SqlParameter("@AcademicYear", academicYear),
                    new SqlParameter("@Semester", semesterValue),
                    new SqlParameter("@Description", string.IsNullOrWhiteSpace(txtDescription.Text) ? (object)DBNull.Value : txtDescription.Text.Trim()),
                    new SqlParameter("@CalendarId", hfCalendarId.Value));

                    ShowMessage(lblMessage, "Academic calendar event updated successfully.", true);
                }

                ClearForm();
                BindGrid();
            }
            catch (Exception ex)
            {
                ShowMessage(lblMessage, ex.Message, false);
            }
        }

        protected void gvCalendar_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "EditRow")
            {
                DataTable dt = GetData(@"
                    SELECT *
                    FROM AcademicCalendar
                    WHERE CalendarId = @CalendarId
                ",
                new SqlParameter("@CalendarId", id));

                if (dt.Rows.Count == 0) return;

                DataRow r = dt.Rows[0];

                hfCalendarId.Value = r["CalendarId"].ToString();
                txtEventName.Text = r["EventName"].ToString();
                ddlEventType.SelectedValue = r["EventType"].ToString();
                txtStartDate.Text = Convert.ToDateTime(r["StartDate"]).ToString("yyyy-MM-dd");
                txtEndDate.Text = Convert.ToDateTime(r["EndDate"]).ToString("yyyy-MM-dd");
                txtAcademicYear.Text = r["AcademicYear"].ToString();
                txtSemester.Text = r["Semester"] == DBNull.Value ? "" : r["Semester"].ToString();
                txtDescription.Text = r["Description"] == DBNull.Value ? "" : r["Description"].ToString();
            }
            else if (e.CommandName == "DeleteRow")
            {
                Execute(@"
                    DELETE FROM AcademicCalendar
                    WHERE CalendarId = @CalendarId
                ",
                new SqlParameter("@CalendarId", id));

                ClearForm();
                BindGrid();

                ShowMessage(lblMessage, "Academic calendar event deleted successfully.", true);
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            hfCalendarId.Value = "";
            txtEventName.Text = "";
            ddlEventType.SelectedIndex = 0;
            txtStartDate.Text = "";
            txtEndDate.Text = "";
            txtAcademicYear.Text = "";
            txtSemester.Text = "";
            txtDescription.Text = "";
        }
    }
}
