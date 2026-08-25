# signalr/SKILL.md — SignalR Hubs

## When To Use

- Real-time notifications only.
- Do NOT use for ordinary HTTP.

## Rules

- `[Authorize]` on hub.
- Use groups for targeted messaging.
- Handle connection/disconnection.
- React client: `@microsoft/signalr`.
