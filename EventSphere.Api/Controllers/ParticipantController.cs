using EventSphere.Api.Common;
using EventSphere.Api.DTOs;
using EventSphere.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventSphere.Api.Controllers;

[ApiController]
[Route("api/participant")]
[Authorize]
public class ParticipantController : ControllerBase
{
    private readonly IEngagementService _engagement;
    private readonly ILogger<ParticipantController> _logger;

    public ParticipantController(IEngagementService engagement, ILogger<ParticipantController> logger)
    {
        _engagement = engagement;
        _logger = logger;
    }

    private int? GetUserId()
    {
        var sub = User.FindFirst("sub")?.Value;
        return int.TryParse(sub, out var id) ? id : null;
    }

    // ───────────────────────────── Registrations ─────────────────────────────

    /// <summary>Get the authenticated user's registration history.</summary>
    [HttpGet("registrations")]
    public async Task<IActionResult> GetMyRegistrations()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var registrations = await _engagement.GetMyRegistrationsAsync(userId.Value);
        return Ok(ApiResponse<List<RegistrationDto>>.Ok(registrations));
    }

    /// <summary>Cancel a registration by registration ID.</summary>
    [HttpDelete("registrations/{id:int}")]
    public async Task<IActionResult> CancelRegistration(int id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var cancelled = await _engagement.CancelRegistrationByIdAsync(id, userId.Value);
        if (!cancelled) return NotFound(ApiResponse.Fail("Registration not found or already cancelled."));

        return Ok(ApiResponse.Ok("Registration cancelled."));
    }

    // ───────────────────────────── Favorites ─────────────────────────────

    /// <summary>Bookmark an event.</summary>
    [HttpPost("favorites/{eventId:int}")]
    public async Task<IActionResult> AddFavorite(int eventId)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var added = await _engagement.AddFavoriteAsync(userId.Value, eventId);
        if (!added) return Conflict(ApiResponse.Fail("Event already bookmarked."));

        return Ok(ApiResponse.Ok("Event bookmarked."));
    }

    /// <summary>Remove a bookmark.</summary>
    [HttpDelete("favorites/{eventId:int}")]
    public async Task<IActionResult> RemoveFavorite(int eventId)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var removed = await _engagement.RemoveFavoriteAsync(userId.Value, eventId);
        if (!removed) return NotFound(ApiResponse.Fail("Bookmark not found."));

        return Ok(ApiResponse.Ok("Bookmark removed."));
    }

    /// <summary>List my bookmarked events.</summary>
    [HttpGet("favorites")]
    public async Task<IActionResult> GetMyFavorites()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var favorites = await _engagement.GetMyFavoritesAsync(userId.Value);
        return Ok(ApiResponse<List<FavoriteDto>>.Ok(favorites));
    }

    // ───────────────────────────── Reviews ─────────────────────────────

    /// <summary>Delete my own review.</summary>
    [HttpDelete("reviews/{id:int}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var deleted = await _engagement.DeleteReviewAsync(id, userId.Value);
        if (!deleted) return NotFound(ApiResponse.Fail("Review not found or you do not have permission."));

        return Ok(ApiResponse.Ok("Review deleted."));
    }

    // ───────────────────────────── Notifications ─────────────────────────────

    /// <summary>List my notifications.</summary>
    [HttpGet("notifications")]
    public async Task<IActionResult> GetMyNotifications()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var notifications = await _engagement.GetMyNotificationsAsync(userId.Value);
        return Ok(ApiResponse<List<NotificationDto>>.Ok(notifications));
    }

    /// <summary>Get unread notification count.</summary>
    [HttpGet("notifications/unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var count = await _engagement.GetUnreadCountAsync(userId.Value);
        return Ok(ApiResponse<object>.Ok(new { count }));
    }

    /// <summary>Mark a notification as read.</summary>
    [HttpPatch("notifications/{id:int}/read")]
    public async Task<IActionResult> MarkNotificationRead(int id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var marked = await _engagement.MarkNotificationReadAsync(id, userId.Value);
        if (!marked) return NotFound(ApiResponse.Fail("Notification not found."));

        return Ok(ApiResponse.Ok("Notification marked as read."));
    }

    /// <summary>Mark all notifications as read.</summary>
    [HttpPatch("notifications/read-all")]
    public async Task<IActionResult> MarkAllNotificationsRead()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var count = await _engagement.MarkAllNotificationsReadAsync(userId.Value);
        return Ok(ApiResponse<object>.Ok(new { markedCount = count }));
    }
}
