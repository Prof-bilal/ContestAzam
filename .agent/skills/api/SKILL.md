# api/SKILL.md — Create/Modify API Endpoints

## Module Ownership

- **Abdullah (Module 1)**: API architecture, auth endpoints
- **Jibran (Module 2)**: Data-heavy endpoints

## Rules

- `[ApiController]` + `[Route("api/[controller]")]`.
- DTOs for request/response.
- `[Authorize]` on protected endpoints.
- Return `CreatedAtAction` for creation.
- Validate all input.

## API Contract Rule

```
Frontend requirement → Contract discussion → Backend impl → Frontend integration
```
