-- =====================================================================
-- Database: GPA Management System
-- Language: T-SQL (SQL Server)
-- Version: 1.1 (Corrected)
-- =====================================================================

USE [master];
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'GPASystem')
BEGIN
    ALTER DATABASE [GPASystem] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [GPASystem];
END
GO

CREATE DATABASE [GPASystem];
GO

USE [GPASystem];
GO

-- =====================================================================
-- 1. Lookup / Reference Tables
-- =====================================================================

CREATE TABLE Department (
    department_id   INT IDENTITY(1,1) PRIMARY KEY,
    department_code NVARCHAR(10) NOT NULL UNIQUE,
    department_name NVARCHAR(100) NOT NULL,
    created_at      DATETIME2 DEFAULT GETUTCDATE()
);
GO

CREATE TABLE Semester (
    semester_id     INT IDENTITY(1,1) PRIMARY KEY,
    semester_name   NVARCHAR(50) NOT NULL,
    start_date      DATE NOT NULL,
    end_date        DATE NOT NULL,
    is_current      BIT NOT NULL DEFAULT 0,
    CONSTRAINT CHK_Semester_Dates CHECK (end_date > start_date)
);
GO

CREATE TABLE GradingPolicy (
    policy_id       INT IDENTITY(1,1) PRIMARY KEY,
    letter_grade    NVARCHAR(3) NOT NULL,
    min_percentage  DECIMAL(5,2) NOT NULL,
    max_percentage  DECIMAL(5,2) NOT NULL,
    grade_point     DECIMAL(3,2) NOT NULL,
    is_active       BIT NOT NULL DEFAULT 1,
    effective_from  DATE NOT NULL,
    CONSTRAINT CHK_GradingPolicy_Range CHECK (min_percentage <= max_percentage),
    CONSTRAINT CHK_GradingPolicy_Value CHECK (grade_point BETWEEN 0 AND 4.33)
);
GO

CREATE TABLE Configuration (
    config_key      NVARCHAR(100) PRIMARY KEY,
    config_value    NVARCHAR(500) NOT NULL,
    description     NVARCHAR(255) NULL,
    updated_at      DATETIME2 DEFAULT GETUTCDATE()
);
GO

INSERT INTO Configuration (config_key, config_value, description) VALUES
    ('pass_fail_cutoff', '50', 'Minimum percentage to pass a course'),
    ('warning_gpa', '2.0', 'GPA below this generates warning list'),
    ('max_repeat_attempts', '2', 'Maximum number of times a course can be repeated');
GO

-- =====================================================================
-- 2. User & Role Management
-- =====================================================================

CREATE TABLE AppUser (
    user_id          INT IDENTITY(1,1) PRIMARY KEY,
    username         NVARCHAR(50) NOT NULL UNIQUE,
    password_hash    NVARCHAR(255) NOT NULL,
    email            NVARCHAR(100) NOT NULL UNIQUE,
    role             NVARCHAR(20) NOT NULL CHECK (role IN ('ADMIN', 'INSTRUCTOR', 'STUDENT')),
    is_active        BIT NOT NULL DEFAULT 1,
    last_login       DATETIME2 NULL,
    password_changed_at DATETIME2 DEFAULT GETUTCDATE(),
    created_at       DATETIME2 DEFAULT GETUTCDATE()
);
GO

CREATE TABLE Administrator (
    admin_id         INT IDENTITY(1,1) PRIMARY KEY,
    user_id          INT NOT NULL UNIQUE REFERENCES AppUser(user_id) ON DELETE CASCADE,
    full_name        NVARCHAR(100) NOT NULL
);
GO

CREATE TABLE Instructor (
    instructor_id    INT IDENTITY(1,1) PRIMARY KEY,
    user_id          INT NOT NULL UNIQUE REFERENCES AppUser(user_id) ON DELETE CASCADE,
    full_name        NVARCHAR(100) NOT NULL,
    department_id    INT NOT NULL REFERENCES Department(department_id),
    hire_date        DATE NOT NULL DEFAULT CAST(GETDATE() AS DATE)
);
GO

