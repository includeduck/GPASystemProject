namespace GpaSystem.API.Models;

public class Course
{
    public int CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public byte CreditHours { get; set; }
    public int DepartmentId { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Department Department { get; set; } = null!;
    public virtual ICollection<CourseOffering> CourseOfferings { get; set; } = new List<CourseOffering>();
    public virtual ICollection<CoursePrerequisite> Prerequisites { get; set; } = new List<CoursePrerequisite>();
    public virtual ICollection<CoursePrerequisite> DependentCourses { get; set; } = new List<CoursePrerequisite>();
}
