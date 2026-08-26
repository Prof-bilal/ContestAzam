using System.Net;
using System.Net.Http.Json;
using EventSphere.Api.Common;
using EventSphere.Api.DTOs;
using EventSphere.Api.Models;
using EventSphere.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EventSphere.Tests;

public class EmailVerificationTests : IntegrationTestBase
{
    public EmailVerificationTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Registration_sends_verification_email()
    {
        var client = NewClient();
        var email = UniqueEmail();

        var response = await RegisterAsync(client, email, StrongPassword);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var emailService = Factory.Services.GetRequiredService<IEmailService>() as NoOpEmailService;
        Assert.NotNull(emailService);
        Assert.Contains(emailService!.Sent, s => s.Template == "VerifyEmail" && s.To == email);
    }

    [Fact]
    public async Task Valid_verification_token_succeeds()
    {
        var client = NewClient();
        var email = UniqueEmail();
        (await RegisterAsync(client, email, StrongPassword)).EnsureSuccessStatusCode();

        // Get the verification token from the NoOp email service.
        var emailService = Factory.Services.GetRequiredService<IEmailService>() as NoOpEmailService;
        var record = emailService!.Sent.Last(s => s.Template == "VerifyEmail" && s.To == email);
        var token = ExtractTokenFromUrl(record.Url);

        var verifyResponse = await client.PostAsJsonAsync("/api/auth/verify-email",
            new { email, token });

        Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);
        var body = await verifyResponse.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.True(body!.Success);
    }

    [Fact]
    public async Task Invalid_token_is_rejected()
    {
        var client = NewClient();
        var email = UniqueEmail();
        (await RegisterAsync(client, email, StrongPassword)).EnsureSuccessStatusCode();

        var verifyResponse = await client.PostAsJsonAsync("/api/auth/verify-email",
            new { email, token = "invalid-token-value" });

        Assert.Equal(HttpStatusCode.BadRequest, verifyResponse.StatusCode);
        var body = await verifyResponse.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.False(body!.Success);
    }

    [Fact]
    public async Task Already_verified_account_returns_ok()
    {
        var client = NewClient();
        var email = UniqueEmail();
        (await RegisterAsync(client, email, StrongPassword)).EnsureSuccessStatusCode();

        var emailService = Factory.Services.GetRequiredService<IEmailService>() as NoOpEmailService;
        var record = emailService!.Sent.Last(s => s.Template == "VerifyEmail" && s.To == email);
        var token = ExtractTokenFromUrl(record.Url);

        // First verification
        (await client.PostAsJsonAsync("/api/auth/verify-email",
            new { email, token })).EnsureSuccessStatusCode();

        // Second verification — should return "already verified"
        var second = await client.PostAsJsonAsync("/api/auth/verify-email",
            new { email, token });
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
    }

    [Fact]
    public async Task Verified_user_can_login()
    {
        var client = NewClient();
        var email = UniqueEmail();
        (await RegisterAsync(client, email, StrongPassword)).EnsureSuccessStatusCode();

        // Verify the email
        var emailService = Factory.Services.GetRequiredService<IEmailService>() as NoOpEmailService;
        var record = emailService!.Sent.Last(s => s.Template == "VerifyEmail" && s.To == email);
        var token = ExtractTokenFromUrl(record.Url);
        (await client.PostAsJsonAsync("/api/auth/verify-email",
            new { email, token })).EnsureSuccessStatusCode();

        // Now enable RequireConfirmedAccount and try login
        using var scope = Factory.Services.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<IdentityOptions>>();
        options.Value.SignIn.RequireConfirmedAccount = true;

        var loginResponse = await LoginAsync(NewClient(), email, StrongPassword);
        // With the override, this should succeed because the email IS confirmed
        var body = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        Assert.True(body!.Success);
    }

    [Fact]
    public async Task Resend_verification_returns_generic_response()
    {
        var client = NewClient();
        var email = UniqueEmail();

        var response = await client.PostAsJsonAsync("/api/auth/resend-verification",
            new { email });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.True(body!.Success);
        Assert.Contains("verification", body!.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Resend_verification_for_existing_user_sends_email()
    {
        var client = NewClient();
        var email = UniqueEmail();
        (await RegisterAsync(client, email, StrongPassword)).EnsureSuccessStatusCode();

        var response = await client.PostAsJsonAsync("/api/auth/resend-verification",
            new { email });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var emailService = Factory.Services.GetRequiredService<IEmailService>() as NoOpEmailService;
        Assert.Equal(2, emailService!.Sent.Count(s => s.Template == "VerifyEmail" && s.To == email));
    }

    [Fact]
    public async Task Resend_verification_for_unknown_email_returns_same_response()
    {
        var client = NewClient();

        var response = await client.PostAsJsonAsync("/api/auth/resend-verification",
            new { email = "nonexistent@example.com" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.True(body!.Success);
    }

    private static string ExtractTokenFromUrl(string url)
    {
        var uri = new Uri(url);
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        return query["token"] ?? "";
    }
}
