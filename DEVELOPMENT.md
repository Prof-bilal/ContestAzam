# DEVELOPMENT.md — EventSphere Auth Setup

Cross-platform (Linux & Windows). One `.csproj` for the API; no OS-specific projects.

## Prerequisites

- .NET SDK 10
- Node.js 20+ and npm
- SQL Server reachable via the configured connection string (LocalDB, a container, or a server)

## Configuration & secrets

Never commit real secrets. Use **User Secrets** in development:

```bash
cd EventSphere.Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=EventSphereDb;User Id=sa;Password=<pw>;TrustServerCertificate=True;"
dotnet user-secrets set "Jwt:Key" "<a-random-string-of-at-least-32-bytes>"
dotnet user-secrets set "Authentication:Google:ClientId" "<id>"
dotnet user-secrets set "Authentication:Google:ClientSecret" "<secret>"
dotnet user-secrets set "Authentication:GitHub:ClientId" "<id>"
dotnet user-secrets set "Authentication:GitHub:ClientSecret" "<secret>"
```

In production, provide the same keys via environment variables or a secret manager, e.g.
`Jwt__Key`, `Authentication__Google__ClientSecret` (double underscore = nesting).

### JWT development configuration

- If `Jwt:Key` is empty in **Development** (or **Testing**), the app generates a random
  ephemeral key at startup (tokens won't survive a restart). Set a stable key via User
  Secrets if you want tokens to persist across restarts.
- Outside Development, an empty or too-short (`< 32 bytes`) key **fails startup** by design.

## Brevo email setup

Brevo is the transactional email provider. Configure via User Secrets or environment variables:

```bash
# User Secrets (development)
dotnet user-secrets set "Brevo:ApiKey" "<your-brevo-api-key>"
dotnet user-secrets set "Brevo:SenderEmail" "noreply@yourdomain.com"
dotnet user-secrets set "Brevo:SenderName" "EventSphere"
```

Or via environment variables:

```bash
export BREVO_API_KEY="<your-brevo-api-key>"
export BREVO_SENDER_EMAIL="noreply@yourdomain.com"
export BREVO_SENDER_NAME="EventSphere"
```

**Never commit the actual Brevo API key.** The `appsettings.json` files contain
empty placeholder values only.

### Brevo setup steps

1. Create a Brevo account at https://app.brevo.com
2. Go to **SMTP & API** → **API Keys** → Generate a new API key
3. Go to **SMTP & API** → **Senders** → Add and verify your sender email
4. Configure the API key and sender details via User Secrets or environment variables

### Brevo in testing

The test environment uses a `NoOpEmailService` that records emails without sending them.
No Brevo credentials are needed to run tests.

## Run the API (HTTPS)

```bash
cd EventSphere.Api
dotnet dev-certs https --trust     # once, so the browser/SPA trusts localhost HTTPS
dotnet restore
dotnet run --launch-profile https  # https://localhost:7054 (+ http://localhost:5244)
```

On startup the app applies EF migrations and seeds the four roles (idempotent). Swagger
is at `https://localhost:7054/swagger` in Development.

### Optional development admin

```bash
dotnet user-secrets set "SeedAdmin:Enabled" "true"
dotnet user-secrets set "SeedAdmin:Email" "admin@example.com"
dotnet user-secrets set "SeedAdmin:Password" "<strong-password>"
```
Disabled by default; credentials come from configuration only.

## Run the React auth client

```bash
cd client
cp .env.example .env      # VITE_API_BASE_URL=https://localhost:7054
npm install
npm run dev               # http://localhost:5173
```

The dev origin `http://localhost:5173` is pre-listed in `Frontend:AllowedOrigins`
(Development). The refresh cookie uses `SameSite=None; Secure` in Development so it flows
from the SPA (5173) to the HTTPS API (7054); this requires the trusted dev cert above.

## Database migrations

```bash
cd EventSphere.Api
dotnet ef migrations add <Name>
dotnet ef database update
```
The latest auth migration is **`AddAuthenticationSystem`** (adds the `RefreshTokens`
table). A design-time factory (`DesignTimeDbContextFactory`) lets tooling build the
context without running the app or a live database.

## Google OAuth setup

1. Google Cloud Console → APIs & Services → **Credentials** → Create OAuth client ID
   (type: Web application).
2. Authorized redirect URI: `https://localhost:7054/signin-google`
   (production: `https://<api-host>/signin-google`).
3. Put the Client ID/Secret in `Authentication:Google` (User Secrets).

## GitHub OAuth setup

1. GitHub → Settings → Developer settings → **OAuth Apps** → New OAuth App.
2. Authorization callback URL: `https://localhost:7054/signin-github`.
3. Put the Client ID/Secret in `Authentication:GitHub`. Scopes requested: `read:user`,
   `user:email` (needed to read a verified email).

## React callback URLs

- The SPA never registers with the providers directly. After the provider round-trip the
  **backend** redirects to the SPA at `Frontend:PostLoginRedirectPath` (default
  `/oauth/callback`) on success, or `PostLoginErrorPath` (default `/login?error=...`) on
  failure. Adjust these paths under `Frontend` if your routes differ.

## Tests

```bash
dotnet test                 # from the repo root (uses EventSphere.sln)
```
Integration tests (`EventSphere.Tests`) run the API in-process with
`WebApplicationFactory` over an EF Core **in-memory** database (no SQL Server needed).
Test config (JWT key, rate limits) is injected via environment variables so it is visible
to startup config reads.

The test environment uses `NoOpEmailService` which records emails without sending them.
Email verification and password reset tests work by extracting tokens from the recorded
email records.

## Windows vs Linux

- Identical commands; paths in the code are OS-agnostic. On Windows PowerShell, env-var
  nesting also uses double underscores (`$env:Jwt__Key = "..."`).
- SQL Server: use LocalDB (`Server=(localdb)\\MSSQLLocalDB;...`) on Windows or a container
  on Linux (`docker run -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD=<pw> -p 1433:1433 mcr.microsoft.com/mssql/server`).
