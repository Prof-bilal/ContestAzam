# OAuth Setup Guide — Google & GitHub

EventSphere supports Google and GitHub OAuth for seamless sign-in. Both providers use ASP.NET Core's generic OAuth handler — no provider-specific NuGet packages required.

## How It Works

```
React "Continue with Google/GitHub" button
  → GET /api/auth/external/{provider}
  → Backend redirects to Google/GitHub
  → User authenticates with provider
  → Provider redirects back to /signin-google or /signin-github
  → Backend validates identity → Creates user (Visitor role) → Issues JWT
  → Redirects to React /oauth/callback → Authenticated session
```

## Prerequisites

- Backend running on `http://localhost:5244`
- Frontend running on `http://localhost:5173`
- `Frontend:AllowedOrigins` includes `http://localhost:5173`

---

## Google OAuth Setup

### Step 1: Create Google Cloud Project

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Click **Select a project** → **New Project**
3. Name: `EventSphere` → Click **Create**
4. Select the new project

### Step 2: Configure OAuth Consent Screen

1. Go to **APIs & Services** → **OAuth consent screen**
2. Select **External** user type → Click **Create**
3. Fill in:
   - **App name**: `EventSphere`
   - **User support email**: your email
   - **Developer contact email**: your email
4. Click **Save and Continue**
5. **Scopes**: Click **Add or Remove Scopes** → Select `email`, `openid`, `profile` → **Update** → **Save and Continue**
6. **Test users**: Add your Google account email (required while app is in testing mode)
7. Click **Save and Continue** → **Back to Dashboard**

### Step 3: Create OAuth Credentials

1. Go to **APIs & Services** → **Credentials**
2. Click **Create Credentials** → **OAuth client ID**
3. **Application type**: `Web application`
4. **Name**: `EventSphere Web Client`
5. **Authorized JavaScript origins**:
   ```
   http://localhost:5173
   ```
6. **Authorized redirect URIs**:
   ```
   http://localhost:5244/signin-google
   ```
7. Click **Create**
8. **Copy** the `Client ID` and `Client Secret`

### Step 4: Configure Backend

Set environment variables (or use User Secrets / appsettings.Development.json):

```bash
# Environment variables
export Authentication__Google__ClientId="YOUR_CLIENT_ID.apps.googleusercontent.com"
export Authentication__Google__ClientSecret="YOUR_CLIENT_SECRET"
```

Or in `appsettings.Development.json`:
```json
{
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_CLIENT_ID.apps.googleusercontent.com",
      "ClientSecret": "YOUR_CLIENT_SECRET"
    }
  }
}
```

### Google URLs Summary

| Setting | Value |
|---|---|
| **Authorized JavaScript origins** | `http://localhost:5173` |
| **Authorized redirect URIs** | `http://localhost:5244/signin-google` |
| **Backend callback path** | `/signin-google` (configured in `ExternalAuthExtensions.cs`) |
| **Frontend callback** | `http://localhost:5173/oauth/callback` |

---

## GitHub OAuth Setup

### Step 1: Create GitHub OAuth App

1. Go to [GitHub Settings](https://github.com/settings/profile)
2. Click **Developer settings** (bottom of left sidebar)
3. Click **OAuth Apps** → **New OAuth App**
4. Fill in:
   - **Application name**: `EventSphere`
   - **Homepage URL**: `http://localhost:5173`
   - **Authorization callback URL**: `http://localhost:5244/signin-github`
5. Click **Register application**
6. **Copy** the `Client ID`
7. Click **Generate a new client secret**
8. **Copy** the `Client Secret` immediately (it won't be shown again)

### Step 2: Configure Backend

Set environment variables:

```bash
# Environment variables
export Authentication__GitHub__ClientId="YOUR_GITHUB_CLIENT_ID"
export Authentication__GitHub__ClientSecret="YOUR_GITHUB_CLIENT_SECRET"
```

Or in `appsettings.Development.json`:
```json
{
  "Authentication": {
    "GitHub": {
      "ClientId": "YOUR_GITHUB_CLIENT_ID",
      "ClientSecret": "YOUR_GITHUB_CLIENT_SECRET"
    }
  }
}
```

### GitHub URLs Summary

| Setting | Value |
|---|---|
| **Homepage URL** | `http://localhost:5173` |
| **Authorization callback URL** | `http://localhost:5244/signin-github` |
| **Backend callback path** | `/signin-github` (configured in `ExternalAuthExtensions.cs`) |
| **Frontend callback** | `http://localhost:5173/oauth/callback` |

---

## Production Configuration

For production, replace `localhost` with your actual domains:

| Setting | Development | Production |
|---|---|---|
| **Frontend URL** | `http://localhost:5173` | `https://yourdomain.com` |
| **Backend URL** | `http://localhost:5244` | `https://api.yourdomain.com` |
| **Google JS Origins** | `http://localhost:5173` | `https://yourdomain.com` |
| **Google Redirect URI** | `http://localhost:5244/signin-google` | `https://api.yourdomain.com/signin-google` |
| **GitHub Callback URL** | `http://localhost:5244/signin-github` | `https://api.yourdomain.com/signin-github` |

### Production Environment Variables

```bash
export Authentication__Google__ClientId="production_client_id"
export Authentication__Google__ClientSecret="production_client_secret"
export Authentication__GitHub__ClientId="production_github_client_id"
export Authentication__GitHub__ClientSecret="production_github_client_secret"
export Frontend__AllowedOrigins__0="https://yourdomain.com"
export Jwt__Key="YOUR_PRODUCTION_JWT_KEY_AT_LEAST_32_BYTES"
```

---

## How the Backend OAuth Flow Works

1. **React** calls `GET /api/auth/external/google` (or `github`)
2. **Backend** creates OAuth challenge → redirects to provider
3. **Provider** authenticates user → redirects to `/signin-google` (or `/signin-github`)
4. **Backend** (`ExternalCallback` action):
   - Validates the external identity
   - Checks if the provider login is already linked → sign in
   - Checks if email already exists → block (prevents account takeover)
   - Creates new user with **Visitor role**
   - Links the external login
   - Issues refresh cookie
   - Redirects to `http://localhost:5173/oauth/callback`
5. **React** (`OAuthCallback` page):
   - Calls `restoreSession()` → `POST /api/auth/refresh` → gets JWT
   - Navigates to `/dashboard`

## Security Notes

- OAuth secrets are **backend-only** — never exposed to React
- New OAuth users always receive the **Visitor** role
- Existing accounts are **not silently linked** (prevents account takeover)
- Provider emails must be **verified** by the provider
- OAuth state is validated to prevent CSRF
- The provider's access token is **never used** as the EventSphere JWT

## Troubleshooting

| Problem | Solution |
|---|---|
| "Provider not available" | Check `ClientId` and `ClientSecret` are set and not empty |
| "OAuth failed" callback | Check redirect URI matches exactly (including `http://` vs `https://`) |
| "Email required" | GitHub user has no public/verified email — ask user to set a public email on GitHub |
| "Email unverified" | Google/GitHub didn't confirm email verification — check provider config |
| CORS error on callback | Add `http://localhost:5173` to `Frontend:AllowedOrigins` |
