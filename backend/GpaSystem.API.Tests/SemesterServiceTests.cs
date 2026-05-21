using GpaSystem.API.DTOs;
using GpaSystem.API.Exceptions;
using GpaSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GpaSystem.API.Tests;

public class SemesterServiceTests
{
    [Fact]
    public async Task CreateAsync_RejectsEndDateBeforeStartDate()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = ServiceFactory.CreateSemesterService(database.Context);

        var ex = await Assert.ThrowsAsync<ApiException>(() => service.CreateAsync(new CreateSemesterRequest
        {
            SemesterName = "Invalid Term",
            StartDate = new DateOnly(2026, 6, 1),
            EndDate = new DateOnly(2026, 1, 1),
            IsCurrent = false
        }));

        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_ClearsOtherCurrentSemesters_WhenMarkedCurrent()
    {
        await using var database = await TestDatabase.CreateAsync();
        var catalog = await TestData.SeedCatalogAsync(database.Context);
        var service = ServiceFactory.CreateSemesterService(database.Context);

        var created = await service.CreateAsync(new CreateSemesterRequest
        {
            SemesterName = "Fall 2026",
            StartDate = new DateOnly(2026, 9, 1),
            EndDate = new DateOnly(2026, 12, 20),
            IsCurrent = true
        });

        Assert.True(created.IsCurrent);

        var previous = await database.Context.Semesters.FindAsync(catalog.Semester.SemesterId);
        Assert.NotNull(previous);
        Assert.False(previous.IsCurrent);
    }

    [Fact]
    public async Task SetCurrentAsync_SwitchesCurrentFlag()
    {
        await using var database = await TestDatabase.CreateAsync();
        var catalog = await TestData.SeedCatalogAsync(database.Context);
        var service = ServiceFactory.CreateSemesterService(database.Context);

        var fall = await service.CreateAsync(new CreateSemesterRequest
        {
            SemesterName = "Fall 2026",
            StartDate = new DateOnly(2026, 9, 1),
            EndDate = new DateOnly(2026, 12, 20),
            IsCurrent = false
        });

        var current = await service.SetCurrentAsync(fall.SemesterId);

        Assert.True(current.IsCurrent);
        var spring = await database.Context.Semesters.FindAsync(catalog.Semester.SemesterId);
        Assert.NotNull(spring);
        Assert.False(spring.IsCurrent);
    }

    [Fact]
    public async Task DeleteAsync_BlocksWhenReferencedByOffering()
    {
        await using var database = await TestDatabase.CreateAsync();
        var catalog = await TestData.SeedCatalogAsync(database.Context);
        var service = ServiceFactory.CreateSemesterService(database.Context);

        var ex = await Assert.ThrowsAsync<ApiException>(() => service.DeleteAsync(catalog.Semester.SemesterId));
        Assert.Equal(409, ex.StatusCode);
    }
}
