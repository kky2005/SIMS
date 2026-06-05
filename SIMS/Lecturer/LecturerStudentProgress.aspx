<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="LecturerStudentProgress.aspx.cs"
    Inherits="SIMS.Lecturer.LecturerStudentProgress"
    MasterPageFile="~/Lecturer/LecturerMaster.master" %>

<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .progress-header {
            background: linear-gradient(135deg, #dc2626 0%, #b91c1c 100%);
            color: white;
            padding: 20px;
            border-radius: 8px;
            margin-bottom: 30px;
        }
        .progress-header h3 { margin: 0 0 5px 0; }
        .filter-section {
            background: white;
            border-radius: 8px;
            border: 1px solid #e2e8f0;
            padding: 20px;
            margin-bottom: 30px;
        }
        .form-group {
            display: flex;
            flex-direction: column;
            margin-bottom: 15px;
        }
        .form-group label {
            font-weight: bold;
            color: #1e293b;
            margin-bottom: 8px;
        }
        .form-group input, .form-group select {
            padding: 10px 12px;
            border: 1px solid #cbd5e1;
            border-radius: 6px;
            font-size: 14px;
        }
        .student-card {
            background: white;
            border: 1px solid #e2e8f0;
            border-radius: 8px;
            padding: 20px;
            margin-bottom: 20px;
        }
        .student-header {
            display: flex;
            justify-content: space-between;
            align-items: start;
            margin-bottom: 15px;
            padding-bottom: 15px;
            border-bottom: 1px solid #e2e8f0;
        }
        .student-name { font-weight: bold; color: #1e293b; font-size: 16px; }
        .student-no { font-size: 13px; color: #64748b; }
        .risk-badge {
            display: inline-block;
            padding: 4px 12px;
            border-radius: 12px;
            font-size: 12px;
            font-weight: bold;
        }
        .risk-High { background: #fee2e2; color: #991b1b; }
        .risk-Medium { background: #fef3c7; color: #92400e; }
        .risk-Low { background: #dcfce7; color: #166534; }
        .progress-metric {
            display: grid;
            grid-template-columns: 1fr 1fr 1fr;
            gap: 15px;
            margin-bottom: 15px;
        }
        .metric-item {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 10px;
            background: #f8fafc;
            border-radius: 6px;
        }
        .metric-label { font-size: 13px; color: #64748b; }
        .metric-value { font-weight: bold; color: #1e293b; font-size: 18px; }
        .card-actions {
            display: flex;
            justify-content: flex-end;
            gap: 10px;
            border-top: 1px dashed #e2e8f0;
            padding-top: 12px;
        }
        .no-data {
            text-align: center;
            padding: 40px 20px;
            background: white;
            border-radius: 8px;
            border: 1px solid #e2e8f0;
            color: #64748b;
        }
        .course-badge {
            display: inline-block;
            background: #f1f5f9;
            color: #475569;
            padding: 2px 8px;
            border-radius: 4px;
            font-size: 12px;
            font-weight: 600;
            margin-left: 10px;
            border: 1px solid #cbd5e1;
            vertical-align: middle;
        }
        @media (max-width: 768px) {
            .progress-metric { grid-template-columns: 1fr; }
            .filter-section div { grid-template-columns: 1fr !important; }
        }
    </style>
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">

    <div class="progress-header">
        <h3><i class="fa fa-chart-line" style="margin-right: 10px;"></i>Student Progress Tracker</h3>
        <p style="margin: 0;">Monitor structural performance flags, issue warnings, and export academic auditing reports.</p>
    </div>

    <asp:Label ID="lblStatusMessage" runat="server" CssClass="alert" Visible="false" style="display:block; padding:12px; margin-bottom:20px; border-radius:6px; font-weight:500;"></asp:Label>

    <div class="filter-section">
        <h5 style="margin: 0 0 15px 0; color: #1e293b; font-weight: bold;">Filter Criteria</h5>
        <div style="display: grid; grid-template-columns: 2fr 1.5fr 1fr 1fr; gap: 15px; align-items: flex-end;">
            <div class="form-group" style="margin-bottom:0;">
                <label>Course Assigned:</label>
                <asp:DropDownList ID="ddlCourse" runat="server" />
            </div>
            <div class="form-group" style="margin-bottom:0;">
                <label>Calculated Risk Profile:</label>
                <asp:DropDownList ID="ddlRiskLevel" runat="server">
                    <asp:ListItem Value="">All Risk Classes</asp:ListItem>
                    <asp:ListItem Value="High">High Risk Focus</asp:ListItem>
                    <asp:ListItem Value="Medium">Medium Risk Focus</asp:ListItem>
                    <asp:ListItem Value="Low">Low Risk/Satisfactory</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="form-group" style="margin-bottom:0;">
                <asp:Button ID="btnApplyFilter" runat="server" Text="Apply Filter" CssClass="btn btn-primary" style="width:100%;" OnClick="btnApplyFilter_Click" />
            </div>
            <div class="form-group" style="margin-bottom:0;">
                <asp:Button ID="btnExportReport" runat="server" Text="Export Report" CssClass="btn btn-success" style="width:100%; background-color:#16a34a; color:white; border:none;" OnClick="btnExportReport_Click" />
            </div>
        </div>
    </div>

    <asp:Repeater ID="rptStudentProgress" runat="server" OnItemCommand="rptStudentProgress_ItemCommand">
        <ItemTemplate>
            <div class="student-card">
                <div class="student-header">
                    <div>
                        <div class="student-name">
                            <%# Eval("FullName") %>
                            <!-- Course Code Badge Component Added Here -->
                            <span class="course-badge"><%# Eval("CourseCode") %></span>
                        </div>
                        <div class="student-no">ID Reference: <%# Eval("StudentNo") %> | Email: <%# Eval("Email") %></div>
                    </div>
                    <span class="risk-badge risk-<%# Eval("RiskLevel") %>">
                        <%# Eval("RiskLevel") %> Risk
                    </span>
                </div>
                <div class="progress-metric">
                    <div class="metric-item">
                        <span class="metric-label">Calculated Attendance</span>
                        <span class="metric-value"><%# Eval("AttendancePercent", "{0:F1}") %>%</span>
                    </div>
                    <div class="metric-item">
                        <span class="metric-label">Latest Semester CGPA</span>
                        <span class="metric-value"><%# Eval("CurrentGPA", "{0:F2}") %></span>
                    </div>
                    <div class="metric-item">
                        <span class="metric-label">Completed Assessments</span>
                        <span class="metric-value"><%# Eval("AssignmentStatus") %> Items</span>
                    </div>
                </div>
                <div class="card-actions">
                    <asp:LinkButton ID="lnkIssueWarning" runat="server" 
                        CommandName="IssueWarning" 
                        CommandArgument='<%# Eval("StudentId") %>'
                        CssClass="btn btn-sm btn-outline-danger" 
                        style="color:#dc2626; font-size:13px; text-decoration:none;"
                        OnClientClick="return confirm('Are you sure you want to log an Academic Warning flag for this student?');">
                        <i class="fa fa-exclamation-triangle"></i> Flag Academic Warning
                    </asp:LinkButton>
                </div>
            </div>
        </ItemTemplate>
    </asp:Repeater>

    <asp:Panel ID="pnlNoData" runat="server" Visible="false" CssClass="no-data">
        <i class="fa fa-inbox" style="font-size: 48px; color: #cbd5e1; display: block; margin-bottom: 15px;"></i>
        <h5 style="color: #1e293b; margin: 0 0 10px 0;">No Match Found</h5>
        <p style="margin: 0;">No active student enrollments found matching the structured filter properties.</p>
    </asp:Panel>

</asp:Content>