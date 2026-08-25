# security/SKILL.md — Security Review Checklist

## Purpose

Guide agents to perform security review of code changes.

## When To Use

- Any change touching auth, data access, or user input.
- Before completing any task.

## Checklist

### Secrets
- [ ] No hardcoded passwords, API keys, tokens.
- [ ] No real connection strings in source code.
- [ ] JWT key is in configuration, not code.
- [ ] React `.env` not committed (use `.env.example`).

### Authentication (JWT)
- [ ] `[Authorize]` on protected API endpoints.
- [ ] JWT token validated (expiry, signing key, issuer/audience).
- [ ] Token not stored in plain JS variables.
- [ ] Refresh token flow implemented (if applicable).

### Authorization
- [ ] Role-based access correct (`Admin`, `Organizer`, `Participant`).
- [ ] Ownership checks in services.
- [ ] React `ProtectedRoute` enforces roles on frontend.

### CORS
- [ ] Specific origins whitelisted (not `AllowAnyOrigin`).
- [ ] React dev server origin included.
- [ ] Credentials allowed for JWT.

### Input Validation
- [ ] Backend: Model validation on DTOs.
- [ ] Frontend: Form validation before submission.
- [ ] No SQL injection (EF Core parameterized queries).
- [ ] No XSS (React auto-escapes; no `dangerouslySetInnerHTML`).

### Data Exposure
- [ ] No sensitive data in API responses.
- [ ] No stack traces in error responses.
- [ ] No logging of sensitive data.

### File Uploads
- [ ] File type whitelist enforced.
- [ ] File size limits enforced.
- [ ] Files stored outside web root.

### Dependencies
- [ ] `dotnet list package --vulnerable` — clean.
- [ ] `npm audit` — clean (or vulnerabilities documented).

## Verification

```bash
dotnet list package --vulnerable
cd EventSphere.React && npm audit
```

## Failure Handling

- Security issue found → fix immediately.
- If unsure → flag for manual review.
