namespace GpaSystem.API.Models;

public class CourseGrade
{
    public int GradeId { get; set; }
    public int EnrollmentId { get; set; }
    public decimal TotalObtained { get; set; }
    public decimal MaxPossible { get; set; }
    public decimal Percentage { get; set; }
    public string LetterGrade { get; set; } = string.Empty;
    public decimal GradePoints { get; set; }
    public bool IsRepeatedAttempt { get; set; } = false;
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Enrollment Enrollment { get; set; } = null!;
}
