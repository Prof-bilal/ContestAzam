# FRONTEND.md — ASP.NET Core MVC + Razor Views

## Overview

EventSphere's frontend is built entirely within the ASP.NET Core ecosystem using **MVC Controllers and Razor Views**. There is no separate SPA framework.

> There is NO React, Vue, Angular, Next.js, or Vite frontend.

## Structure

```
Views/
├── _ViewImports.cshtml        # Global imports (Tag Helpers)
├── _ViewStart.cshtml          # Layout assignment
├── Shared/
│   ├── _Layout.cshtml         # Main layout (navbar, footer)
│   ├── _LoginPartial.cshtml   # Auth-aware nav links
│   └── Partials/
│       └── _EventCard.cshtml  # Reusable event card component
├── Home/
│   └── Index.cshtml           # Landing page
├── Events/
│   ├── Index.cshtml           # Event listing
│   ├── Details.cshtml         # Event detail page
│   ├── Create.cshtml          # Create event form
│   └── MyEvents.cshtml        # User's events
├── Account/
│   ├── Login.cshtml
│   ├── Register.cshtml
│   └── Profile.cshtml
├── Tickets/
│   ├── Index.cshtml           # User tickets
│   └── Details.cshtml         # Ticket detail/code
wwwroot/
├── css/site.css               # Custom styles
├── js/site.js                 # Custom JS
├── lib/bootstrap/             # Bootstrap CSS/JS
└── images/                    # Static images
```

## Layout

- `_Layout.cshtml` wraps all pages.
- Contains navbar with brand, navigation, auth links.
- Bootstrap 5 for responsive grid and components.
- Bootstrap Icons for iconography.
- Footer with copyright.

## Partial Views

| Partial | Purpose |
|---|---|
| `_LoginPartial.cshtml` | Auth-aware navbar (login/register vs profile/logout) |
| `_EventCard.cshtml` | Reusable event card (image, title, date, location, badge) |

## ViewModels

| ViewModel | Used By |
|---|---|
| `HomeIndexViewModel` | Home/Index — upcoming events, stats |
| `EventListViewModel` | Events/Index — paginated list, search, filter |
| `EventDetailViewModel` | Events/Details — single event + registration status |
| `CreateEventViewModel` | Events/Create — form with validation |

## Tag Helpers

- `asp-controller`, `asp-action` — URL generation
- `asp-route-id`, `asp-route-page` — route parameters
- `asp-validation-for` — client-side validation messages
- `asp-validation-summary` — validation summary
- `asp-append-version` — cache busting

## Forms

- All POST forms use `method="post"` with `[ValidateAntiForgeryToken]`.
- Forms include `@Html.AntiForgeryToken()` or tag helper equivalent.
- Client-side validation via jQuery Unobtrusive Validation.

## Routing

```
/                       → Home/Index
/Events                 → Events/Index
/Events/Details/{id}    → Events/Details
/Events/Create          → Events/Create (GET/POST)
/Account/Login          → Account/Login
/Account/Register       → Account/Register
/Tickets                → Tickets/Index
```

## JavaScript

- Minimal vanilla JS in `site.js`.
- Bootstrap bundle (includes Popper).
- SignalR JavaScript client (loaded on pages needing real-time).

## CSS

- Bootstrap 5 via `lib/bootstrap/`.
- Custom styles in `css/site.css`.
- CSS variables for theming (`--primary`, `--secondary`).
- Gradient hero sections.

## Rules

- Never add React, Vue, Angular, or SPA frameworks.
- Always use Tag Helpers for URLs.
- Always use ViewModels — never pass raw entities to views.
- Keep business logic out of `.cshtml` files.
- Use partials for repeated UI components.
- Use `TempData` for flash messages.
