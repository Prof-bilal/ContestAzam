# code-review/SKILL.md — Review Code Changes

## Purpose

Guide agents to review code changes for correctness, security, and quality.

## When To Use

- Before completing any task.
- When reviewing PRs.

## Review Checklist

### Correctness
- [ ] Code does what it claims.
- [ ] Edge cases handled.
- [ ] Error handling present.

### Architecture
- [ ] Backend: Controller → Service → Data.
- [ ] Frontend: Page → Component → Service → API.
- [ ] No business logic in controllers or React components.
- [ ] DTOs used for API responses.

### Security (Both Layers)
- [ ] No hardcoded secrets.
- [ ] Auth/authorization correct.
- [ ] Input validated (frontend + backend).
- [ ] No XSS, SQL injection vectors.

### Performance
- [ ] No N+1 queries (backend `Include()`).
- [ ] No unnecessary re-renders (React `memo`, `useMemo`).
- [ ] Proper async/await.

### Maintainability
- [ ] Follows existing patterns.
- [ ] TypeScript types defined.
- [ ] No duplicate code.

### Tests
- [ ] Backend tests exist for new API behavior.
- [ ] Frontend tests exist for new components.
- [ ] All tests pass.

## Rules

- Prioritize real defects over style.
- Check both backend and frontend for full-stack changes.
- Be constructive.
