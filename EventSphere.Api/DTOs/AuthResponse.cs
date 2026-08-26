namespace EventSphere.Api.DTOs;

/// <summary>Safe, public view of a user. Never includes password hash, security stamp, or tokens.</summary>
public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Returned on successful login/refresh. The refresh token is NOT in this body —
/// it is delivered as an HttpOnly cookie to reduce XSS exposure.
/// </summary>
public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAtUtc { get; set; }
    public UserDto User { get; set; } = new();
}
