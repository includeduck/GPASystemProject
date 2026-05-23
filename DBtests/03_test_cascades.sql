-- =====================================================================
-- GPA System Database Unit Tests: Referential Integrity and Cascading Deletes
-- Language: T-SQL (SQL Server)
-- =====================================================================

USE [GPASystem];
GO

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
GO

-- Initialize or clear test results table
IF OBJECT_ID('tempdb..#TestResults_Cascades') IS NOT NULL DROP TABLE #TestResults_Cascades;
CREATE TABLE #TestResults_Cascades (
    TestName NVARCHAR(100) NOT NULL,
    Result NVARCHAR(10) NOT NULL, -- 'PASS' or 'FAIL'
    ErrorMessage NVARCHAR(MAX) NULL
);
GO

PRINT '==================================================';
PRINT '     RUNNING MODULE 03: DATABASE CASCADE TESTS    ';
PRINT '==================================================';

-- Start of single batch to preserve variable scopes
DECLARE @PassCount INT = 0;
DECLARE @FailCount INT = 0;
DECLARE @ErrorMsg NVARCHAR(MAX);

-- ---------------------------------------------------------------------
-- Test 1: AppUser Deletion Cascades to Student Profile
-- ---------------------------------------------------------------------
BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @DeptId INT, @UserId INT, @StudentId INT;
    DECLARE @IsStudentDeleted BIT = 0;
    
    INSERT INTO Department (department_code, department_name) VALUES ('TDEPT', 'Testing Dept');
    SET @DeptId = SCOPE_IDENTITY();

    INSERT INTO AppUser (username, password_hash, email, role, is_active)
    VALUES ('stud1', 'hash', 'stud1@test.com', 'STUDENT', 1);
    SET @UserId = SCOPE_IDENTITY();

    INSERT INTO Student (user_id, student_number, full_name, department_id, enrollment_date, status)
    VALUES (@UserId, 'STUD-001', 'Alice', @DeptId, '2026-01-01', 'ACTIVE');
    SET @StudentId = SCOPE_IDENTITY();

    -- Delete the user
    DELETE FROM AppUser WHERE user_id = @UserId;

    -- Verify that the student profile row is deleted automatically
    IF NOT EXISTS (SELECT 1 FROM Student WHERE student_id = @StudentId)
    BEGIN
        SET @IsStudentDeleted = 1;
    END

    ROLLBACK TRANSACTION;

    IF @IsStudentDeleted = 1
    BEGIN
        INSERT INTO #TestResults_Cascades (TestName, Result, ErrorMessage)
        VALUES ('Test_Cascade_User_To_Student', 'PASS', NULL);
    END
    ELSE
    BEGIN
        INSERT INTO #TestResults_Cascades (TestName, Result, ErrorMessage)
        VALUES ('Test_Cascade_User_To_Student', 'FAIL', 'Student profile remained after deleting the parent AppUser row.');
    END
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    SET @ErrorMsg = ERROR_MESSAGE();
    INSERT INTO #TestResults_Cascades (TestName, Result, ErrorMessage)
    VALUES ('Test_Cascade_User_To_Student', 'FAIL', 'Transaction failed during delete cascade test. Error: ' + @ErrorMsg);
END CATCH;

