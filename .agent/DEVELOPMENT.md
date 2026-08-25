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
# Default: Server=(localdb)\\mssqllocaldb;Database=EventSphereDb_Dev;...

# Run migrations
dotnet ef database update --project EventSphere.Web

# Seed database (optional, via UI)
# Navigate to /Database/Seed and submit form

# Run application
dotnet run --project EventSphere.Web
```

Application starts at `https://localhost:5001` or `http://localhost:5000`.

## Default Accounts

| Role | Email | Password |
|---|---|---|
| Admin | admin@eventsphere.com | Admin@123 |
| Organizer | organizer@eventsphere.com | Organizer@123 |

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

# Remove last migration
dotnet ef migrations remove --project EventSphere.Web

# Check for vulnerabilities
dotnet list package --vulnerable
```

## Project Structure

```
EventSphere/
├── EventSphere.sln
├── EventSphere.Web/         # Main application
│   ├── Controllers/         # MVC + API controllers
│   ├── Data/                # DbContext, Seed
│   ├── Hubs/                # SignalR hubs
│   ├── Models/Entities/     # Domain models
│   ├── Services/            # Business logic
│   ├── ViewModels/          # View models
│   ├── Views/               # Razor views
│   └── wwwroot/             # Static files
├── EventSphere.Tests/       # Test project
└── .agent/                  # This documentation
```

## Troubleshooting

### Build fails
- Run `dotnet restore` first.
- Check .NET SDK version: `dotnet --list-sdks`.
- Ensure SQL Server is running (for LocalDB).

### Database connection fails
- Verify SQL Server is running.
- Check connection string in `appsettings.Development.json`.
- Try: `sqlcmd -S (localdb)\\mssqllocaldb`

### Migration errors
- Remove `bin/` and `obj/` folders.
- Re-run `dotnet restore`.
- Try `dotnet ef migrations add Reset --project EventSphere.Web`.
