# TESTING.md — Testing Strategy

## Frameworks

### Backend (C#)
- **Test Framework**: xunit
- **Mocking**: Moq
- **In-Memory DB**: Microsoft.EntityFrameworkCore.InMemory
- **Runner**: `dotnet test`

### Frontend (React/TypeScript)
- **Test Framework**: Vitest (or Jest)
- **Component Testing**: @testing-library/react
- **E2E Testing**: Playwright or Cypress (P2 gap)
- **Runner**: `npm test`

## Test Types

### Backend Unit Tests
- Test service methods in isolation.
- Mock `ApplicationDbContext` using InMemory provider.
- File naming: `{ServiceName}Tests.cs`

### Backend Integration Tests
- Use InMemory EF Core provider.
- Test full API pipeline where needed.
- File naming: `{Feature}IntegrationTests.cs`

### Frontend Component Tests
- Test React components in isolation.
- Mock API calls with MSW (Mock Service Worker) or vi.fn().
- Test user interactions, rendering, error states.
- File naming: `{ComponentName}.test.tsx`

### Frontend Hook Tests
- Test custom hooks with `@testing-library/react-hooks`.
- File naming: `{hookName}.test.ts`

## Running Tests

```bash
# Backend
dotnet test
dotnet test --filter "FullyQualifiedName~EventServiceTests"

# Frontend
cd EventSphere.React
npm test
npm run test:coverage
```

## Test Conventions

- One test class/file per service or component.
- Test method names: `MethodName_Scenario_ExpectedResult`.
- Use `[Fact]` for single tests, `[Theory]` for parameterized (xunit).
- Use `describe/it` blocks (Vitest/Jest).
- Arrange / Act / Assert pattern.
- Each test should be independent.

## When to Add Tests

| Change | Test Required |
|---|---|
| New API endpoint | Backend integration test |
| New service method | Backend unit test |
| Modified business logic | Backend unit test |
| New React component | Frontend component test |
| Bug fix | Regression test (both layers) |
| Auth change | Integration test |

## Coverage

- Backend: Aim for service layer > 80%.
- Frontend: Aim for component coverage > 70%.
- Critical paths (auth, registration) must have tests.

## Rules

- Never delete existing tests.
- Never weaken assertions.
- Run tests before committing (both backend and frontend).
- Fix test failures, don't skip them.
