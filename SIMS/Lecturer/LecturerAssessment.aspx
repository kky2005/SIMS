<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="LecturerAssessment.aspx.cs"
    Inherits="SIMS.Lecturer.LecturerAssessments"
    MasterPageFile="~/Lecturer/LecturerMaster.master" %>

<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .assessments-header {
            background: linear-gradient(135deg, #06b6d4 0%, #0891b2 100%);
            color: white;
            padding: 20px;
            border-radius: 8px;
            margin-bottom: 30px;
        }
        .create-assessment-form {
            background: white;
            border: 1px solid #e2e8f0;
            border-radius: 8px;
            padding: 20px;
            margin-bottom: 30px;
        }
        .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; margin-bottom: 12px; }
        .form-row-full { grid-column: 1 / -1; }
        .form-group label { font-weight: bold; color: #1e293b; margin-bottom: 6px; display: block; }
        .form-group input, .form-group select { width:100%; padding:10px; border:1px solid #cbd5e1; border-radius:6px; }
        .form-actions { display:flex; gap:10px; justify-content:flex-end; margin-top:12px; }
        .assessments-table { background:white; border:1px solid #e2e8f0; border-radius:8px; overflow:hidden; margin-top:12px; }
        .table-sims { width:100%; border-collapse:collapse; }
        .table-sims th, .table-sims td { padding:12px; border-bottom:1px solid #e2e8f0; text-align:left; }
        .table-sims th { background:#f8fafc; font-weight:600; }
        .success-message {
            background: #dcfce7;
            border: 1px solid #86efac;
            color: #166534;
            padding: 12px 15px;
            border-radius: 6px;
            margin-bottom: 12px;
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
            margin-bottom: 12px;
            display: flex;
            align-items: center;
            gap: 10px;
        }
        .back-btn {
            display: inline-flex;
            align-items: center;
            gap: 6px;
            color: white;
            text-decoration: none;
            font-size: 13px;
            background: rgba(255, 255, 255, 0.15);
            padding: 6px 12px;
            border-radius: 6px;
            margin-bottom: 12px;
            transition: background 0.2s ease;
        }
        .back-btn:hover {
            background: rgba(255, 255, 255, 0.25);
            color: white;
        }
    </style>
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">

    <div class="assessments-header">
        <a href="LecturerCourses.aspx" class="back-btn">
            <i class="fa fa-arrow-left"></i> Back to Courses
        </a>
        <h3><i class="fa fa-clipboard" style="margin-right:10px;"></i>Assessment Management</h3>
        <p style="margin:0;"><asp:Literal ID="litCourseHeader" runat="server" Text="Loading..." /></p>
    </div>

    <asp:Panel ID="pnlSuccess" runat="server" Visible="false" CssClass="success-message">
        <i class="fa fa-check-circle"></i>
        <span><asp:Literal ID="litSuccessMsg" runat="server" /></span>
    </asp:Panel>

    <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="error-message">
        <i class="fa fa-exclamation-circle"></i>
        <span><asp:Literal ID="litErrorMsg" runat="server" /></span>
    </asp:Panel>

    <div class="create-assessment-form">
        <h4 style="margin:0 0 12px 0;">Create New Assessment</h4>

        <div class="form-row">
            <div class="form-group">
                <label>Course</label>
                <asp:Literal ID="litCourseName" runat="server" />
            </div>
            <div class="form-group">
                <label>Academic Year</label>
                <asp:Literal ID="litAcademicYear" runat="server" />
            </div>
        </div>

        <div class="form-row">
            <div class="form-group">
                <label for="txtAssessmentName">Assessment Name *</label>
                <asp:TextBox ID="txtAssessmentName" runat="server" Placeholder="e.g., Quiz 1, Midterm Exam, Assignment 1" />
            </div>
            <div class="form-group">
                <label for="txtMaxMark">Max Mark *</label>
                <asp:TextBox ID="txtMaxMark" runat="server" TextMode="Number" Placeholder="e.g., 100" />
            </div>
        </div>

        <div class="form-row">
            <div class="form-group">
                <label for="ddlSemester">Semester *</label>
                <asp:DropDownList ID="ddlSemester" runat="server">
                    <asp:ListItem Text="Semester 1" Value="1" />
                    <asp:ListItem Text="Semester 2" Value="2" />
                    <asp:ListItem Text="Semester 3" Value="3" />
                </asp:DropDownList>
            </div>
            <div class="form-group">
                <label for="txtWeightage">Weightage (%) *</label>
                <asp:TextBox ID="txtWeightage" runat="server" TextMode="Number" Placeholder="e.g., 30" />
            </div>
        </div>

        <div class="form-actions">
            <asp:Button ID="btnCreate" runat="server" Text="Create Assessment" CssClass="btn btn-primary" OnClick="btnCreate_Click" />
            <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-outline-secondary" OnClick="btnClear_Click" />
        </div>

        <asp:HiddenField ID="hidCourseId" runat="server" />
        <asp:HiddenField ID="hidAcademicYear" runat="server" />
    </div>

    <h4 style="margin-top:16px;">Assessments for this Course</h4>

    <div class="assessments-table">
        <table class="table-sims">
            <thead>
                <tr>
                    <th>Assessment Name</th>
                    <th>Semester</th>
                    <th>Max Mark</th>
                    <th>Weightage (%)</th>
                    <th>Published</th>
                    <th style="width:120px;">Action</th>
                </tr>
            </thead>
            <tbody>
                <asp:Repeater ID="rptAssessments" runat="server">
                    <ItemTemplate>
                        <tr>
                            <td><strong><%# Eval("AssessmentName") %></strong></td>
                            <td>Semester <%# Eval("Semester") %></td>
                            <td><%# Eval("MaxMark") %></td>
                            <td><%# Eval("Weightage") %>%</td>
                            <td>
                                <span style='<%# Convert.ToBoolean(Eval("IsPublished")) ? "color:#166534;" : "color:#991b1b;" %>'>
                                    <%# Convert.ToBoolean(Eval("IsPublished")) ? "✓ Yes" : "✗ No" %>
                                </span>
                            </td>
                            <td>
                                <asp:Button ID="btnTogglePublish" runat="server" 
                                    Text='<%# Convert.ToBoolean(Eval("IsPublished")) ? "Unpublish" : "Publish" %>'
                                    CssClass='<%# Convert.ToBoolean(Eval("IsPublished")) ? "btn btn-sm btn-warning" : "btn btn-sm btn-success" %>'
                                    CommandArgument='<%# Eval("AssessmentId") %>' 
                                    OnClick="btnTogglePublish_Click" />
                                <asp:Button ID="btnDelete" runat="server" 
                                    Text="Delete"
                                    CssClass="btn btn-sm btn-danger"
                                    CommandArgument='<%# Eval("AssessmentId") %>' 
                                    OnClick="btnDelete_Click" 
                                    OnClientClick="return confirm('Delete this assessment? Any associated marks will also be deleted.');" />
                            </td>
                        </tr>
                    </ItemTemplate>
                </asp:Repeater>
            </tbody>
        </table>

        <asp:Panel ID="pnlNoAssessments" runat="server" Visible="false" CssClass="no-assessments" style="padding:20px; text-align:center;">
            <i class="fa fa-inbox" style="font-size:28px;color:#cbd5e1;display:block;margin-bottom:8px;"></i>
            <strong>No assessments created for this course yet.</strong>
        </asp:Panel>
    </div>

    <script type="text/javascript">
        document.addEventListener('DOMContentLoaded', function () {
            var successEl = document.getElementById('<%= pnlSuccess.ClientID %>');
            var errorEl = document.getElementById('<%= pnlError.ClientID %>');

            [successEl, errorEl].forEach(function (el) {
                if (!el) return;
                if (el.offsetParent !== null) {
                    setTimeout(function () { el.style.display = 'none'; }, 5000);
                }
            });
        });
    </script>

</asp:Content>