-- =====================================================================
-- GPA System Database Unit Tests: CHECK and Integrity Constraints
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
IF OBJECT_ID('tempdb..#TestResults_Constraints') IS NOT NULL DROP TABLE #TestResults_Constraints;
CREATE TABLE #TestResults_Constraints (
    TestName NVARCHAR(100) NOT NULL,
    Result NVARCHAR(10) NOT NULL, -- 'PASS' or 'FAIL'
    ErrorMessage NVARCHAR(MAX) NULL
);
GO

PRINT '==================================================';
PRINT '  RUNNING MODULE 01: DATABASE CONSTRAINTS TESTS   ';
PRINT '==================================================';

-- Start of single batch to preserve variable scopes
DECLARE @PassCount INT = 0;
DECLARE @FailCount INT = 0;
DECLARE @ErrorMsg NVARCHAR(MAX);

-- ---------------------------------------------------------------------
-- Test 1: CHK_Semester_Dates - Invalid (Negative)
-- ---------------------------------------------------------------------
BEGIN TRY
    BEGIN TRANSACTION;
    
    INSERT INTO Semester (semester_name, start_date, end_date, is_current)
    VALUES ('Fall 2026', '2026-09-01', '2026-08-31', 0); -- End before start

    -- If it reaches here, the constraint was NOT enforced!
    ROLLBACK TRANSACTION;
    INSERT INTO #TestResults_Constraints (TestName, Result, ErrorMessage)
    VALUES ('Test_Semester_Dates_Invalid', 'FAIL', 'Inserted semester where end_date was before start_date without error.');
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    INSERT INTO #TestResults_Constraints (TestName, Result, ErrorMessage)
    VALUES ('Test_Semester_Dates_Invalid', 'PASS', NULL);
END CATCH;

-- ---------------------------------------------------------------------
-- Test 2: CHK_Semester_Dates - Valid (Positive)
-- ---------------------------------------------------------------------
BEGIN TRY
    BEGIN TRANSACTION;
    
    INSERT INTO Semester (semester_name, start_date, end_date, is_current)
    VALUES ('Fall 2026', '2026-09-01', '2026-12-20', 0);

    ROLLBACK TRANSACTION; -- Keep DB clean
    INSERT INTO #TestResults_Constraints (TestName, Result, ErrorMessage)
    VALUES ('Test_Semester_Dates_Valid', 'PASS', NULL);
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    SET @ErrorMsg = ERROR_MESSAGE();
    INSERT INTO #TestResults_Constraints (TestName, Result, ErrorMessage)
    VALUES ('Test_Semester_Dates_Valid', 'FAIL', 'Valid semester dates failed to insert. Error: ' + @ErrorMsg);
END CATCH;

-- ---------------------------------------------------------------------
-- Test 3: CHK_GradingPolicy_Range - Invalid (Negative)
-- ---------------------------------------------------------------------
BEGIN TRY
    BEGIN TRANSACTION;

    INSERT INTO GradingPolicy (letter_grade, min_percentage, max_percentage, grade_point, is_active, effective_from)
    VALUES ('A+', 95.00, 90.00, 4.33, 1, '2026-01-01'); -- min > max

    ROLLBACK TRANSACTION;
    INSERT INTO #TestResults_Constraints (TestName, Result, ErrorMessage)
    VALUES ('Test_GradingPolicy_Range_Invalid', 'FAIL', 'Inserted grading policy where min_percentage > max_percentage.');
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    INSERT INTO #TestResults_Constraints (TestName, Result, ErrorMessage)
    VALUES ('Test_GradingPolicy_Range_Invalid', 'PASS', NULL);
END CATCH;

