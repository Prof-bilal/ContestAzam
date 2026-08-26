using EventSphere.Api.Models;

namespace EventSphere.Api.Common;

/// <summary>
/// Maps between Identity role names and the denormalized <see cref="UserRole"/>
/// mirror on <see cref="AppUser"/>. The mirror is display-only; authorization
/// always uses Identity roles.
/// </summary>
public static class RoleMapping
{
    private static readonly Dictionary<string, UserRole> ToEnum = new(StringComparer.OrdinalIgnoreCase)
    {
        [AppRoles.Visitor] = UserRole.Visitor,
        [AppRoles.Participant] = UserRole.Participant,
        [AppRoles.Organizer] = UserRole.Organizer,
        [AppRoles.Admin] = UserRole.Admin,
    };

    /// <summary>Returns the highest-privilege role from a set, for the denormalized mirror.</summary>
    public static UserRole PrimaryRole(IEnumerable<string> roles)
    {
        var highest = UserRole.Visitor;
        foreach (var r in roles)
            if (ToEnum.TryGetValue(r, out var mapped) && mapped > highest)
                highest = mapped;
        return highest;
    }
}
