using GpaSystem.API.DTOs;
using GpaSystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace GpaSystem.API.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reports;
    private readonly IReportExportService _exports;

    public ReportsController(IReportService reports, IReportExportService exports)
    {
        _reports = reports;
        _exports = exports;
    }

    [HttpGet("transcript/{studentId:int}")]
    public async Task<ActionResult<TranscriptResponse>> GetTranscript(int studentId) =>
        Ok(await _reports.GetTranscriptAsync(studentId));

    [HttpGet("semester/{semesterId:int}")]
    public async Task<ActionResult<SemesterResultsReportResponse>> GetSemesterResults(int semesterId) =>
        Ok(await _reports.GetSemesterResultsAsync(semesterId));

    [HttpGet("course/{courseId:int}")]
    public async Task<ActionResult<CoursePerformanceReportResponse>> GetCoursePerformance(
        int courseId,
        [FromQuery] int? semesterId) =>
        Ok(await _reports.GetCoursePerformanceAsync(courseId, semesterId));

    [HttpGet("department/{departmentId:int}")]
    public async Task<ActionResult<DepartmentPerformanceReportResponse>> GetDepartmentPerformance(
        int departmentId,
        [FromQuery] int? semesterId) =>
        Ok(await _reports.GetDepartmentPerformanceAsync(departmentId, semesterId));

    [HttpGet("warnings")]
    public async Task<ActionResult<WarningListReportResponse>> GetWarnings(
        [FromQuery] int semesterId,
        [FromQuery] decimal? threshold) =>
        Ok(await _reports.GetWarningListAsync(semesterId, threshold));

    [HttpGet("rankings")]
    public async Task<ActionResult<ClassRankingsReportResponse>> GetRankings(
        [FromQuery] int? departmentId,
        [FromQuery] int? semesterId) =>
        Ok(await _reports.GetClassRankingsAsync(departmentId, semesterId));

    [HttpGet("transcript/{studentId:int}/export.csv")]
    public async Task<IActionResult> ExportTranscriptCsv(int studentId)
    {
        var file = await _exports.ExportTranscriptCsvAsync(studentId);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("transcript/{studentId:int}/export.pdf")]
    public async Task<IActionResult> ExportTranscriptPdf(int studentId)
    {
        var file = await _exports.ExportTranscriptPdfAsync(studentId);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("semester/{semesterId:int}/export.csv")]
    public async Task<IActionResult> ExportSemesterCsv(int semesterId)
    {
        var file = await _exports.ExportSemesterResultsCsvAsync(semesterId);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("warnings/export.csv")]
    public async Task<IActionResult> ExportWarningsCsv([FromQuery] int semesterId, [FromQuery] decimal? threshold)
    {
        var file = await _exports.ExportWarningsCsvAsync(semesterId, threshold);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("rankings/export.csv")]
    public async Task<IActionResult> ExportRankingsCsv([FromQuery] int? departmentId, [FromQuery] int? semesterId)
    {
        var file = await _exports.ExportRankingsCsvAsync(departmentId, semesterId);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("course/{courseId:int}/export.csv")]
    public async Task<IActionResult> ExportCourseCsv(int courseId, [FromQuery] int? semesterId)
    {
        var file = await _exports.ExportCoursePerformanceCsvAsync(courseId, semesterId);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("department/{departmentId:int}/export.csv")]
    public async Task<IActionResult> ExportDepartmentCsv(int departmentId, [FromQuery] int? semesterId)
    {
        var file = await _exports.ExportDepartmentPerformanceCsvAsync(departmentId, semesterId);
        return File(file.Content, file.ContentType, file.FileName);
    }
}
