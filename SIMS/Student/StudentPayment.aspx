<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Payments.aspx.cs" Inherits="SIMS.Student.Payments" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>SIMS - Payments</title>

    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />

    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" rel="stylesheet" />

    <style>
        body {
            background: #f1f5f9;
            font-family: Arial, sans-serif;
            margin: 0;
            padding: 30px;
        }

        .container-sims {
            max-width: 1100px;
            margin: auto;
        }

        .back-link {
            display: inline-block;
            margin-bottom: 16px;
            text-decoration: none;
            color: #1e293b;
            font-weight: bold;
        }

        .card-sims {
            background: #fff;
            border-radius: 12px;
            border: 1px solid #e2e8f0;
            box-shadow: 0 1px 4px rgba(0,0,0,0.06);
            margin-bottom: 24px;
        }

        .card-header-sims {
            padding: 22px 26px;
            border-bottom: 1px solid #e2e8f0;
        }

        .card-header-sims h2 {
            margin: 0;
            color: #1e293b;
            font-weight: bold;
        }

        .card-header-sims p {
            margin: 6px 0 0;
            color: #64748b;
        }

        .card-body-sims {
            padding: 24px;
        }

        .grid {
            width: 100%;
            border-collapse: collapse;
        }

        .grid th {
            background: #1e3a5f;
            color: white;
            padding: 10px;
            font-size: 13px;
            text-align: left;
        }

        .grid td {
            padding: 10px;
            border-bottom: 1px solid #e2e8f0;
            font-size: 13px;
        }

        .btn-pay {
            background: #16a34a;
            color: white;
            border: none;
            padding: 7px 12px;
            border-radius: 6px;
            font-size: 13px;
            cursor: pointer;
        }

        .btn-pay:hover {
            background: #15803d;
        }

        .message {
            display: block;
            margin-top: 12px;
            font-weight: bold;
        }

        .note-box {
            background: #f8fafc;
            border-left: 5px solid #0d6efd;
            padding: 14px 16px;
            margin-bottom: 18px;
            color: #334155;
        }
    </style>
</head>

<body>
    <form id="form1" runat="server">
        <div class="container-sims">

            <a href="Dashboard.aspx" class="back-link">
                <i class="fa fa-arrow-left"></i> Back to Dashboard
            </a>

            <div class="card-sims">
                <div class="card-header-sims">
                    <h2>Payments</h2>
                    <p>View your semester payment records and complete pending payments.</p>
                    <asp:Label ID="lblMessage" runat="server" CssClass="message"></asp:Label>
                </div>

                <div class="card-body-sims">
                    <div class="note-box">
                        This is a dummy payment page for system testing. Clicking Pay Now will mark the payment as successful.
                    </div>

                    <asp:GridView ID="gvPayments" runat="server"
                        AutoGenerateColumns="False"
                        CssClass="grid"
                        EmptyDataText="No payment records found."
                        OnRowCommand="gvPayments_RowCommand"
                        OnRowDataBound="gvPayments_RowDataBound">

                        <Columns>
                            <asp:BoundField DataField="AcademicYear" HeaderText="Academic Year" />
                            <asp:BoundField DataField="Semester" HeaderText="Semester" />
                            <asp:BoundField DataField="TotalCreditHours" HeaderText="Total Credit Hours" />
                            <asp:BoundField DataField="FeePerCreditHour" HeaderText="Fee / Credit Hour" DataFormatString="RM {0:N2}" />
                            <asp:BoundField DataField="Amount" HeaderText="Amount" DataFormatString="RM {0:N2}" />
                            <asp:BoundField DataField="PaymentStatus" HeaderText="Status" />
                            <asp:BoundField DataField="CreatedAt" HeaderText="Created At" DataFormatString="{0:dd MMM yyyy hh:mm tt}" />
                            <asp:BoundField DataField="PaidAt" HeaderText="Paid At" DataFormatString="{0:dd MMM yyyy hh:mm tt}" />

                            <asp:TemplateField HeaderText="Action">
                                <ItemTemplate>
                                    <asp:Button ID="btnPayNow" runat="server"
                                        Text="Pay Now"
                                        CssClass="btn-pay"
                                        CommandName="PayNow"
                                        CommandArgument='<%# Eval("PaymentId") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>

        </div>
    </form>
</body>
</html>