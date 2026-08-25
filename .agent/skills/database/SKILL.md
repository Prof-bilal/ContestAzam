# database/SKILL.md — EF Core and Schema Changes

## Purpose

Guide agents to modify entities, DbContext, relationships, and run migrations.

## Module Owner

**Jibran (Module 2)** owns database changes.

## Rules

- Use `decimal(18,2)` for monetary values.
- Use `DateTime.UtcNow` for timestamps.
- Configure relationships in `OnModelCreating`.
- Add indexes for frequently queried columns.
- Review generated migrations before committing.
- Jibran reviews all schema changes.

## Verification

```bash
dotnet ef migrations add <Name> --project EventSphere.Web
dotnet ef database update --project EventSphere.Web
dotnet build
```
