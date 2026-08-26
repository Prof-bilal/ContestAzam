# ARCHITECTURE.md — System Architecture

## Overview

EventSphere is a **decoupled full-stack application** with a React SPA frontend and ASP.NET Core Web API backend.

## Architecture Style

**Client-Server Architecture**:

```
React SPA (Frontend)          ASP.NET Core Web API (Backend)
├── React 18+ (Vite)         ├── API Controllers
├── React Router              ├── Services (Business Logic)
├── Fetch API (no Axios)      ├── Entity Framework Core
├── Context API (Auth)        ├── ASP.NET Core Identity
└── TypeScript                ├── JWT + Refresh Tokens (HttpOnly)
                              └── SQL Server
```

## Authentication Architecture

```
React (SPA)
    ↓
ASP.NET Core Web API
    ↓
ASP.NET Core Identity
    ↓
JWT Access Token (15 min) + Refresh Token (HttpOnly cookie, 7 days)
    ↓
SQL Server

External OAuth:
Google / GitHub
    ↓
ASP.NET Core OAuth
    ↓
ASP.NET Core Identity
    ↓
EventSphere JWT
    ↓
React
```

### Role Model

| Role | Assignment | Description |
|---|---|---|
| **Visitor** | Default on registration | Can browse events, manage profile, register for events |
| **Participant** | Auto-assigned after event registration | Has participated in at least one event |
| **Organizer** | Admin approval required | Can create and manage events |
| **Admin** | Provisioned securely only | Full platform administration |

### Registration Flow

```
Register → Choose "Visitor" or "Organizer"
    ↓
Visitor → Create User → Assign Visitor Role → Email Verification → Login
    ↓
Organizer → Create User → Assign Visitor Role → Create OrganizerRequest (Pending)
    → Email Verification → Admin Review → Approve/Reject
```

### Participant Flow

```
Visitor → Register for Event → Backend Creates EventRegistration
    → Backend Assigns Participant Role → Transaction Committed
```

### OAuth Flow

```
React → Continue with Google/GitHub → Backend OAuth Endpoint
    → Provider → Callback → Validate Identity
    → Find/Create User (Visitor role) → Generate JWT + Refresh Token
    → Redirect to React → Authenticated Session
```

## Project Structure

```
EventSphere/
├── EventSphere.Api/              # ASP.NET Core Web API
│   ├── Auth/                     # OAuth configuration
│   │   ├── ExternalAuth.cs
│   │   └── ExternalAuthExtensions.cs
│   ├── Controllers/
│   │   ├── AuthController.cs     # Register, Login, Refresh, OAuth, etc.
│   │   ├── AdminController.cs    # Admin organizer request management
│   │   ├── EventsController.cs   # Event listing + registration
│   │   └── RolesDemoController.cs # Authorization demo
│   ├── Services/
│   │   ├── TokenService.cs       # JWT generation
│   │   ├── RefreshTokenService.cs # Refresh token lifecycle
│   │   ├── BrevoEmailService.cs  # Brevo email integration
│   │   └── NoOpEmailService.cs   # Test double
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   ├── DbSeeder.cs           # Role seeding
│   │   └── Configurations/       # EF Fluent API
│   ├── Models/
│   │   ├── AppUser.cs            # IdentityUser<int>
│   │   ├── RefreshToken.cs
│   │   ├── OrganizerRequest.cs   # Organizer approval workflow
│   │   ├── Registration.cs       # Event registration
│   │   ├── Event.cs
│   │   └── Enums.cs
│   ├── DTOs/
│   │   ├── RegisterRequest.cs    # AccountType field (Visitor/Organizer)
│   │   ├── LoginRequest.cs
│   │   ├── AuthResponse.cs
│   │   ├── EmailDtos.cs
│   │   └── OrganizerRequestDtos.cs
│   ├── Common/
│   │   ├── AppRoles.cs           # Role constants
│   │   ├── ApiResponse.cs        # Uniform API envelope
│   │   ├── RoleMapping.cs
│   │   └── Options/
│   ├── Middleware/
│   ├── Program.cs
│   └── appsettings.json
├── EventSphere.Tests/            # xUnit integration tests
│   ├── RegistrationTests.cs
│   ├── LoginTests.cs
│   ├── TokenTests.cs
│   ├── AuthorizationTests.cs
│   ├── EmailVerificationTests.cs
│   ├── PasswordResetTests.cs
│   ├── RateLimitingTests.cs
│   ├── OrganizerRequestTests.cs  # NEW
│   └── EventRegistrationTests.cs # NEW
├── client/                       # React SPA (Vite)
│   └── src/
│       ├── auth/AuthContext.tsx
│       ├── api/client.ts
│       ├── pages/
│       │   ├── Login.tsx
│       │   ├── Register.tsx      # Account type selector
│       │   ├── OAuthCallback.tsx
│       │   ├── VerifyEmail.tsx
│       │   ├── ForgotPassword.tsx
│       │   ├── ResetPassword.tsx
│       │   └── Dashboard.tsx
│       └── components/
│           ├── ProtectedRoute.tsx
│           └── PasswordRequirements.tsx
├── EventSphere.slnx
└── .agent/
```

