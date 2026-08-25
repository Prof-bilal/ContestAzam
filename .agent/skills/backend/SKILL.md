# backend/SKILL.md — Modify ASP.NET Core Backend

## Purpose

Guide agents to safely modify services, controllers, middleware, DI, authentication, and business logic.

## When To Use

- Adding or modifying a service.
- Changing business logic.
- Modifying DI registrations.
- Changing authentication or authorization.
- Adding middleware.

## Inputs

- The file(s) being modified.
- `.agent/BACKEND.md` for structure.
- `.agent/SECURITY.md` for security rules.

## Preconditions

- Understand existing service patterns.
- Understand DI registration in `Program.cs`.
- Read affected service interface before modifying implementation.

## Workflow

1. **Read interface first**: Check `Services/Interfaces/I{Entity}Service.cs`.
2. **Read implementation**: Check `Services/Implementations/{Entity}Service.cs`.
3. **Check DI**: Verify service is registered in `Program.cs`.
4. **Check controller**: See how the service is consumed.
5. **Make change**: Modify interface + implementation together.
6. **Update DI if new service**: Add `AddScoped<INewService, NewService>()` in `Program.cs`.
7. **Verify build**: `dotnet build`.
8. **Run tests**: `dotnet test`.

## Rules

- Always modify interface and implementation together.
- Services are Scoped (one instance per request).
- Use constructor injection.
- Async/await throughout.
- Never return raw entities from API — use DTOs.
- Use `[Authorize]` on protected endpoints.
- Use `[ValidateAntiForgeryToken]` on MVC POST actions.
- Never hardcode secrets or connection strings.

## Verification

```bash
dotnet build
dotnet test
```

## Failure Handling

- Build fails → check missing references, typos, missing using statements.
- Test fails → check whether the change broke existing behavior.
- DI error → verify registration in `Program.cs`.