-- ---------------------------------------------------------------------
-- Test 4: CHK_GradingPolicy_Value - Invalid High (Negative)
-- ---------------------------------------------------------------------
BEGIN TRY
    BEGIN TRANSACTION;

    INSERT INTO GradingPolicy (letter_grade, min_percentage, max_percentage, grade_point, is_active, effective_from)
    VALUES ('A+', 95.00, 100.00, 4.50, 1, '2026-01-01'); -- GPA > 4.33

    ROLLBACK TRANSACTION;
    INSERT INTO #TestResults_Constraints (TestName, Result, ErrorMessage)
    VALUES ('Test_GradingPolicy_Value_Invalid_High', 'FAIL', 'Inserted grading policy where grade_point was 4.50.');
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    INSERT INTO #TestResults_Constraints (TestName, Result, ErrorMessage)
    VALUES ('Test_GradingPolicy_Value_Invalid_High', 'PASS', NULL);
END CATCH;

-- ---------------------------------------------------------------------
-- Test 5: CHK_GradingPolicy_Value - Invalid Low (Negative)
-- ---------------------------------------------------------------------
BEGIN TRY
    BEGIN TRANSACTION;

    INSERT INTO GradingPolicy (letter_grade, min_percentage, max_percentage, grade_point, is_active, effective_from)
    VALUES ('F', 0.00, 49.99, -0.50, 1, '2026-01-01'); -- GPA < 0.00

    ROLLBACK TRANSACTION;
    INSERT INTO #TestResults_Constraints (TestName, Result, ErrorMessage)
    VALUES ('Test_GradingPolicy_Value_Invalid_Low', 'FAIL', 'Inserted grading policy where grade_point was negative.');
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    INSERT INTO #TestResults_Constraints (TestName, Result, ErrorMessage)
    VALUES ('Test_GradingPolicy_Value_Invalid_Low', 'PASS', NULL);
END CATCH;

-- ---------------------------------------------------------------------
-- Test 6: AppUser_Role_Check - Invalid (Negative)
-- ---------------------------------------------------------------------
BEGIN TRY
    BEGIN TRANSACTION;

    INSERT INTO AppUser (username, password_hash, email, role, is_active)
    VALUES ('bad_role_user', 'hash', 'badrole@test.com', 'GUEST', 1); -- GUEST is invalid role

    ROLLBACK TRANSACTION;
    INSERT INTO #TestResults_Constraints (TestName, Result, ErrorMessage)
    VALUES ('Test_AppUser_Role_Invalid', 'FAIL', 'Inserted AppUser with invalid role GUEST.');
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    INSERT INTO #TestResults_Constraints (TestName, Result, ErrorMessage)
    VALUES ('Test_AppUser_Role_Invalid', 'PASS', NULL);
END CATCH;

-- ---------------------------------------------------------------------
-- Test 7: Student_Status_Check - Invalid (Negative)
-- ---------------------------------------------------------------------
BEGIN TRY
    BEGIN TRANSACTION;

    -- Setup valid department and app user first
    DECLARE @DeptId INT, @UserId INT;
    INSERT INTO Department (department_code, department_name) VALUES ('TESTDEPT', 'Testing Dept');
    SET @DeptId = SCOPE_IDENTITY();
    
    INSERT INTO AppUser (username, password_hash, email, role, is_active)
    VALUES ('test_stud_status', 'hash', 'status@test.com', 'STUDENT', 1);
    SET @UserId = SCOPE_IDENTITY();

    INSERT INTO Student (user_id, student_number, full_name, department_id, enrollment_date, status)
    VALUES (@UserId, 'TEST-1234', 'Test Student', @DeptId, '2026-01-01', 'SUSPENDED'); -- SUSPENDED is invalid

    ROLLBACK TRANSACTION;
    INSERT INTO #TestResults_Constraints (TestName, Result, ErrorMessage)
    VALUES ('Test_Student_Status_Invalid', 'FAIL', 'Inserted Student with invalid status SUSPENDED.');
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    INSERT INTO #TestResults_Constraints (TestName, Result, ErrorMessage)
    VALUES ('Test_Student_Status_Invalid', 'PASS', NULL);
END CATCH;

