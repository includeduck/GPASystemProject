using System.Collections.Generic;
using System.Threading.Tasks;
using GpaSystem.API.DTOs;
using GpaSystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace GpaSystem.API.Controllers;

[ApiController]
[Route("api/offerings/{offeringId:int}")]
public class GradeEntriesController : ControllerBase
{
    private readonly IGradeService _gradeService;

    public GradeEntriesController(IGradeService gradeService)
    {
        _gradeService = gradeService;
    }

    [HttpGet("gradebook")]
    public async Task<ActionResult<List<RosterGradeResponse>>> GetGradebook(int offeringId)
    {
        var roster = await _gradeService.GetGradebookRosterAsync(offeringId);
        return Ok(roster);
    }

    [HttpPost("marks")]
    public async Task<IActionResult> RecordMarks(
        int offeringId,
        [FromBody] List<RecordGradeEntryRequest> requests,
        [FromHeader(Name = "X-Instructor-Id")] int? headerInstructorId,
        [FromQuery] int? queryInstructorId)
    {
        int instructorId = headerInstructorId ?? queryInstructorId ?? 1;
        await _gradeService.RecordMarksAsync(offeringId, requests, instructorId);
        return NoContent();
    }

    [HttpPost("finalize")]
    public async Task<IActionResult> FinalizeGrades(
        int offeringId,
        [FromBody] FinalizeGradesRequest request,
        [FromHeader(Name = "X-Instructor-Id")] int? headerInstructorId,
        [FromQuery] int? queryInstructorId)
    {
        int instructorId = headerInstructorId ?? queryInstructorId ?? 1;
        await _gradeService.FinalizeGradesAsync(offeringId, request.Force, instructorId);
        return NoContent();
    }
}
