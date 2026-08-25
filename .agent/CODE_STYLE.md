# CODE_STYLE.md — Coding Conventions

## C# (Backend)

- `PascalCase` for public members, classes, methods, properties.
- `camelCase` for local variables, parameters, private fields.
- Prefix private fields with `_` (e.g., `_context`).
- Use `var` when type is obvious.
- Use `string.Empty` over `""`.
- Async methods suffixed with `Async`.
- One class per file.

## ASP.NET Core API

- Controllers: inherit `ControllerBase`.
- Use `[ApiController]` + `[Route("api/[controller]")]`.
- Return `Task<IActionResult>` for async actions.
- One endpoint per method.

## Services

- One interface per service class.
- Services registered as Scoped.
- Constructor injection only.
- Async/await throughout.

## React / TypeScript

- **Components**: Functional only, `PascalCase` filenames.
- **Files**: `PascalCase.tsx` for components, `camelCase.ts` for utilities.
- **Props**: TypeScript interfaces, destructure in function signature.
- **Hooks**: `use` prefix, custom hooks in `hooks/` directory.
- **State**: `useState`, `useReducer`, Context API.
- **Styling**: Bootstrap classes, CSS Modules for custom styles.

## TypeScript

- Strict mode enabled.
- Prefer `interface` over `type` for object shapes.
- Use `enum` for fixed value sets.
- Avoid `any` — use `unknown` if type is truly unknown.
- Export types separately from implementations.

## File Organization

```
# Backend
Controllers/    → Thin API controllers
Services/       → Business logic
Models/         → Entities
DTOs/           → Data transfer objects
Data/           → DbContext, migrations

# Frontend
components/     → Reusable UI components
pages/          → Route-level page components
services/       → API call functions
context/        → React Context providers
hooks/          → Custom React hooks
types/          → TypeScript interfaces
utils/          → Helper functions
```

## Formatting

### C#
- 4 spaces indentation.
- Braces on new line (Allman style).
- Blank line between methods.

### TypeScript/React
- 2 spaces indentation.
- Single quotes for strings.
- Semicolons at end of statements.
- Trailing commas in multiline.

## Comments

- Do not add comments unless requested.
- Use XML docs for public API surface (backend).
- Code should be self-documenting.

## Tests

### C# (xunit)
- `Arrange / Act / Assert` pattern.
- Method naming: `MethodName_Scenario_ExpectedResult`.

### TypeScript (Vitest/Jest)
- `describe` blocks for grouping.
- `it` or `test` for individual tests.
- `expect` assertions.
- Mock API calls with `vi.fn()` or MSW.
