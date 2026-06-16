<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CourseRegistration.aspx.cs" Inherits="SIMS.Student.CourseRegistration" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Course Registration</title>

    <style>
        body {
            font-family: Arial, sans-serif;
            background: #f3f6fa;
            margin: 0;
            padding: 30px;
        }

        .container {
            max-width: 1100px;
            margin: auto;
        }

        .card {
            background: white;
            padding: 22px;
            margin-bottom: 22px;
            border-radius: 12px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.08);
        }

        h2, h3 {
            margin-top: 0;
        }

        .message {
            display: block;
            margin: 15px 0;
            font-weight: bold;
        }

        .grid {
            width: 100%;
            border-collapse: collapse;
        }

        .grid th {
            background: #1f3b63;
            color: white;
            padding: 10px;
            text-align: left;
        }

        .grid td {
            padding: 10px;
            border-bottom: 1px solid #ddd;
        }

        .btn-register {
            background: #007bff;
            color: white;
            padding: 6px 12px;
            border: none;
            border-radius: 6px;
            cursor: pointer;
        }

        .btn-drop {
            background: #dc3545;
            color: white;
            padding: 6px 12px;
            border: none;
            border-radius: 6px;
            cursor: pointer;
        }

        .back-link {
            display: inline-block;
            margin-bottom: 15px;
            text-decoration: none;
            color: #1f3b63;
            font-weight: bold;
        }
    </style>
</head>

<body>
    <form id="form1" runat="server">
        <div class="container">

            <a href="Dashboard.aspx" class="back-link">← Back to Dashboard</a>

            <div class="card">
                <h2>Course Registration</h2>
                <p>
                    Submit course registration or drop requests here. All requests will be sent to Admin for approval.
                </p>

                <asp:Label ID="lblSemester" runat="server"></asp:Label>
                <br />
                <asp:Label ID="lblMessage" runat="server" CssClass="message"></asp:Label>
            </div>

            <div class="card">
                <h3>Registration and Drop Periods</h3>

                <asp:GridView ID="gvRegistrationPeriods" runat="server"
                    AutoGenerateColumns="False"
                    CssClass="grid"
                    EmptyDataText="No registration or drop period has been set for your current semester.">

                    <Columns>
                        <asp:BoundField DataField="PeriodType" HeaderText="Period Type" />
                        <asp:BoundField DataField="StartDateText" HeaderText="Start Date" />
                        <asp:BoundField DataField="EndDateText" HeaderText="End Date" />
                        <asp:BoundField DataField="PeriodStatus" HeaderText="Status" />
                    </Columns>
                </asp:GridView>
            </div>

            <div id="pnlAvailableCourses" runat="server" class="card">
                <h3>Available Courses</h3>

                <asp:GridView ID="gvAvailableCourses" runat="server"
                    AutoGenerateColumns="False"
                    CssClass="grid"
                    OnRowCommand="gvAvailableCourses_RowCommand">

                    <Columns>
                        <asp:BoundField DataField="CourseCode" HeaderText="Course Code" />
                        <asp:BoundField DataField="CourseName" HeaderText="Course Name" />
                        <asp:BoundField DataField="CreditHours" HeaderText="Credit Hours" />
                        <asp:BoundField DataField="Semester" HeaderText="Course Semester" />

                        <asp:TemplateField HeaderText="Action">
                            <ItemTemplate>
                                <asp:Button ID="btnRegister" runat="server"
                                    Text="Request Register"
                                    CssClass="btn-register"
                                    CommandName="RegisterCourse"
                                    CommandArgument='<%# Eval("CourseId") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>

            <div class="card">
                <h3>Current Semester Enrolled Courses </h3>

                <asp:GridView ID="gvEnrolledCourses" runat="server"
                    AutoGenerateColumns="False"
                    CssClass="grid"
                    OnRowCommand="gvEnrolledCourses_RowCommand">

                    <Columns>
                        <asp:BoundField DataField="CourseCode" HeaderText="Course Code" />
                        <asp:BoundField DataField="CourseName" HeaderText="Course Name" />
                        <asp:BoundField DataField="CreditHours" HeaderText="Credit Hours" />
                        <asp:BoundField DataField="AcademicYear" HeaderText="Academic Year" />
                        <asp:BoundField DataField="Semester" HeaderText="Enrolment Semester" />
                        <asp:BoundField DataField="Status" HeaderText="Status" />

                        <asp:TemplateField HeaderText="Action">
                            <ItemTemplate>
                                <asp:Button ID="btnDrop" runat="server"
                                    Text="Request Drop"
                                    CssClass="btn-drop"
                                    CommandName="DropCourse"
                                    CommandArgument='<%# Eval("CourseId") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>

            <div class="card">
                <h3>My Course Requests</h3>

                <asp:GridView ID="gvCourseRequests" runat="server"
                    AutoGenerateColumns="False"
                    CssClass="grid">

                    <Columns>
                        <asp:BoundField DataField="CourseCode" HeaderText="Course Code" />
                        <asp:BoundField DataField="CourseName" HeaderText="Course Name" />
                        <asp:BoundField DataField="RequestType" HeaderText="Request Type" />
                        <asp:BoundField DataField="Status" HeaderText="Status" />
                        <asp:BoundField DataField="RequestedAt" HeaderText="Requested At" />
                        <asp:BoundField DataField="AdminRemarks" HeaderText="Admin Remarks" />
                    </Columns>
                </asp:GridView>
            </div>

        </div>
    </form>
</body>
</html>