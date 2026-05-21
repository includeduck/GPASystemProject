using GpaSystem.API.DTOs;
using GpaSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GpaSystem.API.Controllers;

[ApiController]
[Route("api/semesters")]
[Authorize(Roles = AuthRoles.AdminOrInstructorOrStudent)]
public class SemestersController : ControllerBase
{
    private readonly ISemesterService _semesters;

    public SemestersController(ISemesterService semesters)
    {
        _semesters = semesters;
    }

    [HttpGet]
    public async Task<ActionResult<List<SemesterResponse>>> GetAll()
    {
        return Ok(await _semesters.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SemesterResponse>> GetById(int id)
    {
        return Ok(await _semesters.GetByIdAsync(id));
    }

    [HttpPost]
    [Authorize(Roles = AuthRoles.Admin)]
    public async Task<ActionResult<SemesterResponse>> Create(CreateSemesterRequest request)
    {
        var semester = await _semesters.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = semester.SemesterId }, semester);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = AuthRoles.Admin)]
    public async Task<ActionResult<SemesterResponse>> Update(int id, UpdateSemesterRequest request)
    {
        return Ok(await _semesters.UpdateAsync(id, request));
    }

    [HttpPut("{id:int}/current")]
    [Authorize(Roles = AuthRoles.Admin)]
    public async Task<ActionResult<SemesterResponse>> SetCurrent(int id)
    {
        return Ok(await _semesters.SetCurrentAsync(id));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AuthRoles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        await _semesters.DeleteAsync(id);
        return NoContent();
    }
}