CREATE TABLE Student (
    student_id       INT IDENTITY(1,1) PRIMARY KEY,
    user_id          INT NOT NULL UNIQUE REFERENCES AppUser(user_id) ON DELETE CASCADE,
    student_number   NVARCHAR(20) NOT NULL UNIQUE,
    full_name        NVARCHAR(100) NOT NULL,
    phone            NVARCHAR(20) NULL,
    department_id    INT NOT NULL REFERENCES Department(department_id),
    enrollment_date  DATE NOT NULL DEFAULT CAST(GETDATE() AS DATE),
    status           NVARCHAR(20) NOT NULL DEFAULT 'ACTIVE' CHECK (status IN ('ACTIVE', 'INACTIVE', 'GRADUATED'))
);
GO

-- =====================================================================
-- 3. Course Management
-- =====================================================================

CREATE TABLE Course (
    course_id        INT IDENTITY(1,1) PRIMARY KEY,
    course_code      NVARCHAR(20) NOT NULL UNIQUE,
    course_title     NVARCHAR(200) NOT NULL,
    credit_hours     TINYINT NOT NULL CHECK (credit_hours > 0),
    department_id    INT NOT NULL REFERENCES Department(department_id),
    description      NVARCHAR(500) NULL,
    created_at       DATETIME2 DEFAULT GETUTCDATE()
);
GO

CREATE TABLE CoursePrerequisite (
    course_id                INT NOT NULL REFERENCES Course(course_id) ON DELETE CASCADE,
    prerequisite_course_id   INT NOT NULL REFERENCES Course(course_id),
    PRIMARY KEY (course_id, prerequisite_course_id),
    CONSTRAINT CHK_NoSelfPrerequisite CHECK (course_id <> prerequisite_course_id)
);
GO

-- =====================================================================
-- 4. Course Offering
-- =====================================================================

CREATE TABLE CourseOffering (
    offering_id          INT IDENTITY(1,1) PRIMARY KEY,
    course_id            INT NOT NULL REFERENCES Course(course_id),
    semester_id          INT NOT NULL REFERENCES Semester(semester_id),
    instructor_id        INT NOT NULL REFERENCES Instructor(instructor_id),
    max_capacity         INT NOT NULL CHECK (max_capacity > 0),
    current_enrollment   INT NOT NULL DEFAULT 0 CHECK (current_enrollment >= 0),
    is_grade_finalized   BIT NOT NULL DEFAULT 0,
    status               NVARCHAR(20) DEFAULT 'ACTIVE' CHECK (status IN ('ACTIVE', 'COMPLETED', 'CANCELLED')),
    UNIQUE (course_id, semester_id, instructor_id)
);
GO

-- =====================================================================
-- 5. Enrollment
-- =====================================================================

CREATE TABLE Enrollment (
    enrollment_id        INT IDENTITY(1,1) PRIMARY KEY,
    student_id           INT NOT NULL REFERENCES Student(student_id),
    offering_id          INT NOT NULL REFERENCES CourseOffering(offering_id),
    enrollment_date      DATETIME2 DEFAULT GETUTCDATE(),
    status               NVARCHAR(20) DEFAULT 'ENROLLED' CHECK (status IN ('ENROLLED', 'DROPPED', 'COMPLETED')),
    is_repeated          BIT NOT NULL DEFAULT 0,
    original_enrollment_id INT NULL REFERENCES Enrollment(enrollment_id),
    CONSTRAINT UQ_StudentOffering UNIQUE (student_id, offering_id)
);
GO

-- =====================================================================
-- 6. Grade Components
-- =====================================================================

CREATE TABLE GradeComponent (
    component_id     INT IDENTITY(1,1) PRIMARY KEY,
    offering_id      INT NOT NULL REFERENCES CourseOffering(offering_id) ON DELETE CASCADE,
    component_name   NVARCHAR(100) NOT NULL,
    max_points       DECIMAL(6,2) NOT NULL CHECK (max_points > 0),
    sort_order       TINYINT DEFAULT 0,
    UNIQUE (offering_id, component_name)
);
GO

-- =====================================================================
-- 7. Grade Entry (marks per student per component)
-- Fixed: removed invalid CHECK constraint with subquery
-- Will enforce obtained_marks <= max_points via trigger (see below)
-- =====================================================================

