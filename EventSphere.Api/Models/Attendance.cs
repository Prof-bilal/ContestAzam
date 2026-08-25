namespace EventSphere.Api.Models;

public class Attendance
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public int StudentId { get; set; }

    public bool Attended { get; set; }

    public DateTime MarkedOn { get; set; } = DateTime.UtcNow;

    // Navigation
    public Event Event { get; set; } = null!;

    public AppUser Student { get; set; } = null!;
}
