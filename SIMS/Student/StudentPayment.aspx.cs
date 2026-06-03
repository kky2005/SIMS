using SIMS.BLL;
using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SIMS.Student
{
    public partial class Payments : System.Web.UI.Page
    {
        private StudentPaymentBLL paymentBLL = new StudentPaymentBLL();

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
                LoadPayments();
            }
        }

        private void LoadPayments()
        {
            int studentId = Convert.ToInt32(Session["StudentId"]);

            paymentBLL.EnsureStudentPayments(studentId);

            gvPayments.DataSource = paymentBLL.GetStudentPayments(studentId);
            gvPayments.DataBind();
        }

        protected void gvPayments_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "PayNow")
            {
                int paymentId = Convert.ToInt32(e.CommandArgument);
                int studentId = Convert.ToInt32(Session["StudentId"]);

                bool success = paymentBLL.PayStudentFee(paymentId, studentId);

                if (success)
                {
                    lblMessage.Text = "Payment successful.";
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                }
                else
                {
                    lblMessage.Text = "Payment could not be completed. It may already be paid.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                }

                LoadPayments();
            }
        }

        protected void gvPayments_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                string paymentStatus = DataBinder.Eval(e.Row.DataItem, "PaymentStatus").ToString();

                Button btnPayNow = (Button)e.Row.FindControl("btnPayNow");

                if (btnPayNow != null)
                {
                    if (paymentStatus == "Paid")
                    {
                        btnPayNow.Visible = false;
                    }
                    else
                    {
                        btnPayNow.Visible = true;
                    }
                }
            }
        }
    }
}