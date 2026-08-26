using EventSphere.Api.DTOs;
using EventSphere.Api.Models;

namespace EventSphere.Api.Services;

public interface IEventService
{
    Task<EventSummaryDto?> GetByIdAsync(int eventId, int? currentUserId);
    Task<(List<EventSummaryDto> Events, int TotalCount)> GetEventsAsync(EventQueryParams query);
    Task<List<EventCategoryDto>> GetCategoriesAsync();
    Task<EventSummaryDto> CreateAsync(CreateEventRequest request, int organizerId);
    Task<EventSummaryDto?> UpdateAsync(int eventId, UpdateEventRequest request, int organizerId, bool isAdmin);
    Task<bool> DeleteAsync(int eventId, int organizerId, bool isAdmin);
    Task<bool> PublishAsync(int eventId, int organizerId, bool isAdmin);
    Task<bool> CancelAsync(int eventId, int organizerId, bool isAdmin);
    Task<List<EventSummaryDto>> GetOrganizerEventsAsync(int organizerId, EventQueryParams query);
    Task<OrganizerEventStatsDto> GetOrganizerStatsAsync(int organizerId);
    Task<List<EventSummaryDto>> GetCalendarEventsAsync(int organizerId, CalendarQueryParams query);
    Task<(List<AdminEventDto> Events, int TotalCount)> GetAdminEventsAsync(EventQueryParams query);
    Task<bool> ApproveEventAsync(int eventId);
    Task<bool> RejectEventAsync(int eventId, string? reason);

    // Category management (admin)
    Task<List<EventCategoryDto>> GetAdminCategoriesAsync();
    Task<EventCategoryDto> CreateCategoryAsync(CreateCategoryRequest request);
    Task<EventCategoryDto?> UpdateCategoryAsync(int categoryId, UpdateCategoryRequest request);
    Task<bool> DeleteCategoryAsync(int categoryId);
}
