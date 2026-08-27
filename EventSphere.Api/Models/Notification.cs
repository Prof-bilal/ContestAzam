namespace EventSphere.Api.Models;

public class Notification
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Message { get; set; }

    /// <summary>System-controlled category; drives routing and rendering. Never trusted from React.</summary>
    public NotificationType Type { get; set; } = NotificationType.RegistrationConfirmed;

    /// <summary>Optional link/event/message this notification relates to (e.g. an Event Id or Conversation Id).</summary>
    public int? RelatedEntityId { get; set; }

    /// <summary>Optional string discriminator for RelatedEntityId (e.g. "Event", "Conversation").</summary>
    public string? RelatedEntityType { get; set; }

    /// <summary>Optional deep link the client can navigate to on click.</summary>
    public string? ActionUrl { get; set; }

    public bool IsRead { get; set; }

    // Email delivery status (optional channel; never blocks the primary operation).
    public bool EmailSent { get; set; }
    public DateTime? EmailSentAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReadAt { get; set; }

    // Navigation
    public AppUser User { get; set; } = null!;
}
