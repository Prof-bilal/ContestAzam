# GITHUB.md — Git/GitHub Workflow

## Branch Strategy

- `main` — production-ready code
- `develop` — integration branch (if used)
- `feature/*` — new features
- `bugfix/*` — bug fixes
- `hotfix/*` — production fixes

## Branch Naming

```
feature/event-search
feature/user-notifications
bugfix/registration-error
hotfix/security-patch
```

## Commit Conventions

```
type(scope): description

Examples:
feat(events): add event search functionality
fix(auth): resolve login redirect issue
docs(api): update endpoint documentation
refactor(services): extract validation logic
test(events): add unit tests for EventService
```

## PR Workflow

1. Create feature branch from `main`.
2. Make changes, commit with meaningful messages.
3. Push branch, create PR.
4. PR description: what changed, why, how to test.
5. Code review before merge.
6. Merge via squash or rebase.

## Code Review

- Check for correctness, security, performance.
- Verify tests exist for new functionality.
- Ensure no secrets committed.
- Check for breaking changes.

## CI/CD

Currently not configured (P2 gap).

Recommended:
- Build on every PR.
- Run tests on every PR.
- Deploy on merge to `main`.

## Issues

- Use GitHub Issues for bugs and features.
- Label: `bug`, `enhancement`, `documentation`, `security`.
- Milestone for releases.

## Tags

- Semantic versioning: `v1.0.0`, `v1.1.0`, `v1.0.1`.

## Rules

- Never force-push to `main`.
- Never commit directly to `main`.
- Never commit secrets or credentials.
- Squash merge feature branches.
- Delete branches after merge.
