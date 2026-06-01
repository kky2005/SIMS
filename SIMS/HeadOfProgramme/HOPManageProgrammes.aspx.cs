using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace SIMS.HeadOfProgramme
{
    public partial class HOPManageProgrammes : HOPCrudBase
    {
        protected void Page_Load(object sender, EventArgs e) { EnsureAuthenticated(); if (!IsPostBack) BindGrid(); }

        private void BindGrid()
        {
            gvProgrammes.DataSource = GetData(@"SELECT ProgrammeId, ProgrammeCode, ProgrammeName, DurationYears, Description, IsActive,
                CASE WHEN IsActive=1 THEN 'Yes' ELSE 'No' END AS IsActiveText FROM Programmes ORDER BY ProgrammeId DESC");
            gvProgrammes.DataBind();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try {
                if (string.IsNullOrWhiteSpace(txtProgrammeCode.Text) || string.IsNullOrWhiteSpace(txtProgrammeName.Text)) { ShowMessage(lblMessage,"Code and name required.",false); return; }
                if (string.IsNullOrEmpty(hfProgrammeId.Value))
                    Execute(@"INSERT INTO Programmes(ProgrammeCode,ProgrammeName,DurationYears,Description,IsActive) VALUES(@Code,@Name,@Years,@Desc,@Active)",
                        new SqlParameter("@Code", txtProgrammeCode.Text.Trim()), new SqlParameter("@Name", txtProgrammeName.Text.Trim()),
                        new SqlParameter("@Years", txtDurationYears.Text), new SqlParameter("@Desc", txtDescription.Text.Trim()),
                        new SqlParameter("@Active", ddlIsActive.SelectedValue));
                else
                    Execute(@"UPDATE Programmes SET ProgrammeCode=@Code,ProgrammeName=@Name,DurationYears=@Years,Description=@Desc,IsActive=@Active WHERE ProgrammeId=@Id",
                        new SqlParameter("@Code", txtProgrammeCode.Text.Trim()), new SqlParameter("@Name", txtProgrammeName.Text.Trim()),
                        new SqlParameter("@Years", txtDurationYears.Text), new SqlParameter("@Desc", txtDescription.Text.Trim()),
                        new SqlParameter("@Active", ddlIsActive.SelectedValue), new SqlParameter("@Id", hfProgrammeId.Value));
                ClearForm(); BindGrid(); ShowMessage(lblMessage,"Programme saved successfully.",true);
            } catch (Exception ex) { ShowMessage(lblMessage, ex.Message, false); }
        }

        protected void gvProgrammes_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try {
                int id = Convert.ToInt32(e.CommandArgument);
                if (e.CommandName == "EditProgramme") {
                    DataTable dt = GetData("SELECT * FROM Programmes WHERE ProgrammeId=@Id", new SqlParameter("@Id", id));
                    if (dt.Rows.Count == 0) return; DataRow r = dt.Rows[0];
                    hfProgrammeId.Value = id.ToString(); txtProgrammeCode.Text = r["ProgrammeCode"].ToString(); txtProgrammeName.Text = r["ProgrammeName"].ToString();
                    txtDurationYears.Text = r["DurationYears"].ToString(); txtDescription.Text = r["Description"].ToString(); ddlIsActive.SelectedValue = Convert.ToBoolean(r["IsActive"]) ? "1" : "0";
                } else if (e.CommandName == "DeleteProgramme") {
                    Execute("DELETE FROM Programmes WHERE ProgrammeId=@Id", new SqlParameter("@Id", id)); BindGrid(); ShowMessage(lblMessage,"Programme deleted.",true);
                }
            } catch (Exception ex) { ShowMessage(lblMessage, "Delete failed. This programme may be used by courses/students. " + ex.Message, false); }
        }

        protected void btnClear_Click(object sender, EventArgs e) { ClearForm(); }
        private void ClearForm() { hfProgrammeId.Value=""; txtProgrammeCode.Text=""; txtProgrammeName.Text=""; txtDurationYears.Text="3"; txtDescription.Text=""; ddlIsActive.SelectedValue="1"; }
    }
}
