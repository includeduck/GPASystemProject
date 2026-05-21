using GpaSystem.API.DTOs;
using GpaSystem.API.Models;
using GpaSystem.API.Services;
using Microsoft.EntityFrameworkCore;

namespace GpaSystem.API.Tests;

public class GradeServiceFinalizeSuccessTests
{
    [Fact]
    public async Task FinalizeGrades_WithCompleteMarks_AssignsPassingGradeWithoutForce()
    {
        await using var database = await TestDatabase.CreateAsync();
        var db = database.Context;
        var catalog = await TestData.SeedCatalogAsync(db);

        var midterm = new GradeComponent
        {
            OfferingId = catalog.Offering.OfferingId,
            ComponentName = "Midterm",
            MaxPoints = 40,
            SortOrder = 1
        };
        var finalExam = new GradeComponent
        {
            OfferingId = catalog.Offering.OfferingId,
            ComponentName = "Final",
            MaxPoints = 60,
            SortOrder = 2
        };
        db.GradeComponents.AddRange(midterm, finalExam);

        var enrollment = new Enrollment
        {
            StudentId = catalog.Student.StudentId,
            OfferingId = catalog.Offering.OfferingId,
            Status = "ENROLLED"
        };
        db.Enrollments.Add(enrollment);

        db.Configurations.Add(new Configuration
        {
            ConfigKey = "pass_fail_cutoff",
            ConfigValue = "50"
        });
        db.GradingPolicies.AddRange(
            new GradingPolicy { LetterGrade = "A", MinPercentage = 90, MaxPercentage = 100, GradePoint = 4.00m, IsActive = true, EffectiveFrom = DateOnly.FromDateTime(DateTime.Today) },
            new GradingPolicy { LetterGrade = "B", MinPercentage = 80, MaxPercentage = 90, GradePoint = 3.00m, IsActive = true, EffectiveFrom = DateOnly.FromDateTime(DateTime.Today) },
            new GradingPolicy { LetterGrade = "F", MinPercentage = 0, MaxPercentage = 50, GradePoint = 0m, IsActive = true, EffectiveFrom = DateOnly.FromDateTime(DateTime.Today) }
        );
        await db.SaveChangesAsync();

        var service = ServiceFactory.CreateGradeService(db);

        await service.RecordMarksAsync(
            catalog.Offering.OfferingId,
            new List<RecordGradeEntryRequest>
            {
                new() { EnrollmentId = enrollment.EnrollmentId, ComponentId = midterm.ComponentId, ObtainedMarks = 36 },
                new() { EnrollmentId = enrollment.EnrollmentId, ComponentId = finalExam.ComponentId, ObtainedMarks = 54 }
            },
            catalog.Instructor.InstructorId);

        await service.FinalizeGradesAsync(catalog.Offering.OfferingId, force: false, catalog.Instructor.InstructorId);

        var courseGrade = await db.CourseGrades.FirstAsync(cg => cg.EnrollmentId == enrollment.EnrollmentId);
        Assert.Equal(90m, courseGrade.Percentage);
        Assert.Equal("A", courseGrade.LetterGrade);
        Assert.Equal(4.00m, courseGrade.GradePoints);

        var record = await db.AcademicRecords.FirstAsync(r =>
            r.StudentId == catalog.Student.StudentId && r.SemesterId == catalog.Semester.SemesterId);
        Assert.Equal(4.00m, record.SemesterGpa);
    }
}
