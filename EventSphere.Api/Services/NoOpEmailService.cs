namespace EventSphere.Api.Services;

/// <summary>
/// No-op email service used in Testing environment. Emails are never sent;
/// callers can inspect the last-sent details for assertions in tests.
/// </summary>
public class NoOpEmailService : IEmailService
{
    public List<EmailRecord> Sent { get; } = new();

    public Task SendEmailVerificationAsync(string toEmail, string userName, string verificationUrl)
    {
        Sent.Add(new EmailRecord(toEmail, "VerifyEmail", verificationUrl));
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string toEmail, string userName, string resetUrl)
    {
        Sent.Add(new EmailRecord(toEmail, "ResetPassword", resetUrl));
        return Task.CompletedTask;
    }

    public Task SendTransactionalAsync(string toEmail, string subject, string htmlContent)
    {
        Sent.Add(new EmailRecord(toEmail, "Transactional", htmlContent));
        return Task.CompletedTask;
    }
}

public record EmailRecord(string To, string Template, string Url);
