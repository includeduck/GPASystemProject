-- =====================================================================
-- GPA System Database Unit Tests: Triggers
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
IF OBJECT_ID('tempdb..#TestResults_Triggers') IS NOT NULL DROP TABLE #TestResults_Triggers;
CREATE TABLE #TestResults_Triggers (
    TestName NVARCHAR(100) NOT NULL,
    Result NVARCHAR(10) NOT NULL, -- 'PASS' or 'FAIL'
    ErrorMessage NVARCHAR(MAX) NULL
);
GO

PRINT '==================================================';
PRINT '     RUNNING MODULE 02: DATABASE TRIGGERS TESTS   ';
PRINT '==================================================';

-- Start of single batch to preserve variable scopes
DECLARE @PassCount INT = 0;
DECLARE @FailCount INT = 0;
DECLARE @ErrorMsg NVARCHAR(MAX);

-- ---------------------------------------------------------------------
-- Test 1: trg_Enrollment_CheckCapacity - Under Capacity (Positive)
-- ---------------------------------------------------------------------
BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @DeptId INT, @CourseId INT, @SemesterId INT, @UserId INT, @InstId INT, @OfferingId INT;
    DECLARE @StudUserId INT, @StudentId INT, @InitialEnrollment INT, @FinalEnrollment INT;
    
    -- Setup basic infrastructure
    INSERT INTO Department (department_code, department_name) VALUES ('TDEPT', 'Testing Dept');
    SET @DeptId = SCOPE_IDENTITY();

    INSERT INTO Course (course_code, course_title, credit_hours, department_id, description)
    VALUES ('CS-101', 'Intro to CS', 3, @DeptId, 'CS 101');
    SET @CourseId = SCOPE_IDENTITY();

    INSERT INTO Semester (semester_name, start_date, end_date, is_current)
    VALUES ('Fall 2026', '2026-09-01', '2026-12-20', 0);
    SET @SemesterId = SCOPE_IDENTITY();

    INSERT INTO AppUser (username, password_hash, email, role, is_active)
    VALUES ('inst1', 'hash', 'inst1@test.com', 'INSTRUCTOR', 1);
    SET @UserId = SCOPE_IDENTITY();

    INSERT INTO Instructor (user_id, full_name, department_id, hire_date)
    VALUES (@UserId, 'Dr. Tester', @DeptId, '2026-01-01');
    SET @InstId = SCOPE_IDENTITY();

    -- Create offering with capacity = 2, current_enrollment = 0
    INSERT INTO CourseOffering (course_id, semester_id, instructor_id, max_capacity, current_enrollment, status)
    VALUES (@CourseId, @SemesterId, @InstId, 2, 0, 'ACTIVE');
    SET @OfferingId = SCOPE_IDENTITY();

    -- Check current enrollment
    SELECT @InitialEnrollment = current_enrollment FROM CourseOffering WHERE offering_id = @OfferingId;

    -- Register student
    INSERT INTO AppUser (username, password_hash, email, role, is_active)
    VALUES ('stud1', 'hash', 'stud1@test.com', 'STUDENT', 1);
    SET @StudUserId = SCOPE_IDENTITY();

    INSERT INTO Student (user_id, student_number, full_name, department_id, enrollment_date, status)
    VALUES (@StudUserId, 'STUD-001', 'Alice', @DeptId, '2026-01-01', 'ACTIVE');
    SET @StudentId = SCOPE_IDENTITY();

    -- Enroll student
    INSERT INTO Enrollment (student_id, offering_id, status)
    VALUES (@StudentId, @OfferingId, 'ENROLLED');

    -- Check updated enrollment
    SELECT @FinalEnrollment = current_enrollment FROM CourseOffering WHERE offering_id = @OfferingId;

    ROLLBACK TRANSACTION;

    IF @InitialEnrollment = 0 AND @FinalEnrollment = 1
    BEGIN
        INSERT INTO #TestResults_Triggers (TestName, Result, ErrorMessage)
        VALUES ('Test_Trigger_Capacity_SuccessPath', 'PASS', NULL);
    END
    ELSE
    BEGIN
        INSERT INTO #TestResults_Triggers (TestName, Result, ErrorMessage)
        VALUES ('Test_Trigger_Capacity_SuccessPath', 'FAIL', 'Initial enrollment was ' + CAST(@InitialEnrollment AS VARCHAR(5)) + ' and final enrollment was ' + CAST(@FinalEnrollment AS VARCHAR(5)) + '; expected 0 and 1.');
    END
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    SET @ErrorMsg = ERROR_MESSAGE();
    INSERT INTO #TestResults_Triggers (TestName, Result, ErrorMessage)
    VALUES ('Test_Trigger_Capacity_SuccessPath', 'FAIL', 'Failed to enroll student within capacity. Error: ' + @ErrorMsg);
