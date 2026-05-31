<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="SIMS.Student.Profile" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>SIMS - My Profile</title>

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

        .profile-hero {
            background: #fff;
            border-radius: 12px;
            padding: 28px;
            border: 1px solid #e2e8f0;
            margin-bottom: 24px;
            box-shadow: 0 1px 4px rgba(0,0,0,0.06);
            display: flex;
            align-items: center;
            gap: 20px;
        }

        .profile-avatar {
            width: 78px;
            height: 78px;
            border-radius: 50%;
            background: #dbeafe;
            display: flex;
            align-items: center;
            justify-content: center;
            color: #1d4ed8;
            font-size: 34px;
            flex-shrink: 0;
        }

        .profile-name {
            margin: 0;
            font-size: 24px;
            font-weight: bold;
            color: #1e293b;
        }

        .profile-sub {
            color: #64748b;
            font-size: 14px;
            margin-top: 4px;
        }

        .info-row {
            padding: 14px 0;
            border-bottom: 1px solid #e2e8f0;
        }

        .info-row:last-child {
            border-bottom: none;
        }

        .info-label {
            font-size: 13px;
            color: #64748b;
            margin-bottom: 4px;
        }

        .info-value {
            font-size: 15px;
            color: #1e293b;
            font-weight: 600;
        }

        .status-badge {
            display: inline-block;
            padding: 5px 12px;
            border-radius: 999px;
            background: #dcfce7;
            color: #166534;
            font-size: 13px;
            font-weight: bold;
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
                    Welcome, <asp:Label ID="lblSideName" runat="server"></asp:Label>
                </p>
                <span class="user-role">
                    Student No: <asp:Label ID="lblSideStudentNo" runat="server"></asp:Label>
                </span>
            </div>

            <div class="sidebar-nav">

                <div class="nav-label">Main</div>

                <a href="Dashboard.aspx">
                    <i class="fa fa-gauge"></i> Dashboard
                </a>

                <a href="Profile.aspx" class="active">
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
                <p class="topbar-title">My Profile</p>

                <div class="topbar-right">
                    <a href="Notifications.aspx" style="color:#64748b; font-size:20px;">
                        <i class="fa fa-bell"></i>
                    </a>
                    <span style="color:#64748b; font-size:14px;">Student</span>
                </div>
            </div>

            <!-- Page content -->
            <div class="page-content">

                <!-- Profile Hero -->
                <div class="profile-hero">
                    <div class="profile-avatar">
                        <i class="fa fa-user-graduate"></i>
                    </div>

                    <div>
                        <h3 class="profile-name">
                            <asp:Label ID="lblFullName" runat="server"></asp:Label>
                        </h3>

                        <div class="profile-sub">
                            <asp:Label ID="lblStudentNo" runat="server"></asp:Label>
                            ·
                            <asp:Label ID="lblProgrammeCode" runat="server"></asp:Label>
                        </div>

                        <div class="mt-2">
                            <span class="status-badge">
                                <asp:Label ID="lblStatus" runat="server"></asp:Label>
                            </span>
                        </div>
                    </div>
                </div>

                <div class="row g-3">

                    <!-- Personal Information -->
                    <div class="col-md-6">
                        <div class="card-sims h-100">
                            <div class="card-header-sims">
                                <h5>
                                    <i class="fa fa-id-card me-2" style="color:#0d6efd;"></i>
                                    Personal Information
                                </h5>
                            </div>

                            <div class="card-body-sims">

                                <div class="info-row">
                                    <div class="info-label">Full Name</div>
                                    <div class="info-value">
                                        <asp:Label ID="lblPersonalName" runat="server"></asp:Label>
                                    </div>
                                </div>

                                <div class="info-row">
                                    <div class="info-label">Email Address</div>
                                    <div class="info-value">
                                        <asp:Label ID="lblEmail" runat="server"></asp:Label>
                                    </div>
                                </div>

                                <div class="info-row">
                                    <div class="info-label">Phone Number</div>

                                    <div class="input-group mt-2">
                                        <asp:TextBox 
                                            ID="txtPhone" 
                                            runat="server" 
                                            CssClass="form-control" 
                                            MaxLength="20">
                                        </asp:TextBox>

                                        <asp:Button 
                                            ID="btnUpdatePhone" 
                                            runat="server" 
                                            Text="Update" 
                                            CssClass="btn btn-primary" 
                                            OnClick="btnUpdatePhone_Click" />
                                    </div>

                                    <asp:Label 
                                        ID="lblPhoneMessage" 
                                        runat="server" 
                                        CssClass="d-block mt-2">
                                    </asp:Label>
                                </div>

                                <div class="info-row">
                                    <div class="info-label">Student Number</div>
                                    <div class="info-value">
                                        <asp:Label ID="lblPersonalStudentNo" runat="server"></asp:Label>
                                    </div>
                                </div>

                            </div>
                        </div>
                    </div>

                    <!-- Academic Information -->
                    <div class="col-md-6">
                        <div class="card-sims h-100">
                            <div class="card-header-sims">
                                <h5>
                                    <i class="fa fa-graduation-cap me-2" style="color:#f59e0b;"></i>
                                    Academic Information
                                </h5>
                            </div>

                            <div class="card-body-sims">

                                <div class="info-row">
                                    <div class="info-label">Programme</div>
                                    <div class="info-value">
                                        <asp:Label ID="lblProgrammeName" runat="server"></asp:Label>
                                    </div>
                                </div>

                                <div class="info-row">
                                    <div class="info-label">Intake Year</div>
                                    <div class="info-value">
                                        <asp:Label ID="lblIntakeYear" runat="server"></asp:Label>
                                    </div>
                                </div>

                                <div class="info-row">
                                    <div class="info-label">Intake Semester</div>
                                    <div class="info-value">
                                        <asp:Label ID="lblIntakeSemester" runat="server"></asp:Label>
                                    </div>
                                </div>

                                <div class="info-row">
                                    <div class="info-label">Current Semester</div>
                                    <div class="info-value">
                                        <asp:Label ID="lblCurrentSemester" runat="server"></asp:Label>
                                    </div>
                                </div>

                                <div class="info-row">
                                    <div class="info-label">Admission Date</div>
                                    <div class="info-value">
                                        <asp:Label ID="lblAdmissionDate" runat="server"></asp:Label>
                                    </div>
                                </div>

                            </div>
                        </div>
                    </div>

                </div>

                <!-- Back Button -->
                <div class="mt-4">
                    <a href="Dashboard.aspx" class="btn btn-primary">
                        <i class="fa fa-arrow-left me-2"></i>
                        Back to Dashboard
                    </a>
                </div>

            </div>

        </div>

        <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>

    </form>
</body>
</html>