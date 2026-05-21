using GpaSystem.API.DTOs;
using GpaSystem.API.Exceptions;
using GpaSystem.API.Models;
using GpaSystem.API.Services;
using Microsoft.EntityFrameworkCore;

namespace GpaSystem.API.Tests;

public class Phase1ServiceTests
{
    #region CredentialService Tests

    [Fact]
    public async Task CredentialService_GenerateAsync_CreatesExpectedUsernameFormat()
    {
        await using var database = await TestDatabase.CreateAsync();
        var credentialService = ServiceFactory.CreateCredentialService(database.Context);

        var credentials = await credentialService.GenerateAsync("Ada Lovelace", "ada@lovelace.com");

        Assert.Equal("ada.lovelace", credentials.Username);
        Assert.NotNull(credentials.TemporaryPassword);
        Assert.StartsWith("PBKDF2-SHA256:100000:", credentials.PasswordHash);
    }

    [Fact]
    public async Task CredentialService_GenerateAsync_ResolvesUsernameCollisions()
    {
        await using var database = await TestDatabase.CreateAsync();
        var credentialService = ServiceFactory.CreateCredentialService(database.Context);

        // Pre-create user with target username
        database.Context.AppUsers.Add(new AppUser
        {
            Username = "grace.hopper",
            Email = "grace1@hopper.com",
            PasswordHash = "hash",
            Role = "INSTRUCTOR",
            CreatedAt = DateTime.UtcNow
        });
        await database.Context.SaveChangesAsync();

        var credentials = await credentialService.GenerateAsync("Grace Hopper", "grace2@hopper.com");

        Assert.Equal("grace.hopper1", credentials.Username);
    }

    #endregion

    #region DepartmentService Tests

    [Fact]
    public async Task DepartmentService_CreateAsync_SucceedsAndNormalizesCode()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = ServiceFactory.CreateDepartmentService(database.Context);

        var result = await service.CreateAsync(new CreateDepartmentRequest
        {
            DepartmentCode = "  cse  ",
            DepartmentName = "Computer Science and Engineering"
        });

        Assert.Equal("CSE", result.DepartmentCode);
        Assert.Equal("Computer Science and Engineering", result.DepartmentName);

        var saved = await database.Context.Departments.FirstOrDefaultAsync(d => d.DepartmentCode == "CSE");
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task DepartmentService_CreateAsync_RejectsDuplicateCode()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = ServiceFactory.CreateDepartmentService(database.Context);

        await service.CreateAsync(new CreateDepartmentRequest
        {
            DepartmentCode = "MATH",
            DepartmentName = "Mathematics"
        });

        var exception = await Assert.ThrowsAsync<ApiException>(() => service.CreateAsync(new CreateDepartmentRequest
        {
            DepartmentCode = "math",
            DepartmentName = "Applied Mathematics"
        }));

