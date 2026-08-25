# DECISIONS.md — Architecture Decision Records

## ADR-001 — Use React for Frontend

### Status
Accepted

### Context
EventSphere needs a responsive, interactive frontend for browsing events, registration, dashboards, and admin panel. The team chose between ASP.NET Core MVC (Razor) and React SPA.

### Decision
Use React 18+ with Vite, React Router, and TypeScript as the frontend. ASP.NET Core Web API serves as the backend.

### Consequences
- Clear separation of frontend and backend.
- Rich interactivity and component reuse.
- Client-side routing (SPA experience).
- Separate deployment for frontend and backend.
- CORS configuration required.
- JWT authentication (stateless).

---

## ADR-002 — ASP.NET Core Web API (No MVC)

### Status
Accepted

### Context
With React as the frontend, MVC Controllers and Razor Views are unnecessary.

### Decision
Use pure ASP.NET Core Web API. No MVC controllers, no Razor Views.

### Consequences
- Simpler backend — only API controllers.
- All UI logic lives in React.
- JSON-only responses (no server-rendered HTML).
- Cleaner separation of concerns.

---

## ADR-003 — JWT Bearer as Primary Auth

### Status
Accepted

### Context
React SPA needs stateless authentication. Cookie auth doesn't work well with cross-origin SPA + API setup.

### Decision
Use JWT Bearer authentication. Token stored in localStorage/httpOnly cookie by React. Sent via `Authorization` header.

### Consequences
- Stateless — no server-side session.
- Token expiry and refresh needed.
- CORS must allow credentials.
- React manages token lifecycle.

---

## ADR-004 — Entity Framework Core + SQL Server

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

---

## ADR-005 — Vite as Build Tool

### Status
Accepted

### Context
Need fast dev server and optimized production build for React.

### Decision
Use Vite for React development and build.

### Consequences
- Fast HMR (Hot Module Replacement).
- Optimized production builds.
- Native ESM support.
- Plugin ecosystem for Tailwind, etc.

---

## ADR-006 — SignalR for Real-Time

### Status
Accepted

### Context
Users need real-time notifications (event updates, slot changes).

### Decision
Use ASP.NET Core SignalR with React client (`@microsoft/signalr`).

### Consequences
- WebSocket-based communication.
- Automatic fallback to long-polling.
- React connects on app mount.
- Hub authentication via JWT.
