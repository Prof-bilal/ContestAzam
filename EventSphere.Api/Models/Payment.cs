namespace EventSphere.Api.Models;

public class Payment
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public int UserId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "usd";

    public string StripeSessionId { get; set; } = string.Empty;

    public string? StripePaymentIntentId { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? PaidAt { get; set; }

    // Navigation
    public Event Event { get; set; } = null!;

    public AppUser User { get; set; } = null!;
}
