using EventSphere.Api.Data;
using EventSphere.Api.DTOs;
using EventSphere.Api.Hubs;
using EventSphere.Api.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace EventSphere.Api.Services;

/// <summary>
/// Centralized notification pipeline: persist → push via SignalR (real-time).
/// Email delivery is a separate, optional channel invoked by business services via
/// IEmailNotificationService so a failure there never blocks the primary operation.
/// Sender identity always derives from the service parameter (the authenticated
/// user id from the JWT), never from the client.
/// </summary>
public interface INotificationService
{
    Task<NotificationDto> SendAsync(int userId, NotificationType type, string title, string? message = null,
        int? relatedEntityId = null, string? relatedEntityType = null, string? actionUrl = null);

    Task<List<NotificationDto>> GetMyNotificationsAsync(int userId, int page = 1, int pageSize = 20);
    Task<int> GetUnreadCountAsync(int userId);
    Task<bool> MarkReadAsync(int notificationId, int userId);
    Task<int> MarkAllReadAsync(int userId);
    Task<bool> MarkUnreadAsync(int notificationId, int userId);
}

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;
    private readonly IHubContext<NotificationsHub> _hub;

    public NotificationService(AppDbContext db, IHubContext<NotificationsHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    public async Task<NotificationDto> SendAsync(int userId, NotificationType type, string title, string? message = null,
        int? relatedEntityId = null, string? relatedEntityType = null, string? actionUrl = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType,
            ActionUrl = actionUrl,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();

        var dto = Map(notification);

        // Only deliver to the owning user's authenticated connection(s).
        await _hub.Clients.User(userId.ToString()).SendAsync("NotificationReceived", dto);

        return dto;
    }

    public async Task<List<NotificationDto>> GetMyNotificationsAsync(int userId, int page = 1, int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        return await _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type.ToString(),
                RelatedEntityId = n.RelatedEntityId,
                RelatedEntityType = n.RelatedEntityType,
                ActionUrl = n.ActionUrl,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                ReadAt = n.ReadAt
            })
            .ToListAsync();
    }

    public Task<int> GetUnreadCountAsync(int userId)
        => _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

    public async Task<bool> MarkReadAsync(int notificationId, int userId)
    {
        var n = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId);
        if (n is null) return false;
        if (!n.IsRead)
        {
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        return true;
    }

    public async Task<bool> MarkUnreadAsync(int notificationId, int userId)
    {
        var n = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId);
        if (n is null) return false;
        if (n.IsRead)
        {
            n.IsRead = false;
            n.ReadAt = null;
            await _db.SaveChangesAsync();
        }
        return true;
    }

    public async Task<int> MarkAllReadAsync(int userId)
    {
        var unread = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var n in unread)
        {
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return unread.Count;
    }

    private static NotificationDto Map(Notification n) => new()
    {
        Id = n.Id,
        Title = n.Title,
        Message = n.Message,
        Type = n.Type.ToString(),
        RelatedEntityId = n.RelatedEntityId,
        RelatedEntityType = n.RelatedEntityType,
        ActionUrl = n.ActionUrl,
        IsRead = n.IsRead,
        CreatedAt = n.CreatedAt,
        ReadAt = n.ReadAt
    };
}