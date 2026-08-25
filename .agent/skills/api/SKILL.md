# api/SKILL.md — Create/Modify API Endpoints

## Purpose

Guide agents to create or modify Web API endpoints consumed by the React frontend.

## When To Use

- Adding a new API endpoint.
- Modifying existing API behavior.
- Adding DTOs for API responses.
- Changing API authentication or authorization.

## Inputs

- API controllers in `EventSphere.Api/Controllers/`.
- DTOs in `EventSphere.Api/DTOs/`.
- `.agent/API.md` for conventions.

## Preconditions

- Understand existing API patterns.
- Read existing controllers for patterns.
- React frontend consumes these endpoints.

## Workflow

1. **Read existing controller**: Understand route, auth, response patterns.
2. **Define DTO**: Add request/response DTO in `DTOs/`.
3. **Add controller method**: Follow `[HttpGet]`/`[HttpPost]` conventions.
4. **Add authorization**: `[Authorize]` if needed.
5. **Add validation**: Model validation + `[ApiController]`.
6. **Return correct status**: `Ok()`, `Created()`, `NotFound()`, `BadRequest()`.
7. **Verify**: Build, test endpoint with curl or Postman.
8. **Update frontend**: Add corresponding service function in `EventSphere.React/src/services/`.

## Rules

- Use `[ApiController]` + `[Route("api/[controller]")]`.
- Use DTOs for request/response — never expose raw entities.
- Use `[Authorize]` on protected endpoints.
- Return `CreatedAtAction` for resource creation.
- Validate all input with model validation.
- Return appropriate HTTP status codes.
- CORS configured for React origin.
- Keep API stateless (JWT auth).

## HTTP Status Codes

| Code | Use |
|---|---|
| 200 | Successful GET, PUT |
| 201 | Successful POST (created) |
| 400 | Validation error |
| 401 | Not authenticated |
| 403 | Not authorized |
| 404 | Not found |
| 409 | Conflict (duplicate) |
| 500 | Server error |

## Verification

```bash
dotnet build EventSphere.Api
dotnet test
# Test with curl:
curl -X GET http://localhost:5001/api/events
curl -X POST http://localhost:5001/api/auth/login -H "Content-Type: application/json" -d '{"email":"...","password":"..."}'
```

## Failure Handling

- 401 → check JWT token, signing key, issuer/audience.
- 400 → check model validation, required fields.
- 404 → verify route pattern, HTTP method.
- CORS error → check `Program.cs` CORS policy.
