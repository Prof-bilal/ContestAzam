# signalr/SKILL.md — SignalR Real-Time Hubs

## Purpose

Guide agents to implement SignalR for real-time notifications.

## When To Use

- Real-time notifications (event updates, slot changes).
- Do NOT use for ordinary request/response.

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
}
```

## Rules

- Only use for genuine real-time needs.
- Use groups for targeted messaging.
- Add `[Authorize]` on hub.
- Handle connection/disconnection.
