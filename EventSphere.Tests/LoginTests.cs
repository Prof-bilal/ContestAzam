using System.Net;
using EventSphere.Api.DTOs;
using Xunit;

namespace EventSphere.Tests;

public class LoginTests : IntegrationTestBase
{
    public LoginTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Correct_credentials_succeed()
    {
        var client = NewClient();
        var email = UniqueEmail();
        (await RegisterAsync(client, email, StrongPassword)).EnsureSuccessStatusCode();

        var response = await LoginAsync(NewClient(), email, StrongPassword);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var auth = await ReadAuthAsync(response);
        Assert.False(string.IsNullOrWhiteSpace(auth.AccessToken));
    }

    [Fact]
    public async Task Wrong_password_returns_generic_401()
    {
        var client = NewClient();
        var email = UniqueEmail();
        (await RegisterAsync(client, email, StrongPassword)).EnsureSuccessStatusCode();

        var response = await LoginAsync(NewClient(), email, "Wr0ng!Passw0rd#X");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Unknown_email_returns_same_401_as_wrong_password()
    {
        var response = await LoginAsync(NewClient(), UniqueEmail("ghost"), StrongPassword);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Empty_fields_are_rejected()
    {
        var response = await LoginAsync(NewClient(), "", "");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Repeated_failures_lock_the_account()
    {
        var client = NewClient();
        var email = UniqueEmail("lock");
        (await RegisterAsync(client, email, StrongPassword)).EnsureSuccessStatusCode();

        // Default policy: 5 failed attempts triggers lockout.
        for (var i = 0; i < 5; i++)
            await LoginAsync(NewClient(), email, "Bad!Passw0rd#999");

        // Even with the CORRECT password, the account is now locked.
        var locked = await LoginAsync(NewClient(), email, StrongPassword);
        Assert.Equal((HttpStatusCode)423, locked.StatusCode);
    }
}
