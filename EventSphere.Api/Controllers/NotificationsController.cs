using EventSphere.Api.Common;
using EventSphere.Api.DTOs;
using EventSphere.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventSphere.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notifications;

    public NotificationsController(INotificationService notifications)
    {
        _notifications = notifications;
    }

    private int? GetUserId()
    {
        var sub = User.FindFirst("sub")?.Value;
        return int.TryParse(sub, out var id) ? id : null;
    }

    /// <summary>List the authenticated user's notifications (paged, newest first).</summary>
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var notifications = await _notifications.GetMyNotificationsAsync(userId.Value, page, pageSize);
        return Ok(ApiResponse<List<NotificationDto>>.Ok(notifications));
    }

    /// <summary>Get unread notification count.</summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var count = await _notifications.GetUnreadCountAsync(userId.Value);
        return Ok(ApiResponse<object>.Ok(new { count }));
    }

    /// <summary>Mark a single notification as read (owner only).</summary>
    [HttpPatch("{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var marked = await _notifications.MarkReadAsync(id, userId.Value);
        if (!marked) return NotFound(ApiResponse.Fail("Notification not found."));

        return Ok(ApiResponse.Ok("Notification marked as read."));
    }

    /// <summary>Mark a single notification as unread (owner only).</summary>
    [HttpPatch("{id:int}/unread")]
    public async Task<IActionResult> MarkUnread(int id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var marked = await _notifications.MarkUnreadAsync(id, userId.Value);
        if (!marked) return NotFound(ApiResponse.Fail("Notification not found."));

        return Ok(ApiResponse.Ok("Notification marked as unread."));
    }

    /// <summary>Mark all of the user's notifications as read.</summary>
    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var count = await _notifications.MarkAllReadAsync(userId.Value);
        return Ok(ApiResponse<object>.Ok(new { markedCount = count }));
    }
}