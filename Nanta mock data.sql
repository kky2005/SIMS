-- =========================================
-- CLEAN MOCK DATA INSERTION SCRIPT FOR SIMS_DB
-- Run this AFTER the table creation script on a fresh/empty SIMS_DB database.
-- =========================================

USE SIMS_DB;
GO

-- =========================================
-- 1. INSERT ROLES
-- =========================================
INSERT INTO Roles (RoleName, Description) VALUES
('Admin', 'Head of Programme / system administrator'),
('Lecturer', 'Lecturer who manages courses and grades'),
('Student', 'Student enrolled in programmes');
GO

-- =========================================
-- 2. INSERT USERS
-- =========================================
INSERT INTO Users (RoleId, FullName, Email, PasswordHash, Phone, IsActive) VALUES
-- Admin User
(1, 'John Administrator', 'admin@sims.edu', 'hashed_password_admin_123', '+1-555-0001', 1),

-- Lecturers
(2, 'Dr. Sarah Johnson', 'sarah.johnson@sims.edu', 'hashed_password_lecturer_001', '+1-555-0101', 1),
(2, 'Prof. Michael Chen', 'michael.chen@sims.edu', 'hashed_password_lecturer_002', '+1-555-0102', 1),
(2, 'Dr. Emily Rodriguez', 'emily.rodriguez@sims.edu', 'hashed_password_lecturer_003', '+1-555-0103', 1),
(2, 'Mr. James Wilson', 'james.wilson@sims.edu', 'hashed_password_lecturer_004', '+1-555-0104', 1),
(2, 'Dr. Lisa Anderson', 'lisa.anderson@sims.edu', 'hashed_password_lecturer_005', '+1-555-0105', 1),

-- Students
(3, 'Alice Smith', 'alice.smith@student.sims.edu', 'hashed_password_student_001', '+1-555-0201', 1),
(3, 'Bob Johnson', 'bob.johnson@student.sims.edu', 'hashed_password_student_002', '+1-555-0202', 1),
(3, 'Catherine Lee', 'catherine.lee@student.sims.edu', 'hashed_password_student_003', '+1-555-0203', 1),
(3, 'David Martinez', 'david.martinez@student.sims.edu', 'hashed_password_student_004', '+1-555-0204', 1),
(3, 'Emma Brown', 'emma.brown@student.sims.edu', 'hashed_password_student_005', '+1-555-0205', 1),
(3, 'Frank Davis', 'frank.davis@student.sims.edu', 'hashed_password_student_006', '+1-555-0206', 1),
(3, 'Grace Taylor', 'grace.taylor@student.sims.edu', 'hashed_password_student_007', '+1-555-0207', 1),
(3, 'Henry White', 'henry.white@student.sims.edu', 'hashed_password_student_008', '+1-555-0208', 1);
GO

-- =========================================
-- 3. INSERT PROGRAMMES
-- =========================================
INSERT INTO Programmes (ProgrammeCode, ProgrammeName, DurationYears, Description, IsActive) VALUES
('BSCS', 'Bachelor of Science in Computer Science', 4, 'Comprehensive study of computer science and software development', 1),
('BSIT', 'Bachelor of Science in Information Technology', 4, 'Focus on IT infrastructure, networking, and systems', 1),
('BSCE', 'Bachelor of Science in Civil Engineering', 4, 'Study of civil engineering principles and practice', 1),
('BSME', 'Bachelor of Science in Mechanical Engineering', 4, 'Mechanical systems and engineering design', 1),
('BSBUS', 'Bachelor of Science in Business Administration', 3, 'Business management and administration', 1);
GO

-- =========================================
-- 4. INSERT LECTURERS
-- =========================================
INSERT INTO Lecturers (UserId, StaffNo, Department, Specialisation, EmploymentStatus) VALUES
(2, 'LEC001', 'Computer Science', 'Database Systems & Data Science', 'Active'),
(3, 'LEC002', 'Computer Science', 'Software Engineering & Cloud Computing', 'Active'),
(4, 'LEC003', 'Computer Science', 'Web Development & Mobile Apps', 'Active'),
(5, 'LEC004', 'Engineering', 'Structural Design & Analysis', 'Active'),
(6, 'LEC005', 'Business', 'Finance & Strategic Management', 'Active');
GO

