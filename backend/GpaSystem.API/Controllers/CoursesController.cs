using GpaSystem.API.DTOs;
using GpaSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GpaSystem.API.Controllers;

[ApiController]
[Route("api/courses")]
[Authorize(Roles = AuthRoles.Admin)]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courses;

    public CoursesController(ICourseService courses)
    {
        _courses = courses;
    }

    [HttpGet]
    public async Task<ActionResult<List<CourseResponse>>> GetAll()
    {
        return Ok(await _courses.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CourseResponse>> GetById(int id)
    {
        return Ok(await _courses.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<CourseResponse>> Create(CreateCourseRequest request)
    {
        var course = await _courses.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = course.CourseId }, course);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CourseResponse>> Update(int id, UpdateCourseRequest request)
    {
        return Ok(await _courses.UpdateAsync(id, request));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _courses.DeleteAsync(id);
        return NoContent();
    }
}
