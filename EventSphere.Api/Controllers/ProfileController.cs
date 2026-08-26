using System.Security.Claims;
using EventSphere.Api.Common;
using EventSphere.Api.Data;
using EventSphere.Api.DTOs;
using EventSphere.Api.Models;
using EventSphere.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventSphere.Api.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _db;
    private readonly IRefreshTokenService _refreshService;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(
        UserManager<AppUser> userManager,
        AppDbContext db,
        IRefreshTokenService refreshService,
        ILogger<ProfileController> logger)
    {
        _userManager = userManager;
        _db = db;
        _refreshService = refreshService;
        _logger = logger;
    }

    /// <summary>Get the authenticated user's full profile.</summary>
    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        if (user is null || !user.IsActive)
            return Unauthorized(ApiResponse.Fail("Invalid session."));

        var fullName = await _db.UserDetails
            .Where(d => d.UserId == userId.Value)
            .Select(d => new { d.FullName, d.Mobile, d.Department, d.EnrollmentNo, d.ProfileImageUrl })
            .FirstOrDefaultAsync();

        var roles = await _userManager.GetRolesAsync(user);
        // Filter to highest-privilege role only for display.
        var displayRoles = FilterDisplayRoles(roles);
        var emailConfirmed = await _userManager.IsEmailConfirmedAsync(user);

        var organizerRequest = await _db.OrganizerRequests
            .Where(r => r.UserId == userId.Value)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new { r.Status, r.OrganizationName })
            .FirstOrDefaultAsync();

        var dto = new ProfileDto
        {
            Id = user.Id,
            Name = fullName?.FullName ?? user.Email!,
            Email = user.Email ?? string.Empty,
            Roles = displayRoles.ToArray(),
            EmailConfirmed = emailConfirmed,
            CreatedAt = user.CreatedAt,
            FullName = fullName?.FullName,
            Mobile = fullName?.Mobile,
            Department = fullName?.Department,
            EnrollmentNo = fullName?.EnrollmentNo,
            ProfileImageUrl = fullName?.ProfileImageUrl,
            OrganizerRequestStatus = organizerRequest?.Status,
            OrganizationName = organizerRequest?.OrganizationName
        };

        return Ok(ApiResponse<ProfileDto>.Ok(dto));
    }

    /// <summary>Update the authenticated user's profile.</summary>
    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        if (user is null || !user.IsActive)
            return Unauthorized(ApiResponse.Fail("Invalid session."));

        var details = await _db.UserDetails.FirstOrDefaultAsync(d => d.UserId == userId.Value);
        if (details is null)
        {
            details = new UserDetails { UserId = userId.Value, FullName = request.FullName.Trim() };
            _db.UserDetails.Add(details);
        }
        else
        {
            details.FullName = request.FullName.Trim();
        }

        details.Mobile = request.Mobile?.Trim();
        details.Department = request.Department?.Trim();
        details.ProfileImageUrl = request.ProfileImageUrl;

        await _db.SaveChangesAsync();

        _logger.LogInformation("User {UserId} updated their profile.", userId.Value);

        return Ok(ApiResponse.Ok("Profile updated successfully."));
    }

    /// <summary>Delete the authenticated user's account. Requires confirmation.</summary>
    [HttpDelete]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        if (user is null || !user.IsActive)
            return Unauthorized(ApiResponse.Fail("Invalid session."));

        // Revoke all refresh tokens for this user.
        await _refreshService.RevokeAllForUserAsync(user.Id);

        // Remove all roles before deactivating.
        var userRoles = await _userManager.GetRolesAsync(user);
        if (userRoles.Count > 0)
            await _userManager.RemoveFromRolesAsync(user, userRoles);

        // Deactivate the user instead of hard-deleting to preserve referential
        // integrity for events, registrations, audit records, etc.
        user.IsActive = false;
        user.Email = $"deleted_{user.Id}_{user.Email}";
        user.UserName = $"deleted_{user.Id}";
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("User {UserId} deleted their account.", userId.Value);

        return Ok(ApiResponse.Ok("Account deleted successfully."));
    }

    private int? GetUserId()
    {
        var idValue = User.FindFirstValue("sub");
        return int.TryParse(idValue, out var userId) ? userId : null;
    }

    /// <summary>
    /// Filters roles for display purposes. Removes Visitor when a higher-privilege
    /// role exists, so the UI only shows the most significant role.
    /// </summary>
    private static IList<string> FilterDisplayRoles(IList<string> roles)
    {
        if (roles.Count <= 1) return roles;
        var filtered = roles.Where(r => r != AppRoles.Visitor).ToList();
        return filtered.Count > 0 ? filtered : roles;
    }
}
