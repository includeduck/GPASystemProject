using GpaSystem.API.DTOs;
using GpaSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GpaSystem.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = AuthRoles.Admin)]
public class AdminController : ControllerBase
{
    private readonly DemoDataSeeder _seeder;
    private readonly IAuthService _auth;
    private readonly IWebHostEnvironment _environment;

    public AdminController(DemoDataSeeder seeder, IAuthService auth, IWebHostEnvironment environment)
    {
        _seeder = seeder;
        _auth = auth;
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

    [AllowAnonymous]
    [HttpPost("bootstrap-admin")]
    public async Task<ActionResult<CreateAdminResponse>> BootstrapAdmin(BootstrapAdminRequest? request)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        var result = await _auth.BootstrapAdminAsync(request, GetIpAddress());
        return Ok(result);
    }

    [HttpPost("users/{userId:int}/reset-password")]
    public async Task<ActionResult<TemporaryCredentialsResponse>> ResetPassword(int userId)
    {
        return Ok(await _auth.ResetPasswordAsync(userId, User.GetUserId(), GetIpAddress()));
    }

    private string? GetIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}
