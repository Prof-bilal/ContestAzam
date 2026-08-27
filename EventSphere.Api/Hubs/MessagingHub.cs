using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EventSphere.Api.Hubs;

/// <summary>
/// Real-time direct messaging. Messages are persisted by the API first (source of
/// truth) and this hub pushes the result to participants. Routing is per-user via
/// <see cref="Hub.Clients"/>.User(...), keyed on the JWT "sub" claim.
/// </summary>
[Authorize]
public class MessagingHub : Hub
{
    public override Task OnConnectedAsync()
    {
        return base.OnConnectedAsync();
    }
}