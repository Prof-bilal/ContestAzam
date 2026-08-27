using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace EventSphere.Api.Hubs;

/// <summary>
/// Routes SignalR messages by the JWT "sub" claim (the user id). Because the API
/// uses MapInboundClaims=false, "sub" is not mapped to the default NameIdentifier
/// claim, so we provide an explicit provider keyed on "sub".
/// </summary>
public class SubUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        // Never trust a client-provided id: read only from the authenticated principal.
        return connection.User?.FindFirstValue("sub");
    }
}