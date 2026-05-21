using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GpaSystem.API.Models;
using GpaSystem.API.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GpaSystem.API.Tests;

public class GpaCalculatorServiceTests
{
    [Fact]
    public async Task CalculateGpa_SimpleGrades_CalculatesCorrectly()
    {
        await using var database = await TestDatabase.CreateAsync();
        var db = database.Context;
        var catalog = await TestData.SeedCatalogAsync(db);

        // Add another course in the same semester
        var otherCourse = await TestData.AddCourseAsync(db, catalog.Department, "Database Systems");
        var otherOffering = await TestData.AddOfferingAsync(db, otherCourse, catalog.Semester, catalog.Instructor);

        // Enroll student in both
        var enrollment1 = new Enrollment
        {
            StudentId = catalog.Student.StudentId,
            OfferingId = catalog.Offering.OfferingId,
            Status = "COMPLETED"
        };
        var enrollment2 = new Enrollment
        {
            StudentId = catalog.Student.StudentId,
            OfferingId = otherOffering.OfferingId,
            Status = "COMPLETED"
        };
        db.Enrollments.AddRange(enrollment1, enrollment2);
        await db.SaveChangesAsync();

        // Add finalized course grades: Course 1: A (4.00 GP, 3 credits), Course 2: B (3.00 GP, 3 credits)
        var cg1 = new CourseGrade
        {
            EnrollmentId = enrollment1.EnrollmentId,
            TotalObtained = 95,
            MaxPossible = 100,
            Percentage = 95,
            LetterGrade = "A",
            GradePoints = 4.00m,
            IsRepeatedAttempt = false,
            CalculatedAt = DateTime.UtcNow
        };
        var cg2 = new CourseGrade
        {
            EnrollmentId = enrollment2.EnrollmentId,
            TotalObtained = 82,
            MaxPossible = 100,
            Percentage = 82,
            LetterGrade = "B",
            GradePoints = 3.00m,
            IsRepeatedAttempt = false,
            CalculatedAt = DateTime.UtcNow
        };
        db.CourseGrades.AddRange(cg1, cg2);
        await db.SaveChangesAsync();

        var calculator = ServiceFactory.CreateGpaCalculatorService(db);
        await calculator.RecalculateStudentGpaAndCgpaAsync(catalog.Student.StudentId);

        var record = await db.AcademicRecords
            .FirstOrDefaultAsync(r => r.StudentId == catalog.Student.StudentId && r.SemesterId == catalog.Semester.SemesterId);

        Assert.NotNull(record);
        // Expected GPA: (4.0 * 3 + 3.0 * 3) / 6 = 3.50
        Assert.Equal(3.50m, record.SemesterGpa);
        Assert.Equal(3.50m, record.CumulativeGpa);
        Assert.Equal(6, record.TotalCreditsAttempted);
        Assert.Equal(21.00m, record.TotalGradePoints); // 4.0 * 3 + 3.0 * 3 = 21.00 QP
    }

