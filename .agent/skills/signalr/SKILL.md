# signalr/SKILL.md — SignalR Real-Time Hubs

## Purpose

Guide agents to correctly implement SignalR hubs for real-time features.

## When To Use

- Adding real-time notifications.
- Adding live updates (event changes, status).
- Adding real-time collaboration features.
- Do NOT use SignalR for ordinary request/response.

## Inputs

- `Hubs/NotificationHub.cs`.
- `Program.cs` (SignalR registration).
- JavaScript client code.

## Preconditions

- SignalR is registered in `Program.cs`: `builder.Services.AddSignalR()`.
- Hub route mapped: `app.MapHub<NotificationHub>("/hubs/notifications")`.
- Read existing hub before modifying.

## Workflow

1. **Read existing hub**: `Hubs/NotificationHub.cs`.
2. **Check if SignalR is necessary**: Could this work with HTTP polling? If yes, don't use SignalR.
3. **Add hub method**: Public method in hub class.
4. **Add client call**: `await Clients.Group("groupName").SendAsync("MethodName", data)`.
5. **Add JavaScript client**: Use `@microsoft/signalr` npm package or CDN.
6. **Handle connection**: `OnConnectedAsync`, `OnDisconnectedAsync`.
7. **Authorization**: Use `[Authorize]` on hub or methods.

## Hub Pattern

```csharp
[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        await base.OnConnectedAsync();
    }

    public async Task SendToUser(string userId, string message)
    {
        await Clients.Group($"user_{userId}").SendAsync("ReceiveMessage", message);
    }
}
```

## Rules

- Only use SignalR for genuine real-time needs.
- Use groups for targeted messaging.
- Handle connection/disconnection.
- Add `[Authorize]` for authenticated hubs.
- Validate input in hub methods.
- Don't block hub methods (use async).
- Don't store state in hub instances (stateless).

## Verification

- Build succeeds.
- Client connects to hub endpoint.
- Messages are received by connected clients.

## Failure Handling

- Connection fails → check hub route, authentication, CORS.
- Messages not received → check group name, method name match.
- Performance issues → check for message flooding, use batching.
