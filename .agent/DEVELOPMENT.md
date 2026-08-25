# DEVELOPMENT.md — Local Development Setup

## Prerequisites

- .NET 8 SDK
- Node.js 18+ and npm
- SQL Server (LocalDB or full instance)
- IDE: VS Code (both), Visual Studio (backend), or JetBrains Rider
- Git

## Project Structure

```
EventSphere/
├── EventSphere.Api/          # ASP.NET Core Web API (backend)
├── EventSphere.React/        # React SPA (frontend)
├── EventSphere.Tests/        # Tests
├── EventSphere.sln
└── .agent/
```

## Backend Setup

```bash
# Restore packages
dotnet restore

# Update connection string in EventSphere.Api/appsettings.Development.json

# Run migrations
dotnet ef database update --project EventSphere.Api

# Run API
dotnet run --project EventSphere.Api
# API starts at https://localhost:5001
```

## Frontend Setup

```bash
cd EventSphere.React

# Install dependencies
npm install

# Configure API URL
# Create .env file:
echo "VITE_API_URL=http://localhost:5001" > .env
echo "VITE_SIGNALR_URL=http://localhost:5001/hubs/notifications" >> .env

# Run dev server
npm run dev
# React starts at http://localhost:5173
```

## Default Accounts

| Role | Email | Password |
|---|---|---|
| Admin | admin@eventsphere.com | Admin@123 |
| Organizer | organizer@eventsphere.com | Organizer@123 |
| Participant | participant@eventsphere.com | Participant@123 |

## Common Commands

```bash
# Build all
dotnet build EventSphere.sln
cd EventSphere.React && npm run build

# Run tests
dotnet test
cd EventSphere.React && npm test

# Add EF migration
dotnet ef migrations add <Name> --project EventSphere.Api

# Check vulnerabilities
dotnet list package --vulnerable
cd EventSphere.React && npm audit
```

## Troubleshooting

### API won't start
- Check connection string in `appsettings.Development.json`.
- Verify SQL Server is running.
- Run `dotnet restore`.

### React won't start
- Run `npm install`.
- Check `.env` for correct `VITE_API_URL`.
- Verify API is running on expected port.

### CORS errors
- Ensure API CORS policy includes `http://localhost:5173`.
- Check `Program.cs` CORS configuration.

### 401 Unauthorized from React
- Check JWT token is attached in Axios interceptor.
- Verify token hasn't expired.
- Check JWT key matches between API config and token generation.
