<%@ Page Title="Archived Admissions" Language="C#" MasterPageFile="~/HeadOfProgramme/HOPMaster.master"
    AutoEventWireup="true" CodeBehind="HOPArchivedAdmissions.aspx.cs"
    Inherits="SIMS.HeadOfProgramme.HOPArchivedAdmissions" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .page-title { font-size: 24px; font-weight: 700; color: #1e293b; margin-bottom: 6px; }
        .page-subtitle { color: #64748b; margin-bottom: 22px; }
        .form-label { font-weight: 600; color: #334155; }
        .table th { background: #f1f5f9; color: #334155; font-size: 13px; }
        .table td { vertical-align: middle; font-size: 14px; }
        .message-box { margin-bottom: 16px; }
        .filter-box { background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 10px; padding: 16px; margin-bottom: 18px; }
        .status-badge { padding: 5px 10px; border-radius: 999px; font-weight: 600; font-size: 12px; display: inline-block; }
        .status-archived { background: #e0e7ff; color: #3730a3; }
        .empty-box { padding: 24px; text-align: center; color: #64748b; border: 1px dashed #cbd5e1; border-radius: 10px; }
        .section-count { font-size: 13px; color: #64748b; margin-left: 8px; }
        .bulk-actions { margin-bottom: 12px; display: flex; gap: 8px; align-items: center; flex-wrap: wrap; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-2">
        <div>
            <h2 class="page-title">Archived Admissions</h2>
            <p class="page-subtitle">View archived admitted admissions and restore them back to Admitted when needed.</p>
        </div>
        <a href="HOPManageAdmissions.aspx" class="btn btn-outline-secondary">Back to Admissions</a>
    </div>

    <asp:Label ID="lblMessage" runat="server" CssClass="message-box d-block"></asp:Label>

    <div class="card-sims mb-4">
        <div class="card-header-sims"><h5>Filter Archived Admissions</h5></div>
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

    <div class="card-sims">
        <div class="card-header-sims"><h5>Archived Records <asp:Label ID="lblArchivedCount" runat="server" CssClass="section-count"></asp:Label></h5></div>
        <div class="card-body-sims">
            <div class="bulk-actions">
                <asp:Button ID="btnRestoreSelected" runat="server" Text="Restore Selected" CssClass="btn btn-success" OnClick="btnRestoreSelected_Click" OnClientClick="return confirm('Restore selected archived admissions?');" />
            </div>

            <asp:GridView ID="gvArchived" runat="server"
                CssClass="table table-bordered table-hover"
                AutoGenerateColumns="False"
                DataKeyNames="AdmissionId"
                OnRowCommand="gvArchived_RowCommand"
                OnRowDataBound="gvStatus_RowDataBound">
                <Columns>
                    <asp:TemplateField>
                        <HeaderTemplate>
                            <asp:CheckBox ID="chkSelectAllArchived" runat="server" onclick="toggleArchivedAdmissions(this);" />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <asp:CheckBox ID="chkSelectArchived" runat="server" />
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
                    <asp:BoundField DataField="LastActionBy" HeaderText="Archived By" />
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <asp:LinkButton ID="btnRestore" runat="server" CommandName="RestoreAdmission" CommandArgument='<%# Eval("AdmissionId") %>' CssClass="btn btn-sm btn-success" OnClientClick="return confirm('Restore this admission back to Admitted?');">Restore</asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate><div class="empty-box">No archived admissions found.</div></EmptyDataTemplate>
            </asp:GridView>
        </div>
    </div>

    <script type="text/javascript">
        function toggleArchivedAdmissions(source) {
            var table = document.getElementById('<%= gvArchived.ClientID %>');
            if (!table) return;
            var boxes = table.querySelectorAll("input[id*='chkSelectArchived']");
            for (var i = 0; i < boxes.length; i++) {
                boxes[i].checked = source.checked;
            }
        }
    </script>
</asp:Content>
