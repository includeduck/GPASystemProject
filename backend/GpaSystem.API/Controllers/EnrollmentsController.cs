using GpaSystem.API.DTOs;
using GpaSystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace GpaSystem.API.Controllers;

[ApiController]
[Route("api/enrollments")]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollments;

    public EnrollmentsController(IEnrollmentService enrollments)
    {
        _enrollments = enrollments;
    }

    [HttpGet]
    public async Task<ActionResult<List<EnrollmentResponse>>> GetForStudent([FromQuery] int studentId)
    {
        return Ok(await _enrollments.GetForStudentAsync(studentId));
    }

    [HttpGet("available")]
    public async Task<ActionResult<List<AvailableOfferingResponse>>> GetAvailableOfferings(
        [FromQuery] int studentId,
        [FromQuery] int? semesterId = null)
    {
        return Ok(await _enrollments.GetAvailableOfferingsAsync(studentId, semesterId));
    }

    [HttpPost]
    public async Task<ActionResult<EnrollmentResponse>> Enroll(CreateEnrollmentRequest request)
    {
        var enrollment = await _enrollments.EnrollAsync(request);
        return CreatedAtAction(nameof(GetForStudent), new { studentId = enrollment.StudentId }, enrollment);
    }
}
