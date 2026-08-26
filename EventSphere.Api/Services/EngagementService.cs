using EventSphere.Api.Data;
using EventSphere.Api.DTOs;
using EventSphere.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EventSphere.Api.Services;

public class EngagementService : IEngagementService
{
    private readonly AppDbContext _db;

    public EngagementService(AppDbContext db)
    {
        _db = db;
    }

    // ───────────────────────────── Registrations ─────────────────────────────

    public async Task<List<RegistrationDto>> GetMyRegistrationsAsync(int userId)
    {
        return await _db.Registrations
            .Include(r => r.Event)
            .Where(r => r.StudentId == userId)
            .OrderByDescending(r => r.RegisteredOn)
            .Select(r => new RegistrationDto
            {
                Id = r.Id,
                EventId = r.EventId,
                EventTitle = r.Event.Title,
                EventDate = r.Event.EventDate,
                EventTime = r.Event.EventTime,
                EventVenue = r.Event.Venue,
                Status = r.Status.ToString(),
                RegisteredOn = r.RegisteredOn
            })
            .ToListAsync();
    }

    public async Task<bool> CancelRegistrationByIdAsync(int registrationId, int userId)
    {
        var registration = await _db.Registrations.FindAsync(registrationId);
        if (registration is null || registration.StudentId != userId) return false;
        if (registration.Status == RegistrationStatus.Cancelled) return false;

        registration.Status = RegistrationStatus.Cancelled;
        await _db.SaveChangesAsync();
        return true;
    }

    // ───────────────────────────── Favorites ─────────────────────────────

    public async Task<bool> AddFavoriteAsync(int userId, int eventId)
    {
        var exists = await _db.Favorites.AnyAsync(f => f.UserId == userId && f.EventId == eventId);
        if (exists) return false;

        _db.Favorites.Add(new Favorite { UserId = userId, EventId = eventId });
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveFavoriteAsync(int userId, int eventId)
    {
        var favorite = await _db.Favorites.FirstOrDefaultAsync(f => f.UserId == userId && f.EventId == eventId);
        if (favorite is null) return false;

        _db.Favorites.Remove(favorite);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<FavoriteDto>> GetMyFavoritesAsync(int userId)
    {
        return await _db.Favorites
            .Include(f => f.Event).ThenInclude(e => e.Category)
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new FavoriteDto
            {
                EventId = f.EventId,
                EventTitle = f.Event.Title,
                EventDate = f.Event.EventDate,
                EventVenue = f.Event.Venue,
                CategoryName = f.Event.Category.Name,
                BookmarkedOn = f.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<bool> IsFavoritedAsync(int userId, int eventId)
    {
        return await _db.Favorites.AnyAsync(f => f.UserId == userId && f.EventId == eventId);
    }

    // ───────────────────────────── Reviews ─────────────────────────────

    public async Task<ReviewDto?> SubmitReviewAsync(int userId, int eventId, SubmitReviewRequest request)
    {
        // Check if user already reviewed this event
        var existing = await _db.Feedbacks.FirstOrDefaultAsync(f => f.EventId == eventId && f.StudentId == userId);
        if (existing is not null)
        {
            // Update existing review
            existing.Rating = request.Rating;
            existing.Comments = request.Comment?.Trim();
            existing.SubmittedOn = DateTime.UtcNow;
        }
        else
        {
            var feedback = new Feedback
            {
                EventId = eventId,
                StudentId = userId,
                Rating = request.Rating,
                Comments = request.Comment?.Trim(),
                SubmittedOn = DateTime.UtcNow
            };
            _db.Feedbacks.Add(feedback);
        }

        await _db.SaveChangesAsync();

        // Return the submitted review
        var review = await _db.Feedbacks
            .Include(f => f.Student).ThenInclude(u => u.UserDetails)
            .FirstOrDefaultAsync(f => f.EventId == eventId && f.StudentId == userId);

        if (review is null) return null;

        return new ReviewDto
        {
            Id = review.Id,
            EventId = review.EventId,
            UserId = review.StudentId,
            UserName = review.Student.UserDetails?.FullName ?? review.Student.Email ?? "",
            Rating = review.Rating,
            Comment = review.Comments,
            SubmittedOn = review.SubmittedOn
        };
    }

    public async Task<bool> DeleteReviewAsync(int reviewId, int userId)
    {
        var review = await _db.Feedbacks.FindAsync(reviewId);
        if (review is null || review.StudentId != userId) return false;

        _db.Feedbacks.Remove(review);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<EventReviewSummaryDto> GetEventReviewsAsync(int eventId, int? currentUserId)
    {
        var reviews = await _db.Feedbacks
            .Include(f => f.Student).ThenInclude(u => u.UserDetails)
            .Where(f => f.EventId == eventId)
            .OrderByDescending(f => f.SubmittedOn)
            .ToListAsync();

        var avg = reviews.Count > 0 ? reviews.Average(r => r.Rating) : 0;

        return new EventReviewSummaryDto
        {
            AverageRating = Math.Round(avg, 1),
            TotalReviews = reviews.Count,
            Reviews = reviews.Select(r => new ReviewDto
            {
                Id = r.Id,
                EventId = r.EventId,
                UserId = r.StudentId,
                UserName = r.Student.UserDetails?.FullName ?? r.Student.Email ?? "",
                Rating = r.Rating,
                Comment = r.Comments,
                SubmittedOn = r.SubmittedOn
            }).ToList()
        };
    }

    // ───────────────────────────── Notifications ─────────────────────────────

    public async Task<List<NotificationDto>> GetMyNotificationsAsync(int userId)
    {
        return await _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<bool> MarkNotificationReadAsync(int notificationId, int userId)
    {
        var notification = await _db.Notifications.FindAsync(notificationId);
        if (notification is null || notification.UserId != userId) return false;

        notification.IsRead = true;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<int> MarkAllNotificationsReadAsync(int userId)
    {
        var unread = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var n in unread)
            n.IsRead = true;

        await _db.SaveChangesAsync();
        return unread.Count;
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    // ───────────────────────────── Notification Helpers ─────────────────────────────

    public async Task SendNotificationAsync(int userId, string title, string? message)
    {
        _db.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }
}