        Assert.Equal(409, exception.StatusCode);
    }

    [Fact]
    public async Task DepartmentService_DeleteAsync_BlocksIfReferenced()
    {
        await using var database = await TestDatabase.CreateAsync();
        var catalog = await TestData.SeedCatalogAsync(database.Context);
        var service = ServiceFactory.CreateDepartmentService(database.Context);

        var exception = await Assert.ThrowsAsync<ApiException>(() => service.DeleteAsync(catalog.Department.DepartmentId));
        Assert.Equal(409, exception.StatusCode);
        Assert.Contains("reference", exception.Message);
    }

    #endregion

    #region StudentService Tests

    [Fact]
    public async Task StudentService_CreateAsync_CreatesStudentWithStudentNumberAndCredentials()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = ServiceFactory.CreateStudentService(database.Context);
        
        var dept = new Department { DepartmentCode = "CS", DepartmentName = "Computer Science", CreatedAt = DateTime.UtcNow };
        database.Context.Departments.Add(dept);
        await database.Context.SaveChangesAsync();

        var result = await service.CreateAsync(new CreateStudentRequest
        {
            FullName = "Alan Turing",
            Email = "alan@turing.org",
            DepartmentId = dept.DepartmentId,
            Phone = "123-456-7890"
        });

        Assert.NotNull(result.Credentials.TemporaryPassword);
        Assert.Equal("alan.turing", result.Credentials.Username);
        
        var currentYear = DateTime.UtcNow.Year;
        Assert.Equal($"CS-{currentYear}-001", result.Student.StudentNumber);

        var saved = await database.Context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.StudentId == result.Student.StudentId);
        Assert.NotNull(saved);
        Assert.Equal("ACTIVE", saved.Status);
        Assert.True(saved.User.IsActive);
    }

    [Fact]
    public async Task StudentService_CreateAsync_RejectsDuplicateEmail()
    {
        await using var database = await TestDatabase.CreateAsync();
        var catalog = await TestData.SeedCatalogAsync(database.Context);
        var service = ServiceFactory.CreateStudentService(database.Context);

        var exception = await Assert.ThrowsAsync<ApiException>(() => service.CreateAsync(new CreateStudentRequest
        {
            FullName = "Duplicate Student",
            Email = catalog.Student.User.Email,
            DepartmentId = catalog.Department.DepartmentId
        }));

        Assert.Equal(409, exception.StatusCode);
    }

    [Fact]
    public async Task StudentService_DeactivateAsync_UpdatesStatus()
    {
        await using var database = await TestDatabase.CreateAsync();
        var catalog = await TestData.SeedCatalogAsync(database.Context);
        var service = ServiceFactory.CreateStudentService(database.Context);

        await service.DeactivateAsync(catalog.Student.StudentId);

        var student = await database.Context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.StudentId == catalog.Student.StudentId);
        Assert.NotNull(student);
        Assert.Equal("INACTIVE", student.Status);
        Assert.False(student.User.IsActive);
    }

    #endregion

    #region InstructorService Tests

    [Fact]
    public async Task InstructorService_CreateAsync_CreatesInstructorWithCredentials()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = ServiceFactory.CreateInstructorService(database.Context);

        var dept = new Department { DepartmentCode = "EE", DepartmentName = "Electrical Engineering", CreatedAt = DateTime.UtcNow };
        database.Context.Departments.Add(dept);
        await database.Context.SaveChangesAsync();

        var result = await service.CreateAsync(new CreateInstructorRequest
        {
            FullName = "Nikola Tesla",
            Email = "tesla@ee.edu",
            DepartmentId = dept.DepartmentId
        });

        Assert.NotNull(result.Credentials.TemporaryPassword);
        Assert.Equal("nikola.tesla", result.Credentials.Username);

        var saved = await database.Context.Instructors.Include(i => i.User).FirstOrDefaultAsync(i => i.InstructorId == result.Instructor.InstructorId);
        Assert.NotNull(saved);
        Assert.True(saved.User.IsActive);
    }

    [Fact]
    public async Task InstructorService_DeactivateAsync_DeactivatesUserOnly()
    {
        await using var database = await TestDatabase.CreateAsync();
        var catalog = await TestData.SeedCatalogAsync(database.Context);
        var service = ServiceFactory.CreateInstructorService(database.Context);

        await service.DeactivateAsync(catalog.Instructor.InstructorId);

        var instructor = await database.Context.Instructors.Include(i => i.User).FirstOrDefaultAsync(i => i.InstructorId == catalog.Instructor.InstructorId);
        Assert.NotNull(instructor);
        Assert.False(instructor.User.IsActive);
    }

    #endregion

    #region CourseService Tests

    [Fact]
    public async Task CourseService_CreateAsync_AutoGeneratesCourseCode()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = ServiceFactory.CreateCourseService(database.Context);
        var catalog = await TestData.SeedCatalogAsync(database.Context);

        var result = await service.CreateAsync(new CreateCourseRequest
        {
            CourseTitle = "Intro to Database Systems",
            CreditHours = 4,
            DepartmentId = catalog.Department.DepartmentId,
            CourseCode = "" // Trigger auto generation
        });

        Assert.StartsWith($"{catalog.Department.DepartmentCode}-", result.CourseCode);
        Assert.Equal(4, result.CreditHours);

        var saved = await database.Context.Courses.FindAsync(result.CourseId);
        Assert.NotNull(saved);
        Assert.Equal(result.CourseCode, saved.CourseCode);
    }

    [Fact]
    public async Task CourseService_CreateAsync_RejectsDuplicateCode()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = ServiceFactory.CreateCourseService(database.Context);
        var catalog = await TestData.SeedCatalogAsync(database.Context);

        var exception = await Assert.ThrowsAsync<ApiException>(() => service.CreateAsync(new CreateCourseRequest
        {
            CourseTitle = "Algorithms Refactoring",
            CreditHours = 3,
            DepartmentId = catalog.Department.DepartmentId,
            CourseCode = catalog.Course.CourseCode
        }));

        Assert.Equal(409, exception.StatusCode);
    }

    [Fact]
    public async Task CourseService_DeleteAsync_BlocksIfHasPrerequisiteOrOffering()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = ServiceFactory.CreateCourseService(database.Context);
        var catalog = await TestData.SeedCatalogAsync(database.Context);

        // Blocks because catalog.Offering references this course
        var exception = await Assert.ThrowsAsync<ApiException>(() => service.DeleteAsync(catalog.Course.CourseId));
        Assert.Equal(409, exception.StatusCode);
    }

    #endregion
}
