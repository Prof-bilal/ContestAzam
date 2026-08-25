# DEPLOYMENT.md — Deployment Guide

## Build

### Backend
```bash
dotnet publish EventSphere.Api/EventSphere.Api.csproj -c Release -o ./publish-api
```

### Frontend
```bash
cd EventSphere.React
npm run build
# Output in dist/
```

## Configuration

### Backend (API)
- Connection string: environment variable.
- JWT key: environment variable (strong, unique).
- CORS: configure for production domain.

```env
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=...;Database=EventSphereDb;...
Jwt__Key=your-production-secret-key
Cors__AllowedOrigins=https://yourdomain.com
```

### Frontend (React)
```env
VITE_API_URL=https://api.yourdomain.com
VITE_SIGNALR_URL=https://api.yourdomain.com/hubs/notifications
```

## Deployment Targets

| Component | Recommended | Alternatives |
|---|---|---|
| API | Azure App Service | IIS, Docker, AWS ECS |
| React | Vercel / Netlify | Azure Static Web Apps, S3+CloudFront |
| Database | Azure SQL | AWS RDS, on-premise SQL Server |

## Database

```bash
dotnet ef database update --project EventSphere.Api
```

## Health Checks

Not currently implemented. Recommended:
- `/health` endpoint on API.
- Database connectivity check.

## Rollback

1. Keep previous version available.
2. Database migrations must be backward-compatible.
3. Rollback by deploying previous version.

## Current vs Recommended

| Aspect | Current | Recommended |
|---|---|---|
| API Deploy | Manual publish | CI/CD pipeline |
| Frontend Deploy | Manual build | Vercel/Netlify auto-deploy |
| Database | Manual migration | Automated on deploy |
| Secrets | appsettings.json | Azure Key Vault / Env vars |
| Monitoring | None | Application Insights |
