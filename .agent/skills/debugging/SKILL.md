# debugging/SKILL.md — Debug ASP.NET Core App

## Purpose

Systematically diagnose issues in the ASP.NET Core MVC + API application.

## Common Issues

| Symptom | Likely Cause |
|---|---|
| View not found | Wrong view name, missing `_ViewImports` |
| 401 Unauthorized | JWT token expired, wrong key |
| DI error | Missing registration in `Program.cs` |
| EF Core error | Missing `Include()`, FK violation |
| Model validation failed | Missing `[Required]`, wrong field names |

## Rules

- Reproduce before fixing.
- One change at a time.
- Add regression test.
