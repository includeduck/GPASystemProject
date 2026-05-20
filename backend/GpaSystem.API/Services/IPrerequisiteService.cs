using GpaSystem.API.DTOs;

namespace GpaSystem.API.Services;

public interface IPrerequisiteService
{
    Task<List<PrerequisiteResponse>> GetForCourseAsync(int courseId);
    Task<PrerequisiteResponse> AddAsync(int courseId, AddPrerequisiteRequest request);
    Task RemoveAsync(int courseId, int prerequisiteCourseId);
}
