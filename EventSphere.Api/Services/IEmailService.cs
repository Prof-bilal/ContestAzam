namespace EventSphere.Api.Services;

/// <summary>
/// Abstraction for transactional email delivery. The authentication layer depends
/// on this interface — never on a concrete provider. Swap Brevo for any other
/// provider by implementing this interface and registering the new implementation.
/// </summary>
public interface IEmailService
{
    Task SendEmailVerificationAsync(string toEmail, string userName, string verificationUrl);
    Task SendPasswordResetAsync(string toEmail, string userName, string resetUrl);

    /// <summary>Send a fully-rendered transactional email (subject + HTML body).</summary>
    Task SendTransactionalAsync(string toEmail, string subject, string htmlContent);
}
