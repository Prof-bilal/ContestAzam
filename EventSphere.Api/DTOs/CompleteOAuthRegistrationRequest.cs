using System.ComponentModel.DataAnnotations;
using EventSphere.Api.Common.Validation;

namespace EventSphere.Api.DTOs;

/// <summary>
/// Completes OAuth registration for a new user. The PendingToken comes from
/// the OAuth callback redirect. The user selects Visitor or Organizer.
/// </summary>
public class CompleteOAuthRegistrationRequest
{
    [Required(ErrorMessage = "Registration token is required.")]
    public string PendingToken { get; set; } = string.Empty;

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
