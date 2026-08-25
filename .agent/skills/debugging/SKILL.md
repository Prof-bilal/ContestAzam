# debugging/SKILL.md — Debug ASP.NET Core Applications

## Purpose

Guide agents to systematically diagnose and fix issues in EventSphere.

## When To Use

- Application errors or unexpected behavior.
- Build failures.
- Runtime exceptions.
- Test failures.

## Workflow

```
Reproduce → Gather Evidence → Identify Boundary → Narrow Hypothesis
→ Test Hypothesis → Fix Root Cause → Regression Test → Verify
```

## Steps

### 1. Reproduce
- Get exact steps to reproduce.
- Get error message, stack trace, logs.

### 2. Gather Evidence
- Read error message carefully.
- Check logs (console output).
- Check `appsettings.json` configuration.
- Verify database connection.

### 3. Identify Boundary
- Which layer? (Controller, Service, Data, View)
- Which feature? (Auth, Events, Tickets, etc.)
- Is it a build error or runtime error?

### 4. Narrow Hypothesis
- Based on error type, form 2-3 hypotheses.
- Start with most likely.

### 5. Test Hypothesis
- Add logging: `_logger.LogInformation("Debug: {Var}", var)`.
- Check variable values.
- Verify assumptions against code.

### 6. Fix Root Cause
- Make minimal change to fix.
- Don't fix symptoms — fix the cause.

### 7. Regression Test
- Run existing tests: `dotnet test`.
- Add new test for the bug if missing.

### 8. Verify
- Build succeeds.
- All tests pass.
- Issue no longer reproduces.

## Common Error Patterns

| Error | Likely Cause |
|---|---|
| `InvalidOperationException: No service` | Missing DI registration in `Program.cs` |
| `NullReferenceException` | Missing null check or null navigation property |
| `DbUpdateException` | Foreign key violation, missing required field |
| `401 Unauthorized` | JWT token expired, wrong key, missing auth header |
| `View not found` | Wrong view name, missing `_ViewImports` |
| `Model validation failed` | Missing `[Required]`, wrong field names |

## Rules

- Reproduce before fixing.
- One change at a time.
- Verify fix doesn't break other features.
- Add regression test.
- Remove debug logging before commit.
