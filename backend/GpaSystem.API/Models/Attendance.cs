namespace GpaSystem.API.Models;

public class Attendance
{
    public int AttendanceId { get; set; }
    public int OfferingId { get; set; }
    public int StudentId { get; set; }
    public DateOnly AttendanceDate { get; set; }
    public string Status { get; set; } = string.Empty; // PRESENT, ABSENT, LATE
    public int RecordedBy { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual CourseOffering CourseOffering { get; set; } = null!;
    public virtual Student Student { get; set; } = null!;
    public virtual Instructor Instructor { get; set; } = null!;
}
