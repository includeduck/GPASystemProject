using GpaSystem.API.Models;

namespace GpaSystem.API.Repositories;

public interface ICoursePrerequisiteRepository
{
    Task<List<CoursePrerequisite>> GetForCourseAsync(int courseId);
    Task<CoursePrerequisite?> GetAsync(int courseId, int prerequisiteCourseId);
    Task<List<int>> GetPrerequisiteIdsAsync(int courseId);
    Task<bool> ExistsAsync(int courseId, int prerequisiteCourseId);
    Task AddAsync(CoursePrerequisite prerequisite);
    void Remove(CoursePrerequisite prerequisite);
    Task SaveChangesAsync();
}
