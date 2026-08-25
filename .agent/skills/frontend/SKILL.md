# frontend/SKILL.md — Modify React SPA

## Purpose

Guide agents to safely modify React components, pages, services, hooks, and styling.

## When To Use

- Adding or modifying a React component.
- Changing API call functions.
- Modifying routing.
- Adding new pages.
- Changing styling or layout.

## Inputs

- The React file being modified.
- `.agent/FRONTEND.md` for structure.
- `.agent/CODE_STYLE.md` for conventions.

## Preconditions

- Node.js 18+ installed.
- React project in `EventSphere.React/`.
- Read existing component patterns.

## Workflow

1. **Identify the layer**: Page component → reusable component → service → API call.
2. **Read existing similar component**: Understand patterns.
3. **Make change**: Component → service → types (in that order).
4. **Type everything**: Use TypeScript interfaces for props and API responses.
5. **Test in browser**: Verify rendering and API calls.
6. **Run tests**: `npm test` in `EventSphere.React/`.

## Rules

- **Functional components only** — no class components.
- Use TypeScript (`.tsx` for components, `.ts` for logic).
- Props must be typed with interfaces.
- API calls go in `services/` directory.
- Use Axios instance from `api/axios.ts`.
- Handle loading and error states in UI.
- Use `ProtectedRoute` for authenticated routes.
- Never store JWT in plain JavaScript variables — use localStorage or httpOnly cookie.
- Never use `dangerouslySetInnerHTML` with untrusted content.
- Bootstrap for styling (react-bootstrap or CSS classes).

## Project Structure

```
src/
├── api/axios.ts           # Axios instance
├── components/            # Reusable components
│   ├── common/            # Navbar, Footer, Loader, ProtectedRoute
│   ├── events/            # EventCard, EventForm
│   └── ...
├── pages/                 # Route pages
├── services/              # API functions
├── context/               # React Context
├── hooks/                 # Custom hooks
├── types/                 # TypeScript interfaces
└── utils/                 # Helpers
```

## Verification

```bash
cd EventSphere.React
npm run build    # TypeScript compiles
npm test         # Tests pass
npm run dev      # App runs in browser
```

## Failure Handling

- TypeScript errors → fix type definitions.
- Build fails → check imports, missing dependencies.
- Component doesn't render → check props, state, routing.
- API call fails → check Axios config, CORS, backend running.
