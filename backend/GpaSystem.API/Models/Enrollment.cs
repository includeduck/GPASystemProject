namespace GpaSystem.API.Models;

public class Enrollment
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public int OfferingId { get; set; }
    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "ENROLLED"; // ENROLLED, DROPPED, COMPLETED
    public bool IsRepeated { get; set; } = false;
    public int? OriginalEnrollmentId { get; set; }

    // Navigation properties
    public virtual Student Student { get; set; } = null!;
    public virtual CourseOffering CourseOffering { get; set; } = null!;
    public virtual Enrollment? OriginalEnrollment { get; set; }
    public virtual ICollection<Enrollment> RepeatedEnrollments { get; set; } = new List<Enrollment>();
    public virtual ICollection<GradeEntry> GradeEntries { get; set; } = new List<GradeEntry>();
    public virtual CourseGrade? CourseGrade { get; set; }
}
