<%@ Page Title="Manage Programmes" Language="C#" MasterPageFile="~/HeadOfProgramme/HOPMaster.master"
    AutoEventWireup="true" CodeBehind="HOPManageProgrammes.aspx.cs"
    Inherits="SIMS.HeadOfProgramme.HOPManageProgrammes" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .page-title { font-size: 24px; font-weight: 700; color: #1e293b; margin-bottom: 6px; }
        .page-subtitle { color: #64748b; margin-bottom: 22px; }
        .form-label { font-weight: 600; color: #334155; }
        .table th { background: #f1f5f9; color: #334155; font-size: 13px; }
        .table td { vertical-align: middle; font-size: 14px; }
        .action-btns .btn { margin-right: 4px; margin-bottom: 4px; }
        .message-box { margin-bottom: 16px; }
        .small-muted { color: #64748b; font-size: 12px; line-height: 1.4; }
        .stat-pill { display:inline-block; padding:4px 8px; border-radius:999px; background:#f1f5f9; color:#334155; font-size:12px; font-weight:600; margin:2px 4px 2px 0; }
        .wide-grid { min-width: 1200px; }
        .grid-scroll { overflow-x:auto; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2 class="page-title">Manage Programmes</h2>
    <p class="page-subtitle">Add, edit and manage academic programme information with course, lecturer and student details.</p>
    <asp:Label ID="lblMessage" runat="server" CssClass="message-box d-block"></asp:Label>

    <div class="card-sims mb-4">
        <div class="card-header-sims"><h5>Programme Form</h5></div>
        <div class="card-body-sims">
            <asp:HiddenField ID="hfProgrammeId" runat="server" />
            <div class="row g-3">
                <div class="col-md-3">
                    <label class="form-label">Programme Code</label>
                    <asp:TextBox ID="txtProgrammeCode" runat="server" CssClass="form-control" placeholder="e.g. CS"></asp:TextBox>
                </div>
                <div class="col-md-5">
                    <label class="form-label">Programme Name</label>
                    <asp:TextBox ID="txtProgrammeName" runat="server" CssClass="form-control" placeholder="e.g. Computer Science"></asp:TextBox>
                </div>
                <div class="col-md-2">
                    <label class="form-label">Duration Years</label>
                    <asp:TextBox ID="txtDurationYears" runat="server" CssClass="form-control" TextMode="Number" Text="3"></asp:TextBox>
                </div>
                <div class="col-md-2">
                    <label class="form-label">Active</label>
                    <asp:DropDownList ID="ddlIsActive" runat="server" CssClass="form-select">
                        <asp:ListItem Value="1">Yes</asp:ListItem>
                        <asp:ListItem Value="0">No</asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-md-12">
                    <label class="form-label">Description</label>
                    <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3"></asp:TextBox>
                </div>
            </div>
            <div class="mt-3">
                <asp:Button ID="btnSave" runat="server" Text="Save Programme" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-secondary" OnClick="btnClear_Click" CausesValidation="false" />
            </div>
        </div>
    </div>

        <div class="card-sims mb-4">
        <div class="card-header-sims"><h5>Filter Programmes</h5></div>
        <div class="card-body-sims">
            <div class="row g-3 align-items-end">
                <div class="col-md-5">
                    <label class="form-label">Search Programme</label>
                    <asp:TextBox ID="txtFilterProgramme" runat="server" CssClass="form-control" placeholder="Search code, name or description"></asp:TextBox>
                </div>
                <div class="col-md-3">
                    <label class="form-label">Active Status</label>
                    <asp:DropDownList ID="ddlFilterActive" runat="server" CssClass="form-select">
                        <asp:ListItem Value="">All</asp:ListItem>
                        <asp:ListItem Value="1">Active</asp:ListItem>
                        <asp:ListItem Value="0">Inactive</asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-md-4">
                    <asp:Button ID="btnFilter" runat="server" Text="Filter" CssClass="btn btn-primary" OnClick="btnFilter_Click" />
                    <asp:Button ID="btnResetFilter" runat="server" Text="Reset" CssClass="btn btn-secondary ms-2" OnClick="btnResetFilter_Click" CausesValidation="false" />
                </div>
            </div>
        </div>
    </div>

<div class="card-sims">
        <div class="card-header-sims"><h5>Programme List</h5></div>
        <div class="card-body-sims grid-scroll">
            <asp:GridView ID="gvProgrammes" runat="server" CssClass="table table-bordered table-hover wide-grid"
                AutoGenerateColumns="False" DataKeyNames="ProgrammeId" OnRowCommand="gvProgrammes_RowCommand">
                <Columns>
                    <asp:BoundField DataField="ProgrammeCode" HeaderText="Code" />
                    <asp:TemplateField HeaderText="Programme Details">
                        <ItemTemplate>
                            <strong><%# Eval("ProgrammeName") %></strong>
                            <div class="small-muted"><%# Eval("Description") %></div>
                            <div class="mt-1">
                                <span class="stat-pill"><%# Eval("DurationYears") %> years</span>
                                <span class="stat-pill">Active: <%# Eval("IsActiveText") %></span>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Courses">
                        <ItemTemplate>
                            <strong><%# Eval("CourseCount") %></strong> course(s)
                            <div class="small-muted"><%# Eval("CoursesOffered") %></div>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Lecturers">
                        <ItemTemplate>
                            <strong><%# Eval("LecturerCount") %></strong> lecturer(s)
                            <div class="small-muted"><%# Eval("TeachingLecturers") %></div>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Students">
                        <ItemTemplate>
                            <span class="stat-pill">Total: <%# Eval("StudentCount") %></span>
                            <span class="stat-pill">Active: <%# Eval("ActiveStudentCount") %></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <div class="action-btns">
                                <asp:LinkButton runat="server" CommandName="EditProgramme" CommandArgument='<%# Eval("ProgrammeId") %>' CssClass="btn btn-sm btn-warning">Edit</asp:LinkButton>
                                <asp:LinkButton runat="server" CommandName="DeleteProgramme" CommandArgument='<%# Eval("ProgrammeId") %>' CssClass="btn btn-sm btn-danger" OnClientClick="return confirm('This will deactivate the programme instead of deleting it. Continue?');">Deactivate</asp:LinkButton>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate>
                    <div class="alert alert-info mb-0">No programmes found.</div>
                </EmptyDataTemplate>
            </asp:GridView>
        </div>
    </div>
</asp:Content>
