using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GpaSystem.API.Data;
using GpaSystem.API.DTOs;
using GpaSystem.API.Exceptions;
using GpaSystem.API.Models;
using GpaSystem.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GpaSystem.API.Services;

public class GpaCalculatorService : IGpaCalculatorService
{
    private readonly GpaSystemDbContext _db;
    private readonly ICourseGradeRepository _courseGrades;
    private readonly IAcademicRecordRepository _academicRecords;
    private readonly IStudentRepository _students;
    private readonly ISemesterRepository _semesters;
    private readonly IReportService _reports;

    public GpaCalculatorService(
        GpaSystemDbContext db,
        ICourseGradeRepository courseGrades,
        IAcademicRecordRepository academicRecords,
        IStudentRepository students,
        ISemesterRepository semesters,
        IReportService reports)
    {
        _db = db;
        _courseGrades = courseGrades;
        _academicRecords = academicRecords;
        _students = students;
        _semesters = semesters;
        _reports = reports;
    }

    public async Task RecalculateStudentGpaAndCgpaAsync(int studentId)
    {
        // 1. Fetch student
        var student = await _students.GetByIdAsync(studentId)
            ?? throw ApiException.NotFound($"Student with ID {studentId} was not found.");

        // 2. Fetch all completed/finalized course grades of the student across all semesters
        var allGrades = await _db.CourseGrades
            .Where(cg => cg.Enrollment.StudentId == studentId && cg.Enrollment.Status == "COMPLETED")
            .Include(cg => cg.Enrollment)
                .ThenInclude(e => e.CourseOffering)
                    .ThenInclude(co => co.Course)
            .Include(cg => cg.Enrollment)
                .ThenInclude(e => e.CourseOffering)
                    .ThenInclude(co => co.Semester)
            .ToListAsync();

        // 3. Resolve Repeated Attempts sequentially
        // Group grades by course code/ID
        var groupedGrades = allGrades.GroupBy(g => g.Enrollment.CourseOffering.CourseId);

        foreach (var group in groupedGrades)
        {
            var attempts = group.ToList();
            if (attempts.Count > 1)
            {
                // Sort attempts:
                // - Highest GradePoint descending
                // - Highest Percentage descending
                // - Most recent Semester Start Date descending
                var sortedAttempts = attempts
                    .OrderByDescending(a => a.GradePoints)
                    .ThenByDescending(a => a.Percentage)
                    .ThenByDescending(a => a.Enrollment.CourseOffering.Semester.StartDate)
                    .ToList();

                // The first attempt is the active one
                sortedAttempts[0].IsRepeatedAttempt = false;

                // All other attempts are repeated (excluded)
                for (int i = 1; i < sortedAttempts.Count; i++)
                {
                    sortedAttempts[i].IsRepeatedAttempt = true;
                }

                // Update attempts in DB state
                foreach (var att in attempts)
                {
                    _db.Entry(att).State = EntityState.Modified;
                }
            }
            else if (attempts.Count == 1)
            {
                // Single attempt is always active
                attempts[0].IsRepeatedAttempt = false;
                _db.Entry(attempts[0]).State = EntityState.Modified;
            }
        }

        await _db.SaveChangesAsync();

        // 4. Recalculate each semester's GPA and CGPA chronologically
        var semesters = await _db.Semesters.OrderBy(s => s.StartDate).ToListAsync();
        var historicalActiveGrades = new List<CourseGrade>();

        foreach (var semester in semesters)
        {
            var semesterGrades = allGrades
                .Where(g => g.Enrollment.CourseOffering.SemesterId == semester.SemesterId)
                .ToList();

            // If there are no grades in this semester, we don't recalculate/create an academic record unless one already exists
            if (semesterGrades.Count == 0)
            {
                var existingRecord = await _academicRecords.GetByStudentAndSemesterAsync(studentId, semester.SemesterId);
                if (existingRecord != null)
                {
                    // If they have no grades but an academic record exists, let's keep it up to date or remove it.
                    // Let's just update it to 0.00 since there are no active courses in this term.
                    existingRecord.SemesterGpa = 0.00m;
                    existingRecord.CumulativeGpa = historicalActiveGrades.Count > 0 ? CalculateCgpa(historicalActiveGrades) : 0.00m;
                    existingRecord.TotalCreditsAttempted = 0;
                    existingRecord.TotalGradePoints = 0.00m;
                    _db.Entry(existingRecord).State = EntityState.Modified;
                }
                continue;
            }

            // Semester calculations (only count non-repeated course grades)
            var activeSemesterGrades = semesterGrades.Where(g => !g.IsRepeatedAttempt).ToList();
            var semesterCredits = activeSemesterGrades.Sum(g => (int)g.Enrollment.CourseOffering.Course.CreditHours);
            var semesterQualityPoints = activeSemesterGrades.Sum(g => g.GradePoints * g.Enrollment.CourseOffering.Course.CreditHours);
            var semesterGpa = semesterCredits > 0
                ? Math.Round(semesterQualityPoints / semesterCredits, 2, MidpointRounding.AwayFromZero)
                : 0.00m;

            // Add all non-repeated grades of this semester to the historical list for CGPA
            // Note: we must clean historicalActiveGrades of any grades that are now marked as repeated
            historicalActiveGrades.AddRange(activeSemesterGrades);
            historicalActiveGrades = historicalActiveGrades.Where(g => !g.IsRepeatedAttempt).ToList();

            // Cumulative CGPA up to this semester
            var cumulativeCgpa = CalculateCgpa(historicalActiveGrades);

            // Update or Create AcademicRecord
            var record = await _academicRecords.GetByStudentAndSemesterAsync(studentId, semester.SemesterId);
            if (record == null)
            {
                record = new AcademicRecord
                {
                    StudentId = studentId,
                    SemesterId = semester.SemesterId,
                    SemesterGpa = semesterGpa,
                    CumulativeGpa = cumulativeCgpa,
                    TotalCreditsAttempted = semesterCredits,
                    TotalGradePoints = semesterQualityPoints,
                    CalculationDate = DateTime.UtcNow
                };
                await _academicRecords.AddAsync(record);
            }
            else
            {
                record.SemesterGpa = semesterGpa;
                record.CumulativeGpa = cumulativeCgpa;
                record.TotalCreditsAttempted = semesterCredits;
                record.TotalGradePoints = semesterQualityPoints;
                record.CalculationDate = DateTime.UtcNow;
                _db.Entry(record).State = EntityState.Modified;
            }
        }

        await _db.SaveChangesAsync();
    }

    public Task<StudentDashboardResponse> GetStudentDashboardAsync(int studentId) =>
        _reports.GetStudentDashboardAsync(studentId);

    private static decimal CalculateCgpa(List<CourseGrade> activeGrades)
    {
        var cumulativeCredits = activeGrades.Sum(g => (int)g.Enrollment.CourseOffering.Course.CreditHours);
        var cumulativeQualityPoints = activeGrades.Sum(g => g.GradePoints * g.Enrollment.CourseOffering.Course.CreditHours);

        return cumulativeCredits > 0
            ? Math.Round(cumulativeQualityPoints / cumulativeCredits, 2, MidpointRounding.AwayFromZero)
            : 0.00m;
    }
}
