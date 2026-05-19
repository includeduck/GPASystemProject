using GpaSystem.API.DTOs;

namespace GpaSystem.API.Services;

public interface IInstructorService
{
    Task<List<InstructorResponse>> GetAllAsync();
    Task<InstructorResponse> GetByIdAsync(int id);
    Task<CreateInstructorResponse> CreateAsync(CreateInstructorRequest request);
    Task<InstructorResponse> UpdateAsync(int id, UpdateInstructorRequest request);
    Task DeactivateAsync(int id);
}
