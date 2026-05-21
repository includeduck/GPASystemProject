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
[Route("api/offerings/{offeringId:int}/components")]
[Authorize(Roles = AuthRoles.AdminOrInstructor)]
public class GradeComponentsController : ControllerBase
{
    private readonly IGradeService _gradeService;
    private readonly GpaSystemDbContext _db;

    public GradeComponentsController(IGradeService gradeService, GpaSystemDbContext db)
    {
        _gradeService = gradeService;
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<GradeComponentResponse>>> GetComponents(int offeringId)
    {
        if (!await CanAccessOfferingAsync(offeringId))
        {
            return Forbid();
        }

        return Ok(await _gradeService.GetComponentsAsync(offeringId));
    }

    [HttpPost]
    public async Task<ActionResult<GradeComponentResponse>> CreateComponent(int offeringId, CreateGradeComponentRequest request)
    {
        if (!await CanAccessOfferingAsync(offeringId))
        {
            return Forbid();
        }

        var component = await _gradeService.CreateComponentAsync(offeringId, request);
        return CreatedAtAction(nameof(GetComponents), new { offeringId }, component);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<GradeComponentResponse>> UpdateComponent(int offeringId, int id, UpdateGradeComponentRequest request)
    {
        if (!await CanAccessComponentAsync(offeringId, id))
        {
            return Forbid();
        }

        return Ok(await _gradeService.UpdateComponentAsync(id, request));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteComponent(int offeringId, int id)
    {
        if (!await CanAccessComponentAsync(offeringId, id))
        {
            return Forbid();
        }

        await _gradeService.DeleteComponentAsync(id);
        return NoContent();
    }

    private async Task<bool> CanAccessComponentAsync(int offeringId, int componentId)
    {
        var componentBelongsToOffering = await _db.GradeComponents
            .AnyAsync(c => c.ComponentId == componentId && c.OfferingId == offeringId);

        return componentBelongsToOffering && await CanAccessOfferingAsync(offeringId);
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
