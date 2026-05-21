using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GpaSystem.API.Data;
using GpaSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GpaSystem.API.Repositories;

public class GradeComponentRepository : IGradeComponentRepository
{
    private readonly GpaSystemDbContext _db;

    public GradeComponentRepository(GpaSystemDbContext db)
    {
        _db = db;
    }

    public Task<GradeComponent?> GetByIdAsync(int id)
    {
        return _db.GradeComponents
            .Include(gc => gc.CourseOffering)
            .FirstOrDefaultAsync(gc => gc.ComponentId == id);
    }

    public Task<List<GradeComponent>> GetByOfferingIdAsync(int offeringId)
    {
        return _db.GradeComponents
            .Where(gc => gc.OfferingId == offeringId)
            .OrderBy(gc => gc.SortOrder)
            .ThenBy(gc => gc.ComponentName)
            .ToListAsync();
    }

    public Task<bool> ExistsByNameAsync(int offeringId, string name, int? excludeId = null)
    {
        var normalized = name.Trim().ToLower();
        return _db.GradeComponents.AnyAsync(gc =>
            gc.OfferingId == offeringId &&
            gc.ComponentName.Trim().ToLower() == normalized &&
            (!excludeId.HasValue || gc.ComponentId != excludeId.Value));
    }

    public Task AddAsync(GradeComponent component)
    {
        return _db.GradeComponents.AddAsync(component).AsTask();
    }

    public void Remove(GradeComponent component)
    {
        _db.GradeComponents.Remove(component);
    }

    public Task SaveChangesAsync()
    {
        return _db.SaveChangesAsync();
    }
}
