using System.ComponentModel.DataAnnotations;
using EventSphere.Api.Common.Validation;

namespace EventSphere.Api.DTOs;

public class CreateOrganizerRequestDto
{
    [Required(ErrorMessage = "Organization name is required.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Organization name must be between 2 and 200 characters.")]
    [NoEmoji(ErrorMessage = "Organization name contains invalid characters. Emoji are not allowed.")]
    public string OrganizationName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Reason is required.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Reason must be between 10 and 2000 characters.")]
    public string Reason { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Experience is too long.")]
    public string? Experience { get; set; }
}

public class OrganizerRequestDto
{
    public int Id { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? Experience { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminOrganizerRequestDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? Experience { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public int? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReviewOrganizerRequestDto
{
    [StringLength(1000, ErrorMessage = "Rejection reason is too long.")]
    public string? RejectionReason { get; set; }
}

// ───────────────────────────── Admin User Management ─────────────────────────────

public class AdminUserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? SuspendReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminUserDetailDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Mobile { get; set; }
    public string? Department { get; set; }
    public string? EnrollmentNo { get; set; }
    public string[] Roles { get; set; } = Array.Empty<string>();
    public bool IsActive { get; set; }
    public string? SuspendReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AssignRoleRequest
{
    [Required(ErrorMessage = "Role is required.")]
    public string Role { get; set; } = string.Empty;
}

public class SuspendUserRequest
{
    [StringLength(500, ErrorMessage = "Reason is too long.")]
    public string? Reason { get; set; }
}

public class WarnUserRequest
{
    [Required(ErrorMessage = "Warning message is required.")]
    [StringLength(1000, MinimumLength = 5, ErrorMessage = "Message must be between 5 and 1000 characters.")]
    public string Message { get; set; } = string.Empty;

    public bool SendEmail { get; set; }
}

public class SendAnnouncementRequest
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Message is too long.")]
    public string? Message { get; set; }
}

public class AdminReviewDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime SubmittedOn { get; set; }
}
