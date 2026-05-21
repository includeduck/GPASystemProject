using GpaSystem.API.DTOs;
using GpaSystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace GpaSystem.API.Controllers;

[ApiController]
[Route("api/students")]
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
    public async Task<ActionResult<List<StudentResponse>>> GetAll()
    {
        return Ok(await _students.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<StudentResponse>> GetById(int id)
    {
        return Ok(await _students.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<CreateStudentResponse>> Create(CreateStudentRequest request)
    {
        var response = await _students.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = response.Student.StudentId }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<StudentResponse>> Update(int id, UpdateStudentRequest request)
    {
        return Ok(await _students.UpdateAsync(id, request));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _students.DeactivateAsync(id);
        return NoContent();
    }

    [HttpGet("{id:int}/results")]
    public async Task<ActionResult<StudentDashboardResponse>> GetResults(int id)
    {
        return Ok(await _gpaCalculator.GetStudentDashboardAsync(id));
    }
}
