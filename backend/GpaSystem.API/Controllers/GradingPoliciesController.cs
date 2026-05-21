using System.Collections.Generic;
using System.Threading.Tasks;
using GpaSystem.API.DTOs;
using GpaSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GpaSystem.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = AuthRoles.Admin)]
public class GradingPoliciesController : ControllerBase
{
    private readonly IGradingPolicyService _policyService;

    public GradingPoliciesController(IGradingPolicyService policyService)
    {
        _policyService = policyService;
    }

    [HttpGet("grading-policies")]
    public async Task<ActionResult<List<GradingPolicyResponse>>> GetPolicies()
    {
        return Ok(await _policyService.GetPoliciesAsync());
    }

    [HttpPut("grading-policies")]
    public async Task<ActionResult<List<GradingPolicyResponse>>> UpdatePolicies(List<UpdateGradingPolicyRequest> requests)
    {
        var updated = await _policyService.UpdatePoliciesAsync(requests);
        return Ok(updated);
    }

    [HttpGet("configuration")]
    public async Task<ActionResult<object>> GetConfiguration()
    {
        var cutoff = await _policyService.GetPassingCutoffAsync();
        return Ok(new { pass_fail_cutoff = cutoff });
    }

    [HttpPut("configuration")]
    public async Task<IActionResult> UpdateConfiguration([FromBody] UpdateConfigurationRequest request)
    {
        await _policyService.UpdatePassingCutoffAsync(request.PassFailCutoff);
        return NoContent();
    }
}

public class UpdateConfigurationRequest
{
    public decimal PassFailCutoff { get; set; }
}
