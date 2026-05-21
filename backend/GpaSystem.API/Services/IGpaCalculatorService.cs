using System.Threading.Tasks;
using GpaSystem.API.DTOs;

namespace GpaSystem.API.Services;

public interface IGpaCalculatorService
{
    Task RecalculateStudentGpaAndCgpaAsync(int studentId);
    Task<StudentDashboardResponse> GetStudentDashboardAsync(int studentId);
}
