# frontend/SKILL.md — Modify React SPA

## Purpose

Guide agents to safely modify React components, pages, services, hooks, and styling.

## Module Ownership

- **Ramsha (Module 3)**: Layout, shared components, auth pages
- **Marukh (Module 4)**: Feature pages, dashboards, workflows

## Rules

- Functional components only (no class components).
- TypeScript (`.tsx` for components, `.ts` for logic).
- API calls in `services/` directory.
- Use Axios instance from `api/axios.ts`.
- Handle loading and error states.
- Use `ProtectedRoute` for auth routes.
- Never use `dangerouslySetInnerHTML` with untrusted content.

## Verification

```bash
cd EventSphere.React
npm run build
npm test
npm run dev
```
