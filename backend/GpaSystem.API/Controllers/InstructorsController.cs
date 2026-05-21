using GpaSystem.API.DTOs;
using GpaSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GpaSystem.API.Controllers;

[ApiController]
[Route("api/instructors")]
[Authorize(Roles = AuthRoles.Admin)]
public class InstructorsController : ControllerBase
{
    private readonly IInstructorService _instructors;

    public InstructorsController(IInstructorService instructors)
    {
        _instructors = instructors;
    }

    [HttpGet]
    public async Task<ActionResult<List<InstructorResponse>>> GetAll()
    {
        return Ok(await _instructors.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InstructorResponse>> GetById(int id)
    {
        return Ok(await _instructors.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<CreateInstructorResponse>> Create(CreateInstructorRequest request)
    {
        var response = await _instructors.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = response.Instructor.InstructorId }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<InstructorResponse>> Update(int id, UpdateInstructorRequest request)
    {
        return Ok(await _instructors.UpdateAsync(id, request));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _instructors.DeactivateAsync(id);
        return NoContent();
    }
}
