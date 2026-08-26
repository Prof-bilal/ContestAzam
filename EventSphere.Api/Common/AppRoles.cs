namespace EventSphere.Api.Common;

/// <summary>
/// Canonical role names used by ASP.NET Core Identity and [Authorize(Roles=...)].
/// These strings are the single source of truth for authorization.
/// </summary>
public static class AppRoles
{
    public const string Visitor = "Visitor";
    public const string Participant = "Participant";
    public const string Organizer = "Organizer";
    public const string Admin = "Admin";

    /// <summary>All roles, ordered from least to most privileged.</summary>
    public static readonly string[] All = { Visitor, Participant, Organizer, Admin };

    /// <summary>The role every newly registered account receives. Assigned server-side only.</summary>
    public const string Default = Visitor;
}
