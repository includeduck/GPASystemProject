using System;

namespace GpaSystem.API.Models;

public class Notification
{
    public long NotificationId { get; set; }
    public int UserId { get; set; }
    public string Type { get; set; } = string.Empty; // EMAIL, IN_APP
    public string Subject { get; set; } = string.Empty;
    public string MessageBody { get; set; } = string.Empty;
    public string SentStatus { get; set; } = "PENDING"; // PENDING, SENT, FAILED
    public DateTime? SentAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual AppUser User { get; set; } = null!;
}
