using GpaSystem.API.DTOs;
using GpaSystem.API.Exceptions;
using GpaSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GpaSystem.API.Tests;

public class GradingPolicyServiceTests
{
    private static List<UpdateGradingPolicyRequest> StandardPolicyRequests() =>
    [
        new() { LetterGrade = "A", MinPercentage = 0, MaxPercentage = 60, GradePoint = 4.00m, IsActive = true, EffectiveFrom = DateOnly.FromDateTime(DateTime.Today) },
        new() { LetterGrade = "B", MinPercentage = 60, MaxPercentage = 80, GradePoint = 3.00m, IsActive = true, EffectiveFrom = DateOnly.FromDateTime(DateTime.Today) },
        new() { LetterGrade = "C", MinPercentage = 80, MaxPercentage = 100, GradePoint = 2.00m, IsActive = true, EffectiveFrom = DateOnly.FromDateTime(DateTime.Today) }
    ];

    [Fact]
    public async Task UpdatePoliciesAsync_RejectsEmptyList()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = ServiceFactory.CreateGradingPolicyService(database.Context);

        var ex = await Assert.ThrowsAsync<ApiException>(() => service.UpdatePoliciesAsync([]));
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task UpdatePoliciesAsync_RejectsNonContiguousRanges()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = ServiceFactory.CreateGradingPolicyService(database.Context);

        var requests = new List<UpdateGradingPolicyRequest>
        {
            new() { LetterGrade = "A", MinPercentage = 0, MaxPercentage = 50, GradePoint = 4.00m, IsActive = true, EffectiveFrom = DateOnly.FromDateTime(DateTime.Today) },
            new() { LetterGrade = "B", MinPercentage = 60, MaxPercentage = 100, GradePoint = 3.00m, IsActive = true, EffectiveFrom = DateOnly.FromDateTime(DateTime.Today) }
        };

        var ex = await Assert.ThrowsAsync<ApiException>(() => service.UpdatePoliciesAsync(requests));
        Assert.Equal(400, ex.StatusCode);
        Assert.Contains("contiguous", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdatePoliciesAsync_ReplacesPoliciesAtomically()
    {
        await using var database = await TestDatabase.CreateAsync();
        database.Context.GradingPolicies.Add(new GradingPolicy
        {
            LetterGrade = "Z",
            MinPercentage = 0,
            MaxPercentage = 100,
            GradePoint = 0,
            IsActive = true,
            EffectiveFrom = DateOnly.FromDateTime(DateTime.Today)
        });
        await database.Context.SaveChangesAsync();

        var service = ServiceFactory.CreateGradingPolicyService(database.Context);
        var result = await service.UpdatePoliciesAsync(StandardPolicyRequests());

        Assert.Equal(3, result.Count);
        Assert.Contains(result, p => p.LetterGrade == "A");
        Assert.DoesNotContain(await database.Context.GradingPolicies.Select(p => p.LetterGrade).ToListAsync(), g => g == "Z");
    }

    [Fact]
    public async Task GetPassingCutoffAsync_ReturnsConfiguredValue()
    {
        await using var database = await TestDatabase.CreateAsync();
        database.Context.Configurations.Add(new Configuration
        {
            ConfigKey = "pass_fail_cutoff",
            ConfigValue = "65",
            UpdatedAt = DateTime.UtcNow
        });
        await database.Context.SaveChangesAsync();

        var service = ServiceFactory.CreateGradingPolicyService(database.Context);
        var cutoff = await service.GetPassingCutoffAsync();

        Assert.Equal(65m, cutoff);
    }

    [Fact]
    public async Task GetPassingCutoffAsync_FallsBackTo50WhenMissing()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = ServiceFactory.CreateGradingPolicyService(database.Context);

        var cutoff = await service.GetPassingCutoffAsync();

        Assert.Equal(50m, cutoff);
    }

    [Fact]
    public async Task UpdatePassingCutoffAsync_RejectsOutOfRange()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = ServiceFactory.CreateGradingPolicyService(database.Context);

        var ex = await Assert.ThrowsAsync<ApiException>(() => service.UpdatePassingCutoffAsync(150m));
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task UpdatePassingCutoffAsync_PersistsValue()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service = ServiceFactory.CreateGradingPolicyService(database.Context);

        await service.UpdatePassingCutoffAsync(72.5m);

        var config = await database.Context.Configurations.FirstAsync(c => c.ConfigKey == "pass_fail_cutoff");
        Assert.Equal("72.50", config.ConfigValue);
        Assert.Equal(72.5m, await service.GetPassingCutoffAsync());
    }
}