END CATCH;

-- ---------------------------------------------------------------------
-- Test 2: trg_Enrollment_CheckCapacity - Over Capacity (Negative)
-- ---------------------------------------------------------------------
BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @OCDeptId INT, @OCCourseId INT, @OCSemesterId INT, @OCUserId INT, @OCInstId INT, @OCOfferingId INT;
    DECLARE @OCStudUserId1 INT, @OCStudentId1 INT;
    DECLARE @OCStudUserId2 INT, @OCStudentId2 INT;
    
    INSERT INTO Department (department_code, department_name) VALUES ('TDEPT', 'Testing Dept');
    SET @OCDeptId = SCOPE_IDENTITY();

    INSERT INTO Course (course_code, course_title, credit_hours, department_id, description)
    VALUES ('CS-101', 'Intro to CS', 3, @OCDeptId, 'CS 101');
    SET @OCCourseId = SCOPE_IDENTITY();

    INSERT INTO Semester (semester_name, start_date, end_date, is_current)
    VALUES ('Fall 2026', '2026-09-01', '2026-12-20', 0);
    SET @OCSemesterId = SCOPE_IDENTITY();

    INSERT INTO AppUser (username, password_hash, email, role, is_active)
    VALUES ('inst1', 'hash', 'inst1@test.com', 'INSTRUCTOR', 1);
    SET @OCUserId = SCOPE_IDENTITY();

    INSERT INTO Instructor (user_id, full_name, department_id, hire_date)
    VALUES (@OCUserId, 'Dr. Tester', @OCDeptId, '2026-01-01');
    SET @OCInstId = SCOPE_IDENTITY();

    -- Create offering with capacity = 1
    INSERT INTO CourseOffering (course_id, semester_id, instructor_id, max_capacity, current_enrollment, status)
    VALUES (@OCCourseId, @OCSemesterId, @OCInstId, 1, 0, 'ACTIVE');
    SET @OCOfferingId = SCOPE_IDENTITY();

    -- Student 1
    INSERT INTO AppUser (username, password_hash, email, role, is_active)
    VALUES ('stud1', 'hash', 'stud1@test.com', 'STUDENT', 1);
    SET @OCStudUserId1 = SCOPE_IDENTITY();

    INSERT INTO Student (user_id, student_number, full_name, department_id, enrollment_date, status)
    VALUES (@OCStudUserId1, 'STUD-001', 'Alice', @OCDeptId, '2026-01-01', 'ACTIVE');
    SET @OCStudentId1 = SCOPE_IDENTITY();

    -- Enroll Student 1 (Capacity now full)
    INSERT INTO Enrollment (student_id, offering_id, status)
    VALUES (@OCStudentId1, @OCOfferingId, 'ENROLLED');

    -- Student 2
    INSERT INTO AppUser (username, password_hash, email, role, is_active)
    VALUES ('stud2', 'hash', 'stud2@test.com', 'STUDENT', 1);
    SET @OCStudUserId2 = SCOPE_IDENTITY();

    INSERT INTO Student (user_id, student_number, full_name, department_id, enrollment_date, status)
    VALUES (@OCStudUserId2, 'STUD-002', 'Bob', @OCDeptId, '2026-01-01', 'ACTIVE');
    SET @OCStudentId2 = SCOPE_IDENTITY();

    -- Try to enroll Student 2 (Should trigger failure)
    INSERT INTO Enrollment (student_id, offering_id, status)
    VALUES (@OCStudentId2, @OCOfferingId, 'ENROLLED');

    -- If we get here, enrollment succeeded when it shouldn't!
    ROLLBACK TRANSACTION;
    INSERT INTO #TestResults_Triggers (TestName, Result, ErrorMessage)
    VALUES ('Test_Trigger_Capacity_OverLimit', 'FAIL', 'Successfully enrolled a student over capacity limits.');
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    SET @ErrorMsg = ERROR_MESSAGE();
    IF @ErrorMsg LIKE '%exceed maximum capacity%'
    BEGIN
        INSERT INTO #TestResults_Triggers (TestName, Result, ErrorMessage)
        VALUES ('Test_Trigger_Capacity_OverLimit', 'PASS', NULL);
    END
    ELSE
    BEGIN
        INSERT INTO #TestResults_Triggers (TestName, Result, ErrorMessage)
        VALUES ('Test_Trigger_Capacity_OverLimit', 'FAIL', 'Failed with wrong error message: ' + @ErrorMsg);
    END
