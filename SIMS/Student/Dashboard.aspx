<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="SIMS.Student.Dashboard" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>SIMS - Student Dashboard</title>

    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />

    <!-- Bootstrap + Font Awesome -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" rel="stylesheet" />

    <style>
        :root {
            --sidebar-bg: #1e293b;
            --sidebar-hover: #334155;
            --sidebar-active: #0d6efd;
            --sidebar-width: 240px;
            --topbar-height: 60px;
            --body-bg: #f1f5f9;
        }

        body {
            background: var(--body-bg);
            font-family: Arial, sans-serif;
            margin: 0;
        }

        .sidebar {
            position: fixed;
            top: 0;
            left: 0;
            width: var(--sidebar-width);
            height: 100vh;
            background: var(--sidebar-bg);
            display: flex;
            flex-direction: column;
            z-index: 1000;
        }

        .sidebar-brand {
            padding: 20px 20px 16px;
            border-bottom: 1px solid #334155;
        }

        .brand-title {
            color: #fff;
            font-size: 20px;
            font-weight: bold;
            margin: 0;
        }

        .brand-sub {
            color: #94a3b8;
            font-size: 12px;
        }

        .sidebar-user {
            padding: 16px 20px;
            border-bottom: 1px solid #334155;
        }

        .user-name {
            color: #fff;
            font-size: 14px;
            font-weight: bold;
            margin: 0;
        }

        .user-role {
            color: #94a3b8;
            font-size: 12px;
        }

        .sidebar-nav {
            flex: 1;
            padding: 12px 0;
        }

        .nav-label {
            color: #64748b;
            font-size: 11px;
            font-weight: bold;
            text-transform: uppercase;
            letter-spacing: 0.08em;
            padding: 10px 20px 4px;
        }

        .sidebar-nav a {
            display: flex;
            align-items: center;
            gap: 10px;
            padding: 10px 20px;
            color: #cbd5e1;
            text-decoration: none;
            font-size: 14px;
            border-left: 3px solid transparent;
            transition: all 0.15s;
        }

        .sidebar-nav a:hover {
            background: var(--sidebar-hover);
            color: #fff;
        }

        .sidebar-nav a.active {
            background: rgba(13,110,253,0.15);
            border-left-color: var(--sidebar-active);
            color: #60a5fa;
        }

        .sidebar-nav a i {
            width: 18px;
            text-align: center;
            font-size: 15px;
        }

        .sidebar-footer {
            padding: 16px 20px;
            border-top: 1px solid #334155;
        }

        .btn-logout {
            display: flex;
            align-items: center;
            gap: 8px;
            width: 100%;
            padding: 9px 14px;
            background: #dc2626;
            color: #fff;
            border: none;
            border-radius: 8px;
            font-size: 14px;
            cursor: pointer;
            text-decoration: none;
            justify-content: center;
        }

        .btn-logout:hover {
            background: #b91c1c;
            color: #fff;
        }

        .main-wrapper {
            margin-left: var(--sidebar-width);
            min-height: 100vh;
            display: flex;
            flex-direction: column;
        }

        .topbar {
            height: var(--topbar-height);
            background: #fff;
            border-bottom: 1px solid #e2e8f0;
            display: flex;
            align-items: center;
            padding: 0 28px;
            position: sticky;
            top: 0;
            z-index: 900;
        }

        .topbar-title {
            font-size: 17px;
            font-weight: bold;
            color: #1e293b;
            margin: 0;
        }

        .topbar-right {
            margin-left: auto;
            display: flex;
            align-items: center;
            gap: 14px;
        }

        .page-content {
            padding: 28px;
            flex: 1;
        }

        .welcome-card {
            background: #fff;
            border-radius: 12px;
            padding: 24px 28px;
            border: 1px solid #e2e8f0;
            margin-bottom: 24px;
            display: flex;
            align-items: center;
            justify-content: space-between;
            box-shadow: 0 1px 4px rgba(0,0,0,0.06);
        }

        .card-sims {
            background: #fff;
            border-radius: 12px;
            box-shadow: 0 1px 4px rgba(0,0,0,0.08);
            border: 1px solid #e2e8f0;
        }

        .card-header-sims {
            padding: 18px 24px;
            border-bottom: 1px solid #e2e8f0;
            display: flex;
            align-items: center;
            justify-content: space-between;
        }

        .card-header-sims h5 {
            font-size: 16px;
            font-weight: bold;
            color: #1e293b;
            margin: 0;
        }

        .card-body-sims {
            padding: 24px;
        }

        .stat-card {
            background: #fff;
            border-radius: 12px;
            padding: 20px 24px;
            border: 1px solid #e2e8f0;
            box-shadow: 0 1px 4px rgba(0,0,0,0.06);
            display: flex;
            align-items: center;
            gap: 16px;
        }

        .stat-icon {
            width: 48px;
            height: 48px;
            border-radius: 10px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 20px;
            flex-shrink: 0;
        }

        .stat-label {
            font-size: 13px;
            color: #64748b;
            margin: 0;
        }

        .stat-value {
            font-size: 26px;
            font-weight: bold;
            color: #1e293b;
            line-height: 1;
        }

        .quick-link {
            padding: 14px 16px;
            border-radius: 10px;
            text-decoration: none;
            display: flex;
            align-items: center;
            gap: 10px;
            font-size: 14px;
        }
    </style>
