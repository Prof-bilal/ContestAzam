using EventSphere.Api.Data;
using EventSphere.Api.DTOs;
using EventSphere.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EventSphere.Api.Services;

public class EventService : IEventService
{
    private readonly AppDbContext _db;

    public EventService(AppDbContext db)
    {
        _db = db;
    }

    // ───────────────────────────── Public ─────────────────────────────

    public async Task<EventSummaryDto?> GetByIdAsync(int eventId, int? currentUserId)
    {
        var evt = await _db.Events
            .Include(e => e.Category)
            .Include(e => e.Organizer).ThenInclude(u => u.UserDetails)
            .Include(e => e.Registrations)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (evt is null) return null;

        return MapToSummary(evt, currentUserId);
    }

    public async Task<(List<EventSummaryDto> Events, int TotalCount)> GetEventsAsync(EventQueryParams query)
    {
        var q = _db.Events
            .Include(e => e.Category)
            .Include(e => e.Organizer).ThenInclude(u => u.UserDetails)
            .Include(e => e.Registrations)
            .Where(e => e.Status == EventStatus.Approved)
            .AsQueryable();

        q = ApplyFilters(q, query);
        q = ApplySorting(q, query.SortBy, query.SortOrder);

        var total = await q.CountAsync();
        var events = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.EffectivePageSize())
            .ToListAsync();

