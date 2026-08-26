using System.Security.Cryptography;
using System.Text;
using EventSphere.Api.Common.Options;
using EventSphere.Api.Data;
using EventSphere.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EventSphere.Api.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly AppDbContext _db;
    private readonly RefreshTokenOptions _options;

    public RefreshTokenService(AppDbContext db, IOptions<RefreshTokenOptions> options)
    {
        _db = db;
        _options = options.Value;
    }

    public async Task<(string RawToken, DateTime ExpiresAtUtc)> IssueAsync(
        int userId, string? ip, CancellationToken ct = default)
    {
        var (raw, hash) = GenerateToken();
        var now = DateTime.UtcNow;
        var expires = now.AddDays(_options.DaysValid);

        _db.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = hash,
            UserId = userId,
            FamilyId = Guid.NewGuid(),
            CreatedAtUtc = now,
            ExpiresAtUtc = expires,
            CreatedByIp = ip
        });
        await _db.SaveChangesAsync(ct);

        return (raw, expires);
    }

    public async Task<RefreshRotationResult> RotateAsync(string rawToken, string? ip, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
            return new RefreshRotationResult(RefreshOutcome.Invalid);

        var hash = Hash(rawToken);
        var now = DateTime.UtcNow;

        var existing = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);
        if (existing is null)
            return new RefreshRotationResult(RefreshOutcome.Invalid);

        // A token that is present but not active means it was already rotated (revoked)
        // or expired. Presenting an already-rotated token is the classic reuse signal:
        // treat the whole family as compromised and revoke it.
        if (existing.RevokedAtUtc is not null)
        {
            await RevokeFamilyAsync(existing.FamilyId, now, ct);
            return new RefreshRotationResult(RefreshOutcome.Reuse);
        }

        if (existing.IsExpired(now))
            return new RefreshRotationResult(RefreshOutcome.Expired);

        // Rotate: revoke current, issue a successor in the same family.
        var (newRaw, newHash) = GenerateToken();
        var expires = now.AddDays(_options.DaysValid);

        existing.RevokedAtUtc = now;
        existing.ReplacedByTokenHash = newHash;

        _db.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = newHash,
            UserId = existing.UserId,
            FamilyId = existing.FamilyId,
            CreatedAtUtc = now,
            ExpiresAtUtc = expires,
            CreatedByIp = ip
        });

        await _db.SaveChangesAsync(ct);

        return new RefreshRotationResult(RefreshOutcome.Success, existing.UserId, newRaw, expires);
    }

    public async Task RevokeAsync(string rawToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(rawToken)) return;

        var hash = Hash(rawToken);
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);
        if (token is not null && token.RevokedAtUtc is null)
        {
            token.RevokedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task RevokeAllForUserAsync(int userId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var active = await _db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAtUtc == null)
            .ToListAsync(ct);

        foreach (var t in active)
            t.RevokedAtUtc = now;

        if (active.Count > 0)
            await _db.SaveChangesAsync(ct);
    }

    private async Task RevokeFamilyAsync(Guid familyId, DateTime now, CancellationToken ct)
    {
        var active = await _db.RefreshTokens
            .Where(t => t.FamilyId == familyId && t.RevokedAtUtc == null)
            .ToListAsync(ct);

        foreach (var t in active)
            t.RevokedAtUtc = now;

        await _db.SaveChangesAsync(ct);
    }

    private static (string Raw, string Hash) GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var raw = Base64UrlEncode(bytes);
        return (raw, Hash(raw));
    }

    /// <summary>SHA-256, Base64-encoded. Only the hash is ever persisted.</summary>
    private static string Hash(string raw)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToBase64String(bytes);
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
