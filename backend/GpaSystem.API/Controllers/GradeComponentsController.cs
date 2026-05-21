using System.Collections.Generic;
using System.Threading.Tasks;
using GpaSystem.API.DTOs;
using GpaSystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace GpaSystem.API.Controllers;

[ApiController]
[Route("api/offerings/{offeringId:int}/components")]
public class GradeComponentsController : ControllerBase
{
    private readonly IGradeService _gradeService;

    public GradeComponentsController(IGradeService gradeService)
    {
        _gradeService = gradeService;
    }

    [HttpGet]
    public async Task<ActionResult<List<GradeComponentResponse>>> GetComponents(int offeringId)
    {
        return Ok(await _gradeService.GetComponentsAsync(offeringId));
    }

    [HttpPost]
    public async Task<ActionResult<GradeComponentResponse>> CreateComponent(int offeringId, CreateGradeComponentRequest request)
    {
        var component = await _gradeService.CreateComponentAsync(offeringId, request);
        return CreatedAtAction(nameof(GetComponents), new { offeringId }, component);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<GradeComponentResponse>> UpdateComponent(int offeringId, int id, UpdateGradeComponentRequest request)
    {
        return Ok(await _gradeService.UpdateComponentAsync(id, request));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteComponent(int offeringId, int id)
    {
        await _gradeService.DeleteComponentAsync(id);
        return NoContent();
    }
}
