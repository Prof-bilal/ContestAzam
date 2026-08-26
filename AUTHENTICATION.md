# AUTHENTICATION.md — EventSphere

How authentication and authorization work in the EventSphere API and the React
auth client. The **backend is the sole authority**; the React app is never trusted.

## Components

| Concern | Mechanism |
|---|---|
| User management | ASP.NET Core Identity (`AppUser : IdentityUser<int>`) |
| API authentication | JWT Bearer access tokens |
| Session longevity | Server-tracked, rotating refresh tokens (hashed, HttpOnly cookie) |
| External login | Google & GitHub via OAuth 2.0 (generic handler, no extra packages) |
| Authorization | Role-based (`[Authorize(Roles=...)]`) using Identity roles |
| Email delivery | Brevo transactional email (via `IEmailService` abstraction) |
| Email verification | ASP.NET Core Identity email confirmation tokens |
| Password reset | ASP.NET Core Identity password reset tokens |

## Identity

- `AddIdentityCore<AppUser>()` + `AddRoles<IdentityRole<int>>()` + `AddSignInManager()`.
  Identity Core is used (not `AddIdentity`) so the app is JWT-first and does not
  register cookie-login schemes it doesn't need.
- Passwords are hashed and verified by Identity (`PasswordHasher`). The app never
  hashes or stores passwords itself.
- `AppUser.Role` is a **denormalized mirror** of the user's primary role for display
  only. It is never consulted for authorization.
- `RequireConfirmedAccount = true` — users must verify their email before login.

## Roles

Exactly four roles, seeded idempotently on startup (`DbSeeder`):

`Visitor` · `Participant` · `Organizer` · `Admin`

- **Default role at registration is `Visitor`**, assigned server-side.
- The registration DTO has **no** role/`isAdmin`/`isOrganizer` field. Any such fields
  in the request body are ignored by model binding.
- There is no public role-promotion endpoint. Elevating a user is an Admin-only
  concern for a future phase.

Demo endpoints (`/api/demo/*`) prove enforcement. Roles are cumulative upward:

| Endpoint | Allowed roles |
|---|---|
| `GET /api/demo/public` | anonymous |
| `GET /api/demo/visitor` | any authenticated role |
| `GET /api/demo/participant` | Participant, Organizer, Admin |
| `GET /api/demo/organizer` | Organizer, Admin |
| `GET /api/demo/admin` | Admin |

## JWT access tokens

- Lifetime: **15 minutes** (`Jwt:AccessTokenMinutes`), deliberately short.
- Claims: `sub` (user id), `email`, `jti`, `name`, and one `role` claim per role.
  No sensitive data is placed in the token.
- Validation is strict: issuer, audience, signature, lifetime, and signing key are
  all validated; `ClockSkew = 0`. Inbound claim mapping is disabled; `role`/`name`
  are the configured role/name claim types.
- Signing key comes from `Jwt:Key` (env vars / user secrets). In Development a random
  ephemeral key is generated if none is set; every other environment **requires** a
  key of at least 32 bytes or startup fails.

## Refresh tokens

- A refresh token is a 256-bit random value. Only its **SHA-256 hash** is stored in
  the `RefreshTokens` table — the raw value never touches the database.
- Delivered to the browser as an **HttpOnly, Secure, SameSite** cookie scoped to
  `/api/auth`. It is never readable by JavaScript.
- Lifetime: **7 days** (`RefreshToken:DaysValid`).
- **Rotation**: every `/api/auth/refresh` issues a new token and revokes the old one.
- **Reuse detection**: presenting an already-rotated token revokes the entire token
  **family** (all descendants of one login) — a stolen-token replay logs everyone in
  that chain out.
- **Revocation**: logout revokes the current token; refresh checks revocation.
- **Bulk revocation**: password reset revokes all active refresh tokens for the user,
  forcing re-authentication.

## Email verification

When a user registers, the system:

1. Creates the Identity user with `EmailConfirmed = false`
2. Generates a secure email confirmation token via `UserManager.GenerateEmailConfirmationTokenAsync`
3. Sends a verification email through Brevo with a link to `/verify-email?token=...&email=...`
4. Returns the auth response (user is created but must verify email)

