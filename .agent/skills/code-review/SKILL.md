# code-review/SKILL.md — Review Code Changes

## Purpose

Guide agents to review code changes for correctness, security, and quality.

## When To Use

- Before completing any task.
- When reviewing PRs.
- When asked to review code.

## Inputs

- Changed files (diff).
- `.agent/AGENTS.md` for rules.
- `.agent/SECURITY.md` for security.

## Review Checklist

### Correctness
- [ ] Code does what it claims to do.
- [ ] Edge cases handled.
- [ ] Error handling present.
- [ ] No logic bugs.

### Architecture
- [ ] Follows layered architecture (Controller → Service → Data).
- [ ] Thin controllers.
- [ ] Business logic in services.
- [ ] No circular dependencies.

### Security
- [ ] No hardcoded secrets.
- [ ] Authentication/authorization correct.
- [ ] Input validated.
- [ ] No SQL injection, XSS vectors.
- [ ] CSRF protection on forms.

### Performance
- [ ] No N+1 queries (use `Include()`).
- [ ] No unnecessary database calls.
- [ ] Proper async/await usage.
- [ ] No blocking calls.

### Maintainability
- [ ] Follows existing patterns.
- [ ] No duplicate code.
- [ ] Clear naming.
- [ ] Minimal complexity.

### Tests
- [ ] Tests exist for new behavior.
- [ ] Existing tests still pass.
- [ ] Tests are meaningful (not just passing).

### Breaking Changes
- [ ] API backward compatibility maintained.
- [ ] Database migrations are safe.
- [ ] No silent behavior changes.

## Rules

- Prioritize real defects over stylistic preferences.
- Be constructive, not critical.
- Focus on what matters: correctness, security, performance.
- If unsure, ask rather than assume.