-- =========================================
-- 5. INSERT STUDENTS
-- =========================================
INSERT INTO Students (UserId, ProgrammeId, StudentNo, IntakeYear, IntakeSemester, AdmissionDate, CurrentSemester, Status) VALUES
(7, 1, 'STU001', 2023, 1, '2023-08-15', 3, 'Active'),
(8, 1, 'STU002', 2023, 1, '2023-08-15', 3, 'Active'),
(9, 1, 'STU003', 2023, 1, '2023-08-15', 3, 'Active'),
(10, 2, 'STU004', 2023, 2, '2024-01-10', 2, 'Active'),
(11, 3, 'STU005', 2022, 1, '2022-08-20', 5, 'Active'),
(12, 3, 'STU006', 2023, 1, '2023-08-15', 3, 'Active'),
(13, 4, 'STU007', 2023, 2, '2024-01-10', 2, 'Active'),
(14, 5, 'STU008', 2024, 1, '2024-08-01', 1, 'Active');
GO

-- =========================================
-- 6. INSERT COURSES
-- =========================================
INSERT INTO Courses (ProgrammeId, CourseCode, CourseName, CreditHours, Semester, IsActive) VALUES
-- Computer Science Courses
(1, 'CS101', 'Introduction to Programming', 3, 1, 1),
(1, 'CS102', 'Data Structures', 3, 1, 1),
(1, 'CS201', 'Database Systems', 4, 2, 1),
(1, 'CS202', 'Web Development', 3, 2, 1),
(1, 'CS301', 'Software Engineering', 4, 3, 1),
(1, 'CS302', 'Cloud Computing', 3, 3, 1),

-- IT Courses
(2, 'IT101', 'IT Fundamentals', 3, 1, 1),
(2, 'IT201', 'Network Administration', 4, 2, 1),
(2, 'IT301', 'Cybersecurity Basics', 3, 3, 1),

-- Civil Engineering Courses
(3, 'CE101', 'Engineering Mechanics', 4, 1, 1),
(3, 'CE201', 'Structural Analysis', 4, 2, 1),
(3, 'CE301', 'Design of Structures', 4, 3, 1),

-- Mechanical Engineering Courses
(4, 'ME101', 'Thermodynamics', 3, 1, 1),
(4, 'ME201', 'Machine Design', 4, 2, 1),

-- Business Courses
(5, 'BUS101', 'Business Fundamentals', 3, 1, 1),
(5, 'BUS201', 'Financial Management', 3, 2, 1),
(5, 'BUS301', 'Strategic Management', 3, 3, 1);
GO

-- =========================================
-- 7. INSERT COURSE ASSIGNMENTS
-- =========================================
INSERT INTO CourseAssignments (CourseId, LecturerId, AcademicYear, Semester, AssignedDate) VALUES
-- Computer Science / IT lecturers
(1, 1, 2024, 1, '2024-07-01'),
(2, 2, 2024, 1, '2024-07-01'),
(3, 1, 2024, 2, '2024-12-15'),
(4, 3, 2024, 2, '2024-12-15'),
(5, 2, 2024, 3, '2025-05-01'),
(6, 2, 2024, 3, '2025-05-01'),
(7, 3, 2024, 1, '2024-07-01'),
(8, 3, 2024, 2, '2024-12-15'),
(9, 3, 2024, 3, '2025-05-01'),

-- Engineering lecturer
(10, 4, 2024, 1, '2024-07-01'),
(11, 4, 2024, 2, '2024-12-15'),
(12, 4, 2024, 3, '2025-05-01'),
(13, 4, 2024, 1, '2024-07-01'),
(14, 4, 2024, 2, '2024-12-15'),

