namespace EventSphere.Api.Models;

// Persisted as a string (see AppUserConfiguration). This mirrors the user's
// primary Identity role for convenience only; it is NOT the authorization
// source of truth — [Authorize(Roles=...)] reads ASP.NET Core Identity roles.
public enum UserRole
{
    Visitor,
    Participant,
    Organizer,
    Admin
}

public enum RegistrationStatus
{
    Confirmed,
    Cancelled,
    Waitlist
}

public enum MediaType
{
    Image,
    Video
}

public enum WaitlistStatus
{
    Waiting,
    Confirmed,
    Cancelled
}

public enum EventStatus
{
    Draft,
    PendingApproval,
    Approved,
    Rejected,
    Cancelled,
    Completed
}

public enum FeedbackRatingCategory
{
    Venue,
    Coordination,
    Technical,
    Hospitality
}

public enum PaymentStatus
{
    Pending,
    Succeeded,
    Failed,
    Refunded
}
