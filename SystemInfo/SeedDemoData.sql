-- Demo seed for GPA System (SQL Server)
-- Run after core Phase 1 records exist (department, student, instructor, course).
-- Prefer POST /api/admin/seed-demo in Development for idempotent seeding via application services.

USE GPASystem;
GO

IF NOT EXISTS (SELECT 1 FROM [Configuration] WHERE config_key = 'pass_fail_cutoff')
BEGIN
    INSERT INTO [Configuration] (config_key, config_value, description, updated_at)
    VALUES ('pass_fail_cutoff', '50', 'Minimum percentage to pass a course', SYSUTCDATETIME());
END
GO

-- Grading policies are managed through the API; use POST /api/admin/seed-demo to populate them safely.
