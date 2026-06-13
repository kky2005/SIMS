<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AcademicResults.aspx.cs" Inherits="SIMS.Student.AcademicResults" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>SIMS - Academic Results</title>

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
            max-width: 1200px;
            margin: auto;
        }

        .back-link {
            display: inline-block;
            margin-bottom: 16px;
            text-decoration: none;
            color: #1e293b;
            font-weight: bold;
        }

        .page-header {
            background: #fff;
            border-radius: 12px;
            padding: 24px 28px;
            border: 1px solid #e2e8f0;
            margin-bottom: 24px;
            box-shadow: 0 1px 4px rgba(0,0,0,0.06);
        }

        .summary-card {
            background: #fff;
            border-radius: 12px;
            padding: 20px 24px;
            border: 1px solid #e2e8f0;
            box-shadow: 0 1px 4px rgba(0,0,0,0.06);
            height: 100%;
        }

        .summary-label {
            color: #64748b;
            font-size: 13px;
            margin-bottom: 8px;
        }

        .summary-value {
            color: #1e293b;
            font-size: 28px;
            font-weight: bold;
        }

        .semester-card {
            background: #fff;
            border-radius: 12px;
            border: 1px solid #e2e8f0;
            box-shadow: 0 1px 4px rgba(0,0,0,0.06);
            margin-top: 24px;
            overflow: hidden;
        }

        .semester-header {
            padding: 18px 24px;
            border-bottom: 1px solid #e2e8f0;
            display: flex;
            justify-content: space-between;
            align-items: center;
            gap: 12px;
        }

        .semester-header h4 {
            font-size: 17px;
            font-weight: bold;
            margin: 0;
            color: #1e293b;
        }

        .semester-body {
            padding: 24px;
        }

        .grid {
            width: 100%;
            border-collapse: collapse;
        }

        .grid th {
            background: #1e293b;
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

        .grid tr:hover td {
            background: #f8fafc;
        }

        .message {
            display: block;
            margin-top: 12px;
            font-weight: bold;
        }

        .btn-download {
            background: #0d6efd;
            color: white;
            border: none;
            padding: 8px 12px;
            border-radius: 7px;
            font-size: 13px;
            cursor: pointer;
        }

        .btn-download:hover {
            background: #0b5ed7;
        }
    </style>
</head>

<body>
    <form id="form1" runat="server">
        <div class="container-sims">

            <a href="Dashboard.aspx" class="back-link">
                <i class="fa fa-arrow-left"></i> Back to Dashboard
            </a>

            <div class="page-header">
                <h2 style="margin:0; color:#1e293b; font-weight:bold;">
                    Academic Results
                </h2>
                <p style="margin:6px 0 0; color:#64748b;">
                    View your results by semester and generate semester result reports.
                </p>

                <asp:Label ID="lblMessage" runat="server" CssClass="message"></asp:Label>
            </div>

            <!-- GPA / CGPA summary -->
            <div class="row g-3">
                <div class="col-md-3">
                    <div class="summary-card">
                        <div class="summary-label">Latest GPA</div>
                        <div class="summary-value">
                            <asp:Label ID="lblGPA" runat="server" Text="-"></asp:Label>
                        </div>
                    </div>
                </div>

                <div class="col-md-3">
                    <div class="summary-card">
                        <div class="summary-label">Latest CGPA</div>
                        <div class="summary-value">
                            <asp:Label ID="lblCGPA" runat="server" Text="-"></asp:Label>
                        </div>
                    </div>
                </div>

                <div class="col-md-3">
                    <div class="summary-card">
                        <div class="summary-label">Total Credit Hours</div>
                        <div class="summary-value">
                            <asp:Label ID="lblCreditHours" runat="server" Text="-"></asp:Label>
                        </div>
                    </div>
                </div>

                <div class="col-md-3">
                    <div class="summary-card">
                        <div class="summary-label">Latest Semester</div>
                        <div class="summary-value">
                            <asp:Label ID="lblLatestSemester" runat="server" Text="-"></asp:Label>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Semester result cards -->
            <asp:Repeater ID="rptSemesters" runat="server"
                OnItemDataBound="rptSemesters_ItemDataBound"
                OnItemCommand="rptSemesters_ItemCommand">

                <ItemTemplate>
                    <div class="semester-card">

                        <div class="semester-header">
                            <h4>
                                <i class="fa fa-graduation-cap me-2"></i>
                                Academic Year <%# Eval("AcademicYear") %> - Semester <%# Eval("Semester") %>
                            </h4>

                           
                           <asp:Button ID="btnViewMarksDetails" runat="server"
                                Text="View Marks Details"
                                CssClass="btn btn-success btn-sm me-2"
                                Style="margin-left: 520px;"
                                CommandName="ViewMarksDetails"
                                CommandArgument='<%# Eval("AcademicYear").ToString() + "|" + Eval("Semester").ToString() %>' />
                            <asp:LinkButton ID="btnGenerateReport" runat="server"
                                CssClass="btn-download"
                                CommandName="GenerateReport"
                                CommandArgument='<%# Eval("AcademicYear") + "|" + Eval("Semester") %>'>
                                <i class="fa fa-download"></i> Generate Report
                            </asp:LinkButton>
                        </div>

                        <div class="semester-body">
                            <asp:GridView ID="gvSemesterResults" runat="server"
                                AutoGenerateColumns="False"
                                CssClass="grid"
                                EmptyDataText="No results found for this semester.">

                                <Columns>
                                    <asp:BoundField DataField="CourseCode" HeaderText="Course Code" />
                                    <asp:BoundField DataField="CourseName" HeaderText="Course Name" />
                                    <asp:BoundField DataField="CreditHours" HeaderText="Credits" />
                                    <asp:BoundField DataField="TotalWeightedMark" HeaderText="Total Mark" />
                                    <asp:BoundField DataField="Grade" HeaderText="Grade" />
                                    <asp:BoundField DataField="GradePoint" HeaderText="Grade Point" />
                                    <asp:BoundField DataField="ResultStatus" HeaderText="Status" />
                                </Columns>
                            </asp:GridView>
                        </div>

                    </div>
                </ItemTemplate>

            </asp:Repeater>

            <asp:Panel ID="pnlGeneratedReport" runat="server" Visible="false" CssClass="semester-card">

                <div class="semester-header">
                    <h4>
                        <i class="fa fa-file-lines me-2"></i>
                        Generated Academic Result Slip
                    </h4>

                    <asp:Button ID="btnDownloadPdf" runat="server"
                        Text="Download PDF"
                        CssClass="btn-download"
                        OnClick="btnDownloadPdf_Click" />
                </div>

                <div class="semester-body">

                    <div style="text-align:center; margin-bottom:20px;">
                        <h3 style="margin-bottom:4px;">SIMS Academic Result Slip</h3>
                        <p style="margin:0;">Student Information Management System</p>
                    </div>

                    <table style="width:100%; margin-bottom:18px;">
                        <tr>
                            <td><strong>Name:</strong> <asp:Label ID="lblReportName" runat="server"></asp:Label></td>
                            <td><strong>Student No:</strong> <asp:Label ID="lblReportStudentNo" runat="server"></asp:Label></td>
                        </tr>
                        <tr>
                            <td><strong>Academic Year:</strong> <asp:Label ID="lblReportAcademicYear" runat="server"></asp:Label></td>
                            <td><strong>Semester:</strong> <asp:Label ID="lblReportSemester" runat="server"></asp:Label></td>
                        </tr>
                    </table>

                    <asp:GridView ID="gvGeneratedReport" runat="server"
                        AutoGenerateColumns="False"
                        CssClass="grid"
                        EmptyDataText="No results found for this semester.">

                        <Columns>
                            <asp:BoundField DataField="CourseCode" HeaderText="Subject Code" />
                            <asp:BoundField DataField="CourseName" HeaderText="Subject Name" />
                            <asp:BoundField DataField="CreditHours" HeaderText="Credit Hours" />
                            <asp:BoundField DataField="TotalWeightedMark" HeaderText="Marks" />
                            <asp:BoundField DataField="Grade" HeaderText="Grade" />
                            <asp:BoundField DataField="GradePoint" HeaderText="Grade Point" />
                        </Columns>
                    </asp:GridView>

                    <br />

                    <table style="width:100%; font-weight:bold;">
                        <tr>
                            <td>Total Credit Hours: <asp:Label ID="lblReportCreditHours" runat="server"></asp:Label></td>
                            <td>GPA: <asp:Label ID="lblReportGPA" runat="server"></asp:Label></td>
                            <td>CGPA: <asp:Label ID="lblReportCGPA" runat="server"></asp:Label></td>
                        </tr>
                    </table>

                </div>
            </asp:Panel>

        </div>

        <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    </form>
</body>
</html>