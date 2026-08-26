namespace EventSphere.Api.Models;

public class Registration
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public int StudentId { get; set; }

    public DateTime RegisteredOn { get; set; } = DateTime.UtcNow;

    public RegistrationStatus Status { get; set; } = RegistrationStatus.Confirmed;

    [System.ComponentModel.DataAnnotations.MaxLength(64)]
    public string CheckInToken { get; set; } = string.Empty;

    public int? PaymentId { get; set; }

    // Navigation
    public Event Event { get; set; } = null!;

    public AppUser Student { get; set; } = null!;

    public Payment? Payment { get; set; }
}
