# database/SKILL.md — EF Core and Migrations

## Module Owner

**Jibran (Module 2)** owns database changes.

## Rules

- `decimal(18,2)` for money.
- `DateTime.UtcNow` for timestamps.
- Configure in `OnModelCreating`.
- Review migrations before committing.
- Jibran reviews all schema changes.

## Commands

```bash
dotnet ef migrations add <Name> --project EventSphere.Api
dotnet ef database update --project EventSphere.Api
```
