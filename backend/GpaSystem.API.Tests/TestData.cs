using GpaSystem.API.Data;
using GpaSystem.API.Models;
using GpaSystem.API.Repositories;
using GpaSystem.API.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GpaSystem.API.Tests;

internal sealed class TestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    private TestDatabase(SqliteConnection connection, GpaSystemDbContext context)
    {
        _connection = connection;
        Context = context;
    }

    public GpaSystemDbContext Context { get; }

    public static async Task<TestDatabase> CreateAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<GpaSystemDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new GpaSystemDbContext(options);
        await context.Database.EnsureCreatedAsync();

        return new TestDatabase(connection, context);
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await _connection.DisposeAsync();
    }
}

internal static class ServiceFactory
{
    public static EnrollmentService CreateEnrollmentService(GpaSystemDbContext db)
    {
        return new EnrollmentService(
            db,
            new EnrollmentRepository(db),
            new StudentRepository(db),
            new CourseOfferingRepository(db),
            new SemesterRepository(db),
            new CoursePrerequisiteRepository(db));
    }

    public static PrerequisiteService CreatePrerequisiteService(GpaSystemDbContext db)
    {
        return new PrerequisiteService(
            new CourseRepository(db),
            new CoursePrerequisiteRepository(db));
    }

    public static CourseOfferingService CreateCourseOfferingService(GpaSystemDbContext db)
    {
        return new CourseOfferingService(
            new CourseOfferingRepository(db),
            new CourseRepository(db),
            new SemesterRepository(db),
            new InstructorRepository(db));
    }

    public static StudentService CreateStudentService(GpaSystemDbContext db)
    {
        return new StudentService(
            db,
            new StudentRepository(db),
            new DepartmentRepository(db),
            new CredentialService(db, new PasswordService()));
    }

    public static InstructorService CreateInstructorService(GpaSystemDbContext db)
    {
        return new InstructorService(
            db,
            new InstructorRepository(db),
            new DepartmentRepository(db),
            new CredentialService(db, new PasswordService()));
    }

    public static DepartmentService CreateDepartmentService(GpaSystemDbContext db)
    {
        return new DepartmentService(
            new DepartmentRepository(db));
    }

    public static CourseService CreateCourseService(GpaSystemDbContext db)
    {
        return new CourseService(
            new CourseRepository(db),
            new DepartmentRepository(db));
    }

    public static CredentialService CreateCredentialService(GpaSystemDbContext db)
    {
        return new CredentialService(db, new PasswordService());
    }

    public static PasswordService CreatePasswordService()
    {
        return new PasswordService();
    }

