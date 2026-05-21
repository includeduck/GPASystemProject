using System;

namespace GpaSystem.API.Models;

public class AuditLog
{
    public long LogId { get; set; }
    public int UserId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string? TableName { get; set; }
    public int? RecordId { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? IpAddress { get; set; }
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

    public virtual AppUser User { get; set; } = null!;
}
