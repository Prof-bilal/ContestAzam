# testing/SKILL.md — Write and Run Tests

## Purpose

Guide agents to correctly add, modify, and run tests in EventSphere.

## When To Use

- Adding new service method → add unit test.
- Fixing a bug → add regression test.
- Adding API endpoint → add integration test.
- Modifying business logic → verify existing tests still pass.

## Inputs

- The service, controller, or method being changed.
- The test project: `EventSphere.Tests/`.

## Preconditions

- Test project exists: `EventSphere.Tests/EventSphere.Tests.csproj`.
- Framework: xunit + Moq + InMemory EF Core.
- Run `dotnet test` from solution root.

## Workflow

1. **Identify affected code**: Read the service/controller being modified.
2. **Find existing tests**: Check `EventSphere.Tests/Unit/` and `EventSphere.Tests/Integration/`.
3. **Preserve test structure**: Match existing naming, fixture, and pattern conventions.
4. **Write tests**:
   - Unit test for business logic (mock dependencies).
   - Integration test for database operations (InMemory provider).
   - At least one happy path and one failure/edge case.
5. **Run narrow test suite first**: `dotnet test --filter "FullyQualifiedName~{TestClassName}"`.
6. **Run full suite**: `dotnet test`.
7. **Verify all pass**.

## Rules

- Never delete existing tests.
- Never weaken assertions.
- Use `Arrange / Act / Assert`.
- Method naming: `MethodName_Scenario_ExpectedResult`.
- Use `[Fact]` for single tests, `[Theory]` for parameterized.
- One assertion per test (preferred).
- Tests must be independent (no shared state).
- Use InMemory provider for DB tests, not real SQL Server.

## Verification

```bash
dotnet test --verbosity normal
```

All tests must pass. No skipped tests.

## Failure Handling

- If new test fails → fix the test or the implementation.
- If existing test fails → investigate whether the change broke behavior.
- Never disable tests to pass CI.
