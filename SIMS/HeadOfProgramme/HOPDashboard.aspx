<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="HOPDashboard.aspx.cs"
    Inherits="SIMS.HeadOfProgramme.Dashboard"
    MasterPageFile="~/HeadOfProgramme/HOPMaster.master" %>

<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>

    <style>
        .analytics-hero {
            background: linear-gradient(135deg, #0f172a, #1d4ed8);
            border-radius: 18px;
            padding: 26px 30px;
            color: #fff;
            margin-bottom: 24px;
            box-shadow: 0 18px 40px rgba(15, 23, 42, .18);
        }

        .analytics-hero h3 {
            margin: 0;
            font-weight: 800;
            letter-spacing: -.02em;
        }

        .analytics-hero p {
            margin: 8px 0 0;
            color: #dbeafe;
            max-width: 760px;
        }

        .dashboard-date {
            background: rgba(255,255,255,.14);
            border: 1px solid rgba(255,255,255,.22);
            padding: 10px 14px;
            border-radius: 12px;
            color: #eff6ff;
            font-size: 13px;
            white-space: nowrap;
        }

        .stat-card {
            background: #fff;
            border: 1px solid #e2e8f0;
            border-radius: 16px;
            padding: 18px;
            min-height: 116px;
            display: flex;
            align-items: center;
            gap: 14px;
            box-shadow: 0 10px 24px rgba(15, 23, 42, .05);
            transition: .2s ease;
        }

        .stat-card:hover {
            transform: translateY(-3px);
            box-shadow: 0 16px 35px rgba(15, 23, 42, .10);
        }

        .stat-icon {
            width: 52px;
            height: 52px;
            border-radius: 14px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 22px;
            flex-shrink: 0;
        }

        .stat-label {
            color: #64748b;
            font-size: 13px;
            margin: 0 0 3px;
            font-weight: 600;
        }

        .stat-value {
            color: #0f172a;
            font-size: 28px;
            font-weight: 800;
            line-height: 1;
        }

        .stat-note {
            color: #94a3b8;
            font-size: 12px;
            margin-top: 5px;
        }

        .card-sims {
            background: #fff;
            border: 1px solid #e2e8f0;
            border-radius: 16px;
            box-shadow: 0 10px 24px rgba(15, 23, 42, .05);
            overflow: hidden;
        }

        .card-header-sims {
            padding: 16px 18px;
            border-bottom: 1px solid #e2e8f0;
            display: flex;
            align-items: center;
            justify-content: space-between;
            gap: 12px;
        }

        .card-header-sims h5 {
            margin: 0;
            font-weight: 800;
            color: #1e293b;
        }

        .card-body-sims {
            padding: 18px;
        }

        .chart-box {
            height: 315px;
            position: relative;
        }

        .report-card {
            border: 1px solid #e2e8f0;
            border-radius: 14px;
            padding: 14px 16px;
            display: flex;
            justify-content: space-between;
            gap: 12px;
            align-items: center;
            background: #f8fafc;
            margin-bottom: 12px;
        }

        .report-card h6 {
            margin: 0;
            font-weight: 800;
            color: #1e293b;
        }

        .report-card p {
            margin: 4px 0 0;
            color: #64748b;
            font-size: 13px;
        }

        .badge-soft {
            display: inline-block;
            padding: 6px 10px;
            border-radius: 999px;
            background: #e0f2fe;
            color: #0369a1;
            font-size: 12px;
            font-weight: 700;
        }

        .table-sims {
            width: 100%;
            border-collapse: collapse;
        }

        .table-sims th {
            background: #f8fafc;
            color: #475569;
            font-size: 12px;
            text-transform: uppercase;
            letter-spacing: .04em;
            padding: 11px;
            border-bottom: 1px solid #e2e8f0;
        }

        .table-sims td {
            padding: 11px;
            border-bottom: 1px solid #f1f5f9;
            color: #334155;
            font-size: 14px;
        }

        .quick-action {
            display: block;
            text-decoration: none;
            color: #0f172a;
            background: #f8fafc;
            border: 1px solid #e2e8f0;
            border-radius: 13px;
            padding: 13px 14px;
            margin-bottom: 10px;
            transition: .2s ease;
            font-weight: 700;
        }

        .quick-action:hover {
            background: #eff6ff;
            border-color: #93c5fd;
            transform: translateX(3px);
        }


        .filter-panel {
            background: #fff;
            border: 1px solid #e2e8f0;
            border-radius: 16px;
            padding: 16px 18px;
            margin-bottom: 24px;
            box-shadow: 0 10px 24px rgba(15, 23, 42, .05);
        }

        .filter-label {
            font-size: 12px;
            font-weight: 800;
            color: #64748b;
            text-transform: uppercase;
            letter-spacing: .04em;
            margin-bottom: 6px;
        }

        .filter-input {
            width: 100%;
            border: 1px solid #cbd5e1;
            border-radius: 10px;
            padding: 10px 12px;
            color: #0f172a;
            background: #fff;
        }

        .btn-sims-primary {
            border: 0;
            border-radius: 10px;
            padding: 10px 16px;
            background: #2563eb;
            color: #fff !important;
            font-weight: 800;
            text-decoration: none;
            display: inline-block;
        }

        .btn-sims-primary:hover {
            background: #1d4ed8;
            color: #fff !important;
        }

        .btn-report {
            border: 0;
            background: transparent;
            color: #2563eb !important;
            font-weight: 800;
            text-decoration: none;
            padding: 0;
        }

        .empty-note {
            color: #94a3b8;
            font-size: 13px;
            padding: 12px;
            background: #f8fafc;
            border-radius: 12px;
            border: 1px dashed #cbd5e1;
        }

        .dashboard-section-title {
            font-size: 14px;
            font-weight: 900;
            color: #334155;
            text-transform: uppercase;
            letter-spacing: .05em;
            margin: 8px 0 12px;
        }

        .report-message {
            display: block;
            margin: 8px 0 12px;
            padding: 10px 12px;
            border-radius: 12px;
            background: #f8fafc;
            border: 1px dashed #cbd5e1;
            color: #64748b;
            font-size: 13px;
        }
    </style>
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">

    <asp:HiddenField ID="hfProgrammeLabelsJson" runat="server" />
    <asp:HiddenField ID="hfProgrammeDataJson" runat="server" />
    <asp:HiddenField ID="hfStatusLabelsJson" runat="server" />
    <asp:HiddenField ID="hfStatusDataJson" runat="server" />
    <asp:HiddenField ID="hfAttendanceLabelsJson" runat="server" />
    <asp:HiddenField ID="hfAttendanceDataJson" runat="server" />
    <asp:HiddenField ID="hfPerformanceLabelsJson" runat="server" />
    <asp:HiddenField ID="hfPerformanceDataJson" runat="server" />

    <div class="analytics-hero">
        <div class="d-flex align-items-center justify-content-between flex-wrap gap-3">
            <div>
                <h3><i class="fa fa-chart-line me-2"></i> Interactive Admin Analytics Dashboard</h3>
                <p>
                    Monitor student statistics, academic performance summaries, enrolment trends,
                    attendance summaries, and institutional reporting from one dashboard.
                </p>
            </div>

            <div class="dashboard-date">
                <i class="fa fa-calendar me-2"></i>
                <%= DateTime.Now.ToString("dddd, dd MMM yyyy") %>
            </div>
        </div>
    </div>

    <div class="filter-panel">
        <div class="row g-3 align-items-end">
            <div class="col-md-4">
                <div class="filter-label">Academic Year</div>
                <asp:DropDownList ID="ddlAcademicYear" runat="server" CssClass="filter-input" />
            </div>
            <div class="col-md-4">
                <div class="filter-label">Semester</div>
                <asp:DropDownList ID="ddlSemester" runat="server" CssClass="filter-input">
                    <asp:ListItem Value="">All Semesters</asp:ListItem>
                    <asp:ListItem Value="1">Semester 1</asp:ListItem>
                    <asp:ListItem Value="2">Semester 2</asp:ListItem>
                    <asp:ListItem Value="3">Semester 3</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="col-md-4">
                <asp:LinkButton ID="btnApplyFilter" runat="server" CssClass="btn-sims-primary" OnClick="btnApplyFilter_Click">
                    <i class="fa fa-filter me-2"></i> Apply Dashboard Filter
                </asp:LinkButton>
            </div>
        </div>
    </div>

    <div class="dashboard-section-title">Overall System Overview - not affected by academic year / semester filter</div>
    <div class="row g-3 mb-4">
        <div class="col-md-3">
            <div class="stat-card">
                <div class="stat-icon" style="background:#dbeafe;">
                    <i class="fa fa-layer-group" style="color:#2563eb;"></i>
                </div>
                <div>
                    <p class="stat-label">Programmes</p>
                    <div class="stat-value"><asp:Literal ID="litProgrammesCount" runat="server" Text="0" /></div>
                    <div class="stat-note">Academic programmes</div>
                </div>
            </div>
        </div>

        <div class="col-md-3">
            <div class="stat-card">
                <div class="stat-icon" style="background:#dcfce7;">
                    <i class="fa fa-book" style="color:#15803d;"></i>
                </div>
                <div>
                    <p class="stat-label">Courses</p>
                    <div class="stat-value"><asp:Literal ID="litCoursesCount" runat="server" Text="0" /></div>
                    <div class="stat-note">Available courses</div>
                </div>
            </div>
        </div>

        <div class="col-md-3">
            <div class="stat-card">
                <div class="stat-icon" style="background:#fef3c7;">
                    <i class="fa fa-user-graduate" style="color:#ca8a04;"></i>
                </div>
                <div>
                    <p class="stat-label">Students</p>
                    <div class="stat-value"><asp:Literal ID="litStudentsCount" runat="server" Text="0" /></div>
                    <div class="stat-note">Registered students</div>
                </div>
            </div>
        </div>

        <div class="col-md-3">
            <div class="stat-card">
                <div class="stat-icon" style="background:#ede9fe;">
                    <i class="fa fa-chalkboard-teacher" style="color:#7c3aed;"></i>
                </div>
                <div>
                    <p class="stat-label">Lecturers</p>
                    <div class="stat-value"><asp:Literal ID="litLecturersCount" runat="server" Text="0" /></div>
                    <div class="stat-note">Teaching staff</div>
                </div>
            </div>
        </div>
    </div>

    <div class="dashboard-section-title">Filtered Academic Metrics - affected by selected academic year / semester</div>
    <div class="row g-3 mb-4">
        <div class="col-md-3">
            <div class="stat-card">
                <div class="stat-icon" style="background:#cffafe;">
                    <i class="fa fa-clipboard-list" style="color:#0891b2;"></i>
                </div>
                <div>
                    <p class="stat-label">Total Enrolments</p>
                    <div class="stat-value"><asp:Literal ID="litEnrolmentsCount" runat="server" Text="0" /></div>
                    <div class="stat-note">Institutional enrolment records</div>
                </div>
            </div>
        </div>

        <div class="col-md-3">
            <div class="stat-card">
                <div class="stat-icon" style="background:#fee2e2;">
                    <i class="fa fa-user-check" style="color:#dc2626;"></i>
                </div>
                <div>
                    <p class="stat-label">Active Students</p>
                    <div class="stat-value"><asp:Literal ID="litActiveStudentsCount" runat="server" Text="0" /></div>
                    <div class="stat-note">Students with active status</div>
                </div>
            </div>
        </div>

        <div class="col-md-3">
            <div class="stat-card">
                <div class="stat-icon" style="background:#fce7f3;">
                    <i class="fa fa-calendar-check" style="color:#db2777;"></i>
                </div>
                <div>
                    <p class="stat-label">Attendance Rate</p>
                    <div class="stat-value"><asp:Literal ID="litAttendanceRate" runat="server" Text="0%" /></div>
                    <div class="stat-note">Present / total attendance</div>
                </div>
            </div>
        </div>

        <div class="col-md-3">
            <div class="stat-card">
                <div class="stat-icon" style="background:#dcfce7;">
                    <i class="fa fa-chart-simple" style="color:#16a34a;"></i>
                </div>
                <div>
                    <p class="stat-label">Average Performance</p>
                    <div class="stat-value"><asp:Literal ID="litAveragePerformance" runat="server" Text="N/A" /></div>
                    <div class="stat-note">Based on available marks/grades</div>
                </div>
            </div>
        </div>
    </div>

    <div class="row g-3 mb-4">
        <div class="col-md-3">
            <div class="stat-card">
                <div class="stat-icon" style="background:#fee2e2;">
                    <i class="fa fa-triangle-exclamation" style="color:#dc2626;"></i>
                </div>
                <div>
                    <p class="stat-label">Students At Risk</p>
                    <div class="stat-value"><asp:Literal ID="litStudentsAtRisk" runat="server" Text="0" /></div>
                    <div class="stat-note">CGPA below 2.50</div>
                </div>
            </div>
        </div>
    </div>

    <div class="row g-3 mb-4">
        <div class="col-lg-8">
            <div class="card-sims h-100">
                <div class="card-header-sims">
                    <h5><i class="fa fa-chart-column me-2 text-primary"></i> Enrolment Statistics by Programme</h5>
                    <span class="badge-soft">Institutional Report</span>
                </div>
                <div class="card-body-sims">
                    <div class="chart-box">
                        <canvas id="programmeChart"></canvas>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-4">
            <div class="card-sims h-100">
                <div class="card-header-sims">
                    <h5><i class="fa fa-circle-nodes me-2 text-success"></i> Course Enrolment Summary</h5>
                </div>
                <div class="card-body-sims">
                    <div class="chart-box">
                        <canvas id="statusChart"></canvas>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="row g-3 mb-4">
        <div class="col-lg-6">
            <div class="card-sims h-100">
                <div class="card-header-sims">
                    <h5><i class="fa fa-calendar-days me-2 text-danger"></i> Attendance Summary</h5>
                    <span class="badge-soft">Present / Absent / Late</span>
                </div>
                <div class="card-body-sims">
                    <div class="chart-box">
                        <canvas id="attendanceChart"></canvas>
                    </div>
                    <asp:Panel ID="pnlAttendanceNote" runat="server" CssClass="empty-note mt-3" Visible="false">
                        Attendance table or status data was not found yet. Once attendance records are added, this chart will update.
                    </asp:Panel>
                </div>
            </div>
        </div>

        <div class="col-lg-6">
            <div class="card-sims h-100">
                <div class="card-header-sims">
                    <h5><i class="fa fa-award me-2 text-warning"></i> Student Performance Summary</h5>
                    <span class="badge-soft">Average marks / grade distribution</span>
                </div>
                <div class="card-body-sims">
                    <div class="chart-box">
                        <canvas id="performanceChart"></canvas>
                    </div>
                    <asp:Panel ID="pnlPerformanceNote" runat="server" CssClass="empty-note mt-3" Visible="false">
                        Performance data was not found yet. Add marks or grades to your enrolment/result table to activate this report.
                    </asp:Panel>
                </div>
            </div>
        </div>
    </div>

    <div class="row g-3">
        <div class="col-lg-8">
            <div class="card-sims">
                <div class="card-header-sims">
                    <h5><i class="fa fa-file-lines me-2 text-primary"></i> Institutional Reports</h5>
                    <span class="badge-soft">Generated from database</span>
                </div>
                <div class="card-body-sims">
                    <div class="row">
                        <div class="col-md-4">
                            <div class="report-card">
                                <div>
                                    <h6>Enrolment Statistics</h6>
                                    <p>Total students by programme and active enrolment records.</p>
                                    <asp:LinkButton ID="btnEnrolmentReport" runat="server" CssClass="btn-report" OnClick="btnEnrolmentReport_Click">
                                        Generate Report
                                    </asp:LinkButton>
                                </div>
                                <i class="fa fa-users text-primary"></i>
                            </div>
                        </div>

                        <div class="col-md-4">
                            <div class="report-card">
                                <div>
                                    <h6>Student Performance</h6>
                                    <p>CGPA, GPA, and at-risk student performance summary.</p>
                                    <asp:LinkButton ID="btnPerformanceReport" runat="server" CssClass="btn-report" OnClick="btnPerformanceReport_Click">
                                        Generate Report
                                    </asp:LinkButton>
                                </div>
                                <i class="fa fa-chart-line text-success"></i>
                            </div>
                        </div>

                        <div class="col-md-4">
                            <div class="report-card">
                                <div>
                                    <h6>Attendance Summary</h6>
                                    <p>Present, absent, and late records across students and courses.</p>
                                    <asp:LinkButton ID="btnAttendanceReport" runat="server" CssClass="btn-report" OnClick="btnAttendanceReport_Click">
                                        Generate Report
                                    </asp:LinkButton>
                                </div>
                                <i class="fa fa-calendar-check text-danger"></i>
                            </div>
                        </div>
                    </div>

                    <h6 class="mt-3 mb-2 fw-bold">Top Programme Enrolment Report</h6>
                    <asp:GridView ID="gvProgrammeReport" runat="server" AutoGenerateColumns="False"
                        CssClass="table-sims" GridLines="None" EmptyDataText="No programme report data found.">
                        <Columns>
                            <asp:BoundField DataField="ProgrammeName" HeaderText="Programme" />
                            <asp:BoundField DataField="TotalStudents" HeaderText="Students" />
                            <asp:BoundField DataField="ActiveStudents" HeaderText="Active" />
                        </Columns>
                    </asp:GridView>

                    <h6 class="mt-4 mb-2 fw-bold">
                        <asp:Literal ID="litGeneratedReportTitle" runat="server" Text="Generated Report" />
                    </h6>
                    <asp:Label ID="litGeneratedReportMessage" runat="server" CssClass="report-message" Text="Choose a report above to view institutional data for the selected filter." />
                    <asp:GridView ID="gvGeneratedReport" runat="server" AutoGenerateColumns="True"
                        CssClass="table-sims mt-2" GridLines="None" EmptyDataText="Click a report button above to generate a report.">
                        <EmptyDataRowStyle CssClass="empty-note" />
                    </asp:GridView>
                </div>
            </div>
        </div>

        <div class="col-lg-4">
            <div class="card-sims h-100">
                <div class="card-header-sims">
                    <h5><i class="fa fa-bolt me-2 text-warning"></i> Quick Actions</h5>
                </div>

                <div class="card-body-sims">
                    <a href="HOPManageProgrammes.aspx" class="quick-action">
                        <i class="fa fa-layer-group me-2"></i> Manage Programmes
                    </a>

                    <a href="HOPManageCourses.aspx" class="quick-action">
                        <i class="fa fa-book me-2"></i> Manage Courses
                    </a>

                    <a href="HOPRegisterStudent.aspx" class="quick-action">
                        <i class="fa fa-user-plus me-2"></i> Register Student
                    </a>

                    <a href="HOPManageEnrolments.aspx" class="quick-action">
                        <i class="fa fa-clipboard-list me-2"></i> Manage Enrolments
                    </a>

                    <a href="HOPManageAdmissions.aspx" class="quick-action">
                        <i class="fa fa-user-check me-2"></i> Manage Admissions
                    </a>

                    <a href="HOPManageAcademicCalendar.aspx" class="quick-action">
                        <i class="fa fa-calendar-days me-2"></i> Academic Calendar
                    </a>
                </div>
            </div>
        </div>
    </div>

    <script>
        function readJsonFromLiteral(id, fallback) {
            var el = document.getElementById(id);
            if (!el) return fallback;

            try {
                return JSON.parse(el.value || el.innerText || el.textContent || "[]");
            } catch (e) {
                return fallback;
            }
        }

        var programmeLabels = readJsonFromLiteral("<%= hfProgrammeLabelsJson.ClientID %>", []);
        var programmeData = readJsonFromLiteral("<%= hfProgrammeDataJson.ClientID %>", []);
        var courseEnrolmentLabels = readJsonFromLiteral("<%= hfStatusLabelsJson.ClientID %>", []);
        var courseEnrolmentData = readJsonFromLiteral("<%= hfStatusDataJson.ClientID %>", []);
        var attendanceLabels = readJsonFromLiteral("<%= hfAttendanceLabelsJson.ClientID %>", []);
        var attendanceData = readJsonFromLiteral("<%= hfAttendanceDataJson.ClientID %>", []);
        var performanceLabels = readJsonFromLiteral("<%= hfPerformanceLabelsJson.ClientID %>", []);
        var performanceData = readJsonFromLiteral("<%= hfPerformanceDataJson.ClientID %>", []);

        Chart.defaults.font.family = "'Segoe UI', Arial, sans-serif";
        Chart.defaults.color = "#475569";

        new Chart(document.getElementById("programmeChart"), {
            type: "bar",
            data: {
                labels: programmeLabels,
                datasets: [{
                    label: "Students",
                    data: programmeData,
                    backgroundColor: "rgba(37, 99, 235, .75)",
                    borderColor: "rgba(37, 99, 235, 1)",
                    borderWidth: 1,
                    borderRadius: 8
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }
            }
        });

        new Chart(document.getElementById("statusChart"), {
            type: "doughnut",
            data: {
                labels: courseEnrolmentLabels,
                datasets: [{
                    data: courseEnrolmentData,
                    backgroundColor: [
                        "rgba(22, 163, 74, .78)",
                        "rgba(220, 38, 38, .78)",
                        "rgba(245, 158, 11, .78)",
                        "rgba(100, 116, 139, .78)"
                    ],
                    borderWidth: 2
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { position: "bottom" } }
            }
        });

        new Chart(document.getElementById("attendanceChart"), {
            type: "bar",
            data: {
                labels: attendanceLabels,
                datasets: [{
                    label: "Attendance Records",
                    data: attendanceData,
                    backgroundColor: [
                        "rgba(22, 163, 74, .75)",
                        "rgba(220, 38, 38, .75)",
                        "rgba(245, 158, 11, .75)",
                        "rgba(100, 116, 139, .75)"
                    ],
                    borderRadius: 8
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }
            }
        });

        new Chart(document.getElementById("performanceChart"), {
            type: "bar",
            data: {
                labels: performanceLabels,
                datasets: [{
                    label: "Performance",
                    data: performanceData,
                    backgroundColor: "rgba(14, 165, 233, .75)",
                    borderColor: "rgba(14, 165, 233, 1)",
                    borderWidth: 1,
                    borderRadius: 8
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: { y: { beginAtZero: true } }
            }
        });
    </script>

</asp:Content>
