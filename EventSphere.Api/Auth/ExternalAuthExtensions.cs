using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace EventSphere.Api.Auth;

/// <summary>
/// Registers Google and GitHub as OAuth 2.0 providers using the framework's
/// generic OAuth handler (no provider-specific NuGet packages). Each provider is
/// only registered when its ClientId/ClientSecret are present in configuration.
/// </summary>
public static class ExternalAuthExtensions
{
    public static AuthenticationBuilder AddExternalOAuth(
        this AuthenticationBuilder builder, IConfiguration config)
    {
        // Temporary cookie that persists the external principal between the
        // provider callback and our /external/callback endpoint. SameSite=Lax is
        // safe here because OAuth returns via a top-level GET redirect.
        builder.AddCookie(ExternalAuth.CookieScheme, o =>
        {
            o.Cookie.Name = "es_external";
            o.Cookie.HttpOnly = true;
            o.Cookie.SameSite = SameSiteMode.Lax;
            o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            o.SlidingExpiration = false;
        });

        var google = config.GetSection("Authentication:Google");
        if (!string.IsNullOrWhiteSpace(google["ClientId"]) && !string.IsNullOrWhiteSpace(google["ClientSecret"]))
        {
            builder.AddOAuth(ExternalAuth.Google, o =>
            {
                o.SignInScheme = ExternalAuth.CookieScheme;
                o.ClientId = google["ClientId"]!;
                o.ClientSecret = google["ClientSecret"]!;
                o.CallbackPath = "/signin-google";

                o.AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
                o.TokenEndpoint = "https://oauth2.googleapis.com/token";
                o.UserInformationEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";

                o.Scope.Add("openid");
                o.Scope.Add("profile");
                o.Scope.Add("email");

                o.SaveTokens = false;
                o.UsePkce = true;
                o.CorrelationCookie.SameSite = SameSiteMode.Lax;
                o.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

                o.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                o.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                o.ClaimActions.MapJsonKey("name", "name");
                o.ClaimActions.MapJsonKey(ExternalAuth.EmailVerifiedClaim, "verified_email");

                o.Events = new OAuthEvents
                {
                    OnCreatingTicket = async ctx =>
                    {
                        var json = await FetchJsonAsync(ctx, ctx.Options.UserInformationEndpoint);
                        ctx.RunClaimActions(json.RootElement);
                    }
                };
            });
        }
        else
        {
            Console.WriteLine("[INFO] Google OAuth not configured; provider disabled.");
        }

        var github = config.GetSection("Authentication:GitHub");
        if (!string.IsNullOrWhiteSpace(github["ClientId"]) && !string.IsNullOrWhiteSpace(github["ClientSecret"]))
        {
            builder.AddOAuth(ExternalAuth.GitHub, o =>
            {
                o.SignInScheme = ExternalAuth.CookieScheme;
                o.ClientId = github["ClientId"]!;
                o.ClientSecret = github["ClientSecret"]!;
                o.CallbackPath = "/signin-github";

                o.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
                o.TokenEndpoint = "https://github.com/login/oauth/access_token";
                o.UserInformationEndpoint = "https://api.github.com/user";

                // Minimum scopes: read profile and access verified email addresses.
                o.Scope.Add("read:user");
                o.Scope.Add("user:email");

                o.SaveTokens = false;
                o.CorrelationCookie.SameSite = SameSiteMode.Lax;
                o.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

                o.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                o.ClaimActions.MapJsonKey("name", "name");
                o.ClaimActions.MapJsonKey("urn:github:login", "login");

                o.Events = new OAuthEvents
                {
                    OnCreatingTicket = async ctx =>
                    {
                        var profile = await FetchJsonAsync(ctx, ctx.Options.UserInformationEndpoint);
                        ctx.RunClaimActions(profile.RootElement);

                        // GitHub may withhold email from the profile; resolve the
                        // primary, verified address explicitly. Do not assume it exists.
                        var (email, verified) = await ResolveGitHubEmailAsync(ctx);
                        if (!string.IsNullOrEmpty(email))
                        {
                            ctx.Identity!.AddClaim(new Claim(ClaimTypes.Email, email));
                            ctx.Identity!.AddClaim(new Claim(ExternalAuth.EmailVerifiedClaim, verified ? "true" : "false"));
                        }
                    }
                };
            });
        }
        else
        {
            Console.WriteLine("[INFO] GitHub OAuth not configured; provider disabled.");
        }

        return builder;
    }

    private static async Task<JsonDocument> FetchJsonAsync(OAuthCreatingTicketContext ctx, string endpoint)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ctx.AccessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.UserAgent.ParseAdd("EventSphere");

        using var response = await ctx.Backchannel.SendAsync(
            request, HttpCompletionOption.ResponseHeadersRead, ctx.HttpContext.RequestAborted);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync(ctx.HttpContext.RequestAborted);
        return JsonDocument.Parse(payload);
    }

    private static async Task<(string? Email, bool Verified)> ResolveGitHubEmailAsync(OAuthCreatingTicketContext ctx)
    {
        using var doc = await FetchJsonAsync(ctx, "https://api.github.com/user/emails");
        if (doc.RootElement.ValueKind != JsonValueKind.Array) return (null, false);

        string? firstVerified = null;
        foreach (var e in doc.RootElement.EnumerateArray())
        {
            var email = e.TryGetProperty("email", out var em) ? em.GetString() : null;
            var verified = e.TryGetProperty("verified", out var v) && v.GetBoolean();
            var primary = e.TryGetProperty("primary", out var p) && p.GetBoolean();

            if (email is null || !verified) continue;
            firstVerified ??= email;
            if (primary) return (email, true); // prefer the primary verified address
        }

        return (firstVerified, firstVerified is not null);
    }
}