CREATE TABLE GradeEntry (
    grade_entry_id   INT IDENTITY(1,1) PRIMARY KEY,
    enrollment_id    INT NOT NULL REFERENCES Enrollment(enrollment_id),
    component_id     INT NOT NULL REFERENCES GradeComponent(component_id),
    obtained_marks   DECIMAL(6,2) NOT NULL CHECK (obtained_marks >= 0),
    recorded_by      INT NOT NULL REFERENCES Instructor(instructor_id),
    recorded_at      DATETIME2 DEFAULT GETUTCDATE(),
    last_modified_at DATETIME2 DEFAULT GETUTCDATE(),
    UNIQUE (enrollment_id, component_id)
);
GO

-- =====================================================================
-- 8. Final Course Grade (calculated after finalization)
-- =====================================================================

CREATE TABLE CourseGrade (
    grade_id         INT IDENTITY(1,1) PRIMARY KEY,
    enrollment_id    INT NOT NULL UNIQUE REFERENCES Enrollment(enrollment_id),
    total_obtained   DECIMAL(8,2) NOT NULL,
    max_possible     DECIMAL(8,2) NOT NULL,
    percentage       DECIMAL(5,2) NOT NULL CHECK (percentage BETWEEN 0 AND 100),
    letter_grade     NVARCHAR(3) NOT NULL,
    grade_points     DECIMAL(4,2) NOT NULL,
    is_repeated_attempt BIT NOT NULL DEFAULT 0,
    calculated_at    DATETIME2 DEFAULT GETUTCDATE()
);
GO

-- =====================================================================
-- 9. Academic Record (semester GPA and CGPA)
-- =====================================================================

CREATE TABLE AcademicRecord (
    record_id               INT IDENTITY(1,1) PRIMARY KEY,
    student_id              INT NOT NULL REFERENCES Student(student_id),
    semester_id             INT NOT NULL REFERENCES Semester(semester_id),
    semester_gpa            DECIMAL(4,2) NOT NULL,
    cumulative_gpa          DECIMAL(4,2) NOT NULL,
    total_credits_attempted INT NOT NULL,
    total_grade_points      DECIMAL(8,2) NOT NULL,
    calculation_date        DATETIME2 DEFAULT GETUTCDATE(),
    UNIQUE (student_id, semester_id)
);
GO

-- =====================================================================
-- 10. Attendance
-- =====================================================================

CREATE TABLE Attendance (
    attendance_id    INT IDENTITY(1,1) PRIMARY KEY,
    offering_id      INT NOT NULL REFERENCES CourseOffering(offering_id),
    student_id       INT NOT NULL REFERENCES Student(student_id),
    attendance_date  DATE NOT NULL,
    status           NVARCHAR(10) NOT NULL CHECK (status IN ('PRESENT', 'ABSENT', 'LATE')),
    recorded_by      INT NOT NULL REFERENCES Instructor(instructor_id),
    recorded_at      DATETIME2 DEFAULT GETUTCDATE(),
    UNIQUE (offering_id, student_id, attendance_date)
);
GO

-- =====================================================================
-- 11. Audit Log
-- =====================================================================

CREATE TABLE AuditLog (
    log_id           BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_id          INT NOT NULL REFERENCES AppUser(user_id),
    action_type      NVARCHAR(50) NOT NULL,
    table_name       NVARCHAR(100) NULL,
    record_id        INT NULL,
    old_value        NVARCHAR(MAX) NULL,
    new_value        NVARCHAR(MAX) NULL,
    ip_address       NVARCHAR(45) NULL,
    logged_at        DATETIME2 DEFAULT GETUTCDATE()
);
GO

-- =====================================================================
-- 12. Notification
-- =====================================================================

CREATE TABLE Notification (
    notification_id  BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_id          INT NOT NULL REFERENCES AppUser(user_id),
    type             NVARCHAR(20) NOT NULL CHECK (type IN ('EMAIL', 'IN_APP')),
    subject          NVARCHAR(255) NOT NULL,
    message_body     NVARCHAR(MAX) NOT NULL,
    sent_status      NVARCHAR(20) DEFAULT 'PENDING' CHECK (sent_status IN ('PENDING', 'SENT', 'FAILED')),
    sent_at          DATETIME2 NULL,
    read_at          DATETIME2 NULL,
    created_at       DATETIME2 DEFAULT GETUTCDATE()
);
GO

-- =====================================================================
-- 13. Backup Log
-- =====================================================================