-- ---------------------------------------------------------------------
-- Test 8: Course_CreditHours - Invalid (Negative)
-- ---------------------------------------------------------------------
BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @CourseDeptId INT;
    INSERT INTO Department (department_code, department_name) VALUES ('TDEPT', 'Testing Dept');
    SET @CourseDeptId = SCOPE_IDENTITY();

    INSERT INTO Course (course_code, course_title, credit_hours, department_id, description)
    VALUES ('CS-101', 'Intro to CS', 0, @CourseDeptId, 'Zero credits course'); -- Credit hours must be > 0

    ROLLBACK TRANSACTION;
    INSERT INTO #TestResults_Constraints (TestName, Result, ErrorMessage)
    VALUES ('Test_Course_CreditHours_Invalid', 'FAIL', 'Inserted course with 0 credit hours.');
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    INSERT INTO #TestResults_Constraints (TestName, Result, ErrorMessage)
    VALUES ('Test_Course_CreditHours_Invalid', 'PASS', NULL);
END CATCH;

-- ---------------------------------------------------------------------
-- Test 9: CoursePrerequisite_CHK_NoSelfPrerequisite - Invalid (Negative)
-- ---------------------------------------------------------------------
BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @PrereqDeptId INT, @CourseId INT;
    INSERT INTO Department (department_code, department_name) VALUES ('TDEPT', 'Testing Dept');
    SET @PrereqDeptId = SCOPE_IDENTITY();

    INSERT INTO Course (course_code, course_title, credit_hours, department_id, description)
    VALUES ('CS-101', 'Intro to CS', 3, @PrereqDeptId, 'CS 101');
    SET @CourseId = SCOPE_IDENTITY();

    INSERT INTO CoursePrerequisite (course_id, prerequisite_course_id)
    VALUES (@CourseId, @CourseId); -- Self prerequisite

    ROLLBACK TRANSACTION;
    INSERT INTO #TestResults_Constraints (TestName, Result, ErrorMessage)
    VALUES ('Test_Prerequisite_SelfReference_Invalid', 'FAIL', 'Inserted course prerequisite of self.');
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    INSERT INTO #TestResults_Constraints (TestName, Result, ErrorMessage)
    VALUES ('Test_Prerequisite_SelfReference_Invalid', 'PASS', NULL);
END CATCH;

-- ---------------------------------------------------------------------
-- Test 10: CourseOffering_Capacity - Invalid (Negative)
-- ---------------------------------------------------------------------
BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @OffDeptId INT, @OffCourseId INT, @OffSemesterId INT, @OffUserId INT, @OffInstId INT;
    
    INSERT INTO Department (department_code, department_name) VALUES ('TDEPT', 'Testing Dept');
    SET @OffDeptId = SCOPE_IDENTITY();

    INSERT INTO Course (course_code, course_title, credit_hours, department_id, description)
    VALUES ('CS-101', 'Intro to CS', 3, @OffDeptId, 'CS 101');
    SET @OffCourseId = SCOPE_IDENTITY();

    INSERT INTO Semester (semester_name, start_date, end_date, is_current)
    VALUES ('Fall 2026', '2026-09-01', '2026-12-20', 0);
    SET @OffSemesterId = SCOPE_IDENTITY();

    INSERT INTO AppUser (username, password_hash, email, role, is_active)
    VALUES ('inst1', 'hash', 'inst1@test.com', 'INSTRUCTOR', 1);
    SET @OffUserId = SCOPE_IDENTITY();

    INSERT INTO Instructor (user_id, full_name, department_id, hire_date)
    VALUES (@OffUserId, 'Dr. Tester', @OffDeptId, '2026-01-01');
    SET @OffInstId = SCOPE_IDENTITY();

    INSERT INTO CourseOffering (course_id, semester_id, instructor_id, max_capacity, current_enrollment, status)
    VALUES (@OffCourseId, @OffSemesterId, @OffInstId, 0, 0, 'ACTIVE'); -- max_capacity = 0 is invalid

    ROLLBACK TRANSACTION;
    INSERT INTO #TestResults_Constraints (TestName, Result, ErrorMessage)
    VALUES ('Test_Offering_Capacity_Invalid', 'FAIL', 'Inserted course offering with 0 max capacity.');
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    INSERT INTO #TestResults_Constraints (TestName, Result, ErrorMessage)
    VALUES ('Test_Offering_Capacity_Invalid', 'PASS', NULL);
END CATCH;

