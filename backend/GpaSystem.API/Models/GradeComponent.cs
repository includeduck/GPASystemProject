namespace GpaSystem.API.Models;

public class GradeComponent
{
    public int ComponentId { get; set; }
    public int OfferingId { get; set; }
    public string ComponentName { get; set; } = string.Empty;
    public decimal MaxPoints { get; set; }
    public byte SortOrder { get; set; } = 0;

    // Navigation properties
    public virtual CourseOffering CourseOffering { get; set; } = null!;
    public virtual ICollection<GradeEntry> GradeEntries { get; set; } = new List<GradeEntry>();
}
