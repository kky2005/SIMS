using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;

namespace SIMS
{
    public partial class AdmissionChecking : Page
    {
        private readonly string _connStr = ConfigurationManager.ConnectionStrings["SIMS_DB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) LoadStatus();
        }

        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadStatus();
        }

        private void LoadStatus()
        {
            // Authenticate by user session only
            int userId = AuthenticationHelper.GetCurrentUserId();
            if (userId == 0)
            {
                Response.Redirect("AdmissionLogin.aspx");
                return;
            }

            pnlStatus.Visible = true;

            try
            {
                using (var conn = new SqlConnection(_connStr))
                using (var cmd = new SqlCommand("SELECT TOP 1 AdmissionId, Status, RequestedAt, AdmittedAt, RejectedAt FROM dbo.Admissions WHERE UserId = @UserId ORDER BY RequestedAt DESC", conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    conn.Open();
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            int admissionId = rdr["AdmissionId"] != DBNull.Value ? Convert.ToInt32(rdr["AdmissionId"]) : 0;
                            lblAdmissionId.Text = admissionId > 0 ? admissionId.ToString() : "-";

                            var status = rdr["Status"]?.ToString() ?? "Pending";
                            lblStatus.Text = status;
                            SetStatusBadgeClass(status);

                            lblRequestedAt.Text = rdr["RequestedAt"] != DBNull.Value ? Convert.ToDateTime(rdr["RequestedAt"]).ToString("g") : "-";
                            lblAdmittedAt.Text = rdr["AdmittedAt"] != DBNull.Value ? Convert.ToDateTime(rdr["AdmittedAt"]).ToString("g") : "-";
                            lblRejectedAt.Text = rdr["RejectedAt"] != DBNull.Value ? Convert.ToDateTime(rdr["RejectedAt"]).ToString("g") : "-";
                        }
                        else
                        {
                            // No application found for this user - show message and let user choose to apply
                            pnlNoApplication.Visible = true;
                            pnlStatus.Visible = false;
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error";
                lblRequestedAt.Text = lblAdmittedAt.Text = lblRejectedAt.Text = ex.Message;
            }
        }

        private void SetStatusBadgeClass(string status)
        {
            switch ((status ?? string.Empty).ToLower())
            {
                case "admitted":
                    lblStatus.CssClass = "status-badge status-admitted";
                    break;
                case "rejected":
                    lblStatus.CssClass = "status-badge status-rejected";
                    break;
                default:
                    lblStatus.CssClass = "status-badge status-pending";
                    break;
            }
        }
        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("AdmissionDashboard.aspx");
        }

        protected void btnApplyNow_Click(object sender, EventArgs e)
        {
            Response.Redirect("AdmissionForm.aspx");
        }
    }
}
