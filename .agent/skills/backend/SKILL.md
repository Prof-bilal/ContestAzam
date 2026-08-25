# backend/SKILL.md — Modify ASP.NET Core Web API

## Module Ownership

- **Abdullah (Module 1)**: Core architecture, auth, API infrastructure
- **Jibran (Module 2)**: Database, data-heavy services, data controllers

## Rules

- Controllers inherit `ControllerBase`.
- `[ApiController]` + `[Route("api/[controller]")]`.
- DTOs for request/response.
- `[Authorize]` on protected endpoints.
- Services registered as Scoped.
- Async/await throughout.

## Verification

```bash
dotnet build EventSphere.Api
dotnet test
```
