<%@ Page Title="Register Student" Language="C#" MasterPageFile="~/HeadOfProgramme/HOPMaster.master"
    AutoEventWireup="true" CodeBehind="HOPRegisterStudent.aspx.cs"
    Inherits="SIMS.HeadOfProgramme.HOPRegisterStudent" %>

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
    <h2 class="page-title">Register Student</h2>
    <p class="page-subtitle">Create student user accounts and admission records.</p>
    <asp:Label ID="lblMessage" runat="server" CssClass="message-box d-block"></asp:Label>

    <div class="card-sims mb-4"><div class="card-header-sims"><h5>Student Form</h5></div><div class="card-body-sims">
        <asp:HiddenField ID="hfStudentId" runat="server" /><asp:HiddenField ID="hfUserId" runat="server" />
        <div class="row g-3">
            <div class="col-md-4"><label class="form-label">Full Name</label><asp:TextBox ID="txtFullName" runat="server" CssClass="form-control"></asp:TextBox></div>
            <div class="col-md-4"><label class="form-label">Email</label><asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email"></asp:TextBox></div>
            <div class="col-md-4"><label class="form-label">Phone</label><asp:TextBox ID="txtPhone" runat="server" CssClass="form-control"></asp:TextBox></div>
            <div class="col-md-3"><label class="form-label">Password</label><asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password" placeholder="Required for new student"></asp:TextBox></div>
            <div class="col-md-3"><label class="form-label">Student No</label><asp:TextBox ID="txtStudentNo" runat="server" CssClass="form-control"></asp:TextBox></div>
            <div class="col-md-3"><label class="form-label">Programme</label><asp:DropDownList ID="ddlProgramme" runat="server" CssClass="form-select"></asp:DropDownList></div>
            <div class="col-md-2"><label class="form-label">Intake Year</label><asp:TextBox ID="txtIntakeYear" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox></div>
            <div class="col-md-2"><label class="form-label">Intake Sem</label><asp:TextBox ID="txtIntakeSemester" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox></div>
            <div class="col-md-2"><label class="form-label">Current Sem</label><asp:TextBox ID="txtCurrentSemester" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox></div>
            <div class="col-md-3"><label class="form-label">Admission Date</label><asp:TextBox ID="txtAdmissionDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox></div>
            <div class="col-md-3"><label class="form-label">Status</label><asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select"><asp:ListItem>Active</asp:ListItem><asp:ListItem>Inactive</asp:ListItem><asp:ListItem>Graduated</asp:ListItem><asp:ListItem>Suspended</asp:ListItem></asp:DropDownList></div>
        </div>
        <div class="mt-3"><asp:Button ID="btnSave" runat="server" Text="Save Student" CssClass="btn btn-primary" OnClick="btnSave_Click" /><asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-secondary ms-2" OnClick="btnClear_Click" CausesValidation="false" /></div>
    </div></div>

    <div class="card-sims"><div class="card-header-sims"><h5>Student List</h5></div><div class="card-body-sims">
        <asp:GridView ID="gvStudents" runat="server" CssClass="table table-bordered table-hover" AutoGenerateColumns="False" DataKeyNames="StudentId" OnRowCommand="gvStudents_RowCommand">
            <Columns>
                <asp:BoundField DataField="StudentNo" HeaderText="Student No" />
                <asp:BoundField DataField="FullName" HeaderText="Name" />
                <asp:BoundField DataField="Email" HeaderText="Email" />
                <asp:BoundField DataField="ProgrammeName" HeaderText="Programme" />
                <asp:BoundField DataField="CurrentSemester" HeaderText="Sem" />
                <asp:BoundField DataField="Status" HeaderText="Status" />
                <asp:TemplateField HeaderText="Actions"><ItemTemplate><div class="action-btns">
                    <asp:LinkButton runat="server" CommandName="EditStudent" CommandArgument='<%# Eval("StudentId") %>' CssClass="btn btn-sm btn-warning">Edit</asp:LinkButton>
                    <asp:LinkButton runat="server" CommandName="DeleteStudent" CommandArgument='<%# Eval("StudentId") %>' CssClass="btn btn-sm btn-danger" OnClientClick="return confirm('Delete this student?');">Delete</asp:LinkButton>
                </div></ItemTemplate></asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div></div>
</asp:Content>
