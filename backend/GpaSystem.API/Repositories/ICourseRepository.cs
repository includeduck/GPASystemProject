using GpaSystem.API.Models;

namespace GpaSystem.API.Repositories;

public interface ICourseRepository
{
    Task<List<Course>> GetAllAsync();
    Task<Course?> GetByIdAsync(int id);
    Task<bool> CodeExistsAsync(string courseCode, int? excludeCourseId = null);
    Task<int> CountByCourseCodePrefixAsync(string prefix);
    Task<bool> HasReferencesAsync(int id);
    Task AddAsync(Course course);
    void Remove(Course course);
    Task SaveChangesAsync();
}
