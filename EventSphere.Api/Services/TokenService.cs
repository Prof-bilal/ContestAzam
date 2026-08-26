using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EventSphere.Api.Common.Options;
using EventSphere.Api.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EventSphere.Api.Services;

public class TokenService : ITokenService
{
    private readonly JwtOptions _jwt;

    public TokenService(IOptions<JwtOptions> jwt) => _jwt = jwt.Value;

    public (string Token, DateTime ExpiresAtUtc) GenerateAccessToken(
        AppUser user, IEnumerable<string> roles, string displayName)
    {
        var nowUtc = DateTime.UtcNow;
        var expiresUtc = nowUtc.AddMinutes(_jwt.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new("name", displayName)
        };

        // One "role" claim per Identity role. These are validated against the
        // signature; a client editing them invalidates the token.
        foreach (var role in roles)
            claims.Add(new Claim("role", role));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: nowUtc,
            expires: expiresUtc,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresUtc);
    }
}
