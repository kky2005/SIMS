<%@ Page Title="Manage Enrolment Requests" Language="C#" MasterPageFile="~/HeadOfProgramme/HOPMaster.master"
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
        .empty-box { padding: 24px; text-align: center; color: #64748b; border: 1px dashed #cbd5e1; border-radius: 10px; }
        .section-count { font-size: 13px; color: #64748b; margin-left: 8px; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2 class="page-title">Manage Enrolment Requests</h2>
    <p class="page-subtitle">Students submit enrolment requests. HOP can approve or reject pending requests.</p>

    <asp:Label ID="lblMessage" runat="server" CssClass="message-box d-block"></asp:Label>

    <div class="card-sims mb-4">
        <div class="card-header-sims">
            <h5>Filter Enrolments</h5>
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

    <div class="card-sims mb-4">
        <div class="card-header-sims">
            <h5>Pending Requests <asp:Label ID="lblPendingCount" runat="server" CssClass="section-count"></asp:Label></h5>
        </div>
        <div class="card-body-sims">
            <asp:GridView ID="gvPending" runat="server"
                CssClass="table table-bordered table-hover"
                AutoGenerateColumns="False"
                DataKeyNames="EnrolmentId"
                OnRowCommand="gvPending_RowCommand"
                OnRowDataBound="gvStatus_RowDataBound">
                <Columns>
                    <asp:BoundField DataField="EnrolmentId" HeaderText="ID" />
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
                                <asp:LinkButton ID="btnApprove" runat="server" CommandName="ApproveEnrolment" CommandArgument='<%# Eval("EnrolmentId") %>' CssClass="btn btn-sm btn-success" OnClientClick="return confirm('Approve this enrolment request?');">Approve</asp:LinkButton>
                                <asp:LinkButton ID="btnReject" runat="server" CommandName="RejectEnrolment" CommandArgument='<%# Eval("EnrolmentId") %>' CssClass="btn btn-sm btn-danger" OnClientClick="return confirm('Reject this enrolment request?');">Reject</asp:LinkButton>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate><div class="empty-box">No pending enrolment requests found.</div></EmptyDataTemplate>
            </asp:GridView>
        </div>
    </div>

    <div class="card-sims mb-4">
        <div class="card-header-sims">
            <h5>Approved Enrolments <asp:Label ID="lblApprovedCount" runat="server" CssClass="section-count"></asp:Label></h5>
        </div>
        <div class="card-body-sims">
            <asp:GridView ID="gvApproved" runat="server"
                CssClass="table table-bordered table-hover"
                AutoGenerateColumns="False"
                DataKeyNames="EnrolmentId"
                OnRowCommand="gvProcessed_RowCommand"
                OnRowDataBound="gvStatus_RowDataBound">
                <Columns>
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
                            <asp:LinkButton ID="btnDeleteApproved" runat="server" CommandName="DeleteEnrolment" CommandArgument='<%# Eval("EnrolmentId") %>' CssClass="btn btn-sm btn-danger" OnClientClick="return confirm('Delete this approved enrolment record?');">Delete</asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate><div class="empty-box">No approved enrolments found.</div></EmptyDataTemplate>
            </asp:GridView>
        </div>
    </div>

    <div class="card-sims">
        <div class="card-header-sims">
            <h5>Rejected Enrolments <asp:Label ID="lblRejectedCount" runat="server" CssClass="section-count"></asp:Label></h5>
        </div>
        <div class="card-body-sims">
            <asp:GridView ID="gvRejected" runat="server"
                CssClass="table table-bordered table-hover"
                AutoGenerateColumns="False"
                DataKeyNames="EnrolmentId"
                OnRowCommand="gvProcessed_RowCommand"
                OnRowDataBound="gvStatus_RowDataBound">
                <Columns>
                    <asp:BoundField DataField="EnrolmentId" HeaderText="ID" />
                    <asp:BoundField DataField="StudentNo" HeaderText="Student No" />
                    <asp:BoundField DataField="StudentName" HeaderText="Student" />
                    <asp:BoundField DataField="CourseCode" HeaderText="Course Code" />
                    <asp:BoundField DataField="CourseName" HeaderText="Course" />
                    <asp:BoundField DataField="AcademicYear" HeaderText="Year" />
                    <asp:BoundField DataField="Semester" HeaderText="Sem" />
                    <asp:BoundField DataField="RequestedAt" HeaderText="Requested Date" DataFormatString="{0:yyyy-MM-dd HH:mm}" NullDisplayText="-" />
                    <asp:BoundField DataField="DroppedAt" HeaderText="Rejected Date" DataFormatString="{0:yyyy-MM-dd HH:mm}" NullDisplayText="-" />
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate><asp:Label ID="lblStatus" runat="server" Text='<%# Eval("Status") %>'></asp:Label></ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="LastActionBy" HeaderText="Rejected By" />
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <asp:LinkButton ID="btnDeleteRejected" runat="server" CommandName="DeleteEnrolment" CommandArgument='<%# Eval("EnrolmentId") %>' CssClass="btn btn-sm btn-danger" OnClientClick="return confirm('Delete this rejected enrolment record?');">Delete</asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate><div class="empty-box">No rejected enrolments found.</div></EmptyDataTemplate>
            </asp:GridView>
        </div>
    </div>
</asp:Content>
