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
        .grades-header h3 { margin: 0 0 5px 0; }
        .grades-header p { margin: 0; opacity: 0.9; font-size: 14px; }

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

        .control-group input, .control-group select {
            width: 100%;
            padding: 10px 12px;
            border: 1px solid #cbd5e1;
            border-radius: 6px;
            font-size: 14px;
        }

        .control-group input:focus, .control-group select:focus {
            outline: none;
            border-color: #059669;
            box-shadow: 0 0 0 3px rgba(5, 150, 105, 0.1);
        }

        .action-buttons {
            display: flex;
            gap: 10px;
            justify-content: flex-end;
            flex-wrap: wrap;
        }

        .action-buttons .btn {
            padding: 10px 20px;
            font-size: 14px;
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
            flex-wrap: wrap;
            gap: 10px;
        }

        .assessment-title { 
            font-weight: bold;
            color: #1e293b; 
            font-size: 16px; 
        }

        .assessment-actions {
            display: flex;
            gap: 10px;
        }

        .assessment-meta {
            display: flex;
            gap: 20px;
            font-size: 13px;
            color: #64748b;
            margin-bottom: 15px;
        }

        .grades-table-wrapper {
            background: white;
            border-radius: 8px;
            border: 1px solid #e2e8f0;
            overflow: hidden;
            margin-bottom: 15px;
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

        .table-sims tbody tr:hover { background: #f8fafc; }

        .student-name { font-weight: 500; color: #1e293b; }

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

        .grade-a { background: #dcfce7; color: #166534; }
        .grade-b { background: #dbeafe; color: #1e40af; }
        .grade-c { background: #fef3c7; color: #92400e; }
        .grade-d { background: #fee2e2; color: #991b1b; }
        .grade-f { background: #dc2626; color: white; }
        .grade-n-a { background: #f1f5f9; color: #64748b; }

        .status-published { 
            background: #dcfce7;
            color: #166534; 
            padding: 4px 12px; 
            border-radius: 20px; 
            font-size: 12px; 
            font-weight: bold; 
            display: inline-block;
        }
        .status-unpublished { 
            background: #fef3c7;
            color: #92400e; 
            padding: 4px 12px; 
            border-radius: 20px; 
            font-size: 12px; 
            font-weight: bold; 
            display: inline-block;
        }

        .btn-publish {
            background: #059669;
            color: white;
            padding: 6px 16px;
            border: none;
            border-radius: 6px;
            cursor: pointer;
            font-size: 13px;
            font-weight: 500;
            transition: all 0.2s;
        }

        .btn-publish:hover { background: #047857; }

        .btn-unpublish {
            background: #d97706;
            color: white;
            padding: 6px 16px;
            border: none;
            border-radius: 6px;
            cursor: pointer;
            font-size: 13px;
            font-weight: 500;
            transition: all 0.2s;
        }

        .btn-unpublish:hover { background: #b45309; }

        .card-footer {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-top: 15px;
            padding-top: 15px;
            border-top: 1px solid #e2e8f0;
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
            .control-row { grid-template-columns: 1fr; }
            .action-buttons { flex-direction: column; }
            .assessment-header { flex-direction: column; align-items: flex-start; }
            .card-footer { flex-direction: column; gap: 10px; }
        }

        /* Styles for Custom Tabs Interface and Algorithm Box Component */
        .sims-tabs-nav {
            display: flex;
            gap: 6px;
            border-bottom: 2px solid #e2e8f0;
            margin-bottom: 25px;
        }
        .sims-tab-btn {
            padding: 12px 24px;
            background: #f8fafc;
            border: 1px solid #e2e8f0;
            border-bottom: none;
            border-radius: 8px 8px 0 0;
            cursor: pointer;
            font-weight: 600;
            font-size: 14px;
            color: #64748b;
            transition: all 0.2s ease;
            margin-bottom: -2px;
        }
        .sims-tab-btn:hover {
            background: #f1f5f9;
            color: #334155;
        }
        .sims-tab-btn.active {
            background: white;
            color: #059669;
            border-top: 3px solid #059669;
            border-left: 1px solid #cbd5e1;
            border-right: 1px solid #cbd5e1;
            border-bottom: 2px solid white;
        }
        .sims-tab-content {
            display: none;
        }
        .sims-tab-content.active {
            display: block;
        }
        .formula-box {
            display: inline-block;
            background: #f0fdf4;
            border: 1px dashed #22c55e;
            color: #166534;
            padding: 4px 10px;
            border-radius: 4px;
            font-family: 'Consolas', monospace;
            font-size: 12px;
            margin-left: 10px;
            font-weight: normal;
            text-transform: none;
        }
    </style>
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">

    <div class="grades-header">
        <h3><i class="fa fa-star" style="margin-right: 10px;"></i>Manage Grades & Assessment</h3>
        <p>Course: <strong><asp:Literal ID="litCourseName" runat="server" /></strong></p>
    </div>

    <asp:Panel ID="pnlSuccess" runat="server" Visible="false" CssClass="success-message">
        <i class="fa fa-check-circle"></i>
        <span><asp:Literal ID="litSuccessMsg" runat="server" /></span>
    </asp:Panel>

    <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="error-message">
        <i class="fa fa-exclamation-circle"></i>
        <span><asp:Literal ID="litErrorMsg" runat="server" /></span>
    </asp:Panel>

    <div class="grades-controls">
        <div class="control-row">
            <div class="control-group">
                <label for="ddlAcademicYear">Academic Year:</label>
                <asp:DropDownList ID="ddlAcademicYear" runat="server" />
            </div>
            <div class="control-group">
                <label for="ddlSemester">Semester:</label>
                <asp:DropDownList ID="ddlSemester" runat="server" />
            </div>
            <div class="control-group" style="display: flex; flex-direction: column; justify-content: flex-end;">
                <asp:Button ID="btnLoadAssessments" runat="server" Text="Load Course Data" 
                    CssClass="btn btn-outline-success" OnClick="btnLoadAssessments_Click" />
            </div>
        </div>
    </div>

    <div class="sims-tabs-nav">
        <button type="button" class="sims-tab-btn active" onclick="switchSimsTab('tabEnterMarks', this)">
            <i class="fa fa-edit" style="margin-right: 6px;"></i>Enter Assessment Marks
        </button>
        <button type="button" class="sims-tab-btn" onclick="switchSimsTab('tabCourseSummary', this)">
            <i class="fa fa-table" style="margin-right: 6px;"></i>Student Course Summary
        </button>
    </div>

    <div id="tabEnterMarks" class="sims-tab-content active">
        <div id="assessmentsContainer">
            <asp:Panel ID="pnlEnterGrades" runat="server">
                <asp:Repeater ID="rptAssessments" runat="server" OnItemDataBound="rptAssessments_ItemDataBound">
                    <ItemTemplate>
                        <div class="assessment-card">
                            <div class="assessment-header">
                                <div class="assessment-title"><%# Eval("AssessmentName") %></div>
                                <div class="assessment-actions">
                                    <span class='<%# Convert.ToBoolean(Eval("IsPublished")) ? "status-published" : "status-unpublished" %>'>
                                        <%# Convert.ToBoolean(Eval("IsPublished")) ? "Published" : "Unpublished" %>
                                    </span>
                                    <asp:Button ID="btnTogglePublish" runat="server" 
                                        Text='<%# Convert.ToBoolean(Eval("IsPublished")) ? "Unpublish" : "Publish" %>'
                                        CssClass='<%# Convert.ToBoolean(Eval("IsPublished")) ? "btn-unpublish" : "btn-publish" %>'
                                        OnClick="btnTogglePublish_Click"
                                        CommandArgument='<%# Eval("AssessmentId") %>' />
                                </div>
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
                                            <th style="width: 100px;">Mark</th>
                                            <th style="width: 80px;">Grade</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <asp:Repeater ID="rptStudentMarks" runat="server">
                                            <ItemTemplate>
                                                <tr>
                                                    <td><%# Eval("StudentNo") %></td>
                                                    <td class="student-name"><%# Eval("FullName") %></td>
                                                    <td><%# Eval("Email") %></td>
                                                    
                                                    <td>
                                                        <input type="number" 
                                                            id="mark_<%# Eval("StudentId") %>"
                                                            class="mark-input" 
                                                            name="txtMark_<%# ((System.Data.DataRowView)((RepeaterItem)Container.Parent.Parent).DataItem)["AssessmentId"] %>_<%# Eval("StudentId") %>" 
                                                            value="<%# Eval("MarksObtained", "{0:F2}") %>"
                                                            min="0" 
                                                            max="<%# Eval("MaxMark") %>"
                                                            step="0.01" />
                                                     </td>
                                                    <td>
                                                        <span class='grade-badge grade-<%# GetGradeLetter(Eval("MarksObtained")).Substring(0, 1).ToLower() %>'>
                                                            <%# GetGradeLetter(Eval("MarksObtained")) %>
                                                        </span>
                                                   </td>
                                                </tr>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </tbody>
                                </table>
                            </div>
                            <div class="card-footer">
                                <div></div>
                                <asp:Button ID="btnSaveAllMarks" runat="server" 
                                    Text="Save All Marks for this Assessment"
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
    </div>

    <div id="tabCourseSummary" class="sims-tab-content">
        <div class="assessment-card">
            <div class="assessment-header">
                <div class="assessment-title"><i class="fa fa-list-alt" style="margin-right: 6px;"></i>Course Performance Summary Matrix</div>
            </div>
            <asp:Literal ID="litSummaryContainer" runat="server" />
        </div>
    </div>

    <asp:HiddenField ID="hidCourseId" runat="server" />

        </div>
    </div>

    <script type="text/javascript">
        // Client-side execution loop to alternate view states cleanly between tabular modules
        function switchSimsTab(tabId, btnSender) {
            var i, tabcontent, tablinks;
            tabcontent = document.getElementsByClassName("sims-tab-content");
            for (i = 0; i < tabcontent.length; i++) {
                tabcontent[i].classList.remove("active");
            }
            tablinks = document.getElementsByClassName("sims-tab-btn");
            for (i = 0; i < tablinks.length; i++) {
                tablinks[i].classList.remove("active");
            }
            document.getElementById(tabId).classList.add("active");
            btnSender.classList.add("active");
        }

        // Hide notification components after 5 seconds
        var successPanel = document.querySelector('.success-message');
        var errorPanel = document.querySelector('.error-message');
        if (successPanel && successPanel.offsetParent !== null) {
            setTimeout(function () { successPanel.style.display = 'none'; }, 5000);
        }
        if (errorPanel && errorPanel.offsetParent !== null) {
            setTimeout(function () { errorPanel.style.display = 'none'; }, 5000);
        }

        
    </script>

</asp:Content>