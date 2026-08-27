# EventSphere

College Event Information System — a full-stack web application for managing college events, registrations, attendance, certificates, feedback, and real-time communication.

**Status:** ~89% implementation complete | 84 API endpoints | 38 React pages | 125 backend tests passing

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core 10 Web API |
| ORM | Entity Framework Core 10 |
| Database | Microsoft SQL Server |
| Auth | ASP.NET Core Identity + JWT Bearer + OAuth (Google/GitHub) |
| Real-time | SignalR (notifications + messaging) |
| Payments | Stripe Checkout |
| Email | Brevo transactional email |
| QR Codes | QRCoder library |
| Frontend | React 18 + TypeScript (Vite) |
| State | Context API (Auth + Realtime) |
| API Client | Native fetch (no Axios) |

## Features

### Authentication & Authorization
- Registration (Visitor / Organizer with admin approval)
- Login with JWT access token + HttpOnly refresh cookie
- Email verification via Brevo
- Password reset flow
- Google OAuth + GitHub OAuth with account linking
- Account lockout (5 failed attempts / 15 min)
- Rate limiting (auth, email, messaging, general)
- Account suspension by admin

### Event Management
- Full CRUD with status lifecycle (Draft → PendingApproval → Approved/Rejected/Cancelled)
- Search, filter (category, date range, location), sort, pagination
- Image upload (5MB max)
- Category management
- Paid / Free events with Stripe integration
- Registration deadline enforcement

### Registration & Attendance
- Free and paid event registration
- Duplicate prevention, capacity enforcement
- Digital pass with QR code (QRCoder)
- Camera QR scanning + manual token entry
- Event-day check-in window (-24h to +6h)
- Duplicate check-in prevention
- Attendance statistics
- Manual check-in by organizer

### Notifications & Messaging
- In-app notifications (SignalR + DB persist)
- Email notifications (Brevo, 13 templates)
- Background event reminders (24h + <1h)
- 16 notification types
- Real-time 1:1 messaging (SignalR)
- Read state, unread counts

### Social & Calendar
- Social media sharing (Facebook, WhatsApp, Twitter, LinkedIn, Email)
- In-app calendar with month view
- .ics export per event

### Admin
- Dashboard with analytics
- Organizer request management (approve/reject)
- User management (suspend, warn, assign roles)
- Event approval/rejection
- Content moderation (reviews)
- System-wide announcements
- CSV report export

### Profile & Engagement
- Profile view/edit with image upload
- Password change, account deletion
- Favorites/bookmarks with category notifications
- Reviews (1–5 stars + comments)
- Certificates (organizer upload, participant listing)

## Project Structure

```
EventSphere/
├── EventSphere.Api/              # ASP.NET Core 10 Web API
│   ├── Auth/                     # OAuth configuration
│   ├── Controllers/              # 10 API controllers
│   ├── Services/                 # Business logic services
│   ├── Data/                     # DbContext + EF configurations
│   ├── Models/                   # Entity models (21)
│   ├── DTOs/                     # Request/response DTOs
│   ├── Hubs/                     # SignalR hubs (notifications, messaging)
│   ├── Middleware/                # Security headers, exception handler
│   ├── Migrations/               # 10 EF migrations
│   └── Program.cs                # App configuration
├── client/                       # React 18 SPA (Vite + TypeScript)
│   └── src/
│       ├── pages/                # 38 pages
│       ├── components/           # Shared components
│       ├── auth/                 # AuthContext
│       ├── api/                  # API client (fetch)
│       ├── realtime/             # SignalR RealtimeContext
│       └── hooks/                # Custom hooks
├── EventSphere.Tests/            # xUnit integration tests (125 tests)
├── .agent/                       # AI agent documentation
├── SRS.md                        # Software Requirements Specification
└── PROGRESS_REPORT.md            # Full progress report
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server) (local or Docker)
- [Node.js 18+](https://nodejs.org/) (for frontend)

## Quick Start

### 1. Clone the repository

```bash
git clone https://github.com/Prof-bilal/ContestAzam.git
cd ContestAzam
```

### 2. Configure database

Update the connection string in `EventSphere.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EventSphereDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  }
}
```

### 3. Configure optional services

In `EventSphere.Api/appsettings.json`, set:

```json
{
  "Jwt": { "Key": "YOUR_SECRET_KEY_32_CHARS_MINIMUM" },
  "Authentication": {
    "Google": { "ClientId": "...", "ClientSecret": "..." },
    "GitHub": { "ClientId": "...", "ClientSecret": "..." }
  },
  "Brevo": { "ApiKey": "...", "SenderEmail": "...", "SenderName": "EventSphere" },
  "Stripe": { "SecretKey": "...", "PublishableKey": "...", "WebhookSecret": "..." }
}
```

### 4. Run the API

```bash
dotnet restore
dotnet build
dotnet run --project EventSphere.Api
```

API starts at `https://localhost:5001`. Swagger UI: `/swagger`

### 5. Run the frontend

```bash
cd client
npm install
npm run dev
```

Frontend starts at `http://localhost:5173`

### 6. Apply database migrations

Migrations auto-apply on startup. Or manually:

```bash
dotnet ef database update --project EventSphere.Api
```

## Database Schema

20+ tables including: Users, UserDetails, Events, EventCategories, Registrations, Attendances, Feedback, Certificates, MediaGallery, Venues, EventSeating, EventWaitlist, RefreshTokens, OrganizerRequests, Favorites, Payments, Notifications, Conversations, ConversationParticipants, Messages, CalendarSync, EventShareLog.

## User Roles

| Role | Assignment | Description |
|---|---|---|
| **Visitor** | Default on registration | Browse events, manage profile, register |
| **Participant** | Auto-assigned after event registration | Has attended at least one event |
| **Organizer** | Admin approval required | Create and manage events |
| **Admin** | Provisioned via config only | Full platform administration |

## API Endpoints (84 total)

| Controller | Endpoints | Auth |
|---|---|---|
| Auth | 12 | Public + Authenticated |
| Events | 10 | Public + Organizer/Admin |
| Organizer | 14 | Organizer only |
| Participant | 8 | Authenticated |
| Admin | 14 | Admin only |
| Profile | 6 | Authenticated |
| Notifications | 5 | Authenticated |
| Conversations | 6 | Authenticated |
| Payment | 4 | Authenticated + Public |

## Environment Variables

| Variable | Description | Required |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | SQL Server connection string | Yes |
| `Jwt__Key` | JWT signing key (32+ chars) | Yes (prod) |
| `Authentication__Google__ClientId` | Google OAuth client ID | No |
| `Authentication__GitHub__ClientId` | GitHub OAuth client ID | No |
| `Brevo__ApiKey` | Brevo email API key | No (NoOp in dev) |
| `Stripe__SecretKey` | Stripe secret key | No (disabled without key) |

## Running Tests

```bash
dotnet test
```

125 integration tests covering: registration, login, tokens, authorization, email verification, password reset, rate limiting, organizer requests, event CRUD, event registration, notifications, messaging.

## Team

| Member | Role | Module |
|---|---|---|
| Abdullah | Backend | Module 1 — Backend Core & Architecture |
| Jibran | Backend | Module 2 — Database + Data-Heavy Backend |
| Ramsha | Frontend | Module 3 — Frontend Core + Shared UI |
| Marukh | Frontend | Module 4 — Frontend Features + Dashboards |

## Reports

- [PROGRESS_REPORT.md](PROGRESS_REPORT.md) — Full progress report with module-by-module breakdown, completion percentages, and remaining work
- [SRS.md](SRS.md) — Software Requirements Specification

## License

This project is for educational purposes.
