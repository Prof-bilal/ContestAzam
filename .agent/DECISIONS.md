# DECISIONS.md — Architecture Decision Records

## ADR-001 — Use ASP.NET Core MVC + Razor for Frontend

### Status
Accepted

### Context
EventSphere needs a frontend for browsing and managing events.

### Decision
Use ASP.NET Core MVC with Razor Views for the frontend. No separate SPA framework.

### Consequences
- Single technology stack.
- Server-rendered HTML (good SEO, fast initial load).
- No client-side routing complexity.
- All UI logic stays in C# / Razor.

---

## ADR-002 — Use Entity Framework Core as ORM

### Status
Accepted

### Decision
Use EF Core with Code-First approach.

### Consequences
- Migrations for schema versioning.
- LINQ queries (type-safe).
- Migrations must be reviewed before deployment.

---

## ADR-003 — Dual Authentication (Cookie + JWT)

### Status
Accepted

### Decision
- MVC uses Cookie authentication.
- API uses JWT Bearer authentication.
- Both share ASP.NET Core Identity.

---

## ADR-004 — Use SignalR for Real-Time

### Status
Accepted

### Decision
Use ASP.NET Core SignalR for push notifications.

---

## ADR-005 — Service Layer Pattern

### Status
Accepted

### Decision
Implement service layer between controllers and EF Core.

### Consequences
- Controllers stay thin.
- Business logic testable in isolation.

---

## ADR-006 — 4-Person Team with Module Ownership

### Status
Accepted

### Decision
Team divided into 2 backend (Abdullah, Jibran) and 2 frontend (Ramsha, Marukh) with clear module boundaries.

### Consequences
- Clear ownership reduces conflicts.
- Cross-module changes require coordination.
- API contracts must be agreed before implementation.
