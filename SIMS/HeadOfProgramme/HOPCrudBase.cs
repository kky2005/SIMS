using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI.WebControls;

namespace SIMS.HeadOfProgramme
{
    public class HOPCrudBase : HOPBase
    {
        protected string ConnStr
        {
            get
            {
                var cs = ConfigurationManager.ConnectionStrings["SIMS_DB"];
                if (cs == null) cs = ConfigurationManager.ConnectionStrings["SIMSConnectionString"];
                if (cs == null) cs = ConfigurationManager.ConnectionStrings["DefaultConnection"];
                if (cs == null) throw new Exception("Missing connection string. Add name='SIMS_DB' in Web.config.");
                return cs.ConnectionString;
            }
        }

        protected DataTable GetData(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection con = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        protected int Execute(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection con = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                con.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        protected object Scalar(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection con = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                con.Open();
                return cmd.ExecuteScalar();
            }
        }

        protected string HashPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes) sb.Append(b.ToString("X2"));
                return sb.ToString();
            }
        }

        protected int GetRoleId(string roleName)
        {
            object result = Scalar("SELECT RoleId FROM Roles WHERE RoleName=@RoleName", new SqlParameter("@RoleName", roleName));
            if (result == null || result == DBNull.Value) throw new Exception("Role not found: " + roleName);
            return Convert.ToInt32(result);
        }

        protected void BindDropDown(DropDownList ddl, string sql, string textField, string valueField)
        {
            ddl.DataSource = GetData(sql);
            ddl.DataTextField = textField;
            ddl.DataValueField = valueField;
            ddl.DataBind();
        }

        protected void ShowMessage(Label label, string message, bool success)
        {
            label.Text = message;
            label.CssClass = success ? "alert alert-success d-block" : "alert alert-danger d-block";
        }
    }
}
