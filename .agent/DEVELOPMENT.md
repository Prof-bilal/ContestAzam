# DEVELOPMENT.md — Local Development Setup

## Prerequisites

- .NET 8 SDK
- Node.js 18+ and npm
- SQL Server (LocalDB or full instance)
- Git

## Backend Setup

```bash
dotnet restore
dotnet ef database update --project EventSphere.Api
dotnet run --project EventSphere.Api
# API at https://localhost:5001
```

## Frontend Setup

```bash
cd EventSphere.React
npm install
echo "VITE_API_URL=http://localhost:5001" > .env
npm run dev
# React at http://localhost:5173
```

## Default Accounts

| Role | Email | Password |
|---|---|---|
| Admin | admin@eventsphere.com | Admin@123 |
| Organizer | organizer@eventsphere.com | Organizer@123 |
| Participant | participant@eventsphere.com | Participant@123 |

## Common Commands

```bash
dotnet build EventSphere.sln
dotnet test
cd EventSphere.React && npm run build && npm test
```
