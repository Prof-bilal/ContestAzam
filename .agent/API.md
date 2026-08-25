# API.md — Web API Conventions

## Base URL

```
/api/{controller}
```

## Authentication

- **JWT Bearer** for API endpoints.
- Token obtained via `POST /api/auth/login`.
- Include header: `Authorization: Bearer {token}`.
- Cookie authentication used by MVC (browser sessions).

## Endpoints

### Auth — `api/auth`

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/login` | No | Login, returns JWT |
| POST | `/api/auth/register` | No | Register new user |

**Request/Response:**
```
POST /api/auth/login
Body: { "email": "...", "password": "..." }
Response: { "success": true, "token": "...", "userId": "..." }
```

### Events — `api/events`

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/events` | No | List events (paginated, filterable) |
| GET | `/api/events/{id}` | No | Get single event |
| POST | `/api/events` | Yes | Create event |
| POST | `/api/events/{eventId}/register` | Yes | Register for event |
| DELETE | `/api/events/{eventId}/register` | Yes | Cancel registration |

**Query Parameters:**
- `page` (int, default 1)
- `categoryId` (int, optional)
- `search` (string, optional)

### Reviews — `api/reviews`

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/reviews/event/{eventId}` | No | Get reviews for event |
| POST | `/api/reviews/event/{eventId}` | Yes | Add review |

### Notifications — `api/notifications`

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/notifications` | Yes | Get user notifications |
| GET | `/api/notifications/unread-count` | Yes | Get unread count |
| PUT | `/api/notifications/{id}/read` | Yes | Mark as read |

## DTOs

Defined in `Controllers/Api/Dtos/ApiDtos.cs`:

- `EventDto` — event response
- `CreateEventRequest` — create event input
- `LoginRequest` / `RegisterRequest` — auth input
- `AuthResponse` — auth response
- `ReviewRequest` — review input
- `NotificationDto` — notification response

## HTTP Status Codes

| Code | When |
|---|---|
| 200 OK | Successful GET, PUT |
| 201 Created | Successful POST (resource created) |
| 400 Bad Request | Validation error |
| 401 Unauthorized | Missing/invalid auth |
| 403 Forbidden | Insufficient permissions |
| 404 Not Found | Resource not found |
| 500 Internal Server Error | Unexpected failure |

## Error Format

```json
{
  "message": "Description of error",
  "errors": { "field": ["error message"] }
}
```

## Rules

- Use `[ApiController]` + `[Route("api/[controller]")]`.
- Use `[Authorize]` on protected endpoints.
- Return `CreatedAtAction` for resource creation.
- Validate all input with model validation.
- Never expose internal stack traces in production.
