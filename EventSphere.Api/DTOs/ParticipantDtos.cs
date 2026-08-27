using System.ComponentModel.DataAnnotations;

namespace EventSphere.Api.DTOs;

// ───────────────────────────── Registration DTOs ─────────────────────────────

public class RegistrationDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public TimeSpan EventTime { get; set; }
    public string? EventVenue { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RegisteredOn { get; set; }
}

public class AttendeeDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? EnrollmentNo { get; set; }
    public DateTime RegisteredOn { get; set; }
    public bool Attended { get; set; }
    public DateTime? CheckedInAt { get; set; }
}

// ───────────────────────────── Review DTOs ─────────────────────────────

public class SubmitReviewRequest
{
    [Required(ErrorMessage = "Rating is required.")]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public int Rating { get; set; }

    [StringLength(1000, ErrorMessage = "Comment is too long.")]
    public string? Comment { get; set; }
}

public class ReviewDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime SubmittedOn { get; set; }
}

public class EventReviewSummaryDto
{
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public List<ReviewDto> Reviews { get; set; } = new();
}

// ───────────────────────────── Favorite DTOs ─────────────────────────────

public class FavoriteDto
{
    public int EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string? EventVenue { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public DateTime BookmarkedOn { get; set; }
}

// ───────────────────────────── Notification DTOs ─────────────────────────────

public class NotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string Type { get; set; } = string.Empty;
    public int? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
    public string? ActionUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

// ───────────────────────────── Payment DTOs ─────────────────────────────

public class CreateCheckoutRequest
{
    [Required]
    public int EventId { get; set; }
}

public class PaymentSessionDto
{
    public string SessionId { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class PaymentDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
}

// ───────────────────────────── Digital Pass DTOs ─────────────────────────────

public class DigitalPassDto
{
    public int RegistrationId { get; set; }
    public int EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public TimeSpan EventTime { get; set; }
    public string Venue { get; set; } = string.Empty;
    public string ParticipantName { get; set; } = string.Empty;
    public string QrCodeBase64 { get; set; } = string.Empty;
    public string CheckInToken { get; set; } = string.Empty;
}

// ───────────────────────────── Check-In DTOs ─────────────────────────────

public class CheckInRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;
}

public class CheckInResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? AttendeeName { get; set; }
    public string? EventTitle { get; set; }
}

public class AttendanceDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Attended { get; set; }
    public DateTime? CheckedInAt { get; set; }
    public string CheckInMethod { get; set; } = string.Empty;
}

public class AttendanceStatsDto
{
    public int TotalRegistered { get; set; }
    public int TotalCheckedIn { get; set; }
    public int TotalPending { get; set; }
    public double CheckInPercentage { get; set; }
}

// ───────────────────────────── Media DTOs ─────────────────────────────

public class MediaDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string FileType { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public DateTime UploadedOn { get; set; }
}

// ───────────────────────────── Certificate DTOs ─────────────────────────────

public class UploadCertificateRequest
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    [StringLength(500)]
    public string CertificateUrl { get; set; } = string.Empty;

    public bool FeePaid { get; set; }
}

public class CertificateDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public string CertificateUrl { get; set; } = string.Empty;
    public DateTime IssuedOn { get; set; }
    public bool FeePaid { get; set; }
}

// ───────────────────────────── Waitlist DTOs ─────────────────────────────

public class WaitlistEntryDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int EventId { get; set; }
    public DateTime WaitlistTime { get; set; }
    public string Status { get; set; } = string.Empty;
}

// ───────────────────────────── Calendar DTO ─────────────────────────────

public class CalendarEventDto
{
    public int EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public TimeSpan EventTime { get; set; }
    public string? Venue { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}
