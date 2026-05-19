namespace GpaSystem.API.Models;

public class Administrator
{
    public int AdminId { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;

    // Navigation properties
    public virtual AppUser User { get; set; } = null!;
}