-- ---------------------------------------------------------------------
-- Test 2: AppUser Deletion Cascades to Instructor Profile
-- ---------------------------------------------------------------------
BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @IDeptId INT, @IUserId INT, @IInstId INT;
    DECLARE @IsInstructorDeleted BIT = 0;
    
    INSERT INTO Department (department_code, department_name) VALUES ('TDEPT', 'Testing Dept');
    SET @IDeptId = SCOPE_IDENTITY();

    INSERT INTO AppUser (username, password_hash, email, role, is_active)
    VALUES ('inst1', 'hash', 'inst1@test.com', 'INSTRUCTOR', 1);
    SET @IUserId = SCOPE_IDENTITY();

    INSERT INTO Instructor (user_id, full_name, department_id, hire_date)
    VALUES (@IUserId, 'Dr. Tester', @IDeptId, '2026-01-01');
    SET @IInstId = SCOPE_IDENTITY();

    -- Delete the user
    DELETE FROM AppUser WHERE user_id = @IUserId;

    -- Verify that the instructor profile row is deleted automatically
    IF NOT EXISTS (SELECT 1 FROM Instructor WHERE instructor_id = @IInstId)
    BEGIN
        SET @IsInstructorDeleted = 1;
    END

    ROLLBACK TRANSACTION;

    IF @IsInstructorDeleted = 1
    BEGIN
        INSERT INTO #TestResults_Cascades (TestName, Result, ErrorMessage)
        VALUES ('Test_Cascade_User_To_Instructor', 'PASS', NULL);
    END
    ELSE
    BEGIN
        INSERT INTO #TestResults_Cascades (TestName, Result, ErrorMessage)
        VALUES ('Test_Cascade_User_To_Instructor', 'FAIL', 'Instructor profile remained after deleting the parent AppUser row.');
    END
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    SET @ErrorMsg = ERROR_MESSAGE();
    INSERT INTO #TestResults_Cascades (TestName, Result, ErrorMessage)
    VALUES ('Test_Cascade_User_To_Instructor', 'FAIL', 'Transaction failed during delete cascade test. Error: ' + @ErrorMsg);
END CATCH;

-- ---------------------------------------------------------------------
-- Test 3: AppUser Deletion Cascades to Administrator Profile
-- ---------------------------------------------------------------------
BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @AUserId INT, @AAdminId INT;
    DECLARE @IsAdminDeleted BIT = 0;
    
    INSERT INTO AppUser (username, password_hash, email, role, is_active)
    VALUES ('admin1', 'hash', 'admin1@test.com', 'ADMIN', 1);
    SET @AUserId = SCOPE_IDENTITY();

    INSERT INTO Administrator (user_id, full_name)
    VALUES (@AUserId, 'Super Admin');
    SET @AAdminId = SCOPE_IDENTITY();

    -- Delete the user
    DELETE FROM AppUser WHERE user_id = @AUserId;

    -- Verify that the admin profile row is deleted automatically
    IF NOT EXISTS (SELECT 1 FROM Administrator WHERE admin_id = @AAdminId)
    BEGIN
        SET @IsAdminDeleted = 1;
    END

    ROLLBACK TRANSACTION;

    IF @IsAdminDeleted = 1
    BEGIN
        INSERT INTO #TestResults_Cascades (TestName, Result, ErrorMessage)
        VALUES ('Test_Cascade_User_To_Admin', 'PASS', NULL);
    END
    ELSE
    BEGIN
        INSERT INTO #TestResults_Cascades (TestName, Result, ErrorMessage)
        VALUES ('Test_Cascade_User_To_Admin', 'FAIL', 'Administrator profile remained after deleting the parent AppUser row.');
    END
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    SET @ErrorMsg = ERROR_MESSAGE();
    INSERT INTO #TestResults_Cascades (TestName, Result, ErrorMessage)
    VALUES ('Test_Cascade_User_To_Admin', 'FAIL', 'Transaction failed during delete cascade test. Error: ' + @ErrorMsg);
END CATCH;

