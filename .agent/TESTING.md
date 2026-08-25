# TESTING.md — Testing Strategy

## Backend (C#)
- **Framework**: xunit
- **Mocking**: Moq
- **In-Memory DB**: Microsoft.EntityFrameworkCore.InMemory
- **Run**: `dotnet test`

## Frontend (React/TypeScript)
- **Framework**: Vitest
- **Component Testing**: @testing-library/react
- **Run**: `cd EventSphere.React && npm test`

## Team Testing

| Member | Tests Owned |
|---|---|
| Abdullah | Backend service + API tests |
| Jibran | Database + data service tests |
| Ramsha | Frontend component tests |
| Marukh | Feature page tests |

## Rules

- Never delete existing tests.
- Run `dotnet test` AND `npm test` before committing.
