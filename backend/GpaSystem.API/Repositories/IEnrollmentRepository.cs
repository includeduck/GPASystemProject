using GpaSystem.API.Models;

namespace GpaSystem.API.Repositories;

public interface IEnrollmentRepository
{
    Task<List<Enrollment>> GetByStudentAsync(int studentId);
    Task<Enrollment?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(int studentId, int offeringId);
    Task AddAsync(Enrollment enrollment);
    Task SaveChangesAsync();
}
