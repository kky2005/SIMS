<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="LecturerCourses.aspx.cs"
    Inherits="SIMS.Lecturer.LecturerCourses"
    MasterPageFile="~/Lecturer/LecturerMaster.master" %>

<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .course-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
            gap: 20px;
            margin-bottom: 30px;
        }

        .course-card {
            background: white;
            border-radius: 8px;
            border: 1px solid #e2e8f0;
            padding: 20px;
            transition: all 0.3s ease;
            box-shadow: 0 1px 3px rgba(0,0,0,0.1);
        }

        .course-card:hover {
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            transform: translateY(-2px);
        }

        .course-code {
            font-size: 12px;
            color: #0d6efd;
            font-weight: bold;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }

        .course-name {
            font-size: 18px;
            font-weight: bold;
            color: #1e293b;
            margin: 8px 0;
        }

        .course-info {
            display: flex;
            gap: 15px;
            margin: 12px 0;
            font-size: 14px;
            color: #64748b;
        }

        .course-info-item {
            display: flex;
            align-items: center;
            gap: 5px;
        }

        .course-actions {
            display: flex;
            gap: 8px;
            margin-top: 15px;
            flex-wrap: wrap;
        }

        .course-actions .btn {
            flex: 1;
            min-width: 100px;
            padding: 8px 12px;
            font-size: 12px;
        }

        .semester-filter {
            display: flex;
            gap: 10px;
            margin-bottom: 30px;
            align-items: center;
            flex-wrap: wrap;
        }

        .filter-badge {
            padding: 6px 14px;
            border-radius: 20px;
            border: 1px solid #e2e8f0;
            background: white;
            cursor: pointer;
            font-size: 14px;
            transition: all 0.3s ease;
            text-decoration: none;
            color: #1e293b;
        }

        .filter-badge.active {
            background: #0d6efd;
            color: white;
            border-color: #0d6efd;
        }

        .filter-badge:hover {
            border-color: #0d6efd;
        }

        .no-courses {
            text-align: center;
            padding: 40px 20px;
            background: #f8fafc;
            border-radius: 8px;
            color: #64748b;
        }

        .no-courses i {
            font-size: 48px;
            color: #cbd5e1;
            margin-bottom: 15px;
            display: block;
        }

        .filter-section {
            margin-bottom: 30px;
            padding: 20px;
            background: white;
            border-radius: 8px;
            border: 1px solid #e2e8f0;
        }

        .filter-section h5 {
            margin: 0 0 15px 0;
            color: #1e293b;
            font-weight: bold;
        }

        /* Modal Styles */
        .modal-overlay {
            display: none;
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(0,0,0,0.5);
            z-index: 1000;
            align-items: center;
            justify-content: center;
        }

        .modal-overlay.show {
            display: flex;
        }

        .modal-content {
            background: white;
            border-radius: 8px;
            padding: 30px;
            max-width: 600px;
            width: 90%;
            max-height: 80vh;
            overflow-y: auto;
            box-shadow: 0 20px 60px rgba(0,0,0,0.3);
        }

        .modal-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 20px;
            padding-bottom: 15px;
            border-bottom: 1px solid #e2e8f0;
        }

        .modal-header h3 {
            margin: 0;
            color: #1e293b;
        }

        .modal-close {
            background: none;
            border: none;
            font-size: 24px;
            cursor: pointer;
            color: #64748b;
        }

        .form-group {
            display: flex;
            flex-direction: column;
            margin-bottom: 15px;
        }

        .form-group label {
            font-weight: bold;
            color: #1e293b;
            margin-bottom: 8px;
            font-size: 14px;
        }

        .form-group input,
        .form-group textarea,
        .form-group select {
            padding: 10px 12px;
            border: 1px solid #cbd5e1;
            border-radius: 6px;
            font-size: 14px;
            font-family: inherit;
        }

        .form-group textarea {
            resize: vertical;
            min-height: 100px;
        }

        .form-group input:focus,
        .form-group textarea:focus,
        .form-group select:focus {
            outline: none;
            border-color: #0d6efd;
            box-shadow: 0 0 0 3px rgba(13, 110, 253, 0.1);
        }

        .form-actions {
            display: flex;
            gap: 10px;
            justify-content: flex-end;
            margin-top: 20px;
            padding-top: 15px;
            border-top: 1px solid #e2e8f0;
        }

        .alert-message {
            padding: 12px 15px;
            border-radius: 6px;
            margin-bottom: 20px;
            font-size: 14px;
            display: flex;
            align-items: center;
            gap: 10px;
        }

        .alert-success {
            background: #dcfce7;
            border: 1px solid #86efac;
            color: #166534;
        }

        .alert-danger {
            background: #fee2e2;
            border: 1px solid #fca5a5;
            color: #991b1b;
        }

        @media (max-width: 768px) {
            .course-actions {
                flex-direction: column;
            }
            .course-actions .btn {
                flex: 1;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">

    <!-- Header -->
    <div style="margin-bottom: 30px;">
        <h3 style="margin: 0 0 5px 0; color: #1e293b; font-weight: bold;">
            <i class="fa fa-book" style="color: #0d6efd; margin-right: 10px;"></i>My Courses
        </h3>
        <p style="margin: 0; color: #64748b; font-size: 14px;">Manage your assigned courses, materials, and announcements</p>
    </div>

    <!-- Filters Section -->
    <div class="filter-section">
        <h5><i class="fa fa-filter" style="margin-right: 8px;"></i>Filter by Semester</h5>
        <div class="semester-filter">
            <asp:LinkButton ID="btnFilterAll" runat="server" CssClass="filter-badge active" 
                OnClick="btnFilterAll_Click" Text="All Semesters" />
            <asp:PlaceHolder ID="phSemesterFilters" runat="server" />
        </div>
    </div>

    <!-- Courses Grid -->
    <div class="course-grid" id="courseContainer" runat="server">
        <asp:Repeater ID="rptCourses" runat="server" OnItemDataBound="rptCourses_ItemDataBound">
            <ItemTemplate>
                <div class="course-card">
                    <div class="course-code"><%# Eval("CourseCode") %></div>
                    <div class="course-name"><%# Eval("CourseName") %></div>
                    
                    <div class="course-info">
                        <div class="course-info-item">
                            <i class="fa fa-users" style="color: #0d6efd;"></i>
                            <strong><%# Eval("TotalStudents") %></strong> Students
                        </div>
                        <div class="course-info-item">
                            <i class="fa fa-award" style="color: #16a34a;"></i>
                            <strong><%# Eval("CreditHours") %></strong> Credits
                        </div>
                    </div>

                    <div style="padding: 12px 0; border-top: 1px solid #e2e8f0; border-bottom: 1px solid #e2e8f0; font-size: 13px; color: #64748b;">
                        <div style="margin: 8px 0;">
                            <strong>Semester:</strong> <%# Eval("Semester") %>
                        </div>
                        <div style="margin: 8px 0;">
                            <strong>Academic Year:</strong> <%# Eval("AcademicYear") %>
                        </div>
                    </div>

                    <div class="course-actions">
                        <a href='<%# "LecturerAttendance.aspx?CourseID=" + Eval("CourseId") %>' 
                           class="btn btn-sm btn-outline-primary">
                            <i class="fa fa-calendar-check"></i> Attendance
                        </a>
                        <a href='<%# "LecturerGrades.aspx?CourseID=" + Eval("CourseId") %>' 
                           class="btn btn-sm btn-outline-success">
                            <i class="fa fa-star"></i> Grades
                        </a>

                        <!-- New: navigate to course-specific Materials page -->
                        <a href='<%# "LecturerMaterials.aspx?CourseID=" + Eval("CourseId") %>' 
                           class="btn btn-sm btn-outline-warning">
                            <i class="fa fa-file"></i> Materials
                        </a>

                        <!-- New: navigate to course-specific Announcements page -->
                        <a href='<%# "LecturerAnnouncements.aspx?CourseID=" + Eval("CourseId") %>' 
                           class="btn btn-sm btn-outline-info">
                            <i class="fa fa-bullhorn"></i> Announce
                        </a>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>

    <!-- No Courses Message -->
    <asp:Panel ID="pnlNoCourses" runat="server" Visible="false" CssClass="no-courses">
        <i class="fa fa-book"></i>
        <h5 style="margin: 0 0 10px 0; color: #1e293b;">No Courses Found</h5>
        <p style="margin: 0;">You don't have any courses assigned.</p>
    </asp:Panel>

</asp:Content>