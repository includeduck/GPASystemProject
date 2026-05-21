using GpaSystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace GpaSystem.API.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly DemoDataSeeder _seeder;
    private readonly IWebHostEnvironment _environment;

    public AdminController(DemoDataSeeder seeder, IWebHostEnvironment environment)
    {
        _seeder = seeder;
        _environment = environment;
    }

    [HttpPost("seed-demo")]
    public async Task<ActionResult<DemoSeedResult>> SeedDemo()
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        var result = await _seeder.SeedAsync();
        return Ok(result);
    }
}
