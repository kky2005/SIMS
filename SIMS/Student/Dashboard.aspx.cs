using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data.SqlClient;

namespace SIMS.Student
{
    public partial class Dashboard : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null ||
                Session["UserRole"] == null ||
                Session["UserRole"].ToString().ToLower() != "student")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                lblWelcome.Text = "Welcome, " + Session["FullName"].ToString();
                lblStudentNo.Text = "Student No: " + Session["StudentNo"].ToString();
                LoadCurrentSemester();
            }
        }
        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();

            Response.Redirect("../Login.aspx");
        }
        private void LoadCurrentSemester()
        {
            int studentId = Convert.ToInt32(Session["StudentId"]);

            string connStr = ConfigurationManager.ConnectionStrings["SIMS_DB"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
            SELECT CurrentSemester
            FROM Students
            WHERE StudentId = @StudentId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StudentId", studentId);

                conn.Open();
                object result = cmd.ExecuteScalar();

                if (result != null)
                {
                    lblCurrentSemester.Text = result.ToString();
                }
                else
                {
                    lblCurrentSemester.Text = "-";
                }
            }
        }
    }
}