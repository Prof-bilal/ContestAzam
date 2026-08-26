using EventSphere.Api.Models;

namespace EventSphere.Api.Services;

public interface ITokenService
{
    /// <summary>
    /// Builds a signed, short-lived JWT access token containing only the claims
    /// required for authorization (sub, email, jti, name, and one "role" claim per role).
    /// </summary>
    (string Token, DateTime ExpiresAtUtc) GenerateAccessToken(AppUser user, IEnumerable<string> roles, string displayName);
}
