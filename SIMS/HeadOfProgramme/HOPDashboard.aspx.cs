using System;
using System.Configuration;
using System.Data.SqlClient;

namespace SIMS.HeadOfProgramme
{
    public partial class Dashboard : HOPBase
    {
        private readonly string connStr = ConfigurationManager
            .ConnectionStrings["SIMS_DB"]
            .ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            EnsureAuthenticated();

            if (!IsPostBack)
            {
                LoadDashboardCounts();
            }
        }

        private void LoadDashboardCounts()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    litProgrammesCount.Text = GetCount(conn, "SELECT COUNT(*) FROM Programmes").ToString();
                    litCoursesCount.Text = GetCount(conn, "SELECT COUNT(*) FROM Courses").ToString();
                    litStudentsCount.Text = GetCount(conn, "SELECT COUNT(*) FROM Students").ToString();
                    litLecturersCount.Text = GetCount(conn, "SELECT COUNT(*) FROM Lecturers").ToString();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading dashboard counts: " + ex.Message);

                // Keep dashboard visible even if database count loading fails.
                litProgrammesCount.Text = "0";
                litCoursesCount.Text = "0";
                litStudentsCount.Text = "0";
                litLecturersCount.Text = "0";
            }
        }

        private int GetCount(SqlConnection conn, string sql)
        {
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                object result = cmd.ExecuteScalar();
                return result == DBNull.Value || result == null ? 0 : Convert.ToInt32(result);
            }
        }
    }
}