    [Fact]
    public async Task CalculateGpa_IncludesFailedCourse_CarriesZeroGP()
    {
        await using var database = await TestDatabase.CreateAsync();
        var db = database.Context;
        var catalog = await TestData.SeedCatalogAsync(db);

        // Add another course in the same semester
        var otherCourse = await TestData.AddCourseAsync(db, catalog.Department, "Database Systems");
        var otherOffering = await TestData.AddOfferingAsync(db, otherCourse, catalog.Semester, catalog.Instructor);

        // Enroll student in both
        var enrollment1 = new Enrollment
        {
            StudentId = catalog.Student.StudentId,
            OfferingId = catalog.Offering.OfferingId,
            Status = "COMPLETED"
        };
        var enrollment2 = new Enrollment
        {
            StudentId = catalog.Student.StudentId,
            OfferingId = otherOffering.OfferingId,
            Status = "COMPLETED"
        };
        db.Enrollments.AddRange(enrollment1, enrollment2);
        await db.SaveChangesAsync();

        // Finalized course grades: Course 1: A (4.00 GP, 3 credits), Course 2: F (0.00 GP, 3 credits)
        var cg1 = new CourseGrade
        {
            EnrollmentId = enrollment1.EnrollmentId,
            TotalObtained = 95,
            MaxPossible = 100,
            Percentage = 95,
            LetterGrade = "A",
            GradePoints = 4.00m,
            IsRepeatedAttempt = false,
            CalculatedAt = DateTime.UtcNow
        };
        var cg2 = new CourseGrade
        {
            EnrollmentId = enrollment2.EnrollmentId,
            TotalObtained = 40,
            MaxPossible = 100,
            Percentage = 40,
            LetterGrade = "F",
            GradePoints = 0.00m,
            IsRepeatedAttempt = false,
            CalculatedAt = DateTime.UtcNow
        };
        db.CourseGrades.AddRange(cg1, cg2);
        await db.SaveChangesAsync();

        var calculator = ServiceFactory.CreateGpaCalculatorService(db);
        await calculator.RecalculateStudentGpaAndCgpaAsync(catalog.Student.StudentId);

        var record = await db.AcademicRecords
            .FirstOrDefaultAsync(r => r.StudentId == catalog.Student.StudentId && r.SemesterId == catalog.Semester.SemesterId);

        Assert.NotNull(record);
        // Expected GPA: (4.0 * 3 + 0.0 * 3) / 6 = 2.00
        Assert.Equal(2.00m, record.SemesterGpa);
        Assert.Equal(2.00m, record.CumulativeGpa);
        Assert.Equal(6, record.TotalCreditsAttempted);
    }

    [Fact]
    public async Task RepeatedAttempts_RetainsHighestGrade_ExcludesOlderAttemptFromGpa()
    {
        await using var database = await TestDatabase.CreateAsync();
        var db = database.Context;
        
        // Setup Catalog for Semester 1 (Spring 2026)
        var catalog = await TestData.SeedCatalogAsync(db);

        // Setup Semester 2 (Fall 2026)
        var semester2 = new Semester
        {
            SemesterName = "Fall 2026",
            StartDate = new DateOnly(2026, 9, 1),
            EndDate = new DateOnly(2026, 12, 30),
            IsCurrent = false
        };
        db.Semesters.Add(semester2);
        await db.SaveChangesAsync();

        // Add offering for Course in Semester 2
        var offering2 = await TestData.AddOfferingAsync(db, catalog.Course, semester2, catalog.Instructor);

        // Attempt 1 in Semester 1
        var enrollment1 = new Enrollment
        {
            StudentId = catalog.Student.StudentId,
            OfferingId = catalog.Offering.OfferingId,
            Status = "COMPLETED"
        };
        db.Enrollments.Add(enrollment1);
        await db.SaveChangesAsync();

        var cg1 = new CourseGrade
        {
            EnrollmentId = enrollment1.EnrollmentId,
            TotalObtained = 65,
            MaxPossible = 100,
            Percentage = 65,
            LetterGrade = "C",
            GradePoints = 2.00m,
            IsRepeatedAttempt = false,
            CalculatedAt = DateTime.UtcNow
        };
        db.CourseGrades.Add(cg1);
        await db.SaveChangesAsync();

        // Attempt 2 in Semester 2 (Student gets A)
        var enrollment2 = new Enrollment
        {
            StudentId = catalog.Student.StudentId,
            OfferingId = offering2.OfferingId,
            Status = "COMPLETED",
            IsRepeated = true,
            OriginalEnrollmentId = enrollment1.EnrollmentId
        };
        db.Enrollments.Add(enrollment2);
        await db.SaveChangesAsync();

        var cg2 = new CourseGrade
        {
            EnrollmentId = enrollment2.EnrollmentId,
            TotalObtained = 95,
            MaxPossible = 100,
            Percentage = 95,
            LetterGrade = "A",
            GradePoints = 4.00m,
            IsRepeatedAttempt = false,
            CalculatedAt = DateTime.UtcNow
        };
        db.CourseGrades.Add(cg2);
        await db.SaveChangesAsync();

        // Recalculate
        var calculator = ServiceFactory.CreateGpaCalculatorService(db);
        await calculator.RecalculateStudentGpaAndCgpaAsync(catalog.Student.StudentId);

        // Fetch grades to check repeat flag
        var updatedCg1 = await db.CourseGrades.FindAsync(cg1.GradeId);
        var updatedCg2 = await db.CourseGrades.FindAsync(cg2.GradeId);

        Assert.NotNull(updatedCg1);
        Assert.NotNull(updatedCg2);
        
        // Lower attempt (Attempt 1) must be marked repeated and excluded
        Assert.True(updatedCg1.IsRepeatedAttempt);
        // Higher attempt (Attempt 2) must be active
        Assert.False(updatedCg2.IsRepeatedAttempt);

        // Fetch academic records
        var record1 = await db.AcademicRecords.FirstOrDefaultAsync(r => r.StudentId == catalog.Student.StudentId && r.SemesterId == catalog.Semester.SemesterId);
        var record2 = await db.AcademicRecords.FirstOrDefaultAsync(r => r.StudentId == catalog.Student.StudentId && r.SemesterId == semester2.SemesterId);

        Assert.NotNull(record1);
        Assert.NotNull(record2);

        // Semester 1 GPA should exclude Attempt 1 (0 active credits, 0 active quality points)
        Assert.Equal(0.00m, record1.SemesterGpa);
        
        // Semester 2 GPA should include Attempt 2 (3 credits, 4.0 GP => 4.00 GPA)
        Assert.Equal(4.00m, record2.SemesterGpa);

        // Cumulative CGPA in Semester 2 should be 4.00 (only Attempt 2 is counted)
        Assert.Equal(4.00m, record2.CumulativeGpa);
        Assert.Equal(3, record2.TotalCreditsAttempted);
    }

