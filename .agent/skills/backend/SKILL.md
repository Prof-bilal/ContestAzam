# backend/SKILL.md — Modify ASP.NET Core Backend

## Purpose

Guide agents to safely modify controllers, services, middleware, DI, auth, and business logic.

## Module Ownership

- **Abdullah (Module 1)**: Core architecture, auth, API infrastructure
- **Jibran (Module 2)**: Database, data-heavy services, data controllers

## Rules

- Controllers inherit `Controller` (MVC) or `ControllerBase` (API).
- Always modify interface and implementation together.
- Services are Scoped.
- Use constructor injection.
- Async/await throughout.
- Use `[Authorize]` on protected endpoints.
- Use `[ValidateAntiForgeryToken]` on MVC POST actions.

## Verification

```bash
dotnet build
dotnet test
```
