# AGENTS.md — AI Agent Operating Manual

## Mission

EventSphere is a **College Event Information System** — a full-stack web application for managing college events. Students browse and register for events, organizers create and manage events, and administrators oversee the entire platform. See `../SRS.md` for the full SRS.

### User Roles (from SRS)

| Role | Description |
|---|---|
| **Normal Student (Visitor)** | Unregistered user; browses public events, gallery, about/contact pages |
| **Participant (Registered Student)** | Registers, attends events, gets certificates, submits feedback |
| **Organizer (College Staff)** | Creates/manages events, manages registrations, uploads media, issues certificates |
| **Admin (System Administrator)** | Manages users, approves events, moderates content, sends announcements, generates reports |

### Team

| Member | Role | Module |
|---|---|---|
| Abdullah | Backend | Module 1 — Backend Core & Architecture |
| Jibran | Backend | Module 2 — Database + Data-Heavy Backend |
| Ramsha | Frontend | Module 3 — Frontend Core + Shared UI |
| Marukh | Frontend | Module 4 — Frontend Features + Dashboards |

### Key Features (from SRS)

- Event listing, search, filtering (by category, department, date)
- Registration with slot management and waitlist
- QR code check-in for attendance
- Certificate generation and download (fee-based, payment out of scope)
- Feedback and reviews (star ratings + comments)
- Media gallery (images/videos categorized by event)
- Calendar integration (.ics export)
- Social media sharing
- Real-time slot availability
- Dynamic venue capacity management
- User dashboard with activity history
- Admin dashboard with analytics
- Sitemap on home page

## Technology Stack — Hard Rules

```
ASP.NET Core 8
C#
ASP.NET Core MVC + Razor Views (frontend)
ASP.NET Core Web API (backend endpoints)
Entity Framework Core (ORM)
Microsoft SQL Server (database)
ASP.NET Core Identity (user management)
JWT Bearer Authentication (API auth)
Cookie Authentication (MVC auth)
ASP.NET Core SignalR (real-time notifications)
Git / GitHub
```

**The frontend is built with ASP.NET Core MVC and Razor Views.**

> Do NOT introduce React, Vue, Angular, Next.js, Vite, Svelte, or another frontend framework without explicit authorization.

## Before Making Changes

1. Read relevant `.agent/` documentation for the area you are modifying.
2. Inspect the affected files and their dependencies.
3. Identify which layer the change touches: MVC, API, Service, Data, or multiple.
4. Understand existing patterns before introducing new ones.
5. Make the **smallest safe change** that solves the problem.
6. Avoid unnecessary refactoring.

## Development Rules

### Naming Conventions
- Controllers: `{Entity}Controller` (e.g., `EventsController`)
- Services: `{Entity}Service` (e.g., `EventService`)
- Interfaces: `I{Entity}Service` (e.g., `IEventService`)
- ViewModels: `{View}ViewModel` (e.g., `EventDetailViewModel`)
- DTOs: `{Entity}Dto` or `{Action}{Entity}Request` (e.g., `CreateEventRequest`)
- Entities: `{Entity}` (e.g., `Event`, `AppUser`)
- Razor Views: Match action name (e.g., `Index.cshtml`, `Details.cshtml`)

### MVC Rules
- Keep controllers thin — delegate to services.
- Use ViewModels for complex view data, never pass entities directly to views.
- Keep all business logic in service classes.
- Never put database queries inside Razor Views (`.cshtml`).
- Reuse `_Layout.cshtml`, partials, and tag helpers.
- Use `[ValidateAntiForgeryToken]` on all POST actions.

### API Rules
- Use DTOs for request/response.
- Validate input with model validation.
- Return appropriate HTTP status codes (200, 201, 400, 401, 403, 404).
- Use `[Authorize]` where authentication is required.
- Follow existing route conventions (`api/[controller]`).

### Service Rules
- Services are registered in `Program.cs` via DI.
- Services handle business logic, not controllers.
- Services use `ApplicationDbContext` for data access.
- Async/await throughout.

### Database Rules
- Use Entity Framework Core exclusively.
- Follow existing entity patterns.
- Create migrations for all schema changes.
- Review generated migrations before committing.
- Never run destructive migrations without authorization.

### Authentication Rules
- MVC uses cookie authentication.
- API uses JWT Bearer authentication.
- Identity manages users and roles.
- Roles: `Admin`, `Organizer`, `Participant`.
- Use `[Authorize]` attribute with role-based policies where needed.

## Agent Rules — MUST

- Reuse existing abstractions and patterns.
- Follow existing code conventions.
- Avoid duplicate implementations.
- Avoid unnecessary dependencies.
- Never silently remove functionality.
- Never commit secrets, keys, or connection strings.
- Run relevant tests before marking work complete.
- Report failures honestly.

## Agent Rules — MUST NOT

- Rewrite the application unnecessarily.
- Introduce another frontend framework.
- Replace SQL Server with another database.
- Replace Entity Framework Core.
- Introduce microservices without justification.
- Add infrastructure without justification.
- Disable tests.
- Ignore compiler errors.
- Hardcode secrets or API keys.
- Perform destructive Git operations without authorization.

## Change Workflow

```
Understand → Inspect → Plan → Implement → Test → Review → Verify → Summarize
```

## Definition of Done

- Code compiles without errors.
- Existing tests pass.
- New behavior has corresponding tests.
- No security regressions.
- Documentation updated if public API or architecture changed.
- No hardcoded secrets.
- Follows existing code patterns.
