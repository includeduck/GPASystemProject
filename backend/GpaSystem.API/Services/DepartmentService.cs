using GpaSystem.API.DTOs;
using GpaSystem.API.Exceptions;
using GpaSystem.API.Models;
using GpaSystem.API.Repositories;

namespace GpaSystem.API.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departments;

    public DepartmentService(IDepartmentRepository departments)
    {
        _departments = departments;
    }

    public async Task<List<DepartmentResponse>> GetAllAsync()
    {
        var departments = await _departments.GetAllAsync();
        return departments.Select(Map).ToList();
    }

    public async Task<DepartmentResponse> GetByIdAsync(int id)
    {
        var department = await FindDepartmentAsync(id);
        return Map(department);
    }

    public async Task<DepartmentResponse> CreateAsync(CreateDepartmentRequest request)
    {
        var code = NormalizeCode(request.DepartmentCode);
        var name = request.DepartmentName.Trim();

        if (await _departments.CodeExistsAsync(code))
        {
            throw ApiException.Conflict("Department code already exists.");
        }

        var department = new Department
        {
            DepartmentCode = code,
            DepartmentName = name,
            CreatedAt = DateTime.UtcNow
        };

        await _departments.AddAsync(department);
        await _departments.SaveChangesAsync();
        return Map(department);
    }

    public async Task<DepartmentResponse> UpdateAsync(int id, UpdateDepartmentRequest request)
    {
        var department = await FindDepartmentAsync(id);
        var code = NormalizeCode(request.DepartmentCode);

        if (await _departments.CodeExistsAsync(code, id))
        {
            throw ApiException.Conflict("Department code already exists.");
        }

        department.DepartmentCode = code;
        department.DepartmentName = request.DepartmentName.Trim();
        await _departments.SaveChangesAsync();
        return Map(department);
    }

    public async Task DeleteAsync(int id)
    {
        var department = await FindDepartmentAsync(id);

        if (await _departments.HasReferencesAsync(id))
        {
            throw ApiException.Conflict("Department cannot be deleted while students, instructors, or courses reference it.");
        }

        _departments.Remove(department);
        await _departments.SaveChangesAsync();
    }

    private async Task<Department> FindDepartmentAsync(int id)
    {
        return await _departments.GetByIdAsync(id)
            ?? throw ApiException.NotFound("Department was not found.");
    }

    private static string NormalizeCode(string code)
    {
        return code.Trim().ToUpperInvariant();
    }

    private static DepartmentResponse Map(Department department)
    {
        return new DepartmentResponse
        {
            DepartmentId = department.DepartmentId,
            DepartmentCode = department.DepartmentCode,
            DepartmentName = department.DepartmentName,
            CreatedAt = department.CreatedAt
        };
    }
}
