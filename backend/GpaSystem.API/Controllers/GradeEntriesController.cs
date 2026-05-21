using System.Collections.Generic;
using System.Threading.Tasks;
using GpaSystem.API.Data;
using GpaSystem.API.DTOs;
using GpaSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace GpaSystem.API.Controllers;

[ApiController]
[Route("api/offerings/{offeringId:int}")]
[Authorize(Roles = AuthRoles.AdminOrInstructor)]
public class GradeEntriesController : ControllerBase
{
    private readonly IGradeService _gradeService;
    private readonly GpaSystemDbContext _db;

    public GradeEntriesController(IGradeService gradeService, GpaSystemDbContext db)
    {
        _gradeService = gradeService;
        _db = db;
    }

    [HttpGet("gradebook")]
    public async Task<ActionResult<List<RosterGradeResponse>>> GetGradebook(int offeringId)
    {
        if (!await CanAccessOfferingAsync(offeringId))
        {
            return Forbid();
        }

        var roster = await _gradeService.GetGradebookRosterAsync(offeringId);
        return Ok(roster);
    }

    [HttpPost("marks")]
    [Authorize(Roles = AuthRoles.Instructor)]
    public async Task<IActionResult> RecordMarks(
        int offeringId,
        [FromBody] List<RecordGradeEntryRequest> requests)
    {
        var instructorId = User.GetInstructorId();
        if (!instructorId.HasValue || !await CanAccessOfferingAsync(offeringId))
        {
            return Forbid();
        }

        await _gradeService.RecordMarksAsync(offeringId, requests, instructorId.Value);
        return NoContent();
    }

    [HttpPost("finalize")]
    [Authorize(Roles = AuthRoles.Instructor)]
    public async Task<IActionResult> FinalizeGrades(
        int offeringId,
        [FromBody] FinalizeGradesRequest request)
    {
        var instructorId = User.GetInstructorId();
        if (!instructorId.HasValue || !await CanAccessOfferingAsync(offeringId))
        {
            return Forbid();
        }

        await _gradeService.FinalizeGradesAsync(offeringId, request.Force, instructorId.Value);
        return NoContent();
    }

    private async Task<bool> CanAccessOfferingAsync(int offeringId)
    {
        if (User.IsAdmin())
        {
            return true;
        }

        var instructorId = User.GetInstructorId();
        return instructorId.HasValue && await _db.CourseOfferings
            .AnyAsync(o => o.OfferingId == offeringId && o.InstructorId == instructorId.Value);
    }
}