    [Fact]
    public async Task DecimalPrecision_VerifyRoundingToTwoDecimalPlaces()
    {
        await using var database = await TestDatabase.CreateAsync();
        var db = database.Context;
        var catalog = await TestData.SeedCatalogAsync(db);

        // Create a situation where quality points divide by credits resulting in recurring fraction
        // Course 1: 3 credits, B+ (3.33 GP)
        // Course 2: 3 credits, B (3.00 GP)
        // Expected GPA: (3.33 * 3 + 3.00 * 3) / 6 = (9.99 + 9) / 6 = 18.99 / 6 = 3.165 => Rounds to 3.17
        var otherCourse = await TestData.AddCourseAsync(db, catalog.Department, "Database Systems");
        var otherOffering = await TestData.AddOfferingAsync(db, otherCourse, catalog.Semester, catalog.Instructor);

        var enrollment1 = new Enrollment
        {
            StudentId = catalog.Student.StudentId,
            OfferingId = catalog.Offering.OfferingId,
            Status = "COMPLETED"
        };
        var enrollment2 = new Enrollment
        {
            StudentId = catalog.Student.StudentId,
            OfferingId = otherOffering.OfferingId,
            Status = "COMPLETED"
        };
        db.Enrollments.AddRange(enrollment1, enrollment2);
        await db.SaveChangesAsync();

        var cg1 = new CourseGrade
        {
            EnrollmentId = enrollment1.EnrollmentId,
            TotalObtained = 88,
            MaxPossible = 100,
            Percentage = 88,
            LetterGrade = "B+",
            GradePoints = 3.33m,
            IsRepeatedAttempt = false,
            CalculatedAt = DateTime.UtcNow
        };
        var cg2 = new CourseGrade
        {
            EnrollmentId = enrollment2.EnrollmentId,
            TotalObtained = 82,
            MaxPossible = 100,
            Percentage = 82,
            LetterGrade = "B",
            GradePoints = 3.00m,
            IsRepeatedAttempt = false,
            CalculatedAt = DateTime.UtcNow
        };
        db.CourseGrades.AddRange(cg1, cg2);
        await db.SaveChangesAsync();

        var calculator = ServiceFactory.CreateGpaCalculatorService(db);
        await calculator.RecalculateStudentGpaAndCgpaAsync(catalog.Student.StudentId);

        var record = await db.AcademicRecords
            .FirstOrDefaultAsync(r => r.StudentId == catalog.Student.StudentId && r.SemesterId == catalog.Semester.SemesterId);

        Assert.NotNull(record);
        // Rounds exactly to 2 decimal places: 3.165 => 3.17 (using MidpointRounding.AwayFromZero)
        Assert.Equal(3.17m, record.SemesterGpa);
    }
}
