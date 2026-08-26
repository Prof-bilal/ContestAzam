using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace EventSphere.Tests;

public class TokenTests : IntegrationTestBase
{
    public TokenTests(CustomWebApplicationFactory factory) : base(factory) { }

    private static string MintToken(DateTime expiresUtc, string? key = null)
    {
        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key ?? CustomWebApplicationFactory.TestJwtKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "EventSphere",
            audience: "EventSphere",
            claims: new[] { new Claim("sub", "1"), new Claim("name", "x") },
            notBefore: DateTime.UtcNow.AddMinutes(-10),
            expires: expiresUtc,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private HttpClient BearerClient(string token)
    {
        var client = NewClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private async Task<HttpResponseMessage> RefreshWithCookie(string rawToken)
    {
        var client = NewClient(handleCookies: false);
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        req.Headers.Add("Cookie", $"es_refresh={rawToken}");
        return await client.SendAsync(req);
    }

    // ------------------------------------------------------------- JWT validation
    [Fact]
    public async Task Valid_token_reaches_me_and_returns_safe_fields_only()
    {
        var client = NewClient();
        var email = UniqueEmail();
        var reg = await RegisterAsync(client, email, StrongPassword);
        var auth = await ReadAuthAsync(reg);

        var me = await BearerClient(auth.AccessToken).GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);

        var json = await me.Content.ReadAsStringAsync();
        Assert.DoesNotContain("passwordHash", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("securityStamp", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("refreshToken", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Expired_token_is_rejected()
    {
        var expired = MintToken(DateTime.UtcNow.AddMinutes(-1));
        var response = await BearerClient(expired).GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Token_signed_with_wrong_key_is_rejected()
    {
        var wrong = MintToken(DateTime.UtcNow.AddMinutes(30), "A_COMPLETELY_DIFFERENT_KEY_32_BYTES_LONG_XX");
        var response = await BearerClient(wrong).GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Tampered_token_is_rejected()
    {
        var valid = MintToken(DateTime.UtcNow.AddMinutes(30));
        var tampered = valid[..^2] + (valid[^1] == 'a' ? "bb" : "aa");
        var response = await BearerClient(tampered).GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Garbage_token_is_rejected()
    {
        var response = await BearerClient("not.a.jwt").GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // -------------------------------------------------------- Refresh token lifecycle
    [Fact]
    public async Task Missing_refresh_cookie_returns_401()
    {
        var response = await NewClient(handleCookies: false).PostAsync("/api/auth/refresh", null);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_rotates_the_token()
    {
        var reg = await RegisterAsync(NewClient(handleCookies: false), UniqueEmail(), StrongPassword);
        var r1 = ExtractCookie(reg, "es_refresh");
        Assert.False(string.IsNullOrEmpty(r1));

        var refreshed = await RefreshWithCookie(r1!);
        Assert.Equal(HttpStatusCode.OK, refreshed.StatusCode);

        var r2 = ExtractCookie(refreshed, "es_refresh");
        Assert.False(string.IsNullOrEmpty(r2));
        Assert.NotEqual(r1, r2);
    }

    [Fact]
    public async Task Reusing_a_rotated_refresh_token_is_rejected_and_revokes_family()
    {
        var reg = await RegisterAsync(NewClient(handleCookies: false), UniqueEmail("reuse"), StrongPassword);
        var r1 = ExtractCookie(reg, "es_refresh")!;

        var first = await RefreshWithCookie(r1);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        var r2 = ExtractCookie(first, "es_refresh")!;

        // Replay the already-rotated token -> reuse detected.
        var replay = await RefreshWithCookie(r1);
        Assert.Equal(HttpStatusCode.Unauthorized, replay.StatusCode);

        // The whole family is now revoked, so the latest token no longer works either.
        var afterCompromise = await RefreshWithCookie(r2);
        Assert.Equal(HttpStatusCode.Unauthorized, afterCompromise.StatusCode);
    }

    [Fact]
    public async Task Logout_revokes_the_refresh_token()
    {
        var reg = await RegisterAsync(NewClient(handleCookies: false), UniqueEmail("logout"), StrongPassword);
        var r1 = ExtractCookie(reg, "es_refresh")!;

        var logoutClient = NewClient(handleCookies: false);
        var logoutReq = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");
        logoutReq.Headers.Add("Cookie", $"es_refresh={r1}");
        var logout = await logoutClient.SendAsync(logoutReq);
        Assert.Equal(HttpStatusCode.OK, logout.StatusCode);

        var afterLogout = await RefreshWithCookie(r1);
        Assert.Equal(HttpStatusCode.Unauthorized, afterLogout.StatusCode);
    }
}
