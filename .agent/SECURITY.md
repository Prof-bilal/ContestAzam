# SECURITY.md — Security Rules

## Authentication

- **Primary: JWT Bearer** (React → API).
- Access token stored in memory only (never localStorage).
- Refresh token via HttpOnly cookie (`es_refresh`).
- React sends `Authorization: Bearer {token}`.
- CORS for configured frontend origins.
- Email verification required before login.
- Account lockout after 5 failed attempts (15-minute lockout).

## Roles

EventSphere has exactly four application roles:

| Role | Assignment | Description |
|---|---|---|
| **Visitor** | Default on registration | Can browse events, manage profile, register for events |
| **Participant** | Auto-assigned after event registration | Has participated in at least one event |
| **Organizer** | Admin approval required | Can create and manage events |
| **Admin** | Provisioned securely (never via registration) | Full platform administration |

### Role Lifecycle

```
New Account → Visitor
Visitor + Event Registration → Participant
Visitor/Participant + OrganizerRequest + Admin Approval → Organizer
Admin → Securely provisioned only
```

### Security Rules

- Participant is NEVER shown during registration
- Admin is NEVER assignable through public registration or OAuth
- Organizer requires Admin approval — never self-assigned
- Frontend role information is never trusted by the backend
- `[Authorize(Roles=...)]` enforced on all protected API endpoints
- JWT roles come from server-side Identity, not the client

## Secrets

> Never place secrets into source code.

Required environment variables:

| Variable | Purpose |
|---|---|
| `Jwt__Key` | JWT signing key (>= 32 bytes, HMAC-SHA256) |
| `Jwt__Issuer` | JWT issuer (default: "EventSphere") |
| `Jwt__Audience` | JWT audience (default: "EventSphere") |
| `Google__ClientId` | Google OAuth client ID |
| `Google__ClientSecret` | Google OAuth client secret |
| `GitHub__ClientId` | GitHub OAuth client ID |
| `GitHub__ClientSecret` | GitHub OAuth client secret |
| `Brevo__ApiKey` | Brevo transactional email API key |
| `Brevo__SenderEmail` | Brevo sender email address |

- Use `appsettings.json` with placeholders for development.
- Use Environment Variables in production.
- React `.env` with `VITE_` prefix, never commit.
- OAuth secrets are backend-only, never exposed to React.

## OAuth

- Google and GitHub OAuth supported.
- New OAuth users always receive the Visitor role.
- Email verification from provider required for new accounts.
- Existing accounts are not silently linked (prevents account takeover).
- OAuth state validation enabled.

## CORS

```csharp
policy.WithOrigins(configuredOrigins) // Never AllowAnyOrigin
      .AllowAnyHeader()
      .AllowAnyMethod()
      .AllowCredentials(); // Required for HttpOnly refresh cookie
```

## Rate Limiting

| Policy | Limit | Window |
|---|---|---|
| `auth` (login, register, refresh, OAuth) | 10 requests | 60 seconds |
| `email` (forgot-password, resend-verification) | 5 requests | 60 seconds |
| General (all other traffic) | 100 requests | 60 seconds |

Rate limit responses include `Retry-After` header.

## Password Policy

- Minimum 12 characters
- Requires uppercase, lowercase, digit, and symbol
- Requires 4 unique characters

## Refresh Tokens

- Stored as SHA-256 hash (raw token never touches database)
- HttpOnly, Secure cookie
- 7-day lifetime with rotation
- Reuse detection: reusing a rotated token revokes the entire token family
- Logout invalidates the refresh token

## Checklist

- [ ] No secrets hardcoded
- [ ] Input validated (frontend + backend)
- [ ] `[Authorize]` on protected API
- [ ] CORS specific origins only
- [ ] No XSS (React auto-escapes)
- [ ] No SQL injection (EF Core parameterized)
- [ ] Email verification required
- [ ] Rate limiting enabled
- [ ] Account lockout enabled
- [ ] Password policy enforced
- [ ] Refresh tokens rotated with reuse detection
- [ ] OAuth secrets backend-only
- [ ] Roles assigned server-side only
- [ ] Participant never self-assignable
- [ ] Organizer requires Admin approval
- [ ] Admin never publicly registerable
