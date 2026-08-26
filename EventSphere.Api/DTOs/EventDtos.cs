using System.ComponentModel.DataAnnotations;

namespace EventSphere.Api.DTOs;

// ───────────────────────────── Request DTOs ─────────────────────────────

public class CreateEventRequest
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 150 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Description is too long.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Event date is required.")]
    public DateTime EventDate { get; set; }

    [Required(ErrorMessage = "Event time is required.")]
    public TimeSpan EventTime { get; set; }

    [StringLength(100, ErrorMessage = "Venue is too long.")]
    public string? Venue { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Max participants must be at least 1.")]
    public int MaxParticipants { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime? RegistrationDeadline { get; set; }

    /// <summary>Set to true to save as Draft. Default is PendingApproval.</summary>
    public bool SaveAsDraft { get; set; }
}

public class UpdateEventRequest
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 150 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Description is too long.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Event date is required.")]
    public DateTime EventDate { get; set; }

    [Required(ErrorMessage = "Event time is required.")]
    public TimeSpan EventTime { get; set; }

    [StringLength(100, ErrorMessage = "Venue is too long.")]
    public string? Venue { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Max participants must be at least 1.")]
    public int MaxParticipants { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime? RegistrationDeadline { get; set; }
}

public class RejectEventRequest
{
    [StringLength(1000, ErrorMessage = "Rejection reason is too long.")]
    public string? Reason { get; set; }
}

public class CreateCategoryRequest
{
    [Required(ErrorMessage = "Category name is required.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 characters.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Description is too long.")]
    public string? Description { get; set; }
}

public class UpdateCategoryRequest
{
    [Required(ErrorMessage = "Category name is required.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 characters.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Description is too long.")]
    public string? Description { get; set; }
}

// ───────────────────────────── Response DTOs ─────────────────────────────

public class EventSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public TimeSpan EventTime { get; set; }
    public string? Venue { get; set; }
    public int OrganizerId { get; set; }
    public string OrganizerName { get; set; } = string.Empty;
    public int MaxParticipants { get; set; }
    public int RegisteredCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public DateTime? RegistrationDeadline { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsRegistered { get; set; }
}

public class EventDetailDto : EventSummaryDto
{
    public bool IsOrganizer { get; set; }
}

public class EventCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int EventCount { get; set; }
}

public class OrganizerEventStatsDto
{
    public int TotalEvents { get; set; }
    public int DraftEvents { get; set; }
    public int PendingEvents { get; set; }
    public int ApprovedEvents { get; set; }
    public int RejectedEvents { get; set; }
    public int CancelledEvents { get; set; }
    public int CompletedEvents { get; set; }
    public int TotalRegistrations { get; set; }
}

public class AdminEventDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string? Venue { get; set; }
    public string OrganizerName { get; set; } = string.Empty;
    public string OrganizerEmail { get; set; } = string.Empty;
    public int MaxParticipants { get; set; }
    public int RegisteredCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// ───────────────────────────── Query DTOs ─────────────────────────────

public class EventQueryParams
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Location { get; set; }
    public string? SortBy { get; set; } = "EventDate";
    public string? SortOrder { get; set; } = "asc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;

    /// <summary>Clamp PageSize to prevent abuse.</summary>
    public int EffectivePageSize() => Math.Clamp(PageSize, 1, 50);
}

public class CalendarQueryParams
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
