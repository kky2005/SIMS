<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AttendanceRecord.aspx.cs" Inherits="SIMS.Student.AttendanceRecord" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>SIMS - Attendance Record</title>

    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />

    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" rel="stylesheet" />

    <style>
        body {
            background: #f1f5f9;
            font-family: Arial, sans-serif;
            margin: 0;
            padding: 30px;
        }

        .container-sims {
            max-width: 1100px;
            margin: auto;
        }

        .back-link {
            display: inline-block;
            margin-bottom: 16px;
            text-decoration: none;
            color: #1e293b;
            font-weight: bold;
        }

        .card-sims {
            background: #fff;
            border-radius: 12px;
            border: 1px solid #e2e8f0;
            box-shadow: 0 1px 4px rgba(0,0,0,0.06);
            margin-bottom: 24px;
        }

        .card-header-sims {
            padding: 22px 26px;
            border-bottom: 1px solid #e2e8f0;
        }

        .card-header-sims h2 {
            margin: 0;
            color: #1e293b;
            font-weight: bold;
        }

        .card-header-sims p {
            margin: 6px 0 0;
            color: #64748b;
        }

        .card-body-sims {
            padding: 24px;
        }
        .filter-row {
        display: flex;
        align-items: center;
        gap: 12px;
        margin-bottom: 18px;
        }

        .filter-label {
            font-weight: bold;
            color: #1e293b;
            white-space: nowrap;
        }

        .filter-dropdown {
            max-width: 420px;
        }

        .summary-grid {
            display: grid;
            grid-template-columns: repeat(4, 1fr);
            gap: 16px;
        }

        .summary-box {
            background: #f8fafc;
            padding: 16px;
            border-radius: 10px;
            border: 1px solid #e2e8f0;
        }

        .summary-label {
            font-size: 12px;
            color: #64748b;
            margin-bottom: 6px;
        }

        .summary-value {
            font-size: 24px;
            color: #0f172a;
            font-weight: bold;
        }

        .grid {
            width: 100%;
            border-collapse: collapse;
        }

        .grid th {
            background: #1e3a5f;
            color: white;
            padding: 10px;
            font-size: 13px;
            text-align: left;
        }

        .grid td {
            padding: 10px;
            border-bottom: 1px solid #e2e8f0;
            font-size: 13px;
        }

        .message {
            display: block;
            margin-top: 12px;
            font-weight: bold;
        }

        .status-present {
            color: #15803d;
            font-weight: bold;
        }

        .status-late {
            color: #ca8a04;
            font-weight: bold;
        }

        .status-absent {
            color: #dc2626;
            font-weight: bold;
        }

        @media (max-width: 768px) {
            body {
                padding: 18px;
            }

            .summary-grid {
                grid-template-columns: 1fr;
            }

            .grid {
                font-size: 12px;
            }
        }
    </style>
</head>

