namespace EventSphere.Api.Models;

public class Venue
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Location { get; set; }

    public int Capacity { get; set; }

    // Navigation
    public EventSeating? EventSeating { get; set; }
}