-- Business lecturer
(15, 5, 2024, 1, '2024-07-01'),
(16, 5, 2024, 2, '2024-12-15'),
(17, 5, 2024, 3, '2025-05-01');
GO

-- =========================================
-- 8. INSERT REGISTRATION PERIODS
-- PeriodType is kept as Registration/Drop to match the ERD purpose.
-- =========================================
INSERT INTO RegistrationPeriods (ProgrammeId, AcademicYear, Semester, PeriodType, StartDate, EndDate, IsActive) VALUES
-- Computer Science
(1, 2024, 1, 'Registration', '2024-07-15', '2024-08-15', 0),
(1, 2024, 1, 'Drop',         '2024-08-16', '2024-09-15', 0),
(1, 2024, 2, 'Registration', '2024-12-20', '2025-01-20', 0),
(1, 2024, 2, 'Drop',         '2025-01-21', '2025-02-20', 0),
(1, 2024, 3, 'Registration', '2025-05-01', '2025-06-01', 1),
(1, 2024, 3, 'Drop',         '2025-06-02', '2025-06-20', 1),

-- Information Technology
(2, 2024, 1, 'Registration', '2024-07-15', '2024-08-15', 0),
(2, 2024, 1, 'Drop',         '2024-08-16', '2024-09-15', 0),
(2, 2024, 2, 'Registration', '2024-12-20', '2025-01-20', 0),
(2, 2024, 2, 'Drop',         '2025-01-21', '2025-02-20', 0),

-- Civil Engineering
(3, 2024, 1, 'Registration', '2024-07-15', '2024-08-15', 0),
(3, 2024, 1, 'Drop',         '2024-08-16', '2024-09-15', 0),
(3, 2024, 2, 'Registration', '2024-12-20', '2025-01-20', 0),
(3, 2024, 2, 'Drop',         '2025-01-21', '2025-02-20', 0),

-- Mechanical Engineering
(4, 2024, 1, 'Registration', '2024-07-15', '2024-08-15', 0),
(4, 2024, 1, 'Drop',         '2024-08-16', '2024-09-15', 0),

-- Business
(5, 2024, 1, 'Registration', '2024-07-15', '2024-08-15', 0),
(5, 2024, 1, 'Drop',         '2024-08-16', '2024-09-15', 0);
GO

-- =========================================
-- 9. INSERT ENROLMENTS
-- =========================================
INSERT INTO Enrolments (StudentId, CourseId, AcademicYear, Semester, Status, EnrolledAt) VALUES
-- Student 1 (Alice) - Computer Science
(1, 1, 2024, 1, 'Active', '2024-08-01 09:30:00'),
(1, 2, 2024, 1, 'Active', '2024-08-01 09:35:00'),
(1, 3, 2024, 2, 'Active', '2024-12-20 10:00:00'),
(1, 4, 2024, 2, 'Active', '2024-12-20 10:05:00'),

-- Student 2 (Bob) - Computer Science
(2, 1, 2024, 1, 'Active', '2024-08-01 10:00:00'),
(2, 2, 2024, 1, 'Active', '2024-08-01 10:05:00'),
(2, 3, 2024, 2, 'Active', '2024-12-20 10:30:00'),

-- Student 3 (Catherine) - Computer Science
(3, 1, 2024, 1, 'Active', '2024-08-02 09:00:00'),
(3, 2, 2024, 1, 'Active', '2024-08-02 09:10:00'),
(3, 5, 2024, 3, 'Active', '2025-05-05 11:00:00'),

-- Student 4 (David) - Information Technology
(4, 7, 2024, 1, 'Active', '2024-08-01 11:00:00'),

-- Student 5 (Emma) - Civil Engineering
(5, 10, 2024, 1, 'Active', '2024-08-01 09:00:00'),
(5, 11, 2024, 2, 'Active', '2024-12-20 14:00:00'),

-- Student 6 (Frank) - Civil Engineering
(6, 10, 2024, 1, 'Active', '2024-08-01 09:15:00'),
(6, 11, 2024, 2, 'Active', '2024-12-20 14:15:00'),

