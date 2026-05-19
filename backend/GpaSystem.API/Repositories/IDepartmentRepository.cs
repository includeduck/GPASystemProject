using GpaSystem.API.Models;

namespace GpaSystem.API.Repositories;

public interface IDepartmentRepository
{
    Task<List<Department>> GetAllAsync();
    Task<Department?> GetByIdAsync(int id);
    Task<bool> CodeExistsAsync(string departmentCode, int? excludeDepartmentId = null);
    Task<bool> HasReferencesAsync(int id);
    Task AddAsync(Department department);
    void Remove(Department department);
    Task SaveChangesAsync();
}
