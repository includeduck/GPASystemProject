using GpaSystem.API.DTOs;
using GpaSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GpaSystem.API.Controllers;

[ApiController]
[Route("api/students")]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _students;
    private readonly IGpaCalculatorService _gpaCalculator;

    public StudentsController(IStudentService students, IGpaCalculatorService gpaCalculator)
    {
        _students = students;
        _gpaCalculator = gpaCalculator;
    }

    [HttpGet]
    [Authorize(Roles = AuthRoles.AdminOrInstructor)]
    public async Task<ActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? departmentId,
        [FromQuery] string? status,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDir,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        var hasQuery = !string.IsNullOrWhiteSpace(search)
            || departmentId.HasValue
            || !string.IsNullOrWhiteSpace(status)
            || page.HasValue
            || pageSize.HasValue
            || !string.IsNullOrWhiteSpace(sortBy)
            || !string.IsNullOrWhiteSpace(sortDir);

        if (!hasQuery)
        {
            return Ok(await _students.GetAllAsync());
        }

        var query = new StudentSearchQuery
        {
            Search = search,
            DepartmentId = departmentId,
            Status = status,
            SortBy = sortBy ?? "name",
            SortDir = sortDir ?? "asc",
            Page = page ?? 1,
            PageSize = pageSize ?? 25
        };

        return Ok(await _students.SearchAsync(query));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<StudentResponse>> GetById(int id)
    {
        if (User.IsInRole(AuthRoles.Student) && User.GetStudentId() != id)
        {
            return Forbid();
        }

        return Ok(await _students.GetByIdAsync(id));
    }

    [HttpPost]
    [Authorize(Roles = AuthRoles.Admin)]
    public async Task<ActionResult<CreateStudentResponse>> Create(CreateStudentRequest request)
    {
        var response = await _students.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = response.Student.StudentId }, response);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = AuthRoles.Admin)]
    public async Task<ActionResult<StudentResponse>> Update(int id, UpdateStudentRequest request)
    {
        return Ok(await _students.UpdateAsync(id, request));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AuthRoles.Admin)]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _students.DeactivateAsync(id);
        return NoContent();
    }

    [HttpGet("{id:int}/results")]
    [Authorize(Roles = AuthRoles.AdminOrStudent)]
    public async Task<ActionResult<StudentDashboardResponse>> GetResults(int id)
    {
        if (User.IsInRole(AuthRoles.Student) && User.GetStudentId() != id)
        {
            return Forbid();
        }

        return Ok(await _gpaCalculator.GetStudentDashboardAsync(id));
    }
}
