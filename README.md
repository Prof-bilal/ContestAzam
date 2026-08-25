# EventSphere

College Event Information System — a full-stack web application for managing college events, registrations, attendance, certificates, and feedback.

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core 10 Web API |
| ORM | Entity Framework Core 10 |
| Database | Microsoft SQL Server |
| Auth | ASP.NET Core Identity + JWT Bearer |
| API Docs | Swagger / OpenAPI |
| Frontend | React 18 (Vite) — `client/` |

## Project Structure

```
EventSphere/
├── EventSphere.Api/        # ASP.NET Core 10 Web API
│   ├── Controllers/
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   └── Configurations/
│   ├── Models/
│   ├── Migrations/
│   ├── Program.cs
│   └── appsettings.json
├── client/                  # React frontend (coming soon)
├── .agent/                  # AI agent documentation
├── SRS.md                   # Software Requirements Specification
└── README.md
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

### 3. Run the API

```bash
dotnet restore
dotnet build
dotnet run --project EventSphere.Api
```

API starts at `https://localhost:5001` (or `http://localhost:5000`).

Swagger UI: `https://localhost:5001/swagger`

### 4. Apply database migrations

```bash
dotnet ef database update --project EventSphere.Api
```

## Database

22 tables including:

- **Users** — Identity users with roles (Admin, Organizer, Participant)
- **Events** — Events with categories, dates, venues
- **Registrations** — Student event registrations
- **Attendance** — QR code check-in tracking
- **Feedback** — Star ratings + comments
- **Certificates** — Certificate URLs and issue dates
- **MediaGallery** — Event images and videos
- **EventSeating** — Venue capacity management
- **EventWaitlist** — Automatic waitlist management
- **CalendarSync** — .ics calendar integration
- **EventShareLog** — Social media sharing tracking
- **Notifications** — User notifications

## User Roles

| Role | Description |
|---|---|
| **Admin** | Manages users, approves events, moderates content |
| **Organizer** | Creates/manages events, uploads media, issues certificates |
| **Participant** | Registers for events, submits feedback, downloads certificates |

## API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/swagger` | API documentation |
| `POST` | `/api/auth/register` | Register new user |
| `POST` | `/api/auth/login` | Login and get JWT token |

> Full API endpoints coming soon.

## Environment Variables

| Variable | Description | Default |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | SQL Server connection string | — |
| `Jwt__Key` | JWT signing key (32+ chars) | — |
| `Jwt__Issuer` | JWT issuer | EventSphere |
| `Jwt__Audience` | JWT audience | EventSphere |
| `Jwt__ExpirationInMinutes` | Token expiry | 60 |

## Team

| Member | Role | Module |
|---|---|---|
| Abdullah | Backend | Module 1 — Backend Core & Architecture |
| Jibran | Backend | Module 2 — Database + Data-Heavy Backend |
| Ramsha | Frontend | Module 3 — Frontend Core + Shared UI |
| Marukh | Frontend | Module 4 — Frontend Features + Dashboards |

## License

This project is for educational purposes.
