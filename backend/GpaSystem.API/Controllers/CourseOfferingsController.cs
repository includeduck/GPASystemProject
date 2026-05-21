using GpaSystem.API.Data;
using GpaSystem.API.DTOs;
using GpaSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace GpaSystem.API.Controllers;

[ApiController]
[Route("api/course-offerings")]
[Authorize(Roles = AuthRoles.AdminOrInstructor)]
public class CourseOfferingsController : ControllerBase
{
    private readonly ICourseOfferingService _offerings;
    private readonly GpaSystemDbContext _db;

    public CourseOfferingsController(ICourseOfferingService offerings, GpaSystemDbContext db)
    {
        _offerings = offerings;
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<CourseOfferingResponse>>> GetAll([FromQuery] int? semesterId = null)
    {
        var offerings = await _offerings.GetAllAsync(semesterId);
        if (User.IsInRole(AuthRoles.Instructor))
        {
            var instructorId = User.GetInstructorId();
            offerings = offerings.Where(o => o.InstructorId == instructorId).ToList();
        }

        return Ok(offerings);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CourseOfferingResponse>> GetById(int id)
    {
        if (!await CanAccessOfferingAsync(id))
        {
            return Forbid();
        }

        return Ok(await _offerings.GetByIdAsync(id));
    }

    [HttpPost]
    [Authorize(Roles = AuthRoles.Admin)]
    public async Task<ActionResult<CourseOfferingResponse>> Create(CreateCourseOfferingRequest request)
    {
        var offering = await _offerings.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = offering.OfferingId }, offering);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = AuthRoles.Admin)]
    public async Task<ActionResult<CourseOfferingResponse>> Update(int id, UpdateCourseOfferingRequest request)
    {
        return Ok(await _offerings.UpdateAsync(id, request));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AuthRoles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        await _offerings.DeleteAsync(id);
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
