using GpaSystem.API.DTOs;
using GpaSystem.API.Exceptions;

namespace GpaSystem.API.Tests;

public class PrerequisiteServiceTests
{
    [Fact]
    public async Task AddAsync_RejectsSelfDuplicateAndCircularPrerequisites()
    {
        await using var database = await TestDatabase.CreateAsync();
        var catalog = await TestData.SeedCatalogAsync(database.Context);
        var prerequisiteCourse = await TestData.AddCourseAsync(database.Context, catalog.Department);
        var service = ServiceFactory.CreatePrerequisiteService(database.Context);

        var selfException = await Assert.ThrowsAsync<ApiException>(() => service.AddAsync(
            catalog.Course.CourseId,
            new AddPrerequisiteRequest { PrerequisiteCourseId = catalog.Course.CourseId }));
        Assert.Equal(400, selfException.StatusCode);

        await service.AddAsync(
            catalog.Course.CourseId,
            new AddPrerequisiteRequest { PrerequisiteCourseId = prerequisiteCourse.CourseId });

        var duplicateException = await Assert.ThrowsAsync<ApiException>(() => service.AddAsync(
            catalog.Course.CourseId,
            new AddPrerequisiteRequest { PrerequisiteCourseId = prerequisiteCourse.CourseId }));
        Assert.Equal(409, duplicateException.StatusCode);

        var circularException = await Assert.ThrowsAsync<ApiException>(() => service.AddAsync(
            prerequisiteCourse.CourseId,
            new AddPrerequisiteRequest { PrerequisiteCourseId = catalog.Course.CourseId }));
        Assert.Equal(409, circularException.StatusCode);
    }
}