-- Student 7 (Grace) - Mechanical Engineering
(7, 13, 2024, 1, 'Active', '2024-08-01 10:00:00'),

-- Student 8 (Henry) - Business
(8, 15, 2024, 1, 'Active', '2024-08-01 14:00:00');
GO

-- =========================================
-- 10. INSERT ATTENDANCE RECORDS
-- =========================================
INSERT INTO Attendance (EnrolmentId, AttendanceDate, Status, Remarks, RecordedBy) VALUES
-- Alice - CS101
(1, '2024-08-05', 'Present', 'On time', 2),
(1, '2024-08-08', 'Present', 'On time', 2),
(1, '2024-08-12', 'Late', 'Arrived 10 minutes late', 2),
(1, '2024-08-15', 'Absent', 'No reason provided', 2),

-- Alice - CS102
(2, '2024-08-05', 'Present', 'On time', 3),
(2, '2024-08-08', 'Present', 'On time', 3),
(2, '2024-08-12', 'Present', 'On time', 3),

-- Alice - CS201
(3, '2024-12-20', 'Present', 'On time', 2),
(3, '2024-12-23', 'Present', 'On time', 2),
(3, '2024-12-27', 'Absent', 'Sick leave', 2),

-- Bob - CS101
(5, '2024-08-05', 'Present', 'On time', 2),
(5, '2024-08-08', 'Absent', 'No excuse', 2),

-- Bob - CS102
(6, '2024-08-05', 'Present', 'On time', 3),
(6, '2024-08-08', 'Present', 'On time', 3);
GO

-- =========================================
-- 11. INSERT GRADE SCALE
-- =========================================
INSERT INTO GradeScale (MinMark, MaxMark, GradeLetter, GradePoint, Description) VALUES
(90, 100, 'A+', 4.0, 'Excellent'),
(85, 89.99, 'A', 3.9, 'Very Good'),
(80, 84.99, 'A-', 3.7, 'Good'),
(75, 79.99, 'B+', 3.3, 'Good'),
(70, 74.99, 'B', 3.0, 'Above Average'),
(65, 69.99, 'B-', 2.7, 'Average'),
(60, 64.99, 'C+', 2.3, 'Average'),
(55, 59.99, 'C', 2.0, 'Below Average'),
(50, 54.99, 'C-', 1.7, 'Below Average'),
(40, 49.99, 'D', 1.0, 'Poor'),
(0, 39.99, 'F', 0.0, 'Fail');
GO

-- =========================================
-- 12. INSERT ASSESSMENTS
-- =========================================
INSERT INTO Assessments (CourseId, AcademicYear, Semester, AssessmentName, MaxMark, Weightage, IsPublished) VALUES
-- CS101 Assessments
(1, 2024, 1, 'Quiz 1', 20, 10, 1),
(1, 2024, 1, 'Midterm Exam', 30, 30, 1),
(1, 2024, 1, 'Final Exam', 50, 60, 1),

-- CS102 Assessments
(2, 2024, 1, 'Assignment 1', 20, 20, 1),
(2, 2024, 1, 'Midterm Exam', 30, 30, 1),
(2, 2024, 1, 'Final Exam', 50, 50, 1),

-- CS201 Assessments
(3, 2024, 2, 'Project 1', 25, 25, 1),
(3, 2024, 2, 'Midterm Exam', 30, 35, 1),
(3, 2024, 2, 'Final Exam', 45, 40, 1),

-- IT101 Assessments
(7, 2024, 1, 'Quiz', 20, 15, 1),
(7, 2024, 1, 'Midterm', 30, 35, 1),
(7, 2024, 1, 'Final', 50, 50, 1),

-- CE101 Assessments
(10, 2024, 1, 'Quiz', 20, 20, 1),
(10, 2024, 1, 'Lab Assessment', 30, 30, 1),
(10, 2024, 1, 'Final Exam', 50, 50, 1);
GO

