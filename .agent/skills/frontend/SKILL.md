# frontend/SKILL.md — Modify MVC/Razor Frontend

## Purpose

Guide agents to safely modify MVC Controllers, Razor Views, ViewModels, and shared UI.

## When To Use

- Adding or modifying a view.
- Changing controller actions that serve views.
- Adding ViewModels.
- Changing layouts, partials, or styles.

## Module Ownership

- **Ramsha (Module 3)**: Layout, shared components, auth UI, event listing
- **Marukh (Module 4)**: Feature pages, dashboards, event forms

## Rules

- **NEVER introduce React, Vue, Angular, or SPA frameworks.**
- Use Tag Helpers for URLs.
- Use ViewModels — never pass raw entities to views.
- Keep business logic out of `.cshtml` files.
- Use `[ValidateAntiForgeryToken]` on POST actions.
- Use partials for repeated UI.
- Ramsha owns shared components; Marukh consumes them.

## Verification

```bash
dotnet build
dotnet run --project EventSphere.Web
```
