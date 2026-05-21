using GpaSystem.API.Data;
using GpaSystem.API.DTOs;
using GpaSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace GpaSystem.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reports;
    private readonly IReportExportService _exports;
    private readonly GpaSystemDbContext _db;

    public ReportsController(IReportService reports, IReportExportService exports, GpaSystemDbContext db)
    {
        _reports = reports;
        _exports = exports;
        _db = db;
    }

    [HttpGet("transcript/{studentId:int}")]
    [Authorize(Roles = AuthRoles.AdminOrStudent)]
    public async Task<ActionResult<TranscriptResponse>> GetTranscript(int studentId)
    {
        if (!CanAccessStudent(studentId))
        {
            return Forbid();
        }

        return Ok(await _reports.GetTranscriptAsync(studentId));
    }

    [HttpGet("semester/{semesterId:int}")]
    [Authorize(Roles = AuthRoles.Admin)]
    public async Task<ActionResult<SemesterResultsReportResponse>> GetSemesterResults(int semesterId) =>
        Ok(await _reports.GetSemesterResultsAsync(semesterId));

    [HttpGet("course/{courseId:int}")]
    [Authorize(Roles = AuthRoles.AdminOrInstructor)]
    public async Task<ActionResult<CoursePerformanceReportResponse>> GetCoursePerformance(
        int courseId,
        [FromQuery] int? semesterId)
    {
        if (!await CanAccessCoursePerformanceAsync(courseId, semesterId))
        {
            return Forbid();
        }

        return Ok(await _reports.GetCoursePerformanceAsync(
            courseId,
            semesterId,
            User.IsInRole(AuthRoles.Instructor) ? User.GetInstructorId() : null));
    }

    [HttpGet("department/{departmentId:int}")]
    [Authorize(Roles = AuthRoles.Admin)]
    public async Task<ActionResult<DepartmentPerformanceReportResponse>> GetDepartmentPerformance(
        int departmentId,
        [FromQuery] int? semesterId) =>
        Ok(await _reports.GetDepartmentPerformanceAsync(departmentId, semesterId));

    [HttpGet("warnings")]
    [Authorize(Roles = AuthRoles.Admin)]
    public async Task<ActionResult<WarningListReportResponse>> GetWarnings(
        [FromQuery] int semesterId,
        [FromQuery] decimal? threshold) =>
        Ok(await _reports.GetWarningListAsync(semesterId, threshold));

    [HttpGet("rankings")]
    [Authorize(Roles = AuthRoles.Admin)]
    public async Task<ActionResult<ClassRankingsReportResponse>> GetRankings(
        [FromQuery] int? departmentId,
        [FromQuery] int? semesterId) =>
        Ok(await _reports.GetClassRankingsAsync(departmentId, semesterId));

    [HttpGet("transcript/{studentId:int}/export.csv")]
    [Authorize(Roles = AuthRoles.AdminOrStudent)]
    public async Task<IActionResult> ExportTranscriptCsv(int studentId)
    {
        if (!CanAccessStudent(studentId))
        {
            return Forbid();
        }

        var file = await _exports.ExportTranscriptCsvAsync(studentId);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("transcript/{studentId:int}/export.pdf")]
    [Authorize(Roles = AuthRoles.AdminOrStudent)]
    public async Task<IActionResult> ExportTranscriptPdf(int studentId)
    {
        if (!CanAccessStudent(studentId))
        {
            return Forbid();
        }

        var file = await _exports.ExportTranscriptPdfAsync(studentId);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("semester/{semesterId:int}/export.csv")]
    [Authorize(Roles = AuthRoles.Admin)]
    public async Task<IActionResult> ExportSemesterCsv(int semesterId)
    {
        var file = await _exports.ExportSemesterResultsCsvAsync(semesterId);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("warnings/export.csv")]
    [Authorize(Roles = AuthRoles.Admin)]
    public async Task<IActionResult> ExportWarningsCsv([FromQuery] int semesterId, [FromQuery] decimal? threshold)
    {
        var file = await _exports.ExportWarningsCsvAsync(semesterId, threshold);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("rankings/export.csv")]
    [Authorize(Roles = AuthRoles.Admin)]
    public async Task<IActionResult> ExportRankingsCsv([FromQuery] int? departmentId, [FromQuery] int? semesterId)
    {
        var file = await _exports.ExportRankingsCsvAsync(departmentId, semesterId);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("course/{courseId:int}/export.csv")]
    [Authorize(Roles = AuthRoles.Admin)]
    public async Task<IActionResult> ExportCourseCsv(int courseId, [FromQuery] int? semesterId)
    {
        var file = await _exports.ExportCoursePerformanceCsvAsync(courseId, semesterId);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("department/{departmentId:int}/export.csv")]
    [Authorize(Roles = AuthRoles.Admin)]
    public async Task<IActionResult> ExportDepartmentCsv(int departmentId, [FromQuery] int? semesterId)
    {
        var file = await _exports.ExportDepartmentPerformanceCsvAsync(departmentId, semesterId);
        return File(file.Content, file.ContentType, file.FileName);
    }

    private bool CanAccessStudent(int studentId)
    {
        return User.IsAdmin() || User.GetStudentId() == studentId;
    }

    private async Task<bool> CanAccessCoursePerformanceAsync(int courseId, int? semesterId)
    {
        if (User.IsAdmin())
        {
            return true;
        }

        var instructorId = User.GetInstructorId();
        return instructorId.HasValue && await _db.CourseOfferings.AnyAsync(o =>
            o.CourseId == courseId &&
            o.InstructorId == instructorId.Value &&
            (!semesterId.HasValue || o.SemesterId == semesterId.Value));
    }
}
