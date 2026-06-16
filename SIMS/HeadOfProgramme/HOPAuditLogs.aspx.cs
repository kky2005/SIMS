using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace SIMS.HeadOfProgramme
{
    public partial class HOPAuditLogs : HOPBase
    {
        private string ConnStr
        {
            get { return ConfigurationManager.ConnectionStrings["SIMS_DB"].ConnectionString; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                lblMessage.Visible = false;
                LoadAuditLogs();
            }
        }

        private void LoadAuditLogs()
        {
            using (SqlConnection con = new SqlConnection(ConnStr))
            {
                string query = @"
                    SELECT 
                        A.LogId,
                        A.UserId,
                        ISNULL(U.FullName, 'Unknown User') AS FullName,
                        ISNULL(U.Email, '-') AS Email,
                        A.Action,
                        A.TableAffected,
                        A.RecordId,
                        A.OldValue,
                        A.NewValue,
                        A.ActionDate
                    FROM AuditLogs A
                    LEFT JOIN Users U ON A.UserId = U.UserId
                    WHERE
                        (@Search = '' OR
                         A.Action LIKE '%' + @Search + '%' OR
                         A.TableAffected LIKE '%' + @Search + '%' OR
                         U.FullName LIKE '%' + @Search + '%' OR
                         U.Email LIKE '%' + @Search + '%')
                    ORDER BY A.ActionDate DESC, A.LogId DESC";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Search", txtSearch.Text.Trim());

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        gvAuditLogs.DataSource = dt;
                        gvAuditLogs.DataBind();
                    }
                }
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            lblMessage.Visible = false;
            gvAuditLogs.PageIndex = 0;
            LoadAuditLogs();
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            lblMessage.Visible = false;
            txtSearch.Text = "";
            gvAuditLogs.PageIndex = 0;
            LoadAuditLogs();
        }

        protected void gvAuditLogs_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvAuditLogs.PageIndex = e.NewPageIndex;
            LoadAuditLogs();
        }

        protected void gvAuditLogs_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DeleteLog")
            {
                int logId;

                if (int.TryParse(e.CommandArgument.ToString(), out logId))
                {
                    DeleteAuditLog(logId);

                    ShowMessage("success", "<i class='fa fa-check-circle me-2'></i>Audit log deleted successfully.");

                    LoadAuditLogs();
                }
            }
        }

        protected void btnDeleteSelected_Click(object sender, EventArgs e)
        {
            int deletedCount = 0;

            foreach (GridViewRow row in gvAuditLogs.Rows)
            {
                CheckBox chkSelect = row.FindControl("chkSelect") as CheckBox;

                if (chkSelect != null && chkSelect.Checked)
                {
                    int logId = Convert.ToInt32(gvAuditLogs.DataKeys[row.RowIndex].Value);
                    DeleteAuditLog(logId);
                    deletedCount++;
                }
            }

            if (deletedCount > 0)
            {
                ShowMessage("success", "<i class='fa fa-check-circle me-2'></i>" + deletedCount + " audit log(s) deleted successfully.");
            }
            else
            {
                ShowMessage("warning", "<i class='fa fa-exclamation-triangle me-2'></i>Please select at least one audit log to delete.");
            }

            LoadAuditLogs();
        }

        private void DeleteAuditLog(int logId)
        {
            using (SqlConnection con = new SqlConnection(ConnStr))
            {
                string query = "DELETE FROM AuditLogs WHERE LogId = @LogId";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@LogId", logId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void ShowMessage(string type, string message)
        {
            lblMessage.Visible = true;
            lblMessage.CssClass = "alert alert-" + type + " d-block mb-3";
            lblMessage.Text = message;
        }
    }
}
