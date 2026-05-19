using GpaSystem.API.DTOs;
using GpaSystem.API.Exceptions;
using GpaSystem.API.Models;
using GpaSystem.API.Repositories;

namespace GpaSystem.API.Services;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courses;
    private readonly IDepartmentRepository _departments;

    public CourseService(ICourseRepository courses, IDepartmentRepository departments)
    {
        _courses = courses;
        _departments = departments;
    }

    public async Task<List<CourseResponse>> GetAllAsync()
    {
        var courses = await _courses.GetAllAsync();
        return courses.Select(Map).ToList();
    }

    public async Task<CourseResponse> GetByIdAsync(int id)
    {
        var course = await FindCourseAsync(id);
        return Map(course);
    }

    public async Task<CourseResponse> CreateAsync(CreateCourseRequest request)
    {
        var department = await FindDepartmentForRequestAsync(request.DepartmentId);
        var courseCode = string.IsNullOrWhiteSpace(request.CourseCode)
            ? await GenerateCourseCodeAsync(department.DepartmentCode)
            : NormalizeCode(request.CourseCode);

        if (await _courses.CodeExistsAsync(courseCode))
        {
            throw ApiException.Conflict("Course code already exists.");
        }

        var course = new Course
        {
            CourseCode = courseCode,
            CourseTitle = request.CourseTitle.Trim(),
            CreditHours = request.CreditHours,
            DepartmentId = department.DepartmentId,
            Department = department,
            Description = NormalizeOptional(request.Description),
            CreatedAt = DateTime.UtcNow
        };

        await _courses.AddAsync(course);
        await _courses.SaveChangesAsync();
        return Map(course);
    }

    public async Task<CourseResponse> UpdateAsync(int id, UpdateCourseRequest request)
    {
        var course = await FindCourseAsync(id);
        var department = await FindDepartmentForRequestAsync(request.DepartmentId);
        var courseCode = NormalizeCode(request.CourseCode);

        if (await _courses.CodeExistsAsync(courseCode, id))
        {
            throw ApiException.Conflict("Course code already exists.");
        }

        course.CourseCode = courseCode;
        course.CourseTitle = request.CourseTitle.Trim();
        course.CreditHours = request.CreditHours;
        course.DepartmentId = department.DepartmentId;
        course.Department = department;
        course.Description = NormalizeOptional(request.Description);

        await _courses.SaveChangesAsync();
        return Map(course);
    }

    public async Task DeleteAsync(int id)
    {
        var course = await FindCourseAsync(id);

        if (await _courses.HasReferencesAsync(id))
        {
            throw ApiException.Conflict("Course cannot be deleted while offerings or prerequisites reference it.");
        }

        _courses.Remove(course);
        await _courses.SaveChangesAsync();
    }

    private async Task<Course> FindCourseAsync(int id)
    {
        return await _courses.GetByIdAsync(id)
            ?? throw ApiException.NotFound("Course was not found.");
    }

    private async Task<Department> FindDepartmentForRequestAsync(int id)
    {
        return await _departments.GetByIdAsync(id)
            ?? throw ApiException.BadRequest("Department was not found.");
    }

    private async Task<string> GenerateCourseCodeAsync(string departmentCode)
    {
        var prefix = $"{departmentCode.Trim().ToUpperInvariant()}-";
        var sequence = await _courses.CountByCourseCodePrefixAsync(prefix) + 1;
        var courseCode = $"{prefix}{sequence:D3}";

        while (await _courses.CodeExistsAsync(courseCode))
        {
            sequence++;
            courseCode = $"{prefix}{sequence:D3}";
        }

        return courseCode;
    }

    private static CourseResponse Map(Course course)
    {
        return new CourseResponse
        {
            CourseId = course.CourseId,
            CourseCode = course.CourseCode,
            CourseTitle = course.CourseTitle,
            CreditHours = course.CreditHours,
            DepartmentId = course.DepartmentId,
            DepartmentCode = course.Department.DepartmentCode,
            DepartmentName = course.Department.DepartmentName,
            Description = course.Description,
            CreatedAt = course.CreatedAt
        };
    }

    private static string NormalizeCode(string? code)
    {
        return code?.Trim().ToUpperInvariant() ?? string.Empty;
    }

    private static string? NormalizeOptional(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }
}
