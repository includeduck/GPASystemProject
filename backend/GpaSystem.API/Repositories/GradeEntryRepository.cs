using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GpaSystem.API.Data;
using GpaSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GpaSystem.API.Repositories;

public class GradeEntryRepository : IGradeEntryRepository
{
    private readonly GpaSystemDbContext _db;

    public GradeEntryRepository(GpaSystemDbContext db)
    {
        _db = db;
    }

    public Task<GradeEntry?> GetByIdAsync(int id)
    {
        return _db.GradeEntries
            .Include(ge => ge.Enrollment)
            .Include(ge => ge.GradeComponent)
            .Include(ge => ge.Instructor)
            .FirstOrDefaultAsync(ge => ge.GradeEntryId == id);
    }

    public Task<List<GradeEntry>> GetByOfferingIdAsync(int offeringId)
    {
        return _db.GradeEntries
            .Where(ge => ge.Enrollment.OfferingId == offeringId)
            .Include(ge => ge.Enrollment)
                .ThenInclude(e => e.Student)
            .Include(ge => ge.GradeComponent)
            .Include(ge => ge.Instructor)
            .ToListAsync();
    }

    public Task<List<GradeEntry>> GetByEnrollmentIdAsync(int enrollmentId)
    {
        return _db.GradeEntries
            .Where(ge => ge.EnrollmentId == enrollmentId)
            .Include(ge => ge.GradeComponent)
            .Include(ge => ge.Instructor)
            .ToListAsync();
    }

    public Task<GradeEntry?> GetSingleEntryAsync(int enrollmentId, int componentId)
    {
        return _db.GradeEntries
            .FirstOrDefaultAsync(ge => ge.EnrollmentId == enrollmentId && ge.ComponentId == componentId);
    }

    public Task AddAsync(GradeEntry entry)
    {
        return _db.GradeEntries.AddAsync(entry).AsTask();
    }

    public void Remove(GradeEntry entry)
    {
        _db.GradeEntries.Remove(entry);
    }

    public Task SaveChangesAsync()
    {
        return _db.SaveChangesAsync();
    }
}
