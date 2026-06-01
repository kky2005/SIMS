<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="LecturerAnnouncements.aspx.cs"
    Inherits="SIMS.Lecturer.LecturerAnnouncements"
    MasterPageFile="~/Lecturer/LecturerMaster.master" %>

<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server">
    <style>

    .announcements-header {
        background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
        color: white;
        padding: 20px;
        border-radius: 8px;
        margin-bottom: 30px;
    }

    .announcements-header h3 {
        margin: 0 0 5px 0;
    }

    .create-announcement-form {
        background: white;
        border: 1px solid #e2e8f0;
        border-radius: 8px;
        padding: 20px;
        margin-bottom: 30px;
    }

    .form-row {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 20px;
        margin-bottom: 20px;
    }

    .form-group {
        display: flex;
        flex-direction: column;
    }

    .form-group label {
        font-weight: bold;
        color: #1e293b;
        margin-bottom: 8px;
        font-size: 14px;
    }

    .form-group input,
    .form-group select,
    .form-group textarea {
        padding: 10px 12px;
        border: 1px solid #cbd5e1;
        border-radius: 6px;
        font-size: 14px;
        font-family: inherit;
    }

    .form-group textarea {
        resize: vertical;
        min-height: 120px;
    }

    .form-group input:focus,
    .form-group select:focus,
    .form-group textarea:focus {
        outline: none;
        border-color: #f59e0b;
        box-shadow: 0 0 0 3px rgba(245, 158, 11, 0.1);
    }

    .form-full {
        grid-column: 1 / -1;
    }

    .form-actions {
        display: flex;
        gap: 10px;
        justify-content: flex-end;
        padding-top: 15px;
        border-top: 1px solid #e2e8f0;
    }

    .form-actions .btn {
        padding: 10px 20px;
        font-size: 14px;
    }

    .announcements-list {
        margin-bottom: 30px;
    }

    .announcement-card {
        background: white;
        border: 1px solid #e2e8f0;
        border-radius: 8px;
        overflow: hidden;
        margin-bottom: 15px;
        transition: all 0.3s ease;
    }

    .announcement-card:hover {
        box-shadow: 0 4px 12px rgba(0,0,0,0.1);
    }

    .announcement-header {
        background: #fef3c7;
        padding: 15px 20px;
        border-bottom: 1px solid #fde68a;
        display: flex;
        justify-content: space-between;
        align-items: center;
    }

    .announcement-title {
        font-weight: bold;
        color: #1e293b;
        font-size: 16px;
    }

    .announcement-audience {
        display: inline-block;
        background: #f59e0b;
        color: white;
        padding: 4px 10px;
        border-radius: 12px;
        font-size: 12px;
        font-weight: bold;
    }

    .announcement-body {
        padding: 20px;
        color: #475569;
        line-height: 1.6;
    }

    .announcement-footer {
        padding: 12px 20px;
        background: #f8fafc;
        border-top: 1px solid #e2e8f0;
        display: flex;
        justify-content: space-between;
        align-items: center;
        font-size: 13px;
        color: #64748b;
    }

    .announcement-meta {
        display: flex;
        gap: 15px;
    }

    .announcement-meta-item {
        display: flex;
        align-items: center;
        gap: 5px;
    }

    .announcement-actions {
        display: flex;
        gap: 8px;
    }

    .announcement-actions .btn {
        padding: 6px 12px;
        font-size: 12px;
    }

    .no-announcements {
        text-align: center;
        padding: 40px 20px;
        background: white;
        border-radius: 8px;
        border: 1px solid #e2e8f0;
        color: #64748b;
    }

    .no-announcements i {
        font-size: 48px;
        color: #cbd5e1;
        display: block;
        margin-bottom: 15px;
    }

    .success-message {
        background: #dcfce7;
        border: 1px solid #86efac;
        color: #166534;
        padding: 12px 15px;
        border-radius: 6px;
        margin-bottom: 20px;
        display: flex;
        align-items: center;
        gap: 10px;
    }

    .error-message {
        background: #fee2e2;
        border: 1px solid #fca5a5;
        color: #991b1b;
        padding: 12px 15px;
        border-radius: 6px;
        margin-bottom: 20px;
        display: flex;
        align-items: center;
        gap: 10px;
    }

    .char-count {
        font-size: 12px;
        color: #64748b;
        margin-top: 5px;
        text-align: right;
    }

    @media (max-width: 768px) {
        .form-row {
            grid-template-columns: 1fr;
        }

        .announcement-header {
            flex-direction: column;
            align-items: flex-start;
            gap: 10px;
        }

        .announcement-footer {
            flex-direction: column;
            align-items: flex-start;
            gap: 10px;
        }

        .form-actions {
            flex-direction: column;
        }
    }

        .announcements-header {
            background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
            color: white;
            padding: 20px;
            border-radius: 8px;
            margin-bottom: 30px;
        }
        .create-announcement-form {
            background: white;
            border: 1px solid #e2e8f0;
            border-radius: 8px;
            padding: 20px;
            margin-bottom: 30px;
        }
        .form-row { display: grid; grid-template-columns: 1fr; gap: 12px; margin-bottom: 12px; }
        .form-group label { font-weight: bold; color: #1e293b; margin-bottom: 6px; display: block; }
        .form-group input, .form-group textarea { width:100%; padding:10px; border:1px solid #cbd5e1; border-radius:6px; }
        .form-actions { display:flex; gap:10px; justify-content:flex-end; margin-top:12px; }
        .announcement-list { margin-top: 20px; }
        .announcement-card { background:white; border:1px solid #e2e8f0; border-radius:8px; margin-bottom:12px; overflow:hidden; }
        .announcement-header { padding:12px 16px; border-bottom:1px solid #e2e8f0; display:flex; justify-content:space-between; align-items:center; }
        .announcement-body { padding:16px; color:#475569; }
        .announcement-footer { padding:10px 16px; background:#f8fafc; border-top:1px solid #e2e8f0; display:flex; justify-content:space-between; align-items:center; }
        .no-announcements { text-align:center; padding:24px; color:#64748b; background:white; border:1px solid #e2e8f0; border-radius:8px; }
    </style>
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">

    <div class="announcements-header">
        <h3><i class="fa fa-bullhorn" style="margin-right:10px;"></i>Announcements</h3>
        <p style="margin:0;"><asp:Literal ID="litCourseHeader" runat="server" Text="Loading..." /></p>
    </div>

    <asp:Panel ID="pnlSuccess" runat="server" Visible="false" CssClass="success-message" style="margin-bottom:12px;">
        <i class="fa fa-check-circle"></i>
        <span><asp:Literal ID="litSuccessMsg" runat="server" /></span>
    </asp:Panel>

    <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="error-message" style="margin-bottom:12px;">
        <i class="fa fa-exclamation-circle"></i>
        <span><asp:Literal ID="litErrorMsg" runat="server" /></span>
    </asp:Panel>

    <div class="create-announcement-form">
        <h4 style="margin:0 0 12px 0;">Create Announcement for this Course</h4>

        <div class="form-row">
            <div class="form-group">
                <label>Course</label>
                <asp:Literal ID="litCourseName" runat="server" />
            </div>

            <div class="form-group">
                <label for="txtTitle">Title</label>
                <asp:TextBox ID="txtTitle" runat="server" Placeholder="Enter announcement title" />
            </div>

            <div class="form-group">
                <label for="txtBody">Content</label>
                <asp:TextBox ID="txtBody" runat="server" TextMode="MultiLine" Placeholder="Enter announcement message" />
            </div>

            <div class="form-group">
                <label for="txtExpiresAt">Expiration Date (optional)</label>
                <asp:TextBox ID="txtExpiresAt" runat="server" TextMode="Date" />
                <small style="color:#64748b;">Leave empty for no expiration</small>
            </div>
        </div>

        <div class="form-actions">
            <asp:Button ID="btnPublish" runat="server" Text="Publish Announcement" CssClass="btn btn-primary" OnClick="btnPublish_Click" />
            <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-outline-secondary" OnClick="btnClear_Click" />
        </div>

        <asp:HiddenField ID="hidCourseId" runat="server" />
    </div>

    <h4 style="margin-top:16px;">Your Announcements for this Course</h4>

    <div class="announcement-list">
        <asp:Repeater ID="rptAnnouncements" runat="server">
            <ItemTemplate>
                <div class="announcement-card">
                    <div class="announcement-header">
                        <div>
                            <div style="font-weight:bold;"><%# Eval("Title") %></div>
                            <div style="font-size:12px;color:#64748b;"><%# Eval("Audience") %> • Posted: <%# Eval("PublishedAt", "{0:dd MMM yyyy HH:mm}") %></div>
                        </div>
                        <div>
                            <asp:Button ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-sm btn-outline-danger" CommandArgument='<%# Eval("AnnouncementId") %>' OnClick="btnDelete_Click" OnClientClick="return confirm('Delete this announcement?');" />
                        </div>
                    </div>
                    <div class="announcement-body">
                        <%# Eval("Body") %>
                    </div>
                    <div class="announcement-footer">
                        <div style="font-size:12px;color:#64748b;">
                            <%# Eval("ExpiresAt") != DBNull.Value ? ("Expires: " + Eval("ExpiresAt", "{0:dd MMM yyyy}")) : "No expiry" %>
                        </div>
                        <div style="font-size:12px;color:#64748b;">
                            <!-- empty right side for future actions -->
                        </div>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>

        <asp:Panel ID="pnlNoAnnouncements" runat="server" Visible="false" CssClass="no-announcements">
            <i class="fa fa-inbox" style="font-size:28px;color:#cbd5e1;display:block;margin-bottom:8px;"></i>
            <strong>No announcements yet for this course.</strong>
        </asp:Panel>
    </div>

    <script type="text/javascript">
        document.addEventListener('DOMContentLoaded', function () {
            var successEl = document.getElementById('<%= pnlSuccess.ClientID %>');
            var errorEl = document.getElementById('<%= pnlError.ClientID %>');

            [successEl, errorEl].forEach(function (el) {
                if (!el) return;
                if (el.offsetParent !== null) {
                    setTimeout(function () { el.style.display = 'none'; }, 5000);
                }
            });
        });
    </script>

</asp:Content>