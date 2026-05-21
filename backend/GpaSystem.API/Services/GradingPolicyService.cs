using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GpaSystem.API.Data;
using GpaSystem.API.DTOs;
using GpaSystem.API.Exceptions;
using GpaSystem.API.Models;
using GpaSystem.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GpaSystem.API.Services;

public class GradingPolicyService : IGradingPolicyService
{
    private readonly GpaSystemDbContext _db;
    private readonly IGradingPolicyRepository _repository;

    public GradingPolicyService(
        GpaSystemDbContext db,
        IGradingPolicyRepository repository)
    {
        _db = db;
        _repository = repository;
    }

    public async Task<List<GradingPolicyResponse>> GetPoliciesAsync()
    {
        var list = await _repository.GetAllAsync();
        return list.Select(Map).ToList();
    }

    public async Task<List<GradingPolicyResponse>> UpdatePoliciesAsync(List<UpdateGradingPolicyRequest> requests)
    {
        if (requests == null || requests.Count == 0)
        {
            throw ApiException.BadRequest("Grading policies list cannot be empty.");
        }

        // 1. Sort the incoming list by MinPercentage ascending
        var sorted = requests
            .OrderBy(r => r.MinPercentage)
            .ToList();

        // 2. Perform validations
        // - Enforce range bounds [0, 100]
        // - MinPercentage <= MaxPercentage
        // - Contiguous: first Min must be 0, last Max must be 100, gaps/overlaps checked
        for (int i = 0; i < sorted.Count; i++)
        {
            var req = sorted[i];

            if (req.MinPercentage < 0 || req.MaxPercentage > 100)
            {
                throw ApiException.BadRequest($"Policy grade '{req.LetterGrade}' percentage boundaries [{req.MinPercentage}, {req.MaxPercentage}] must fall within [0, 100].");
            }

            if (req.MinPercentage >= req.MaxPercentage)
            {
                throw ApiException.BadRequest($"Policy grade '{req.LetterGrade}' minimum percentage ({req.MinPercentage}) must be strictly less than maximum percentage ({req.MaxPercentage}).");
            }

            if (req.GradePoint < 0 || req.GradePoint > 4.33m)
            {
                throw ApiException.BadRequest($"Policy grade '{req.LetterGrade}' grade point ({req.GradePoint}) must be between 0.00 and 4.33.");
            }

            if (string.IsNullOrWhiteSpace(req.LetterGrade))
            {
                throw ApiException.BadRequest("Letter grade label is required.");
            }
        }

        if (sorted[0].MinPercentage != 0m)
        {
            throw ApiException.BadRequest($"The lowest policy grade must start at 0% (currently starts at {sorted[0].MinPercentage}%).");
        }

        if (sorted[^1].MaxPercentage != 100m)
        {
            throw ApiException.BadRequest($"The highest policy grade must end at 100% (currently ends at {sorted[^1].MaxPercentage}%).");
        }

        for (int i = 0; i < sorted.Count - 1; i++)
        {
            var current = sorted[i];
            var next = sorted[i + 1];

            if (current.MaxPercentage != next.MinPercentage)
            {
                throw ApiException.BadRequest($"Ranges must be contiguous and non-overlapping. Gap/Overlap detected between '{current.LetterGrade}' (ends at {current.MaxPercentage}%) and '{next.LetterGrade}' (starts at {next.MinPercentage}%).");
            }
        }

        // 3. Perform bulk update inside a transaction to ensure atomic execution
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            // Remove all existing policies
            var existing = await _db.GradingPolicies.ToListAsync();
            _db.GradingPolicies.RemoveRange(existing);
            await _db.SaveChangesAsync();

            // Insert new policies
            var newPolicies = sorted.Select(req => new GradingPolicy
            {
                LetterGrade = req.LetterGrade.Trim().ToUpperInvariant(),
                MinPercentage = req.MinPercentage,
                MaxPercentage = req.MaxPercentage,
                GradePoint = req.GradePoint,
                IsActive = req.IsActive,
                EffectiveFrom = req.EffectiveFrom
            }).ToList();

            foreach (var policy in newPolicies)
            {
                await _repository.AddAsync(policy);
            }

            await _repository.SaveChangesAsync();
            await transaction.CommitAsync();

            return newPolicies.Select(Map).ToList();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<decimal> GetPassingCutoffAsync()
    {
        var config = await _db.Configurations.FirstOrDefaultAsync(c => c.ConfigKey == "pass_fail_cutoff");
        if (config != null && decimal.TryParse(config.ConfigValue, out var cutoff))
        {
            return cutoff;
        }

        return 50m; // Fallback default
    }

    public async Task UpdatePassingCutoffAsync(decimal cutoff)
    {
        if (cutoff < 0 || cutoff > 100)
        {
            throw ApiException.BadRequest("Passing cutoff must be between 0 and 100.");
        }

        var config = await _db.Configurations.FirstOrDefaultAsync(c => c.ConfigKey == "pass_fail_cutoff");
        if (config == null)
        {
            config = new Configuration
            {
                ConfigKey = "pass_fail_cutoff",
                ConfigValue = cutoff.ToString("F2"),
                Description = "Minimum percentage to pass a course",
                UpdatedAt = DateTime.UtcNow
            };
            await _db.Configurations.AddAsync(config);
        }
        else
        {
            config.ConfigValue = cutoff.ToString("F2");
            config.UpdatedAt = DateTime.UtcNow;
            _db.Entry(config).State = EntityState.Modified;
        }

        await _db.SaveChangesAsync();
    }

    private static GradingPolicyResponse Map(GradingPolicy policy)
    {
        return new GradingPolicyResponse
        {
            PolicyId = policy.PolicyId,
            LetterGrade = policy.LetterGrade,
            MinPercentage = policy.MinPercentage,
            MaxPercentage = policy.MaxPercentage,
            GradePoint = policy.GradePoint,
            IsActive = policy.IsActive,
            EffectiveFrom = policy.EffectiveFrom
        };
    }
}
