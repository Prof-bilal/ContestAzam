using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EventSphere.Api.Common.Options;
using Microsoft.Extensions.Options;

namespace EventSphere.Api.Services;

/// <summary>
/// Brevo transactional email provider. All Brevo-specific logic is contained here.
/// The rest of the application depends only on IEmailService.
/// </summary>
public class BrevoEmailService : IEmailService
{
    private readonly HttpClient _http;
    private readonly BrevoOptions _options;
    private readonly ILogger<BrevoEmailService> _logger;

    private const string BrevoApiUrl = "https://api.brevo.com/v3/smtp/email";

    public BrevoEmailService(
        HttpClient http,
        IOptions<BrevoOptions> options,
        ILogger<BrevoEmailService> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendEmailVerificationAsync(string toEmail, string userName, string verificationUrl)
    {
        var subject = "EventSphere — Verify your email address";
        var htmlContent = BuildVerificationEmail(userName, verificationUrl);
        await SendAsync(toEmail, subject, htmlContent);
    }

    public async Task SendPasswordResetAsync(string toEmail, string userName, string resetUrl)
    {
        var subject = "EventSphere — Reset your password";
        var htmlContent = BuildPasswordResetEmail(userName, resetUrl);
        await SendAsync(toEmail, subject, htmlContent);
    }

    public Task SendTransactionalAsync(string toEmail, string subject, string htmlContent)
        => SendAsync(toEmail, subject, htmlContent);

    private async Task SendAsync(string toEmail, string subject, string htmlContent)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogWarning("Brevo API key not configured. Email to {Email} was not sent.", toEmail);
            return;
        }

        var payload = new BrevoEmailRequest
        {
            Sender = new BrevoSender
            {
                Email = _options.SenderEmail,
                Name = _options.SenderName
            },
            To = new[]
            {
                new BrevoRecipient { Email = toEmail }
            },
            Subject = subject,
            HtmlContent = htmlContent
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, BrevoApiUrl);
        request.Headers.Add("api-key", _options.ApiKey);
        request.Headers.Add("accept", "application/json");
        request.Content = JsonContent.Create(payload, options: new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        try
        {
            var response = await _http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Brevo API returned {StatusCode} for email to {Email}.",
                    response.StatusCode, toEmail);
                throw new EmailDeliveryException(
                    $"Brevo API returned {(int)response.StatusCode}.",
                    response.StatusCode);
            }
        }
        catch (EmailDeliveryException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error sending email to {Email} via Brevo.", toEmail);
            throw new EmailDeliveryException("Unable to reach the email service.", System.Net.HttpStatusCode.ServiceUnavailable, ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout sending email to {Email} via Brevo.", toEmail);
            throw new EmailDeliveryException("Email service timed out.", System.Net.HttpStatusCode.RequestTimeout, ex);
        }
    }

    private static string BuildVerificationEmail(string userName, string verificationUrl)
    {
        return $"""
        <!DOCTYPE html>
        <html>
        <head><meta charset="utf-8"></head>
        <body style="font-family:system-ui,-apple-system,sans-serif;background:#0f172a;color:#e2e8f0;padding:2rem;">
          <div style="max-width:480px;margin:0 auto;background:#1e293b;border-radius:12px;padding:2rem;border:1px solid #334155;">
            <h1 style="color:#818cf8;font-size:1.25rem;margin:0 0 1rem;">EventSphere</h1>
            <h2 style="font-size:1.1rem;margin:0 0 1rem;">Verify your email address</h2>
            <p style="color:#94a3b8;line-height:1.6;">Hi {System.Net.WebUtility.HtmlEncode(userName)},</p>
            <p style="color:#94a3b8;line-height:1.6;">Click the button below to verify your email address.</p>
            <a href="{verificationUrl}" style="display:inline-block;background:#6366f1;color:white;padding:0.75rem 1.5rem;border-radius:8px;text-decoration:none;font-weight:600;margin:1rem 0;">Verify Email</a>
            <p style="color:#94a3b8;font-size:0.85rem;line-height:1.6;">This link will expire after a limited time. If you did not create an account, you can safely ignore this email.</p>
          </div>
        </body>
        </html>
        """;
    }

    private static string BuildPasswordResetEmail(string userName, string resetUrl)
    {
        return $"""
        <!DOCTYPE html>
        <html>
        <head><meta charset="utf-8"></head>
        <body style="font-family:system-ui,-apple-system,sans-serif;background:#0f172a;color:#e2e8f0;padding:2rem;">
          <div style="max-width:480px;margin:0 auto;background:#1e293b;border-radius:12px;padding:2rem;border:1px solid #334155;">
            <h1 style="color:#818cf8;font-size:1.25rem;margin:0 0 1rem;">EventSphere</h1>
            <h2 style="font-size:1.1rem;margin:0 0 1rem;">Reset your password</h2>
            <p style="color:#94a3b8;line-height:1.6;">Hi {System.Net.WebUtility.HtmlEncode(userName)},</p>
            <p style="color:#94a3b8;line-height:1.6;">We received a request to reset your password. Click the button below to choose a new one.</p>
            <a href="{resetUrl}" style="display:inline-block;background:#6366f1;color:white;padding:0.75rem 1.5rem;border-radius:8px;text-decoration:none;font-weight:600;margin:1rem 0;">Reset Password</a>
            <p style="color:#94a3b8;font-size:0.85rem;line-height:1.6;">This link will expire after a limited time. If you did not request this, you can safely ignore this email.</p>
          </div>
        </body>
        </html>
        """;
    }
}

public class EmailDeliveryException : Exception
{
    public System.Net.HttpStatusCode StatusCode { get; }

    public EmailDeliveryException(string message, System.Net.HttpStatusCode statusCode, Exception? inner = null)
        : base(message, inner)
    {
        StatusCode = statusCode;
    }
}

// ---- Brevo API request/response models (internal, never leaked) ----

internal class BrevoEmailRequest
{
    [JsonPropertyName("sender")]
    public BrevoSender Sender { get; set; } = new();

    [JsonPropertyName("to")]
    public BrevoRecipient[] To { get; set; } = Array.Empty<BrevoRecipient>();

    [JsonPropertyName("subject")]
    public string Subject { get; set; } = string.Empty;

    [JsonPropertyName("htmlContent")]
    public string HtmlContent { get; set; } = string.Empty;
}

internal class BrevoSender
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

internal class BrevoRecipient
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
}
