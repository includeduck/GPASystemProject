namespace GpaSystem.API.Models;

public class GradingPolicy
{
    public int PolicyId { get; set; }
    public string LetterGrade { get; set; } = string.Empty;
    public decimal MinPercentage { get; set; }
    public decimal MaxPercentage { get; set; }
    public decimal GradePoint { get; set; }
    public bool IsActive { get; set; } = true;
    public DateOnly EffectiveFrom { get; set; }
}
