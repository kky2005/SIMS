<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CourseDetails.aspx.cs" Inherits="SIMS.Student.CourseDetails" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>SIMS - Course Details</title>

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

        .info-grid {
            display: grid;
            grid-template-columns: repeat(4, 1fr);
            gap: 16px;
        }

        .info-box {
            background: #f8fafc;
            padding: 14px;
            border-radius: 10px;
            border: 1px solid #e2e8f0;
        }

        .info-label {
            font-size: 12px;
            color: #64748b;
            margin-bottom: 6px;
        }

        .info-value {
            font-size: 16px;
            color: #1e293b;
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
            vertical-align: top;
        }

        .btn-download {
            background: #0d6efd;
            color: white;
            padding: 7px 12px;
            border-radius: 6px;
            font-size: 13px;
            text-decoration: none;
            display: inline-block;
        }

        .btn-download:hover {
            background: #0b5ed7;
            color: white;
        }

        .message {
            display: block;
            margin-top: 12px;
            font-weight: bold;
        }

        .announcement-title {
            font-weight: bold;
            color: #1e293b;
            margin-bottom: 4px;
        }

        .announcement-body {
            color: #475569;
            line-height: 1.5;
        }

        @media (max-width: 768px) {
            body {
                padding: 18px;
            }

            .info-grid {
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

            <a href="EnrolledCourses.aspx" class="back-link">
                <i class="fa fa-arrow-left"></i> Back to Enrolled Courses
            </a>

            <!-- Course Information -->
            <div class="card-sims">
                <div class="card-header-sims">
                    <h2>
                        <asp:Label ID="lblCourseTitle" runat="server" Text="Course Details"></asp:Label>
                    </h2>
                    <p>View course information, lecturer-uploaded materials, and course announcements.</p>
                    <asp:Label ID="lblMessage" runat="server" CssClass="message"></asp:Label>
                </div>

                <div class="card-body-sims">
                    <div class="info-grid">
                        <div class="info-box">
                            <div class="info-label">Course Code</div>
                            <div class="info-value">
                                <asp:Label ID="lblCourseCode" runat="server" Text="-"></asp:Label>
                            </div>
                        </div>

                        <div class="info-box">
                            <div class="info-label">Course Name</div>
                            <div class="info-value">
                                <asp:Label ID="lblCourseName" runat="server" Text="-"></asp:Label>
                            </div>
                        </div>

                        <div class="info-box">
                            <div class="info-label">Credit Hours</div>
                            <div class="info-value">
                                <asp:Label ID="lblCreditHours" runat="server" Text="-"></asp:Label>
                            </div>
                        </div>

                        <div class="info-box">
                            <div class="info-label">Academic Year / Semester</div>
                            <div class="info-value">
                                <asp:Label ID="lblYearSemester" runat="server" Text="-"></asp:Label>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Course Materials -->
            <div class="card-sims">
                <div class="card-header-sims">
                    <h2 style="font-size:22px;">
                        <i class="fa fa-folder-open me-2"></i>Course Materials
                    </h2>
                    <p>Materials uploaded by lecturer for this course.</p>
                </div>

                <div class="card-body-sims">
                    <asp:GridView ID="gvMaterials" runat="server"
                        AutoGenerateColumns="False"
                        CssClass="grid"
                        EmptyDataText="No course materials uploaded yet.">

                        <Columns>
                            <asp:BoundField DataField="Title" HeaderText="Title" />
                            <asp:BoundField DataField="Description" HeaderText="Description" />
                            <asp:BoundField DataField="FileType" HeaderText="File Type" />
                            <asp:BoundField DataField="FileSizeKB" HeaderText="Size (KB)" />
                            <asp:BoundField DataField="UploadedAt" HeaderText="Uploaded At" DataFormatString="{0:dd MMM yyyy hh:mm tt}" />

                            <asp:TemplateField HeaderText="Action">
                                <ItemTemplate>
                                    <a class="btn-download" href='<%# ResolveUrl(Eval("FileUrl").ToString()) %>' target="_blank">
                                        <i class="fa fa-download"></i> Open
                                    </a>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>

            <!-- Course Announcements -->
            <div class="card-sims">
                <div class="card-header-sims">
                    <h2 style="font-size:22px;">
                        <i class="fa fa-bullhorn me-2"></i>Course Announcements
                    </h2>
                    <p>Announcements posted by lecturers for this course.</p>
                </div>

                <div class="card-body-sims">
                    <asp:GridView ID="gvAnnouncements" runat="server"
                        AutoGenerateColumns="False"
                        CssClass="grid"
                        EmptyDataText="No course announcements available.">

                        <Columns>
                            <asp:TemplateField HeaderText="Announcement">
                                <ItemTemplate>
                                    <div class="announcement-title">
                                        <%# Eval("Title") %>
                                    </div>

                                    <div class="announcement-body">
                                        <%# Eval("Body") %>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:BoundField DataField="PostedBy" HeaderText="Posted By" />

                            <asp:BoundField DataField="PublishedAt"
                                HeaderText="Published At"
                                DataFormatString="{0:dd MMM yyyy hh:mm tt}" />

                            <asp:BoundField DataField="ExpiresAt"
                                HeaderText="Expires At"
                                DataFormatString="{0:dd MMM yyyy}" />
                        </Columns>
                    </asp:GridView>
                </div>
            </div>

        </div>
    </form>
</body>
</html>