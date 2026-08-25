# GITHUB.md — Git/GitHub Workflow

## Branch Strategy

- `main` — production-ready code
- `feature/*` — new features
- `bugfix/*` — bug fixes
- `hotfix/*` — production fixes

## Branch Naming

```
feature/event-search-api
feature/react-event-filters
bugfix/cors-preflight-error
hotfix/jwt-expiry-fix
```

## Monorepo Structure

This repo contains two projects:
- `EventSphere.Api/` — ASP.NET Core backend
- `EventSphere.React/` — React frontend

Commit changes to both when the feature spans both layers.

## Commit Conventions

```
type(scope): description

feat(api): add event search endpoint
feat(react): add event filter component
fix(auth): resolve JWT expiry issue
docs(agent): update frontend docs
test(api): add EventService unit tests
refactor(react): extract EventCard component
```

## PR Workflow

1. Create feature branch from `main`.
2. Make changes in both `EventSphere.Api/` and `EventSphere.React/` if needed.
3. Run `dotnet test` and `npm test`.
4. Push branch, create PR.
5. PR description: what changed, why, how to test (include both API and UI steps).
6. Code review before merge.

## Rules

- Never force-push to `main`.
- Never commit directly to `main`.
- Never commit secrets, `.env`, or credentials.
- Squash merge feature branches.
- Delete branch after merge.
