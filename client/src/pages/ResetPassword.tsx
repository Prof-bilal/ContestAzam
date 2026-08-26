import { useState } from "react";
import type { FormEvent } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { useToast } from "../components/Toast";
import { PasswordRequirements, passwordMeetsPolicy } from "../components/PasswordRequirements";
import { ApiError, NetworkError, resetPassword } from "../api/client";

export function ResetPassword() {
  const [searchParams] = useSearchParams();
  const { addToast } = useToast();

  const token = searchParams.get("token") ?? "";
  const email = searchParams.get("email") ?? "";

  const [password, setPassword] = useState("");
  const [confirm, setConfirm] = useState("");
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [submitting, setSubmitting] = useState(false);
  const [success, setSuccess] = useState(false);

  const disabled = submitting;
  const confirmMismatch = confirm.length > 0 && confirm !== password;

  if (!token || !email) {
    return (
      <div className="center-screen">
        <div className="card auth-form" style={{ textAlign: "center" }}>
          <h1 className="brand-sm">EventSphere</h1>
          <h2>Invalid link</h2>
          <p className="muted" style={{ marginBottom: "1rem" }}>
            This password reset link is invalid. Please request a new one.
          </p>
          <Link to="/forgot-password" className="btn btn-primary" style={{ display: "inline-block", textDecoration: "none" }}>
            Request Reset Link
          </Link>
        </div>
      </div>
    );
  }

  const validate = (): boolean => {
    const next: Record<string, string> = {};
    if (!passwordMeetsPolicy(password)) next.password = "Password does not meet the requirements.";
    if (confirm !== password) next.confirmPassword = "Passwords do not match.";
    setErrors(next);
    return Object.keys(next).length === 0;
  };

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (disabled) return;
    if (!validate()) return;

    setSubmitting(true);
    try {
      await resetPassword(email, token, password, confirm);
      setSuccess(true);
      addToast("success", "Password reset successfully.");
    } catch (err) {
      if (err instanceof ApiError) {
        if (err.errors) {
          const mapped: Record<string, string> = {};
          for (const [key, msgs] of Object.entries(err.errors)) mapped[key] = msgs[0];
          setErrors(mapped);
          addToast("error", "Please fix the highlighted fields.");
        } else {
          addToast("error", err.message || "Invalid or expired reset token.");
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

  return (
    <div className="center-screen">
      <form className="card auth-form" onSubmit={onSubmit} noValidate>
        <h1 className="brand-sm">EventSphere</h1>

        {success ? (
          <>
            <h2>Password reset!</h2>
            <p className="muted" style={{ marginBottom: "1rem" }}>
              Your password has been reset successfully. You can now sign in with your new password.
            </p>
            <Link to="/login" className="btn btn-primary" style={{ display: "inline-block", textDecoration: "none" }}>
              Continue to Login
            </Link>
          </>
        ) : (
          <>
            <h2>Reset your password</h2>

            <label htmlFor="password">New Password</label>
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

            <button className="btn btn-primary" type="submit" disabled={disabled}>
              {submitting ? "Resetting password..." : "Reset Password"}
            </button>
          </>
        )}

        <p className="switch">
          <Link to="/login">Back to Sign In</Link>
        </p>
      </form>
    </div>
  );
}
