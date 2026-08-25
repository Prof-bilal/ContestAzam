namespace EventSphere.Api.Models;

public enum UserRole
{
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
