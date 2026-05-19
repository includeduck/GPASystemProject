using GpaSystem.API.Models;

namespace GpaSystem.API.Repositories;

public interface IInstructorRepository
{
    Task<List<Instructor>> GetAllAsync();
    Task<Instructor?> GetByIdAsync(int id);
    Task<bool> EmailExistsAsync(string email, int? excludeUserId = null);
    Task AddAsync(Instructor instructor);
    Task SaveChangesAsync();
}
