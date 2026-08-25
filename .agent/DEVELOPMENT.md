# DEVELOPMENT.md — Local Development Setup

## Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB or full instance)
- IDE: Visual Studio 2022, JetBrains Rider, or VS Code
- Git

## Setup

```bash
# Clone repository
git clone <repo-url>
cd EventSphere

# Restore packages
dotnet restore

# Update connection string in appsettings.Development.json

# Run migrations
dotnet ef database update --project EventSphere.Web

# Run application
dotnet run --project EventSphere.Web
```

Application starts at `https://localhost:5001` or `http://localhost:5000`.

## Default Accounts

| Role | Email | Password |
|---|---|---|
| Admin | admin@eventsphere.com | Admin@123 |
| Organizer | organizer@eventsphere.com | Organizer@123 |
| Participant | participant@eventsphere.com | Participant@123 |

## Common Commands

```bash
# Build
dotnet build

# Run
dotnet run --project EventSphere.Web

# Run tests
dotnet test

# Add migration
dotnet ef migrations add <Name> --project EventSphere.Web

# Update database
dotnet ef database update --project EventSphere.Web

# Check vulnerabilities
dotnet list package --vulnerable
```

## Team Setup

Each team member works on their own feature branch:

```bash
# Abdullah
git checkout -b feature/abdullah-auth

# Jibran
git checkout -b feature/jibran-database

# Ramsha
git checkout -b feature/ramsha-layout

# Marukh
git checkout -b feature/marukh-sitemap
```

See `.agent/PHASES.md` for full phase breakdown.

## Troubleshooting

### Build fails
- Run `dotnet restore` first.
- Check .NET SDK version: `dotnet --list-sdks`.

### Database connection fails
- Verify SQL Server is running.
- Check connection string in `appsettings.Development.json`.

### Migration errors
- Remove `bin/` and `obj/` folders.
- Re-run `dotnet restore`.
