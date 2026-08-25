# deployment/SKILL.md — Deploy Safely

## Purpose

Guide agents to verify builds, config, and database before deployment.

## When To Use

- Before merging to `main`.
- Before deploying to production.

## Pre-Deployment Checklist

### Backend Build
```bash
dotnet build EventSphere.Api -c Release
dotnet test
```
- [ ] Build succeeds.
- [ ] All tests pass.

### Frontend Build
```bash
cd EventSphere.React
npm run build
npm test
```
- [ ] Build succeeds (no TypeScript errors).
- [ ] All tests pass.

### Configuration
- [ ] No hardcoded secrets.
- [ ] CORS configured for production domain.
- [ ] JWT key is strong and unique.
- [ ] Connection string uses environment variable.

### Database
```bash
dotnet ef migrations list --project EventSphere.Api
```
- [ ] Migrations up to date.
- [ ] No destructive migrations.

### Security
- [ ] `dotnet list package --vulnerable` — clean.
- [ ] `npm audit` — clean.
- [ ] HTTPS enforced.

## Publish

```bash
# Backend
dotnet publish EventSphere.Api -c Release -o ./publish-api

# Frontend
cd EventSphere.React && npm run build
# Output in dist/ — deploy to CDN or static hosting
```

## Deployment Targets

| Component | Deployment |
|---|---|
| API | Azure App Service, IIS, Docker |
| React | Vercel, Netlify, Azure Static Web Apps, CDN |
| Database | Azure SQL, AWS RDS, on-premise SQL Server |

## Rollback

1. Keep previous version available.
2. Database migrations must be backward-compatible.
3. Rollback by deploying previous version.

## Rules

- Never deploy without running tests.
- Never deploy with known vulnerabilities.
- Never deploy hardcoded secrets.