END CATCH;

-- ---------------------------------------------------------------------
-- Test 3: trg_GradeEntry_CheckMaxPoints - Valid Marks (Positive)
-- ---------------------------------------------------------------------
BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @MDeptId INT, @MCourseId INT, @MSemesterId INT, @MUserId INT, @MInstId INT, @MOfferingId INT;
    DECLARE @MStudUserId INT, @MStudentId INT, @MEnrollId INT, @MCompId INT;
    
    INSERT INTO Department (department_code, department_name) VALUES ('TDEPT', 'Testing Dept');
    SET @MDeptId = SCOPE_IDENTITY();

    INSERT INTO Course (course_code, course_title, credit_hours, department_id, description)
    VALUES ('CS-101', 'Intro to CS', 3, @MDeptId, 'CS 101');
    SET @MCourseId = SCOPE_IDENTITY();

    INSERT INTO Semester (semester_name, start_date, end_date, is_current)
    VALUES ('Fall 2026', '2026-09-01', '2026-12-20', 0);
    SET @MSemesterId = SCOPE_IDENTITY();

    INSERT INTO AppUser (username, password_hash, email, role, is_active)
    VALUES ('inst1', 'hash', 'inst1@test.com', 'INSTRUCTOR', 1);
    SET @MUserId = SCOPE_IDENTITY();

    INSERT INTO Instructor (user_id, full_name, department_id, hire_date)
    VALUES (@MUserId, 'Dr. Tester', @MDeptId, '2026-01-01');
    SET @MInstId = SCOPE_IDENTITY();

    INSERT INTO CourseOffering (course_id, semester_id, instructor_id, max_capacity, current_enrollment, status)
    VALUES (@MCourseId, @MSemesterId, @MInstId, 30, 0, 'ACTIVE');
    SET @MOfferingId = SCOPE_IDENTITY();

    INSERT INTO AppUser (username, password_hash, email, role, is_active)
    VALUES ('stud1', 'hash', 'stud1@test.com', 'STUDENT', 1);
    SET @MStudUserId = SCOPE_IDENTITY();

    INSERT INTO Student (user_id, student_number, full_name, department_id, enrollment_date, status)
    VALUES (@MStudUserId, 'STUD-001', 'Alice', @MDeptId, '2026-01-01', 'ACTIVE');
    SET @MStudentId = SCOPE_IDENTITY();

    INSERT INTO Enrollment (student_id, offering_id, status)
    VALUES (@MStudentId, @MOfferingId, 'ENROLLED');
    SET @MEnrollId = SCOPE_IDENTITY();

    INSERT INTO GradeComponent (offering_id, component_name, max_points, sort_order)
    VALUES (@MOfferingId, 'Midterm', 30.00, 1);
    SET @MCompId = SCOPE_IDENTITY();

    -- Insert valid marks (25 out of 30)
    INSERT INTO GradeEntry (enrollment_id, component_id, obtained_marks, recorded_by)
    VALUES (@MEnrollId, @MCompId, 25.00, @MInstId);

    ROLLBACK TRANSACTION;
    INSERT INTO #TestResults_Triggers (TestName, Result, ErrorMessage)
    VALUES ('Test_Trigger_MaxPoints_SuccessPath', 'PASS', NULL);
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    SET @ErrorMsg = ERROR_MESSAGE();
    INSERT INTO #TestResults_Triggers (TestName, Result, ErrorMessage)
    VALUES ('Test_Trigger_MaxPoints_SuccessPath', 'FAIL', 'Valid marks insertion failed. Error: ' + @ErrorMsg);
END CATCH;

