using GpaSystem.API.DTOs;

namespace GpaSystem.API.Services;

public interface IStudentService
{
    Task<List<StudentResponse>> GetAllAsync();
    Task<PagedResult<StudentListItemResponse>> SearchAsync(StudentSearchQuery query);
    Task<StudentResponse> GetByIdAsync(int id);
    Task<CreateStudentResponse> CreateAsync(CreateStudentRequest request);
    Task<StudentResponse> UpdateAsync(int id, UpdateStudentRequest request);
    Task DeactivateAsync(int id);
}
