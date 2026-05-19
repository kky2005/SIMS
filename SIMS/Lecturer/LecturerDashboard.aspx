<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="LecturerDashboard.aspx.cs"
    Inherits="SIMS.Lecturer.LecturerDashboard"
    MasterPageFile="~/Lecturer/LecturerMaster.master" %>

<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">

    <!-- Welcome banner -->
    <div style="background:#fff; border-radius:12px; padding:24px 28px;
                border:1px solid #e2e8f0; margin-bottom:24px;
                display:flex; align-items:center; justify-content:space-between;">
        <div>
            <h4 style="margin:0; color:#1e293b; font-weight:bold;">
                Welcome back, <asp:Literal ID="litName" runat="server" /> 👋
            </h4>
            <p style="margin:4px 0 0; color:#64748b; font-size:14px;">
                <asp:Literal ID="litDept" runat="server" /> &nbsp;|&nbsp;
                Staff No: <asp:Literal ID="litStaffNo" runat="server" />
            </p>
        </div>
        <div style="text-align:right; color:#64748b; font-size:13px;">
            <i class="fa fa-calendar"></i>
            <asp:Literal ID="litDate" runat="server" />
        </div>
    </div>

    <!-- Stat cards row -->
    <div class="row g-3 mb-4">

        <div class="col-md-3">
            <div class="stat-card">
                <div class="stat-icon" style="background:#dbeafe;">
                    <i class="fa fa-book" style="color:#1d4ed8;"></i>
                </div>
                <div>
                    <p class="stat-label">My Courses</p>
                    <div class="stat-value">
                        <asp:Literal ID="litTotalCourses" runat="server" Text="0" />
                    </div>
                </div>
            </div>
        </div>

        <div class="col-md-3">
            <div class="stat-card">
                <div class="stat-icon" style="background:#dcfce7;">
                    <i class="fa fa-users" style="color:#166534;"></i>
                </div>
                <div>
                    <p class="stat-label">Total Students</p>
                    <div class="stat-value">
                        <asp:Literal ID="litTotalStudents" runat="server" Text="0" />
                    </div>
                </div>
            </div>
        </div>

        <div class="col-md-3">
            <div class="stat-card">
                <div class="stat-icon" style="background:#fef3c7;">
                    <i class="fa fa-triangle-exclamation" style="color:#b45309;"></i>
                </div>
                <div>
                    <p class="stat-label">At-Risk Students</p>
                    <div class="stat-value">
                        <asp:Literal ID="litAtRisk" runat="server" Text="0" />
                    </div>
                </div>
            </div>
        </div>

        <div class="col-md-3">
            <div class="stat-card">
                <div class="stat-icon" style="background:#ede9fe;">
                    <i class="fa fa-clipboard-list" style="color:#6d28d9;"></i>
                </div>
                <div>
                    <p class="stat-label">Pending Marks</p>
                    <div class="stat-value">
                        <asp:Literal ID="litPendingMarks" runat="server" Text="0" />
                    </div>
                </div>
            </div>
        </div>

    </div>

    <div class="row g-3">

        <!-- My courses this semester -->
        <div class="col-md-8">
            <div class="card-sims">
                <div class="card-header-sims">
                    <h5><i class="fa fa-book me-2" style="color:#0d6efd;"></i>My Courses This Semester</h5>
                    <a href="LecturerCourses.aspx" class="btn btn-sm btn-outline-primary">
                        View All
                    </a>
                </div>
                <div class="card-body-sims" style="padding:0;">
                    <asp:GridView ID="gvDashboardCourses"
                        runat="server"
                        AutoGenerateColumns="False"
                        CssClass="table table-sims table-hover mb-0"
                        Width="100%"
                        EmptyDataText="No courses assigned for this semester.">
                        <Columns>
                            <asp:BoundField DataField="CourseCode"  HeaderText="Code" />
                            <asp:BoundField DataField="CourseName"  HeaderText="Course Name" />
                            <asp:BoundField DataField="CreditHours" HeaderText="Credits" />
                            <asp:BoundField DataField="TotalStudents" HeaderText="Students" />
                            <asp:TemplateField HeaderText="">
                                <ItemTemplate>
                                    <a href='<%# "LecturerAttendance.aspx?CourseID=" + Eval("CourseId") %>'
                                       class="btn btn-sm btn-outline-secondary me-1">
                                        Attendance
                                    </a>
                                    <a href='<%# "LecturerGrades.aspx?CourseID=" + Eval("CourseId") %>'
                                       class="btn btn-sm btn-outline-success">
                                        Grades
                                    </a>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>

        <!-- Quick actions -->
        <div class="col-md-4">
            <div class="card-sims h-100">
                <div class="card-header-sims">
                    <h5><i class="fa fa-bolt me-2" style="color:#f59e0b;"></i>Quick Actions</h5>
                </div>
                <div class="card-body-sims">
                    <div class="d-grid gap-2">
                        <a href="LecturerAttendance.aspx"
                           class="btn btn-outline-primary text-start">
                            <i class="fa fa-calendar-check me-2"></i> Record Attendance
                        </a>
                        <a href="LecturerGrades.aspx"
                           class="btn btn-outline-success text-start">
                            <i class="fa fa-star me-2"></i> Enter / Publish Grades
                        </a>
                        <a href="LecturerMaterials.aspx"
                           class="btn btn-outline-secondary text-start">
                            <i class="fa fa-upload me-2"></i> Upload Materials
                        </a>
                        <a href="LecturerAnnouncements.aspx"
                           class="btn btn-outline-warning text-start">
                            <i class="fa fa-bullhorn me-2"></i> Post Announcement
                        </a>
                        <a href="LecturerStudentProgress.aspx"
                           class="btn btn-outline-danger text-start">
                            <i class="fa fa-chart-line me-2"></i> View Student Progress
                        </a>
                    </div>
                </div>
            </div>
        </div>

        <!-- At-risk students -->
        <div class="col-12">
            <div class="card-sims">
                <div class="card-header-sims">
                    <h5><i class="fa fa-triangle-exclamation me-2" style="color:#dc2626;"></i>
                        At-Risk Students
                        <span class="badge bg-danger ms-2" style="font-size:12px;">
                            <asp:Literal ID="litAtRiskBadge" runat="server" Text="0" />
                        </span>
                    </h5>
                    <a href="LecturerStudentProgress.aspx" class="btn btn-sm btn-outline-danger">
                        View All
                    </a>
                </div>
                <div class="card-body-sims" style="padding:0;">
                    <asp:GridView ID="gvAtRisk"
                        runat="server"
                        AutoGenerateColumns="False"
                        CssClass="table table-sims table-hover mb-0"
                        Width="100%"
                        EmptyDataText="No at-risk students found.">
                        <Columns>
                            <asp:BoundField DataField="StudentNo"   HeaderText="Student No" />
                            <asp:BoundField DataField="FullName"    HeaderText="Student Name" />
                            <asp:BoundField DataField="CourseName"  HeaderText="Course" />
                            <asp:BoundField DataField="AttendancePct" HeaderText="Attendance %" />
                            <asp:TemplateField HeaderText="Status">
                                <ItemTemplate>
                                    <span class='badge badge-warning'>At Risk</span>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>

    </div>

</asp:Content>
