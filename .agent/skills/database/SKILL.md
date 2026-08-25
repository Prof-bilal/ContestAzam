# database/SKILL.md — EF Core and Schema Changes

## Purpose

Guide agents to safely modify entities, DbContext, relationships, indexes, and run migrations.

## When To Use

- Adding or modifying an entity.
- Changing relationships or constraints.
- Adding indexes.
- Modifying seed data.
- Creating EF Core migrations.

## Inputs

- Entity files in `Models/Entities/`.
- `ApplicationDbContext.cs`.
- Existing migrations in `Migrations/`.

## Preconditions

- SQL Server is accessible.
- EF Core tools installed.
- Read existing entity patterns before adding new ones.

## Workflow

1. **Read existing entities**: Understand naming, types, relationships.
2. **Modify entity**: Add/change properties in `Models/Entities/`.
3. **Update DbContext**: Add `DbSet`, configure in `OnModelCreating`.
4. **Add migration**: `dotnet ef migrations add <Name> --project EventSphere.Web`.
5. **Review migration**: Read the generated migration file.
6. **Update database**: `dotnet ef database update --project EventSphere.Web`.
7. **Verify**: Build succeeds, application runs.

## Rules

- Use `decimal(18,2)` for monetary values.
- Use `DateTime.UtcNow` for timestamps.
- Configure relationships in `OnModelCreating` (not just data annotations).
- Add indexes for frequently queried columns.
- Unique composite indexes for: (UserId, EventId) on registrations and reviews.
- Unique index on `Ticket.TicketCode` and `EventCategory.Name`.
- Never delete migration files without creating a replacement.
- Review auto-generated migrations before committing.
- Seed data in `SeedData.cs`.

## Verification

```bash
dotnet build
dotnet ef migrations add <Name> --project EventSphere.Web
dotnet ef database update --project EventSphere.Web
dotnet run --project EventSphere.Web  # verify app starts
```

## Failure Handling

- Migration conflicts → remove `bin/obj`, re-run `dotnet restore`.
- Schema error → verify `OnModelCreating` configuration.
- SQL error → verify SQL Server is running.
