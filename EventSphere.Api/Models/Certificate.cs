namespace EventSphere.Api.Models;

public class Certificate
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public int StudentId { get; set; }

    public string CertificateUrl { get; set; } = string.Empty;

    public DateTime IssuedOn { get; set; } = DateTime.UtcNow;

    public bool FeePaid { get; set; } = false;

    // Navigation
    public Event Event { get; set; } = null!;

    public AppUser Student { get; set; } = null!;
}
