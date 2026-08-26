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

public class PasswordResetTests : IntegrationTestBase
{
    public PasswordResetTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Forgot_password_returns_generic_response_for_existing_user()
    {
        var client = NewClient();
        var email = UniqueEmail();
        (await RegisterAsync(client, email, StrongPassword)).EnsureSuccessStatusCode();

        var response = await client.PostAsJsonAsync("/api/auth/forgot-password",
            new { email });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.True(body!.Success);
        Assert.Contains("reset", body!.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Forgot_password_returns_same_response_for_unknown_email()
    {
        var client = NewClient();

        var response = await client.PostAsJsonAsync("/api/auth/forgot-password",
            new { email = "nonexistent@example.com" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.True(body!.Success);
        // Same generic message — no email enumeration.
        Assert.Contains("reset", body!.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Forgot_password_generates_reset_token()
    {
        var client = NewClient();
        var email = UniqueEmail();
        (await RegisterAsync(client, email, StrongPassword)).EnsureSuccessStatusCode();

        (await client.PostAsJsonAsync("/api/auth/forgot-password",
            new { email })).EnsureSuccessStatusCode();

        var emailService = Factory.Services.GetRequiredService<IEmailService>() as NoOpEmailService;
        Assert.NotNull(emailService);
        Assert.Contains(emailService!.Sent, s => s.Template == "ResetPassword" && s.To == email);
    }

    [Fact]
    public async Task Reset_password_with_valid_token_succeeds()
    {
        var client = NewClient();
        var email = UniqueEmail();
        (await RegisterAsync(client, email, StrongPassword)).EnsureSuccessStatusCode();

        // Request password reset
        (await client.PostAsJsonAsync("/api/auth/forgot-password",
            new { email })).EnsureSuccessStatusCode();

        var emailService = Factory.Services.GetRequiredService<IEmailService>() as NoOpEmailService;
        var record = emailService!.Sent.Last(s => s.Template == "ResetPassword" && s.To == email);
        var token = ExtractTokenFromUrl(record.Url);

        var newPassword = "N3w!P@ssw0rd#2026";

        var resetResponse = await client.PostAsJsonAsync("/api/auth/reset-password",
            new { email, token, newPassword, confirmPassword = newPassword });

        Assert.Equal(HttpStatusCode.OK, resetResponse.StatusCode);
        var body = await resetResponse.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.True(body!.Success);
    }

    [Fact]
    public async Task Reset_password_with_invalid_token_fails()
    {
        var client = NewClient();
        var email = UniqueEmail();
        (await RegisterAsync(client, email, StrongPassword)).EnsureSuccessStatusCode();

        var newPassword = "N3w!P@ssw0rd#2026";

        var resetResponse = await client.PostAsJsonAsync("/api/auth/reset-password",
            new { email, token = "invalid-token", newPassword, confirmPassword = newPassword });

        Assert.Equal(HttpStatusCode.BadRequest, resetResponse.StatusCode);
        var body = await resetResponse.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.False(body!.Success);
    }

    [Fact]
    public async Task Reset_password_requires_strong_password()
    {
        var client = NewClient();
        var email = UniqueEmail();
        (await RegisterAsync(client, email, StrongPassword)).EnsureSuccessStatusCode();

        (await client.PostAsJsonAsync("/api/auth/forgot-password",
            new { email })).EnsureSuccessStatusCode();

        var emailService = Factory.Services.GetRequiredService<IEmailService>() as NoOpEmailService;
        var record = emailService!.Sent.Last(s => s.Template == "ResetPassword" && s.To == email);
        var token = ExtractTokenFromUrl(record.Url);

        var resetResponse = await client.PostAsJsonAsync("/api/auth/reset-password",
            new { email, token, newPassword = "weak", confirmPassword = "weak" });

        Assert.Equal(HttpStatusCode.BadRequest, resetResponse.StatusCode);
    }

    [Fact]
    public async Task Reset_password_requires_matching_confirmation()
    {
        var client = NewClient();
        var email = UniqueEmail();
        (await RegisterAsync(client, email, StrongPassword)).EnsureSuccessStatusCode();

        (await client.PostAsJsonAsync("/api/auth/forgot-password",
            new { email })).EnsureSuccessStatusCode();

        var emailService = Factory.Services.GetRequiredService<IEmailService>() as NoOpEmailService;
        var record = emailService!.Sent.Last(s => s.Template == "ResetPassword" && s.To == email);
        var token = ExtractTokenFromUrl(record.Url);

        var resetResponse = await client.PostAsJsonAsync("/api/auth/reset-password",
            new { email, token, newPassword = "N3w!P@ssw0rd#2026", confirmPassword = "Different!123ABC" });

        Assert.Equal(HttpStatusCode.BadRequest, resetResponse.StatusCode);
    }

    [Fact]
    public async Task After_password_reset_user_can_login_with_new_password()
    {
        var client = NewClient();
        var email = UniqueEmail();
        (await RegisterAsync(client, email, StrongPassword)).EnsureSuccessStatusCode();

        // Request reset
        (await client.PostAsJsonAsync("/api/auth/forgot-password",
            new { email })).EnsureSuccessStatusCode();

        var emailService = Factory.Services.GetRequiredService<IEmailService>() as NoOpEmailService;
        var record = emailService!.Sent.Last(s => s.Template == "ResetPassword" && s.To == email);
        var token = ExtractTokenFromUrl(record.Url);

        var newPassword = "N3w!P@ssw0rd#2026";

        // Reset password
        (await client.PostAsJsonAsync("/api/auth/reset-password",
            new { email, token, newPassword, confirmPassword = newPassword })).EnsureSuccessStatusCode();

        // Old password should fail
        var oldLogin = await LoginAsync(NewClient(), email, StrongPassword);
        Assert.Equal(HttpStatusCode.Unauthorized, oldLogin.StatusCode);

        // New password should succeed
        var newLogin = await LoginAsync(NewClient(), email, newPassword);
        Assert.Equal(HttpStatusCode.OK, newLogin.StatusCode);
    }

    [Fact]
    public async Task Forgot_password_for_inactive_user_returns_generic_response()
    {
        var client = NewClient();
        var email = UniqueEmail();
        (await RegisterAsync(client, email, StrongPassword)).EnsureSuccessStatusCode();

        // Deactivate the user
        using var scope = Factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = await users.FindByEmailAsync(email);
        user!.IsActive = false;
        await users.UpdateAsync(user);

        var response = await client.PostAsJsonAsync("/api/auth/forgot-password",
            new { email });

        // Same generic response — no revelation about account state.
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
