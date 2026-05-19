using GpaSystem.API.DTOs;

namespace GpaSystem.API.Services;

public interface ICourseService
{
    Task<List<CourseResponse>> GetAllAsync();
    Task<CourseResponse> GetByIdAsync(int id);
    Task<CourseResponse> CreateAsync(CreateCourseRequest request);
    Task<CourseResponse> UpdateAsync(int id, UpdateCourseRequest request);
    Task DeleteAsync(int id);
}