</head>

<body>
    <form id="form1" runat="server">

        <!-- Sidebar -->
        <div class="sidebar">

            <div class="sidebar-brand">
                <p class="brand-title">🎓 SIMS</p>
                <span class="brand-sub">Student Information System</span>
            </div>

            <div class="sidebar-user">
                <p class="user-name">
                    <asp:Label ID="lblWelcome" runat="server"></asp:Label>
                </p>
                <span class="user-role">
                    <asp:Label ID="lblStudentNo" runat="server"></asp:Label>
                </span>
            </div>

            <div class="sidebar-nav">

                <div class="nav-label">Main</div>

                <a href="Dashboard.aspx" class="active">
                    <i class="fa fa-gauge"></i> Dashboard
                </a>

                <a href="Profile.aspx">
                    <i class="fa fa-user"></i> My Profile
                </a>

                <div class="nav-label">Academic</div>

                <a href="EnrolledCourses.aspx">
                    <i class="fa fa-book"></i> Enrolled Courses
                </a>

                <a href="CourseRegistration.aspx">
                    <i class="fa fa-pen-to-square"></i> Course Registration
                </a>

                <a href="AttendanceRecord.aspx">
                    <i class="fa fa-calendar-check"></i> Attendance Record
                </a>

                <a href="AcademicResults.aspx">
                    <i class="fa fa-square-poll-vertical"></i> Academic Results
                </a>

                <div class="nav-label">Updates</div>

                <a href="Notifications.aspx">
                    <i class="fa fa-bell"></i> Notifications
                </a>

            </div>

            <div class="sidebar-footer">
                <asp:LinkButton 
                    ID="btnLogout" 
                    runat="server" 
                    CssClass="btn-logout" 
                    OnClick="btnLogout_Click">
                    <i class="fa fa-right-from-bracket"></i> Logout
                </asp:LinkButton>
            </div>

        </div>

        <!-- Main wrapper -->
        <div class="main-wrapper">

            <!-- Topbar -->
            <div class="topbar">
                <p class="topbar-title">Student Portal</p>

                <div class="topbar-right">
                    <a href="Notifications.aspx" style="color:#64748b; font-size:20px;">
                        <i class="fa fa-bell"></i>
                    </a>
                    <span style="color:#64748b; font-size:14px;">
                        Student
                    </span>
                </div>
            </div>

            <!-- Page content -->
            <div class="page-content">

                <!-- Welcome banner -->
                <div class="welcome-card">
                    <div>
                        <h4 style="margin:0; color:#1e293b; font-weight:bold;">
                            Welcome to your dashboard 👋
                        </h4>
                        <p style="margin:4px 0 0; color:#64748b; font-size:14px;">
                            View your courses, attendance, results, and academic updates here.
                        </p>
                    </div>

                    <div style="text-align:right; color:#64748b; font-size:13px;">
                        <i class="fa fa-calendar"></i>
                        <%= DateTime.Now.ToString("dddd, dd MMMM yyyy") %>
                    </div>
                </div>

                <!-- Stat cards -->
                <div class="row g-3 mb-4">

                    <div class="col-md-3">
                        <div class="stat-card">
                            <div class="stat-icon" style="background:#dbeafe;">
                                <i class="fa fa-layer-group" style="color:#1d4ed8;"></i>
                            </div>
                            <div>
                                <p class="stat-label">Current Semester</p>
                                <div class="stat-value">
                                    <asp:Label ID="lblCurrentSemester" runat="server" Text="3"></asp:Label>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="col-md-3">
                        <div class="stat-card">
                            <div class="stat-icon" style="background:#dcfce7;">
                                <i class="fa fa-book" style="color:#166534;"></i>
                            </div>
                            <div>
                                <p class="stat-label">Enrolled Courses</p>
                                <div class="stat-value">
                                    <asp:Label ID="lblEnrolledCourses" runat="server" Text="0"></asp:Label>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="col-md-3">
                        <div class="stat-card">
                            <div class="stat-icon" style="background:#fef3c7;">
                                <i class="fa fa-graduation-cap" style="color:#b45309;"></i>
                            </div>
                            <div>
                                <p class="stat-label">CGPA</p>
                                <div class="stat-value">
                                    <asp:Label ID="lblCGPA" runat="server" Text="0.00"></asp:Label>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="col-md-3">
                        <div class="stat-card">
                            <div class="stat-icon" style="background:#ede9fe;">
                                <i class="fa fa-bell" style="color:#6d28d9;"></i>
                            </div>
                            <div>
                                <p class="stat-label">Notifications</p>
                                <div class="stat-value">
                                    <asp:Label ID="lblNotifications" runat="server" Text="0"></asp:Label>
                                </div>
                            </div>
                        </div>
                    </div>

                </div>

                <div class="row g-3">

                    <!-- Student overview -->
                    <div class="col-md-8">
                        <div class="card-sims">
                            <div class="card-header-sims">
                                <h5>
                                    <i class="fa fa-id-card me-2" style="color:#0d6efd;"></i>
                                    Student Overview
                                </h5>
                            </div>

                            <div class="card-body-sims">
                                <p style="color:#64748b; margin-bottom:18px;">
                                    Use the dashboard shortcuts to access your academic information quickly.
                                </p>

                                <div class="row g-3">
                                    <div class="col-md-6">
                                        <a href="Profile.aspx" class="quick-link bg-light text-dark border">
                                            <i class="fa fa-user text-primary"></i>
                                            View My Profile
                                        </a>
                                    </div>

                                    <div class="col-md-6">
                                        <a href="EnrolledCourses.aspx" class="quick-link bg-light text-dark border">
                                            <i class="fa fa-book text-success"></i>
                                            View Enrolled Courses
                                        </a>
                                    </div>

                                    <div class="col-md-6">
                                        <a href="AttendanceRecord.aspx" class="quick-link bg-light text-dark border">
                                            <i class="fa fa-calendar-check text-warning"></i>
                                            View Attendance Record
                                        </a>
                                    </div>

                                    <div class="col-md-6">
                                        <a href="AcademicResults.aspx" class="quick-link bg-light text-dark border">
                                            <i class="fa fa-square-poll-vertical text-danger"></i>
                                            View Academic Results
                                        </a>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- Quick actions -->
                    <div class="col-md-4">
                        <div class="card-sims h-100">
                            <div class="card-header-sims">
                                <h5>
                                    <i class="fa fa-bolt me-2" style="color:#f59e0b;"></i>
                                    Quick Actions
                                </h5>
                            </div>

                            <div class="card-body-sims">
                                <div class="d-grid gap-2">
                                    <a href="CourseRegistration.aspx" class="btn btn-outline-primary text-start">
                                        <i class="fa fa-pen-to-square me-2"></i> Register Course
                                    </a>

                                    <a href="EnrolledCourses.aspx" class="btn btn-outline-success text-start">
                                        <i class="fa fa-book me-2"></i> My Enrolled Courses
                                    </a>

                                    <a href="AcademicResults.aspx" class="btn btn-outline-warning text-start">
                                        <i class="fa fa-graduation-cap me-2"></i> Check Results
                                    </a>

                                    <a href="Notifications.aspx" class="btn btn-outline-secondary text-start">
                                        <i class="fa fa-bell me-2"></i> View Notifications
                                    </a>
                                </div>
                            </div>
                        </div>
                    </div>

                </div>

            </div>

        </div>

        <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>

    </form>
</body>
</html>