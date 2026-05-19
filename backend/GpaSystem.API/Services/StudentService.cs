using GpaSystem.API.Data;
using GpaSystem.API.DTOs;
using GpaSystem.API.Exceptions;
using GpaSystem.API.Models;
using GpaSystem.API.Repositories;

namespace GpaSystem.API.Services;

public class StudentService : IStudentService
{
    private readonly GpaSystemDbContext _db;
    private readonly IStudentRepository _students;
    private readonly IDepartmentRepository _departments;
    private readonly ICredentialService _credentials;

    public StudentService(
        GpaSystemDbContext db,
        IStudentRepository students,
        IDepartmentRepository departments,
        ICredentialService credentials)
    {
        _db = db;
        _students = students;
        _departments = departments;
        _credentials = credentials;
    }

    public async Task<List<StudentResponse>> GetAllAsync()
    {
        var students = await _students.GetAllAsync();
        return students.Select(Map).ToList();
    }

    public async Task<StudentResponse> GetByIdAsync(int id)
    {
        var student = await FindStudentAsync(id);
        return Map(student);
    }

    public async Task<CreateStudentResponse> CreateAsync(CreateStudentRequest request)
    {
        var department = await FindDepartmentForRequestAsync(request.DepartmentId);
        var email = NormalizeEmail(request.Email);

        if (await _students.EmailExistsAsync(email))
        {
            throw ApiException.Conflict("Email is already registered.");
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();
        var generatedCredentials = await _credentials.GenerateAsync(request.FullName, email);

        var user = new AppUser
        {
            Username = generatedCredentials.Username,
            PasswordHash = generatedCredentials.PasswordHash,
            Email = email,
            Role = "STUDENT",
            IsActive = true,
            PasswordChangedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        var student = new Student
        {
            User = user,
            StudentNumber = await GenerateStudentNumberAsync(department.DepartmentCode),
            FullName = request.FullName.Trim(),
            Phone = NormalizeOptional(request.Phone),
            DepartmentId = department.DepartmentId,
            Department = department,
            EnrollmentDate = request.EnrollmentDate ?? DateOnly.FromDateTime(DateTime.Today),
            Status = "ACTIVE"
        };

        await _students.AddAsync(student);
        await _students.SaveChangesAsync();
        await transaction.CommitAsync();

        return new CreateStudentResponse
        {
            Student = Map(student),
            Credentials = new TemporaryCredentialsResponse
            {
                Username = generatedCredentials.Username,
                TemporaryPassword = generatedCredentials.TemporaryPassword
            }
        };
    }

    public async Task<StudentResponse> UpdateAsync(int id, UpdateStudentRequest request)
    {
        var student = await FindStudentAsync(id);
        var department = await FindDepartmentForRequestAsync(request.DepartmentId);
        var email = NormalizeEmail(request.Email);

        if (await _students.EmailExistsAsync(email, student.UserId))
        {
            throw ApiException.Conflict("Email is already registered.");
        }

        var status = request.Status.Trim().ToUpperInvariant();
        student.FullName = request.FullName.Trim();
        student.Phone = NormalizeOptional(request.Phone);
        student.DepartmentId = department.DepartmentId;
        student.Department = department;
        student.EnrollmentDate = request.EnrollmentDate ?? student.EnrollmentDate;
        student.Status = status;
        student.User.Email = email;
        student.User.IsActive = status != "INACTIVE";

        await _students.SaveChangesAsync();
        return Map(student);
    }

    public async Task DeactivateAsync(int id)
    {
        var student = await FindStudentAsync(id);
        student.Status = "INACTIVE";
        student.User.IsActive = false;
        await _students.SaveChangesAsync();
    }

    private async Task<Student> FindStudentAsync(int id)
    {
        return await _students.GetByIdAsync(id)
            ?? throw ApiException.NotFound("Student was not found.");
    }

    private async Task<Department> FindDepartmentForRequestAsync(int id)
    {
        return await _departments.GetByIdAsync(id)
            ?? throw ApiException.BadRequest("Department was not found.");
    }

    private async Task<string> GenerateStudentNumberAsync(string departmentCode)
    {
        var prefix = $"{departmentCode.Trim().ToUpperInvariant()}-{DateTime.UtcNow.Year}-";
        var sequence = await _students.CountByStudentNumberPrefixAsync(prefix) + 1;
        var studentNumber = $"{prefix}{sequence:D3}";

        while (await _students.StudentNumberExistsAsync(studentNumber))
        {
            sequence++;
            studentNumber = $"{prefix}{sequence:D3}";
        }

        return studentNumber;
    }

    private static StudentResponse Map(Student student)
    {
        return new StudentResponse
        {
            StudentId = student.StudentId,
            UserId = student.UserId,
            StudentNumber = student.StudentNumber,
            FullName = student.FullName,
            Email = student.User.Email,
            Username = student.User.Username,
            Phone = student.Phone,
            DepartmentId = student.DepartmentId,
            DepartmentCode = student.Department.DepartmentCode,
            DepartmentName = student.Department.DepartmentName,
            EnrollmentDate = student.EnrollmentDate,
            Status = student.Status,
            IsActive = student.User.IsActive
        };
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static string? NormalizeOptional(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }
}
