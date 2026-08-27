using EventSphere.Api.Data;
using EventSphere.Api.Hubs;
using EventSphere.Api.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace EventSphere.Api.Services;

/// <summary>
/// Lightweight background reminder sender. Scans upcoming confirmed events once a
/// window and emits EventReminder / EventStartingSoon notifications to registered
/// attendees. It is intentionally simple (no queue/broker) and idempotent: a
/// reminder is only created once per (event, milestone) because the notification
/// record for that milestone already exists.
/// </summary>
public class EventReminderService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(10);

    public EventReminderService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_interval);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDueRemindersAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception)
            {
                // Reminders are best-effort; a failure should not stop the service.
            }

            await timer.WaitForNextTickAsync(stoppingToken);
        }
    }

    private async Task ProcessDueRemindersAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var notifications = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<EventReminderService>>();
        var now = DateTime.UtcNow;

        // 24h and 1h milestones.
        var comingSoon = await db.Events
            .Where(e => e.Status == EventStatus.Approved)
            .ToListAsync(ct);

        foreach (var eventItem in comingSoon)
        {
            var startsAtUtc = DateTime.SpecifyKind(eventItem.EventDate.Add(eventItem.EventTime), DateTimeKind.Utc);
            var delta = startsAtUtc - now;
            var hours = delta.TotalHours;

            if (delta > TimeSpan.Zero && hours <= 24 && hours > 1)
            {
                await SendIfPendingAsync(db, notifications, eventItem, NotificationType.EventReminder,
                    "Event starts soon",
                    $"Don't forget: \"{eventItem.Title}\" starts tomorrow.", ct);
            }
            else if (delta > TimeSpan.Zero && hours <= 1)
            {
                await SendIfPendingAsync(db, notifications, eventItem, NotificationType.EventStartingSoon,
                    "Event starting soon",
                    $"\"{eventItem.Title}\" starts in about {(int)Math.Ceiling(delta.TotalMinutes)} minutes.", ct);
            }
        }
    }

    private static async Task SendIfPendingAsync(
        AppDbContext db,
        INotificationService notifications,
        Event eventItem,
        NotificationType type,
        string title,
        string message,
        CancellationToken ct)
    {
        var alreadySent = await db.Notifications.AnyAsync(n =>
            n.RelatedEntityId == eventItem.Id &&
            n.RelatedEntityType == "Event" &&
            n.Type == type, ct);
        if (alreadySent) return;

        var attendees = await db.Registrations
            .Where(r => r.EventId == eventItem.Id && r.Status == RegistrationStatus.Confirmed)
            .Select(r => r.StudentId)
            .ToListAsync(ct);

        foreach (var userId in attendees)
        {
            await notifications.SendAsync(userId, type, title, message,
                relatedEntityId: eventItem.Id, relatedEntityType: "Event", actionUrl: $"/events/{eventItem.Id}");
        }
    }
}