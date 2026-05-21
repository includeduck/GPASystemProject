using GpaSystem.API.DTOs;
using GpaSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GpaSystem.API.Controllers;

[ApiController]
[Route("api/courses/{courseId:int}/prerequisites")]
[Authorize(Roles = AuthRoles.Admin)]
public class PrerequisitesController : ControllerBase
{
    private readonly IPrerequisiteService _prerequisites;

    public PrerequisitesController(IPrerequisiteService prerequisites)
    {
        _prerequisites = prerequisites;
    }

    [HttpGet]
    public async Task<ActionResult<List<PrerequisiteResponse>>> GetForCourse(int courseId)
    {
        return Ok(await _prerequisites.GetForCourseAsync(courseId));
    }

    [HttpPost]
    public async Task<ActionResult<PrerequisiteResponse>> Add(int courseId, AddPrerequisiteRequest request)
    {
        var prerequisite = await _prerequisites.AddAsync(courseId, request);
        return CreatedAtAction(
            nameof(GetForCourse),
            new { courseId = prerequisite.CourseId },
            prerequisite);
    }

    [HttpDelete("{prerequisiteCourseId:int}")]
    public async Task<IActionResult> Remove(int courseId, int prerequisiteCourseId)
    {
        await _prerequisites.RemoveAsync(courseId, prerequisiteCourseId);
        return NoContent();
    }
}
