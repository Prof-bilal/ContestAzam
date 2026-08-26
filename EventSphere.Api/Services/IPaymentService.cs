using EventSphere.Api.DTOs;

namespace EventSphere.Api.Services;

public interface IPaymentService
{
    Task<PaymentSessionDto> CreateCheckoutSessionAsync(int eventId, int userId, string origin);
    Task<bool> HandleWebhookAsync(string json, string? stripeSignature);
    Task<PaymentDto?> GetPaymentStatusAsync(int eventId, int userId);
    string GetPublishableKey();
}
