# backend/SKILL.md — Modify ASP.NET Core Web API

## Purpose

Guide agents to safely modify API controllers, services, middleware, DI, JWT auth, and business logic.

## When To Use

- Adding or modifying an API endpoint.
- Changing business logic in services.
- Modifying DI registrations.
- Changing JWT or authorization configuration.
- Adding middleware.

## Inputs

- The file(s) being modified.
- `.agent/BACKEND.md` for structure.
- `.agent/SECURITY.md` for security rules.

## Preconditions

- Understand existing service patterns.
- Understand DI registration in `Program.cs`.
- Read service interface before modifying implementation.

## Workflow

1. **Read interface first**: Check `Services/Interfaces/I{Entity}Service.cs`.
2. **Read implementation**: Check `Services/Implementations/{Entity}Service.cs`.
3. **Check DI**: Verify service is registered in `Program.cs`.
4. **Check controller**: See how the service is consumed.
5. **Make change**: Modify interface + implementation together.
6. **Update DI if new service**: Add `AddScoped<INewService, NewService>()`.
7. **Verify build**: `dotnet build`.
8. **Run tests**: `dotnet test`.

## Rules

- Controllers inherit `ControllerBase` (not `Controller`).
- Use `[ApiController]` + `[Route("api/[controller]")]`.
- Always modify interface and implementation together.
- Services are Scoped (one instance per request).
- Use constructor injection.
- Async/await throughout.
- Return DTOs from API — never raw entities.
- Use `[Authorize]` on protected endpoints.
- Return `Ok()`, `Created()`, `NotFound()`, `BadRequest()`.
- Never hardcode secrets.

## Verification

```bash
dotnet build EventSphere.Api
dotnet test
```

## Failure Handling

- Build fails → check missing references, typos, missing usings.
- Test fails → check whether change broke existing behavior.
- DI error → verify registration in `Program.cs`.
