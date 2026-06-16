/*
use SIMS_DB
GO
Select * from Users
Select * from Courses
SELECT 
    u.FullName,
    s.StudentNo,
    s.CurrentSemester
FROM Users u
INNER JOIN Students s ON u.UserId = s.UserId
WHERE u.Email = 'henry.white@student.sims.edu';

use SIMS_DB
GO
-- 1. Approve Alice's CS301 request
UPDATE CourseRegistrationRequests
SET Status = 'Approved',
    ReviewedAt = SYSUTCDATETIME(),
    AdminRemarks = 'Approved for registration'
WHERE StudentId = 1
  AND CourseId = 5
  AND RequestType = 'Register'
  AND Status = 'Pending';

-- 2. Add course into Enrolments
INSERT INTO Enrolments (StudentId, CourseId, AcademicYear, Semester, Status, EnrolledAt)
VALUES (1, 5, 2024, 3, 'Active', SYSUTCDATETIME());



--DRopping Course Testing ---
USE SIMS_DB;
GO

-- 1. Approve Alice's pending drop request for CS301
UPDATE CourseRegistrationRequests
SET Status = 'Approved',
    ReviewedAt = SYSUTCDATETIME(),
    AdminRemarks = 'Drop request approved for testing'
WHERE StudentId = (
        SELECT StudentId 
        FROM Students 
        WHERE StudentNo = 'STU001'
)
AND CourseId = (
        SELECT CourseId 
        FROM Courses 
        WHERE CourseCode = 'CS301'
)
AND RequestType = 'Drop'
AND Status = 'Pending';
GO

-- 2. Mark Alice's CS301 enrolment as dropped
UPDATE Enrolments
SET Status = 'Dropped',
    DroppedAt = SYSUTCDATETIME()
WHERE StudentId = (
        SELECT StudentId 
        FROM Students 
        WHERE StudentNo = 'STU001'
)
AND CourseId = (
        SELECT CourseId 
        FROM Courses 
        WHERE CourseCode = 'CS301'
)
AND Status = 'Active';
GO
*/
-----TESTING TESTING REGISTERING COURSE---------
USE SIMS_DB;
GO

SELECT 
    s.StudentId,
    s.StudentNo,
    u.FullName,
    s.CurrentSemester,
    p.ProgrammeName
FROM Students s
INNER JOIN Users u ON s.UserId = u.UserId
INNER JOIN Programmes p ON s.ProgrammeId = p.ProgrammeId
WHERE s.StudentNo = 'STU001';

--Close the registration and drop period--
USE SIMS_DB;
GO

UPDATE RegistrationPeriods
SET IsActive = 0
WHERE ProgrammeId = (
    SELECT ProgrammeId FROM Students WHERE StudentNo = 'STU001'
)
AND Semester = (
    SELECT CurrentSemester FROM Students WHERE StudentNo = 'STU001'
)
AND PeriodType IN ('Registration', 'Drop');
GO

Select * from RegistrationPeriods

--Open Registration and Drop period for testing--
USE SIMS_DB;
GO

UPDATE RegistrationPeriods
SET StartDate = CAST(GETDATE() AS DATE),
    EndDate = DATEADD(DAY, 30, CAST(GETDATE() AS DATE)),
    IsActive = 1
WHERE ProgrammeId = (
    SELECT ProgrammeId FROM Students WHERE StudentNo = 'STU001'
)
AND Semester = (
    SELECT CurrentSemester FROM Students WHERE StudentNo = 'STU001'
)
AND PeriodType = 'Registration';

UPDATE RegistrationPeriods
SET IsActive = 0
WHERE ProgrammeId = (
    SELECT ProgrammeId FROM Students WHERE StudentNo = 'STU001'
)
AND Semester = (
    SELECT CurrentSemester FROM Students WHERE StudentNo = 'STU001'
)
AND PeriodType = 'Drop';
GO