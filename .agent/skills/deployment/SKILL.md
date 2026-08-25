# deployment/SKILL.md — Deploy Safely

## Backend

```bash
dotnet publish EventSphere.Api -c Release -o ./publish-api
```

## Frontend

```bash
cd EventSphere.React && npm run build
# Output in dist/
```

## Pre-Deploy

```bash
dotnet test
cd EventSphere.React && npm test
dotnet list package --vulnerable
cd EventSphere.React && npm audit
```
