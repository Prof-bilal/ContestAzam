# FRONTEND.md — React SPA

## Overview

EventSphere's frontend is a **React 18+ Single Page Application** built with Vite. It communicates with the ASP.NET Core Web API over HTTP/JSON.

> There are NO ASP.NET Core MVC Controllers or Razor Views in the frontend.

## Tech Stack

| Tool | Purpose |
|---|---|
| React 18+ | UI library |
| TypeScript | Type safety |
| Vite | Build tool / dev server |
| React Router v6 | Client-side routing |
| Axios | HTTP client |
| React Bootstrap / Bootstrap 5 | UI components/styling |
| Context API | State management (auth, notifications) |
| @microsoft/signalr | Real-time WebSocket client |

## Project Structure

```
EventSphere.React/
├── public/
│   └── index.html
├── src/
│   ├── api/
│   │   └── axios.ts              # Axios instance with baseURL, interceptors
│   ├── components/
│   │   ├── common/               # Reusable UI (Navbar, Footer, Loader, ProtectedRoute)
│   │   ├── events/               # EventCard, EventForm, EventFilters
│   │   ├── gallery/              # GalleryGrid, MediaUpload
│   │   ├── dashboard/            # StatsCard, Charts
│   │   └── reviews/              # ReviewForm, ReviewList, StarRating
│   ├── pages/
│   │   ├── Home.tsx
│   │   ├── Events.tsx
│   │   ├── EventDetail.tsx
│   │   ├── Login.tsx
│   │   ├── Register.tsx
│   │   ├── Dashboard.tsx
│   │   ├── MyRegistrations.tsx
│   │   ├── Gallery.tsx
│   │   ├── AdminPanel.tsx
│   │   └── NotFound.tsx
│   ├── context/
│   │   ├── AuthContext.tsx       # JWT token, user state
│   │   └── NotificationContext.tsx
│   ├── hooks/
│   │   ├── useEvents.ts
│   │   ├── useAuth.ts
│   │   └── useNotifications.ts
│   ├── services/
│   │   ├── authService.ts
│   │   ├── eventService.ts
│   │   ├── registrationService.ts
│   │   ├── feedbackService.ts
│   │   └── certificateService.ts
│   ├── types/
│   │   └── index.ts              # TypeScript interfaces
│   ├── utils/
│   │   ├── dates.ts
│   │   └── validators.ts
│   ├── App.tsx                    # Route definitions
│   ├── main.tsx                   # Entry point
│   └── vite-env.d.ts
├── index.html
├── package.json
├── vite.config.ts
├── tsconfig.json
└── tsconfig.node.json
```

## Routing

```tsx
// App.tsx
<Routes>
  <Route path="/" element={<Home />} />
  <Route path="/events" element={<Events />} />
  <Route path="/events/:id" element={<EventDetail />} />
  <Route path="/login" element={<Login />} />
  <Route path="/register" element={<Register />} />
  <Route path="/dashboard" element={<ProtectedRoute><Dashboard /></ProtectedRoute>} />
  <Route path="/my-registrations" element={<ProtectedRoute><MyRegistrations /></ProtectedRoute>} />
  <Route path="/gallery" element={<Gallery />} />
  <Route path="/admin" element={<ProtectedRoute role="Admin"><AdminPanel /></ProtectedRoute>} />
  <Route path="*" element={<NotFound />} />
</Routes>
```

## API Communication

```ts
// services/eventService.ts
import axios from '../api/axios';

export const getEvents = async (page = 1, category?: string) => {
  const { data } = await axios.get('/api/events', { params: { page, category } });
  return data;
};

export const getEvent = async (id: number) => {
  const { data } = await axios.get(`/api/events/${id}`);
  return data;
};
```

## Auth Flow

1. User logs in → POST `/api/auth/login` → receives JWT + refresh token.
2. JWT stored in `localStorage` or `httpOnly` cookie.
3. Axios interceptor attaches `Authorization: Bearer {token}` to all requests.
4. On 401 → redirect to `/login`.
5. AuthContext provides `user`, `token`, `login()`, `logout()` to all components.

## State Management

- **Auth state**: `AuthContext` (React Context)
- **Notifications**: `NotificationContext`
- **Server state**: React Query or direct Axios calls with local state
- **Form state**: React Hook Form or controlled components

## Component Rules

- Functional components only (no class components).
- One component per file.
- Props typed with TypeScript interfaces.
- Keep components under 200 lines.
- Extract reusable UI into `components/common/`.
- Page components go in `pages/`.

## Styling

- Bootstrap 5 via `react-bootstrap` or CDN.
- Custom CSS in `src/styles/` if needed.
- CSS Modules or styled-components for scoped styles.
- Responsive design via Bootstrap grid.

## Environment Variables

```env
VITE_API_URL=http://localhost:5001
VITE_SIGNALR_URL=http://localhost:5001/hubs/notifications
```

## Rules

- No class components.
- No jQuery.
- No inline styles for complex components.
- TypeScript strict mode.
- All API calls go through `services/` layer.
- Handle loading and error states in UI.
- Use `ProtectedRoute` wrapper for authenticated routes.
