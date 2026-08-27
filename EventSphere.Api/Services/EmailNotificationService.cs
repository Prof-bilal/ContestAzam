using System.Net;
using System.Text;

namespace EventSphere.Api.Services;

/// <summary>
/// Convenience abstraction over IEmailService for domain notification emails.
/// Rendering lives here; the actual transport (Brevo / no-op) stays behind
/// IEmailService so this code never touches an API key.
/// Email failures are caught and logged so they never fail the primary business op.
/// </summary>
public interface IEmailNotificationService
{
    Task<bool> TrySendRegistrationConfirmedAsync(string email, string name, string eventTitle, DateTime eventDate);
    Task<bool> TrySendRegistrationCancelledAsync(string email, string name, string eventTitle);
    Task<bool> TrySendPaymentSuccessfulAsync(string email, string name, string eventTitle, decimal amount);
    Task<bool> TrySendPaymentFailedAsync(string email, string name, string eventTitle);
    Task<bool> TrySendEventCancelledAsync(string email, string name, string eventTitle);
    Task<bool> TrySendAttendanceConfirmedAsync(string email, string name, string eventTitle);
    Task<bool> TrySendOrganizerApprovedAsync(string email, string name);
    Task<bool> TrySendOrganizerRejectedAsync(string email, string name, string? reason);
    Task<bool> TrySendNewAttendeeAsync(string email, string name, string eventTitle, string attendeeName);
    Task<bool> TrySendNewEventInCategoryAsync(string email, string name, string eventTitle, string categoryName, DateTime eventDate, string eventUrl);
    Task<bool> TrySendEventApprovedAsync(string email, string name, string eventTitle);
    Task<bool> TrySendEventRejectedAsync(string email, string name, string eventTitle, string? reason);
    Task<bool> TrySendAccountWarningAsync(string email, string name, string message);
}

public class EmailNotificationService : IEmailNotificationService
{
    private readonly IEmailService _email;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(IEmailService email, ILogger<EmailNotificationService> logger)
    {
        _email = email;
        _logger = logger;
    }
public Task<bool> TrySendRegistrationConfirmedAsync(string email, string name, string eventTitle, DateTime eventDate)
        => GuardAsync("registration-confirmed", email, "Your registration is confirmed",
            $"Hi {Encode(name)}, your registration for <strong>{Encode(eventTitle)}</strong> on {eventDate:dd MMM yyyy} is confirmed.");

    public Task<bool> TrySendRegistrationCancelledAsync(string email, string name, string eventTitle)
        => GuardAsync("registration-cancelled", email, "Your registration was cancelled",
            $"Hi {Encode(name)}, your registration for <strong>{Encode(eventTitle)}</strong> has been cancelled.");

    public Task<bool> TrySendPaymentSuccessfulAsync(string email, string name, string eventTitle, decimal amount)
        => GuardAsync("payment-successful", email, "Payment successful",
            $"Hi {Encode(name)}, your payment of <strong>{amount:C}</strong> for <strong>{Encode(eventTitle)}</strong> was successful.");

    public Task<bool> TrySendPaymentFailedAsync(string email, string name, string eventTitle)
        => GuardAsync("payment-failed", email, "Payment could not be completed",
            $"Hi {Encode(name)}, your payment for <strong>{Encode(eventTitle)}</strong> could not be completed. Please try again.");

    public Task<bool> TrySendEventCancelledAsync(string email, string name, string eventTitle)
        => GuardAsync("event-cancelled", email, "Event cancelled",
            $"Hi {Encode(name)}, the event <strong>{Encode(eventTitle)}</strong> you registered for has been cancelled.");

    public Task<bool> TrySendAttendanceConfirmedAsync(string email, string name, string eventTitle)
        => GuardAsync("attendance-confirmed", email, "Attendance confirmed",
            $"Hi {Encode(name)}, your attendance for <strong>{Encode(eventTitle)}</strong> was confirmed. Welcome!");

    public Task<bool> TrySendOrganizerApprovedAsync(string email, string name)
        => GuardAsync("organizer-approved", email, "You are now an organizer",
            $"Hi {Encode(name)}, your organizer request was approved. You can now create and manage events.");

