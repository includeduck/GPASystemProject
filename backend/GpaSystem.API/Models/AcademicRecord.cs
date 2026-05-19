namespace GpaSystem.API.Models;

public class AcademicRecord
{
    public int RecordId { get; set; }
    public int StudentId { get; set; }
    public int SemesterId { get; set; }
    public decimal SemesterGpa { get; set; }
    public decimal CumulativeGpa { get; set; }
    public int TotalCreditsAttempted { get; set; }
    public decimal TotalGradePoints { get; set; }
    public DateTime CalculationDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Student Student { get; set; } = null!;
    public virtual Semester Semester { get; set; } = null!;
}
