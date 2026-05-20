using GpaSystem.API.DTOs;
using GpaSystem.API.Exceptions;
using GpaSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GpaSystem.API.Tests;

public class EnrollmentServiceTests
{
    [Fact]
    public async Task EnrollAsync_CreatesEnrollmentAndReconcilesSeatCount()
    {
        await using var database = await TestDatabase.CreateAsync();
        var catalog = await TestData.SeedCatalogAsync(database.Context);
        var service = ServiceFactory.CreateEnrollmentService(database.Context);

        var enrollment = await service.EnrollAsync(new CreateEnrollmentRequest
        {
            StudentId = catalog.Student.StudentId,
            OfferingId = catalog.Offering.OfferingId
        });

        var offering = await database.Context.CourseOfferings.FindAsync(catalog.Offering.OfferingId);
        Assert.Equal(catalog.Student.StudentId, enrollment.StudentId);
        Assert.Equal("ENROLLED", enrollment.Status);
        Assert.Equal(1, offering?.CurrentEnrollment);
    }

    [Fact]
    public async Task EnrollAsync_RejectsDuplicateEnrollment()
    {
        await using var database = await TestDatabase.CreateAsync();
        var catalog = await TestData.SeedCatalogAsync(database.Context);
        var service = ServiceFactory.CreateEnrollmentService(database.Context);

        await service.EnrollAsync(new CreateEnrollmentRequest
        {
            StudentId = catalog.Student.StudentId,
            OfferingId = catalog.Offering.OfferingId
        });

        var exception = await Assert.ThrowsAsync<ApiException>(() => service.EnrollAsync(new CreateEnrollmentRequest
        {
            StudentId = catalog.Student.StudentId,
            OfferingId = catalog.Offering.OfferingId
        }));

        Assert.Equal(409, exception.StatusCode);
    }

    [Fact]
    public async Task EnrollAsync_RejectsFullOffering()
    {
        await using var database = await TestDatabase.CreateAsync();
        var catalog = await TestData.SeedCatalogAsync(database.Context, maxCapacity: 1);
        var otherStudent = await TestData.AddStudentAsync(database.Context, catalog.Department);
        database.Context.Enrollments.Add(new Enrollment
        {
            Student = otherStudent,
            CourseOffering = catalog.Offering,
            EnrollmentDate = DateTime.UtcNow,
            Status = "ENROLLED"
        });
        catalog.Offering.CurrentEnrollment = 1;
        await database.Context.SaveChangesAsync();
        var service = ServiceFactory.CreateEnrollmentService(database.Context);

        var exception = await Assert.ThrowsAsync<ApiException>(() => service.EnrollAsync(new CreateEnrollmentRequest
        {
            StudentId = catalog.Student.StudentId,
            OfferingId = catalog.Offering.OfferingId
        }));

        Assert.Equal(409, exception.StatusCode);
    }

    [Fact]
    public async Task EnrollAsync_RejectsInactiveStudent()
    {
        await using var database = await TestDatabase.CreateAsync();
        var catalog = await TestData.SeedCatalogAsync(database.Context, studentStatus: "INACTIVE");
        var service = ServiceFactory.CreateEnrollmentService(database.Context);

        var exception = await Assert.ThrowsAsync<ApiException>(() => service.EnrollAsync(new CreateEnrollmentRequest
        {
            StudentId = catalog.Student.StudentId,
            OfferingId = catalog.Offering.OfferingId
        }));

        Assert.Equal(400, exception.StatusCode);
    }

    [Fact]
    public async Task EnrollAsync_RejectsMissingPrerequisite()
    {
        await using var database = await TestDatabase.CreateAsync();
        var catalog = await TestData.SeedCatalogAsync(database.Context);
        var prerequisiteCourse = await TestData.AddCourseAsync(database.Context, catalog.Department);
        database.Context.CoursePrerequisites.Add(new CoursePrerequisite
        {
            Course = catalog.Course,
            PrerequisiteCourse = prerequisiteCourse
        });
        await database.Context.SaveChangesAsync();
        var service = ServiceFactory.CreateEnrollmentService(database.Context);

        var exception = await Assert.ThrowsAsync<ApiException>(() => service.EnrollAsync(new CreateEnrollmentRequest
        {
            StudentId = catalog.Student.StudentId,
            OfferingId = catalog.Offering.OfferingId
        }));

        Assert.Equal(409, exception.StatusCode);
        Assert.Contains(prerequisiteCourse.CourseCode, exception.Message);
    }

    [Fact]
    public async Task EnrollAsync_AllowsPassedPrerequisite()
    {
        await using var database = await TestDatabase.CreateAsync();
        var catalog = await TestData.SeedCatalogAsync(database.Context);
        var prerequisiteCourse = await TestData.AddCourseAsync(database.Context, catalog.Department);
        var prerequisiteOffering = await TestData.AddOfferingAsync(
            database.Context,
            prerequisiteCourse,
            catalog.Semester,
            catalog.Instructor);

        var completedEnrollment = new Enrollment
        {
            Student = catalog.Student,
            CourseOffering = prerequisiteOffering,
            EnrollmentDate = DateTime.UtcNow.AddMonths(-3),
            Status = "COMPLETED"
        };
        database.Context.Enrollments.Add(completedEnrollment);
        database.Context.CourseGrades.Add(new CourseGrade
        {
            Enrollment = completedEnrollment,
            TotalObtained = 82,
            MaxPossible = 100,
            Percentage = 82,
            LetterGrade = "B",
            GradePoints = 3,
            CalculatedAt = DateTime.UtcNow
        });
        database.Context.CoursePrerequisites.Add(new CoursePrerequisite
        {
            Course = catalog.Course,
            PrerequisiteCourse = prerequisiteCourse
        });
        await database.Context.SaveChangesAsync();
        var service = ServiceFactory.CreateEnrollmentService(database.Context);

        var enrollment = await service.EnrollAsync(new CreateEnrollmentRequest
        {
            StudentId = catalog.Student.StudentId,
            OfferingId = catalog.Offering.OfferingId
        });

        Assert.Equal(catalog.Course.CourseId, enrollment.CourseId);
        Assert.Equal(1, await database.Context.Enrollments.CountAsync(e =>
            e.StudentId == catalog.Student.StudentId &&
            e.OfferingId == catalog.Offering.OfferingId));
    }
}
