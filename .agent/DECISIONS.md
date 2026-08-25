# DECISIONS.md — Architecture Decision Records

## ADR-001 — Use React for Frontend

### Status
Accepted

### Decision
React 18+ with Vite, React Router, TypeScript as frontend. ASP.NET Core Web API as backend.

### Consequences
- Clear separation of frontend and backend.
- Rich interactivity and component reuse.
- CORS configuration required.
- JWT authentication (stateless).

---

## ADR-002 — ASP.NET Core Web API (No MVC)

### Status
Accepted

### Decision
Pure Web API. No MVC controllers, no Razor Views.

---

## ADR-003 — JWT Bearer as Primary Auth

### Status
Accepted

### Decision
JWT Bearer authentication. Token stored in localStorage. Sent via `Authorization` header.

---

## ADR-004 — Entity Framework Core + SQL Server

### Status
Accepted

### Decision
EF Core with Code-First approach.

---

## ADR-005 — Vite as Build Tool

### Status
Accepted

### Decision
Vite for React development and build.

---

## ADR-006 — SignalR for Real-Time

### Status
Accepted

### Decision
ASP.NET Core SignalR with React client (`@microsoft/signalr`).

---

## ADR-007 — 4-Person Team with Module Ownership

### Status
Accepted

### Decision
2 backend (Abdullah, Jibran) + 2 frontend (Ramsha, Marukh) with clear module boundaries.

### Consequences
- Clear ownership reduces conflicts.
- API contracts agreed before implementation.
