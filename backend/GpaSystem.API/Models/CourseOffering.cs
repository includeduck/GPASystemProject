namespace GpaSystem.API.Models;

public class CourseOffering
{
    public int OfferingId { get; set; }
    public int CourseId { get; set; }
    public int SemesterId { get; set; }
    public int InstructorId { get; set; }
    public int MaxCapacity { get; set; }
    public int CurrentEnrollment { get; set; } = 0;
    public bool IsGradeFinalized { get; set; } = false;
    public string Status { get; set; } = "ACTIVE"; // ACTIVE, COMPLETED, CANCELLED

    // Navigation properties
    public virtual Course Course { get; set; } = null!;
    public virtual Semester Semester { get; set; } = null!;
    public virtual Instructor Instructor { get; set; } = null!;
    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public virtual ICollection<GradeComponent> GradeComponents { get; set; } = new List<GradeComponent>();
    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}
