using GpaSystem.API.DTOs;

namespace GpaSystem.API.Services;

public interface IDepartmentService
{
    Task<List<DepartmentResponse>> GetAllAsync();
    Task<DepartmentResponse> GetByIdAsync(int id);
    Task<DepartmentResponse> CreateAsync(CreateDepartmentRequest request);
    Task<DepartmentResponse> UpdateAsync(int id, UpdateDepartmentRequest request);
    Task DeleteAsync(int id);
}
