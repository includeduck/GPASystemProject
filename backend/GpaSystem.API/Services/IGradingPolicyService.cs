using System.Collections.Generic;
using System.Threading.Tasks;
using GpaSystem.API.DTOs;

namespace GpaSystem.API.Services;

public interface IGradingPolicyService
{
    Task<List<GradingPolicyResponse>> GetPoliciesAsync();
    Task<List<GradingPolicyResponse>> UpdatePoliciesAsync(List<UpdateGradingPolicyRequest> requests);
    Task<decimal> GetPassingCutoffAsync();
    Task UpdatePassingCutoffAsync(decimal cutoff);
}
