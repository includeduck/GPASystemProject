using GpaSystem.API.Data;
using GpaSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GpaSystem.API.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly GpaSystemDbContext _db;

    public CourseRepository(GpaSystemDbContext db)
    {
        _db = db;
    }

    public Task<List<Course>> GetAllAsync()
    {
        return _db.Courses
            .Include(c => c.Department)
            .OrderBy(c => c.CourseCode)
            .ToListAsync();
    }

    public Task<Course?> GetByIdAsync(int id)
    {
        return _db.Courses
            .Include(c => c.Department)
            .FirstOrDefaultAsync(c => c.CourseId == id);
    }

    public Task<bool> CodeExistsAsync(string courseCode, int? excludeCourseId = null)
    {
        var normalized = courseCode.Trim().ToUpper();
        return _db.Courses.AnyAsync(c =>
            c.CourseCode.ToUpper() == normalized &&
            (!excludeCourseId.HasValue || c.CourseId != excludeCourseId.Value));
    }

    public Task<int> CountByCourseCodePrefixAsync(string prefix)
    {
        return _db.Courses.CountAsync(c => c.CourseCode.StartsWith(prefix));
    }

    public async Task<bool> HasReferencesAsync(int id)
    {
        return await _db.CourseOfferings.AnyAsync(co => co.CourseId == id) ||
               await _db.CoursePrerequisites.AnyAsync(cp => cp.CourseId == id || cp.PrerequisiteCourseId == id);
    }

    public Task AddAsync(Course course)
    {
        return _db.Courses.AddAsync(course).AsTask();
    }

    public void Remove(Course course)
    {
        _db.Courses.Remove(course);
    }

    public Task SaveChangesAsync()
    {
        return _db.SaveChangesAsync();
    }
}
