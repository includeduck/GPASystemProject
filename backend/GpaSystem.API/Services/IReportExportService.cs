namespace GpaSystem.API.Services;

public interface IReportExportService
{
    Task<ExportedFile> ExportTranscriptCsvAsync(int studentId);
    Task<ExportedFile> ExportTranscriptPdfAsync(int studentId);
    Task<ExportedFile> ExportSemesterResultsCsvAsync(int semesterId);
    Task<ExportedFile> ExportWarningsCsvAsync(int semesterId, decimal? threshold);
    Task<ExportedFile> ExportRankingsCsvAsync(int? departmentId, int? semesterId);
    Task<ExportedFile> ExportCoursePerformanceCsvAsync(int courseId, int? semesterId);
    Task<ExportedFile> ExportDepartmentPerformanceCsvAsync(int departmentId, int? semesterId);
}

public class ExportedFile
{
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}
