namespace EventSphere.Api.Auth;

/// <summary>Names and claim keys used by the external (OAuth) authentication flow.</summary>
public static class ExternalAuth
{
    /// <summary>Temporary cookie scheme that carries the external identity through the OAuth handshake.</summary>
    public const string CookieScheme = "External";

    public const string Google = "Google";
    public const string GitHub = "GitHub";

    /// <summary>Custom claim recording whether the provider asserts the email is verified ("true"/"false").</summary>
    public const string EmailVerifiedClaim = "email_verified";
}
