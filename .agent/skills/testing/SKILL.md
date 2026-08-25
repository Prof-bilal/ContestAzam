# testing/SKILL.md — Write and Run Tests

## Purpose

Guide agents to correctly add, modify, and run tests for both backend (C#) and frontend (React).

## When To Use

- Adding new API endpoint → backend test.
- Adding new React component → frontend test.
- Fixing a bug → regression test.
- Modifying business logic → verify existing tests.

## Inputs

- Backend: `EventSphere.Tests/`
- Frontend: `EventSphere.React/src/**/*.test.tsx`

## Preconditions

- Test projects exist.
- Backend: xunit + Moq + InMemory EF Core.
- Frontend: Vitest + @testing-library/react.

## Workflow

### Backend Tests
1. Find existing test class in `EventSphere.Tests/Unit/` or `Integration/`.
2. Preserve test structure and naming conventions.
3. Write tests: `Arrange / Act / Assert`.
4. Run: `dotnet test`.

### Frontend Tests
1. Find existing test in `EventSphere.React/src/**/*.test.tsx`.
2. Use `describe/it` blocks.
3. Mock API calls with `vi.fn()` or MSW.
4. Run: `cd EventSphere.React && npm test`.

## Rules

- Never delete existing tests.
- Never weaken assertions.
- One assertion per test (preferred).
- Tests must be independent.
- Run both backend and frontend tests before committing.

## Verification

```bash
dotnet test                           # Backend
cd EventSphere.React && npm test      # Frontend
```

## Failure Handling

- New test fails → fix test or implementation.
- Existing test fails → investigate regression.
- Never disable tests to pass CI.
