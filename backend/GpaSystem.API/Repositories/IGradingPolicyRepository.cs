using System.Collections.Generic;
using System.Threading.Tasks;
using GpaSystem.API.Models;

namespace GpaSystem.API.Repositories;

public interface IGradingPolicyRepository
{
    Task<List<GradingPolicy>> GetAllAsync();
    Task<List<GradingPolicy>> GetActivePoliciesAsync();
    Task AddAsync(GradingPolicy policy);
    void Remove(GradingPolicy policy);
    Task SaveChangesAsync();
}
