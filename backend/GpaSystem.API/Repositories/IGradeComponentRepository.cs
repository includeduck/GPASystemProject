using System.Collections.Generic;
using System.Threading.Tasks;
using GpaSystem.API.Models;

namespace GpaSystem.API.Repositories;

public interface IGradeComponentRepository
{
    Task<GradeComponent?> GetByIdAsync(int id);
    Task<List<GradeComponent>> GetByOfferingIdAsync(int offeringId);
    Task<bool> ExistsByNameAsync(int offeringId, string name, int? excludeId = null);
    Task AddAsync(GradeComponent component);
    void Remove(GradeComponent component);
    Task SaveChangesAsync();
}
