<%@ Page Title="Manage Admission Requests" Language="C#" MasterPageFile="~/HeadOfProgramme/HOPMaster.master"
    AutoEventWireup="true" CodeBehind="HOPManageAdmissions.aspx.cs"
    Inherits="SIMS.HeadOfProgramme.HOPManageAdmissions" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .page-title { font-size: 24px; font-weight: 700; color: #1e293b; margin-bottom: 6px; }
        .page-subtitle { color: #64748b; margin-bottom: 22px; }
        .form-label { font-weight: 600; color: #334155; }
        .table th { background: #f1f5f9; color: #334155; font-size: 13px; }
        .table td { vertical-align: middle; font-size: 14px; }
        .action-btns .btn { margin-right: 4px; margin-bottom: 4px; }
        .message-box { margin-bottom: 16px; }
        .filter-box { background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 10px; padding: 16px; margin-bottom: 18px; }
        .status-badge { padding: 5px 10px; border-radius: 999px; font-weight: 600; font-size: 12px; display: inline-block; }
        .status-pending { background: #fef3c7; color: #92400e; }
        .status-approved { background: #dcfce7; color: #166534; }
        .status-rejected { background: #fee2e2; color: #991b1b; }
        .status-archived { background: #e0e7ff; color: #3730a3; }
        .bulk-actions { margin-bottom: 12px; display: flex; gap: 8px; align-items: center; flex-wrap: wrap; }
        .empty-box { padding: 24px; text-align: center; color: #64748b; border: 1px dashed #cbd5e1; border-radius: 10px; }
        .section-count { font-size: 13px; color: #64748b; margin-left: 8px; }
    </style>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2 class="page-title">Manage Admission Requests</h2>
    <p class="page-subtitle">Applicants submit admission requests. HOP can admit or reject pending requests.</p>

    <asp:Label ID="lblMessage" runat="server" CssClass="message-box d-block"></asp:Label>

    <div class="card-sims mb-4">
        <div class="card-header-sims">
            <h5>Filter Admissions</h5>
        </div>
        <div class="card-body-sims">
            <div class="filter-box">
                <div class="row g-3">
                    <div class="col-md-3">
                        <label class="form-label">Applicant Name / No / Email</label>
                        <asp:TextBox ID="txtSearchStudent" runat="server" CssClass="form-control" placeholder="Search applicant"></asp:TextBox>
                    </div>
                    <div class="col-md-3">
                        <label class="form-label">Programme</label>
                        <asp:DropDownList ID="ddlFilterProgramme" runat="server" CssClass="form-select"></asp:DropDownList>
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

    <div class="card-sims mb-4">
        <div class="card-header-sims">
            <h5>Pending Requests <asp:Label ID="lblPendingCount" runat="server" CssClass="section-count"></asp:Label></h5>
        </div>
        <div class="card-body-sims">
            <asp:GridView ID="gvPending" runat="server"
                CssClass="table table-bordered table-hover"
                AutoGenerateColumns="False"
                DataKeyNames="AdmissionId"
                OnRowCommand="gvPending_RowCommand"
                OnRowDataBound="gvStatus_RowDataBound">
                <Columns>
                    <asp:BoundField DataField="AdmissionId" HeaderText="ID" />
                    <asp:BoundField DataField="StudentNo" HeaderText="Student No" />
                    <asp:BoundField DataField="StudentName" HeaderText="Applicant" />
                    <asp:BoundField DataField="ProgrammeName" HeaderText="Programme" />
                    <asp:BoundField DataField="IntakeYear" HeaderText="Intake Year" />
                    <asp:BoundField DataField="IntakeSemester" HeaderText="Intake Sem" />
                    <asp:BoundField DataField="RequestedAt" HeaderText="Requested Date" DataFormatString="{0:yyyy-MM-dd HH:mm}" NullDisplayText="-" />
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate><asp:Label ID="lblStatus" runat="server" Text='<%# Eval("Status") %>'></asp:Label></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <div class="action-btns">
                                <asp:LinkButton ID="btnApprove" runat="server" CommandName="ApproveAdmission" CommandArgument='<%# Eval("AdmissionId") %>' CssClass="btn btn-sm btn-success" OnClientClick="return confirm('Admit this admission request?');">Admit</asp:LinkButton>
                                <asp:LinkButton ID="btnReject" runat="server" CommandName="RejectAdmission" CommandArgument='<%# Eval("AdmissionId") %>' CssClass="btn btn-sm btn-danger" OnClientClick="return confirm('Reject this admission request?');">Reject</asp:LinkButton>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate><div class="empty-box">No pending admission requests found.</div></EmptyDataTemplate>
            </asp:GridView>
        </div>
    </div>

    <div class="card-sims mb-4">
        <div class="card-header-sims">
            <div class="d-flex justify-content-between align-items-center w-100">
                <h5>Admitted Admissions <asp:Label ID="lblApprovedCount" runat="server" CssClass="section-count"></asp:Label></h5>
                <a href="HOPArchivedAdmissions.aspx" class="btn btn-sm btn-outline-secondary">View Archived Admissions</a>
            </div>
        </div>
        <div class="card-body-sims">
            <div class="bulk-actions">
                <asp:Button ID="btnArchiveSelectedApproved" runat="server" Text="Archive Selected" CssClass="btn btn-warning" OnClick="btnArchiveSelectedApproved_Click" OnClientClick="return confirm('Archive selected admitted admissions?');" />
            </div>
            <asp:GridView ID="gvApproved" runat="server"
                CssClass="table table-bordered table-hover"
                AutoGenerateColumns="False"
                DataKeyNames="AdmissionId"
                OnRowCommand="gvProcessed_RowCommand"
                OnRowDataBound="gvStatus_RowDataBound">
                <Columns>
                    <asp:TemplateField>
                        <HeaderTemplate>
                            <asp:CheckBox ID="chkSelectAllApproved" runat="server" onclick="toggleApprovedAdmissions(this);" />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <asp:CheckBox ID="chkSelectApproved" runat="server" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="AdmissionId" HeaderText="ID" />
                    <asp:BoundField DataField="StudentNo" HeaderText="Student No" />
                    <asp:BoundField DataField="StudentName" HeaderText="Applicant" />
                    <asp:BoundField DataField="ProgrammeName" HeaderText="Programme" />
                    <asp:BoundField DataField="IntakeYear" HeaderText="Intake Year" />
                    <asp:BoundField DataField="IntakeSemester" HeaderText="Intake Sem" />
                    <asp:BoundField DataField="RequestedAt" HeaderText="Requested Date" DataFormatString="{0:yyyy-MM-dd HH:mm}" NullDisplayText="-" />
                    <asp:BoundField DataField="AdmittedAt" HeaderText="Admitted Date" DataFormatString="{0:yyyy-MM-dd HH:mm}" NullDisplayText="-" />
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate><asp:Label ID="lblStatus" runat="server" Text='<%# Eval("Status") %>'></asp:Label></ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="LastActionBy" HeaderText="Admitted By" />
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <asp:LinkButton ID="btnArchiveApproved" runat="server" CommandName="ArchiveAdmission" CommandArgument='<%# Eval("AdmissionId") %>' CssClass="btn btn-sm btn-warning" OnClientClick="return confirm('Archive this admitted admission record?');">Archive</asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate><div class="empty-box">No admitted admissions found.</div></EmptyDataTemplate>
            </asp:GridView>
        </div>
    </div>

    <div class="card-sims">
        <div class="card-header-sims">
            <h5>Rejected Admissions <asp:Label ID="lblRejectedCount" runat="server" CssClass="section-count"></asp:Label></h5>
        </div>
        <div class="card-body-sims">
            <div class="bulk-actions">
                <asp:Button ID="btnDeleteSelectedRejected" runat="server" Text="Delete Selected" CssClass="btn btn-danger" OnClick="btnDeleteSelectedRejected_Click" OnClientClick="return confirm('Delete selected rejected admissions?');" />
            </div>
            <asp:GridView ID="gvRejected" runat="server"
                CssClass="table table-bordered table-hover"
                AutoGenerateColumns="False"
                DataKeyNames="AdmissionId"
                OnRowCommand="gvProcessed_RowCommand"
                OnRowDataBound="gvStatus_RowDataBound">
                <Columns>
                    <asp:TemplateField>
                        <HeaderTemplate>
                            <asp:CheckBox ID="chkSelectAllRejected" runat="server" onclick="toggleRejectedAdmissions(this);" />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <asp:CheckBox ID="chkSelectRejected" runat="server" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="AdmissionId" HeaderText="ID" />
                    <asp:BoundField DataField="StudentNo" HeaderText="Student No" />
                    <asp:BoundField DataField="StudentName" HeaderText="Applicant" />
                    <asp:BoundField DataField="ProgrammeName" HeaderText="Programme" />
                    <asp:BoundField DataField="IntakeYear" HeaderText="Intake Year" />
                    <asp:BoundField DataField="IntakeSemester" HeaderText="Intake Sem" />
                    <asp:BoundField DataField="RequestedAt" HeaderText="Requested Date" DataFormatString="{0:yyyy-MM-dd HH:mm}" NullDisplayText="-" />
                    <asp:BoundField DataField="RejectedAt" HeaderText="Rejected Date" DataFormatString="{0:yyyy-MM-dd HH:mm}" NullDisplayText="-" />
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate><asp:Label ID="lblStatus" runat="server" Text='<%# Eval("Status") %>'></asp:Label></ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="LastActionBy" HeaderText="Rejected By" />
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <asp:LinkButton ID="btnDeleteRejected" runat="server" CommandName="DeleteAdmission" CommandArgument='<%# Eval("AdmissionId") %>' CssClass="btn btn-sm btn-danger" OnClientClick="return confirm('Delete this rejected admission record?');">Delete</asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate><div class="empty-box">No rejected admissions found.</div></EmptyDataTemplate>
            </asp:GridView>
        </div>
    </div>

    <script type="text/javascript">
        function toggleApprovedAdmissions(source) {
            var table = document.getElementById('<%= gvApproved.ClientID %>');
            if (!table) return;
            var boxes = table.querySelectorAll("input[id*='chkSelectApproved']");
            for (var i = 0; i < boxes.length; i++) {
                boxes[i].checked = source.checked;
            }
        }

        function toggleRejectedAdmissions(source) {
            var table = document.getElementById('<%= gvRejected.ClientID %>');
            if (!table) return;
            var boxes = table.querySelectorAll("input[id*='chkSelectRejected']");
            for (var i = 0; i < boxes.length; i++) {
                boxes[i].checked = source.checked;
            }
        }
    </script>
</asp:Content>
