using Microsoft.AspNetCore.Identity;

namespace EventSphere.Api.Models;

public class AppUser : IdentityUser<int>
{
    
    
    public UserRole Role { get; set; } = UserRole.Visitor;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public UserDetails? UserDetails { get; set; }

    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();

    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public ICollection<MediaGallery> UploadedMedia { get; set; } = new List<MediaGallery>();

    public ICollection<Event> OrganizedEvents { get; set; } = new List<Event>();

    public ICollection<EventWaitlist> WaitlistEntries { get; set; } = new List<EventWaitlist>();

    public ICollection<CalendarSync> CalendarSyncs { get; set; } = new List<CalendarSync>();

    public ICollection<EventShareLog> ShareLogs { get; set; } = new List<EventShareLog>();

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
