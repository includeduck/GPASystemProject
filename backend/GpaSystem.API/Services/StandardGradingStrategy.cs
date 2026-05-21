using System.Collections.Generic;
using System.Linq;
using GpaSystem.API.Models;

namespace GpaSystem.API.Services;

public class StandardGradingStrategy : IGradingStrategy
{
    public (string LetterGrade, decimal GradePoint) MapPercentage(decimal percentage, List<GradingPolicy> activePolicies, decimal passFailCutoff)
    {
        // 1. If percentage is strictly below the passFailCutoff, it is a fail.
        if (percentage < passFailCutoff)
        {
            var failPolicy = activePolicies.FirstOrDefault(p => p.GradePoint == 0)
                             ?? new GradingPolicy { LetterGrade = "F", GradePoint = 0 };
            return (failPolicy.LetterGrade, failPolicy.GradePoint);
        }

        // 2. Find matching policy from active grading policies where GP > 0.
        // We evaluate matching range using a half-open interval: [Min, Max)
        // Except for the absolute upper bound (100%), which is [Min, Max]
        var policy = activePolicies
            .Where(p => p.GradePoint > 0)
            .FirstOrDefault(p => percentage >= p.MinPercentage &&
                                (percentage < p.MaxPercentage || (percentage == 100m && p.MaxPercentage == 100m)));

        if (policy != null)
        {
            return (policy.LetterGrade, policy.GradePoint);
        }

        // 3. Fallback to the lowest active passing policy or F
        var fallback = activePolicies
            .Where(p => p.GradePoint > 0)
            .OrderBy(p => p.MinPercentage)
            .FirstOrDefault();

        if (fallback != null)
        {
            return (fallback.LetterGrade, fallback.GradePoint);
        }

        return ("F", 0m);
    }
}
