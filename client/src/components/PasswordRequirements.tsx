const rules = [
  { label: "At least 12 characters", test: (p: string) => p.length >= 12 },
  { label: "An uppercase letter", test: (p: string) => /[A-Z]/.test(p) },
  { label: "A lowercase letter", test: (p: string) => /[a-z]/.test(p) },
  { label: "A number", test: (p: string) => /[0-9]/.test(p) },
  { label: "A special character", test: (p: string) => /[^A-Za-z0-9]/.test(p) },
  { label: "At least 4 unique characters", test: (p: string) => new Set(p).size >= 4 },
];

/// Live client-side checklist. This is UX only — the backend performs the
/// authoritative password validation.
export function passwordMeetsPolicy(password: string): boolean {
  return rules.every((r) => r.test(password));
}

export function PasswordRequirements({ password }: { password: string }) {
  return (
    <ul className="pw-requirements">
      {rules.map((r) => {
        const ok = r.test(password);
        return (
          <li key={r.label} className={ok ? "pw-ok" : "pw-todo"}>
            <span aria-hidden="true">{ok ? "✓" : "○"}</span> {r.label}
          </li>
        );
      })}
    </ul>
  );
}
