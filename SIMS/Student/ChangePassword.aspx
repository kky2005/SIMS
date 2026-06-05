<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ChangePassword.aspx.cs" Inherits="SIMS.Student.ChangePassword" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>SIMS - Change Password</title>

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
            max-width: 700px;
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
        }

        .card-header-sims {
            padding: 24px 28px;
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
            padding: 28px;
        }

        .form-label {
            font-weight: bold;
            color: #1e293b;
            margin-bottom: 6px;
        }

        .form-control {
            border-radius: 8px;
            padding: 10px 12px;
        }

        .message {
            display: block;
            margin-bottom: 16px;
            font-weight: bold;
        }

        .btn-save {
            background: #0d6efd;
            color: white;
            border: none;
            padding: 9px 16px;
            border-radius: 7px;
            font-weight: bold;
        }

        .btn-save:hover {
            background: #0b5ed7;
        }

        .btn-cancel {
            background: #64748b;
            color: white;
            border: none;
            padding: 9px 16px;
            border-radius: 7px;
            font-weight: bold;
            margin-left: 8px;
        }

        .btn-cancel:hover {
            background: #475569;
        }

        .note-box {
            background: #f8fafc;
            border-left: 5px solid #0d6efd;
            padding: 14px 16px;
            margin-bottom: 20px;
            color: #334155;
        }
    </style>
</head>

<body>
    <form id="form1" runat="server">
        <div class="container-sims">

            <a href="Profile.aspx" class="back-link">
                <i class="fa fa-arrow-left"></i> Back to Profile
            </a>

            <div class="card-sims">
                <div class="card-header-sims">
                    <h2>Change Password</h2>
                    <p>Update your account password securely.</p>
                </div>

                <div class="card-body-sims">
                    <asp:Label ID="lblMessage" runat="server" CssClass="message"></asp:Label>

                    <div class="note-box">
                        Enter your current password first. The system will verify it before saving your new password.
                    </div>

                    <div class="mb-3">
                        <label class="form-label">Current Password</label>
                        <asp:TextBox ID="txtCurrentPassword" runat="server"
                            TextMode="Password"
                            CssClass="form-control"
                            placeholder="Enter current password">
                        </asp:TextBox>
                    </div>

                    <div class="mb-3">
                        <label class="form-label">New Password</label>
                        <asp:TextBox ID="txtNewPassword" runat="server"
                            TextMode="Password"
                            CssClass="form-control"
                            placeholder="Enter new password">
                        </asp:TextBox>
                    </div>

                    <div class="mb-4">
                        <label class="form-label">Confirm New Password</label>
                        <asp:TextBox ID="txtConfirmPassword" runat="server"
                            TextMode="Password"
                            CssClass="form-control"
                            placeholder="Confirm new password">
                        </asp:TextBox>
                    </div>

                    <asp:Button ID="btnChangePassword" runat="server"
                        Text="Change Password"
                        CssClass="btn-save"
                        OnClick="btnChangePassword_Click" />

                    <asp:Button ID="btnCancel" runat="server"
                        Text="Cancel"
                        CssClass="btn-cancel"
                        CausesValidation="false"
                        OnClick="btnCancel_Click" />
                </div>
            </div>

        </div>
    </form>
</body>
</html>