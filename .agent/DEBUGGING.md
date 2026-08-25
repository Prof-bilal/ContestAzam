# DEBUGGING.md — ASP.NET Core Debugging Playbook

## Workflow

```
Reproduce → Gather Evidence → Identify Boundary → Narrow Hypothesis
→ Test Hypothesis → Fix Root Cause → Regression Test → Verify
```

## Common Issues

### Application Won't Start

1. Check `Program.cs` for DI registration errors.
2. Verify all NuGet packages are restored.
3. Check connection string is valid.
4. Look for missing service registrations.
5. Check for duplicate middleware.

### DI Failures

- `InvalidOperationException: Unable to resolve service`
- Check `Program.cs` for missing `AddScoped`/`AddSingleton`/`AddTransient`.
- Verify interface and implementation are both registered.

### MVC Routing Problems

- 404 on known routes → check route pattern in `Program.cs`.
- Wrong action called → verify `asp-controller` and `asp-action` in Razor.
- Check for `[Route]` attribute conflicts.

### Razor View Errors

- `InvalidOperationException: The view 'X' was not found`
- Check Views folder structure matches controller name.
- Verify `_ViewImports.cshtml` has correct `@addTagHelper`.
- Check for compilation errors in `.cshtml` files.

### Model Binding Failures

- Properties are null/zero → check property names match form field names.
- Complex objects not binding → check for `[FromBody]` vs `[FromForm]`.
- Collection binding → ensure correct naming convention.

### API Errors

- 401 Unauthorized → check JWT token validity, expiry, signing key.
- 400 Bad Request → check model validation, required fields.
- 404 Not Found → verify route pattern and HTTP method.

### EF Core Errors

- `DbUpdateException` → check foreign key constraints.
- `InvalidOperationException` → check for missing `Include()`.
- Migration errors → check for pending model changes.
- `SqlException` → verify SQL Server is running and accessible.

### Authentication Problems

- Cookie not set → check `SignInManager` configuration.
- JWT invalid → verify key, issuer, audience match `Program.cs`.
- Identity errors → check password policy, user lockout settings.

### Build Failures

- Missing references → `dotnet restore`.
- Version conflicts → check `.csproj` package versions.
- Compilation errors → check C# syntax, nullable reference types.

## Debugging Tools

```bash
# Check package vulnerabilities
dotnet list package --vulnerable

# Verify build
dotnet build --verbosity detailed

# Check migrations
dotnet ef migrations list --project EventSphere.Web

# View SQL (in Development)
# Enable EF Core logging in appsettings.Development.json
```

## Logging

Add temporary logging for debugging:
```csharp
_logger.LogInformation("Debug: {Variable}", variableValue);
```

Check logs in console output or configured logging provider.

## Rules

- Reproduce the issue before fixing.
- Make one change at a time.
- Verify fix doesn't break other functionality.
- Add regression test for the fix.
- Remove debug logging before committing.
