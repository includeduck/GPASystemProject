using GpaSystem.API.DTOs;
using GpaSystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace GpaSystem.API.Controllers;

[ApiController]
[Route("api/course-offerings")]
public class CourseOfferingsController : ControllerBase
{
    private readonly ICourseOfferingService _offerings;

    public CourseOfferingsController(ICourseOfferingService offerings)
    {
        _offerings = offerings;
    }

    [HttpGet]
    public async Task<ActionResult<List<CourseOfferingResponse>>> GetAll([FromQuery] int? semesterId = null)
    {
        return Ok(await _offerings.GetAllAsync(semesterId));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CourseOfferingResponse>> GetById(int id)
    {
        return Ok(await _offerings.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<CourseOfferingResponse>> Create(CreateCourseOfferingRequest request)
    {
        var offering = await _offerings.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = offering.OfferingId }, offering);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CourseOfferingResponse>> Update(int id, UpdateCourseOfferingRequest request)
    {
        return Ok(await _offerings.UpdateAsync(id, request));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _offerings.DeleteAsync(id);
        return NoContent();
    }
}
