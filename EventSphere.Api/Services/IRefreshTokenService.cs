namespace EventSphere.Api.Services;

public enum RefreshOutcome
{
    Success,
    Invalid,   // no matching token
    Expired,   // matched but past expiry
    Reuse      // matched but already rotated/revoked -> family compromised
}

/// <summary>Result of a rotation attempt. NewRawToken is set only on Success.</summary>
public record RefreshRotationResult(
    RefreshOutcome Outcome,
    int UserId = 0,
    string? NewRawToken = null,
    DateTime? NewExpiresAtUtc = null);

public interface IRefreshTokenService
{
    /// <summary>Issues a brand-new refresh token (new family) for a user. Returns the raw token to send to the client.</summary>
    Task<(string RawToken, DateTime ExpiresAtUtc)> IssueAsync(int userId, string? ip, CancellationToken ct = default);

    /// <summary>
    /// Validates and rotates a refresh token: checks existence, expiry, and revocation;
    /// on reuse of an already-rotated token, revokes the entire token family.
    /// </summary>
    Task<RefreshRotationResult> RotateAsync(string rawToken, string? ip, CancellationToken ct = default);

    /// <summary>Revokes a single active refresh token (used by logout). Idempotent.</summary>
    Task RevokeAsync(string rawToken, CancellationToken ct = default);

    /// <summary>Revokes all active refresh tokens for a user (used after password reset). Idempotent.</summary>
    Task RevokeAllForUserAsync(int userId, CancellationToken ct = default);
}
