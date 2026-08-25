# github/SKILL.md — Git Operations and PRs

## Purpose

Guide agents to make safe Git operations and PRs for EventSphere.

## When To Use

- Starting any code change.
- Committing changes.
- Creating pull requests.
- Resolving merge conflicts.

## Inputs

- The repository root.
- Current branch status.

## Preconditions

- Git is installed.
- Repository is initialized.
- Agent has write access.

## Workflow

1. **Check status**: `git status` and `git log --oneline -5`.
2. **Create branch**: `git checkout -b feature/description` or `bugfix/description`.
3. **Make changes**: Follow AGENTS.md rules.
4. **Stage changes**: `git add <files>` — never `git add .` blindly.
5. **Commit**: `git commit -m "type(scope): description"`.
6. **Push**: `git push origin <branch>`.
7. **Create PR**: Describe what, why, and how to test.

## Branch Naming

```
feature/event-search
feature/user-notifications
bugfix/registration-error
hotfix/security-patch
```

## Commit Format

```
type(scope): description

feat(events): add event search
fix(auth): resolve login redirect
docs(api): update endpoint docs
test(events): add EventService tests
refactor(services): extract validation
```

## Rules

- Never commit directly to `main`.
- Never force-push to shared branches.
- Never commit secrets, keys, or connection strings.
- Stage only intended files.
- Squash merge feature branches.
- Delete branch after merge.

## Verification

```bash
git log --oneline -5    # verify commit messages
git diff --staged       # verify staged changes
```

## Failure Handling

- Merge conflict → read both sides, resolve logically, test.
- Pre-commit hook fails → fix the issue, re-stage, re-commit.
