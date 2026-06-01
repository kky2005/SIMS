<%@ Page Title="Manage Admissions" Language="C#" MasterPageFile="~/HeadOfProgramme/HOPMaster.master"
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
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2 class="page-title">Manage Admissions</h2>
    <p class="page-subtitle">Manage student admission programme, intake and academic status.</p>
    <asp:Label ID="lblMessage" runat="server" CssClass="message-box d-block"></asp:Label>
    <div class="card-sims mb-4"><div class="card-header-sims"><h5>Admission Record</h5></div><div class="card-body-sims">
        <asp:HiddenField ID="hfStudentId" runat="server" />
        <div class="row g-3">
            <div class="col-md-4"><label class="form-label">Student</label><asp:TextBox ID="txtStudent" runat="server" CssClass="form-control" Enabled="false"></asp:TextBox></div>
            <div class="col-md-3"><label class="form-label">Programme</label><asp:DropDownList ID="ddlProgramme" runat="server" CssClass="form-select"></asp:DropDownList></div>
            <div class="col-md-2"><label class="form-label">Intake Year</label><asp:TextBox ID="txtIntakeYear" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox></div>
            <div class="col-md-2"><label class="form-label">Intake Sem</label><asp:TextBox ID="txtIntakeSemester" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox></div>
            <div class="col-md-2"><label class="form-label">Current Sem</label><asp:TextBox ID="txtCurrentSemester" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox></div>
            <div class="col-md-3"><label class="form-label">Admission Date</label><asp:TextBox ID="txtAdmissionDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox></div>
            <div class="col-md-3"><label class="form-label">Status</label><asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select"><asp:ListItem>Active</asp:ListItem><asp:ListItem>Inactive</asp:ListItem><asp:ListItem>Graduated</asp:ListItem><asp:ListItem>Suspended</asp:ListItem></asp:DropDownList></div>
        </div>
        <div class="mt-3"><asp:Button ID="btnSave" runat="server" Text="Update Admission" CssClass="btn btn-primary" OnClick="btnSave_Click" /><asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-secondary ms-2" OnClick="btnClear_Click" /></div>
    </div></div>

    <div class="card-sims"><div class="card-header-sims"><h5>Admission List</h5></div><div class="card-body-sims">
        <asp:GridView ID="gvAdmissions" runat="server" CssClass="table table-bordered table-hover" AutoGenerateColumns="False" DataKeyNames="StudentId" OnRowCommand="gvAdmissions_RowCommand">
            <Columns>
                <asp:BoundField DataField="StudentNo" HeaderText="Student No" />
                <asp:BoundField DataField="FullName" HeaderText="Name" />
                <asp:BoundField DataField="ProgrammeName" HeaderText="Programme" />
                <asp:BoundField DataField="IntakeYear" HeaderText="Year" />
                <asp:BoundField DataField="IntakeSemester" HeaderText="Intake Sem" />
                <asp:BoundField DataField="CurrentSemester" HeaderText="Current Sem" />
                <asp:BoundField DataField="Status" HeaderText="Status" />
                <asp:TemplateField HeaderText="Actions"><ItemTemplate><asp:LinkButton runat="server" CommandName="EditAdmission" CommandArgument='<%# Eval("StudentId") %>' CssClass="btn btn-sm btn-warning">Edit</asp:LinkButton></ItemTemplate></asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div></div>
</asp:Content>
