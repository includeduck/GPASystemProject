using GpaSystem.API.DTOs;

namespace GpaSystem.API.Services;

public interface IEnrollmentService
{
    Task<List<EnrollmentResponse>> GetForStudentAsync(int studentId);
    Task<List<AvailableOfferingResponse>> GetAvailableOfferingsAsync(int studentId, int? semesterId = null);
    Task<EnrollmentResponse> EnrollAsync(CreateEnrollmentRequest request);
}