    public Task<bool> TrySendOrganizerRejectedAsync(string email, string name, string? reason)
        => GuardAsync("organizer-rejected", email, "Organizer request update",
            $"Hi {Encode(name)}, your organizer request was not approved." +
            (string.IsNullOrWhiteSpace(reason) ? string.Empty : $" Reason: {Encode(reason)}"));

    public Task<bool> TrySendNewAttendeeAsync(string email, string name, string eventTitle, string attendeeName)
        => GuardAsync("new-attendee", email, "New attendee registered",
            $"Hi {Encode(name)}, <strong>{Encode(attendeeName)}</strong> just registered for your event <strong>{Encode(eventTitle)}</strong>.");

    public Task<bool> TrySendNewEventInCategoryAsync(string email, string name, string eventTitle, string categoryName, DateTime eventDate, string eventUrl)
        => GuardAsync("new-event-in-category", email, "New event in your interest",
            $"Hi {Encode(name)}, a new event <strong>{Encode(eventTitle)}</strong> in <strong>{Encode(categoryName)}</strong> was just created! " +
            $"It's happening on {eventDate:dd MMM yyyy}. " +
            $"<a href=\"{eventUrl}\" style=\"color:#818cf8\">View Event &rarr;</a>");

    public Task<bool> TrySendEventApprovedAsync(string email, string name, string eventTitle)
        => GuardAsync("event-approved", email, "Your event has been approved",
            $"Hi {Encode(name)}, great news! Your event <strong>{Encode(eventTitle)}</strong> has been approved and is now live on EventSphere.");

    public Task<bool> TrySendEventRejectedAsync(string email, string name, string eventTitle, string? reason)
        => GuardAsync("event-rejected", email, "Event not approved",
            $"Hi {Encode(name)}, your event <strong>{Encode(eventTitle)}</strong> was not approved." +
            (string.IsNullOrWhiteSpace(reason) ? string.Empty : $" Reason: {Encode(reason)}"));

    private async Task<bool> GuardAsync(string template, string email, string subject, string body)
    {
        try
        {
            var html = BuildShell(body);
            await _email.SendTransactionalAsync(email, $"EventSphere — {subject}", html);
            return true;
        }
        catch (EmailDeliveryException ex)
        {
            // Non-fatal: log status only, never the API key or message content.
            _logger.LogWarning("Email template {Template} for {Email} failed with {Status}.", template, email, ex.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Email template {Template} for {Email} failed.", template, email);
            return false;
        }
    }

    public async Task<bool> TrySendAccountWarningAsync(string email, string name, string message)
    {
        try
        {
            var body = $"""
                <p>Hi {Encode(name)},</p>
                <p style="color:#fbbf24;font-weight:bold;font-size:1.05rem;">⚠️ Account Warning</p>
                <p>An administrator has issued a warning for your EventSphere account:</p>
                <div style="background:#312e2e;border-left:4px solid #fbbf24;padding:0.75rem 1rem;margin:1rem 0;border-radius:4px;">
                    <p style="color:#fbbf24;margin:0;">{Encode(message)}</p>
                </div>
                <p>Please review our community guidelines. Further violations may result in account suspension.</p>
                <p>If you believe this is a mistake, please contact the site administrator.</p>
            """;
            await _email.SendTransactionalAsync(email, "⚠️ Account Warning — EventSphere", BuildShell(body));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send warning email to {Email}.", email);
            return false;
        }
    }

    private static string BuildShell(string body) =>
        $"""
        <!DOCTYPE html>
        <html>
        <head><meta charset="utf-8"></head>
        <body style="font-family:system-ui,-apple-system,sans-serif;background:#0f172a;color:#e2e8f0;padding:2rem;">
          <div style="max-width:480px;margin:0 auto;background:#1e293b;border-radius:12px;padding:2rem;border:1px solid #334155;">
            <h1 style="color:#818cf8;font-size:1.25rem;margin:0 0 1rem;">EventSphere</h1>
            <p style="color:#94a3b8;line-height:1.6;">{body}</p>
            <p style="color:#94a3b8;font-size:0.85rem;margin-top:2rem;">You received this because you have an account on EventSphere.</p>
          </div>
        </body>
        </html>
        """;

    private static string Encode(string value) => System.Net.WebUtility.HtmlEncode(value);
}