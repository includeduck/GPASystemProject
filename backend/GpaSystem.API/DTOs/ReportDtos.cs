namespace GpaSystem.API.DTOs;

public class TranscriptResponse
{
    public int StudentId { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string DepartmentCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public DateOnly EnrollmentDate { get; set; }
    public decimal CGPA { get; set; }
    public int TotalCreditsAttempted { get; set; }
    public int TotalCreditsEarned { get; set; }
    public DateTime GeneratedAt { get; set; }
    public List<SemesterResultResponse> Semesters { get; set; } = new();
    public List<StudentCourseGradeResponse> FailedCourses { get; set; } = new();
}

public class SemesterResultsReportResponse
{
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public List<SemesterStudentResultResponse> Students { get; set; } = new();
}

public class SemesterStudentResultResponse
{
    public int StudentId { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string DepartmentCode { get; set; } = string.Empty;
    public decimal SemesterGpa { get; set; }
    public decimal CumulativeGpa { get; set; }
    public int CreditsAttempted { get; set; }
    public List<StudentCourseGradeResponse> Courses { get; set; } = new();
}

public class CoursePerformanceReportResponse
{
    public int CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public int? SemesterId { get; set; }
    public string? SemesterName { get; set; }
    public int TotalEnrollments { get; set; }
    public int PassedCount { get; set; }
    public int FailedCount { get; set; }
    public decimal AveragePercentage { get; set; }
    public List<CourseOfferingPerformanceResponse> Offerings { get; set; } = new();
}

public class CourseOfferingPerformanceResponse
{
    public int OfferingId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    public int EnrollmentCount { get; set; }
    public decimal AveragePercentage { get; set; }
}

public class DepartmentPerformanceReportResponse
{
    public int DepartmentId { get; set; }
    public string DepartmentCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int? SemesterId { get; set; }
    public string? SemesterName { get; set; }
    public int StudentCount { get; set; }
    public decimal AverageSemesterGpa { get; set; }
    public decimal PassRate { get; set; }
    public List<DepartmentStudentSummaryResponse> Students { get; set; } = new();
}

public class DepartmentStudentSummaryResponse
{
    public int StudentId { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public decimal SemesterGpa { get; set; }
    public decimal CumulativeGpa { get; set; }
}

public class WarningListReportResponse
{
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public decimal Threshold { get; set; }
    public List<WarningStudentResponse> Students { get; set; } = new();
}

public class WarningStudentResponse
{
    public int StudentId { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string DepartmentCode { get; set; } = string.Empty;
    public decimal SemesterGpa { get; set; }
    public decimal CumulativeGpa { get; set; }
}

public class ClassRankingsReportResponse
{
    public int? SemesterId { get; set; }
    public string? SemesterName { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentCode { get; set; }
    public List<ClassRankingEntryResponse> Rankings { get; set; } = new();
}

public class ClassRankingEntryResponse
{
    public int Rank { get; set; }
    public int StudentId { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string DepartmentCode { get; set; } = string.Empty;
    public decimal Cgpa { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
}

public class StudentSearchQuery
{
    public string? Search { get; set; }
    public int? DepartmentId { get; set; }
    public string? Status { get; set; }
    public string SortBy { get; set; } = "name";
    public string SortDir { get; set; } = "asc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public class StudentListItemResponse
{
    public int StudentId { get; set; }
    public int UserId { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int DepartmentId { get; set; }
    public string DepartmentCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public DateOnly EnrollmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public decimal Cgpa { get; set; }
    public string? LatestSemesterName { get; set; }
}
