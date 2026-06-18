<%@ Page Title="Manage Course Registration Requests" Language="C#" MasterPageFile="~/HeadOfProgramme/HOPMaster.master"
    AutoEventWireup="true" CodeBehind="HOPManageEnrolments.aspx.cs"
    Inherits="SIMS.HeadOfProgramme.HOPManageEnrolments" %>

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
        .empty-box { padding: 24px; text-align: center; color: #64748b; border: 1px dashed #cbd5e1; border-radius: 10px; }
        .section-count { font-size: 13px; color: #64748b; margin-left: 8px; }
        .bulk-actions { margin-bottom: 10px; display: flex; gap: 8px; align-items: center; flex-wrap: wrap; }
        .select-col { width: 45px; text-align: center; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2 class="page-title">Manage Course Registration Requests</h2>
    <p class="page-subtitle">Students submit Register or Drop course requests. Pending requests are stored separately from final enrolment records.</p>

    <asp:Label ID="lblMessage" runat="server" CssClass="message-box d-block"></asp:Label>

    <div class="card-sims mb-4">
        <div class="card-header-sims">
            <h5>Filter Requests / Enrolments</h5>
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
                    <div class="col-md-2">
                        <label class="form-label">Request Type</label>
                        <asp:DropDownList ID="ddlFilterRequestType" runat="server" CssClass="form-select">
                            <asp:ListItem Text="All Types" Value=""></asp:ListItem>
                            <asp:ListItem Text="Register" Value="Register"></asp:ListItem>
                            <asp:ListItem Text="Drop" Value="Drop"></asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="col-md-2">
                        <label class="form-label">Requested From</label>
                        <asp:TextBox ID="txtFromDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                    </div>
                    <div class="col-md-2">
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
            <h5>Pending Course Requests <asp:Label ID="lblPendingCount" runat="server" CssClass="section-count"></asp:Label></h5>
        </div>
        <div class="card-body-sims">
            <asp:GridView ID="gvPending" runat="server"
                CssClass="table table-bordered table-hover"
                AutoGenerateColumns="False"
                DataKeyNames="RequestId"
                OnRowCommand="gvPending_RowCommand"
                OnRowDataBound="gvStatus_RowDataBound">
                <Columns>
                    <asp:BoundField DataField="RequestId" HeaderText="Request ID" />
                    <asp:BoundField DataField="RequestType" HeaderText="Type" />
                    <asp:BoundField DataField="StudentNo" HeaderText="Student No" />
                    <asp:BoundField DataField="StudentName" HeaderText="Student" />
                    <asp:BoundField DataField="CourseCode" HeaderText="Course Code" />
                    <asp:BoundField DataField="CourseName" HeaderText="Course" />
                    <asp:BoundField DataField="AcademicYear" HeaderText="Year" />
                    <asp:BoundField DataField="Semester" HeaderText="Sem" />
                    <asp:BoundField DataField="RequestedAt" HeaderText="Requested Date" DataFormatString="{0:yyyy-MM-dd HH:mm}" NullDisplayText="-" />
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate><asp:Label ID="lblStatus" runat="server" Text='<%# Eval("Status") %>'></asp:Label></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <div class="action-btns">
                                <asp:LinkButton ID="btnApprove" runat="server" CommandName="ApproveRequest" CommandArgument='<%# Eval("RequestId") %>' CssClass="btn btn-sm btn-success" OnClientClick="return confirm('Approve this course request?');">Approve</asp:LinkButton>
                                <asp:LinkButton ID="btnReject" runat="server" CommandName="RejectRequest" CommandArgument='<%# Eval("RequestId") %>' CssClass="btn btn-sm btn-danger" OnClientClick="return confirm('Reject this course request?');">Reject</asp:LinkButton>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate><div class="empty-box">No pending course requests found.</div></EmptyDataTemplate>
            </asp:GridView>
        </div>
    </div>

    <div class="card-sims mb-4">
        <div class="card-header-sims">
            <h5>Approved Enrolments <asp:Label ID="lblApprovedCount" runat="server" CssClass="section-count"></asp:Label></h5>
            <asp:HyperLink ID="lnkArchivedEnrolments" runat="server" NavigateUrl="~/HeadOfProgramme/HOPArchivedEnrolments.aspx" CssClass="btn btn-sm btn-outline-secondary">View Archived Enrolments</asp:HyperLink>
        </div>
        <div class="card-body-sims">
            <div class="bulk-actions">
                <asp:Button ID="btnArchiveSelected" runat="server" Text="Archive Selected" CssClass="btn btn-warning btn-sm" OnClick="btnArchiveSelected_Click" OnClientClick="return confirm('Archive all selected approved enrolments?');" />
            </div>
            <asp:GridView ID="gvApproved" runat="server"
                CssClass="table table-bordered table-hover"
                AutoGenerateColumns="False"
                DataKeyNames="EnrolmentId"
                OnRowCommand="gvProcessed_RowCommand"
                OnRowDataBound="gvStatus_RowDataBound">
                <Columns>
                    <asp:TemplateField HeaderStyle-CssClass="select-col" ItemStyle-CssClass="select-col">
                        <HeaderTemplate>
                            <asp:CheckBox ID="chkSelectAllApproved" runat="server" onclick="toggleApproved(this);" ToolTip="Select all approved enrolments" />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <asp:CheckBox ID="chkSelectApproved" runat="server" />
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
                        <ItemTemplate><asp:Label ID="lblStatus" runat="server" Text='<%# Eval("Status") %>'></asp:Label></ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="LastActionBy" HeaderText="Approved By" />
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <asp:LinkButton ID="btnDeleteApproved" runat="server" CommandName="ArchiveEnrolment" CommandArgument='<%# Eval("EnrolmentId") %>' CssClass="btn btn-sm btn-warning" OnClientClick="return confirm('Archive this approved enrolment record? It will be moved to the archived enrolments page.');">Archive</asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate><div class="empty-box">No approved enrolments found.</div></EmptyDataTemplate>
            </asp:GridView>
        </div>
    </div>

    <div class="card-sims">
        <div class="card-header-sims">
            <h5>Dropped Enrolments <asp:Label ID="lblRejectedCount" runat="server" CssClass="section-count"></asp:Label></h5>
        </div>
        <div class="card-body-sims">
            <div class="bulk-actions">
                <asp:Button ID="btnDeleteSelected" runat="server" Text="Delete Selected" CssClass="btn btn-danger btn-sm" OnClick="btnDeleteSelected_Click" OnClientClick="return confirm('Delete all selected dropped enrolments? Records with attendance cannot be deleted.');" />
            </div>
            <asp:GridView ID="gvRejected" runat="server"
                CssClass="table table-bordered table-hover"
                AutoGenerateColumns="False"
                DataKeyNames="EnrolmentId"
                OnRowCommand="gvProcessed_RowCommand"
                OnRowDataBound="gvStatus_RowDataBound">
                <Columns>
                    <asp:TemplateField HeaderStyle-CssClass="select-col" ItemStyle-CssClass="select-col">
                        <HeaderTemplate>
                            <asp:CheckBox ID="chkSelectAllRejected" runat="server" onclick="toggleRejected(this);" ToolTip="Select all dropped enrolments" />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <asp:CheckBox ID="chkSelectRejected" runat="server" />
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
                    <asp:BoundField DataField="DroppedAt" HeaderText="Dropped Date" DataFormatString="{0:yyyy-MM-dd HH:mm}" NullDisplayText="-" />
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate><asp:Label ID="lblStatus" runat="server" Text='<%# Eval("Status") %>'></asp:Label></ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="LastActionBy" HeaderText="Dropped By" />
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <asp:LinkButton ID="btnDeleteRejected" runat="server" CommandName="DeleteEnrolment" CommandArgument='<%# Eval("EnrolmentId") %>' CssClass="btn btn-sm btn-danger" OnClientClick="return confirm('Delete this dropped enrolment record?');">Delete</asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate><div class="empty-box">No dropped enrolments found.</div></EmptyDataTemplate>
            </asp:GridView>
        </div>
    </div>

    <script type="text/javascript">
        function toggleApproved(source) {
            var table = document.getElementById('<%= gvApproved.ClientID %>');
            if (!table) return;
            var checkboxes = table.querySelectorAll("input[id*='chkSelectApproved']");
            for (var i = 0; i < checkboxes.length; i++) {
                checkboxes[i].checked = source.checked;
            }
        }

        function toggleRejected(source) {
            var table = document.getElementById('<%= gvRejected.ClientID %>');
            if (!table) return;
            var checkboxes = table.querySelectorAll("input[id*='chkSelectRejected']");
            for (var i = 0; i < checkboxes.length; i++) {
                checkboxes[i].checked = source.checked;
            }
        }
    </script>
</asp:Content>
