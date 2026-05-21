using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GpaSystem.API.Data;
using GpaSystem.API.DTOs;
using GpaSystem.API.Models;
using GpaSystem.API.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GpaSystem.API.Tests;

public class Phase5AuthorizationIntegrationTests
{
    [Fact]
    public async Task AnonymousProtectedEndpoint_ReturnsUnauthorized()
    {
        using var factory = new AuthWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/departments");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminCanAccessManagementEndpoint()
    {
        using var factory = new AuthWebApplicationFactory();
        var client = factory.CreateClient();
        await SignInAsync(client, factory.Seed.AdminUsername);

        var response = await client.GetAsync("/api/departments");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task StudentCannotAccessAnotherStudentsResults()
    {
        using var factory = new AuthWebApplicationFactory();
        var client = factory.CreateClient();
        await SignInAsync(client, factory.Seed.StudentUsername);

        var response = await client.GetAsync($"/api/students/{factory.Seed.OtherStudentId}/results");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task InstructorCannotAccessUnassignedGradebook()
    {
        using var factory = new AuthWebApplicationFactory();
        var client = factory.CreateClient();
        await SignInAsync(client, factory.Seed.InstructorUsername);

        var response = await client.GetAsync($"/api/offerings/{factory.Seed.UnassignedOfferingId}/gradebook");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static async Task SignInAsync(HttpClient client, string username)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Username = username,
            Password = AuthWebApplicationFactory.Password
        });
        response.EnsureSuccessStatusCode();
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(login);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
    }
}

internal sealed class AuthWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string Password = "Strong@123";
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public AuthSeed Seed { get; private set; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "GpaSystem",
                ["Jwt:Audience"] = "GpaSystemClient",
                ["Jwt:SigningKey"] = "GpaSystemPhase5LocalDevelopmentSigningKey2026",
                ["Jwt:ExpiryMinutes"] = "15"
            });
        });

        builder.ConfigureServices(services =>
        {
            _connection.Open();
            services.RemoveAll<DbContextOptions<GpaSystemDbContext>>();
            services.AddDbContext<GpaSystemDbContext>(options => options.UseSqlite(_connection));

            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<GpaSystemDbContext>();
            db.Database.EnsureCreated();
            Seed = SeedDatabase(db);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection.Dispose();
    }

    private static AuthSeed SeedDatabase(GpaSystemDbContext db)
    {
        var passwordService = new PasswordService();
        var hash = passwordService.HashPassword(Password);

        var department = new Department
        {
            DepartmentCode = "CS",
            DepartmentName = "Computer Science",
            CreatedAt = DateTime.UtcNow
        };

        var adminUser = CreateUser("phase5.admin", "phase5.admin@example.edu", AuthRoles.Admin, hash);
        var admin = new Administrator { User = adminUser, FullName = "Phase Five Admin" };

        var instructorUser = CreateUser("phase5.instructor", "phase5.instructor@example.edu", AuthRoles.Instructor, hash);
        var instructor = new Instructor
        {
            User = instructorUser,
            FullName = "Phase Five Instructor",
            Department = department,
            HireDate = DateOnly.FromDateTime(DateTime.Today)
        };

        var otherInstructorUser = CreateUser("phase5.other.instructor", "phase5.other.instructor@example.edu", AuthRoles.Instructor, hash);
        var otherInstructor = new Instructor
        {
            User = otherInstructorUser,
            FullName = "Other Instructor",
            Department = department,
            HireDate = DateOnly.FromDateTime(DateTime.Today)
        };

        var studentUser = CreateUser("phase5.student", "phase5.student@example.edu", AuthRoles.Student, hash);
        var student = new Student
        {
            User = studentUser,
            StudentNumber = "CS-2026-901",
            FullName = "Phase Five Student",
            Department = department,
            EnrollmentDate = DateOnly.FromDateTime(DateTime.Today),
            Status = "ACTIVE"
        };

        var otherStudentUser = CreateUser("phase5.other.student", "phase5.other.student@example.edu", AuthRoles.Student, hash);
        var otherStudent = new Student
        {
            User = otherStudentUser,
            StudentNumber = "CS-2026-902",
            FullName = "Other Student",
            Department = department,
            EnrollmentDate = DateOnly.FromDateTime(DateTime.Today),
            Status = "ACTIVE"
        };

        var semester = new Semester
        {
            SemesterName = "Spring 2026",
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 5, 30),
            IsCurrent = true
        };

        var course = new Course
        {
            CourseCode = "CS-401",
            CourseTitle = "Secure Systems",
            CreditHours = 3,
            Department = department,
            CreatedAt = DateTime.UtcNow
        };

        var assignedOffering = new CourseOffering
        {
            Course = course,
            Semester = semester,
            Instructor = instructor,
            MaxCapacity = 30,
            CurrentEnrollment = 0,
            Status = "ACTIVE"
        };

        var unassignedOffering = new CourseOffering
        {
            Course = course,
            Semester = semester,
            Instructor = otherInstructor,
            MaxCapacity = 30,
            CurrentEnrollment = 0,
            Status = "ACTIVE"
        };

        db.AddRange(
            department,
            admin,
            instructor,
            otherInstructor,
            student,
            otherStudent,
            semester,
            course,
            assignedOffering,
            unassignedOffering);
        db.SaveChanges();

        return new AuthSeed
        {
            AdminUsername = adminUser.Username,
            InstructorUsername = instructorUser.Username,
            StudentUsername = studentUser.Username,
            OtherStudentId = otherStudent.StudentId,
            UnassignedOfferingId = unassignedOffering.OfferingId
        };
    }

    private static AppUser CreateUser(string username, string email, string role, string passwordHash)
    {
        return new AppUser
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            PasswordChangedAt = DateTime.UtcNow
        };
    }
}

internal sealed class AuthSeed
{
    public string AdminUsername { get; set; } = string.Empty;
    public string InstructorUsername { get; set; } = string.Empty;
    public string StudentUsername { get; set; } = string.Empty;
    public int OtherStudentId { get; set; }
    public int UnassignedOfferingId { get; set; }
}
