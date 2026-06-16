<%@ Page Title="Manage Academic Calendar" Language="C#" MasterPageFile="~/HeadOfProgramme/HOPMaster.Master" AutoEventWireup="true" CodeBehind="HOPManageAcademicCalendar.aspx.cs" Inherits="SIMS.HeadOfProgramme.HOPManageAcademicCalendar" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .form-card {
            background: #fff;
            padding: 24px;
            border-radius: 12px;
            border: 1px solid #e2e8f0;
            box-shadow: 0 1px 4px rgba(0,0,0,0.06);
            margin-bottom: 24px;
        }

        .form-card label {
            font-weight: 600;
            color: #334155;
            margin-top: 12px;
            margin-bottom: 6px;
        }

        .page-title {
            font-size: 24px;
            font-weight: 700;
            color: #1e293b;
            margin-bottom: 20px;
        }

        .btn-save {
            background: #0d6efd;
            color: #fff;
            border: none;
            padding: 9px 18px;
            border-radius: 8px;
            margin-right: 8px;
        }

        .btn-clear {
            background: #64748b;
            color: #fff;
            border: none;
            padding: 9px 18px;
            border-radius: 8px;
        }

        .btn-edit {
            background: #f59e0b;
            color: #fff;
            border: none;
            padding: 6px 12px;
            border-radius: 6px;
            margin-right: 5px;
        }

        .btn-delete {
            background: #dc2626;
            color: #fff;
            border: none;
            padding: 6px 12px;
            border-radius: 6px;
        }

        .message {
            display: block;
            margin-bottom: 12px;
            font-weight: 600;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2 class="page-title">Manage Academic Calendar</h2>

    <asp:Label ID="lblMessage" runat="server" CssClass="message"></asp:Label>
    <asp:HiddenField ID="hfCalendarId" runat="server" />

    <div class="form-card">
        <div class="row">
            <div class="col-md-6">
                <label>Event Name</label>
                <asp:TextBox ID="txtEventName" runat="server" CssClass="form-control" placeholder="Example: Semester 1 Final Exam" />
            </div>

            <div class="col-md-6">
                <label>Event Type</label>
                <asp:DropDownList ID="ddlEventType" runat="server" CssClass="form-control">
                    <asp:ListItem Text="Semester Start" Value="Semester Start" />
                    <asp:ListItem Text="Semester End" Value="Semester End" />
                    <asp:ListItem Text="Exam" Value="Exam" />
                    <asp:ListItem Text="Holiday" Value="Holiday" />
                    <asp:ListItem Text="Registration" Value="Registration" />
                    <asp:ListItem Text="Other" Value="Other" />
                </asp:DropDownList>
            </div>
        </div>

        <div class="row">
            <div class="col-md-6">
                <label>Start Date</label>
                <asp:TextBox ID="txtStartDate" runat="server" CssClass="form-control" TextMode="Date" />
            </div>

            <div class="col-md-6">
                <label>End Date</label>
                <asp:TextBox ID="txtEndDate" runat="server" CssClass="form-control" TextMode="Date" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-6">
                <label>Academic Year</label>
                <asp:TextBox ID="txtAcademicYear" runat="server" CssClass="form-control" placeholder="2026" />
            </div>

            <div class="col-md-6">
                <label>Semester</label>
                <asp:TextBox ID="txtSemester" runat="server" CssClass="form-control" placeholder="1 / 2 / 3" />
            </div>
        </div>

        <label>Description</label>
        <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" placeholder="Optional description" />

        <br />
        <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="btn-save" OnClick="btnSave_Click" />
        <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn-clear" OnClick="btnClear_Click" />
    </div>

        <div class="card-sims mb-4">
        <div class="card-header-sims"><h5>Filter Calendar Events</h5></div>
        <div class="card-body-sims">
            <div class="row g-3 align-items-end">
                <div class="col-md-4"><label class="form-label">Search Event</label><asp:TextBox ID="txtFilterEvent" runat="server" CssClass="form-control" placeholder="Search event name or description"></asp:TextBox></div>
                <div class="col-md-3"><label class="form-label">Event Type</label><asp:DropDownList ID="ddlFilterEventType" runat="server" CssClass="form-select"><asp:ListItem Value="">All</asp:ListItem><asp:ListItem>Semester Start</asp:ListItem><asp:ListItem>Semester End</asp:ListItem><asp:ListItem>Exam</asp:ListItem><asp:ListItem>Holiday</asp:ListItem><asp:ListItem>Event</asp:ListItem></asp:DropDownList></div>
                <div class="col-md-2"><label class="form-label">Academic Year</label><asp:TextBox ID="txtFilterAcademicYear" runat="server" CssClass="form-control" placeholder="2026"></asp:TextBox></div>
                <div class="col-md-3"><asp:Button ID="btnFilter" runat="server" Text="Filter" CssClass="btn btn-primary" OnClick="btnFilter_Click" /><asp:Button ID="btnResetFilter" runat="server" Text="Reset" CssClass="btn btn-secondary ms-2" OnClick="btnResetFilter_Click" CausesValidation="false" /></div>
            </div>
        </div>
    </div>

<div class="card-sims">
        <div class="card-header-sims">
            <h5>Academic Calendar List</h5>
        </div>

        <div class="card-body-sims">
            <asp:GridView ID="gvCalendar" runat="server"
                AutoGenerateColumns="False"
                CssClass="table table-bordered table-hover"
                DataKeyNames="CalendarId"
                OnRowCommand="gvCalendar_RowCommand">

                <Columns>
                    <asp:BoundField DataField="CalendarId" HeaderText="ID" />
                    <asp:BoundField DataField="EventName" HeaderText="Event Name" />
                    <asp:BoundField DataField="EventType" HeaderText="Type" />
                    <asp:BoundField DataField="StartDate" HeaderText="Start Date" DataFormatString="{0:yyyy-MM-dd}" />
                    <asp:BoundField DataField="EndDate" HeaderText="End Date" DataFormatString="{0:yyyy-MM-dd}" />
                    <asp:BoundField DataField="AcademicYear" HeaderText="Academic Year" />
                    <asp:BoundField DataField="Semester" HeaderText="Semester" />

                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <asp:Button ID="btnEdit" runat="server" Text="Edit"
                                CommandName="EditRow"
                                CommandArgument='<%# Eval("CalendarId") %>'
                                CssClass="btn-edit" />

                            <asp:Button ID="btnDelete" runat="server" Text="Delete"
                                CommandName="DeleteRow"
                                CommandArgument='<%# Eval("CalendarId") %>'
                                CssClass="btn-delete"
                                OnClientClick="return confirm('Are you sure you want to delete this academic calendar event?');" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </div>

</asp:Content>
