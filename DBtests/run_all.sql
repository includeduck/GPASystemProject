-- =====================================================================
-- GPA System Database Unit Test Runner (SQLCMD Mode)
-- To execute this master script:
-- 1. Using sqlcmd utility from command prompt:
--    sqlcmd -S "PHOENIX\SQLEXPRESS" -d GPASystem -i run_all.sql
-- 2. Inside SQL Server Management Studio (SSMS):
--    Go to Menu -> Query -> SQLCMD Mode, then click Execute.
-- =====================================================================

:On Error Exit

USE [GPASystem];
GO

PRINT '================================================================';
PRINT '         GPA SYSTEM DATABASE COMPLETE UNIT TEST SUITE           ';
PRINT '================================================================';
PRINT 'Start Time: ' + CAST(GETUTCDATE() AS VARCHAR(30)) + ' UTC';
PRINT '';

-- 1. Run CHECK and Integrity Constraints tests
:r 01_test_constraints.sql

-- 2. Run Database Triggers tests
:r 02_test_triggers.sql

-- 3. Run Foreign Key Cascading Deletes tests
:r 03_test_cascades.sql

PRINT '';
PRINT '================================================================';
PRINT '   GPA SYSTEM DATABASE UNIT TEST SUITE RUN COMPLETED SUCCESSFULLY   ';
PRINT '================================================================';
PRINT 'End Time: ' + CAST(GETUTCDATE() AS VARCHAR(30)) + ' UTC';
GO
