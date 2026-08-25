namespace EventSphere.Api.Models;

public class EventCategory
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    // Navigation
    public ICollection<Event> Events { get; set; } = new List<Event>();
}
