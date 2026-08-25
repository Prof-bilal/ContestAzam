# SECURITY.md — Security Rules

## Authentication

- **Primary: JWT Bearer** (React → API).
- Token via login, stored in localStorage.
- React sends `Authorization: Bearer {token}`.
- CORS for React dev server.

## Roles

- `Admin`, `Organizer`, `Participant`.
- `[Authorize]` on protected endpoints.
- React `ProtectedRoute` enforces on frontend.

## Secrets

> Never place secrets into source code.

- Use `appsettings.json` with placeholders.
- Use Environment Variables in production.
- React `.env` with `VITE_` prefix, never commit.

## CORS

```csharp
policy.WithOrigins("http://localhost:5173")
      .AllowAnyHeader().AllowAnyMethod().AllowCredentials();
```

## Checklist

- [ ] No secrets hardcoded
- [ ] Input validated (frontend + backend)
- [ ] `[Authorize]` on protected API
- [ ] CORS specific origins only
- [ ] No XSS (React auto-escapes)
- [ ] No SQL injection (EF Core parameterized)