-- =========================================
-- 13. INSERT STUDENT MARKS
-- WeightedMark = (MarksObtained / MaxMark) * Weightage
-- GradeScaleId is based on the percentage mark.
-- =========================================
INSERT INTO StudentMarks (AssessmentId, StudentId, GradeScaleId, MarksObtained, WeightedMark, GradedBy, IsPublished) VALUES
-- Alice - CS101
(1, 1, 1, 18, 9.00, 2, 1),
(2, 1, 2, 26, 26.00, 2, 1),
(3, 1, 3, 42, 50.40, 2, 1),

-- Alice - CS102
(4, 1, 1, 18, 18.00, 3, 1),
(5, 1, 1, 28, 28.00, 3, 1),
(6, 1, 3, 42, 42.00, 3, 1),

-- Bob - CS101
(1, 2, 1, 19, 9.50, 2, 1),
(2, 2, 1, 28, 28.00, 2, 1),
(3, 2, 2, 44, 52.80, 2, 1),

-- Bob - CS102
(4, 2, 1, 19, 19.00, 3, 1),
(5, 2, 1, 27, 27.00, 3, 1),
(6, 2, 2, 43, 43.00, 3, 1),

-- Catherine - CS101, intentionally lower marks for academic warning testing
(1, 3, 8, 11, 5.50, 2, 1),
(2, 3, 9, 16, 16.00, 2, 1),
(3, 3, 10, 24, 28.80, 2, 1),

-- Emma - CE101
(13, 5, 3, 16, 16.00, 5, 1),
(14, 5, 3, 24, 24.00, 5, 1),
(15, 5, 4, 39, 39.00, 5, 1);
GO

-- =========================================
-- 14. INSERT GPA RECORDS
-- =========================================
INSERT INTO GPARecords (StudentId, AcademicYear, Semester, GPA, CGPA, TotalCreditHours) VALUES
(1, 2024, 1, 3.80, 3.80, 6),
(2, 2024, 1, 3.90, 3.90, 6),
(3, 2024, 1, 1.85, 1.85, 6),
(4, 2024, 1, 3.30, 3.30, 3),
(5, 2024, 1, 3.20, 3.25, 4),
(6, 2024, 1, 3.35, 3.40, 4),
(7, 2024, 1, 3.10, 3.10, 3),
(8, 2024, 1, 2.10, 2.10, 3);
GO

-- =========================================
-- 15. INSERT ANNOUNCEMENTS
-- =========================================
INSERT INTO Announcements (AuthorUserId, CourseId, Title, Body, Audience, PublishedAt, ExpiresAt) VALUES
(2, 1, 'Welcome to CS101', 'Welcome to Introduction to Programming. This course will cover fundamental programming concepts.', 'Student', '2024-08-01 08:00:00', '2024-09-01'),
(3, 2, 'Important: Assignment Due Date Extended', 'The assignment due date has been extended to next Friday.', 'Student', '2024-08-10 14:30:00', '2024-08-15'),
(1, NULL, 'System Maintenance Notice', 'The system will be under maintenance on August 25, 2024 from 10 PM to 6 AM.', 'All', '2024-08-15 09:00:00', '2024-08-25'),
(2, 3, 'Midterm Exam Schedule', 'Midterm exams for Database Systems will be held on December 20, 2024.', 'Student', '2024-12-01 10:00:00', '2024-12-19'),
(5, 10, 'Lab Session Cancelled', 'The Engineering Mechanics lab session scheduled for August 8 is cancelled due to equipment maintenance.', 'Student', '2024-08-05 11:00:00', '2024-08-08');
GO

