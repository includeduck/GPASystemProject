using GpaSystem.API.Models;

namespace GpaSystem.API.Repositories;

public interface ISemesterRepository
{
    Task<List<Semester>> GetAllAsync();
    Task<Semester?> GetByIdAsync(int id);
    Task<Semester?> GetCurrentAsync();
    Task<bool> HasReferencesAsync(int id);
    Task ClearCurrentAsync(int? excludeSemesterId = null);
    Task AddAsync(Semester semester);
    void Remove(Semester semester);
    Task SaveChangesAsync();
}
