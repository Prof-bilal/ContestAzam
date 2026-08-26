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
