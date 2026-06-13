<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="LecturerDashboard.aspx.cs"
    Inherits="SIMS.Lecturer.LecturerDashboard"
    MasterPageFile="~/Lecturer/LecturerMaster.master" %>

<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <style>
        .stat-card {
            background: #fff;
            border-radius: 12px;
            padding: 20px;
            border: 1px solid #e2e8f0;
            height: 100%;
        }
        .stat-header {
            display: flex;
            align-items: center;
            margin-bottom: 12px;
        }
        .stat-icon {
            width: 44px;
            height: 44px;
            border-radius: 8px;
            display: flex;
            align-items: center;
            justify-content: center;
            margin-right: 12px;
            font-size: 18px;
        }
        .stat-label {
            margin: 0;
            color: #64748b;
            font-size: 14px;
            font-weight: 500;
        }
        .stat-value {
            font-size: 24px;
            font-weight: bold;
            color: #1e293b;
        }
        .stat-details {
            margin-top: 10px;
            padding-top: 10px;
            border-top: 1px dashed #e2e8f0;
            font-size: 12px;
            color: #475569;
            max-height: 110px;
            overflow-y: auto;
        }
        .stat-detail-item {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 4px;
        }
        .stat-detail-name {
            text-overflow: ellipsis;
            overflow: hidden;
            white-space: nowrap;
            max-width: 70%;
        }
        .stat-detail-count {
            font-weight: 600;
            background: #f1f5f9;
            padding: 1px 6px;
            border-radius: 4px;
        }
    </style>
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">

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

    <div class="row g-3 mb-4">
        <div class="col-md-3">
            <div class="stat-card">
                <div class="stat-header">
                    <div class="stat-icon" style="background:#dbeafe;">
                        <i class="fa fa-book" style="color:#1d4ed8;"></i>
                    </div>
                    <div>
                        <p class="stat-label">My Courses</p>
                        <div class="stat-value"><asp:Literal ID="litTotalCourses" runat="server" Text="0" /></div>
                    </div>
                </div>
                <div class="stat-details">
                    <asp:Repeater ID="rptCoursesDetail" runat="server">
                        <ItemTemplate>
                            <div class="stat-detail-item">
                                <span class="stat-detail-name" title='<%# Eval("CourseName") %>'><%# Eval("CourseCode") %></span>
                                <span class="stat-detail-count"><%# Eval("SemesterName") %></span>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </div>

        <div class="col-md-3">
            <div class="stat-card">
                <div class="stat-header">
                    <div class="stat-icon" style="background:#dcfce7;">
                        <i class="fa fa-users" style="color:#166534;"></i>
                    </div>
                    <div>
                        <p class="stat-label">Total Students</p>
                        <div class="stat-value"><asp:Literal ID="litTotalStudents" runat="server" Text="0" /></div>
                    </div>
                </div>
                <div class="stat-details">
                    <asp:Repeater ID="rptStudentsDetail" runat="server">
                        <ItemTemplate>
                            <div class="stat-detail-item">
                                <span class="stat-detail-name" title='<%# Eval("CourseName") %>'><%# Eval("CourseCode") %></span>
                                <span class="stat-detail-count"><%# Eval("StudentCount") %> Pax</span>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </div>

        <div class="col-md-3">
            <div class="stat-card">
                <div class="stat-header">
                    <div class="stat-icon" style="background:#fef3c7;">
                        <i class="fa fa-triangle-exclamation" style="color:#b45309;"></i>
                    </div>
                    <div>
                        <p class="stat-label">At-Risk Students</p>
                        <div class="stat-value"><asp:Literal ID="litAtRisk" runat="server" Text="0" /></div>
                    </div>
                </div>
                <div class="stat-details">
                    <asp:Repeater ID="rptAtRiskDetail" runat="server">
                        <ItemTemplate>
                            <div class="stat-detail-item">
                                <span class="stat-detail-name" title='<%# Eval("CourseName") %>'><%# Eval("CourseCode") %></span>
                                <span class="stat-detail-count style-danger" style="color:#dc2626;"><%# Eval("RiskCount") %> Risk</span>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </div>

        <div class="col-md-3">
            <div class="stat-card">
                <div class="stat-header">
                    <div class="stat-icon" style="background:#ede9fe;">
                        <i class="fa fa-clipboard-list" style="color:#6d28d9;"></i>
                    </div>
                    <div>
                        <p class="stat-label">Pending Marks</p>
                        <div class="stat-value"><asp:Literal ID="litPendingMarks" runat="server" Text="0" /></div>
                    </div>
                </div>
                <div class="stat-details">
                    <asp:Repeater ID="rptPendingDetail" runat="server">
                        <ItemTemplate>
                            <div class="stat-detail-item">
                                <span class="stat-detail-name" title='<%# Eval("CourseName") %>'><%# Eval("CourseCode") %></span>
                                <span class="stat-detail-count" style="color:#6d28d9;"><%# Eval("PendingCount") %> Item(s)</span>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </div>
    </div>

    <div class="row g-3 mb-4">
        <div class="col-md-6">
            <div class="card-sims h-100">
                <div class="card-header-sims">
                    <h5><i class="fa fa-chart-bar me-2" style="color:#0284c7;"></i>Course Average Performance</h5>
                </div>
                <div class="card-body-sims" style="padding: 20px; min-height: 260px;">
                    <canvas id="chartPerformance"></canvas>
                </div>
            </div>
        </div>

        <div class="col-md-6">
            <div class="card-sims h-100">
                <div class="card-header-sims">
                    <h5><i class="fa fa-pie-chart me-2" style="color:#e11d48;"></i>Student Risk Distribution</h5>
                </div>
                <div class="card-body-sims" style="padding: 20px; min-height: 260px;">
                    <canvas id="chartRiskDistribution"></canvas>
                </div>
            </div>
        </div>
    </div>

    <div class="row g-3 mb-4">
        <div class="col-12">
            <div class="card-sims">
                <div class="card-header-sims">
                    <h5><i class="fa fa-book me-2" style="color:#0d6efd;"></i>My Courses This Semester</h5>
                    <a href="LecturerCourses.aspx" class="btn btn-sm btn-outline-primary">View All</a>
                </div>
                <div class="card-body-sims" style="padding:0;">
                    <asp:GridView ID="gvDashboardCourses" runat="server" AutoGenerateColumns="False"
                        CssClass="table table-sims table-hover mb-0" Width="100%"
                        EmptyDataText="No courses assigned for this semester.">
                        <Columns>
                            <asp:BoundField DataField="CourseCode" HeaderText="Code" ItemStyle-Width="10%" />
                            <asp:BoundField DataField="CourseName" HeaderText="Course Name" ItemStyle-Width="30%" />
                            <asp:BoundField DataField="CreditHours" HeaderText="Credits" ItemStyle-Width="8%" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" />
                            <asp:BoundField DataField="TotalStudents" HeaderText="Students" ItemStyle-Width="10%" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" />
                            <asp:TemplateField HeaderText="Actions" ItemStyle-Width="42%" HeaderStyle-CssClass="text-end" ItemStyle-CssClass="text-end">
                                <ItemTemplate>
                                    <div style="display: inline-flex; gap: 4px; justify-content: flex-end; width: 100%;">
                                        <a href='<%# "LecturerAttendance.aspx?CourseID=" + Eval("CourseId") %>' class="btn btn-outline-primary" style="padding: 5px 10px; font-size: 11px; font-weight: 500; border-radius: 4px;"><i class="fa fa-calendar-check me-1"></i>Attendance</a>
                                        <a href='<%# "LecturerGrades.aspx?CourseID=" + Eval("CourseId") %>' class="btn btn-outline-success" style="padding: 5px 10px; font-size: 11px; font-weight: 500; border-radius: 4px;"><i class="fa fa-star me-1"></i>Grades</a>
                                        <a href='<%# "LecturerAssessment.aspx?CourseID=" + Eval("CourseId") %>' class="btn btn-outline-secondary" style="padding: 5px 10px; font-size: 11px; font-weight: 500; border-radius: 4px;"><i class="fa fa-clipboard me-1"></i>Assessments</a>
                                        <a href='<%# "LecturerMaterials.aspx?CourseID=" + Eval("CourseId") %>' class="btn btn-outline-dark" style="padding: 5px 10px; font-size: 11px; font-weight: 500; border-radius: 4px;"><i class="fa fa-upload me-1"></i>Materials</a>
                                        <a href='<%# "LecturerAnnouncements.aspx?CourseID=" + Eval("CourseId") %>' class="btn btn-outline-warning" style="padding: 5px 10px; font-size: 11px; font-weight: 500; border-radius: 4px;"><i class="fa fa-bullhorn me-1"></i>Announce</a>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>

    <div class="row g-3">
        <div class="col-12">
            <div class="card-sims">
                <div class="card-header-sims">
                    <h5><i class="fa fa-triangle-exclamation me-2" style="color:#dc2626;"></i> At-Risk Students Details 
                        <span class="badge bg-danger ms-2" style="font-size:12px;">
                            <asp:Literal ID="litAtRiskBadge" runat="server" Text="0" />
                        </span>
                    </h5>
                    <a href="LecturerStudentProgress.aspx" class="btn btn-sm btn-outline-danger">View All</a>
                </div>
                <div class="card-body-sims" style="padding:0;">
                    <asp:GridView ID="gvAtRisk" runat="server" AutoGenerateColumns="False" CssClass="table table-sims table-hover mb-0"
                        Width="100%" EmptyDataText="No at-risk students found.">
                        <Columns>
                            <asp:BoundField DataField="StudentNo" HeaderText="Student No" />
                            <asp:BoundField DataField="FullName" HeaderText="Name" />
                            <asp:BoundField DataField="CourseName" HeaderText="Course" />
                            <asp:BoundField DataField="AttendancePct" HeaderText="Attendance %" />
                            <asp:BoundField DataField="AcademicAvg" HeaderText="Academic Avg %" />
                            <asp:TemplateField HeaderText="Risk Evaluation">
                                <ItemTemplate>
                                    <span class='<%# GetRiskBadgeClass(Eval("RiskReason").ToString()) %>'>
                                        <%# Eval("RiskReason") %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        document.addEventListener('DOMContentLoaded', function () {
            var perfData = <%= PerformanceJsonData %>;
            var perfLabels = perfData.map(function (item) { return item.CourseCode; });
            var perfAverages = perfData.map(function (item) { return item.AvgMarkPct; });
            var ctxPerf = document.getElementById('chartPerformance').getContext('2d');
            new Chart(ctxPerf, {
                type: 'bar',
                data: {
                    labels: perfLabels,
                    datasets: [{
                        label: 'Average Performance (%)',
                        data: perfAverages,
                        backgroundColor: '#3b82f6',
                        borderRadius: 6
                    }]
                },
                options: { responsive: true, maintainAspectRatio: false, scales: { y: { min: 0, max: 100 } } }
            });

            var riskData = <%= RiskJsonData %>;
            var ctxRisk = document.getElementById('chartRiskDistribution').getContext('2d');
            new Chart(ctxRisk, {
                type: 'doughnut',
                data: {
                    labels: ['Low Attendance Only (<80%)', 'Low Marks Only (<50%)', 'Critical (Both Risks)'],
                    datasets: [{
                        data: [riskData.AttendanceRisk, riskData.AcademicRisk, riskData.CriticalRisk],
                        backgroundColor: ['#f59e0b', '#ef4444', '#7f1d1d']
                    }]
                },
                options: { responsive: true, maintainAspectRatio: false }
            });
        });
    </script>
</asp:Content>