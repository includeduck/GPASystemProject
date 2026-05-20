using GpaSystem.API.Data;
using GpaSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GpaSystem.API.Repositories;

public class SemesterRepository : ISemesterRepository
{
    private readonly GpaSystemDbContext _db;

    public SemesterRepository(GpaSystemDbContext db)
    {
        _db = db;
    }

    public Task<List<Semester>> GetAllAsync()
    {
        return _db.Semesters
            .OrderByDescending(s => s.IsCurrent)
            .ThenByDescending(s => s.StartDate)
            .ThenBy(s => s.SemesterName)
            .ToListAsync();
    }

    public Task<Semester?> GetByIdAsync(int id)
    {
        return _db.Semesters.FirstOrDefaultAsync(s => s.SemesterId == id);
    }

    public Task<Semester?> GetCurrentAsync()
    {
        return _db.Semesters.FirstOrDefaultAsync(s => s.IsCurrent);
    }

    public async Task<bool> HasReferencesAsync(int id)
    {
        return await _db.CourseOfferings.AnyAsync(co => co.SemesterId == id) ||
               await _db.AcademicRecords.AnyAsync(ar => ar.SemesterId == id);
    }

    public async Task ClearCurrentAsync(int? excludeSemesterId = null)
    {
        var currentSemesters = await _db.Semesters
            .Where(s => s.IsCurrent && (!excludeSemesterId.HasValue || s.SemesterId != excludeSemesterId.Value))
            .ToListAsync();

        foreach (var semester in currentSemesters)
        {
            semester.IsCurrent = false;
        }
    }

    public Task AddAsync(Semester semester)
    {
        return _db.Semesters.AddAsync(semester).AsTask();
    }

    public void Remove(Semester semester)
    {
        _db.Semesters.Remove(semester);
    }

    public Task SaveChangesAsync()
    {
        return _db.SaveChangesAsync();
    }
}
