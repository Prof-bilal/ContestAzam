# security/SKILL.md — Security Review

## Checklist

- [ ] No secrets hardcoded
- [ ] `[Authorize]` on protected endpoints
- [ ] `[ValidateAntiForgeryToken]` on POST forms
- [ ] Input validated (server-side)
- [ ] No SQL injection (EF Core parameterized)
- [ ] No XSS (Razor auto-encodes)
- [ ] CORS configured correctly
- [ ] Sensitive data not logged

## Rules

- Security issue found → fix immediately.
- If unsure → flag for review.