<body>
    <form id="form1" runat="server">
        <div class="container-sims">

            <a href="Dashboard.aspx" class="back-link">
                <i class="fa fa-arrow-left"></i> Back to Dashboard
            </a>

            <div class="card-sims">
                <div class="card-header-sims">
                    <h2>Attendance Record</h2>
                    <p>View your course attendance records and attendance percentage.</p>
                    <asp:Label ID="lblMessage" runat="server" CssClass="message"></asp:Label>
                </div>

                <div class="card-body-sims">
                    <div class="summary-grid">
                        <div class="summary-box">
                            <div class="summary-label">Total Courses</div>
                            <div class="summary-value">
                                <asp:Label ID="lblTotalCourses" runat="server" Text="0"></asp:Label>
                            </div>
                        </div>

                        <div class="summary-box">
                            <div class="summary-label">Total Classes</div>
                            <div class="summary-value">
                                <asp:Label ID="lblTotalClasses" runat="server" Text="0"></asp:Label>
                            </div>
                        </div>

                        <div class="summary-box">
                            <div class="summary-label">Attended Classes</div>
                            <div class="summary-value">
                                <asp:Label ID="lblAttendedClasses" runat="server" Text="0"></asp:Label>
                            </div>
                        </div>

                        <div class="summary-box">
                            <div class="summary-label">Overall Attendance</div>
                            <div class="summary-value">
                                <asp:Label ID="lblOverallAttendance" runat="server" Text="0.00%"></asp:Label>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div class="card-sims">
                <div class="card-header-sims">
                    <h2 style="font-size:22px;">
                        <i class="fa fa-chart-simple me-2"></i>Course Attendance Summary
                    </h2>
                    <p>Attendance percentage calculated for each enrolled course.</p>
                </div>

                <div class="card-body-sims">
                    <asp:GridView ID="gvAttendanceSummary" runat="server"
                        AutoGenerateColumns="False"
                        CssClass="grid"
                        EmptyDataText="No attendance summary available.">

                        <Columns>
                            <asp:BoundField DataField="CourseCode" HeaderText="Course Code" />
                            <asp:BoundField DataField="CourseName" HeaderText="Course Name" />
                            <asp:BoundField DataField="AcademicYear" HeaderText="Academic Year" />
                            <asp:BoundField DataField="Semester" HeaderText="Semester" />
                            <asp:BoundField DataField="TotalClasses" HeaderText="Total Classes" />
                            <asp:BoundField DataField="PresentCount" HeaderText="Present" />
                            <asp:BoundField DataField="LateCount" HeaderText="Late" />
                            <asp:BoundField DataField="AbsentCount" HeaderText="Absent" />
                            <asp:BoundField DataField="ExcusedCount" HeaderText="Excused" />
                            <asp:BoundField DataField="AttendancePercentage" HeaderText="Attendance %" DataFormatString="{0:0.00}%" />
                        </Columns>
                    </asp:GridView>
                </div>
            </div>

            <div class="card-sims">
                <div class="card-header-sims">
                    <h2 style="font-size:22px;">
                        <i class="fa fa-calendar-check me-2"></i>Detailed Attendance Records
                    </h2>
                    <p>Class-by-class attendance records recorded by lecturers.</p>
                </div>

                
                <div class="card-body-sims">
                    <div class="filter-row">
                        <label class="filter-label">Filter by Course:</label>

                        <asp:DropDownList ID="ddlCourseFilter" runat="server"
                            CssClass="form-select filter-dropdown"
                            AutoPostBack="true"
                            OnSelectedIndexChanged="ddlCourseFilter_SelectedIndexChanged">
                        </asp:DropDownList>
                    </div>
                    <asp:GridView ID="gvAttendanceDetails" runat="server"
                        AutoGenerateColumns="False"
                        CssClass="grid"
                        EmptyDataText="No detailed attendance records found."
                        OnRowDataBound="gvAttendanceDetails_RowDataBound">

                        <Columns>
                            <asp:BoundField DataField="CourseCode" HeaderText="Course Code" />
                            <asp:BoundField DataField="CourseName" HeaderText="Course Name" />
                            <asp:BoundField DataField="AcademicYear" HeaderText="Academic Year" />
                            <asp:BoundField DataField="Semester" HeaderText="Semester" />
                            <asp:BoundField DataField="AttendanceDate" HeaderText="Date" DataFormatString="{0:dd MMM yyyy}" />

                            <asp:TemplateField HeaderText="Status">
                                <ItemTemplate>
                                    <asp:Label ID="lblStatus" runat="server" Text='<%# Eval("Status") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:BoundField DataField="Remarks" HeaderText="Remarks" />
                            <asp:BoundField DataField="RecordedBy" HeaderText="Recorded By" />
                            <asp:BoundField DataField="RecordedAt" HeaderText="Recorded At" DataFormatString="{0:dd MMM yyyy hh:mm tt}" />
                        </Columns>
                    </asp:GridView>
                </div>
            </div>

        </div>
    </form>
</body>
</html>