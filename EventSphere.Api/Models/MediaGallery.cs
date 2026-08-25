namespace EventSphere.Api.Models;

public class MediaGallery
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public MediaType FileType { get; set; }

    public string FileUrl { get; set; } = string.Empty;

    public int UploadedBy { get; set; }

    public string? Caption { get; set; }

    public DateTime UploadedOn { get; set; } = DateTime.UtcNow;

    // Navigation
    public Event Event { get; set; } = null!;

    public AppUser Uploader { get; set; } = null!;
}
