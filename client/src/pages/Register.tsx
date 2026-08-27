import { useState } from "react";
import type { FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { useToast } from "../components/Toast";
import { useCountdown } from "../hooks/useCountdown";
import { PasswordRequirements, passwordMeetsPolicy } from "../components/PasswordRequirements";
import { ApiError, NetworkError, RateLimitError, oauthUrl } from "../api/client";

const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

function containsEmoji(text: string): boolean {
  for (const char of text) {
    const code = char.codePointAt(0)!;
    // Supplementary planes (U+10000+) — covers most emoji
    if (code >= 0x10000) return true;
    // Known BMP emoji ranges
    if (
      (code >= 0x2600 && code <= 0x26FF) || // Misc Symbols
      (code >= 0x2700 && code <= 0x27BF) || // Dingbats
      (code >= 0x2300 && code <= 0x23FF) || // Misc Technical
      (code >= 0x25A0 && code <= 0x25FF) || // Geometric Shapes
      (code >= 0x2B00 && code <= 0x2BFF) || // Misc Symbols and Arrows
      code === 0x2122 // ™
    ) {
      return true;
    }
  }
  return false;
}

export function Register() {
  const { register } = useAuth();
  const { addToast } = useToast();
  const navigate = useNavigate();
  const countdown = useCountdown();

  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirm, setConfirm] = useState("");
  const [department, setDepartment] = useState("");
  const [enrollmentNo, setEnrollmentNo] = useState("");
  const [accountType, setAccountType] = useState<"Visitor" | "Organizer">("Visitor");
  const [orgName, setOrgName] = useState("");
  const [orgReason, setOrgReason] = useState("");
  const [orgExperience, setOrgExperience] = useState("");
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [submitting, setSubmitting] = useState(false);
  const [oauthBusy, setOauthBusy] = useState<string | null>(null);

  const disabled = submitting || countdown.active || oauthBusy !== null;
  const confirmMismatch = confirm.length > 0 && confirm !== password;

  const validate = (): boolean => {
    const next: Record<string, string> = {};
    if (name.trim().length < 2) next.name = "Name must be at least 2 characters.";
    else if (containsEmoji(name)) next.name = "Name contains invalid characters. Emoji are not allowed.";
    if (!emailPattern.test(email)) next.email = "Enter a valid email address.";
    if (!passwordMeetsPolicy(password)) next.password = "Password does not meet the requirements.";
    if (confirm !== password) next.confirmPassword = "Passwords do not match.";

    if (accountType === "Organizer") {
      if (orgName.trim().length < 2) next.orgName = "Organization name must be at least 2 characters.";
      else if (containsEmoji(orgName)) next.orgName = "Organization name contains invalid characters. Emoji are not allowed.";
      if (orgReason.trim().length < 10) next.orgReason = "Please provide a reason (at least 10 characters).";
    }

    setErrors(next);
    return Object.keys(next).length === 0;
  };

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (disabled) return;
    if (!validate()) return;

    setSubmitting(true);
    try {
      await register(
        name.trim(),
        email.trim(),
        password,
        confirm,
        accountType,
        accountType === "Organizer" ? orgName.trim() : undefined,
        accountType === "Organizer" ? orgReason.trim() : undefined,
        accountType === "Organizer" ? orgExperience.trim() || undefined : undefined,
        department.trim() || undefined,
        enrollmentNo.trim() || undefined,
      );
      addToast("success", "Account created. Check your email to verify your account.");
      navigate("/verify-email?email=" + encodeURIComponent(email.trim()));
    } catch (err) {
      if (err instanceof RateLimitError) {
        countdown.start(err.retryAfterSeconds);
        addToast("error", `Too many attempts. Try again in ${err.retryAfterSeconds} seconds.`);
      } else if (err instanceof ApiError) {
        if (err.status === 409) {
          setErrors({ email: "An account with this email already exists." });
          addToast("error", "Unable to create account.");
        } else if (err.errors) {
          const mapped: Record<string, string> = {};
          for (const [key, msgs] of Object.entries(err.errors)) mapped[key] = msgs[0];
          setErrors(mapped);
          addToast("error", "Please fix the highlighted fields.");
        } else {
          addToast("error", "Unable to create account.");
        }
      } else if (err instanceof NetworkError) {
        addToast("error", err.message);
      } else {
        addToast("error", "Something went wrong. Please try again.");
      }
    } finally {
      setSubmitting(false);
    }
  };

  const startOauth = (provider: "google" | "github") => {
    setOauthBusy(provider);
    window.location.href = oauthUrl(provider);
  };

  const label = submitting
    ? "Creating account…"
    : countdown.active
      ? `Try again in ${countdown.seconds}s`
      : accountType === "Organizer"
        ? "Create Account & Apply"
        : "Create Account";

  return (
    <div className="center-screen">
      <form className="card auth-form" onSubmit={onSubmit} noValidate>
        <h1 className="brand-sm">EventSphere</h1>
        <h2>Create your account</h2>

        {/* Account type selector */}
        <fieldset style={{ border: "none", padding: 0, margin: "0.5rem 0" }}>
          <legend style={{ fontSize: "0.9rem", color: "var(--muted)", marginBottom: "0.5rem" }}>
            How do you want to use EventSphere?
          </legend>
          <div style={{ display: "flex", gap: "1rem", marginBottom: "0.5rem" }}>
            <label
              style={{
                flex: 1,
                display: "flex",
                alignItems: "flex-start",
                gap: "0.5rem",
                padding: "0.75rem",
                border: "1px solid var(--ink-violet)",
                borderRadius: "0px",
                cursor: disabled ? "not-allowed" : "pointer",
                background: accountType === "Visitor" ? "rgba(250,229,155,0.3)" : "transparent",
                opacity: disabled ? 0.6 : 1,
              }}
            >
              <input
                type="radio"
                name="accountType"
                value="Visitor"
                checked={accountType === "Visitor"}
                onChange={() => setAccountType("Visitor")}
                disabled={disabled}
                style={{ marginTop: "0.2rem" }}
              />
              <div>
                <div style={{ fontWeight: 600, fontSize: "0.9rem" }}>Visitor</div>
                <div style={{ fontSize: "0.8rem", color: "var(--muted)" }}>
                  Discover and participate in events.
                </div>
              </div>
            </label>
            <label
              style={{
                flex: 1,
                display: "flex",
                alignItems: "flex-start",
                gap: "0.5rem",
                padding: "0.75rem",
                border: "1px solid var(--ink-violet)",
                borderRadius: "0px",
                cursor: disabled ? "not-allowed" : "pointer",
                background: accountType === "Organizer" ? "rgba(250,229,155,0.3)" : "transparent",
                opacity: disabled ? 0.6 : 1,
              }}
            >
              <input
                type="radio"
                name="accountType"
                value="Organizer"
                checked={accountType === "Organizer"}
                onChange={() => setAccountType("Organizer")}
                disabled={disabled}
                style={{ marginTop: "0.2rem" }}
              />
              <div>
                <div style={{ fontWeight: 600, fontSize: "0.9rem" }}>Organizer</div>
                <div style={{ fontSize: "0.8rem", color: "var(--muted)" }}>
                  Create and manage events.
                  <br />
                  Requires Admin approval.
                </div>
              </div>
            </label>
          </div>
        </fieldset>

        <label htmlFor="name">Name</label>
        <input id="name" value={name} onChange={(e) => setName(e.target.value)} disabled={disabled} />
        {errors.name && <p className="field-error">{errors.name}</p>}

        <label htmlFor="email">Email</label>
        <input
          id="email"
          type="email"
          value={email}
          autoComplete="email"
          onChange={(e) => setEmail(e.target.value)}
          disabled={disabled}
        />
        {errors.email && <p className="field-error">{errors.email}</p>}

        <label htmlFor="department">Department (optional)</label>
        <input
          id="department"
          value={department}
          onChange={(e) => setDepartment(e.target.value)}
          disabled={disabled}
          placeholder="e.g. Computer Science"
        />

        <label htmlFor="enrollmentNo">Enrollment Number (optional)</label>
        <input
          id="enrollmentNo"
          value={enrollmentNo}
          onChange={(e) => setEnrollmentNo(e.target.value)}
          disabled={disabled}
          placeholder="e.g. ENR-2024-001"
        />

        <label htmlFor="password">Password</label>
        <input
          id="password"
          type="password"
          value={password}
          autoComplete="new-password"
          onChange={(e) => setPassword(e.target.value)}
          disabled={disabled}
        />
        <PasswordRequirements password={password} />
        {errors.password && <p className="field-error">{errors.password}</p>}

        <label htmlFor="confirm">Confirm Password</label>
        <input
          id="confirm"
          type="password"
          value={confirm}
          autoComplete="new-password"
          onChange={(e) => setConfirm(e.target.value)}
          disabled={disabled}
        />
        {(confirmMismatch || errors.confirmPassword) && (
          <p className="field-error">Passwords do not match.</p>
        )}

        {/* Organizer-specific fields */}
        {accountType === "Organizer" && (
          <>
            <label htmlFor="orgName">Organization Name</label>
            <input
              id="orgName"
              value={orgName}
              onChange={(e) => setOrgName(e.target.value)}
              disabled={disabled}
              placeholder="Your organization or team name"
            />
            {errors.orgName && <p className="field-error">{errors.orgName}</p>}

            <label htmlFor="orgReason">Why do you want to organize events?</label>
            <textarea
              id="orgReason"
              value={orgReason}
              onChange={(e) => setOrgReason(e.target.value)}
              disabled={disabled}
              rows={3}
              placeholder="Tell us why you want to organize events on EventSphere"
              style={{ resize: "vertical" }}
            />
            {errors.orgReason && <p className="field-error">{errors.orgReason}</p>}

            <label htmlFor="orgExperience">Previous Experience (optional)</label>
            <textarea
              id="orgExperience"
              value={orgExperience}
              onChange={(e) => setOrgExperience(e.target.value)}
              disabled={disabled}
              rows={3}
              placeholder="Describe any previous event organizing experience"
              style={{ resize: "vertical" }}
            />
          </>
        )}

        <button className="btn btn-primary" type="submit" disabled={disabled}>
          {label}
        </button>

        <div className="divider">or</div>

        <button
          type="button"
          className="btn btn-oauth"
          onClick={() => startOauth("google")}
          disabled={disabled}
        >
          {oauthBusy === "google" ? "Redirecting…" : "Continue with Google"}
        </button>
        <button
          type="button"
          className="btn btn-oauth"
          onClick={() => startOauth("github")}
          disabled={disabled}
        >
          {oauthBusy === "github" ? "Redirecting…" : "Continue with GitHub"}
        </button>

        <p className="switch">
          Already have an account? <Link to="/login">Sign in</Link>
        </p>
      </form>
    </div>
  );
}
