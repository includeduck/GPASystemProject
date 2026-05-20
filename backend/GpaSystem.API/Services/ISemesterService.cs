using GpaSystem.API.DTOs;

namespace GpaSystem.API.Services;

public interface ISemesterService
{
    Task<List<SemesterResponse>> GetAllAsync();
    Task<SemesterResponse> GetByIdAsync(int id);
    Task<SemesterResponse> CreateAsync(CreateSemesterRequest request);
    Task<SemesterResponse> UpdateAsync(int id, UpdateSemesterRequest request);
    Task<SemesterResponse> SetCurrentAsync(int id);
    Task DeleteAsync(int id);
}
