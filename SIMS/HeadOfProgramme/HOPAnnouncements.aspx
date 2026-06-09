<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="HOPAnnouncements.aspx.cs"
    Inherits="SIMS.HeadOfProgramme.HOPAnnouncements"
    MasterPageFile="~/HeadOfProgramme/HOPMaster.master" %>

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
            white-space: nowrap;
        }

        .announcement-body {
            padding: 20px;
            color: #475569;
            line-height: 1.6;
            white-space: pre-wrap;
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
            flex-wrap: wrap;
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


        .filter-panel {
            background: white;
            border: 1px solid #e2e8f0;
            border-radius: 8px;
            padding: 20px;
            margin-bottom: 25px;
        }

        .filter-panel h4 {
            color: #1e293b;
            margin-top: 0;
            margin-bottom: 20px;
            font-weight: bold;
        }

        .filter-actions {
            display: flex;
            gap: 10px;
            justify-content: flex-end;
            padding-top: 5px;
        }

        @media (max-width: 768px) {
            .form-row {
                grid-template-columns: 1fr;
            }

            .announcement-header,
            .announcement-footer {
                flex-direction: column;
                align-items: flex-start;
                gap: 10px;
            }

            .form-actions {
                flex-direction: column;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">

    <div class="announcements-header">
        <h3>
            <i class="fa fa-bullhorn" style="margin-right: 10px;"></i>HOP Announcements Management
        </h3>
        <p>Create announcements and remove old announcements</p>
    </div>

    <asp:Panel ID="pnlSuccess" runat="server" Visible="false" CssClass="success-message">
        <i class="fa fa-check-circle"></i>
        <span><asp:Literal ID="litSuccessMsg" runat="server" /></span>
    </asp:Panel>

    <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="error-message">
        <i class="fa fa-exclamation-circle"></i>
        <span><asp:Literal ID="litErrorMsg" runat="server" /></span>
    </asp:Panel>

    <div class="create-announcement-form">
        <h4 style="color: #1e293b; margin-top: 0; margin-bottom: 20px; font-weight: bold;">
            <i class="fa fa-plus-circle" style="color: #f59e0b; margin-right: 8px;"></i>Create New Announcement
        </h4>

        <div class="form-row">
            <div class="form-group">
                <label>Course (Optional):</label>
                <asp:DropDownList ID="ddlCourse" runat="server">
                    <asp:ListItem Text="-- General Announcement --" Value="" />
                </asp:DropDownList>
            </div>
            <div class="form-group">
                <label>Audience:</label>
                <asp:DropDownList ID="ddlAudience" runat="server">
                    <asp:ListItem Text="All Students" Value="AllStudents" />
                    <asp:ListItem Text="Course Students" Value="CourseStudents" />
                    <asp:ListItem Text="All Lecturers" Value="AllLecturers" />
                    <asp:ListItem Text="Everyone" Value="Everyone" />
                </asp:DropDownList>
            </div>
        </div>

        <div class="form-group form-full">
            <label for="txtTitle">Announcement Title:</label>
            <asp:TextBox ID="txtTitle" runat="server" placeholder="Enter announcement title" />
        </div>

        <div class="form-group form-full">
            <label for="txtBody">Announcement Content:</label>
            <asp:TextBox ID="txtBody" runat="server" TextMode="MultiLine" placeholder="Enter announcement message" />
            <div class="char-count">
                <span id="charCount">0</span> / 5000 characters
            </div>
        </div>

        <div class="form-group form-full">
            <label for="txtExpiryDays">Expire After (Days) Optional:</label>
            <asp:TextBox ID="txtExpiryDays" runat="server" TextMode="Number" placeholder="Enter number of days, e.g. 7" />
            <small style="color: #64748b; display: block; margin-top: 5px;">Enter how many days before this announcement expires. Example: 7 means it will expire after 7 days. Leave empty for no expiration.</small>
        </div>

        <div class="form-actions">
            <asp:Button ID="btnPublish" runat="server" Text="Publish Announcement"
                CssClass="btn btn-primary" OnClick="btnPublish_Click" />
            <asp:Button ID="btnClear" runat="server" Text="Clear"
                CssClass="btn btn-outline-secondary" OnClick="btnClear_Click" />
        </div>
    </div>

    <div class="filter-panel">
        <h4>
            <i class="fa fa-filter" style="color: #f59e0b; margin-right: 8px;"></i>Filter Announcements
        </h4>

        <div class="form-row">
            <div class="form-group">
                <label for="txtFilterTitle">Title:</label>
                <asp:TextBox ID="txtFilterTitle" runat="server" placeholder="Search by announcement title" />
            </div>
            <div class="form-group">
                <label for="ddlFilterAuthor">Posted By:</label>
                <asp:DropDownList ID="ddlFilterAuthor" runat="server">
                    <asp:ListItem Text="-- All Authors --" Value="" />
                </asp:DropDownList>
            </div>
        </div>

        <div class="form-row">
            <div class="form-group">
                <label for="ddlFilterCourse">Course:</label>
                <asp:DropDownList ID="ddlFilterCourse" runat="server">
                    <asp:ListItem Text="-- All Courses --" Value="" />
                    <asp:ListItem Text="General Announcements" Value="GENERAL" />
                </asp:DropDownList>
            </div>
            <div class="form-group">
                <label for="txtFilterPostedDate">Posted Date:</label>
                <asp:TextBox ID="txtFilterPostedDate" runat="server" TextMode="Date" />
            </div>
        </div>

        <div class="form-row">
            <div class="form-group">
                <label for="txtFilterExpiryDate">Expiry Date:</label>
                <asp:TextBox ID="txtFilterExpiryDate" runat="server" TextMode="Date" />
            </div>
            <div class="form-group">
                <label>&nbsp;</label>
                <div class="filter-actions">
                    <asp:Button ID="btnApplyFilter" runat="server" Text="Apply Filter"
                        CssClass="btn btn-primary" OnClick="btnApplyFilter_Click" />
                    <asp:Button ID="btnClearFilter" runat="server" Text="Clear Filter"
                        CssClass="btn btn-outline-secondary" OnClick="btnClearFilter_Click" />
                </div>
            </div>
        </div>
    </div>

    <h4 style="color: #1e293b; margin-bottom: 20px; font-weight: bold;">
        <i class="fa fa-list" style="color: #f59e0b; margin-right: 8px;"></i>All Announcements
    </h4>

    <div class="announcements-list">
        <asp:Repeater ID="rptAnnouncements" runat="server">
            <ItemTemplate>
                <div class="announcement-card">
                    <div class="announcement-header">
                        <div>
                            <div class="announcement-title"><%# Eval("Title") %></div>
                        </div>
                        <span class="announcement-audience">
                            <%# FormatAudience(Eval("Audience"), Eval("CourseCode")) %>
                        </span>
                    </div>

                    <div class="announcement-body"><%# Eval("Body") %></div>

                    <div class="announcement-footer">
                        <div class="announcement-meta">
                            <div class="announcement-meta-item">
                                <i class="fa fa-user"></i>
                                Posted by: <%# Eval("AuthorName") %>
                            </div>
                            <div class="announcement-meta-item">
                                <i class="fa fa-calendar"></i>
                                Posted: <%# ((DateTime)Eval("PublishedAt")).ToString("dd MMM yyyy HH:mm") %>
                            </div>
                            <asp:Panel ID="pnlExpiry" runat="server" Visible='<%# Eval("ExpiresAt") != DBNull.Value %>'>
                                <div class="announcement-meta-item">
                                    <i class="fa fa-clock"></i>
                                    Expires: <%# Eval("ExpiresAt", "{0:dd MMM yyyy HH:mm}") %>
                                </div>
                            </asp:Panel>
                        </div>
                        <div class="announcement-actions">
                            <asp:Button ID="btnDelete" runat="server"
                                Text="Delete"
                                CssClass="btn btn-sm btn-outline-danger"
                                OnClick="btnDelete_Click"
                                CommandArgument='<%# Eval("AnnouncementId") %>'
                                OnClientClick="return confirm('Are you sure you want to delete this announcement?');" />
                        </div>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>

        <asp:Panel ID="pnlNoAnnouncements" runat="server" Visible="false" CssClass="no-announcements">
            <i class="fa fa-bullhorn"></i>
            <h5 style="color: #1e293b; margin: 0 0 10px 0;">No Announcements Yet</h5>
            <p style="margin: 0;">Create the first announcement using the form above.</p>
        </asp:Panel>
    </div>

    <script type="text/javascript">
        document.addEventListener('DOMContentLoaded', function () {
            var txtBody = document.getElementById('<%= txtBody.ClientID %>');
            var charCount = document.getElementById('charCount');

            if (txtBody && charCount) {
                txtBody.addEventListener('input', function () {
                    charCount.textContent = this.value.length;
                });
            }
        });
    </script>

</asp:Content>