-- ---------------------------------------------------------------------
-- Test 4: trg_GradeEntry_CheckMaxPoints - Exceed Max Points (Negative)
-- ---------------------------------------------------------------------
BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @EDeptId INT, @ECourseId INT, @ESemesterId INT, @EUserId INT, @EInstId INT, @EOfferingId INT;
    DECLARE @EStudUserId INT, @EStudentId INT, @EEnrollId INT, @ECompId INT;
    
    INSERT INTO Department (department_code, department_name) VALUES ('TDEPT', 'Testing Dept');
    SET @EDeptId = SCOPE_IDENTITY();

    INSERT INTO Course (course_code, course_title, credit_hours, department_id, description)
    VALUES ('CS-101', 'Intro to CS', 3, @EDeptId, 'CS 101');
    SET @ECourseId = SCOPE_IDENTITY();

    INSERT INTO Semester (semester_name, start_date, end_date, is_current)
    VALUES ('Fall 2026', '2026-09-01', '2026-12-20', 0);
    SET @ESemesterId = SCOPE_IDENTITY();

    INSERT INTO AppUser (username, password_hash, email, role, is_active)
    VALUES ('inst1', 'hash', 'inst1@test.com', 'INSTRUCTOR', 1);
    SET @EUserId = SCOPE_IDENTITY();

    INSERT INTO Instructor (user_id, full_name, department_id, hire_date)
    VALUES (@EUserId, 'Dr. Tester', @EDeptId, '2026-01-01');
    SET @EInstId = SCOPE_IDENTITY();

    INSERT INTO CourseOffering (course_id, semester_id, instructor_id, max_capacity, current_enrollment, status)
    VALUES (@ECourseId, @ESemesterId, @EInstId, 30, 0, 'ACTIVE');
    SET @EOfferingId = SCOPE_IDENTITY();

    INSERT INTO AppUser (username, password_hash, email, role, is_active)
    VALUES ('stud1', 'hash', 'stud1@test.com', 'STUDENT', 1);
    SET @EStudUserId = SCOPE_IDENTITY();

    INSERT INTO Student (user_id, student_number, full_name, department_id, enrollment_date, status)
    VALUES (@EStudUserId, 'STUD-001', 'Alice', @EDeptId, '2026-01-01', 'ACTIVE');
    SET @EStudentId = SCOPE_IDENTITY();

    INSERT INTO Enrollment (student_id, offering_id, status)
    VALUES (@EStudentId, @EOfferingId, 'ENROLLED');
    SET @EEnrollId = SCOPE_IDENTITY();

    INSERT INTO GradeComponent (offering_id, component_name, max_points, sort_order)
    VALUES (@EOfferingId, 'Midterm', 30.00, 1);
    SET @ECompId = SCOPE_IDENTITY();

    -- Try to insert marks exceeding max points (35 out of 30)
    INSERT INTO GradeEntry (enrollment_id, component_id, obtained_marks, recorded_by)
    VALUES (@EEnrollId, @ECompId, 35.00, @EInstId);

    -- If we get here, trigger did not work!
    ROLLBACK TRANSACTION;
    INSERT INTO #TestResults_Triggers (TestName, Result, ErrorMessage)
    VALUES ('Test_Trigger_MaxPoints_Exceeded', 'FAIL', 'Successfully entered marks that exceed the component''s maximum points.');
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    SET @ErrorMsg = ERROR_MESSAGE();
    IF @ErrorMsg LIKE '%cannot exceed the component%'
    BEGIN
        INSERT INTO #TestResults_Triggers (TestName, Result, ErrorMessage)
        VALUES ('Test_Trigger_MaxPoints_Exceeded', 'PASS', NULL);
    END
    ELSE
    BEGIN
        INSERT INTO #TestResults_Triggers (TestName, Result, ErrorMessage)
        VALUES ('Test_Trigger_MaxPoints_Exceeded', 'FAIL', 'Failed with wrong error message: ' + @ErrorMsg);
    END
END CATCH;

