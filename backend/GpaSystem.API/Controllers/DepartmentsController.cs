using GpaSystem.API.DTOs;
using GpaSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GpaSystem.API.Controllers;

[ApiController]
[Route("api/departments")]
[Authorize(Roles = AuthRoles.Admin)]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departments;

    public DepartmentsController(IDepartmentService departments)
    {
        _departments = departments;
    }

    [HttpGet]
    public async Task<ActionResult<List<DepartmentResponse>>> GetAll()
    {
        return Ok(await _departments.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DepartmentResponse>> GetById(int id)
    {
        return Ok(await _departments.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<DepartmentResponse>> Create(CreateDepartmentRequest request)
    {
        var department = await _departments.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = department.DepartmentId }, department);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<DepartmentResponse>> Update(int id, UpdateDepartmentRequest request)
    {
        return Ok(await _departments.UpdateAsync(id, request));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _departments.DeleteAsync(id);
        return NoContent();
    }
}