The verification token is:
- Cryptographically secure (Identity-generated)
- Single-purpose (email confirmation only)
- Time-limited (expires according to Identity policy)
- Validated server-side via `UserManager.ConfirmEmailAsync`

**Login requires email confirmation** (`RequireConfirmedAccount = true`). Unverified
users receive a `401` with `emailVerificationRequired` flag.

### Endpoints

| Method | Route | Purpose |
|---|---|---|
| POST | `/api/auth/verify-email` | Confirm email with token |
| POST | `/api/auth/resend-verification` | Resend verification email (rate-limited) |

### Resend verification

- Rate-limited via the `email` policy
- Always returns the same generic response regardless of whether the email exists
- Prevents email enumeration

## Password reset (forgot password)

### Flow

1. User enters email at `/forgot-password`
2. `POST /api/auth/forgot-password` — rate-limited, returns generic response
3. If user exists, Identity generates a password reset token
4. Reset email sent through Brevo with link to `/reset-password?token=...&email=...`
5. User enters new password at `/reset-password`
6. `POST /api/auth/reset-password` — validates token, resets password via Identity
7. All existing refresh tokens are revoked (forces re-authentication)

### Security

- **Generic response**: always returns "If an account exists for this email, we sent
  a password reset link" — never reveals whether the email exists
- **Rate-limited** via the `email` policy (separate from auth rate limits)
- **Token security**: Identity-generated cryptographic tokens, single-purpose, expire,
  validated server-side
- **Session revocation**: all refresh tokens for the user are revoked after reset
- **No auto-login**: user must authenticate fresh after password reset

### Endpoints

| Method | Route | Purpose |
|---|---|---|
| POST | `/api/auth/forgot-password` | Request password reset email |
| POST | `/api/auth/reset-password` | Reset password with token |

## Endpoints

| Method | Route | Auth | Rate Limit | Purpose |
|---|---|---|---|---|
| POST | `/api/auth/register` | none | auth | Create account, send verification email |
| POST | `/api/auth/login` | none | auth | Authenticate (requires verified email) |
| POST | `/api/auth/refresh` | refresh cookie | auth | Rotate refresh token, issue new access token |
| POST | `/api/auth/logout` | refresh cookie | none | Revoke refresh token, clear cookie |
| GET | `/api/auth/me` | Bearer | none | Current user (safe fields only) |
| POST | `/api/auth/verify-email` | none | none | Confirm email with token |
| POST | `/api/auth/resend-verification` | none | email | Resend verification email |
| POST | `/api/auth/forgot-password` | none | email | Request password reset email |
| POST | `/api/auth/reset-password` | none | email | Reset password with token |
| GET | `/api/auth/external/{provider}` | none | auth | Begin Google/GitHub login |
| GET | `/api/auth/external/callback` | external cookie | none | Complete external login |

## Token lifecycle (login → API → refresh → logout)

1. `register` → user created, verification email sent, auth response returned.
2. `verify-email` → email confirmed, user can now log in.
3. `login` → access token in JSON body (kept in React memory), refresh
   token in HttpOnly cookie.
4. API calls send `Authorization: Bearer <access>`.
5. On `401`, the client silently calls `refresh` (cookie) once, then retries.
6. On app reload, the client calls `refresh` to restore the session (access token is
   not persisted anywhere).
7. `logout` revokes the refresh token server-side and clears the cookie.

## Google & GitHub OAuth

- Implemented with the framework's generic `AddOAuth` handler (no provider NuGet
  packages), each registered only when its ClientId/ClientSecret are configured.
- Backend-driven: SPA sends the browser to `/api/auth/external/{provider}`; after the
  provider round-trip the backend issues the refresh cookie and redirects to the SPA's
  `/oauth/callback`, which calls `refresh` to obtain an access token. The access token
  is never placed in a URL.
- OAuth-created accounts have `EmailConfirmed = true` (provider-verified email).
- **Account-linking policy** (in `AuthController.ExternalCallback`):
  1. Already-linked external login → sign in.
  2. No email from provider → refuse (`email_required`).
  3. Email matches an existing account → refuse to auto-link (`account_exists`);
     explicit linking after password login is a future phase (avoids account takeover).
  4. New, provider-**verified** email → create a Visitor account and link the login.
  5. New but unverified email → refuse (`email_unverified`).

See `DEVELOPMENT.md` for provider setup and callback URLs.
