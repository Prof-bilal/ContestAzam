namespace EventSphere.Api.Models;

public class Favorite
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int EventId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public AppUser User { get; set; } = null!;

    public Event Event { get; set; } = null!;
}
