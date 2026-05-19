using GpaSystem.API.Data;
using GpaSystem.API.DTOs;
using GpaSystem.API.Exceptions;
using GpaSystem.API.Models;
using GpaSystem.API.Repositories;

namespace GpaSystem.API.Services;

public class InstructorService : IInstructorService
{
    private readonly GpaSystemDbContext _db;
    private readonly IInstructorRepository _instructors;
    private readonly IDepartmentRepository _departments;
    private readonly ICredentialService _credentials;

    public InstructorService(
        GpaSystemDbContext db,
        IInstructorRepository instructors,
        IDepartmentRepository departments,
        ICredentialService credentials)
    {
        _db = db;
        _instructors = instructors;
        _departments = departments;
        _credentials = credentials;
    }

    public async Task<List<InstructorResponse>> GetAllAsync()
    {
        var instructors = await _instructors.GetAllAsync();
        return instructors.Select(Map).ToList();
    }

    public async Task<InstructorResponse> GetByIdAsync(int id)
    {
        var instructor = await FindInstructorAsync(id);
        return Map(instructor);
    }

    public async Task<CreateInstructorResponse> CreateAsync(CreateInstructorRequest request)
    {
        var department = await FindDepartmentForRequestAsync(request.DepartmentId);
        var email = NormalizeEmail(request.Email);

        if (await _instructors.EmailExistsAsync(email))
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
            Role = "INSTRUCTOR",
            IsActive = true,
            PasswordChangedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        var instructor = new Instructor
        {
            User = user,
            FullName = request.FullName.Trim(),
            DepartmentId = department.DepartmentId,
            Department = department,
            HireDate = request.HireDate ?? DateOnly.FromDateTime(DateTime.Today)
        };

        await _instructors.AddAsync(instructor);
        await _instructors.SaveChangesAsync();
        await transaction.CommitAsync();

        return new CreateInstructorResponse
        {
            Instructor = Map(instructor),
            Credentials = new TemporaryCredentialsResponse
            {
                Username = generatedCredentials.Username,
                TemporaryPassword = generatedCredentials.TemporaryPassword
            }
        };
    }

    public async Task<InstructorResponse> UpdateAsync(int id, UpdateInstructorRequest request)
    {
        var instructor = await FindInstructorAsync(id);
        var department = await FindDepartmentForRequestAsync(request.DepartmentId);
        var email = NormalizeEmail(request.Email);

        if (await _instructors.EmailExistsAsync(email, instructor.UserId))
        {
            throw ApiException.Conflict("Email is already registered.");
        }

        instructor.FullName = request.FullName.Trim();
        instructor.DepartmentId = department.DepartmentId;
        instructor.Department = department;
        instructor.HireDate = request.HireDate ?? instructor.HireDate;
        instructor.User.Email = email;

        await _instructors.SaveChangesAsync();
        return Map(instructor);
    }

    public async Task DeactivateAsync(int id)
    {
        var instructor = await FindInstructorAsync(id);
        instructor.User.IsActive = false;
        await _instructors.SaveChangesAsync();
    }

    private async Task<Instructor> FindInstructorAsync(int id)
    {
        return await _instructors.GetByIdAsync(id)
            ?? throw ApiException.NotFound("Instructor was not found.");
    }

    private async Task<Department> FindDepartmentForRequestAsync(int id)
    {
        return await _departments.GetByIdAsync(id)
            ?? throw ApiException.BadRequest("Department was not found.");
    }

    private static InstructorResponse Map(Instructor instructor)
    {
        return new InstructorResponse
        {
            InstructorId = instructor.InstructorId,
            UserId = instructor.UserId,
            FullName = instructor.FullName,
            Email = instructor.User.Email,
            Username = instructor.User.Username,
            DepartmentId = instructor.DepartmentId,
            DepartmentCode = instructor.Department.DepartmentCode,
            DepartmentName = instructor.Department.DepartmentName,
            HireDate = instructor.HireDate,
            IsActive = instructor.User.IsActive
        };
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
