# deployment/SKILL.md — Deploy Safely

## Purpose

Guide agents to verify builds, configuration, and database before deployment.

## When To Use

- Before merging to `main`.
- Before deploying to production.
- When setting up CI/CD.

## Inputs

- `.agent/DEPLOYMENT.md` for deployment guide.
- `.agent/DEVELOPMENT.md` for local setup.

## Pre-Deployment Checklist

### Build
```bash
dotnet build EventSphere.sln --configuration Release
```
- [ ] Build succeeds with zero errors.
- [ ] No warnings related to security or deprecated APIs.

### Tests
```bash
dotnet test
```
- [ ] All tests pass.
- [ ] No skipped tests.

### Configuration
- [ ] No hardcoded secrets in code.
- [ ] `appsettings.json` uses placeholders.
- [ ] Production config uses environment variables.

### Database
```bash
dotnet ef migrations list --project EventSphere.Web
```
- [ ] Migrations are up to date.
- [ ] No destructive migrations without authorization.
- [ ] Seed data reviewed.

### Security
- [ ] `dotnet list package --vulnerable` — no known vulnerabilities.
- [ ] No secrets committed.
- [ ] JWT key is strong and unique.
- [ ] HTTPS enforced.

## Publish
```bash
dotnet publish EventSphere.Web/EventSphere.Web.csproj -c Release -o ./publish
```

## Rollback Plan

1. Keep previous version available.
2. Database migrations must be backward-compatible.
3. Rollback by deploying previous published version.
4. Revert migrations only if safe.

## Rules

- Never deploy without running tests.
- Never deploy with known vulnerabilities.
- Never deploy hardcoded secrets.
- Verify database migrations before applying.
