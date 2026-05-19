namespace GpaSystem.API.Models;

public class Instructor
{
    public int InstructorId { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public DateOnly HireDate { get; set; }

    // Navigation properties
    public virtual AppUser User { get; set; } = null!;
    public virtual Department Department { get; set; } = null!;
    public virtual ICollection<CourseOffering> CourseOfferings { get; set; } = new List<CourseOffering>();
    public virtual ICollection<GradeEntry> GradeEntries { get; set; } = new List<GradeEntry>();
    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}