## API Endpoints

### Auth (Public)

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/auth/register` | Register (Visitor or Organizer) |
| POST | `/api/auth/login` | Login with email/password |
| POST | `/api/auth/forgot-password` | Request password reset email |
| POST | `/api/auth/reset-password` | Reset password with token |
| POST | `/api/auth/verify-email` | Verify email with token |
| POST | `/api/auth/resend-verification` | Resend verification email |
| GET | `/api/auth/external/{provider}` | Initiate OAuth (Google/GitHub) |
| GET | `/api/auth/external/callback` | OAuth callback |

### Auth (Authenticated)

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/auth/refresh` | Refresh access token |
| POST | `/api/auth/logout` | Logout (revoke refresh token) |
| GET | `/api/auth/me` | Get current user info |
| POST | `/api/auth/organizer-requests` | Submit organizer request |
| GET | `/api/auth/organizer-requests/me` | Check organizer request status |

### Events (Public)

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/events` | List approved events |
| GET | `/api/events/{id}` | Get event details |

### Events (Authenticated)

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/events/{id}/register` | Register for event (assigns Participant) |
| DELETE | `/api/events/{id}/register` | Cancel event registration |

### Admin (Admin only)

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/admin/organizer-requests` | List organizer requests |
| GET | `/api/admin/organizer-requests/{id}` | Get specific request |
| POST | `/api/admin/organizer-requests/{id}/approve` | Approve organizer |
| POST | `/api/admin/organizer-requests/{id}/reject` | Reject organizer |

## Request Lifecycles

### Registration (Visitor)
```
React Register Form → POST /api/auth/register
→ Validate AccountType = Visitor
→ Create AppUser → Assign Visitor Role
→ Generate Email Verification Token → Send via Brevo
→ Issue JWT + Refresh Token → Return to React
```

### Registration (Organizer)
```
React Register Form → POST /api/auth/register (AccountType = Organizer)
→ Validate AccountType + organizer fields
→ Create AppUser → Assign Visitor Role
→ Create OrganizerRequest (Status = Pending)
→ Generate Email Verification Token → Send via Brevo
→ Issue JWT + Refresh Token → Return to React
→ Admin reviews → Approve → Assign Organizer Role
```

### Event Registration (Participant)
```
React Event Details → POST /api/events/{id}/register
→ Validate: authenticated, event exists, open, capacity, no duplicate
→ Create EventRegistration
→ Assign Participant Role (if not already assigned)
→ Commit transaction
```

### OAuth Login
```
React → GET /api/auth/external/{provider}
→ Backend redirects to Google/GitHub
→ Provider callback → GET /api/auth/external/callback
→ Validate identity → Find/Create User (Visitor role)
→ Issue Refresh Cookie → Redirect to React /oauth/callback
→ React calls /api/auth/refresh → Gets JWT
```

## Dependencies

### Backend
```
Microsoft.AspNetCore.Identity.EntityFrameworkCore
Microsoft.EntityFrameworkCore.SqlServer
Microsoft.AspNetCore.Authentication.JwtBearer
Microsoft.AspNetCore.Cors
Microsoft.AspNetCore.RateLimiting
Swashbuckle.AspNetCore (Swagger)
```

### Frontend
```
react 18, react-dom, react-router-dom 6
typescript 5, vite 5
(no axios — uses native fetch)
```
