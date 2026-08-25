# TESTING.md — Testing Strategy

## Framework

- **Test Framework**: xunit
- **Mocking**: Moq
- **In-Memory DB**: Microsoft.EntityFrameworkCore.InMemory
- **Runner**: `dotnet test`

## Project

```
EventSphere.Tests/
├── Unit/           # Unit tests for services
└── Integration/    # Integration tests
```

## Test Types

### Unit Tests
- Test service methods in isolation.
- Mock `ApplicationDbContext` using InMemory provider.
- File naming: `{ServiceName}Tests.cs`

### Integration Tests
- Use InMemory EF Core provider.
- Test full request pipeline where needed.
- File naming: `{Feature}IntegrationTests.cs`

## Running Tests

```bash
dotnet test
dotnet test --filter "FullyQualifiedName~EventServiceTests"
```

## Team Testing

| Member | Tests Owned |
|---|---|
| Abdullah | Backend service + API tests |
| Jibran | Database + data service tests |
| Ramsha | Frontend core (if applicable) |
| Marukh | Feature UI tests (if applicable) |

## Rules

- Never delete existing tests.
- Never weaken assertions.
- Run tests before committing.
