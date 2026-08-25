# DEPLOYMENT.md — Deployment Guide

## Build

```bash
dotnet build EventSphere.sln --configuration Release
```

## Publish

```bash
dotnet publish EventSphere.Web/EventSphere.Web.csproj -c Release -o ./publish
```

## Configuration

### Production appsettings.json
- Connection string: environment variable or secrets manager.
- JWT key: strong, unique, from environment variable.
- Logging: structured provider (Serilog recommended).

### Environment Variables
```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=...;Database=EventSphereDb;...
Jwt__Key=your-production-secret-key
```

## SQL Server

- Create production database.
- Run EF Core migrations on deployment.
- Seed admin account.

```bash
dotnet ef database update --project EventSphere.Web
```

## Health Checks

Not currently implemented. Recommended:
- `/health` endpoint.
- Database connectivity check.
- SQL Server dependency.

## Logging

- Use Serilog or Application Insights in production.
- Never log sensitive data.
- Configure log levels via configuration.

## Monitoring

- Application performance monitoring (APM).
- Error tracking (Sentry, Application Insights).
- Database performance monitoring.

## Rollback

1. Keep previous version published.
2. Database migrations must be backward-compatible.
3. Rollback by deploying previous version.
4. Revert migrations only if safe.

## Current vs Recommended

| Aspect | Current | Recommended |
|---|---|---|
| Build | Manual `dotnet publish` | CI/CD pipeline |
| Database | Manual migration | Automated migration on deploy |
| Secrets | appsettings.json | Azure Key Vault / Environment |
| Logging | Console | Serilog + central sink |
| Monitoring | None | Application Insights |
| Health Checks | None | `/health` endpoint |
| HTTPS | Developer certs | Production certificate |
