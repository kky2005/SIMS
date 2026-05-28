﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="SIMS.HeadOfProgramme.Dashboard" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Admin Dashboard</title>

    <!-- Bootstrap -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
</head>

<body>

<form runat="server">

<!-- NAVBAR -->
<nav class="navbar navbar-expand-lg navbar-dark bg-dark">
    <div class="container-fluid">

        <span class="navbar-brand">Student Management System</span>

        <div class="collapse navbar-collapse">

            <!-- MENU -->
            <ul class="navbar-nav me-auto">

                <li class="nav-item">
                    <a class="nav-link active" href="Dashboard.aspx">Home</a>
                </li>

                <li class="nav-item">
                    <a class="nav-link" href="Programme.aspx">Manage Programme</a>
                </li>

                <li class="nav-item">
                    <a class="nav-link" href="Course.aspx">Manage Courses</a>
                </li>

                <li class="nav-item">
                    <a class="nav-link" href="Enrolment.aspx">Enrol Student</a>
                </li>

                <li class="nav-item">
                    <a class="nav-link" href="RegisterLecturer.aspx">Register Lecturer</a>
                </li>

                <li class="nav-item">
                    <a class="nav-link" href="AssignLecturer.aspx">Assign Lecturer</a>
                </li>

            </ul>

            <!-- LOGOUT -->
            <asp:Button ID="btnLogout" runat="server"
                Text="Logout"
                CssClass="btn btn-danger"
                OnClick="btnLogout_Click" />

        </div>
    </div>
</nav>

<!-- CONTENT -->
<div class="container mt-5">

    <h2>Welcome Admin 🎉</h2>
    <p>Select a module below:</p>

    <!-- CARD MENU -->
    <div class="row mt-4">

        <div class="col-md-3">
            <div class="card shadow p-3 text-center">
                <h5>Programme</h5>
                <a href="Programme.aspx" class="btn btn-primary mt-2">Manage</a>
            </div>
        </div>

        <div class="col-md-3">
            <div class="card shadow p-3 text-center">
                <h5>Courses</h5>
                <a href="Course.aspx" class="btn btn-success mt-2">Manage</a>
            </div>
        </div>

        <div class="col-md-3">
            <div class="card shadow p-3 text-center">
                <h5>Enroll Student</h5>
                <a href="Enrolment.aspx" class="btn btn-warning mt-2">Go</a>
            </div>
        </div>

        <div class="col-md-3">
            <div class="card shadow-sm text-center p-3 rounded-3">
                <h6 class="fw-semibold">Register Lecturer</h6>
                <a href="RegisterLecturer.aspx" class="btn btn-dark w-100 mt-2">Register</a>
            </div>
        </div>

    </div>
    <div class="row mt-4">
          <div class="col-md-3">
            <div class="card shadow-sm text-center p-3 rounded-3">
                <h6 class="fw-semibold">Assign Lecturer</h6>
                <a href="AssignLecturer.aspx" class="btn btn-success w-100 mt-2">Assign</a>
            </div>
        </div>
    </div>

</div>

</form>

</body>
</html>