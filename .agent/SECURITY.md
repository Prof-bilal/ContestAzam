# SECURITY.md — Security Rules

## Authentication

- **Primary: JWT Bearer** (React frontend → API).
- Token obtained via login, stored in localStorage/httpOnly cookie.
- React sends `Authorization: Bearer {token}` on all protected requests.
- Token expiry: configurable (default 60 min).
- Refresh token flow recommended for production.

## Authorization

- Roles: `Admin`, `Organizer`, `Participant`.
- `[Authorize]` on protected API controllers/actions.
- `[Authorize(Roles = "Admin")]` for admin-only endpoints.
- Ownership checks in services (user can only access own data).
- React `ProtectedRoute` component checks role on frontend.

## Secrets

> Never place secrets, API keys, passwords, tokens, private credentials, or production connection strings into source code or documentation.

- Use `appsettings.json` with placeholders.
- Use User Secrets for local development.
- Use Environment Variables in production.
- JWT key must be at least 32 characters.
- React `.env` files: use `VITE_` prefix, never commit `.env`.

## CORS

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("React", policy =>
    {
        policy.WithOrigins("http://localhost:5173")  // Vite dev server
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

- Never use `AllowAnyOrigin()` in production.
- Whitelist specific origins.

## CSRF

- JWT Bearer is stateless — CSRF not applicable for API calls.
- If using cookies for auth, add CSRF protection.

## XSS Prevention

- **Backend**: JSON responses — no HTML rendering.
- **Frontend**: React auto-escapes JSX by default.
- Never use `dangerouslySetInnerHTML` with untrusted content.
- Sanitize any user-generated content before display.

## SQL Injection

- Entity Framework Core uses parameterized queries.
- Never use raw SQL with string concatenation.

## Input Validation

- Backend: Model validation attributes on DTOs.
- Frontend: Form validation (React Hook Form / Yup).
- Both layers validate — never trust client-side alone.

## File Uploads

- Validate file types (whelist allowed extensions).
- Limit file size (e.g., 10MB).
- Store outside web root.
- Scan for malware in production.

## Password Policy

- Minimum 6 characters.
- Requires digit, lowercase, uppercase.
- Hashed by ASP.NET Core Identity (PBKDF2).

## Logging

- Never log sensitive data (passwords, tokens, credit cards).
- Log authentication events (login, logout, failed attempts).
- Log authorization failures.

## Dependencies

- Monitor NuGet packages (backend): `dotnet list package --vulnerable`.
- Monitor npm packages (frontend): `npm audit`.

## Security Checklist for Code Changes

- [ ] No secrets hardcoded
- [ ] Input validated (both frontend + backend)
- [ ] Authorization attributes present on API
- [ ] JWT token validated correctly
- [ ] CORS configured for specific origins only
- [ ] No XSS vectors (unescaped user input)
- [ ] No SQL injection (parameterized queries)
- [ ] File uploads validated (type, size)
- [ ] Sensitive data not logged
- [ ] `npm audit` clean (frontend)
