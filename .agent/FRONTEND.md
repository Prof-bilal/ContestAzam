# FRONTEND.md — ASP.NET Core MVC + Razor Views

## Overview

EventSphere's frontend is built entirely within the ASP.NET Core ecosystem using **MVC Controllers and Razor Views**. There is no separate SPA framework.

> There is NO React, Vue, Angular, Next.js, or Vite frontend.

## Team Ownership

| Area | Owner |
|---|---|
| Layout, shared components, auth UI | **Ramsha** (Module 3) |
| Feature pages, dashboards, workflows | **Marukh** (Module 4) |

## Structure

```
Views/
├── _ViewImports.cshtml        # Global imports (Tag Helpers)
├── _ViewStart.cshtml          # Layout assignment
├── Shared/
│   ├── _Layout.cshtml         # Main layout (navbar, footer)
│   ├── _LoginPartial.cshtml   # Auth-aware nav links
│   └── Partials/
│       ├── _EventCard.cshtml  # Reusable event card
│       ├── _Pagination.cshtml # Page controls
│       └── _Alerts.cshtml     # Flash messages
├── Home/
│   └── Index.cshtml           # Landing page
├── Events/
│   ├── Index.cshtml           # Event listing with filters
│   ├── Details.cshtml         # Event detail page
│   ├── Create.cshtml          # Create event form
│   ├── Edit.cshtml            # Edit event form
│   └── MyEvents.cshtml        # User's events
├── Account/
│   ├── Login.cshtml
│   ├── Register.cshtml
│   └── Profile.cshtml
├── Dashboard/
│   ├── Index.cshtml           # User dashboard
│   ├── Organizer.cshtml       # Organizer dashboard
│   └── Admin.cshtml           # Admin dashboard
├── Tickets/
│   ├── Index.cshtml           # User tickets
│   └── Details.cshtml         # Ticket detail/QR
├── Gallery/
│   └── Index.cshtml           # Media gallery
├── Feedback/
│   └── Create.cshtml          # Review form
├── Admin/
│   ├── Users.cshtml           # User management
│   ├── Events.cshtml          # Event approval
│   └── Reports.cshtml         # Reports
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
- Footer with copyright and sitemap link.

## Partial Views

| Partial | Purpose | Owner |
|---|---|---|
| `_LoginPartial.cshtml` | Auth-aware navbar | Ramsha |
| `_EventCard.cshtml` | Reusable event card | Ramsha |
| `_Pagination.cshtml` | Page controls | Ramsha |
| `_Alerts.cshtml` | Flash messages | Ramsha |

## ViewModels

| ViewModel | Used By | Owner |
|---|---|---|
| `HomeIndexViewModel` | Home/Index | Ramsha |
| `EventListViewModel` | Events/Index | Ramsha |
| `EventDetailViewModel` | Events/Details | Marukh |
| `CreateEventViewModel` | Events/Create | Marukh |
| `DashboardViewModel` | Dashboard/Index | Marukh |
| `AdminDashboardViewModel` | Dashboard/Admin | Marukh |

## Tag Helpers

- `asp-controller`, `asp-action` — URL generation
- `asp-route-id`, `asp-route-page` — route parameters
- `asp-validation-for` — client-side validation messages
- `asp-append-version` — cache busting

## Forms

- All POST forms use `method="post"` with `[ValidateAntiForgeryToken]`.
- Client-side validation via jQuery Unobtrusive Validation.
- Shared form styles in `site.css`.

## Rules

- Never add React, Vue, Angular, or SPA frameworks.
- Always use Tag Helpers for URLs.
- Always use ViewModels — never pass raw entities to views.
- Keep business logic out of `.cshtml` files.
- Use partials for repeated UI components.
- Use `TempData` for flash messages.
- Ramsha owns shared components; Marukh consumes them.
