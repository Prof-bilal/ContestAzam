using EventSphere.Api.Common.Options;
using EventSphere.Api.Data;
using EventSphere.Api.DTOs;
using EventSphere.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace EventSphere.Api.Services;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _db;
    private readonly StripeOptions _stripeOptions;
    private readonly UserManager<AppUser> _userManager;
    private readonly INotificationService _notifications;
    private readonly IEmailNotificationService _emails;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        AppDbContext db,
        IOptions<StripeOptions> stripeOptions,
        UserManager<AppUser> userManager,
        INotificationService notifications,
        IEmailNotificationService emails,
        ILogger<PaymentService> logger)
    {
        _db = db;
        _stripeOptions = stripeOptions.Value;
        _userManager = userManager;
        _notifications = notifications;
        _emails = emails;
        _logger = logger;

        if (!string.IsNullOrEmpty(_stripeOptions.SecretKey))
        {
            StripeConfiguration.ApiKey = _stripeOptions.SecretKey;
        }
    }

    public string GetPublishableKey() => _stripeOptions.PublishableKey;

    public async Task<PaymentSessionDto> CreateCheckoutSessionAsync(int eventId, int userId, string origin)
    {
        var evt = await _db.Events.FindAsync(eventId)
            ?? throw new InvalidOperationException("Event not found.");

        if (evt.Status != EventStatus.Approved)
            throw new InvalidOperationException("Event is not approved for registration.");

        if (!evt.IsPaid)
            throw new InvalidOperationException("This is a free event. Use direct registration.");

        var existingPayment = await _db.Payments
            .FirstOrDefaultAsync(p => p.EventId == eventId && p.UserId == userId && p.Status == PaymentStatus.Succeeded);
        if (existingPayment is not null)
            throw new InvalidOperationException("You have already paid for this event.");

        var pendingPayment = await _db.Payments
            .FirstOrDefaultAsync(p => p.EventId == eventId && p.UserId == userId && p.Status == PaymentStatus.Pending);

        var amountInCents = (long)(evt.Price * 100);

        var sessionOptions = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = evt.Title,
                            Description = $"Registration for {evt.Title} on {evt.EventDate:yyyy-MM-dd}"
                        },
                        UnitAmount = amountInCents,
                    },
                    Quantity = 1,
                }
            },
            Mode = "payment",
            SuccessUrl = $"{origin}/payment/success?eventId={eventId}",
            CancelUrl = $"{origin}/payment/cancel?eventId={eventId}",
            Metadata = new Dictionary<string, string>
            {
                { "eventId", eventId.ToString() },
                { "userId", userId.ToString() }
            }
        };

        var sessionService = new SessionService();
        var session = await sessionService.CreateAsync(sessionOptions);

        if (pendingPayment is not null)
        {
            pendingPayment.StripeSessionId = session.Id;
            pendingPayment.Amount = evt.Price;
            await _db.SaveChangesAsync();
        }
        else
        {
            var payment = new Payment
            {
                EventId = eventId,
                UserId = userId,
                Amount = evt.Price,
                Currency = "usd",
                StripeSessionId = session.Id,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();
        }

        _logger.LogInformation("Stripe Checkout session created for Event {EventId}, User {UserId}, Session {SessionId}",
            eventId, userId, session.Id);

        return new PaymentSessionDto
        {
            SessionId = session.Id,
            Url = session.Url
        };
    }

    public async Task<bool> HandleWebhookAsync(string json, string? stripeSignature)
    {
        if (string.IsNullOrEmpty(_stripeOptions.WebhookSecret))
        {
            _logger.LogWarning("Stripe webhook secret not configured. Skipping webhook processing.");
            return false;
        }

        Stripe.Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, _stripeOptions.WebhookSecret);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify Stripe webhook signature.");
            return false;
        }

        if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
        {
            var session = stripeEvent.Data.Object as Session;
            if (session?.Metadata is null)
            {
                _logger.LogWarning("Checkout session completed but missing metadata.");
                return false;
            }

            var eventId = int.Parse(session.Metadata["eventId"]);
            var userId = int.Parse(session.Metadata["userId"]);

            var payment = await _db.Payments
                .FirstOrDefaultAsync(p => p.StripeSessionId == session.Id);

            if (payment is null)
            {
                _logger.LogError("No payment record found for Stripe session {SessionId}", session.Id);
                return false;
            }

            if (payment.Status == PaymentStatus.Succeeded)
            {
                _logger.LogInformation("Payment for session {SessionId} already processed. Idempotent.", session.Id);
                return true;
            }

            payment.Status = PaymentStatus.Succeeded;
            payment.StripePaymentIntentId = session.PaymentIntentId;
            payment.PaidAt = DateTime.UtcNow;

            var existingRegistration = await _db.Registrations
                .FirstOrDefaultAsync(r => r.EventId == eventId && r.StudentId == userId);

            if (existingRegistration is not null)
            {
                if (existingRegistration.Status != RegistrationStatus.Confirmed)
                {
                    existingRegistration.Status = RegistrationStatus.Confirmed;
                    existingRegistration.RegisteredOn = DateTime.UtcNow;
                    existingRegistration.CheckInToken = Guid.NewGuid().ToString("N");
                    existingRegistration.PaymentId = payment.Id;
                }
            }
            else
            {
                _db.Registrations.Add(new Registration
                {
                    EventId = eventId,
                    StudentId = userId,
                    Status = RegistrationStatus.Confirmed,
                    RegisteredOn = DateTime.UtcNow,
                    CheckInToken = Guid.NewGuid().ToString("N"),
                    PaymentId = payment.Id
                });
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is not null && !await _userManager.IsInRoleAsync(user, "Participant"))
            {
                await _userManager.AddToRoleAsync(user, "Participant");
            }

            await _db.SaveChangesAsync();

            // In-app + email on successful payment.
            var user2 = await _userManager.FindByIdAsync(userId.ToString());
            var evt = await _db.Events.FindAsync(eventId);
            if (user2 is not null && evt is not null)
            {
                var name = user2.UserDetails?.FullName ?? user2.UserName ?? "there";
                await _notifications.SendAsync(userId, NotificationType.PaymentSuccessful,
                    "Payment Successful",
                    $"Your payment for {evt.Title} was completed successfully.",
                    relatedEntityId: evt.Id, relatedEntityType: "Event", actionUrl: $"/events/{evt.Id}");
                await _emails.TrySendPaymentSuccessfulAsync(user2.Email ?? string.Empty, name, evt.Title, payment.Amount);
            }

            _logger.LogInformation("Payment confirmed and registration created for Event {EventId}, User {UserId}", eventId, userId);
            return true;
        }

        if (stripeEvent.Type == EventTypes.CheckoutSessionExpired)
        {
            var session = stripeEvent.Data.Object as Session;
            if (session is not null)
            {
                var payment = await _db.Payments
                    .FirstOrDefaultAsync(p => p.StripeSessionId == session.Id);
                if (payment is not null && payment.Status == PaymentStatus.Pending)
                {
                    payment.Status = PaymentStatus.Failed;
                    await _db.SaveChangesAsync();

                    if (session.Metadata is not null &&
                        session.Metadata.TryGetValue("eventId", out var evId) &&
                        session.Metadata.TryGetValue("userId", out var usrId) &&
                        int.TryParse(usrId, out var uid) &&
                        int.TryParse(evId, out var eid))
                    {
                        var user = await _userManager.FindByIdAsync(uid.ToString());
                        var evt = await _db.Events.FindAsync(eid);
                        if (user is not null && evt is not null)
                        {
                            var name = user.UserDetails?.FullName ?? user.UserName ?? "there";
                            await _notifications.SendAsync(user.Id, NotificationType.PaymentFailed,
                                "Payment Failed",
                                $"Your payment for {evt.Title} could not be completed.",
                                relatedEntityId: evt.Id, relatedEntityType: "Event", actionUrl: $"/events/{evt.Id}");
                            await _emails.TrySendPaymentFailedAsync(user.Email ?? string.Empty, name, evt.Title);
                        }
                    }
                }
            }
        }

        return true;
    }

    public async Task<PaymentDto?> GetPaymentStatusAsync(int eventId, int userId)
    {
        var payment = await _db.Payments
            .Where(p => p.EventId == eventId && p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();

        if (payment is null) return null;

        return new PaymentDto
        {
            Id = payment.Id,
            Amount = payment.Amount,
            Status = payment.Status.ToString(),
            PaidAt = payment.PaidAt
        };
    }
}
