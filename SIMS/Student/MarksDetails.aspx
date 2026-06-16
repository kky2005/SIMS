<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MarksDetails.aspx.cs" Inherits="SIMS.Student.MarksDetails" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>SIMS - Marks Details</title>

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

        .summary-grid {
            display: grid;
            grid-template-columns: repeat(4, 1fr);
            gap: 16px;
        }

        .summary-box {
            background: #f8fafc;
            padding: 14px;
            border-radius: 10px;
            border: 1px solid #e2e8f0;
        }

        .summary-label {
            font-size: 12px;
            color: #64748b;
            margin-bottom: 6px;
        }

        .summary-value {
            font-size: 16px;
            color: #1e293b;
            font-weight: bold;
        }

        .course-title {
            font-size: 20px;
            font-weight: bold;
            color: #1e293b;
            margin-bottom: 8px;
        }

        .course-meta {
            display: flex;
            flex-wrap: wrap;
            gap: 10px;
            margin-bottom: 16px;
        }

        .badge-sims {
            background: #eef2ff;
            color: #3730a3;
            padding: 6px 10px;
            border-radius: 999px;
            font-size: 13px;
            font-weight: bold;
        }

        .badge-success {
            background: #dcfce7;
            color: #166534;
        }

        .badge-warning {
            background: #fef3c7;
            color: #92400e;
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

            <a href="AcademicResults.aspx" class="back-link">
                <i class="fa fa-arrow-left"></i> Back to Academic Results
            </a>

            <div class="card-sims">
                <div class="card-header-sims">
                    <h2>Marks Details</h2>
                    <p>View detailed assessment marks for quizzes, tests, assignments, and final exams.</p>
                    <asp:Label ID="lblMessage" runat="server" CssClass="message"></asp:Label>
                </div>

                <div class="card-body-sims">
                    <div class="summary-grid">
                        <div class="summary-box">
                            <div class="summary-label">Student Name</div>
                            <div class="summary-value">
                                <asp:Label ID="lblStudentName" runat="server" Text="-"></asp:Label>
                            </div>
                        </div>

                        <div class="summary-box">
                            <div class="summary-label">Student No</div>
                            <div class="summary-value">
                                <asp:Label ID="lblStudentNo" runat="server" Text="-"></asp:Label>
                            </div>
                        </div>

                        <div class="summary-box">
                            <div class="summary-label">Academic Year</div>
                            <div class="summary-value">
                                <asp:Label ID="lblAcademicYear" runat="server" Text="-"></asp:Label>
                            </div>
                        </div>

                        <div class="summary-box">
                            <div class="summary-label">Semester</div>
                            <div class="summary-value">
                                <asp:Label ID="lblSemester" runat="server" Text="-"></asp:Label>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <asp:Repeater ID="rptCourses" runat="server" OnItemDataBound="rptCourses_ItemDataBound">
                <ItemTemplate>
                    <div class="card-sims">
                        <div class="card-header-sims">
                            <div class="course-title">
                                <%# Eval("CourseCode") %> - <%# Eval("CourseName") %>
                            </div>

                            <div class="course-meta">
                                <span class="badge-sims">Credit Hours: <%# Eval("CreditHours") %></span>
                                <span class="badge-sims">Total Mark: <%# Eval("TotalMark", "{0:0.00}") %></span>
                                <span class="badge-sims">Grade: <%# Eval("GradeLetter") %></span>
                                <span class="badge-sims">Grade Point: <%# Eval("GradePoint") %></span>
                                <span class='<%# Eval("ResultStatus").ToString() == "Published" ? "badge-sims badge-success" : "badge-sims badge-warning" %>'>
                                    <%# Eval("ResultStatus") %>
                                </span>
                            </div>
                        </div>

                        <div class="card-body-sims">
                            <asp:HiddenField ID="hfCourseId" runat="server" Value='<%# Eval("CourseId") %>' />

                            <asp:GridView ID="gvAssessments" runat="server"
                                AutoGenerateColumns="False"
                                CssClass="grid"
                                EmptyDataText="No assessment marks available for this course.">

                                <Columns>
                                    <asp:BoundField DataField="AssessmentName" HeaderText="Assessment" />
                                    <asp:BoundField DataField="MaxMark" HeaderText="Max Mark" DataFormatString="{0:0.00}" />
                                    <asp:BoundField DataField="Weightage" HeaderText="Weightage (%)" DataFormatString="{0:0.00}" />
                                    <asp:BoundField DataField="MarksObtainedDisplay" HeaderText="Marks Obtained" />
                                    <asp:BoundField DataField="WeightedMarkDisplay" HeaderText="Weighted Mark" />
                                    <asp:BoundField DataField="StatusDisplay" HeaderText="Status" />
                                    <asp:BoundField DataField="GradedAtDisplay" HeaderText="Graded At" />
                                </Columns>
                            </asp:GridView>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>

        </div>
    </form>
</body>
</html>