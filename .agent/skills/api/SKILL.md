# api/SKILL.md — Create/Modify API Endpoints

## Purpose

Guide agents to create or modify Web API endpoints with correct patterns.

## When To Use

- Adding a new API endpoint.
- Modifying existing API behavior.
- Adding DTOs for API responses.
- Changing API authentication or authorization.

## Inputs

- API controllers in `Controllers/Api/`.
- DTOs in `Controllers/Api/Dtos/`.
- `.agent/API.md` for conventions.

## Preconditions

- Understand existing API patterns.
- Read `ApiDtos.cs` for existing DTOs.
- Read existing API controllers for patterns.

## Workflow

1. **Read existing controller**: Understand route, auth, response patterns.
2. **Define DTO**: Add request/response DTO in `ApiDtos.cs`.
3. **Add controller method**: Follow `[HttpGet]`/`[HttpPost]` conventions.
4. **Add authorization**: `[Authorize]` if needed.
5. **Add validation**: Model validation + `[ApiController]` attribute.
6. **Return correct status**: `Ok()`, `Created()`, `NotFound()`, `BadRequest()`.
7. **Verify**: Build, test endpoint with curl or API client.

## Rules

- Use `[ApiController]` + `[Route("api/[controller]")]`.
- Use DTOs for request/response — never expose raw entities.
- Use `[Authorize]` on protected endpoints.
- Return `CreatedAtAction` for resource creation.
- Validate all input with model validation attributes.
- Return appropriate HTTP status codes.
- Never expose internal stack traces.
- Keep API stateless (no session in API controllers).

## HTTP Status Codes

| Code | Use |
|---|---|
| 200 | Successful GET, PUT |
| 201 | Successful POST (created) |
| 400 | Validation error |
| 401 | Not authenticated |
| 403 | Not authorized |
| 404 | Not found |
| 500 | Server error |

## Verification

```bash
dotnet build
# Test with curl:
curl -X GET https://localhost:5001/api/events
curl -X POST https://localhost:5001/api/auth/login -H "Content-Type: application/json" -d '{"email":"...","password":"..."}'
```

## Failure Handling

- 401 → check JWT token, signing key, issuer/audience.
- 400 → check model validation, required fields.
- 404 → verify route pattern, HTTP method.
