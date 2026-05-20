using GpaSystem.API.Models;

namespace GpaSystem.API.Repositories;

public interface ICourseOfferingRepository
{
    Task<List<CourseOffering>> GetAllAsync(int? semesterId = null);
    Task<CourseOffering?> GetByIdAsync(int id);
    Task<bool> OfferingKeyExistsAsync(int courseId, int semesterId, int instructorId, int? excludeOfferingId = null);
    Task<int> CountActiveEnrollmentsAsync(int offeringId);
    Task ReconcileCurrentEnrollmentAsync(int offeringId);
    Task<bool> HasReferencesAsync(int id);
    Task AddAsync(CourseOffering offering);
    void Remove(CourseOffering offering);
    Task SaveChangesAsync();
}
