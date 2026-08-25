# API.md — Web API Conventions

## Base URL

```
/api/{controller}
```

## Authentication

- **JWT Bearer** for all API calls (primary auth).
- Token obtained via `POST /api/auth/login`.
- Include header: `Authorization: Bearer {token}`.
- CORS configured for React dev server (`http://localhost:5173`).

## Endpoints

### Auth — `api/auth`

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/login` | No | Login, returns JWT |
| POST | `/api/auth/register` | No | Register new user |
| POST | `/api/auth/refresh` | Yes | Refresh JWT token |

### Events — `api/events`

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/events` | No | List events (paginated, filterable) |
| GET | `/api/events/{id}` | No | Get single event |
| POST | `/api/events` | Yes (Organizer/Admin) | Create event |
| PUT | `/api/events/{id}` | Yes (Owner/Admin) | Update event |
| DELETE | `/api/events/{id}` | Yes (Owner/Admin) | Delete/cancel event |
| PUT | `/api/events/{id}/approve` | Yes (Admin) | Approve/reject event |

### Registrations — `api/registrations`

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/api/registrations` | Yes | Register for event |
| DELETE | `/api/registrations/{eventId}` | Yes | Cancel registration |
| GET | `/api/registrations/my` | Yes | Get my registrations |
| GET | `/api/registrations/event/{eventId}` | Yes (Organizer) | Get event registrations |

### Attendance — `api/attendance`

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/api/attendance/checkin` | Yes (Organizer) | QR code check-in |
| GET | `/api/attendance/event/{eventId}` | Yes (Organizer) | Get attendance list |

### Feedback — `api/feedback`

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/feedback/event/{eventId}` | No | Get reviews for event |
| POST | `/api/feedback` | Yes | Submit review |

### Certificates — `api/certificates`

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/certificates/my` | Yes | Get my certificates |
| GET | `/api/certificates/{id}/download` | Yes | Download certificate |

### Media — `api/media`

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/media/event/{eventId}` | No | Get event gallery |
| POST | `/api/media` | Yes (Organizer) | Upload media |

### Users — `api/users` (Admin only)

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/users` | Yes (Admin) | List all users |
| PUT | `/api/users/{id}/role` | Yes (Admin) | Assign role |
| DELETE | `/api/users/{id}` | Yes (Admin) | Delete user |

### Notifications — `api/notifications`

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/notifications` | Yes | Get user notifications |
| PUT | `/api/notifications/{id}/read` | Yes | Mark as read |

### Dashboard — `api/dashboard`

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/dashboard/admin` | Yes (Admin) | Admin analytics |
| GET | `/api/dashboard/organizer` | Yes (Organizer) | Organizer stats |

### Venues — `api/venues`

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/api/venues` | No | List venues |
| POST | `/api/venues` | Yes (Admin) | Create venue |
| PUT | `/api/venues/{id}` | Yes (Admin) | Update venue |

## Query Parameters

| Param | Type | Used On | Description |
|---|---|---|---|
| `page` | int | GET /events | Page number (default 1) |
| `pageSize` | int | GET /events | Items per page (default 12) |
| `category` | string | GET /events | Filter by category |
| `search` | string | GET /events | Search in title/description |
| `status` | string | GET /events | Filter by status |

## HTTP Status Codes

| Code | When |
|---|---|
| 200 OK | Successful GET, PUT |
| 201 Created | Successful POST (resource created) |
| 400 Bad Request | Validation error |
| 401 Unauthorized | Missing/invalid JWT |
| 403 Forbidden | Insufficient permissions |
| 404 Not Found | Resource not found |
| 409 Conflict | Duplicate registration |
| 422 Unprocessable | Business rule violation |
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
- CORS must allow React origin.
