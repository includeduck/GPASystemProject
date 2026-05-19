namespace GpaSystem.API.Models;

public class Student
{
    public int StudentId { get; set; }
    public int UserId { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int DepartmentId { get; set; }
    public DateOnly EnrollmentDate { get; set; }
    public string Status { get; set; } = "ACTIVE"; // ACTIVE, INACTIVE, GRADUATED

    // Navigation properties
    public virtual AppUser User { get; set; } = null!;
    public virtual Department Department { get; set; } = null!;
    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public virtual ICollection<AcademicRecord> AcademicRecords { get; set; } = new List<AcademicRecord>();
    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}
