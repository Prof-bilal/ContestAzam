# security/SKILL.md — Security Review Checklist

## Purpose

Guide agents to perform security review of code changes before completing work.

## When To Use

- Any code change that touches authentication, authorization, data access, or user input.
- Before completing any task.

## Inputs

- Changed files.
- `.agent/SECURITY.md` for full security rules.

## Preconditions

- Read `.agent/SECURITY.md`.
- Understand what was changed.

## Checklist

### Secrets
- [ ] No hardcoded passwords, API keys, tokens, or secrets.
- [ ] No real connection strings in source code.
- [ ] JWT keys are in configuration, not code.

### Authentication
- [ ] `[Authorize]` on protected endpoints.
- [ ] Role-based access where needed.
- [ ] No bypass of authentication.

### Input Validation
- [ ] All user input validated (model validation).
- [ ] No SQL injection (use EF Core parameterized queries).
- [ ] No XSS (Razor auto-encodes; no unsafe `@Html.Raw()`).
- [ ] File uploads validated (type, size) if applicable.

### Data Exposure
- [ ] No sensitive data in API responses (passwords, tokens, internal IDs).
- [ ] No stack traces in error responses.
- [ ] No logging of sensitive data.

### CSRF
- [ ] `[ValidateAntiForgeryToken]` on MVC POST actions.
- [ ] Anti-forgery tokens in forms.

### Dependencies
- [ ] No known vulnerable packages added.
- [ ] `dotnet list package --vulnerable` checked.

## Workflow

1. Review all changed files against checklist.
2. Flag any issues found.
3. Fix issues before marking work complete.
4. Document any security-relevant decisions.

## Verification

```bash
dotnet list package --vulnerable
```

No security regressions in changed code.

## Failure Handling

- If a security issue is found → fix it immediately.
- If unsure → flag for manual review, do not proceed.
