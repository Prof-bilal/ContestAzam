# DECISIONS.md — Architecture Decision Records

## ADR-001 — Use ASP.NET Core MVC + Razor for Frontend

### Status
Accepted

### Context
EventSphere needs a frontend for browsing and managing events. The team decided between SPA (React/Vue) and server-rendered (Razor).

### Decision
Use ASP.NET Core MVC with Razor Views for the frontend. No separate SPA framework.

### Consequences
- Single technology stack (simpler deployment, fewer dependencies).
- Server-rendered HTML (good SEO, fast initial load).
- No client-side routing complexity.
- Limited client-side interactivity compared to SPA.
- All UI logic stays in C# / Razor.

---

## ADR-002 — Use Entity Framework Core as ORM

### Status
Accepted

### Context
Need ORM for SQL Server data access.

### Decision
Use Entity Framework Core with Code-First approach.

### Consequences
- Migrations for schema versioning.
- LINQ queries (type-safe).
- Change tracking built-in.
- Potential N+1 query issues (mitigate with Include()).
- Migrations must be reviewed before deployment.

---

## ADR-003 — Dual Authentication (Cookie + JWT)

### Status
Accepted

### Context
MVC controllers need session-based auth; API needs stateless auth.

### Decision
- MVC uses Cookie authentication.
- API uses JWT Bearer authentication.
- Both share ASP.NET Core Identity.

### Consequences
- Single user store (Identity).
- Two authentication schemes configured.
- JWT for API consumers, cookies for browser.
- Token refresh not implemented (P2 gap).

---

## ADR-004 — Use SignalR for Real-Time Notifications

### Status
Accepted

### Context
Users need real-time notification delivery.

### Decision
Use ASP.NET Core SignalR for push notifications.

### Consequences
- WebSocket-based communication.
- Hub-based architecture.
- Connection management required.
- Fallback to long-polling if WebSocket unavailable.

---

## ADR-005 — Service Layer Pattern

### Status
Accepted

### Context
Need clean separation between controllers and data access.

### Decision
Implement service layer between controllers and EF Core.

### Consequences
- Controllers stay thin.
- Business logic testable in isolation.
- Services registered via DI.
- Slight overhead of additional abstraction layer.
