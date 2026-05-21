using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GpaSystem.API.Data;
using GpaSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GpaSystem.API.Repositories;

public class GradingPolicyRepository : IGradingPolicyRepository
{
    private readonly GpaSystemDbContext _db;

    public GradingPolicyRepository(GpaSystemDbContext db)
    {
        _db = db;
    }

    public async Task<List<GradingPolicy>> GetAllAsync()
    {
        var list = await _db.GradingPolicies.ToListAsync();
        return list.OrderBy(p => p.MinPercentage).ToList();
    }

    public async Task<List<GradingPolicy>> GetActivePoliciesAsync()
    {
        var list = await _db.GradingPolicies.Where(p => p.IsActive).ToListAsync();
        return list.OrderBy(p => p.MinPercentage).ToList();
    }

    public Task AddAsync(GradingPolicy policy)
    {
        return _db.GradingPolicies.AddAsync(policy).AsTask();
    }

    public void Remove(GradingPolicy policy)
    {
        _db.GradingPolicies.Remove(policy);
    }

    public Task SaveChangesAsync()
    {
        return _db.SaveChangesAsync();
    }
}