-- =========================================
-- 16. INSERT NOTIFICATIONS
-- =========================================
INSERT INTO Notifications (UserId, Title, Message, NotificationType, IsRead, CreatedAt, LinkUrl) VALUES
(7, 'Course Enrolment Confirmation', 'You have successfully enrolled in CS101.', 'Enrolment', 1, '2024-08-01 09:30:00', '/Student/EnrolledCourses.aspx'),
(7, 'New Announcement', 'Dr. Sarah Johnson posted a new announcement in CS101.', 'Announcement', 0, '2024-08-10 14:35:00', '/Student/Notifications.aspx'),
(8, 'Attendance Alert', 'You were marked absent in CS101 on August 8.', 'Attendance', 1, '2024-08-08 16:00:00', '/Student/AttendanceRecord.aspx'),
(1, 'New Student Enrolled', 'Alice Smith has enrolled in CS101.', 'System', 0, '2024-08-01 09:31:00', '/Admin/ManageStudents.aspx'),
(7, 'Grade Published', 'Your grades for CS101 Quiz 1 have been published.', 'Grade', 0, '2024-08-15 10:00:00', '/Student/AcademicResults.aspx');
GO

-- =========================================
-- 17. INSERT ACADEMIC WARNINGS
-- =========================================
INSERT INTO AcademicWarnings (StudentId, CourseId, WarningType, Reason, Severity, Status, IssuedBy, IssuedAt) VALUES
(2, 1, 'Low Attendance', 'Attendance is below the expected threshold for CS101.', 'High', 'Active', 1, '2024-08-20 10:00:00'),
(3, 1, 'Poor Marks', 'Overall CS101 assessment performance is below the expected target.', 'High', 'Active', 1, '2024-08-20 14:30:00'),
(8, NULL, 'Low GPA', 'Cumulative GPA is below the recommended academic progress threshold.', 'Medium', 'Active', 1, '2024-08-25 09:00:00');
GO

-- =========================================
-- 18. INSERT ACADEMIC CALENDAR
-- =========================================
INSERT INTO AcademicCalendar (EventName, EventType, StartDate, EndDate, AcademicYear, Semester, Description) VALUES
('Semester 1 Registration', 'Registration', '2024-07-15', '2024-08-15', 2024, 1, 'Registration period for first semester'),
('Semester 1 Classes', 'Classes', '2024-08-19', '2024-11-15', 2024, 1, 'Classes for first semester'),
('Midterm Exams Sem 1', 'Exam', '2024-09-23', '2024-09-27', 2024, 1, 'Midterm exams for semester 1'),
('Final Exams Sem 1', 'Exam', '2024-11-18', '2024-11-29', 2024, 1, 'Final exams for semester 1'),
('Semester 2 Registration', 'Registration', '2024-12-20', '2025-01-20', 2024, 2, 'Registration period for second semester'),
('Semester 2 Classes', 'Classes', '2025-01-27', '2025-04-18', 2024, 2, 'Classes for second semester'),
('Midterm Exams Sem 2', 'Exam', '2025-02-24', '2025-02-28', 2024, 2, 'Midterm exams for semester 2'),
('Final Exams Sem 2', 'Exam', '2025-04-21', '2025-05-02', 2024, 2, 'Final exams for semester 2'),
('Semester 3 Registration', 'Registration', '2025-05-01', '2025-06-01', 2024, 3, 'Registration period for third semester'),
('Semester 3 Classes', 'Classes', '2025-06-02', '2025-08-22', 2024, 3, 'Classes for third semester');
GO

