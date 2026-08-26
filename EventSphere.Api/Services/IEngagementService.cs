using EventSphere.Api.DTOs;

namespace EventSphere.Api.Services;

public interface IEngagementService
{
    // Registrations
    Task<List<RegistrationDto>> GetMyRegistrationsAsync(int userId);
    Task<bool> CancelRegistrationByIdAsync(int registrationId, int userId);

    // Favorites
    Task<bool> AddFavoriteAsync(int userId, int eventId);
    Task<bool> RemoveFavoriteAsync(int userId, int eventId);
    Task<List<FavoriteDto>> GetMyFavoritesAsync(int userId);
    Task<bool> IsFavoritedAsync(int userId, int eventId);

    // Reviews
    Task<ReviewDto?> SubmitReviewAsync(int userId, int eventId, SubmitReviewRequest request);
    Task<bool> DeleteReviewAsync(int reviewId, int userId);
    Task<EventReviewSummaryDto> GetEventReviewsAsync(int eventId, int? currentUserId);

    // Notifications
    Task<List<NotificationDto>> GetMyNotificationsAsync(int userId);
    Task<bool> MarkNotificationReadAsync(int notificationId, int userId);
    Task<int> MarkAllNotificationsReadAsync(int userId);
    Task<int> GetUnreadCountAsync(int userId);

    // Notification helpers (called by other services)
    Task SendNotificationAsync(int userId, string title, string? message);
}
