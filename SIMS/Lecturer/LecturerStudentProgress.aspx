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

        .progress-header h3 {
            margin: 0 0 5px 0;
        }

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

        .form-group input,
        .form-group select {
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

        .student-name {
            font-weight: bold;
            color: #1e293b;
            font-size: 16px;
        }

        .student-no {
            font-size: 13px;
            color: #64748b;
        }

        .risk-badge {
            display: inline-block;
            padding: 4px 12px;
            border-radius: 12px;
            font-size: 12px;
            font-weight: bold;
        }

        .risk-High {
            background: #fee2e2;
            color: #991b1b;
        }

        .risk-Medium {
            background: #fef3c7;
            color: #92400e;
        }

        .risk-Low {
            background: #dcfce7;
            color: #166534;
        }

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

        .metric-label {
            font-size: 13px;
            color: #64748b;
        }

        .metric-value {
            font-weight: bold;
            color: #1e293b;
            font-size: 18px;
        }

        .no-data {
            text-align: center;
            padding: 40px 20px;
            background: white;
            border-radius: 8px;
            border: 1px solid #e2e8f0;
            color: #64748b;
        }

        @media (max-width: 768px) {
            .progress-metric { grid-template-columns: 1fr; }
            .filter-section div { grid-template-columns: 1fr !important; }
        }
    </style>
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">

    <!-- Header -->
    <div class="progress-header">
        <h3>
            <i class="fa fa-chart-line" style="margin-right: 10px;"></i>Student Progress Report
        </h3>
        <p style="margin: 0;">Monitor student performance and identify at-risk students</p>
    </div>

    <!-- Filters -->
    <div class="filter-section">
        <h5 style="margin: 0 0 15px 0; color: #1e293b; font-weight: bold;">Filters</h5>
        <div style="display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 20px;">
            <div class="form-group">
                <label>Course:</label>
                <asp:DropDownList ID="ddlCourse" runat="server" />
            </div>
            <div class="form-group">
                <label>Risk Level:</label>
                <asp:DropDownList ID="ddlRiskLevel" runat="server">
                    <asp:ListItem Value="">All Students</asp:ListItem>
                    <asp:ListItem Value="High">High Risk</asp:ListItem>
                    <asp:ListItem Value="Medium">Medium Risk</asp:ListItem>
                    <asp:ListItem Value="Low">Low Risk</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="form-group" style="display: flex; flex-direction: column; justify-content: flex-end;">
                <asp:Button ID="btnApplyFilter" runat="server" Text="Apply Filter" CssClass="btn btn-primary" OnClick="btnApplyFilter_Click" />
            </div>
        </div>
    </div>

    <!-- Student Progress Cards -->
    <asp:Repeater ID="rptStudentProgress" runat="server">
        <ItemTemplate>
            <div class="student-card">
                <div class="student-header">
                    <div>
                        <div class="student-name"><%# Eval("FullName") %></div>
                        <div class="student-no">Student No: <%# Eval("StudentNo") %></div>
                    </div>
                    <span class="risk-badge risk-<%# Eval("RiskLevel") %>">
                        <%# Eval("RiskLevel") %> Risk
                    </span>
                </div>
                <div class="progress-metric">
                    <div class="metric-item">
                        <span class="metric-label">Attendance</span>
                        <span class="metric-value"><%# Eval("AttendancePercent") %>%</span>
                    </div>
                    <div class="metric-item">
                        <span class="metric-label">Current GPA</span>
                        <span class="metric-value"><%# Eval("CurrentGPA") %></span>
                    </div>
                    <div class="metric-item">
                        <span class="metric-label">Assessments</span>
                        <span class="metric-value"><%# Eval("AssignmentStatus") %></span>
                    </div>
                </div>
            </div>
        </ItemTemplate>
    </asp:Repeater>

    <!-- No Data -->
    <asp:Panel ID="pnlNoData" runat="server" Visible="false" CssClass="no-data">
        <i class="fa fa-inbox" style="font-size: 48px; color: #cbd5e1; display: block; margin-bottom: 15px;"></i>
        <h5 style="color: #1e293b; margin: 0 0 10px 0;">No Students Found</h5>
        <p style="margin: 0;">No student data available for the selected filters.</p>
    </asp:Panel>

</asp:Content>