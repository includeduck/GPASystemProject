namespace GpaSystem.API.Models;

public class Department
{
    public int DepartmentId { get; set; }
    public string DepartmentCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    public virtual ICollection<Instructor> Instructors { get; set; } = new List<Instructor>();
}
