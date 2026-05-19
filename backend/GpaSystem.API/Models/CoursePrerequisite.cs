namespace GpaSystem.API.Models;

public class CoursePrerequisite
{
    public int CourseId { get; set; }
    public int PrerequisiteCourseId { get; set; }

    // Navigation properties
    public virtual Course Course { get; set; } = null!;
    public virtual Course PrerequisiteCourse { get; set; } = null!;
}
