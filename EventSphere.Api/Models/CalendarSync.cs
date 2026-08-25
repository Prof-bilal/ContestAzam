namespace EventSphere.Api.Models;

public class CalendarSync
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int EventId { get; set; }

    public string CalendarType { get; set; } = string.Empty; // Google, Outlook, Apple

    public DateTime SyncTimestamp { get; set; } = DateTime.UtcNow;

    public string? CalendarUrl { get; set; }

    // Navigation
    public AppUser User { get; set; } = null!;

    public Event Event { get; set; } = null!;
}
