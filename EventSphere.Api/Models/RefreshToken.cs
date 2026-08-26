namespace EventSphere.Api.Models;

/// <summary>
/// Server-tracked refresh token. The raw token value is NEVER stored; only a
/// SHA-256 hash is persisted. Supports expiration, rotation, revocation, and
/// token-family reuse detection.
/// </summary>
public class RefreshToken
{
    public int Id { get; set; }

    /// <summary>SHA-256 hash (Base64) of the raw refresh token. The raw value never touches the database.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public int UserId { get; set; }

    /// <summary>
    /// Identifies a chain of rotated tokens descending from one login.
    /// Reusing any already-rotated token in a family revokes the whole family.
    /// </summary>
    public Guid FamilyId { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    /// <summary>Hash of the token that replaced this one during rotation (null until rotated).</summary>
    public string? ReplacedByTokenHash { get; set; }

    /// <summary>Optional audit metadata — never used for authorization.</summary>
    public string? CreatedByIp { get; set; }

    public bool IsExpired(DateTime nowUtc) => nowUtc >= ExpiresAtUtc;

    public bool IsActive(DateTime nowUtc) => RevokedAtUtc is null && !IsExpired(nowUtc);

    // Navigation
    public AppUser User { get; set; } = null!;
}