        return (events.Select(e => MapToSummary(e)).ToList(), total);
    }

    public async Task<List<EventCategoryDto>> GetCategoriesAsync()
    {
        return await _db.EventCategories
            .Include(c => c.Events)
            .Select(c => new EventCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                EventCount = c.Events.Count(e => e.Status == EventStatus.Approved)
            })
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    // ───────────────────────────── Organizer CRUD ─────────────────────────────

    public async Task<EventSummaryDto> CreateAsync(CreateEventRequest request, int organizerId)
    {
        var evt = new Event
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            CategoryId = request.CategoryId,
            EventDate = request.EventDate,
            EventTime = request.EventTime,
            Venue = request.Venue?.Trim(),
            OrganizerId = organizerId,
            MaxParticipants = request.MaxParticipants,
            ImageUrl = request.ImageUrl?.Trim(),
            RegistrationDeadline = request.RegistrationDeadline,
            Status = request.SaveAsDraft ? EventStatus.Draft : EventStatus.PendingApproval,
            CreatedAt = DateTime.UtcNow
        };

        _db.Events.Add(evt);
        await _db.SaveChangesAsync();

        // Reload with navigation properties
        return (await GetByIdAsync(evt.Id, organizerId))!;
    }

    public async Task<EventSummaryDto?> UpdateAsync(int eventId, UpdateEventRequest request, int organizerId, bool isAdmin)
    {
        var evt = await _db.Events.FindAsync(eventId);
        if (evt is null) return null;

        if (!isAdmin && evt.OrganizerId != organizerId) return null;
        if (!isAdmin && evt.Status != EventStatus.Draft && evt.Status != EventStatus.PendingApproval)
            return null;

        evt.Title = request.Title.Trim();
        evt.Description = request.Description?.Trim();
        evt.CategoryId = request.CategoryId;
        evt.EventDate = request.EventDate;
        evt.EventTime = request.EventTime;
        evt.Venue = request.Venue?.Trim();
        evt.MaxParticipants = request.MaxParticipants;
        evt.ImageUrl = request.ImageUrl?.Trim();
        evt.RegistrationDeadline = request.RegistrationDeadline;
        evt.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return (await GetByIdAsync(eventId, organizerId))!;
    }

    public async Task<bool> DeleteAsync(int eventId, int organizerId, bool isAdmin)
    {
        var evt = await _db.Events.FindAsync(eventId);
        if (evt is null) return false;

        if (!isAdmin && evt.OrganizerId != organizerId) return false;

        // Organizer can only delete Draft events. Admin can delete any.
        if (!isAdmin && evt.Status != EventStatus.Draft) return false;

        _db.Events.Remove(evt);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PublishAsync(int eventId, int organizerId, bool isAdmin)
    {
        var evt = await _db.Events.FindAsync(eventId);
        if (evt is null) return false;

        if (!isAdmin && evt.OrganizerId != organizerId) return false;

        // Draft → PendingApproval (organizer publishes for admin review)
        if (evt.Status != EventStatus.Draft) return false;

        evt.Status = EventStatus.PendingApproval;
        evt.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelAsync(int eventId, int organizerId, bool isAdmin)
    {
        var evt = await _db.Events.FindAsync(eventId);
        if (evt is null) return false;

        if (!isAdmin && evt.OrganizerId != organizerId) return false;

        // Can cancel Draft, PendingApproval, or Approved events
        if (evt.Status == EventStatus.Cancelled || evt.Status == EventStatus.Completed || evt.Status == EventStatus.Rejected)
            return false;

        evt.Status = EventStatus.Cancelled;
        evt.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    // ───────────────────────────── Organizer Views ─────────────────────────────

    public async Task<List<EventSummaryDto>> GetOrganizerEventsAsync(int organizerId, EventQueryParams query)
    {
        var q = _db.Events
            .Include(e => e.Category)
            .Include(e => e.Organizer).ThenInclude(u => u.UserDetails)
            .Include(e => e.Registrations)
            .Where(e => e.OrganizerId == organizerId)
            .AsQueryable();

        // Organizer can filter by status
        if (!string.IsNullOrWhiteSpace(query.Status) &&
            Enum.TryParse<EventStatus>(query.Status, true, out var statusFilter))
        {
            q = q.Where(e => e.Status == statusFilter);
        }

        if (query.CategoryId.HasValue)
            q = q.Where(e => e.CategoryId == query.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            q = q.Where(e => e.Title.ToLower().Contains(search) ||
                             (e.Description != null && e.Description.ToLower().Contains(search)));
        }

        q = ApplySorting(q, query.SortBy, query.SortOrder);

        var events = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.EffectivePageSize())
            .ToListAsync();

        return events.Select(e => MapToSummary(e, organizerId)).ToList();
    }

    public async Task<OrganizerEventStatsDto> GetOrganizerStatsAsync(int organizerId)
    {
        var events = await _db.Events
            .Include(e => e.Registrations)
            .Where(e => e.OrganizerId == organizerId)
            .ToListAsync();

        return new OrganizerEventStatsDto
        {
            TotalEvents = events.Count,
            DraftEvents = events.Count(e => e.Status == EventStatus.Draft),
            PendingEvents = events.Count(e => e.Status == EventStatus.PendingApproval),
            ApprovedEvents = events.Count(e => e.Status == EventStatus.Approved),
            RejectedEvents = events.Count(e => e.Status == EventStatus.Rejected),
            CancelledEvents = events.Count(e => e.Status == EventStatus.Cancelled),
            CompletedEvents = events.Count(e => e.Status == EventStatus.Completed),
            TotalRegistrations = events.Sum(e => e.Registrations.Count(r => r.Status == RegistrationStatus.Confirmed))
        };
    }

    public async Task<List<EventSummaryDto>> GetCalendarEventsAsync(int organizerId, CalendarQueryParams query)
    {
        var from = query.FromDate ?? DateTime.UtcNow;
        var to = query.ToDate ?? from.AddMonths(3);

        var events = await _db.Events
            .Include(e => e.Category)
            .Include(e => e.Organizer).ThenInclude(u => u.UserDetails)
            .Include(e => e.Registrations)
            .Where(e => e.OrganizerId == organizerId &&
                        e.Status == EventStatus.Approved &&
                        e.EventDate >= from && e.EventDate <= to)
            .OrderBy(e => e.EventDate)
            .ToListAsync();

        return events.Select(e => MapToSummary(e, organizerId)).ToList();
    }

    // ───────────────────────────── Admin ─────────────────────────────

    public async Task<(List<AdminEventDto> Events, int TotalCount)> GetAdminEventsAsync(EventQueryParams query)
    {
        var q = _db.Events
            .Include(e => e.Category)
            .Include(e => e.Organizer).ThenInclude(u => u.UserDetails)
            .Include(e => e.Registrations)
            .AsQueryable();

        q = ApplyFilters(q, query);

        // Admin can filter by status (default: all)
        if (!string.IsNullOrWhiteSpace(query.Status) &&
            Enum.TryParse<EventStatus>(query.Status, true, out var statusFilter))
        {
            q = q.Where(e => e.Status == statusFilter);
        }

        q = ApplySorting(q, query.SortBy, query.SortOrder);

        var total = await q.CountAsync();
        var events = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.EffectivePageSize())
            .ToListAsync();

        var dtos = events.Select(e => new AdminEventDto
        {
            Id = e.Id,
            Title = e.Title,
            CategoryName = e.Category.Name,
            EventDate = e.EventDate,
            Venue = e.Venue,
            OrganizerName = e.Organizer.UserDetails?.FullName ?? e.Organizer.Email ?? "",
            OrganizerEmail = e.Organizer.Email ?? "",
            MaxParticipants = e.MaxParticipants,
            RegisteredCount = e.Registrations.Count(r => r.Status == RegistrationStatus.Confirmed),
            Status = e.Status.ToString(),
            CreatedAt = e.CreatedAt
        }).ToList();

        return (dtos, total);
    }

    public async Task<bool> ApproveEventAsync(int eventId)
    {
        var evt = await _db.Events.FindAsync(eventId);
        if (evt is null || evt.Status != EventStatus.PendingApproval) return false;

        evt.Status = EventStatus.Approved;
        evt.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RejectEventAsync(int eventId, string? reason)
    {
        var evt = await _db.Events.FindAsync(eventId);
        if (evt is null || evt.Status != EventStatus.PendingApproval) return false;

        evt.Status = EventStatus.Rejected;
        evt.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    // ───────────────────────────── Category Management ─────────────────────────────

    public async Task<List<EventCategoryDto>> GetAdminCategoriesAsync()
    {
        return await _db.EventCategories
            .Include(c => c.Events)
            .Select(c => new EventCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                EventCount = c.Events.Count
            })
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<EventCategoryDto> CreateCategoryAsync(CreateCategoryRequest request)
    {
        var category = new EventCategory
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim()
        };

        _db.EventCategories.Add(category);
        await _db.SaveChangesAsync();

        return new EventCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            EventCount = 0
        };
    }

    public async Task<EventCategoryDto?> UpdateCategoryAsync(int categoryId, UpdateCategoryRequest request)
    {
        var category = await _db.EventCategories.FindAsync(categoryId);
        if (category is null) return null;

        category.Name = request.Name.Trim();
        category.Description = request.Description?.Trim();

        await _db.SaveChangesAsync();

        var eventCount = await _db.Events.CountAsync(e => e.CategoryId == categoryId);

        return new EventCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            EventCount = eventCount
        };
    }

    public async Task<bool> DeleteCategoryAsync(int categoryId)
    {
        var category = await _db.EventCategories.FindAsync(categoryId);
        if (category is null) return false;

        var hasEvents = await _db.Events.AnyAsync(e => e.CategoryId == categoryId);
        if (hasEvents) return false;

        _db.EventCategories.Remove(category);
        await _db.SaveChangesAsync();
        return true;
    }

    // ───────────────────────────── Helpers ─────────────────────────────

    private static IQueryable<Event> ApplyFilters(IQueryable<Event> q, EventQueryParams query)
    {
        if (query.CategoryId.HasValue)
            q = q.Where(e => e.CategoryId == query.CategoryId.Value);

        if (query.FromDate.HasValue)
            q = q.Where(e => e.EventDate >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            q = q.Where(e => e.EventDate <= query.ToDate.Value);

        if (!string.IsNullOrWhiteSpace(query.Location))
        {
            var loc = query.Location.Trim().ToLower();
            q = q.Where(e => e.Venue != null && e.Venue.ToLower().Contains(loc));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            q = q.Where(e => e.Title.ToLower().Contains(search) ||
                             (e.Description != null && e.Description.ToLower().Contains(search)) ||
                             (e.Venue != null && e.Venue.ToLower().Contains(search)));
        }

        return q;
    }

    private static IQueryable<Event> ApplySorting(IQueryable<Event> q, string? sortBy, string? sortOrder)
    {
        var desc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy?.ToLower() switch
        {
            "title" => desc ? q.OrderByDescending(e => e.Title) : q.OrderBy(e => e.Title),
            "venue" => desc ? q.OrderByDescending(e => e.Venue) : q.OrderBy(e => e.Venue),
            "createdat" => desc ? q.OrderByDescending(e => e.CreatedAt) : q.OrderBy(e => e.CreatedAt),
            "status" => desc ? q.OrderByDescending(e => e.Status) : q.OrderBy(e => e.Status),
            _ => desc ? q.OrderByDescending(e => e.EventDate) : q.OrderBy(e => e.EventDate)
        };
    }

    private static EventSummaryDto MapToSummary(Event e, int? currentUserId = null)
    {
        return new EventSummaryDto
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            CategoryId = e.CategoryId,
            CategoryName = e.Category?.Name ?? "",
            EventDate = e.EventDate,
            EventTime = e.EventTime,
            Venue = e.Venue,
            OrganizerId = e.OrganizerId,
            OrganizerName = e.Organizer?.UserDetails?.FullName ?? e.Organizer?.Email ?? "",
            MaxParticipants = e.MaxParticipants,
            RegisteredCount = e.Registrations?.Count(r => r.Status == RegistrationStatus.Confirmed) ?? 0,
            Status = e.Status.ToString(),
            ImageUrl = e.ImageUrl,
            RegistrationDeadline = e.RegistrationDeadline,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt,
            IsRegistered = currentUserId.HasValue && e.Registrations != null &&
                           e.Registrations.Any(r => r.StudentId == currentUserId.Value && r.Status == RegistrationStatus.Confirmed)
        };
    }
}
