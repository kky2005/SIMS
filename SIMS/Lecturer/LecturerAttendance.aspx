<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="LecturerAttendance.aspx.cs"
    Inherits="SIMS.Lecturer.LecturerAttendance"
    MasterPageFile="~/Lecturer/LecturerMaster.master" %>

<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .attendance-header {
            background: linear-gradient(135deg, #0d6efd 0%, #0a58ca 100%);
            color: white;
            padding: 20px;
            border-radius: 8px;
            margin-bottom: 30px;
        }

        .attendance-header h3 {
            margin: 0 0 5px 0;
        }

        .attendance-header p {
            margin: 0;
            opacity: 0.9;
            font-size: 14px;
        }

        .attendance-controls {
            background: white;
            border-radius: 8px;
            border: 1px solid #e2e8f0;
            padding: 20px;
            margin-bottom: 30px;
        }

        .control-row {
            display: grid;
            grid-template-columns: 1fr 1fr 1fr;
            gap: 20px;
            margin-bottom: 15px;
        }

        .control-group label {
            display: block;
            font-weight: bold;
            color: #1e293b;
            margin-bottom: 8px;
            font-size: 14px;
        }

        .control-group input,
        .control-group select {
            width: 100%;
            padding: 10px 12px;
            border: 1px solid #cbd5e1;
            border-radius: 6px;
            font-size: 14px;
        }

        .control-group input:focus,
        .control-group select:focus {
            outline: none;
            border-color: #0d6efd;
            box-shadow: 0 0 0 3px rgba(13, 110, 253, 0.1);
        }

        .action-buttons {
            display: flex;
            gap: 10px;
            justify-content: flex-end;
            margin-top: 15px;
        }

        .action-buttons .btn {
            padding: 10px 20px;
            font-size: 14px;
        }

        .attendance-table-wrapper {
            background: white;
            border-radius: 8px;
            border: 1px solid #e2e8f0;
            overflow: hidden;
            margin-bottom: 30px;
        }

        .table-sims {
            width: 100%;
            border-collapse: collapse;
            margin: 0;
        }

        .table-sims thead {
            background: #f1f5f9;
            border-bottom: 2px solid #cbd5e1;
        }

        .table-sims th {
            padding: 12px 15px;
            text-align: left;
            font-weight: bold;
            color: #1e293b;
            font-size: 14px;
        }

        .table-sims td {
            padding: 12px 15px;
            border-bottom: 1px solid #e2e8f0;
            font-size: 14px;
            color: #475569;
        }

        .table-sims tbody tr:hover {
            background: #f8fafc;
        }

        .student-name {
            font-weight: 500;
            color: #1e293b;
        }

        .attendance-checkbox {
            width: 18px;
            height: 18px;
            cursor: pointer;
        }

        .status-present {
            background: #dcfce7;
            color: #166534;
            padding: 4px 8px;
            border-radius: 4px;
            font-size: 12px;
            font-weight: bold;
        }

        .status-absent {
            background: #fee2e2;
            color: #991b1b;
            padding: 4px 8px;
            border-radius: 4px;
            font-size: 12px;
            font-weight: bold;
        }

        .no-data {
            text-align: center;
            padding: 40px 20px;
            color: #64748b;
        }

        .no-data i {
            font-size: 48px;
            color: #cbd5e1;
            display: block;
            margin-bottom: 15px;
        }

        .stats-row {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 15px;
            margin-bottom: 30px;
        }

        .stat-box {
            background: white;
            border: 1px solid #e2e8f0;
            border-radius: 8px;
            padding: 20px;
            text-align: center;
        }

        .stat-label {
            font-size: 13px;
            color: #64748b;
            margin-bottom: 8px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }

        .stat-value {
            font-size: 28px;
            font-weight: bold;
            color: #1e293b;
        }

        .stat-box.present .stat-value {
            color: #16a34a;
        }

        .stat-box.absent .stat-value {
            color: #dc2626;
        }

        .success-message {
            background: #dcfce7;
            border: 1px solid #86efac;
            color: #166534;
            padding: 12px 15px;
            border-radius: 6px;
            margin-bottom: 20px;
            display: flex;
            align-items: center;
            gap: 10px;
        }

        @media (max-width: 768px) {
            .control-row {
                grid-template-columns: 1fr;
            }

            .action-buttons {
                flex-direction: column;
            }

            .table-sims {
                font-size: 12px;
            }

            .table-sims td,
            .table-sims th {
                padding: 8px 10px;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">

    <!-- Header -->
    <div class="attendance-header">
        <h3>
            <i class="fa fa-calendar-check" style="margin-right: 10px;"></i>Record Attendance
        </h3>
        <p>Course: <strong><asp:Literal ID="litCourseName" runat="server" /></strong></p>
    </div>

    <!-- Success Message -->
    <asp:Panel ID="pnlSuccess" runat="server" Visible="false" CssClass="success-message">
        <i class="fa fa-check-circle"></i>
        <span><asp:Literal ID="litSuccessMsg" runat="server" /></span>
    </asp:Panel>

    <!-- Statistics -->
    <div class="stats-row">
        <div class="stat-box present">
            <div class="stat-label">Students Present</div>
            <div class="stat-value"><asp:Literal ID="litPresentCount" runat="server" Text="0" /></div>
        </div>
        <div class="stat-box absent">
            <div class="stat-label">Students Absent</div>
            <div class="stat-value"><asp:Literal ID="litAbsentCount" runat="server" Text="0" /></div>
        </div>
        <div class="stat-box">
            <div class="stat-label">Total Students</div>
            <div class="stat-value"><asp:Literal ID="litTotalCount" runat="server" Text="0" /></div>
        </div>
    </div>

    <!-- Controls -->
    <div class="attendance-controls">
        <div class="control-row">
            <div class="control-group">
                <label>Attendance Date:</label>
                <asp:TextBox ID="txtAttendanceDate" runat="server" TextMode="Date" />
            </div>
            <div class="control-group">
                <label>Filter by Status:</label>
                <asp:DropDownList ID="ddlStatusFilter" runat="server">
                    <asp:ListItem Value="">All Students</asp:ListItem>
                    <asp:ListItem Value="Present">Present</asp:ListItem>
                    <asp:ListItem Value="Absent">Absent</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="control-group" style="display: flex; flex-direction: column; justify-content: flex-end;">
                <asp:Button ID="btnRefresh" runat="server" Text="Load Attendance" 
                    CssClass="btn btn-outline-primary" OnClick="btnRefresh_Click" />
            </div>
        </div>
        <div class="action-buttons">
            <asp:Button ID="btnMarkAllPresent" runat="server" Text="Mark All Present" 
                CssClass="btn btn-success" OnClick="btnMarkAllPresent_Click" />
            <asp:Button ID="btnMarkAllAbsent" runat="server" Text="Mark All Absent" 
                CssClass="btn btn-danger" OnClick="btnMarkAllAbsent_Click" />
            <asp:Button ID="btnSaveAttendance" runat="server" Text="Save Attendance" 
                CssClass="btn btn-primary" OnClick="btnSaveAttendance_Click" />
        </div>
    </div>

    <!-- Attendance Table -->
    <div class="attendance-table-wrapper">
        <table class="table-sims">
            <thead>
                <tr>
                    <th style="width: 50px;">
                        <input type="checkbox" id="chkSelectAll" />
                    </th>
                    <th>Student No</th>
                    <th>Student Name</th>
                    <th>Email</th>
                    <th>Programme</th>
                    <th style="width: 100px;">Status</th>
                </tr>
            </thead>
            <tbody>
                <asp:Repeater ID="rptAttendance" runat="server" OnItemDataBound="rptAttendance_ItemDataBound">
                    <ItemTemplate>
                        <tr>
                            <td>
                                <input type="checkbox" class="attendance-checkbox" 
                                    name="chkAttendance" value="<%# Eval("EnrolmentId") %>" />
                            </td>
                            <td><%# Eval("StudentNo") %></td>
                            <td class="student-name"><%# Eval("FullName") %></td>
                            <td><%# Eval("Email") %></td>
                            <td><%# Eval("ProgrammeName") %></td>
                            <td>
                                <asp:Label ID="lblStatus" runat="server" 
                                    CssClass='<%# Eval("Status").ToString() == "Present" ? "status-present" : "status-absent" %>'
                                    Text='<%# Eval("Status") %>' />
                            </td>
                        </tr>
                    </ItemTemplate>
                </asp:Repeater>
            </tbody>
        </table>

        <asp:Panel ID="pnlNoData" runat="server" Visible="false" CssClass="no-data">
            <i class="fa fa-inbox"></i>
            <h5 style="color: #1e293b; margin: 0 0 10px 0;">No Students Enrolled</h5>
            <p style="margin: 0;">There are no students enrolled in this course.</p>
        </asp:Panel>
    </div>

    <!-- Hidden field to track course ID -->
    <asp:HiddenField ID="hidCourseId" runat="server" />

    <script type="text/javascript">
        document.addEventListener('DOMContentLoaded', function () {
            var chkSelectAll = document.getElementById('chkSelectAll');
            var chkboxes = document.querySelectorAll('.attendance-checkbox');

            if (chkSelectAll) {
                chkSelectAll.addEventListener('change', function () {
                    chkboxes.forEach(function (checkbox) {
                        checkbox.checked = chkSelectAll.checked;
                    });
                });
            }

            chkboxes.forEach(function (checkbox) {
                checkbox.addEventListener('change', function () {
                    var allChecked = Array.from(chkboxes).every(cb => cb.checked);
                    var anyChecked = Array.from(chkboxes).some(cb => cb.checked);
                    if (chkSelectAll) {
                        chkSelectAll.checked = allChecked;
                        chkSelectAll.indeterminate = anyChecked && !allChecked;
                    }
                });
            });
        });
    </script>

</asp:Content>