-- =========================================
-- 19. INSERT COURSE MATERIALS
-- =========================================
INSERT INTO CourseMaterials (CourseId, UploadedBy, Title, Description, FileUrl, FileType, FileSizeKB, AcademicYear, Semester, IsVisible, UploadedAt) VALUES
(1, 2, 'Chapter 1: Programming Fundamentals', 'Introduction to programming concepts', '/materials/CS101_Ch1.pdf', 'PDF', 2540, 2024, 1, 1, '2024-08-02 09:00:00'),
(1, 2, 'Lecture Slides Week 1', 'Week 1 lecture slides for CS101', '/materials/CS101_Week1_Slides.pptx', 'PPTX', 3200, 2024, 1, 1, '2024-08-02 09:15:00'),
(2, 3, 'Data Structures Reference', 'Common data structures implementation guide', '/materials/CS102_DataStructures.pdf', 'PDF', 1850, 2024, 1, 1, '2024-08-03 10:00:00'),
(3, 2, 'Database Design Fundamentals', 'Introduction to database design principles', '/materials/CS201_DBDesign.pdf', 'PDF', 2100, 2024, 2, 1, '2024-12-21 08:00:00'),
(3, 2, 'SQL Query Examples', 'Common SQL queries and examples', '/materials/CS201_SQLExamples.sql', 'SQL', 450, 2024, 2, 1, '2024-12-21 08:30:00'),
(7, 4, 'IT Fundamentals Overview', 'Overview of IT fundamentals course', '/materials/IT101_Overview.pdf', 'PDF', 1200, 2024, 1, 1, '2024-08-01 14:00:00'),
(10, 5, 'Engineering Mechanics Textbook Chapter 1', 'Mechanics fundamentals', '/materials/CE101_Ch1.pdf', 'PDF', 3400, 2024, 1, 1, '2024-08-04 11:00:00'),
(15, 6, 'Business Fundamentals Slides', 'Business basics and concepts', '/materials/BUS101_Slides.pptx', 'PPTX', 2800, 2024, 1, 1, '2024-08-05 13:00:00');
GO

-- =========================================
-- 20. INSERT FEES
-- =========================================
INSERT INTO Fees (StudentId, AcademicYear, Semester, TotalAmount, PaidAmount, Balance, DueDate, Status) VALUES
(1, 2024, 1, 5000.00, 5000.00, 0.00, '2024-08-15', 'Paid'),
(2, 2024, 1, 5000.00, 5000.00, 0.00, '2024-08-15', 'Paid'),
(3, 2024, 1, 5000.00, 2500.00, 2500.00, '2024-08-15', 'Partial'),
(4, 2024, 1, 5000.00, 0.00, 5000.00, '2024-08-15', 'Unpaid'),
(5, 2024, 1, 5000.00, 5000.00, 0.00, '2024-08-15', 'Paid'),
(6, 2024, 1, 5000.00, 3500.00, 1500.00, '2024-08-15', 'Partial'),
(7, 2024, 1, 5000.00, 5000.00, 0.00, '2024-08-15', 'Paid'),
(8, 2024, 1, 5000.00, 0.00, 5000.00, '2024-08-15', 'Unpaid'),
(1, 2024, 2, 5000.00, 5000.00, 0.00, '2025-01-20', 'Paid'),
(2, 2024, 2, 5000.00, 5000.00, 0.00, '2025-01-20', 'Paid');
GO

-- =========================================
-- 21. INSERT REPORTS
-- =========================================
INSERT INTO Reports (GeneratedBy, ReportType, AcademicYear, Semester, FilterCriteria, GeneratedAt) VALUES
(1, 'Student Progress Report', 2024, 1, 'ProgrammeId=1', '2024-08-25 10:30:00'),
(1, 'Attendance Summary', 2024, 1, 'CourseId=1', '2024-08-26 14:00:00'),
(1, 'Grade Distribution Report', 2024, 1, 'AcademicYear=2024,Semester=1', '2024-08-27 09:15:00'),
(1, 'Fee Collection Report', 2024, 1, 'Status=Paid', '2024-08-28 11:00:00'),
(1, 'Course Performance Report', 2024, 1, 'TopPerformers=true', '2024-08-29 15:30:00');
GO

-- =========================================
-- 22. INSERT REPORT EXPORTS
-- =========================================
INSERT INTO ReportExports (ReportId, ExportFormat, FilePath, ExportedAt) VALUES
(1, 'PDF', '/exports/StudentProgressReport_2024_Sem1.pdf', '2024-08-25 10:35:00'),
(2, 'Excel', '/exports/AttendanceSummary_2024_Sem1.xlsx', '2024-08-26 14:05:00'),
(3, 'Excel', '/exports/GradeDistribution_2024_Sem1.xlsx', '2024-08-27 09:20:00'),
(4, 'PDF', '/exports/FeeCollection_2024_Sem1.pdf', '2024-08-28 11:05:00'),
(5, 'Excel', '/exports/CoursePerformance_2024_Sem1.xlsx', '2024-08-29 15:35:00');
GO

