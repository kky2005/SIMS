Modules: 

HOP
Lecturer
Student
Login

MUST ADD FIRST USER AS ADMIN IN THE DATABASE: 

INSERT INTO Users
(
    RoleId,
    FullName,
    Email,
    PasswordHash,
    Phone,
    PhotoUrl,
    IsActive,
    CreatedAt,
    LastLoginAt
)
VALUES
(
    1,
    'System Administrator',
    'admin@sims.com',
    '240BE518FABD2724DDB6F04EEBEECF656B2DFA4FDFDDAAFB6E5E6F0FECF0E4F5',
    '0123456789',
    NULL,
    1,
    SYSDATETIME(),
    NULL
);
