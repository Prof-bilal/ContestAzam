namespace EventSphere.Api.Models;

public class Feedback
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public int StudentId { get; set; }

    public int Rating { get; set; } // 1 to 5

    public string? Comments { get; set; }

    public DateTime SubmittedOn { get; set; } = DateTime.UtcNow;

    // Navigation
    public Event Event { get; set; } = null!;

    public AppUser Student { get; set; } = null!;
}
