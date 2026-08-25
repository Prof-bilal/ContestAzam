# DEBUGGING.md — Debug Full-Stack App

## Identify Layer

| Symptom | Layer |
|---|---|
| 500 error, DI failure | Backend |
| White screen, JS error | Frontend |
| 401/403, CORS | Auth/CORS |
| Data missing in UI | Both |

## Backend

- Won't start → check `Program.cs`, connection string.
- 401 → check JWT token, signing key.
- EF error → check migrations.

## Frontend

- Blank page → console, `.env` API URL.
- API fails → Network tab, CORS, auth header.
- State issue → React DevTools.

## Rules

- Reproduce before fixing.
- One change at a time.
- Add regression test.
