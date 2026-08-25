# CODE_STYLE.md — Coding Conventions

## C#

- Use `PascalCase` for public members, classes, methods, properties.
- Use `camelCase` for local variables, parameters, private fields.
- Prefix private fields with `_` (e.g., `_context`).
- Use `var` when type is obvious.
- Prefer `is null` over `== null`.
- Use `string.Empty` over `""`.
- Use expression-bodied members for simple one-liners.
- Async methods suffixed with `Async`.
- One class per file.

## ASP.NET Core

- Controllers: inherit `Controller` (MVC) or `ControllerBase` (API).
- Use `[ValidateAntiForgeryToken]` on all POST actions.
- Use `[Authorize]` with role parameters where needed.
- Return `Task<IActionResult>` for async actions.
- Use `TempData` for flash messages.
- Use `ViewData`/`ViewBag` sparingly — prefer ViewModels.

## Services

- One interface per service class.
- Services registered as Scoped.
- Constructor injection only.
- Async/await throughout.
- Throw exceptions for business rule violations, return null for "not found".

## Models/Entities

- One entity per file.
- Use data annotations for validation.
- Navigation properties as virtual (for EF Core proxying).
- No business logic in entities.

## ViewModels

- One ViewModel per view (or shared view).
- Use `[Required]`, `[StringLength]`, `[Display]` annotations.
- Name: `{Purpose}ViewModel`.

## Razor Views

- `@model` directive at top.
- Use Tag Helpers for URLs and forms.
- No business logic in `.cshtml`.
- Use partials for repeated UI.
- Use `@section` for scripts/styles.

## JavaScript

- Vanilla JS (no jQuery dependency for new code).
- Use `addEventListener` for events.
- Keep JS minimal — prefer server-side rendering.

## CSS

- Use Bootstrap classes where possible.
- Custom styles in `site.css`.
- Use CSS variables for theming.
- Avoid `!important`.

## Tests

- `Arrange / Act / Assert` pattern.
- Method naming: `MethodName_Scenario_ExpectedResult`.
- One assertion per test (preferred).
- Use `[Fact]` for single, `[Theory]` for parameterized.

## Comments

- Do not add comments unless requested.
- Use XML docs for public API surface only.
- Code should be self-documenting.

## Formatting

- 4 spaces for C# indentation.
- Braces on new line (Allman style).
- Blank line between methods.
- Max line length: prefer readability over strict limit.