-- =========================================
-- 23. INSERT AUDIT LOGS
-- =========================================
INSERT INTO AuditLogs (UserId, Action, TableAffected, RecordId, OldValue, NewValue, ActionDate) VALUES
(1, 'INSERT', 'Students', 1, NULL, 'Alice Smith enrolled in BSCS', '2024-08-01 09:30:00'),
(2, 'UPDATE', 'Attendance', 1, 'Absent', 'Present', '2024-08-05 16:00:00'),
(1, 'INSERT', 'Announcements', 1, NULL, 'Welcome announcement created', '2024-08-01 08:00:00'),
(1, 'UPDATE', 'GradeScale', 1, 'A+ = 3.9', 'A+ = 4.0', '2024-08-10 10:00:00'),
(1, 'UPDATE', 'Fees', 3, 'Status=Unpaid', 'Status=Partial', '2024-08-15 13:00:00'),
(2, 'INSERT', 'StudentMarks', 1, NULL, 'Mark recorded: 18/20', '2024-08-15 10:15:00');
GO

-- =========================================
-- 24. INSERT LOGIN ATTEMPTS
-- =========================================
INSERT INTO LoginAttempts (UserId, Email, IsSuccessful, AttemptDate) VALUES
(1, 'admin@sims.edu', 1, '2024-08-20 08:00:00'),
(2, 'sarah.johnson@sims.edu', 1, '2024-08-20 08:15:00'),
(7, 'alice.smith@student.sims.edu', 1, '2024-08-20 09:00:00'),
(NULL, 'invalid.user@sims.edu', 0, '2024-08-20 09:30:00'),
(8, 'bob.johnson@student.sims.edu', 1, '2024-08-20 10:00:00'),
(NULL, 'alice.smith@student.sims.edu', 0, '2024-08-21 08:00:00'),
(1, 'admin@sims.edu', 1, '2024-08-21 08:05:00'),
(3, 'michael.chen@sims.edu', 1, '2024-08-21 08:30:00');
GO

-- =========================================
-- SUMMARY
-- =========================================
-- Clean mock data insertion complete.
-- Total records inserted:
-- - Roles: 3
-- - Users: 14 (1 Admin, 5 Lecturers, 8 Students)
-- - Programmes: 5
-- - Lecturers: 5
-- - Students: 8
-- - Courses: 17
-- - Course Assignments: 17
-- - Registration Periods: 18
-- - Enrolments: 17
-- - Attendance Records: 14
-- - Grade Scale: 11
-- - Assessments: 15
-- - Student Marks: 18
-- - GPA Records: 8
-- - Announcements: 5
-- - Notifications: 5
-- - Academic Warnings: 3
-- - Academic Calendar: 10
-- - Course Materials: 8
-- - Fees: 10
-- - Reports: 5
-- - Report Exports: 5
-- - Audit Logs: 6
-- - Login Attempts: 8
-- =========================================

USE SIMS_DB;
GO

UPDATE Users
SET PasswordHash = 'admin123'
WHERE Email = 'admin@sims.edu';
GO

UPDATE Users
SET PasswordHash = UPPER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CAST(PasswordHash AS VARCHAR(MAX))), 2))
WHERE PasswordHash IS NOT NULL;

USE SIMS_DB;
GO

UPDATE Users
SET PasswordHash = UPPER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CAST('emily123' AS VARCHAR(MAX))), 2))
WHERE Email = 'emily.rodriguez@sims.edu';
GO


UPDATE Users
SET PasswordHash = UPPER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CAST('alice123' AS VARCHAR(MAX))), 2))
WHERE Email = 'alice.smith@student.sims.edu';
GO

USE SIMS_DB
UPDATE Users
SET PasswordHash = CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'henry123'), 2)
WHERE Email = 'henry.white@student.sims.edu';
GO