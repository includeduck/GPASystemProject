using GpaSystem.API.Data;
using GpaSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GpaSystem.API.Repositories;

public class CourseOfferingRepository : ICourseOfferingRepository
{
    private readonly GpaSystemDbContext _db;

    public CourseOfferingRepository(GpaSystemDbContext db)
    {
        _db = db;
    }

    public Task<List<CourseOffering>> GetAllAsync(int? semesterId = null)
    {
        var query = IncludeDetails(_db.CourseOfferings.AsQueryable());

        if (semesterId.HasValue)
        {
            query = query.Where(o => o.SemesterId == semesterId.Value);
        }

        return query
            .OrderByDescending(o => o.Semester.IsCurrent)
            .ThenByDescending(o => o.Semester.StartDate)
            .ThenBy(o => o.Course.CourseCode)
            .ThenBy(o => o.Instructor.FullName)
            .ToListAsync();
    }

    public Task<CourseOffering?> GetByIdAsync(int id)
    {
        return IncludeDetails(_db.CourseOfferings.AsQueryable())
            .FirstOrDefaultAsync(o => o.OfferingId == id);
    }

    public Task<bool> OfferingKeyExistsAsync(
        int courseId,
        int semesterId,
        int instructorId,
        int? excludeOfferingId = null)
    {
        return _db.CourseOfferings.AnyAsync(o =>
            o.CourseId == courseId &&
            o.SemesterId == semesterId &&
            o.InstructorId == instructorId &&
            (!excludeOfferingId.HasValue || o.OfferingId != excludeOfferingId.Value));
    }

    public Task<int> CountActiveEnrollmentsAsync(int offeringId)
    {
        return _db.Enrollments.CountAsync(e => e.OfferingId == offeringId && e.Status == "ENROLLED");
    }

    public async Task ReconcileCurrentEnrollmentAsync(int offeringId)
    {
        var offering = await _db.CourseOfferings.FirstAsync(o => o.OfferingId == offeringId);
        offering.CurrentEnrollment = await CountActiveEnrollmentsAsync(offeringId);
    }

    public async Task<bool> HasReferencesAsync(int id)
    {
        return await _db.Enrollments.AnyAsync(e => e.OfferingId == id) ||
               await _db.GradeComponents.AnyAsync(gc => gc.OfferingId == id) ||
               await _db.Attendances.AnyAsync(a => a.OfferingId == id);
    }

    public Task AddAsync(CourseOffering offering)
    {
        return _db.CourseOfferings.AddAsync(offering).AsTask();
    }

    public void Remove(CourseOffering offering)
    {
        _db.CourseOfferings.Remove(offering);
    }

    public Task SaveChangesAsync()
    {
        return _db.SaveChangesAsync();
    }

    private static IQueryable<CourseOffering> IncludeDetails(IQueryable<CourseOffering> query)
    {
        return query
            .Include(o => o.Course)
                .ThenInclude(c => c.Department)
            .Include(o => o.Semester)
            .Include(o => o.Instructor)
                .ThenInclude(i => i.User)
            .Include(o => o.Instructor)
                .ThenInclude(i => i.Department);
    }
}
