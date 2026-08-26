namespace EventSphere.Api.Models;

public enum OrganizerRequestStatus
{
    Pending,
    Approved,
    Rejected
}

public class OrganizerRequest
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string OrganizationName { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public string? Experience { get; set; }

    public OrganizerRequestStatus Status { get; set; } = OrganizerRequestStatus.Pending;

    public int? ReviewedBy { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public string? RejectionReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation — only UserId is the owning FK.
    // ReviewedBy is configured as a shadow FK without a navigation to avoid
    // SQL Server multiple-cascade-path restrictions.
    public AppUser User { get; set; } = null!;
}
