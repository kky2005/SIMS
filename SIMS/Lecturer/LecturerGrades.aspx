<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="LecturerGrades.aspx.cs"
    Inherits="SIMS.Lecturer.LecturerGrades"
    MasterPageFile="~/Lecturer/LecturerMaster.master" %>

<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .grades-header {
            background: linear-gradient(135deg, #059669 0%, #047857 100%);
            color: white;
            padding: 20px;
            border-radius: 8px;
            margin-bottom: 30px;
        }

        .grades-header h3 {
            margin: 0 0 5px 0;
        }

        .grades-header p {
            margin: 0;
            opacity: 0.9;
            font-size: 14px;
        }

        .grades-controls {
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
            border-color: #059669;
            box-shadow: 0 0 0 3px rgba(5, 150, 105, 0.1);
        }

        .action-buttons {
            display: flex;
            gap: 10px;
            justify-content: flex-end;
        }

        .action-buttons .btn {
            padding: 10px 20px;
            font-size: 14px;
        }

        .tabs {
            display: flex;
            gap: 0;
            margin-bottom: 20px;
            border-bottom: 2px solid #e2e8f0;
        }

        .tab-btn {
            padding: 12px 20px;
            background: none;
            border: none;
            border-bottom: 3px solid transparent;
            cursor: pointer;
            font-weight: 500;
            color: #64748b;
            transition: all 0.3s ease;
        }

        .tab-btn.active {
            color: #059669;
            border-bottom-color: #059669;
        }

        .tab-content {
            display: none;
        }

        .tab-content.active {
            display: block;
        }

        .grades-table-wrapper {
            background: white;
            border-radius: 8px;
            border: 1px solid #e2e8f0;
            overflow: hidden;
            margin-bottom: 30px;
        }

        .table-sims {
            width: 100%;
            border-collapse: collapse;
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

        .mark-input {
            width: 80px;
            padding: 6px 8px;
            border: 1px solid #cbd5e1;
            border-radius: 4px;
            font-size: 14px;
            text-align: center;
        }

        .mark-input:focus {
            outline: none;
            border-color: #059669;
            box-shadow: 0 0 0 2px rgba(5, 150, 105, 0.1);
        }

        .grade-badge {
            display: inline-block;
            padding: 4px 12px;
            border-radius: 12px;
            font-weight: bold;
            font-size: 12px;
        }

        .grade-a {
            background: #dcfce7;
            color: #166534;
        }

        .grade-b {
            background: #dbeafe;
            color: #1e40af;
        }

        .grade-c {
            background: #fef3c7;
            color: #92400e;
        }

        .grade-d {
            background: #fee2e2;
            color: #991b1b;
        }

        .grade-f {
            background: #dc2626;
            color: white;
        }

        .status-published {
            background: #dcfce7;
            color: #166534;
            padding: 4px 8px;
            border-radius: 4px;
            font-size: 12px;
            font-weight: bold;
        }

        .status-unpublished {
            background: #fef3c7;
            color: #92400e;
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

        .assessment-card {
            background: white;
            border: 1px solid #e2e8f0;
            border-radius: 8px;
            padding: 20px;
            margin-bottom: 20px;
        }

        .assessment-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 15px;
        }

        .assessment-title {
            font-weight: bold;
            color: #1e293b;
            font-size: 16px;
        }

        .assessment-meta {
            display: flex;
            gap: 20px;
            font-size: 13px;
            color: #64748b;
            margin-bottom: 15px;
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

        .error-message {
            background: #fee2e2;
            border: 1px solid #fca5a5;
            color: #991b1b;
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

            .tabs {
                flex-wrap: wrap;
            }

            .tab-btn {
                font-size: 13px;
                padding: 10px 15px;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">

    <!-- Header -->
    <div class="grades-header">
        <h3>
            <i class="fa fa-star" style="margin-right: 10px;"></i>Manage Grades & Assessment
        </h3>
        <p>Course: <strong><asp:Literal ID="litCourseName" runat="server" /></strong></p>
    </div>

    <!-- Success Message -->
    <asp:Panel ID="pnlSuccess" runat="server" Visible="false" CssClass="success-message">
        <i class="fa fa-check-circle"></i>
        <span><asp:Literal ID="litSuccessMsg" runat="server" /></span>
    </asp:Panel>

    <!-- Error Message -->
    <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="error-message">
        <i class="fa fa-exclamation-circle"></i>
        <span><asp:Literal ID="litErrorMsg" runat="server" /></span>
    </asp:Panel>

    <!-- Controls -->
    <div class="grades-controls">
        <div class="control-row">
            <div class="control-group">
                <label>Academic Year:</label>
                <asp:DropDownList ID="ddlAcademicYear" runat="server">
                    <asp:ListItem Text="2023" Value="2023" />
                    <asp:ListItem Text="2024" Value="2024" Selected="True" />
                    <asp:ListItem Text="2025" Value="2025" />
                    <asp:ListItem Text="2026" Value="2026" />
                </asp:DropDownList>
            </div>
            <div class="control-group">
                <label>Semester:</label>
                <asp:DropDownList ID="ddlSemester" runat="server">
                    <asp:ListItem Text="Semester 1" Value="1" />
                    <asp:ListItem Text="Semester 2" Value="2" />
                    <asp:ListItem Text="Semester 3" Value="3" />
                </asp:DropDownList>
            </div>
            <div class="control-group" style="display: flex; flex-direction: column; justify-content: flex-end;">
                <asp:Button ID="btnLoadAssessments" runat="server" Text="Load Assessments" 
                    CssClass="btn btn-outline-success" OnClick="btnLoadAssessments_Click" />
            </div>
        </div>
    </div>

    <!-- Tabs for Assessments -->
    <div class="tabs">
        <asp:LinkButton ID="btnTabEnterGrades" runat="server" CssClass="tab-btn active" 
            OnClick="SwitchTab" CommandArgument="enterGrades">
            <i class="fa fa-pencil"></i> Enter Grades
        </asp:LinkButton>
        <asp:LinkButton ID="btnTabPublish" runat="server" CssClass="tab-btn" 
            OnClick="SwitchTab" CommandArgument="publishGrades">
            <i class="fa fa-check"></i> Publish Grades
        </asp:LinkButton>
    </div>

    <!-- Tab 1: Enter Grades -->
    <div id="tabEnterGrades" class="tab-content active">
        <asp:Panel ID="pnlEnterGrades" runat="server">
            <asp:Repeater ID="rptAssessments" runat="server" OnItemDataBound="rptAssessments_ItemDataBound">
                <ItemTemplate>
                    <div class="assessment-card">
                        <div class="assessment-header">
                            <div class="assessment-title"><%# Eval("AssessmentName") %></div>
                            <span class="status-unpublished">
                                <asp:Literal ID="litPublishStatus" runat="server" 
                                    Text='<%# Convert.ToBoolean(Eval("IsPublished")) ? "Published" : "Unpublished" %>' />
                            </span>
                        </div>
                        <div class="assessment-meta">
                            <span><strong>Max Mark:</strong> <%# Eval("MaxMark") %></span>
                            <span><strong>Weightage:</strong> <%# Eval("Weightage") %>%</span>
                        </div>
                        <div class="grades-table-wrapper">
                            <table class="table-sims">
                                <thead>
                                    <tr>
                                        <th>Student No</th>
                                        <th>Student Name</th>
                                        <th>Email</th>
                                        <th style="width: 100px;">Mark Obtained</th>
                                        <th style="width: 100px;">Grade</th>
                                        <th style="width: 120px;">Action</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <asp:Repeater ID="rptStudentMarks" runat="server" OnItemDataBound="rptStudentMarks_ItemDataBound">
                                        <ItemTemplate>
                                            <tr>
                                                <td><%# Eval("StudentNo") %></td>
                                                <td class="student-name"><%# Eval("FullName") %></td>
                                                <td><%# Eval("Email") %></td>
                                                <td>
                                                    <input type="number" class="mark-input" 
                                                        name="txtMark_<%# Eval("StudentId") %>" 
                                                        value="<%# Eval("MarksObtained") %>"
                                                        min="0" max="<%# Eval("MaxMark") %>"
                                                        step="0.01" />
                                                </td>
                                                <td>
                                                    <span class='grade-badge grade-<%# GetGradeLetter(Eval("MarksObtained").ToString()) %>'>
                                                        <%# GetGradeLetter(Eval("MarksObtained").ToString()) %>
                                                    </span>
                                                </td>
                                                <td>
                                                    <asp:Button ID="btnSaveMark" runat="server" 
                                                        Text="Save"
                                                        CssClass="btn btn-sm btn-success"
                                                        OnClick="btnSaveMark_Click"
                                                        CommandArgument='<%# Eval("MarkId") %>' />
                                                </td>
                                            </tr>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </tbody>
                            </table>
                        </div>
                        <div style="text-align: right; margin-top: 15px;">
                            <asp:Button ID="btnSaveAllMarks" runat="server" 
                                Text="Save All Marks"
                                CssClass="btn btn-success"
                                OnClick="btnSaveAllMarks_Click"
                                CommandArgument='<%# Eval("AssessmentId") %>' />
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>

            <asp:Panel ID="pnlNoAssessments" runat="server" Visible="false" CssClass="no-data">
                <i class="fa fa-inbox"></i>
                <h5 style="color: #1e293b; margin: 0 0 10px 0;">No Assessments Found</h5>
                <p style="margin: 0;">There are no assessments created for this course in the selected period.</p>
            </asp:Panel>
        </asp:Panel>
    </div>

    <!-- Tab 2: Publish Grades -->
    <div id="tabPublishGrades" class="tab-content">
        <asp:Panel ID="pnlPublishGrades" runat="server">
            <div class="grades-table-wrapper">
                <table class="table-sims">
                    <thead>
                        <tr>
                            <th>Assessment</th>
                            <th>Max Mark</th>
                            <th>Weightage</th>
                            <th style="width: 120px;">Status</th>
                            <th style="width: 150px;">Action</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptPublishAssessments" runat="server">
                            <ItemTemplate>
                                <tr>
                                    <td><strong><%# Eval("AssessmentName") %></strong></td>
                                    <td><%# Eval("MaxMark") %></td>
                                    <td><%# Eval("Weightage") %>%</td>
                                    <td>
                                        <span class='<%# Convert.ToBoolean(Eval("IsPublished")) ? "status-published" : "status-unpublished" %>'>
                                            <%# Convert.ToBoolean(Eval("IsPublished")) ? "Published" : "Unpublished" %>
                                        </span>
                                    </td>
                                    <td>
                                        <asp:Button ID="btnTogglePublish" runat="server" 
                                            Text='<%# Convert.ToBoolean(Eval("IsPublished")) ? "Unpublish" : "Publish" %>'
                                            CssClass='<%# Convert.ToBoolean(Eval("IsPublished")) ? "btn btn-sm btn-warning" : "btn btn-sm btn-success" %>'
                                            OnClick="btnTogglePublish_Click"
                                            CommandArgument='<%# Eval("AssessmentId") %>' />
                                    </asp:Button>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>

            <asp:Panel ID="pnlNoPublishAssessments" runat="server" Visible="false" CssClass="no-data">
                <i class="fa fa-inbox"></i>
                <h5 style="color: #1e293b; margin: 0 0 10px 0;">No Assessments Found</h5>
                <p style="margin: 0;">There are no assessments to publish for this course.</p>
            </asp:Panel>
        </asp:Panel>
    </div>

    <!-- Hidden field to track course ID -->
    <asp:HiddenField ID="hidCourseId" runat="server" />

    <script type="text/javascript">
        function switchTab(tabName) {
            // Hide all tabs
            document.getElementById('tabEnterGrades').style.display = 'none';
            document.getElementById('tabPublishGrades').style.display = 'none';

            // Remove active class from all buttons
            document.getElementById('<%= btnTabEnterGrades.ClientID %>').classList.remove('active');
            document.getElementById('<%= btnTabPublish.ClientID %>').classList.remove('active');

            // Show selected tab
            if (tabName === 'enterGrades') {
                document.getElementById('tabEnterGrades').style.display = 'block';
                document.getElementById('<%= btnTabEnterGrades.ClientID %>').classList.add('active');
            } else {
                document.getElementById('tabPublishGrades').style.display = 'block';
                document.getElementById('<%= btnTabPublish.ClientID %>').classList.add('active');
            }
        }
    </script>

</asp:Content>
