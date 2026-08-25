# testing/SKILL.md — Write and Run Tests

## Purpose

Guide agents to add, modify, and run tests.

## Team Testing

| Member | Tests |
|---|---|
| Abdullah | Backend service + API tests |
| Jibran | Database + data service tests |
| Ramsha | Frontend core (if applicable) |
| Marukh | Feature UI tests (if applicable) |

## Rules

- Never delete existing tests.
- Never weaken assertions.
- Use `Arrange / Act / Assert`.
- Run `dotnet test` before committing.
