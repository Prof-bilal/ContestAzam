# debugging/SKILL.md — Debug Full-Stack App

## Purpose

Guide agents to systematically diagnose issues in both backend (ASP.NET Core) and frontend (React).

## Workflow

```
Reproduce → Gather Evidence → Identify Boundary → Narrow Hypothesis
→ Test Hypothesis → Fix Root Cause → Regression Test → Verify
```

## Identify the Layer

| Symptom | Layer |
|---|---|
| 500 error, DI failure, SQL error | Backend |
| White screen, JS error, state issue | Frontend |
| 401/403, CORS error | Auth/CORS (both) |
| Data missing in UI | Both (API response + React rendering) |

## Backend Issues

- API won't start → check `Program.cs`, connection string, packages.
- 500 error → check logs, service code.
- 401 → check JWT token, signing key.
- EF Core error → check migrations, entity configuration.

## Frontend Issues

- Blank page → browser console, API URL in `.env`.
- API call fails → Network tab, CORS, auth header.
- State not updating → React DevTools, Context providers.
- Routing broken → check `App.tsx` routes.

## Cross-Layer

- Test API directly with curl/Postman.
- Check browser Network tab for request/response.
- Verify JWT token is attached to requests.
- Check CORS in API `Program.cs`.

## Rules

- Reproduce before fixing.
- One change at a time.
- Verify fix doesn't break other features.
- Add regression test.
