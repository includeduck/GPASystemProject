using GpaSystem.API.Data;
using GpaSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GpaSystem.API.Repositories;

public class CoursePrerequisiteRepository : ICoursePrerequisiteRepository
{
    private readonly GpaSystemDbContext _db;

    public CoursePrerequisiteRepository(GpaSystemDbContext db)
    {
        _db = db;
    }

    public Task<List<CoursePrerequisite>> GetForCourseAsync(int courseId)
    {
        return _db.CoursePrerequisites
            .Include(cp => cp.Course)
            .Include(cp => cp.PrerequisiteCourse)
            .Where(cp => cp.CourseId == courseId)
            .OrderBy(cp => cp.PrerequisiteCourse.CourseCode)
            .ToListAsync();
    }

    public Task<CoursePrerequisite?> GetAsync(int courseId, int prerequisiteCourseId)
    {
        return _db.CoursePrerequisites
            .Include(cp => cp.Course)
            .Include(cp => cp.PrerequisiteCourse)
            .FirstOrDefaultAsync(cp => cp.CourseId == courseId && cp.PrerequisiteCourseId == prerequisiteCourseId);
    }

    public Task<List<int>> GetPrerequisiteIdsAsync(int courseId)
    {
        return _db.CoursePrerequisites
            .Where(cp => cp.CourseId == courseId)
            .Select(cp => cp.PrerequisiteCourseId)
            .ToListAsync();
    }

    public Task<bool> ExistsAsync(int courseId, int prerequisiteCourseId)
    {
        return _db.CoursePrerequisites.AnyAsync(cp =>
            cp.CourseId == courseId && cp.PrerequisiteCourseId == prerequisiteCourseId);
    }

    public Task AddAsync(CoursePrerequisite prerequisite)
    {
        return _db.CoursePrerequisites.AddAsync(prerequisite).AsTask();
    }

    public void Remove(CoursePrerequisite prerequisite)
    {
        _db.CoursePrerequisites.Remove(prerequisite);
    }

    public Task SaveChangesAsync()
    {
        return _db.SaveChangesAsync();
    }
}