-- ---------------------------------------------------------------------
-- Test 5: trg_PreventGradeChangeAfterFinalization - Under Active Status (Positive)
-- ---------------------------------------------------------------------
BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @LDeptId INT, @LCourseId INT, @LSemesterId INT, @LUserId INT, @LInstId INT, @LOfferingId INT;
    DECLARE @LStudUserId INT, @LStudentId INT, @LEnrollId INT, @LCompId INT, @LEntryId INT;
    
    INSERT INTO Department (department_code, department_name) VALUES ('TDEPT', 'Testing Dept');
    SET @LDeptId = SCOPE_IDENTITY();

    INSERT INTO Course (course_code, course_title, credit_hours, department_id, description)
    VALUES ('CS-101', 'Intro to CS', 3, @LDeptId, 'CS 101');
    SET @LCourseId = SCOPE_IDENTITY();

    INSERT INTO Semester (semester_name, start_date, end_date, is_current)
    VALUES ('Fall 2026', '2026-09-01', '2026-12-20', 0);
    SET @LSemesterId = SCOPE_IDENTITY();

    INSERT INTO AppUser (username, password_hash, email, role, is_active)
    VALUES ('inst1', 'hash', 'inst1@test.com', 'INSTRUCTOR', 1);
    SET @LUserId = SCOPE_IDENTITY();

    INSERT INTO Instructor (user_id, full_name, department_id, hire_date)
    VALUES (@LUserId, 'Dr. Tester', @LDeptId, '2026-01-01');
    SET @LInstId = SCOPE_IDENTITY();

    -- Create offering with is_grade_finalized = 0 (NOT finalized)
    INSERT INTO CourseOffering (course_id, semester_id, instructor_id, max_capacity, current_enrollment, is_grade_finalized, status)
    VALUES (@LCourseId, @LSemesterId, @LInstId, 30, 0, 0, 'ACTIVE');
    SET @LOfferingId = SCOPE_IDENTITY();

    INSERT INTO AppUser (username, password_hash, email, role, is_active)
    VALUES ('stud1', 'hash', 'stud1@test.com', 'STUDENT', 1);
    SET @LStudUserId = SCOPE_IDENTITY();

    INSERT INTO Student (user_id, student_number, full_name, department_id, enrollment_date, status)
    VALUES (@LStudUserId, 'STUD-001', 'Alice', @LDeptId, '2026-01-01', 'ACTIVE');
    SET @LStudentId = SCOPE_IDENTITY();

    INSERT INTO Enrollment (student_id, offering_id, status)
    VALUES (@LStudentId, @LOfferingId, 'ENROLLED');
    SET @LEnrollId = SCOPE_IDENTITY();

    INSERT INTO GradeComponent (offering_id, component_name, max_points, sort_order)
    VALUES (@LOfferingId, 'Midterm', 30.00, 1);
    SET @LCompId = SCOPE_IDENTITY();

    INSERT INTO GradeEntry (enrollment_id, component_id, obtained_marks, recorded_by)
    VALUES (@LEnrollId, @LCompId, 25.00, @LInstId);
    SET @LEntryId = SCOPE_IDENTITY();

    -- Update marks (25 -> 28) - should succeed
    UPDATE GradeEntry
    SET obtained_marks = 28.00
    WHERE grade_entry_id = @LEntryId;

    ROLLBACK TRANSACTION;
    INSERT INTO #TestResults_Triggers (TestName, Result, ErrorMessage)
    VALUES ('Test_Trigger_Lockout_SuccessPath', 'PASS', NULL);
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    SET @ErrorMsg = ERROR_MESSAGE();
    INSERT INTO #TestResults_Triggers (TestName, Result, ErrorMessage)
    VALUES ('Test_Trigger_Lockout_SuccessPath', 'FAIL', 'Failed to update grades for non-finalized course. Error: ' + @ErrorMsg);
END CATCH;

