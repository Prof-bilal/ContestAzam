using System.ComponentModel.DataAnnotations;
using EventSphere.Api.Common.Validation;
using EventSphere.Api.Models;

namespace EventSphere.Api.DTOs;

/// <summary>Full profile view returned to the authenticated user.</summary>
public class ProfileDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
    public bool EmailConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? FullName { get; set; }
    public string? Mobile { get; set; }
    public string? Department { get; set; }
    public string? EnrollmentNo { get; set; }
    public string? ProfileImageUrl { get; set; }
    public OrganizerRequestStatus? OrganizerRequestStatus { get; set; }
    public string? OrganizationName { get; set; }
}

/// <summary>Update profile payload.</summary>
public class UpdateProfileRequest
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters.")]
    [NoEmoji(ErrorMessage = "Full name contains invalid characters. Emoji are not allowed.")]
    public string FullName { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "Phone number is too long.")]
    public string? Mobile { get; set; }

    [StringLength(100, ErrorMessage = "Department is too long.")]
    public string? Department { get; set; }

    /// <summary>Base64-encoded image data (data:image/...;base64,...) or a URL.</summary>
    [StringLength(5_000_000, ErrorMessage = "Profile image is too large.")]
    public string? ProfileImageUrl { get; set; }
}

/// <summary>Delete account confirmation payload.</summary>
public class DeleteAccountRequest
{
    [Required(ErrorMessage = "Confirmation text is required.")]
    [RegularExpression("^DELETE$", ErrorMessage = "Type DELETE to confirm account deletion.")]
    public string Confirmation { get; set; } = string.Empty;
}
