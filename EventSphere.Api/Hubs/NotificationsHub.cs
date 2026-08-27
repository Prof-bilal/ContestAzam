using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EventSphere.Api.Hubs;

/// <summary>
/// Delivers private, real-time notifications only to the authenticated owner.
/// The client never identifies the target — the server routes by the JWT "sub" claim.
/// </summary>
[Authorize]
public class NotificationsHub : Hub
{
    public override Task OnConnectedAsync()
    {
        // No client-supplied targeting is used at connect time; authentication
        // (via the access token) decides routing through Context.UserIdentifier.
        return base.OnConnectedAsync();
    }
}