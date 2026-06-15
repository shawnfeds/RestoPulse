using System;

namespace RestoPulse.NotificationService.Domains.Entities;

public class Notification
{
    public int Id { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public DateTime Timestamp { get; private set; }
    public bool Read { get; private set; }
    public string ForRoles { get; private set; } = string.Empty; // Comma-separated roles, e.g., "Chef,Manager"
    public string? EntityId { get; private set; }

    // Required by EF Core
    private Notification() { }

    public static Notification Create(string type, string title, string message, string[] forRoles, string? entityId = null)
    {
        return new Notification
        {
            Type = type,
            Title = title,
            Message = message,
            Timestamp = DateTime.UtcNow,
            Read = false,
            ForRoles = string.Join(",", forRoles),
            EntityId = entityId
        };
    }

    public void MarkAsRead()
    {
        Read = true;
    }
}
