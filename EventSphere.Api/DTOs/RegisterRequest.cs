using System.ComponentModel.DataAnnotations;
using EventSphere.Api.Common.Validation;

namespace EventSphere.Api.DTOs;

/// <summary>
/// Registration payload. The AccountType field allows "Visitor" or "Organizer".
/// "Participant" and "Admin" must never be accepted — role assignment is server-controlled.
/// </summary>
public class RegisterRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
    [NoEmoji(ErrorMessage = "Name contains invalid characters. Emoji are not allowed.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(256, ErrorMessage = "Email is too long.")]
    public string Email { get; set; } = string.Empty;

    // Length/complexity is authoritatively enforced by ASP.NET Core Identity's
    // password policy. This bound only prevents absurd payloads.
    [Required(ErrorMessage = "Password is required.")]
    [StringLength(128, MinimumLength = 12, ErrorMessage = "Password must be at least 12 characters.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password confirmation is required.")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// "Visitor" or "Organizer". Participant and Admin are rejected.
    /// Defaults to "Visitor" if omitted.
    /// </summary>
    public string AccountType { get; set; } = "Visitor";

    // --- Organizer-specific fields (required when AccountType == "Organizer") ---

    [StringLength(200, ErrorMessage = "Organization name is too long.")]
    [NoEmoji(ErrorMessage = "Organization name contains invalid characters. Emoji are not allowed.")]
    public string? OrganizationName { get; set; }

    [StringLength(2000, ErrorMessage = "Reason is too long.")]
    public string? OrganizationReason { get; set; }

    [StringLength(2000, ErrorMessage = "Experience is too long.")]
    public string? OrganizationExperience { get; set; }
}
