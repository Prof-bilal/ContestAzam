# frontend/SKILL.md — Modify MVC/Razor Frontend

## Purpose

Guide agents to safely modify MVC Controllers, Razor Views, ViewModels, Layouts, JavaScript, and CSS.

## When To Use

- Adding or modifying a view.
- Changing controller actions that serve views.
- Adding or modifying ViewModels.
- Changing layouts, partials, or styles.
- Adding JavaScript functionality.

## Inputs

- The view, controller, or ViewModel being modified.
- `.agent/FRONTEND.md` for structure.
- `.agent/CODE_STYLE.md` for conventions.

## Preconditions

- Understand existing view structure.
- Understand ViewModel patterns.
- Read `_Layout.cshtml` and `_ViewImports.cshtml`.

## Workflow

1. **Identify the layer**: Controller action → ViewModel → View.
2. **Read controller**: Understand what data is passed to the view.
3. **Read ViewModel**: Check existing ViewModel or create new one.
4. **Read view**: Understand existing Razor patterns.
5. **Make change**: Controller → ViewModel → View (in that order).
6. **Verify build**: `dotnet build`.
7. **Test in browser**: Check rendering and form submissions.

## Rules

- **NEVER introduce React, Vue, Angular, Next.js, or SPA frameworks.**
- Use Tag Helpers for URLs (`asp-controller`, `asp-action`).
- Use ViewModels — never pass raw entities to views.
- Keep business logic out of `.cshtml` files.
- Use `[ValidateAntiForgeryToken]` on POST actions.
- Use `TempData` for flash messages.
- Use partials for repeated UI components.
- Keep JavaScript minimal — prefer server-side rendering.
- Use Bootstrap classes for styling.
- Use `@Html.Raw()` only with trusted content.

## Verification

```bash
dotnet build
dotnet run --project EventSphere.Web
```

Visit pages in browser, verify forms submit correctly.

## Failure Handling

- View not found → check folder structure matches controller name.
- Model is null → check controller passes data to view.
- Validation not working → check ViewModel annotations.
- Form not submitting → check anti-forgery token.
