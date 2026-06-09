<%@ Page Title="Audit Logs" Language="C#" MasterPageFile="~/HeadOfProgramme/HOPMaster.Master"
    AutoEventWireup="true" CodeBehind="HOPAuditLogs.aspx.cs"
    Inherits="SIMS.HeadOfProgramme.HOPAuditLogs" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .audit-page-title {
            font-size: 24px;
            font-weight: 700;
            color: #1e293b;
            margin-bottom: 4px;
        }

        .audit-page-subtitle {
            color: #64748b;
            margin-bottom: 20px;
        }

        .audit-toolbar {
            display: flex;
            justify-content: space-between;
            align-items: center;
            gap: 12px;
            flex-wrap: wrap;
        }

        .audit-search {
            max-width: 360px;
        }

        .audit-grid {
            width: 100%;
            border-collapse: collapse;
        }

        .audit-grid th {
            background: #f8fafc;
            color: #334155;
            font-weight: 700;
            padding: 12px;
            border-bottom: 1px solid #e2e8f0;
            white-space: nowrap;
        }

        .audit-grid td {
            padding: 12px;
            border-bottom: 1px solid #e2e8f0;
            color: #334155;
            vertical-align: top;
        }

        .audit-grid tr:hover {
            background: #f8fafc;
        }

        .value-cell {
            max-width: 220px;
            word-break: break-word;
            font-size: 13px;
            color: #475569;
        }

        .empty-box {
            text-align: center;
            padding: 35px;
            color: #64748b;
        }
    </style>

    <script type="text/javascript">
        function toggleAuditLogs(source) {
            var table = document.getElementById('<%= gvAuditLogs.ClientID %>');

            if (!table) {
                return;
            }

            var checkboxes = table.querySelectorAll("input[type='checkbox'][id*='chkSelect']");

            for (var i = 0; i < checkboxes.length; i++) {
                if (checkboxes[i] !== source) {
                    checkboxes[i].checked = source.checked;
                }
            }
        }
    </script>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2 class="audit-page-title">Audit Logs</h2>
    <p class="audit-page-subtitle">View and delete system activity records.</p>

    <asp:Label ID="lblMessage" runat="server" CssClass="d-block mb-3"></asp:Label>

    <div class="card-sims">
        <div class="card-header-sims audit-toolbar">
            <h5><i class="fa fa-clock-rotate-left me-2"></i>System Audit Logs</h5>

            <div class="d-flex gap-2 flex-wrap">
                <asp:TextBox ID="txtSearch" runat="server"
                    CssClass="form-control audit-search"
                    placeholder="Search action, table, user name, email..." />

                <asp:Button ID="btnSearch" runat="server"
                    Text="Search"
                    CssClass="btn btn-primary"
                    OnClick="btnSearch_Click" />

                <asp:Button ID="btnClear" runat="server"
                    Text="Clear"
                    CssClass="btn btn-secondary"
                    OnClick="btnClear_Click" />

                <asp:Button ID="btnDeleteSelected" runat="server"
                    Text="Delete Selected"
                    CssClass="btn btn-danger"
                    OnClick="btnDeleteSelected_Click"
                    OnClientClick="return confirm('Are you sure you want to delete the selected audit logs?');" />
            </div>
        </div>

        <div class="card-body-sims">
            <div class="table-responsive">
                <asp:GridView ID="gvAuditLogs" runat="server"
                    AutoGenerateColumns="False"
                    CssClass="audit-grid"
                    DataKeyNames="LogId"
                    AllowPaging="True"
                    PageSize="10"
                    OnPageIndexChanging="gvAuditLogs_PageIndexChanging"
                    OnRowCommand="gvAuditLogs_RowCommand"
                    EmptyDataText="No audit logs found.">

                    <Columns>
                        <asp:TemplateField HeaderText="Select All">
                            <HeaderTemplate>
                                <div class="d-flex align-items-center gap-1">
                                    <asp:CheckBox ID="chkSelectAll" runat="server" onclick="toggleAuditLogs(this);" ToolTip="Select all audit logs on this page" />
                                    <span>Select All</span>
                                </div>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <asp:CheckBox ID="chkSelect" runat="server" />
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:BoundField DataField="LogId" HeaderText="Log ID" />
                        <asp:BoundField DataField="FullName" HeaderText="User" />
                        <asp:BoundField DataField="Email" HeaderText="Email" />
                        <asp:BoundField DataField="Action" HeaderText="Action" />
                        <asp:BoundField DataField="TableAffected" HeaderText="Table" />
                        <asp:BoundField DataField="RecordId" HeaderText="Record ID" />
                        <asp:BoundField DataField="ActionDate" HeaderText="Date" DataFormatString="{0:yyyy-MM-dd HH:mm:ss}" />

                        <asp:TemplateField HeaderText="Old Value">
                            <ItemTemplate>
                                <div class="value-cell"><%# Eval("OldValue") %></div>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="New Value">
                            <ItemTemplate>
                                <div class="value-cell"><%# Eval("NewValue") %></div>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Action">
                            <ItemTemplate>
                                <asp:LinkButton ID="btnDelete" runat="server"
                                    CssClass="btn btn-sm btn-outline-danger"
                                    CommandName="DeleteLog"
                                    CommandArgument='<%# Eval("LogId") %>'
                                    OnClientClick="return confirm('Are you sure you want to delete this audit log?');">
                                    <i class="fa fa-trash"></i>
                                </asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>

                    <EmptyDataTemplate>
                        <div class="empty-box">
                            <i class="fa fa-circle-info me-2"></i>No audit logs found.
                        </div>
                    </EmptyDataTemplate>

                </asp:GridView>
            </div>
        </div>
    </div>


    <script type="text/javascript">
        function toggleAuditLogs(source) {
            var table = document.getElementById('<%= gvAuditLogs.ClientID %>');

            if (!table) {
                return;
            }

            var checkboxes = table.querySelectorAll("input[type='checkbox'][id*='chkSelect']");

            for (var i = 0; i < checkboxes.length; i++) {
                if (checkboxes[i] !== source) {
                    checkboxes[i].checked = source.checked;
                }
            }
        }
    </script>

</asp:Content>
