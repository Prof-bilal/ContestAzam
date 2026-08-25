# AGENTS.md — EventSphere

> **Start here.** This file points to all agent documentation.

## Quick Rules

1. **Tech stack**: ASP.NET Core 8 Web API + React 18 (Vite) + SQL Server + EF Core + JWT + SignalR.
2. **Frontend is React SPA** — not MVC, not Razor Views.
3. **Backend is Web API only** — no MVC controllers.
4. **Read `.agent/` docs** before making changes.
5. **Make smallest safe change** that solves the problem.
6. **Never commit secrets**.
7. **Run `dotnet test` AND `npm test`** before completing work.

## Documentation

| Document | Path |
|---|---|
| Operating Manual | `.agent/AGENTS.md` |
| Architecture | `.agent/ARCHITECTURE.md` |
| Backend (API) | `.agent/BACKEND.md` |
| Frontend (React) | `.agent/FRONTEND.md` |
| Database | `.agent/DATABASE.md` |
| API Conventions | `.agent/API.md` |
| Testing | `.agent/TESTING.md` |
| Security | `.agent/SECURITY.md` |
| GitHub | `.agent/GITHUB.md` |
| Deployment | `.agent/DEPLOYMENT.md` |
| Development | `.agent/DEVELOPMENT.md` |
| Debugging | `.agent/DEBUGGING.md` |
| Code Style | `.agent/CODE_STYLE.md` |
| Decisions | `.agent/DECISIONS.md` |
| Skills Index | `.agent/skills/README.md` |

## Before Any Change

1. Read relevant `.agent/` document.
2. Read affected source files (both API and React if applicable).
3. Identify dependencies and patterns.
4. Make the change.
5. Run `dotnet build && dotnet test` (backend).
6. Run `npm run build && npm test` (frontend).
7. Verify no regressions.
