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
Frontend:  React 18+ (Vite, React Router, Axios, Bootstrap/React-Bootstrap)
Backend:   ASP.NET Core 8 Web API
Database:  Microsoft SQL Server
ORM:       Entity Framework Core 8
Auth:      ASP.NET Core Identity + JWT Bearer
Real-time: ASP.NET Core SignalR
Git:       Git / GitHub
```

**Frontend is a React SPA. Backend is ASP.NET Core Web API. They are separate projects.**

> Do NOT use ASP.NET Core MVC or Razor Views for the frontend. The frontend is React.

## Before Making Changes

1. Read relevant `.agent/` documentation for the area you are modifying.
2. Inspect the affected files and their dependencies.
3. Identify which layer the change touches: React, API, Service, Data, or multiple.
4. Understand existing patterns before introducing new ones.
5. Make the **smallest safe change** that solves the problem.
6. Avoid unnecessary refactoring.

## Development Rules

### Naming Conventions
- API Controllers: `{Entity}Controller` (e.g., `EventsController`)
- Services: `{Entity}Service` (e.g., `EventService`)
- Interfaces: `I{Entity}Service` (e.g., `IEventService`)
- DTOs: `{Entity}Dto` or `{Action}{Entity}Request` (e.g., `CreateEventRequest`)
- Entities: `{Entity}` (e.g., `Event`, `AppUser`)
- React Components: `PascalCase` (e.g., `EventCard.tsx`)
- React Hooks: `use` prefix (e.g., `useEvents.ts`)
- React Pages: `PascalCasePage` (e.g., `EventDetailPage.tsx`)

### Backend Rules (ASP.NET Core Web API)
- Controllers inherit from `ControllerBase`.
- Use `[ApiController]` + `[Route("api/[controller]")]`.
- Use DTOs for all request/response — never expose raw entities.
- Validate input with model validation.
- Return appropriate HTTP status codes (200, 201, 400, 401, 403, 404).
- Use `[Authorize]` where authentication is required.
- Keep controllers thin — delegate to services.

### Frontend Rules (React)
- Use functional components with hooks.
- Use React Router for client-side routing.
- Use Axios for HTTP requests to the API.
- Use Context API or Zustand for state management.
- Use React Bootstrap or Tailwind for styling.
- Component files: `.tsx`. Logic files: `.ts`.
- Keep components small and focused.
- Extract reusable components into `components/`.
- API calls go in `services/` or `api/` directory.

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
- **Primary auth: JWT Bearer** (used by React frontend via API).
- ASP.NET Core Identity manages users and roles.
- Roles: `Admin`, `Organizer`, `Participant`.
- JWT token stored in localStorage/sessionStorage by React.
- React sends `Authorization: Bearer {token}` header on protected requests.
- CORS configured for React dev server origin.

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

- Code compiles without errors (both backend and frontend).
- Existing tests pass.
- New behavior has corresponding tests.
- No security regressions.
- Documentation updated if public API or architecture changed.
- No hardcoded secrets.
- Follows existing code patterns.
