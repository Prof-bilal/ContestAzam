# TESTING.md — Testing Strategy

## Framework

- **Test Framework**: xunit
- **Mocking**: Moq
- **In-Memory DB**: Microsoft.EntityFrameworkCore.InMemory
- **Test Runner**: `dotnet test`

## Project

```
EventSphere.Tests/
├── EventSphere.Tests.csproj
├── Unit/           # Unit tests for services
└── Integration/    # Integration tests (in-memory DB)
```

## Test Types

### Unit Tests
- Test service methods in isolation.
- Mock `ApplicationDbContext` using Moq or InMemory provider.
- Test business logic, validation, edge cases.
- File naming: `{ServiceName}Tests.cs`

### Integration Tests
- Use InMemory EF Core provider.
- Test full request pipeline where needed.
- Verify database interactions.
- File naming: `{Feature}IntegrationTests.cs`

## Running Tests

```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~EventServiceTests"

# Run with verbosity
dotnet test --verbosity normal
```

## Test Conventions

- One test class per service or controller.
- Test method names: `MethodName_Scenario_ExpectedResult`
- Use `[Fact]` for single tests, `[Theory]` for parameterized.
- Arrange / Act / Assert pattern.
- Each test should be independent (no shared state).

## When to Add Tests

| Change | Test Required |
|---|---|
| New service method | Unit test |
| Modified business logic | Unit test for new/changed paths |
| New API endpoint | Integration test |
| Bug fix | Regression test |
| Schema change | Migration test |

## Coverage

- Aim for service layer coverage > 80%.
- Critical paths (auth, registration, payments) must have tests.
- No coverage tool currently configured (P2 gap).

## Rules

- Never delete existing tests to make code pass.
- Never weaken assertions to fix a failing test.
- Run tests before committing.
- Fix test failures, don't skip them.
