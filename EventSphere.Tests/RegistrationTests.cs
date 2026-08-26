using System.Net;
using System.Net.Http.Json;
using EventSphere.Api.Common;
using EventSphere.Api.DTOs;
using Xunit;

namespace EventSphere.Tests;

public class RegistrationTests : IntegrationTestBase
{
    public RegistrationTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Valid_registration_succeeds_and_assigns_Visitor_role()
    {
        var client = NewClient();
        var email = UniqueEmail();

        var response = await RegisterAsync(client, email, StrongPassword);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var auth = await ReadAuthAsync(response);
        Assert.Equal(email, auth.User.Email);
        Assert.Equal(new[] { "Visitor" }, auth.User.Roles);
        Assert.False(string.IsNullOrWhiteSpace(auth.AccessToken));
    }

    [Fact]
    public async Task Weak_password_is_rejected_with_field_error()
    {
        var client = NewClient();
        var response = await RegisterAsync(client, UniqueEmail(), "weak");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.False(body!.Success);
        Assert.True(body.Errors!.ContainsKey("password"));
    }

    [Fact]
    public async Task Invalid_email_is_rejected()
    {
        var client = NewClient();
        var response = await client.PostAsJsonAsync("/api/auth/register",
            new { name = "X", email = "not-an-email", password = StrongPassword, confirmPassword = StrongPassword });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.True(body!.Errors!.ContainsKey("email"));
    }

    [Fact]
    public async Task Password_mismatch_is_rejected()
    {
        var client = NewClient();
        var response = await client.PostAsJsonAsync("/api/auth/register",
            new { name = "X", email = UniqueEmail(), password = StrongPassword, confirmPassword = "Different!123ABC" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.True(body!.Errors!.ContainsKey("confirmPassword"));
    }

    [Fact]
    public async Task Duplicate_email_returns_conflict()
    {
        var client = NewClient();
        var email = UniqueEmail();
        (await RegisterAsync(client, email, StrongPassword)).EnsureSuccessStatusCode();

        var second = await RegisterAsync(NewClient(), email, StrongPassword);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Missing_fields_are_rejected()
    {
        var client = NewClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", new { email = UniqueEmail() });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Client_supplied_privileged_role_is_ignored()
    {
        var client = NewClient();
        var email = UniqueEmail("sneaky");

        // Attempt to smuggle privileged role fields — they must be ignored.
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            name = "Sneaky",
            email,
            password = StrongPassword,
            confirmPassword = StrongPassword,
            role = "Admin",
            isAdmin = true,
            isOrganizer = true
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var auth = await ReadAuthAsync(response);
        Assert.Equal(new[] { "Visitor" }, auth.User.Roles);
        Assert.DoesNotContain("Admin", auth.User.Roles);
    }

    [Theory]
    [InlineData("Abdullah \U0001F600")]
    [InlineData("Test \U0001F680")]
    [InlineData("User \u2764\uFE0F")]
    [InlineData("Company \U0001F44D\U0001F3FD")]
    public async Task Emoji_in_name_is_rejected(string name)
    {
        var client = NewClient();
        var response = await client.PostAsJsonAsync("/api/auth/register",
            new { name, email = UniqueEmail(), password = StrongPassword, confirmPassword = StrongPassword });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.True(body!.Errors!.ContainsKey("name"));
    }

    [Theory]
    [InlineData("Abdullah")]
    [InlineData("Mar\u00EDa Jos\u00E9")]
    [InlineData("\u0627\u0644\u0639\u0628\u062F")]
    [InlineData("\u5F20\u4F1F")]
    public async Task Legitimate_unicode_names_are_accepted(string name)
    {
        var client = NewClient();
        var response = await client.PostAsJsonAsync("/api/auth/register",
            new { name, email = UniqueEmail(), password = StrongPassword, confirmPassword = StrongPassword });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Theory]
    [InlineData("<script>alert(1)</script>")]
    [InlineData("' OR 1=1 --")]
    [InlineData("<img src=x onerror=alert(1)>")]
    public async Task Xss_and_sql_injection_attempts_are_handled_safely(string name)
    {
        var client = NewClient();
        var response = await client.PostAsJsonAsync("/api/auth/register",
            new { name, email = UniqueEmail(), password = StrongPassword, confirmPassword = StrongPassword });

        // Should either succeed (name accepted as plain text) or fail with validation
        // but must never return 500 or expose internals
        Assert.True(response.StatusCode == HttpStatusCode.Created ||
                    response.StatusCode == HttpStatusCode.BadRequest);
    }
}
