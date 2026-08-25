namespace EventSphere.Api.Models;

public class EventSeating
{
    public int EventId { get; set; }

    public int? VenueId { get; set; }

    public int TotalSeats { get; set; }

    public int SeatsBooked { get; set; }

    public bool WaitlistEnabled { get; set; }

    // Navigation
    public Event Event { get; set; } = null!;

    public Venue? Venue { get; set; }
}
