# deployment/SKILL.md — Deploy Safely

## Pre-Deployment

```bash
dotnet build EventSphere.sln --configuration Release
dotnet test
dotnet list package --vulnerable
```

## Publish

```bash
dotnet publish EventSphere.Web -c Release -o ./publish
```

## Database

```bash
dotnet ef database update --project EventSphere.Web
```

## Rules

- Never deploy without running tests.
- Never deploy hardcoded secrets.
- Verify migrations before applying.