-- ---------------------------------------------------------------------
-- Test 4: CourseOffering Deletion Cascades to GradeComponents
-- ---------------------------------------------------------------------
BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @CDeptId INT, @CCourseId INT, @CSemesterId INT, @CUserId INT, @CInstId INT, @COfferingId INT;
    DECLARE @CCompId INT;
    DECLARE @IsCompDeleted BIT = 0;
    
    INSERT INTO Department (department_code, department_name) VALUES ('TDEPT', 'Testing Dept');
    SET @CDeptId = SCOPE_IDENTITY();

    INSERT INTO Course (course_code, course_title, credit_hours, department_id, description)
    VALUES ('CS-101', 'Intro to CS', 3, @CDeptId, 'CS 101');
    SET @CCourseId = SCOPE_IDENTITY();

    INSERT INTO Semester (semester_name, start_date, end_date, is_current)
    VALUES ('Fall 2026', '2026-09-01', '2026-12-20', 0);
    SET @CSemesterId = SCOPE_IDENTITY();

    INSERT INTO AppUser (username, password_hash, email, role, is_active)
    VALUES ('inst1', 'hash', 'inst1@test.com', 'INSTRUCTOR', 1);
    SET @CUserId = SCOPE_IDENTITY();

    INSERT INTO Instructor (user_id, full_name, department_id, hire_date)
    VALUES (@CUserId, 'Dr. Tester', @CDeptId, '2026-01-01');
    SET @CInstId = SCOPE_IDENTITY();

    INSERT INTO CourseOffering (course_id, semester_id, instructor_id, max_capacity, current_enrollment, status)
    VALUES (@CCourseId, @CSemesterId, @CInstId, 30, 0, 'ACTIVE');
    SET @COfferingId = SCOPE_IDENTITY();

    INSERT INTO GradeComponent (offering_id, component_name, max_points, sort_order)
    VALUES (@COfferingId, 'Midterm', 30.00, 1);
    SET @CCompId = SCOPE_IDENTITY();

    -- Delete CourseOffering
    DELETE FROM CourseOffering WHERE offering_id = @COfferingId;

    -- Verify that GradeComponent was deleted
    IF NOT EXISTS (SELECT 1 FROM GradeComponent WHERE component_id = @CCompId)
    BEGIN
        SET @IsCompDeleted = 1;
    END

    ROLLBACK TRANSACTION;

    IF @IsCompDeleted = 1
    BEGIN
        INSERT INTO #TestResults_Cascades (TestName, Result, ErrorMessage)
        VALUES ('Test_Cascade_Offering_To_GradeComponent', 'PASS', NULL);
    END
    ELSE
    BEGIN
        INSERT INTO #TestResults_Cascades (TestName, Result, ErrorMessage)
        VALUES ('Test_Cascade_Offering_To_GradeComponent', 'FAIL', 'GradeComponent remained after deleting the parent CourseOffering row.');
    END
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    SET @ErrorMsg = ERROR_MESSAGE();
    INSERT INTO #TestResults_Cascades (TestName, Result, ErrorMessage)
    VALUES ('Test_Cascade_Offering_To_GradeComponent', 'FAIL', 'Transaction failed during delete cascade test. Error: ' + @ErrorMsg);
END CATCH;

-- ---------------------------------------------------------------------
-- Report Module 03 results
-- ---------------------------------------------------------------------
DECLARE @TestName NVARCHAR(100), @Result NVARCHAR(10), @ErrMsg NVARCHAR(MAX);
DECLARE test_cursor CURSOR FOR SELECT TestName, Result, ErrorMessage FROM #TestResults_Cascades;
OPEN test_cursor;
FETCH NEXT FROM test_cursor INTO @TestName, @Result, @ErrMsg;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF @Result = 'PASS'
    BEGIN
        PRINT '[PASS] ' + @TestName;
        SET @PassCount = @PassCount + 1;
    END
    ELSE
    BEGIN
        PRINT '[FAIL] ' + @TestName + ' - Reason: ' + ISNULL(@ErrMsg, 'Unknown');
        SET @FailCount = @FailCount + 1;
    END
    FETCH NEXT FROM test_cursor INTO @TestName, @Result, @ErrMsg;
END;
CLOSE test_cursor;
DEALLOCATE test_cursor;

PRINT '--------------------------------------------------';
PRINT 'Module 03 Summary: ' + CAST(@PassCount AS VARCHAR(5)) + ' passed, ' + CAST(@FailCount AS VARCHAR(5)) + ' failed.';
PRINT '==================================================';
-- End of single batch
GO
