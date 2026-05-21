using GpaSystem.API.Models;
using GpaSystem.API.Services;

namespace GpaSystem.API.Tests;

public class StandardGradingStrategyTests
{
    private static List<GradingPolicy> SamplePolicies() =>
    [
        new() { LetterGrade = "A", MinPercentage = 90, MaxPercentage = 100, GradePoint = 4.00m, IsActive = true },
        new() { LetterGrade = "B", MinPercentage = 80, MaxPercentage = 90, GradePoint = 3.00m, IsActive = true },
        new() { LetterGrade = "C", MinPercentage = 70, MaxPercentage = 80, GradePoint = 2.00m, IsActive = true },
        new() { LetterGrade = "F", MinPercentage = 0, MaxPercentage = 50, GradePoint = 0m, IsActive = true }
    ];

    [Theory]
    [InlineData(95, "A", 4.00)]
    [InlineData(90, "A", 4.00)]
    [InlineData(89.9, "B", 3.00)]
    [InlineData(80, "B", 3.00)]
    [InlineData(79.9, "C", 2.00)]
    [InlineData(100, "A", 4.00)]
    public void MapPercentage_AssignsExpectedLetterGrade(decimal percentage, string expectedLetter, decimal expectedGp)
    {
        var strategy = new StandardGradingStrategy();
        var (letter, gp) = strategy.MapPercentage(percentage, SamplePolicies(), passFailCutoff: 50m);

        Assert.Equal(expectedLetter, letter);
        Assert.Equal(expectedGp, gp);
    }

    [Fact]
    public void MapPercentage_BelowCutoff_ReturnsFailGrade()
    {
        var strategy = new StandardGradingStrategy();
        var (letter, gp) = strategy.MapPercentage(49.9m, SamplePolicies(), passFailCutoff: 50m);

        Assert.Equal("F", letter);
        Assert.Equal(0m, gp);
    }
}
