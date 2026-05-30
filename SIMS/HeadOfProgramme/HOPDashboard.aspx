<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="HOPDashboard.aspx.cs"
    Inherits="SIMS.HeadOfProgramme.Dashboard"
    MasterPageFile="~/HeadOfProgramme/HOPMaster.master" %>

<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">

    <div style="background:#fff; border-radius:12px; padding:24px 28px;
                border:1px solid #e2e8f0; margin-bottom:24px;
                display:flex; align-items:center; justify-content:space-between;">
        <div>
            <h4 style="margin:0; color:#1e293b; font-weight:bold;">
                Welcome back, Admin 👋
            </h4>
            <p style="margin:4px 0 0; color:#64748b; font-size:14px;">
                Head Of Programme Dashboard
            </p>
        </div>

        <div style="text-align:right; color:#64748b; font-size:13px;">
            <i class="fa fa-calendar"></i>
            <%= DateTime.Now.ToString("dddd, dd MMM yyyy") %>
        </div>
    </div>

    <div class="row g-3 mb-4">

        <div class="col-md-3">
            <div class="stat-card">
                <div class="stat-icon" style="background:#dbeafe;">
                    <i class="fa fa-layer-group" style="color:#2563eb;"></i>
                </div>
                <div>
                    <p class="stat-label">Programmes</p>
                    <div class="stat-value">3</div>
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
                    <div class="stat-value">24</div>
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
                    <div class="stat-value">150</div>
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
                    <div class="stat-value">12</div>
                </div>
            </div>
        </div>

    </div>

    <div class="row g-3">

        <div class="col-md-8">
            <div class="card-sims">
                <div class="card-header-sims">
                    <h5>
                        <i class="fa fa-sliders me-2 text-primary"></i>
                        Management Modules
                    </h5>
                </div>

                <div class="card-body-sims">
                    <div class="row g-3">

                        <div class="col-md-6">
                            <div class="card shadow-sm p-3 text-center rounded-4">
                                <h5>Programme</h5>
                                <a href="Programme.aspx" class="btn btn-primary mt-2">Manage</a>
                            </div>
                        </div>

                        <div class="col-md-6">
                            <div class="card shadow-sm p-3 text-center rounded-4">
                                <h5>Courses</h5>
                                <a href="Course.aspx" class="btn btn-success mt-2">Manage</a>
                            </div>
                        </div>

                        <div class="col-md-6">
                            <div class="card shadow-sm p-3 text-center rounded-4">
                                <h5>Enroll Student</h5>
                                <a href="Enrolment.aspx" class="btn btn-warning mt-2">Go</a>
                            </div>
                        </div>

                        <div class="col-md-6">
                            <div class="card shadow-sm p-3 text-center rounded-4">
                                <h5>Register Lecturer</h5>
                                <a href="RegisterLecturer.aspx" class="btn btn-dark mt-2">Register</a>
                            </div>
                        </div>

                        <div class="col-md-6">
                            <div class="card shadow-sm p-3 text-center rounded-4">
                                <h5>Assign Lecturer</h5>
                                <a href="AssignLecturer.aspx" class="btn btn-success mt-2">Assign</a>
                            </div>
                        </div>

                    </div>
                </div>
            </div>
        </div>

        <div class="col-md-4">
            <div class="card-sims h-100">
                <div class="card-header-sims">
                    <h5>
                        <i class="fa fa-bolt me-2 text-warning"></i>
                        Quick Actions
                    </h5>
                </div>

                <div class="card-body-sims d-grid gap-2">
                    <a href="Programme.aspx" class="btn btn-outline-primary text-start">
                        <i class="fa fa-layer-group me-2"></i> Manage Programme
                    </a>

                    <a href="Course.aspx" class="btn btn-outline-success text-start">
                        <i class="fa fa-book me-2"></i> Manage Courses
                    </a>

                    <a href="Enrolment.aspx" class="btn btn-outline-warning text-start">
                        <i class="fa fa-user-plus me-2"></i> Enroll Student
                    </a>

                    <a href="RegisterLecturer.aspx" class="btn btn-outline-dark text-start">
                        <i class="fa fa-user-tie me-2"></i> Register Lecturer
                    </a>

                    <a href="AssignLecturer.aspx" class="btn btn-outline-danger text-start">
                        <i class="fa fa-link me-2"></i> Assign Lecturer
                    </a>
                </div>
            </div>
        </div>

    </div>

</asp:Content>