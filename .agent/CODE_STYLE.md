# CODE_STYLE.md — Coding Conventions

## C#

- `PascalCase` for public members, classes, methods, properties.
- `camelCase` for local variables, parameters, private fields.
- Prefix private fields with `_` (e.g., `_context`).
- Use `var` when type is obvious.
- Use `string.Empty` over `""`.
- Async methods suffixed with `Async`.
- One class per file.

## ASP.NET Core

- MVC Controllers: inherit `Controller`.
- API Controllers: inherit `ControllerBase`.
- Use `[ValidateAntiForgeryToken]` on all POST actions.
- Use `[Authorize]` with role parameters where needed.
- Return `Task<IActionResult>` for async actions.

## Services

- One interface per service class.
- Services registered as Scoped.
- Constructor injection only.
- Async/await throughout.

## Razor Views

- `@model` directive at top.
- Use Tag Helpers for URLs and forms.
- No business logic in `.cshtml`.
- Use partials for repeated UI.
- Use `@section` for scripts/styles.

## Formatting

- 4 spaces for C# indentation.
- Braces on new line (Allman style).
- Blank line between methods.

## Tests

- `Arrange / Act / Assert` pattern.
- Method naming: `MethodName_Scenario_ExpectedResult`.
- Use `[Fact]` for single, `[Theory]` for parameterized.
