using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GpaSystem.API.DTOs;
using GpaSystem.API.Exceptions;
using GpaSystem.API.Models;
using GpaSystem.API.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GpaSystem.API.Tests;

public class GradeServiceTests
{
    [Fact]
    public async Task CreateComponent_FailsIfOfferingFinalized()
    {
        await using var database = await TestDatabase.CreateAsync();
        var db = database.Context;
        var catalog = await TestData.SeedCatalogAsync(db);

        // Finalize offering
        catalog.Offering.IsGradeFinalized = true;
        await db.SaveChangesAsync();

        var service = ServiceFactory.CreateGradeService(db);

        var ex = await Assert.ThrowsAsync<ApiException>(() => service.CreateComponentAsync(catalog.Offering.OfferingId, new CreateGradeComponentRequest
        {
            ComponentName = "Midterm",
            MaxPoints = 30,
            SortOrder = 1
        }));

        Assert.Equal(400, ex.StatusCode);
        Assert.Contains("finalized", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RecordMarks_ThrowsException_IfMarksOutOfBounds()
    {
        await using var database = await TestDatabase.CreateAsync();
        var db = database.Context;
        var catalog = await TestData.SeedCatalogAsync(db);

        // Add a component
        var component = new GradeComponent
        {
            OfferingId = catalog.Offering.OfferingId,
            ComponentName = "Quizzes",
            MaxPoints = 20,
            SortOrder = 1
        };
        db.GradeComponents.Add(component);

        // Add enrollment
        var enrollment = new Enrollment
        {
            StudentId = catalog.Student.StudentId,
            OfferingId = catalog.Offering.OfferingId,
            Status = "ENROLLED"
        };
        db.Enrollments.Add(enrollment);
        await db.SaveChangesAsync();

        var service = ServiceFactory.CreateGradeService(db);

        // Record negative marks - should throw
        var exNegative = await Assert.ThrowsAsync<ApiException>(() => service.RecordMarksAsync(
            catalog.Offering.OfferingId,
            new List<RecordGradeEntryRequest>
            {
                new() { EnrollmentId = enrollment.EnrollmentId, ComponentId = component.ComponentId, ObtainedMarks = -5 }
            },
            catalog.Instructor.InstructorId
        ));

        // Record marks greater than MaxPoints - should throw
        var exExceeding = await Assert.ThrowsAsync<ApiException>(() => service.RecordMarksAsync(
            catalog.Offering.OfferingId,
            new List<RecordGradeEntryRequest>
            {
                new() { EnrollmentId = enrollment.EnrollmentId, ComponentId = component.ComponentId, ObtainedMarks = 25 }
            },
            catalog.Instructor.InstructorId
        ));

        Assert.Equal(400, exNegative.StatusCode);
        Assert.Equal(400, exExceeding.StatusCode);
    }

    [Fact]
    public async Task FinalizeGrades_GeneratesCourseGrades_LocksOffering_FillsMissingWithZero()
    {
        await using var database = await TestDatabase.CreateAsync();
        var db = database.Context;
        var catalog = await TestData.SeedCatalogAsync(db);

        // Add two components: Midterm (40), Final (60) => Total 100
        var comp1 = new GradeComponent
        {
            OfferingId = catalog.Offering.OfferingId,
            ComponentName = "Midterm",
            MaxPoints = 40,
            SortOrder = 1
        };
        var comp2 = new GradeComponent
        {
            OfferingId = catalog.Offering.OfferingId,
            ComponentName = "Final",
            MaxPoints = 60,
            SortOrder = 2
        };
        db.GradeComponents.AddRange(comp1, comp2);

        // Add enrollment
        var enrollment = new Enrollment
        {
            StudentId = catalog.Student.StudentId,
            OfferingId = catalog.Offering.OfferingId,
            Status = "ENROLLED"
        };
        db.Enrollments.Add(enrollment);
        
        // Add a base configuration for passing cutoff
        var config = new Configuration
        {
            ConfigKey = "pass_fail_cutoff",
            ConfigValue = "50"
        };
        db.Configurations.Add(config);

        // Add standard grading policy
        var policyA = new GradingPolicy
        {
            LetterGrade = "A",
            MinPercentage = 90,
            MaxPercentage = 100,
            GradePoint = 4.00m,
            IsActive = true,
            EffectiveFrom = DateOnly.FromDateTime(DateTime.Today)
        };
        var policyB = new GradingPolicy
        {
            LetterGrade = "B",
            MinPercentage = 80,
            MaxPercentage = 90,
            GradePoint = 3.00m,
            IsActive = true,
            EffectiveFrom = DateOnly.FromDateTime(DateTime.Today)
        };
        db.GradingPolicies.AddRange(policyA, policyB);
        await db.SaveChangesAsync();

        var service = ServiceFactory.CreateGradeService(db);

        // Only record marks for Midterm (36/40). Final is left blank.
        await service.RecordMarksAsync(
            catalog.Offering.OfferingId,
            new List<RecordGradeEntryRequest>
            {
                new() { EnrollmentId = enrollment.EnrollmentId, ComponentId = comp1.ComponentId, ObtainedMarks = 36 }
            },
            catalog.Instructor.InstructorId
        );

        // Attempt finalization without forcing - should fail because Final is missing
        var exNoForce = await Assert.ThrowsAsync<ApiException>(() => service.FinalizeGradesAsync(
            catalog.Offering.OfferingId,
            force: false,
            catalog.Instructor.InstructorId
        ));
        Assert.Equal(400, exNoForce.StatusCode);

        // Finalize by forcing (missing final exam should become 0)
        // Total obtained: 36 (Midterm) + 0 (Final) = 36% => Letter F, GP 0
        await service.FinalizeGradesAsync(
            catalog.Offering.OfferingId,
            force: true,
            catalog.Instructor.InstructorId
        );

        // Verify that offering is now locked (IsGradeFinalized = true, Status = COMPLETED)
        var updatedOffering = await db.CourseOfferings.FindAsync(catalog.Offering.OfferingId);
        Assert.NotNull(updatedOffering);
        Assert.True(updatedOffering.IsGradeFinalized);
        Assert.Equal("COMPLETED", updatedOffering.Status);

        // Verify that the enrollment is now marked COMPLETED
        var updatedEnrollment = await db.Enrollments.FindAsync(enrollment.EnrollmentId);
        Assert.NotNull(updatedEnrollment);
        Assert.Equal("COMPLETED", updatedEnrollment.Status);

        // Verify that the course grade was correctly generated
        var courseGrade = await db.CourseGrades.FirstOrDefaultAsync(cg => cg.EnrollmentId == enrollment.EnrollmentId);
        Assert.NotNull(courseGrade);
        Assert.Equal(36.00m, courseGrade.TotalObtained);
        Assert.Equal(100.00m, courseGrade.MaxPossible);
        Assert.Equal(36.00m, courseGrade.Percentage);
        Assert.Equal("F", courseGrade.LetterGrade);
        Assert.Equal(0.00m, courseGrade.GradePoints);

        // Verify that the GPA calculation was triggered and academic record created
        var academicRecord = await db.AcademicRecords.FirstOrDefaultAsync(r => r.StudentId == catalog.Student.StudentId && r.SemesterId == catalog.Semester.SemesterId);
        Assert.NotNull(academicRecord);
        Assert.Equal(0.00m, academicRecord.SemesterGpa); // failed course GP = 0
        Assert.Equal(3, academicRecord.TotalCreditsAttempted);

        // Verify notification was sent
        var notification = await db.Notification.FirstOrDefaultAsync(n => n.UserId == catalog.Student.UserId);
        Assert.NotNull(notification);
        Assert.Contains("results", notification.Subject, StringComparison.OrdinalIgnoreCase);
    }
}
