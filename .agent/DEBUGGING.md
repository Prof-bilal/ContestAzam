# DEBUGGING.md — Debugging Playbook

## Workflow

```
Reproduce → Gather Evidence → Identify Boundary → Narrow Hypothesis
→ Test Hypothesis → Fix Root Cause → Regression Test → Verify
```

## Common Issues

### Application Won't Start
- Check `Program.cs` for DI errors.
- Verify NuGet packages restored.
- Check connection string.

### MVC Routing Problems
- 404 on known routes → check route pattern in `Program.cs`.
- Wrong action → verify `asp-controller` and `asp-action` in Razor.

### Razor View Errors
- `View not found` → check Views folder structure matches controller name.
- Verify `_ViewImports.cshtml` has correct `@addTagHelper`.

### API Errors
- 401 Unauthorized → check JWT token, signing key.
- 400 Bad Request → check model validation.

### EF Core Errors
- `DbUpdateException` → check foreign key constraints.
- `InvalidOperationException` → check for missing `Include()`.

### Authentication Problems
- Cookie not set → check `SignInManager` configuration.
- JWT invalid → verify key, issuer, audience match `Program.cs`.

## Rules

- Reproduce before fixing.
- One change at a time.
- Verify fix doesn't break other features.
- Add regression test.
