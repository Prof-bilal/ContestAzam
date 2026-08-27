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

// Notification delivery is system→user. The backend determines these types;
// they are never trusted from the client.
public enum NotificationType
{
    RegistrationConfirmed,
    RegistrationCancelled,
    PaymentSuccessful,
    PaymentFailed,
    EventUpdated,
    EventCancelled,
    EventReminder,
    EventStartingSoon,
    OrganizerRegistration,
    AttendanceConfirmed,
    OrganizerRequestApproved,
    OrganizerRequestRejected,
    MessageReceived,
    CertificateAvailable,
    FeedbackAvailable,
    NewEventInCategory
}
