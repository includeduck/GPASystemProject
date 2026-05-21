using GpaSystem.API.Models;
using GpaSystem.API.Services;
using Microsoft.EntityFrameworkCore;

namespace GpaSystem.API.Tests;

public class ReportServiceTests
{
    [Fact]
    public async Task GetTranscriptAsync_IncludesFailedCoursesSeparately()
    {
        await using var database = await TestDatabase.CreateAsync();
        var db = database.Context;
        var catalog = await TestData.SeedCatalogAsync(db);

        var enrollment = new Enrollment
        {
            StudentId = catalog.Student.StudentId,
            OfferingId = catalog.Offering.OfferingId,
            Status = "COMPLETED"
        };
        db.Enrollments.Add(enrollment);
        await db.SaveChangesAsync();

        db.CourseGrades.Add(new CourseGrade
        {
            EnrollmentId = enrollment.EnrollmentId,
            TotalObtained = 40,
            MaxPossible = 100,
            Percentage = 40,
            LetterGrade = "F",
            GradePoints = 0m,
            IsRepeatedAttempt = false,
            CalculatedAt = DateTime.UtcNow
        });
        db.AcademicRecords.Add(new AcademicRecord
        {
            StudentId = catalog.Student.StudentId,
            SemesterId = catalog.Semester.SemesterId,
            SemesterGpa = 0m,
            CumulativeGpa = 0m,
            TotalCreditsAttempted = 3,
            TotalGradePoints = 0m,
            CalculationDate = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = ServiceFactory.CreateReportService(db);
        var transcript = await service.GetTranscriptAsync(catalog.Student.StudentId);

        Assert.Single(transcript.FailedCourses);
        Assert.Equal("F", transcript.FailedCourses[0].LetterGrade);
        Assert.Equal(catalog.Department.DepartmentCode, transcript.DepartmentCode);
    }

    [Fact]
    public async Task GetWarningListAsync_ReturnsStudentsBelowThreshold()
    {
        await using var database = await TestDatabase.CreateAsync();
        var db = database.Context;
        var catalog = await TestData.SeedCatalogAsync(db);
        var other = await TestData.AddStudentAsync(db, catalog.Department);

        db.AcademicRecords.AddRange(
            new AcademicRecord
            {
                StudentId = catalog.Student.StudentId,
                SemesterId = catalog.Semester.SemesterId,
                SemesterGpa = 1.80m,
                CumulativeGpa = 1.80m,
                TotalCreditsAttempted = 3,
                TotalGradePoints = 5.4m,
                CalculationDate = DateTime.UtcNow
            },
            new AcademicRecord
            {
                StudentId = other.StudentId,
                SemesterId = catalog.Semester.SemesterId,
                SemesterGpa = 3.50m,
                CumulativeGpa = 3.50m,
                TotalCreditsAttempted = 3,
                TotalGradePoints = 10.5m,
                CalculationDate = DateTime.UtcNow
            });
        await db.SaveChangesAsync();

        var service = ServiceFactory.CreateReportService(db);
        var report = await service.GetWarningListAsync(catalog.Semester.SemesterId, 2.0m);

        Assert.Single(report.Students);
        Assert.Equal(catalog.Student.StudentId, report.Students[0].StudentId);
    }

    [Fact]
    public async Task GetClassRankingsAsync_OrdersByCgpaWithTieBreak()
    {
        await using var database = await TestDatabase.CreateAsync();
        var db = database.Context;
        var catalog = await TestData.SeedCatalogAsync(db);
        var studentB = await TestData.AddStudentAsync(db, catalog.Department);

        db.AcademicRecords.AddRange(
            new AcademicRecord
            {
                StudentId = catalog.Student.StudentId,
                SemesterId = catalog.Semester.SemesterId,
                SemesterGpa = 3.50m,
                CumulativeGpa = 3.50m,
                TotalCreditsAttempted = 3,
                TotalGradePoints = 10.5m,
                CalculationDate = DateTime.UtcNow
            },
            new AcademicRecord
            {
                StudentId = studentB.StudentId,
                SemesterId = catalog.Semester.SemesterId,
                SemesterGpa = 3.50m,
                CumulativeGpa = 3.50m,
                TotalCreditsAttempted = 3,
                TotalGradePoints = 10.5m,
                CalculationDate = DateTime.UtcNow
            });
        await db.SaveChangesAsync();

        var service = ServiceFactory.CreateReportService(db);
        var report = await service.GetClassRankingsAsync(catalog.Department.DepartmentId);

        Assert.Equal(2, report.Rankings.Count);
        Assert.Equal(1, report.Rankings[0].Rank);
        Assert.True(report.Rankings[0].StudentId < report.Rankings[1].StudentId);
    }

    [Fact]
    public async Task GetSemesterResultsAsync_ReturnsStudentsWithRecords()
    {
        await using var database = await TestDatabase.CreateAsync();
        var db = database.Context;
        var catalog = await TestData.SeedCatalogAsync(db);

        db.AcademicRecords.Add(new AcademicRecord
        {
            StudentId = catalog.Student.StudentId,
            SemesterId = catalog.Semester.SemesterId,
            SemesterGpa = 3.00m,
            CumulativeGpa = 3.00m,
            TotalCreditsAttempted = 3,
            TotalGradePoints = 9m,
            CalculationDate = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = ServiceFactory.CreateReportService(db);
        var report = await service.GetSemesterResultsAsync(catalog.Semester.SemesterId);

        Assert.Equal(catalog.Semester.SemesterName, report.SemesterName);
        Assert.Single(report.Students);
    }
}
