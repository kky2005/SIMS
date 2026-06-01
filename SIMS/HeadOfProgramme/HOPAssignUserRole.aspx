<%@ Page Title="Assign User Role" Language="C#" MasterPageFile="~/HeadOfProgramme/HOPMaster.master"
    AutoEventWireup="true" CodeBehind="HOPAssignUserRole.aspx.cs"
    Inherits="SIMS.HeadOfProgramme.HOPAssignUserRole" %>

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
    <h2 class="page-title">Assign User Roles</h2>
    <p class="page-subtitle">Update user role and account active status.</p>
    <asp:Label ID="lblMessage" runat="server" CssClass="message-box d-block"></asp:Label>
    <div class="card-sims mb-4"><div class="card-header-sims"><h5>Role Assignment</h5></div><div class="card-body-sims">
        <asp:HiddenField ID="hfUserId" runat="server" />
        <div class="row g-3">
            <div class="col-md-4"><label class="form-label">Selected User</label><asp:TextBox ID="txtSelectedUser" runat="server" CssClass="form-control" Enabled="false"></asp:TextBox></div>
            <div class="col-md-4"><label class="form-label">Role</label><asp:DropDownList ID="ddlRole" runat="server" CssClass="form-select"></asp:DropDownList></div>
            <div class="col-md-2"><label class="form-label">Active</label><asp:DropDownList ID="ddlIsActive" runat="server" CssClass="form-select"><asp:ListItem Value="1">Yes</asp:ListItem><asp:ListItem Value="0">No</asp:ListItem></asp:DropDownList></div>
        </div>
        <div class="mt-3"><asp:Button ID="btnSave" runat="server" Text="Update Role" CssClass="btn btn-primary" OnClick="btnSave_Click" /><asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-secondary ms-2" OnClick="btnClear_Click" /></div>
    </div></div>

    <div class="card-sims"><div class="card-header-sims"><h5>User List</h5></div><div class="card-body-sims">
        <asp:GridView ID="gvUsers" runat="server" CssClass="table table-bordered table-hover" AutoGenerateColumns="False" DataKeyNames="UserId" OnRowCommand="gvUsers_RowCommand">
            <Columns>
                <asp:BoundField DataField="FullName" HeaderText="Name" />
                <asp:BoundField DataField="Email" HeaderText="Email" />
                <asp:BoundField DataField="RoleName" HeaderText="Role" />
                <asp:BoundField DataField="IsActiveText" HeaderText="Active" />
                <asp:TemplateField HeaderText="Actions"><ItemTemplate><asp:LinkButton runat="server" CommandName="SelectUser" CommandArgument='<%# Eval("UserId") %>' CssClass="btn btn-sm btn-warning">Edit Role</asp:LinkButton></ItemTemplate></asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div></div>
</asp:Content>