    public static AuthService CreateAuthService(GpaSystemDbContext db)
    {
        var settings = new Dictionary<string, string?>
        {
            ["Jwt:Issuer"] = "GpaSystem.Tests",
            ["Jwt:Audience"] = "GpaSystem.Tests.Client",
            ["Jwt:SigningKey"] = "GpaSystemTestsSigningKeyForPhaseFive2026",
            ["Jwt:ExpiryMinutes"] = "15"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        return new AuthService(db, new PasswordService(), configuration);
    }

    public static ReportService CreateReportService(GpaSystemDbContext db)
    {
        return new ReportService(
            db,
            new StudentRepository(db),
            new AcademicRecordRepository(db),
            new SemesterRepository(db),
            new DepartmentRepository(db),
            new CourseRepository(db));
    }

    public static GpaCalculatorService CreateGpaCalculatorService(GpaSystemDbContext db)
    {
        return new GpaCalculatorService(
            db,
            new CourseGradeRepository(db),
            new AcademicRecordRepository(db),
            new StudentRepository(db),
            new SemesterRepository(db),
            CreateReportService(db));
    }

    public static GradeService CreateGradeService(GpaSystemDbContext db)
    {
        return new GradeService(
            db,
            new GradeComponentRepository(db),
            new GradeEntryRepository(db),
            new CourseOfferingRepository(db),
            new EnrollmentRepository(db),
            new CourseGradeRepository(db),
            new GradingPolicyRepository(db),
            CreateGpaCalculatorService(db),
            new StandardGradingStrategy());
    }

    public static GradingPolicyService CreateGradingPolicyService(GpaSystemDbContext db)
    {
        return new GradingPolicyService(
            db,
            new GradingPolicyRepository(db));
    }

    public static SemesterService CreateSemesterService(GpaSystemDbContext db)
    {
        return new SemesterService(new SemesterRepository(db));
    }
}

internal sealed record SeededCatalog(
    Department Department,
    Student Student,
    Instructor Instructor,
    Semester Semester,
    Course Course,
    CourseOffering Offering);

internal static class TestData
{
    public static async Task<SeededCatalog> SeedCatalogAsync(
        GpaSystemDbContext db,
        int maxCapacity = 3,
        string offeringStatus = "ACTIVE",
        string studentStatus = "ACTIVE")
    {
        var department = new Department
        {
            DepartmentCode = $"CS{Guid.NewGuid():N}"[..8].ToUpperInvariant(),
            DepartmentName = "Computer Science",
            CreatedAt = DateTime.UtcNow
        };

        var studentUser = new AppUser
        {
            Username = $"student.{Guid.NewGuid():N}"[..20],
            PasswordHash = "hash",
            Email = $"student-{Guid.NewGuid():N}@example.edu",
            Role = "STUDENT",
            IsActive = studentStatus == "ACTIVE",
            CreatedAt = DateTime.UtcNow,
            PasswordChangedAt = DateTime.UtcNow
        };

        var instructorUser = new AppUser
        {
            Username = $"instructor.{Guid.NewGuid():N}"[..23],
            PasswordHash = "hash",
            Email = $"instructor-{Guid.NewGuid():N}@example.edu",
            Role = "INSTRUCTOR",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            PasswordChangedAt = DateTime.UtcNow
        };

        var student = new Student
        {
            User = studentUser,
            StudentNumber = $"CS-{Guid.NewGuid():N}"[..14].ToUpperInvariant(),
            FullName = "Ada Lovelace",
            Department = department,
            EnrollmentDate = DateOnly.FromDateTime(DateTime.Today),
            Status = studentStatus
        };

        var instructor = new Instructor
        {
            User = instructorUser,
            FullName = "Grace Hopper",
            Department = department,
            HireDate = DateOnly.FromDateTime(DateTime.Today)
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
            CourseCode = $"CS-{Guid.NewGuid():N}"[..8].ToUpperInvariant(),
            CourseTitle = "Algorithms",
            CreditHours = 3,
            Department = department,
            CreatedAt = DateTime.UtcNow
        };

        var offering = new CourseOffering
        {
            Course = course,
            Semester = semester,
            Instructor = instructor,
            MaxCapacity = maxCapacity,
            CurrentEnrollment = 0,
            Status = offeringStatus,
            IsGradeFinalized = false
        };

        db.AddRange(department, student, instructor, semester, course, offering);
        await db.SaveChangesAsync();

        return new SeededCatalog(department, student, instructor, semester, course, offering);
    }

    public static async Task<Student> AddStudentAsync(
        GpaSystemDbContext db,
        Department department,
        string status = "ACTIVE")
    {
        var user = new AppUser
        {
            Username = $"student.{Guid.NewGuid():N}"[..20],
            PasswordHash = "hash",
            Email = $"student-{Guid.NewGuid():N}@example.edu",
            Role = "STUDENT",
            IsActive = status == "ACTIVE",
            CreatedAt = DateTime.UtcNow,
            PasswordChangedAt = DateTime.UtcNow
        };

        var student = new Student
        {
            User = user,
            StudentNumber = $"CS-{Guid.NewGuid():N}"[..14].ToUpperInvariant(),
            FullName = "Alan Turing",
            Department = department,
            EnrollmentDate = DateOnly.FromDateTime(DateTime.Today),
            Status = status
        };

        db.Students.Add(student);
        await db.SaveChangesAsync();
        return student;
    }

    public static async Task<Course> AddCourseAsync(
        GpaSystemDbContext db,
        Department department,
        string title = "Programming Fundamentals")
    {
        var course = new Course
        {
            CourseCode = $"CS-{Guid.NewGuid():N}"[..8].ToUpperInvariant(),
            CourseTitle = title,
            CreditHours = 3,
            Department = department,
            CreatedAt = DateTime.UtcNow
        };

        db.Courses.Add(course);
        await db.SaveChangesAsync();
        return course;
    }

    public static async Task<CourseOffering> AddOfferingAsync(
        GpaSystemDbContext db,
        Course course,
        Semester semester,
        Instructor instructor,
        int maxCapacity = 3)
    {
        var offering = new CourseOffering
        {
            Course = course,
            Semester = semester,
            Instructor = instructor,
            MaxCapacity = maxCapacity,
            CurrentEnrollment = 0,
            Status = "ACTIVE"
        };

        db.CourseOfferings.Add(offering);
        await db.SaveChangesAsync();
        return offering;
    }
}
