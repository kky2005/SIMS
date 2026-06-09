<%@ Page Title="Archived Enrolments" Language="C#" MasterPageFile="~/HeadOfProgramme/HOPMaster.master"
    AutoEventWireup="true" CodeBehind="HOPArchivedEnrolments.aspx.cs"
    Inherits="SIMS.HeadOfProgramme.HOPArchivedEnrolments" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .page-title { font-size: 24px; font-weight: 700; color: #1e293b; margin-bottom: 6px; }
        .page-subtitle { color: #64748b; margin-bottom: 22px; }
        .form-label { font-weight: 600; color: #334155; }
        .table th { background: #f1f5f9; color: #334155; font-size: 13px; }
        .table td { vertical-align: middle; font-size: 14px; }
        .message-box { margin-bottom: 16px; }
        .filter-box { background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 10px; padding: 16px; margin-bottom: 18px; }
        .status-badge { padding: 5px 10px; border-radius: 999px; font-weight: 600; font-size: 12px; display: inline-block; background: #e0e7ff; color: #3730a3; }
        .empty-box { padding: 24px; text-align: center; color: #64748b; border: 1px dashed #cbd5e1; border-radius: 10px; }
        .section-count { font-size: 13px; color: #64748b; margin-left: 8px; }
        .top-action-bar { display: flex; justify-content: space-between; align-items: flex-start; gap: 12px; margin-bottom: 18px; }
        .top-action-bar .right-actions { white-space: nowrap; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="top-action-bar">
        <div>
            <h2 class="page-title">Archived Enrolments</h2>
            <p class="page-subtitle">These are approved enrolments that were archived instead of deleted.</p>
        </div>
        <div class="right-actions">
            <asp:HyperLink ID="lnkBack" runat="server"
                NavigateUrl="~/HeadOfProgramme/HOPManageEnrolments.aspx"
                CssClass="btn btn-outline-secondary">
                Back to Enrolments
            </asp:HyperLink>
        </div>
    </div>

    <asp:Label ID="lblMessage" runat="server" CssClass="message-box d-block"></asp:Label>

    <div class="card-sims mb-4">
        <div class="card-header-sims">
            <h5>Filter Archived Enrolments</h5>
        </div>
        <div class="card-body-sims">
            <div class="filter-box">
                <div class="row g-3">
                    <div class="col-md-3">
                        <label class="form-label">Student Name / No</label>
                        <asp:TextBox ID="txtSearchStudent" runat="server" CssClass="form-control" placeholder="Search student"></asp:TextBox>
                    </div>
                    <div class="col-md-3">
                        <label class="form-label">Course</label>
                        <asp:DropDownList ID="ddlFilterCourse" runat="server" CssClass="form-select"></asp:DropDownList>
                    </div>
                    <div class="col-md-3">
                        <label class="form-label">Requested From</label>
                        <asp:TextBox ID="txtFromDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                    </div>
                    <div class="col-md-3">
                        <label class="form-label">Requested To</label>
                        <asp:TextBox ID="txtToDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                    </div>
                </div>
                <div class="mt-3">
                    <asp:Button ID="btnFilter" runat="server" Text="Apply Filter" CssClass="btn btn-primary" OnClick="btnFilter_Click" />
                    <asp:Button ID="btnReset" runat="server" Text="Reset" CssClass="btn btn-secondary ms-2" OnClick="btnReset_Click" />
                </div>
            </div>
        </div>
    </div>

    <div class="card-sims">
        <div class="card-header-sims d-flex justify-content-between align-items-center">
            <h5>Archived Enrolments <asp:Label ID="lblArchivedCount" runat="server" CssClass="section-count"></asp:Label></h5>
            <asp:Button ID="btnRestoreSelected" runat="server"
                Text="Restore Selected"
                CssClass="btn btn-success btn-sm"
                OnClick="btnRestoreSelected_Click" />
        </div>
        <div class="card-body-sims">
            <asp:GridView ID="gvArchived" runat="server"
                CssClass="table table-bordered table-hover"
                AutoGenerateColumns="False"
                DataKeyNames="EnrolmentId"
                OnRowCommand="gvArchived_RowCommand">
                <Columns>
                    <asp:TemplateField>
                        <HeaderTemplate>
                            <asp:CheckBox ID="chkSelectAllArchived" runat="server" onclick="toggleArchived(this);" />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <asp:CheckBox ID="chkSelectArchived" runat="server" />
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:BoundField DataField="EnrolmentId" HeaderText="ID" />
                    <asp:BoundField DataField="StudentNo" HeaderText="Student No" />
                    <asp:BoundField DataField="StudentName" HeaderText="Student" />
                    <asp:BoundField DataField="CourseCode" HeaderText="Course Code" />
                    <asp:BoundField DataField="CourseName" HeaderText="Course" />
                    <asp:BoundField DataField="AcademicYear" HeaderText="Year" />
                    <asp:BoundField DataField="Semester" HeaderText="Sem" />
                    <asp:BoundField DataField="RequestedAt" HeaderText="Requested Date" DataFormatString="{0:yyyy-MM-dd HH:mm}" NullDisplayText="-" />
                    <asp:BoundField DataField="EnrolledAt" HeaderText="Approved Date" DataFormatString="{0:yyyy-MM-dd HH:mm}" NullDisplayText="-" />
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate><asp:Label ID="lblStatus" runat="server" Text='<%# Eval("Status") %>' CssClass="status-badge"></asp:Label></ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="LastActionBy" HeaderText="Archived By" />
                    <asp:BoundField DataField="LastActionDate" HeaderText="Archived Date" DataFormatString="{0:yyyy-MM-dd HH:mm}" NullDisplayText="-" />
                    <asp:TemplateField HeaderText="Action">
                        <ItemTemplate>
                            <asp:LinkButton ID="btnRestore" runat="server"
                                Text="Restore"
                                CssClass="btn btn-success btn-sm"
                                CommandName="RestoreEnrolment"
                                CommandArgument='<%# Eval("EnrolmentId") %>'
                                OnClientClick="return confirm('Restore this archived enrolment back to Approved?');">
                            </asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate><div class="empty-box">No archived enrolments found.</div></EmptyDataTemplate>
            </asp:GridView>
        </div>
    </div>

    <script type="text/javascript">
        function toggleArchived(source) {
            var table = document.getElementById('<%= gvArchived.ClientID %>');
            if (!table) return;

            var checkboxes = table.querySelectorAll("input[id*='chkSelectArchived']");
            for (var i = 0; i < checkboxes.length; i++) {
                checkboxes[i].checked = source.checked;
            }
        }
    </script>
</asp:Content>
