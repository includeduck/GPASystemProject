using GpaSystem.API.DTOs;

namespace GpaSystem.API.Services;

public interface ICourseOfferingService
{
    Task<List<CourseOfferingResponse>> GetAllAsync(int? semesterId = null);
    Task<CourseOfferingResponse> GetByIdAsync(int id);
    Task<CourseOfferingResponse> CreateAsync(CreateCourseOfferingRequest request);
    Task<CourseOfferingResponse> UpdateAsync(int id, UpdateCourseOfferingRequest request);
    Task DeleteAsync(int id);
}
