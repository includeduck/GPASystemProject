namespace GpaSystem.API.Models;

public class GradeEntry
{
    public int GradeEntryId { get; set; }
    public int EnrollmentId { get; set; }
    public int ComponentId { get; set; }
    public decimal ObtainedMarks { get; set; }
    public int RecordedBy { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Enrollment Enrollment { get; set; } = null!;
    public virtual GradeComponent GradeComponent { get; set; } = null!;
    public virtual Instructor Instructor { get; set; } = null!;
}
