namespace GpaSystem.API.Models;

public class Semester
{
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsCurrent { get; set; }

    // Navigation properties
    public virtual ICollection<CourseOffering> CourseOfferings { get; set; } = new List<CourseOffering>();
    public virtual ICollection<AcademicRecord> AcademicRecords { get; set; } = new List<AcademicRecord>();
}