-- ---------------------------------------------------------------------
-- Test 11: GradeEntry_ObtainedMarks - Invalid (Negative)
-- ---------------------------------------------------------------------
BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @GDeptId INT, @GCourseId INT, @GSemesterId INT, @GUserId INT, @GInstId INT, @GOfferingId INT;
    DECLARE @GStudUserId INT, @GStudentId INT, @GEnrollId INT, @GCompId INT;
    
    INSERT INTO Department (department_code, department_name) VALUES ('TDEPT', 'Testing Dept');
    SET @GDeptId = SCOPE_IDENTITY();

    INSERT INTO Course (course_code, course_title, credit_hours, department_id, description)
    VALUES ('CS-101', 'Intro to CS', 3, @GDeptId, 'CS 101');
    SET @GCourseId = SCOPE_IDENTITY();

    INSERT INTO Semester (semester_name, start_date, end_date, is_current)
    VALUES ('Fall 2026', '2026-09-01', '2026-12-20', 0);
    SET @GSemesterId = SCOPE_IDENTITY();

    INSERT INTO AppUser (username, password_hash, email, role, is_active)
    VALUES ('inst1', 'hash', 'inst1@test.com', 'INSTRUCTOR', 1);
    SET @GUserId = SCOPE_IDENTITY();

    INSERT INTO Instructor (user_id, full_name, department_id, hire_date)
    VALUES (@GUserId, 'Dr. Tester', @GDeptId, '2026-01-01');
    SET @GInstId = SCOPE_IDENTITY();

    INSERT INTO CourseOffering (course_id, semester_id, instructor_id, max_capacity, current_enrollment, status)
    VALUES (@GCourseId, @GSemesterId, @GInstId, 30, 0, 'ACTIVE');
    SET @GOfferingId = SCOPE_IDENTITY();

    INSERT INTO AppUser (username, password_hash, email, role, is_active)
    VALUES ('stud1', 'hash', 'stud1@test.com', 'STUDENT', 1);
    SET @GStudUserId = SCOPE_IDENTITY();

    INSERT INTO Student (user_id, student_number, full_name, department_id, enrollment_date, status)
    VALUES (@GStudUserId, 'STUD-001', 'Alice', @GDeptId, '2026-01-01', 'ACTIVE');
    SET @GStudentId = SCOPE_IDENTITY();

    -- Enroll Alice
    INSERT INTO Enrollment (student_id, offering_id, status)
    VALUES (@GStudentId, @GOfferingId, 'ENROLLED');
    SET @GEnrollId = SCOPE_IDENTITY();

    INSERT INTO GradeComponent (offering_id, component_name, max_points, sort_order)
    VALUES (@GOfferingId, 'Midterm', 30.00, 1);
    SET @GCompId = SCOPE_IDENTITY();

    -- Try to insert negative marks
    INSERT INTO GradeEntry (enrollment_id, component_id, obtained_marks, recorded_by)
    VALUES (@GEnrollId, @GCompId, -5.00, @GInstId); -- Negative marks not allowed

    ROLLBACK TRANSACTION;
    INSERT INTO #TestResults_Constraints (TestName, Result, ErrorMessage)
    VALUES ('Test_GradeEntry_ObtainedMarks_Invalid', 'FAIL', 'Inserted negative obtained marks.');
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    INSERT INTO #TestResults_Constraints (TestName, Result, ErrorMessage)
    VALUES ('Test_GradeEntry_ObtainedMarks_Invalid', 'PASS', NULL);
END CATCH;

-- ---------------------------------------------------------------------
-- Report Module 01 results
-- ---------------------------------------------------------------------
DECLARE @TestName NVARCHAR(100), @Result NVARCHAR(10), @ErrMsg NVARCHAR(MAX);
DECLARE test_cursor CURSOR FOR SELECT TestName, Result, ErrorMessage FROM #TestResults_Constraints;
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
PRINT 'Module 01 Summary: ' + CAST(@PassCount AS VARCHAR(5)) + ' passed, ' + CAST(@FailCount AS VARCHAR(5)) + ' failed.';
PRINT '==================================================';
-- End of single batch
GO
