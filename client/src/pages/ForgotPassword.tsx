import { useState } from "react";
import type { FormEvent } from "react";
import { Link } from "react-router-dom";
import { useToast } from "../components/Toast";
import { useCountdown } from "../hooks/useCountdown";
import { NetworkError, RateLimitError, forgotPassword } from "../api/client";

const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export function ForgotPassword() {
  const { addToast } = useToast();
  const countdown = useCountdown();

  const [email, setEmail] = useState("");
  const [fieldError, setFieldError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [success, setSuccess] = useState(false);

  const disabled = submitting || countdown.active;

  const validate = (): boolean => {
    if (!emailPattern.test(email)) {
      setFieldError("Enter a valid email address.");
      return false;
    }
    setFieldError(null);
    return true;
  };

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (disabled) return;
    if (!validate()) return;

    setSubmitting(true);
    try {
      await forgotPassword(email.trim());
      setSuccess(true);
      addToast("success", "If an account exists, we sent a password reset link.");
    } catch (err) {
      if (err instanceof RateLimitError) {
        countdown.start(err.retryAfterSeconds);
        addToast("error", `Too many requests. Try again in ${err.retryAfterSeconds} seconds.`);
      } else if (err instanceof NetworkError) {
        addToast("error", err.message);
      } else {
        // Always show success message to prevent email enumeration.
        setSuccess(true);
        addToast("success", "If an account exists, we sent a password reset link.");
      }
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="center-screen">
      <form className="card auth-form" onSubmit={onSubmit} noValidate>
        <h1 className="brand-sm">EventSphere</h1>
        <h2>Forgot your password?</h2>

        {success ? (
          <>
            <p className="muted" style={{ lineHeight: 1.6 }}>
              If an account exists for this email, we sent a password reset link.
              Check your inbox and follow the instructions.
            </p>
            <p className="switch">
              <Link to="/login">Back to Sign In</Link>
            </p>
          </>
        ) : (
          <>
            <p className="muted" style={{ lineHeight: 1.6 }}>
              Enter your email address and we'll send you a link to reset your password.
            </p>

            <label htmlFor="email">Email</label>
            <input
              id="email"
              type="email"
              value={email}
              autoComplete="email"
              onChange={(e) => setEmail(e.target.value)}
              disabled={disabled}
            />
            {fieldError && <p className="field-error">{fieldError}</p>}

            <button className="btn btn-primary" type="submit" disabled={disabled}>
              {submitting
                ? "Sending..."
                : countdown.active
                  ? `Try again in ${countdown.seconds}s`
                  : "Send Reset Link"}
            </button>

            <p className="switch">
              Remember your password? <Link to="/login">Sign in</Link>
            </p>
          </>
        )}
      </form>
    </div>
  );
}
