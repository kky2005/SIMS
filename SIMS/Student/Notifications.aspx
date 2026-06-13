<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Notifications.aspx.cs" Inherits="SIMS.Student.Notifications" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>SIMS - Notifications</title>

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
            display: flex;
            justify-content: space-between;
            align-items: center;
            gap: 16px;
        }

        .header-text h2 {
            margin: 0;
            color: #1e293b;
            font-weight: bold;
        }

        .header-text p {
            margin: 6px 0 0;
            color: #64748b;
        }

        .card-body-sims {
            padding: 24px;
        }

        .message {
            display: block;
            margin-top: 10px;
            font-weight: bold;
        }

        .note-box {
            background: #f8fafc;
            border-left: 5px solid #0d6efd;
            padding: 14px 16px;
            margin-bottom: 18px;
            color: #334155;
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
            vertical-align: middle;
        }

        .btn-open {
            background: #0d6efd;
            color: white;
            border: none;
            padding: 7px 12px;
            border-radius: 6px;
            font-size: 13px;
            cursor: pointer;
            margin-right: 6px;
        }

        .btn-open:hover {
            background: #0b5ed7;
        }

        .btn-read {
            background: #16a34a;
            color: white;
            border: none;
            padding: 7px 12px;
            border-radius: 6px;
            font-size: 13px;
            cursor: pointer;
            margin-right: 6px;
        }

        .btn-read:hover {
            background: #15803d;
        }

        .btn-delete {
            background: #dc2626;
            color: white;
            border: none;
            padding: 7px 12px;
            border-radius: 6px;
            font-size: 13px;
            cursor: pointer;
        }

        .btn-delete:hover {
            background: #b91c1c;
        }

        .btn-mark-all {
            background: #6f42c1;
            color: white;
            border: none;
            padding: 9px 14px;
            border-radius: 7px;
            font-size: 13px;
            cursor: pointer;
            white-space: nowrap;
        }

        .btn-mark-all:hover {
            background: #5a32a3;
        }

        .type-badge {
            display: inline-block;
            background: #eef2ff;
            color: #3730a3;
            padding: 4px 8px;
            border-radius: 999px;
            font-size: 12px;
            font-weight: bold;
        }

        .notification-title {
            font-weight: bold;
            color: #1e293b;
        }

        .notification-message {
            color: #475569;
        }

        @media (max-width: 768px) {
            body {
                padding: 18px;
            }

            .card-header-sims {
                flex-direction: column;
                align-items: flex-start;
            }

            .grid {
                font-size: 12px;
            }

            .btn-open,
            .btn-read,
            .btn-delete {
                margin-bottom: 6px;
            }
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
                    <div class="header-text">
                        <h2>Notifications</h2>
                        <p>View your academic updates, announcements, grades, and system alerts.</p>
                        <asp:Label ID="lblMessage" runat="server" CssClass="message"></asp:Label>
                    </div>

                    <asp:Button ID="btnMarkAllRead" runat="server"
                        Text="Mark All as Read"
                        CssClass="btn-mark-all"
                        OnClick="btnMarkAllRead_Click" />
                </div>

                <div class="card-body-sims">
                    <div class="note-box">
                        Unread notifications are shown in bold. Opening a notification will mark it as read and take you to the related page.
                    </div>

                    <asp:GridView ID="gvNotifications" runat="server"
                        AutoGenerateColumns="False"
                        CssClass="grid"
                        EmptyDataText="No notifications found."
                        OnRowCommand="gvNotifications_RowCommand"
                        OnRowDataBound="gvNotifications_RowDataBound">

                        <Columns>
                            <asp:TemplateField HeaderText="Notification">
                                <ItemTemplate>
                                    <div class="notification-title">
                                        <%# Eval("Title") %>
                                    </div>
                                    <div class="notification-message">
                                        <%# Eval("Message") %>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Type">
                                <ItemTemplate>
                                    <span class="type-badge">
                                        <%# Eval("NotificationType") %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:BoundField DataField="CreatedAt"
                                HeaderText="Date"
                                DataFormatString="{0:dd MMM yyyy hh:mm tt}" />

                            <asp:TemplateField HeaderText="Status">
                                <ItemTemplate>
                                    <asp:Label ID="lblReadStatus" runat="server"
                                        Text='<%# Eval("ReadStatus") %>'>
                                    </asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Action">
                                <ItemTemplate>
                                    <asp:Button ID="btnOpen" runat="server"
                                        Text="Open"
                                        CssClass="btn-open"
                                        CommandName="OpenNotification"
                                        CommandArgument='<%# Eval("NotificationId") %>' />

                                    <asp:Button ID="btnMarkRead" runat="server"
                                        Text="Mark Read"
                                        CssClass="btn-read"
                                        CommandName="MarkRead"
                                        CommandArgument='<%# Eval("NotificationId") %>' />

                                    <asp:Button ID="btnDelete" runat="server"
                                        Text="Delete"
                                        CssClass="btn-delete"
                                        CommandName="DeleteNotification"
                                        CommandArgument='<%# Eval("NotificationId") %>'
                                        OnClientClick="return confirm('Are you sure you want to delete this notification?');" />
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