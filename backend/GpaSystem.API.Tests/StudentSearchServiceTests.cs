using GpaSystem.API.DTOs;
using GpaSystem.API.Models;
using GpaSystem.API.Services;

namespace GpaSystem.API.Tests;

public class StudentSearchServiceTests
{
    [Fact]
    public async Task SearchAsync_FiltersByNameAndSortsByCgpaDescending()
    {
        await using var database = await TestDatabase.CreateAsync();
        var db = database.Context;
        var catalog = await TestData.SeedCatalogAsync(db);
        var lowGpa = await TestData.AddStudentAsync(db, catalog.Department);

        db.AcademicRecords.AddRange(
            new AcademicRecord
            {
                StudentId = catalog.Student.StudentId,
                SemesterId = catalog.Semester.SemesterId,
                SemesterGpa = 3.80m,
                CumulativeGpa = 3.80m,
                TotalCreditsAttempted = 3,
                TotalGradePoints = 11.4m,
                CalculationDate = DateTime.UtcNow
            },
            new AcademicRecord
            {
                StudentId = lowGpa.StudentId,
                SemesterId = catalog.Semester.SemesterId,
                SemesterGpa = 2.00m,
                CumulativeGpa = 2.00m,
                TotalCreditsAttempted = 3,
                TotalGradePoints = 6m,
                CalculationDate = DateTime.UtcNow
            });
        await db.SaveChangesAsync();

        var service = ServiceFactory.CreateStudentService(db);
        var result = await service.SearchAsync(new StudentSearchQuery
        {
            Search = "Ada",
            SortBy = "cgpa",
            SortDir = "desc",
            Page = 1,
            PageSize = 25
        });

        Assert.Single(result.Items);
        Assert.Equal(catalog.Student.StudentId, result.Items[0].StudentId);
        Assert.Equal(3.80m, result.Items[0].Cgpa);
    }

    [Fact]
    public async Task SearchAsync_FiltersByDepartment()
    {
        await using var database = await TestDatabase.CreateAsync();
        var db = database.Context;
        var catalog = await TestData.SeedCatalogAsync(db);

        var otherDept = new Department
        {
            DepartmentCode = "EE",
            DepartmentName = "Electrical Engineering",
            CreatedAt = DateTime.UtcNow
        };
        db.Departments.Add(otherDept);
        await db.SaveChangesAsync();

        await TestData.AddStudentAsync(db, otherDept);

        var service = ServiceFactory.CreateStudentService(db);
        var result = await service.SearchAsync(new StudentSearchQuery
        {
            DepartmentId = catalog.Department.DepartmentId,
            Page = 1,
            PageSize = 25
        });

        Assert.Single(result.Items);
        Assert.Equal(catalog.Department.DepartmentId, result.Items[0].DepartmentId);
    }

    [Fact]
    public async Task SearchAsync_FindsStudentByNumber()
    {
        await using var database = await TestDatabase.CreateAsync();
        var catalog = await TestData.SeedCatalogAsync(database.Context);
        var service = ServiceFactory.CreateStudentService(database.Context);

        var result = await service.SearchAsync(new StudentSearchQuery
        {
            Search = catalog.Student.StudentNumber[..6],
            Page = 1,
            PageSize = 25
        });

        Assert.Contains(result.Items, i => i.StudentId == catalog.Student.StudentId);
    }
}
