namespace EventSphere.Api.Models;

public class Event
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int CategoryId { get; set; }

    public DateTime EventDate { get; set; }

    public TimeSpan EventTime { get; set; }

    public string? Venue { get; set; }

    public int OrganizerId { get; set; }

    public int MaxParticipants { get; set; }

    public EventStatus Status { get; set; } = EventStatus.PendingApproval;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public EventCategory Category { get; set; } = null!;

    public AppUser Organizer { get; set; } = null!;

    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();

    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public ICollection<MediaGallery> MediaGalleries { get; set; } = new List<MediaGallery>();

    public EventSeating? EventSeating { get; set; }

    public ICollection<EventWaitlist> WaitlistEntries { get; set; } = new List<EventWaitlist>();

    public ICollection<CalendarSync> CalendarSyncs { get; set; } = new List<CalendarSync>();

    public ICollection<EventShareLog> ShareLogs { get; set; } = new List<EventShareLog>();
}
