namespace GpaSystem.API.Models;

public class AppUser
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // ADMIN, INSTRUCTOR, STUDENT
    public bool IsActive { get; set; } = true;
    public DateTime? LastLogin { get; set; }
    public DateTime PasswordChangedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Administrator? Administrator { get; set; }
    public virtual Instructor? Instructor { get; set; }
    public virtual Student? Student { get; set; }
}
