using System.Net.Http.Json;
using System.Text.Json;
using EventSphere.Api.Common;
using EventSphere.Api.Data;
using EventSphere.Api.DTOs;
using EventSphere.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EventSphere.Tests;

public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    protected readonly CustomWebApplicationFactory Factory;

    /// <summary>A password that satisfies the strong policy (12+, upper, lower, digit, symbol).</summary>
    protected const string StrongPassword = "Str0ng!Passw0rd#2025";

    protected IntegrationTestBase(CustomWebApplicationFactory factory) => Factory = factory;

    public async Task InitializeAsync() => await Factory.SeedAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    protected HttpClient NewClient(bool handleCookies = true) =>
        Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = handleCookies,
            AllowAutoRedirect = false
        });

    protected static string UniqueEmail(string prefix = "user") =>
        $"{prefix}.{Guid.NewGuid():N}@example.com";

    protected static Task<HttpResponseMessage> RegisterAsync(
        HttpClient client, string email, string password, string name = "Test User") =>
        client.PostAsJsonAsync("/api/auth/register",
            new { name, email, password, confirmPassword = password });

    protected static Task<HttpResponseMessage> LoginAsync(
        HttpClient client, string email, string password) =>
        client.PostAsJsonAsync("/api/auth/login", new { email, password });

    protected static async Task<AuthResponse> ReadAuthAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        Assert.NotNull(body);
        Assert.True(body!.Success);
        Assert.NotNull(body.Data);
        return body.Data!;
    }

    /// <summary>Registers a user, elevates to the given role, and returns an access token carrying it.</summary>
    protected async Task<string> CreateUserWithRoleAsync(string role)
    {
        var email = UniqueEmail(role.ToLowerInvariant());
        var client = NewClient();
        (await RegisterAsync(client, email, StrongPassword)).EnsureSuccessStatusCode();

        if (role != "Visitor")
            await Factory.AddUserToRoleAsync(email, role);

        var login = await LoginAsync(client, email, StrongPassword);
        login.EnsureSuccessStatusCode();
        return (await ReadAuthAsync(login)).AccessToken;
    }

    /// <summary>Directly confirms a user's email via UserManager (for testing endpoints that require email verification).</summary>
    protected async Task ConfirmUserEmailAsync(string email)
    {
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = await userManager.FindByEmailAsync(email);
        if (user is not null && !user.EmailConfirmed)
        {
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            await userManager.ConfirmEmailAsync(user, token);
        }
    }

    /// <summary>Extracts a cookie value from Set-Cookie headers (for manual refresh/reuse tests).</summary>
    protected static string? ExtractCookie(HttpResponseMessage response, string name)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var cookies)) return null;
        foreach (var c in cookies)
        {
            var first = c.Split(';')[0];
            var idx = first.IndexOf('=');
            if (idx > 0 && first[..idx].Trim() == name)
                return first[(idx + 1)..].Trim();
        }
        return null;
    }
}
