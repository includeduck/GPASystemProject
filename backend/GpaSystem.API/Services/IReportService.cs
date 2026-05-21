using GpaSystem.API.DTOs;

namespace GpaSystem.API.Services;

public interface IReportService
{
    Task<TranscriptResponse> GetTranscriptAsync(int studentId);
    Task<StudentDashboardResponse> GetStudentDashboardAsync(int studentId);
    Task<SemesterResultsReportResponse> GetSemesterResultsAsync(int semesterId);
    Task<CoursePerformanceReportResponse> GetCoursePerformanceAsync(int courseId, int? semesterId = null, int? instructorId = null);
    Task<DepartmentPerformanceReportResponse> GetDepartmentPerformanceAsync(int departmentId, int? semesterId = null);
    Task<WarningListReportResponse> GetWarningListAsync(int semesterId, decimal? threshold = null);
    Task<ClassRankingsReportResponse> GetClassRankingsAsync(int? departmentId = null, int? semesterId = null);
    Task<decimal> GetWarningGpaThresholdAsync();
}
