using System.Collections.Generic;
using GpaSystem.API.Models;

namespace GpaSystem.API.Services;

public interface IGradingStrategy
{
    (string LetterGrade, decimal GradePoint) MapPercentage(decimal percentage, List<GradingPolicy> activePolicies, decimal passFailCutoff);
}
