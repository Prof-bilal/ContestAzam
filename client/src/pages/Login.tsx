import { useEffect, useState } from "react";
import type { FormEvent } from "react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { useToast } from "../components/Toast";
import { useCountdown } from "../hooks/useCountdown";
import { ApiError, NetworkError, RateLimitError, SuspendedError, oauthUrl } from "../api/client";

const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const LOCKOUT_STORAGE_KEY = "es_lockout_until";

function getRemainingLockout(): number {
  try {
    const until = parseInt(sessionStorage.getItem(LOCKOUT_STORAGE_KEY) ?? "0", 10);
    if (!until) return 0;
    const remaining = Math.ceil((until - Date.now()) / 1000);
    return remaining > 0 ? remaining : 0;
  } catch {
    return 0;
  }
}

function setLockoutExpiry(seconds: number) {
  sessionStorage.setItem(LOCKOUT_STORAGE_KEY, String(Date.now() + seconds * 1000));
}

function clearLockout() {
  sessionStorage.removeItem(LOCKOUT_STORAGE_KEY);
}

export function Login() {
  const { login } = useAuth();
  const { addToast } = useToast();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const countdown = useCountdown();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [fieldError, setFieldError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [oauthBusy, setOauthBusy] = useState<string | null>(null);

  // Handle error query params from OAuth redirects (e.g. account_suspended).
  useEffect(() => {
    const error = searchParams.get("error");
    if (error === "account_suspended") {
      const reason = searchParams.get("reason") || "";
      addToast("error", reason || "Your account has been suspended by an administrator.");
      navigate(`/suspended${reason ? `?reason=${encodeURIComponent(reason)}` : ""}`, { replace: true });
      return;
    }
    // Restore lockout timer on mount (survives page refresh)
    const remaining = getRemainingLockout();
    if (remaining > 0) {
      countdown.start(remaining);
    } else {
      clearLockout();
    }
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  const disabled = submitting || countdown.active || oauthBusy !== null;

  const validate = (): boolean => {
    if (!emailPattern.test(email)) {
      setFieldError("Enter a valid email address.");
      return false;
    }
    if (password.length === 0) {
      setFieldError("Password is required.");
      return false;
    }
    setFieldError(null);
    return true;
  };

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (disabled) return; // one-click / duplicate-submit protection
    if (!validate()) return;

    setSubmitting(true);
    try {
      await login(email, password);
      clearLockout();
      addToast("success", "Welcome back.");
      navigate("/dashboard");
    } catch (err) {
      if (err instanceof SuspendedError) {
        // Show the suspension message and redirect.
        addToast("error", err.reason || err.message);
        navigate("/suspended");
      } else if (err instanceof RateLimitError) {
        countdown.start(err.retryAfterSeconds);
        addToast("error", `Too many attempts. Try again in ${err.retryAfterSeconds} seconds.`);
      } else if (err instanceof ApiError) {
        if (err.status === 423) {
          // Account lockout: use the server-provided duration if available.
          const retryAfter = err.errors?.retryAfter ? parseInt(err.errors.retryAfter[0], 10) : 60;
          const seconds = Number.isFinite(retryAfter) && retryAfter > 0 ? retryAfter : 60;
          setLockoutExpiry(seconds);
          countdown.start(seconds);
          addToast("error", `Account locked. Try again in ${seconds} seconds.`);
        } else {
          addToast("error", err.status === 401 ? "Invalid email or password." : err.message);
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
    ? "Signing in…"
    : countdown.active
      ? `Locked (${countdown.seconds}s)`
      : "Login";

  return (
    <div className="center-screen">
      <form className="card auth-form" onSubmit={onSubmit} noValidate>
        <h1 className="brand-sm">EventSphere</h1>
        <h2>Sign in</h2>

        <label htmlFor="email">Email</label>
        <input
          id="email"
          type="email"
          value={email}
          autoComplete="email"
          onChange={(e) => setEmail(e.target.value)}
          disabled={disabled}
        />

        <label htmlFor="password">Password</label>
        <input
          id="password"
          type="password"
          value={password}
          autoComplete="current-password"
          onChange={(e) => setPassword(e.target.value)}
          disabled={disabled}
        />

        <p className="switch" style={{ marginTop: "0.5rem", marginBottom: 0 }}>
          <Link to="/forgot-password">Forgot Password?</Link>
        </p>

        {fieldError && <p className="field-error">{fieldError}</p>}

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
          No account? <Link to="/register">Create one</Link>
        </p>
      </form>
    </div>
  );
}
