# DEBUGGING.md — Debugging Playbook

## Workflow

```
Reproduce → Gather Evidence → Identify Boundary → Narrow Hypothesis
→ Test Hypothesis → Fix Root Cause → Regression Test → Verify
```

## Backend Debugging (ASP.NET Core)

### Application Won't Start
- Check `Program.cs` for DI errors.
- Verify NuGet packages restored.
- Check connection string validity.
- Look for missing middleware.

### API Returns 401/403
- Check JWT token validity (expiry, signing key).
- Verify `[Authorize]` attribute placement.
- Check CORS configuration.
- Verify token is sent in `Authorization` header.

### EF Core Errors
- `DbUpdateException` → foreign key constraint.
- `InvalidOperationException` → missing `Include()`.
- Migration errors → pending model changes.
- `SqlException` → SQL Server not running.

### DI Failures
- `Unable to resolve service` → missing registration in `Program.cs`.

## Frontend Debugging (React)

### Blank Page / White Screen
- Open browser DevTools console.
- Check for JavaScript errors.
- Verify API URL in `.env`.
- Check React Router configuration.

### API Calls Failing
- Open Network tab in DevTools.
- Check request URL, headers, payload.
- Verify `Authorization: Bearer {token}` header.
- Check CORS errors in console.

### State Not Updating
- Use React DevTools extension.
- Check Context providers wrapping components.
- Verify state update is not mutated directly.

### Routing Issues
- Check `React Router` route definitions in `App.tsx`.
- Verify `BrowserRouter` wraps the app.
- Check for typos in route paths.

### Build Errors
```bash
cd EventSphere.React
npm run build
# Read error messages carefully
# Check TypeScript errors: npx tsc --noEmit
```

## Cross-Layer Debugging

### Auth Flow Broken
1. Test API directly: `curl -X POST http://localhost:5001/api/auth/login -d '{...}'`
2. Verify JWT token returned.
3. Test authenticated endpoint: `curl -H "Authorization: Bearer {token}" http://localhost:5001/api/events`
4. Check React Axios interceptor is attaching token.

### Data Not Appearing in UI
1. Test API endpoint directly (Postman/curl).
2. Check browser Network tab for response.
3. Verify React component is fetching data.
4. Check component rendering logic.

## Logging

```csharp
// Backend
_logger.LogInformation("Processing event {EventId}", eventId);
_logger.LogError(ex, "Failed to process registration");
```

```typescript
// Frontend
console.log('API Response:', data);
console.error('API Error:', error);
```

## Rules

- Reproduce before fixing.
- One change at a time.
- Verify fix doesn't break other features.
- Add regression test.
- Remove debug logging before commit.
