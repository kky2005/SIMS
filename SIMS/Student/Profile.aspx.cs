using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SIMS.Student
{
    public partial class Profile : System.Web.UI.Page
    {
        string cs = ConfigurationManager.ConnectionStrings["SIMS_DB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null)
            {
                Response.Redirect("../Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadStudentProfile();
            }
        }

        private void LoadStudentProfile()
        {
            int userId = Convert.ToInt32(Session["UserId"]);

            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = @"
                    SELECT 
                        u.FullName,
                        u.Email,
                        u.Phone,
                        s.StudentNo,
                        s.IntakeYear,
                        s.IntakeSemester,
                        s.AdmissionDate,
                        s.CurrentSemester,
                        s.Status,
                        p.ProgrammeCode,
                        p.ProgrammeName
                    FROM Users u
                    INNER JOIN Students s ON u.UserId = s.UserId
                    INNER JOIN Programmes p ON s.ProgrammeId = p.ProgrammeId
                    WHERE u.UserId = @UserId";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    con.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            string fullName = dr["FullName"].ToString();
                            string studentNo = dr["StudentNo"].ToString();

                            lblSideName.Text = fullName;
                            lblSideStudentNo.Text = studentNo;

                            lblFullName.Text = fullName;
                            lblStudentNo.Text = studentNo;
                            lblProgrammeCode.Text = dr["ProgrammeCode"].ToString();
                            lblStatus.Text = dr["Status"].ToString();

                            lblPersonalName.Text = fullName;
                            lblEmail.Text = dr["Email"].ToString();
                            txtPhone.Text = dr["Phone"] == DBNull.Value ? "" : dr["Phone"].ToString();
                            lblPersonalStudentNo.Text = studentNo;

                            lblProgrammeName.Text = dr["ProgrammeName"].ToString();
                            lblIntakeYear.Text = dr["IntakeYear"] == DBNull.Value ? "-" : dr["IntakeYear"].ToString();
                            lblIntakeSemester.Text = dr["IntakeSemester"] == DBNull.Value ? "-" : dr["IntakeSemester"].ToString();
                            lblCurrentSemester.Text = dr["CurrentSemester"] == DBNull.Value ? "-" : dr["CurrentSemester"].ToString();

                            if (dr["AdmissionDate"] == DBNull.Value)
                            {
                                lblAdmissionDate.Text = "-";
                            }
                            else
                            {
                                DateTime admissionDate = Convert.ToDateTime(dr["AdmissionDate"]);
                                lblAdmissionDate.Text = admissionDate.ToString("dd MMMM yyyy");
                            }
                        }
                    }
                }
            }
        }
        protected void btnUpdatePhone_Click(object sender, EventArgs e)
        {
            if (Session["UserId"] == null)
            {
                Response.Redirect("../Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserId"]);
            string newPhone = txtPhone.Text.Trim();

            if (newPhone.Length > 20)
            {
                lblPhoneMessage.Text = "Phone number cannot be more than 20 characters.";
                lblPhoneMessage.CssClass = "d-block mt-2 text-danger";
                return;
            }

            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = @"
            UPDATE Users
            SET Phone = @Phone
            WHERE UserId = @UserId";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Phone", string.IsNullOrWhiteSpace(newPhone) ? (object)DBNull.Value : newPhone);
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            lblPhoneMessage.Text = "Phone number updated successfully.";
            lblPhoneMessage.CssClass = "d-block mt-2 text-success";

            LoadStudentProfile();
        }
        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();

            Response.Redirect("../Login.aspx");
        }
    }
}