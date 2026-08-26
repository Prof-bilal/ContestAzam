namespace EventSphere.Api.Models;

public class UserDetails
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? Mobile { get; set; }

    public string? Department { get; set; }

    public string? EnrollmentNo { get; set; }

    /// <summary>Base64-encoded profile image or a URL. Stored for demo purposes.</summary>
    public string? ProfileImageUrl { get; set; }

    // Navigation
    public AppUser User { get; set; } = null!;
}
