using GpaSystem.API.DTOs;
using GpaSystem.API.Exceptions;
using GpaSystem.API.Models;

namespace GpaSystem.API.Tests;

public class CourseOfferingServiceTests
{
    [Fact]
    public async Task CreateAsync_RejectsInvalidReferences()
    {
        await using var database = await TestDatabase.CreateAsync();
        var catalog = await TestData.SeedCatalogAsync(database.Context);
        var service = ServiceFactory.CreateCourseOfferingService(database.Context);

        var invalidCourse = await Assert.ThrowsAsync<ApiException>(() => service.CreateAsync(new CreateCourseOfferingRequest
        {
            CourseId = 999,
            SemesterId = catalog.Semester.SemesterId,
            InstructorId = catalog.Instructor.InstructorId,
            MaxCapacity = 10
        }));

        var invalidSemester = await Assert.ThrowsAsync<ApiException>(() => service.CreateAsync(new CreateCourseOfferingRequest
        {
            CourseId = catalog.Course.CourseId,
            SemesterId = 999,
            InstructorId = catalog.Instructor.InstructorId,
            MaxCapacity = 10
        }));

        var invalidInstructor = await Assert.ThrowsAsync<ApiException>(() => service.CreateAsync(new CreateCourseOfferingRequest
        {
            CourseId = catalog.Course.CourseId,
            SemesterId = catalog.Semester.SemesterId,
            InstructorId = 999,
            MaxCapacity = 10
        }));

        Assert.Equal(400, invalidCourse.StatusCode);
        Assert.Equal(400, invalidSemester.StatusCode);
        Assert.Equal(400, invalidInstructor.StatusCode);
    }

    [Fact]
    public async Task UpdateAsync_RejectsCapacityBelowCurrentEnrollment()
    {
        await using var database = await TestDatabase.CreateAsync();
        var catalog = await TestData.SeedCatalogAsync(database.Context, maxCapacity: 3);
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
        var service = ServiceFactory.CreateCourseOfferingService(database.Context);

        var exception = await Assert.ThrowsAsync<ApiException>(() => service.UpdateAsync(
            catalog.Offering.OfferingId,
            new UpdateCourseOfferingRequest
            {
                CourseId = catalog.Course.CourseId,
                SemesterId = catalog.Semester.SemesterId,
                InstructorId = catalog.Instructor.InstructorId,
                MaxCapacity = 0,
                Status = "ACTIVE"
            }));

        Assert.Equal(409, exception.StatusCode);
    }
}
