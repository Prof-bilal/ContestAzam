# FRONTEND.md — React SPA

## Overview

EventSphere frontend is a **React 18+ SPA** built with Vite. Communicates with ASP.NET Core Web API over HTTP/JSON.

> NO ASP.NET Core MVC or Razor Views.

## Tech Stack

| Tool | Purpose |
|---|---|
| React 18+ | UI library |
| TypeScript | Type safety |
| Vite | Build tool / dev server |
| React Router v6 | Client-side routing |
| Axios | HTTP client |
| React Bootstrap | UI components |
| Context API | State management |
| @microsoft/signalr | Real-time client |

## Project Structure

```
EventSphere.React/src/
├── api/axios.ts
├── components/
│   ├── common/          # Navbar, Footer, Loader, ProtectedRoute
│   ├── events/          # EventCard, EventForm, EventFilters
│   ├── gallery/         # GalleryGrid, MediaUpload
│   ├── dashboard/       # StatsCard, Charts
│   └── reviews/         # ReviewForm, StarRating
├── pages/
│   ├── Home.tsx
│   ├── Events.tsx
│   ├── EventDetail.tsx
│   ├── Login.tsx
│   ├── Register.tsx
│   ├── Dashboard.tsx
│   ├── MyRegistrations.tsx
│   ├── Gallery.tsx
│   ├── AdminPanel.tsx
│   └── NotFound.tsx
├── context/
│   ├── AuthContext.tsx
│   └── NotificationContext.tsx
├── hooks/
├── services/
│   ├── authService.ts
│   ├── eventService.ts
│   └── ...
├── types/index.ts
├── App.tsx
└── main.tsx
```

## Team Ownership

| Area | Owner |
|---|---|
| Layout, shared components, auth pages | **Ramsha** (Module 3) |
| Feature pages, dashboards, workflows | **Marukh** (Module 4) |

## Routing

```tsx
<Routes>
  <Route path="/" element={<Home />} />
  <Route path="/events" element={<Events />} />
  <Route path="/events/:id" element={<EventDetail />} />
  <Route path="/login" element={<Login />} />
  <Route path="/register" element={<Register />} />
  <Route path="/dashboard" element={<ProtectedRoute><Dashboard /></ProtectedRoute>} />
  <Route path="/admin" element={<ProtectedRoute role="Admin"><AdminPanel /></ProtectedRoute>} />
  <Route path="*" element={<NotFound />} />
</Routes>
```

## Auth Flow

1. Login → POST `/api/auth/login` → JWT token
2. Token stored in localStorage
3. Axios interceptor attaches `Authorization: Bearer {token}`
4. On 401 → redirect to `/login`
5. AuthContext provides `user`, `token`, `login()`, `logout()`

## Rules

- Functional components only.
- TypeScript strict mode.
- All API calls through `services/` layer.
- Handle loading and error states.
- Use `ProtectedRoute` for auth routes.
- Bootstrap for styling.
