# api/SKILL.md — Create/Modify API Endpoints

## Purpose

Guide agents to create or modify Web API endpoints.

## Module Ownership

- **Abdullah (Module 1)**: API architecture, auth endpoints
- **Jibran (Module 2)**: Data-heavy endpoints (events, registrations)

## Rules

- Use `[ApiController]` + `[Route("api/[controller]")]`.
- Use DTOs for request/response.
- Use `[Authorize]` on protected endpoints.
- Return `CreatedAtAction` for resource creation.
- Validate all input.
- Return appropriate HTTP status codes.

## API Contract Rule

Frontend developers should NOT modify API behavior. If frontend needs a change:

```
Frontend requirement → API contract discussion → Backend implementation → Frontend integration
```
