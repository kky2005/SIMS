<%@ Page Title="Manage Courses" Language="C#" MasterPageFile="~/HeadOfProgramme/HOPMaster.master"
    AutoEventWireup="true" CodeBehind="HOPManageCourses.aspx.cs"
    Inherits="SIMS.HeadOfProgramme.HOPManageCourses" %>

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
    <h2 class="page-title">Manage Courses</h2>
    <p class="page-subtitle">Add, edit, delete and manage course information.</p>
    <asp:Label ID="lblMessage" runat="server" CssClass="message-box d-block"></asp:Label>

    <div class="card-sims mb-4">
        <div class="card-header-sims"><h5>Course Form</h5></div>
        <div class="card-body-sims">
            <asp:HiddenField ID="hfCourseId" runat="server" />
            <div class="row g-3">
                <div class="col-md-4">
                    <label class="form-label">Programme</label>
                    <asp:DropDownList ID="ddlProgramme" runat="server" CssClass="form-select"></asp:DropDownList>
                </div>
                <div class="col-md-2">
                    <label class="form-label">Course Code</label>
                    <asp:TextBox ID="txtCourseCode" runat="server" CssClass="form-control" placeholder="e.g. CS101"></asp:TextBox>
                </div>
                <div class="col-md-4">
                    <label class="form-label">Course Name</label>
                    <asp:TextBox ID="txtCourseName" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="col-md-1">
                    <label class="form-label">Credits</label>
                    <asp:TextBox ID="txtCreditHours" runat="server" CssClass="form-control" TextMode="Number" Text="3"></asp:TextBox>
                </div>
                <div class="col-md-1">
                    <label class="form-label">Sem</label>
                    <asp:TextBox ID="txtSemester" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox>
                </div>
                <div class="col-md-2">
                    <label class="form-label">Active</label>
                    <asp:DropDownList ID="ddlIsActive" runat="server" CssClass="form-select">
                        <asp:ListItem Value="1">Yes</asp:ListItem>
                        <asp:ListItem Value="0">No</asp:ListItem>
                    </asp:DropDownList>
                </div>
            </div>
            <div class="mt-3">
                <asp:Button ID="btnSave" runat="server" Text="Save Course" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-secondary" OnClick="btnClear_Click" CausesValidation="false" />
            </div>
        </div>
    </div>

    <div class="card-sims">
        <div class="card-header-sims"><h5>Course List</h5></div>
        <div class="card-body-sims">
            <asp:GridView ID="gvCourses" runat="server" CssClass="table table-bordered table-hover"
                AutoGenerateColumns="False" DataKeyNames="CourseId" OnRowCommand="gvCourses_RowCommand">
                <Columns>
                    <asp:BoundField DataField="CourseCode" HeaderText="Code" />
                    <asp:BoundField DataField="CourseName" HeaderText="Course Name" />
                    <asp:BoundField DataField="ProgrammeName" HeaderText="Programme" />
                    <asp:BoundField DataField="CreditHours" HeaderText="Credits" />
                    <asp:BoundField DataField="Semester" HeaderText="Semester" />
                    <asp:BoundField DataField="IsActiveText" HeaderText="Active" />
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <div class="action-btns">
                                <asp:LinkButton runat="server" CommandName="EditCourse" CommandArgument='<%# Eval("CourseId") %>' CssClass="btn btn-sm btn-warning">Edit</asp:LinkButton>
                                <asp:LinkButton runat="server" CommandName="DeleteCourse" CommandArgument='<%# Eval("CourseId") %>' CssClass="btn btn-sm btn-danger" OnClientClick="return confirm('Delete this course?');">Delete</asp:LinkButton>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </div>
</asp:Content>