-- ---------------------------------------------------------------------
-- Test 6: trg_PreventGradeChangeAfterFinalization - Under Locked Status (Negative)
-- ---------------------------------------------------------------------
BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @FDeptId INT, @FCourseId INT, @FSemesterId INT, @FUserId INT, @FInstId INT, @FOfferingId INT;
    DECLARE @FStudUserId INT, @FStudentId INT, @FEnrollId INT, @FCompId INT, @FEntryId INT;
    
    INSERT INTO Department (department_code, department_name) VALUES ('TDEPT', 'Testing Dept');
    SET @FDeptId = SCOPE_IDENTITY();

    INSERT INTO Course (course_code, course_title, credit_hours, department_id, description)
    VALUES ('CS-101', 'Intro to CS', 3, @FDeptId, 'CS 101');
    SET @FCourseId = SCOPE_IDENTITY();

    INSERT INTO Semester (semester_name, start_date, end_date, is_current)
    VALUES ('Fall 2026', '2026-09-01', '2026-12-20', 0);
    SET @FSemesterId = SCOPE_IDENTITY();

    INSERT INTO AppUser (username, password_hash, email, role, is_active)
    VALUES ('inst1', 'hash', 'inst1@test.com', 'INSTRUCTOR', 1);
    SET @FUserId = SCOPE_IDENTITY();

    INSERT INTO Instructor (user_id, full_name, department_id, hire_date)
    VALUES (@FUserId, 'Dr. Tester', @FDeptId, '2026-01-01');
    SET @FInstId = SCOPE_IDENTITY();

    -- Create offering with is_grade_finalized = 0 initially so we can insert marks
    INSERT INTO CourseOffering (course_id, semester_id, instructor_id, max_capacity, current_enrollment, is_grade_finalized, status)
    VALUES (@FCourseId, @FSemesterId, @FInstId, 30, 0, 0, 'ACTIVE');
    SET @FOfferingId = SCOPE_IDENTITY();

    INSERT INTO AppUser (username, password_hash, email, role, is_active)
    VALUES ('stud1', 'hash', 'stud1@test.com', 'STUDENT', 1);
    SET @FStudUserId = SCOPE_IDENTITY();

    INSERT INTO Student (user_id, student_number, full_name, department_id, enrollment_date, status)
    VALUES (@FStudUserId, 'STUD-001', 'Alice', @FDeptId, '2026-01-01', 'ACTIVE');
    SET @FStudentId = SCOPE_IDENTITY();

    INSERT INTO Enrollment (student_id, offering_id, status)
    VALUES (@FStudentId, @FOfferingId, 'ENROLLED');
    SET @FEnrollId = SCOPE_IDENTITY();

    INSERT INTO GradeComponent (offering_id, component_name, max_points, sort_order)
    VALUES (@FOfferingId, 'Midterm', 30.00, 1);
    SET @FCompId = SCOPE_IDENTITY();

    INSERT INTO GradeEntry (enrollment_id, component_id, obtained_marks, recorded_by)
    VALUES (@FEnrollId, @FCompId, 25.00, @FInstId);
    SET @FEntryId = SCOPE_IDENTITY();

    -- Now finalise the gradebook at offering level
    UPDATE CourseOffering
    SET is_grade_finalized = 1
    WHERE offering_id = @FOfferingId;

    -- Try to update marks (25 -> 28) - should be blocked by trigger
    UPDATE GradeEntry
    SET obtained_marks = 28.00
    WHERE grade_entry_id = @FEntryId;

    -- If we get here, trigger failed to block the change!
    ROLLBACK TRANSACTION;
    INSERT INTO #TestResults_Triggers (TestName, Result, ErrorMessage)
    VALUES ('Test_Trigger_Lockout_Enforced', 'FAIL', 'Successfully modified grades after offering finalization.');
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    SET @ErrorMsg = ERROR_MESSAGE();
    IF @ErrorMsg LIKE '%locked%' OR @ErrorMsg LIKE '%finalized%'
    BEGIN
        INSERT INTO #TestResults_Triggers (TestName, Result, ErrorMessage)
        VALUES ('Test_Trigger_Lockout_Enforced', 'PASS', NULL);
    END
    ELSE
    BEGIN
        INSERT INTO #TestResults_Triggers (TestName, Result, ErrorMessage)
        VALUES ('Test_Trigger_Lockout_Enforced', 'FAIL', 'Failed with wrong error message: ' + @ErrorMsg);
    END
END CATCH;

-- ---------------------------------------------------------------------
-- Report Module 02 results
-- ---------------------------------------------------------------------
DECLARE @TestName NVARCHAR(100), @Result NVARCHAR(10), @ErrMsg NVARCHAR(MAX);
DECLARE test_cursor CURSOR FOR SELECT TestName, Result, ErrorMessage FROM #TestResults_Triggers;
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
PRINT 'Module 02 Summary: ' + CAST(@PassCount AS VARCHAR(5)) + ' passed, ' + CAST(@FailCount AS VARCHAR(5)) + ' failed.';
PRINT '==================================================';
-- End of single batch
GO
