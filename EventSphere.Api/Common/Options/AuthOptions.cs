namespace EventSphere.Api.Common.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;

    /// <summary>Access-token lifetime in minutes. Kept short; refresh tokens provide longevity.</summary>
    public int AccessTokenMinutes { get; set; } = 15;
}

public class RefreshTokenOptions
{
    public const string SectionName = "RefreshToken";

    public int DaysValid { get; set; } = 7;

    /// <summary>Name of the HttpOnly cookie that carries the refresh token.</summary>
    public string CookieName { get; set; } = "es_refresh";

    /// <summary>
    /// SameSite mode for the refresh cookie. "None" is required for cross-site
    /// SPA→API (and forces Secure); "Lax"/"Strict" for same-site deployments.
    /// </summary>
    public string CookieSameSite { get; set; } = "None";
}

public class FrontendOptions
{
    public const string SectionName = "Frontend";

    /// <summary>Origins allowed by CORS. Never use "*". Comes from configuration per environment.</summary>
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();

    /// <summary>Where the backend redirects after a successful OAuth login.</summary>
    public string PostLoginRedirectPath { get; set; } = "/oauth/callback";

    /// <summary>Where the backend redirects after a failed/cancelled OAuth login.</summary>
    public string PostLoginErrorPath { get; set; } = "/login";
}

public class RateLimitOptions
{
    public const string SectionName = "RateLimiting";

    /// <summary>Strict limit for sensitive auth endpoints (register/login/refresh/oauth-init).</summary>
    public int AuthPermitLimit { get; set; } = 10;
    public int AuthWindowSeconds { get; set; } = 60;

    /// <summary>Strict limit for email-related endpoints (forgot-password, resend-verification).</summary>
    public int EmailPermitLimit { get; set; } = 5;
    public int EmailWindowSeconds { get; set; } = 60;

    /// <summary>General limit applied to all other API traffic.</summary>
    public int GeneralPermitLimit { get; set; } = 100;
    public int GeneralWindowSeconds { get; set; } = 60;
}
