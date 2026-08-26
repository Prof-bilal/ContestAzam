# API.md — Authentication Endpoints

Base URL (dev, HTTPS profile): `https://localhost:7054`. All responses use the
envelope `{ "success": bool, "message": string, "errors"?: { field: [msg] }, "data"?: T }`.

## POST /api/auth/register

Create an account. Always assigned the `Visitor` role server-side. Sends a verification
email through Brevo. Returns auth tokens (user can be authenticated but must verify email).

Request:
```json
{ "name": "Jane Doe", "email": "jane@example.com", "password": "Str0ng!Passw0rd#2025", "confirmPassword": "Str0ng!Passw0rd#2025" }
```
Responses: `201` `{ data: { accessToken, accessTokenExpiresAtUtc, user }, message: "Account created. Check your email to verify your account." }` + refresh
cookie · `400` validation errors · `409` duplicate email · `429` rate limited.
Any `role`/`isAdmin`/`isOrganizer` fields in the body are ignored.

## POST /api/auth/login

Requires a verified email. If email is not confirmed, returns `401` with
`emailVerificationRequired` flag.

```json
{ "email": "jane@example.com", "password": "Str0ng!Passw0rd#2025" }
```
Responses: `200` `{ data: AuthResponse }` + refresh cookie · `401` invalid credentials
(same message whether email exists or not) · `401` email not verified
(`emailVerificationRequired`) · `423` account locked · `400` missing
fields · `429` rate limited.

## POST /api/auth/refresh

No body. Reads the `es_refresh` HttpOnly cookie. Rotates the refresh token.

Responses: `200` `{ data: AuthResponse }` + new refresh cookie · `401` missing/expired/
revoked/reused token (cookie cleared) · `429` rate limited.

## POST /api/auth/logout

No body. Revokes the refresh token from the cookie and clears it. Always `200`
(idempotent). Works without a valid access token.

## GET /api/auth/me

Requires `Authorization: Bearer <accessToken>`. Returns safe fields only.

`200`:
```json
{ "success": true, "data": { "id": 1, "name": "Jane Doe", "email": "jane@example.com", "roles": ["Visitor"], "createdAt": "2026-01-01T00:00:00Z" } }
```
Never returns password hash, security stamp, refresh tokens, or internal Identity data.
`401` if unauthenticated/expired/tampered.

## POST /api/auth/verify-email

Confirm email with a verification token received via email.

```json
{ "email": "jane@example.com", "token": "<verification-token>" }
```
Responses: `200` email verified · `200` already verified · `400` invalid/expired token.

## POST /api/auth/resend-verification

Resend the verification email. Rate-limited via the `email` policy. Always returns the
same generic response regardless of whether the email exists (prevents email enumeration).

```json
{ "email": "jane@example.com" }
```
Responses: `200` generic success message · `429` rate limited.

## POST /api/auth/forgot-password

Request a password reset email. Rate-limited via the `email` policy. Always returns the
same generic response regardless of whether the email exists (prevents email enumeration).

```json
{ "email": "jane@example.com" }
```
Responses: `200` "If an account exists for this email, we sent a password reset link." ·
`429` rate limited.

## POST /api/auth/reset-password

Reset password with a token received via the forgot-password email. Validates the token,
password strength, and confirmation. Revokes all existing refresh tokens for the user.

```json
{ "email": "jane@example.com", "token": "<reset-token>", "newPassword": "N3w!P@ssw0rd#2026", "confirmPassword": "N3w!P@ssw0rd#2026" }
```
Responses: `200` password reset successfully · `400` invalid/expired token · `400`
password validation errors · `429` rate limited.

## GET /api/auth/external/{provider}

`provider` ∈ `google`, `github`. Redirects (302) to the provider. On completion the
backend sets the refresh cookie and redirects to the SPA `Frontend:PostLoginRedirectPath`
(default `/oauth/callback`); on failure to `PostLoginErrorPath` (default `/login`) with
`?error=<code>`. Error codes: `oauth_failed`, `account_exists`, `email_required`,
`email_unverified`, `provider_unavailable`, `account_disabled`.

## GET /api/auth/external/callback

Internal continuation of the OAuth flow (consumes the temporary external cookie). Not
called directly by the SPA.

## Demo authorization endpoints

`GET /api/demo/public` (anonymous), `/visitor` (any authenticated), `/participant`
(Participant+), `/organizer` (Organizer+), `/admin` (Admin). Return `200` when allowed,
`401` if unauthenticated, `403` if the role is insufficient.

## Rate limiting

| Policy | Endpoints | Default Limit |
|---|---|---|
| `auth` | register, login, refresh, external init | 10 requests / 60s |
| `email` | forgot-password, resend-verification, reset-password | 5 requests / 60s |
| General | all other traffic | 100 requests / 60s |

Rate limits are per client IP. Rejections return `429` with a `Retry-After` header.

## Status codes

`200` OK · `201` Created · `400` validation · `401` unauthenticated/invalid credentials
· `403` forbidden (role) · `409` conflict (duplicate) · `423` account locked · `429`
too many requests (`Retry-After` header) · `500` generic server error.
