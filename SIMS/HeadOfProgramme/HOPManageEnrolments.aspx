<%@ Page Title="Manage Enrolments" Language="C#" MasterPageFile="~/HeadOfProgramme/HOPMaster.master"
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
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2 class="page-title">Manage Enrolments</h2>
    <p class="page-subtitle">Add, edit, drop and delete course enrolment records.</p>
    <asp:Label ID="lblMessage" runat="server" CssClass="message-box d-block"></asp:Label>

    <div class="card-sims mb-4"><div class="card-header-sims"><h5>Enrolment Form</h5></div><div class="card-body-sims">
        <asp:HiddenField ID="hfEnrolmentId" runat="server" />
        <div class="row g-3">
            <div class="col-md-4"><label class="form-label">Student</label><asp:DropDownList ID="ddlStudent" runat="server" CssClass="form-select"></asp:DropDownList></div>
            <div class="col-md-4"><label class="form-label">Course</label><asp:DropDownList ID="ddlCourse" runat="server" CssClass="form-select"></asp:DropDownList></div>
            <div class="col-md-2"><label class="form-label">Academic Year</label><asp:TextBox ID="txtAcademicYear" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox></div>
            <div class="col-md-2"><label class="form-label">Semester</label><asp:TextBox ID="txtSemester" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox></div>
            <div class="col-md-3"><label class="form-label">Status</label><asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select"><asp:ListItem>Active</asp:ListItem><asp:ListItem>Dropped</asp:ListItem><asp:ListItem>Completed</asp:ListItem></asp:DropDownList></div>
        </div>
        <div class="mt-3"><asp:Button ID="btnSave" runat="server" Text="Save Enrolment" CssClass="btn btn-primary" OnClick="btnSave_Click" /><asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-secondary ms-2" OnClick="btnClear_Click" /></div>
    </div></div>

    <div class="card-sims"><div class="card-header-sims"><h5>Enrolment List</h5></div><div class="card-body-sims">
        <asp:GridView ID="gvEnrolments" runat="server" CssClass="table table-bordered table-hover" AutoGenerateColumns="False" DataKeyNames="EnrolmentId" OnRowCommand="gvEnrolments_RowCommand">
            <Columns>
                <asp:BoundField DataField="StudentNo" HeaderText="Student No" />
                <asp:BoundField DataField="FullName" HeaderText="Student" />
                <asp:BoundField DataField="CourseCode" HeaderText="Course Code" />
                <asp:BoundField DataField="CourseName" HeaderText="Course" />
                <asp:BoundField DataField="AcademicYear" HeaderText="Year" />
                <asp:BoundField DataField="Semester" HeaderText="Sem" />
                <asp:BoundField DataField="Status" HeaderText="Status" />
                <asp:TemplateField HeaderText="Actions"><ItemTemplate><div class="action-btns">
                    <asp:LinkButton runat="server" CommandName="EditEnrolment" CommandArgument='<%# Eval("EnrolmentId") %>' CssClass="btn btn-sm btn-warning">Edit</asp:LinkButton>
                    <asp:LinkButton runat="server" CommandName="DropEnrolment" CommandArgument='<%# Eval("EnrolmentId") %>' CssClass="btn btn-sm btn-secondary">Drop</asp:LinkButton>
                    <asp:LinkButton runat="server" CommandName="DeleteEnrolment" CommandArgument='<%# Eval("EnrolmentId") %>' CssClass="btn btn-sm btn-danger" OnClientClick="return confirm('Delete this enrolment?');">Delete</asp:LinkButton>
                </div></ItemTemplate></asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div></div>
</asp:Content>
