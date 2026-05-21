using GpaSystem.API.DTOs;
using GpaSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GpaSystem.API.Controllers;

[ApiController]
[Route("api/enrollments")]
[Authorize(Roles = AuthRoles.AdminOrStudent)]
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
        if (!CanAccessStudent(studentId))
        {
            return Forbid();
        }

        return Ok(await _enrollments.GetForStudentAsync(studentId));
    }

    [HttpGet("available")]
    public async Task<ActionResult<List<AvailableOfferingResponse>>> GetAvailableOfferings(
        [FromQuery] int studentId,
        [FromQuery] int? semesterId = null)
    {
        if (!CanAccessStudent(studentId))
        {
            return Forbid();
        }

        return Ok(await _enrollments.GetAvailableOfferingsAsync(studentId, semesterId));
    }

    [HttpPost]
    public async Task<ActionResult<EnrollmentResponse>> Enroll(CreateEnrollmentRequest request)
    {
        if (!CanAccessStudent(request.StudentId))
        {
            return Forbid();
        }

        var enrollment = await _enrollments.EnrollAsync(request);
        return CreatedAtAction(nameof(GetForStudent), new { studentId = enrollment.StudentId }, enrollment);
    }

    private bool CanAccessStudent(int studentId)
    {
        return User.IsAdmin() || User.GetStudentId() == studentId;
    }
}
