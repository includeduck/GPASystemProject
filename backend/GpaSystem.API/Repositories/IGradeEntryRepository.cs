using System.Collections.Generic;
using System.Threading.Tasks;
using GpaSystem.API.Models;

namespace GpaSystem.API.Repositories;

public interface IGradeEntryRepository
{
    Task<GradeEntry?> GetByIdAsync(int id);
    Task<List<GradeEntry>> GetByOfferingIdAsync(int offeringId);
    Task<List<GradeEntry>> GetByEnrollmentIdAsync(int enrollmentId);
    Task<GradeEntry?> GetSingleEntryAsync(int enrollmentId, int componentId);
    Task AddAsync(GradeEntry entry);
    void Remove(GradeEntry entry);
    Task SaveChangesAsync();
}
