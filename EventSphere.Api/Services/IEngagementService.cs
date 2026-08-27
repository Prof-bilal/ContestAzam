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

    // Certificates
    Task<List<CertificateDto>> GetMyCertificatesAsync(int userId);

    // Waitlist
    Task<bool> JoinWaitlistAsync(int userId, int eventId);
    Task<bool> LeaveWaitlistAsync(int userId, int eventId);

    // Calendar
    Task<CalendarEventDto?> GetEventForCalendarAsync(int eventId);
}
