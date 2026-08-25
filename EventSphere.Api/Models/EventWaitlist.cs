namespace EventSphere.Api.Models;

public class EventWaitlist
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int EventId { get; set; }

    public DateTime WaitlistTime { get; set; } = DateTime.UtcNow;

    public WaitlistStatus Status { get; set; } = WaitlistStatus.Waiting;

    // Navigation
    public AppUser User { get; set; } = null!;

    public Event Event { get; set; } = null!;
}
