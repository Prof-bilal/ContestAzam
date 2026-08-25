namespace EventSphere.Api.Models;

public class EventShareLog
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int EventId { get; set; }

    public string Platform { get; set; } = string.Empty; // Facebook, WhatsApp, etc.

    public DateTime ShareTimestamp { get; set; } = DateTime.UtcNow;

    public string? ShareMessage { get; set; }

    // Navigation
    public AppUser User { get; set; } = null!;

    public Event Event { get; set; } = null!;
}