CREATE TABLE BackupLog (
    backup_id        INT IDENTITY(1,1) PRIMARY KEY,
    backup_file_name NVARCHAR(255) NOT NULL,
    backup_size_mb   DECIMAL(10,2) NULL,
    backup_type      NVARCHAR(20) NOT NULL CHECK (backup_type IN ('FULL', 'INCREMENTAL')),
    status           NVARCHAR(20) DEFAULT 'SUCCESS',
    error_message    NVARCHAR(500) NULL,
    performed_by     INT NOT NULL REFERENCES AppUser(user_id),
    performed_at     DATETIME2 DEFAULT GETUTCDATE()
);
GO

-- =====================================================================
-- Indexes for Performance
-- =====================================================================

CREATE INDEX IX_Student_StudentNumber ON Student(student_number);
CREATE INDEX IX_Student_FullName ON Student(full_name);
CREATE INDEX IX_Student_Department ON Student(department_id);
CREATE INDEX IX_Student_Status ON Student(status);

CREATE INDEX IX_Course_Code ON Course(course_code);
CREATE INDEX IX_Course_Title ON Course(course_title);

CREATE INDEX IX_Enrollment_Student ON Enrollment(student_id);
CREATE INDEX IX_Enrollment_Offering ON Enrollment(offering_id);
CREATE INDEX IX_Enrollment_Status ON Enrollment(status);

CREATE INDEX IX_GradeEntry_Enrollment ON GradeEntry(enrollment_id);
CREATE INDEX IX_GradeEntry_Component ON GradeEntry(component_id);

CREATE INDEX IX_Attendance_Offering ON Attendance(offering_id);
CREATE INDEX IX_Attendance_Student ON Attendance(student_id);
CREATE INDEX IX_Attendance_Date ON Attendance(attendance_date);

CREATE INDEX IX_AuditLog_User ON AuditLog(user_id);
CREATE INDEX IX_AuditLog_Action ON AuditLog(action_type);
CREATE INDEX IX_AuditLog_LoggedAt ON AuditLog(logged_at);

CREATE INDEX IX_Notification_Status ON Notification(sent_status) WHERE sent_status = 'PENDING';

CREATE INDEX IX_AcademicRecord_CGPA ON AcademicRecord(cumulative_gpa DESC);
CREATE INDEX IX_AcademicRecord_StudentSemester ON AcademicRecord(student_id, semester_id);

CREATE INDEX IX_CourseOffering_Semester ON CourseOffering(semester_id);
CREATE INDEX IX_CourseOffering_Instructor ON CourseOffering(instructor_id);
CREATE INDEX IX_CourseOffering_GradeFinalized ON CourseOffering(is_grade_finalized) WHERE is_grade_finalized = 0;

GO

-- =====================================================================
-- Trigger 1: Enforce max capacity on enrollment (FR-012)
-- =====================================================================
CREATE TRIGGER trg_Enrollment_CheckCapacity
ON Enrollment
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN CourseOffering co ON i.offering_id = co.offering_id
        WHERE co.current_enrollment + 1 > co.max_capacity
    )
    BEGIN
        RAISERROR('Course enrollment would exceed maximum capacity.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    UPDATE co
    SET current_enrollment = co.current_enrollment + 1
    FROM CourseOffering co
    INNER JOIN inserted i ON co.offering_id = i.offering_id
    WHERE i.status = 'ENROLLED';
END;
GO

-- =====================================================================
-- Trigger 2: Ensure obtained_marks does not exceed component's max_points
-- Replaces the invalid CHECK constraint
-- =====================================================================
CREATE TRIGGER trg_GradeEntry_CheckMaxPoints
ON GradeEntry
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN GradeComponent gc ON i.component_id = gc.component_id
        WHERE i.obtained_marks > gc.max_points
    )
    BEGIN
        RAISERROR('Obtained marks cannot exceed the component''s maximum points.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;
GO

-- =====================================================================
-- Optional trigger to prevent grade modification after finalization
-- (Enforces FR-045 at DB level; can be extended)
-- =====================================================================
CREATE TRIGGER trg_PreventGradeChangeAfterFinalization
ON GradeEntry
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN Enrollment e ON i.enrollment_id = e.enrollment_id
        INNER JOIN CourseOffering co ON e.offering_id = co.offering_id
        WHERE co.is_grade_finalized = 1
    )
    BEGIN
        RAISERROR('Grades are finalized and locked. Modifications not allowed.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;
GO

PRINT 'GPASystem database schema created successfully with all triggers.';
GO