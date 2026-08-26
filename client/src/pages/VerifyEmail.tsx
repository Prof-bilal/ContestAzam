import { useEffect, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { useToast } from "../components/Toast";
import { useCountdown } from "../hooks/useCountdown";
import { ApiError, NetworkError, RateLimitError, verifyEmail, resendVerification } from "../api/client";

type ViewState = "verifying" | "success" | "alreadyVerified" | "invalid" | "expired" | "error" | "rateLimited";

export function VerifyEmail() {
  const [searchParams] = useSearchParams();
  const { addToast } = useToast();
  const countdown = useCountdown();

  const [state, setState] = useState<ViewState>("verifying");
  const [resendEmail, setResendEmail] = useState("");
  const [resending, setResending] = useState(false);

  const token = searchParams.get("token") ?? "";
  const email = searchParams.get("email") ?? "";

  useEffect(() => {
    if (!token || !email) {
      setState("invalid");
      return;
    }

    let cancelled = false;

    verifyEmail(email, token)
      .then(() => {
        if (!cancelled) setState("success");
      })
      .catch((err) => {
        if (cancelled) return;
        if (err instanceof ApiError) {
          const msg = err.message.toLowerCase();
          if (msg.includes("already verified")) setState("alreadyVerified");
          else if (msg.includes("expired")) setState("expired");
          else setState("invalid");
        } else {
          setState("error");
        }
      });

    return () => { cancelled = true; };
  }, [token, email]);

  const handleResend = async () => {
    if (resending || countdown.active) return;
    if (!resendEmail) {
      addToast("error", "Please enter your email address.");
      return;
    }

    setResending(true);
    try {
      await resendVerification(resendEmail);
      addToast("success", "Verification email sent. Check your inbox.");
      countdown.start(60);
    } catch (err) {
      if (err instanceof RateLimitError) {
        countdown.start(err.retryAfterSeconds);
        addToast("error", `Too many requests. Try again in ${err.retryAfterSeconds} seconds.`);
      } else if (err instanceof NetworkError) {
        addToast("error", "We couldn't send the email right now. Please try again later.");
      } else {
        addToast("success", "If an account exists for this email, a verification link has been sent.");
        countdown.start(60);
      }
    } finally {
      setResending(false);
    }
  };

  return (
    <div className="center-screen">
      <div className="card auth-form" style={{ textAlign: "center" }}>
        <h1 className="brand-sm">EventSphere</h1>

        {state === "verifying" && (
          <>
            <h2>Verifying your email...</h2>
            <p className="muted">Please wait while we verify your email address.</p>
          </>
        )}

        {state === "success" && (
          <>
            <h2>Email verified!</h2>
            <p className="muted" style={{ marginBottom: "1rem" }}>
              Your email has been verified. You can now use all features of EventSphere.
            </p>
            <a href="/login" className="btn btn-primary" style={{ display: "inline-block", textDecoration: "none" }}>
              Continue to Login
            </a>
          </>
        )}

        {state === "alreadyVerified" && (
          <>
            <h2>Already verified</h2>
            <p className="muted" style={{ marginBottom: "1rem" }}>
              Your email is already verified. You can sign in.
            </p>
            <a href="/login" className="btn btn-primary" style={{ display: "inline-block", textDecoration: "none" }}>
              Sign In
            </a>
          </>
        )}

        {state === "invalid" && (
          <>
            <h2>Invalid link</h2>
            <p className="muted" style={{ marginBottom: "1rem" }}>
              This verification link is invalid. Please request a new one.
            </p>
            <label htmlFor="resend-email">Email</label>
            <input
              id="resend-email"
              type="email"
              value={resendEmail}
              onChange={(e) => setResendEmail(e.target.value)}
              placeholder="Enter your email"
              disabled={resending || countdown.active}
            />
            <button
              className="btn btn-primary"
              onClick={handleResend}
              disabled={resending || countdown.active}
            >
              {resending
                ? "Sending..."
                : countdown.active
                  ? `Resend in ${countdown.seconds}s`
                  : "Resend Verification Email"}
            </button>
          </>
        )}

        {state === "expired" && (
          <>
            <h2>Link expired</h2>
            <p className="muted" style={{ marginBottom: "1rem" }}>
              This verification link has expired. Please request a new one.
            </p>
            <label htmlFor="resend-email">Email</label>
            <input
              id="resend-email"
              type="email"
              value={resendEmail}
              onChange={(e) => setResendEmail(e.target.value)}
              placeholder="Enter your email"
              disabled={resending || countdown.active}
            />
            <button
              className="btn btn-primary"
              onClick={handleResend}
              disabled={resending || countdown.active}
            >
              {resending
                ? "Sending..."
                : countdown.active
                  ? `Resend in ${countdown.seconds}s`
                  : "Resend Verification Email"}
            </button>
          </>
        )}

        {state === "error" && (
          <>
            <h2>Something went wrong</h2>
            <p className="muted" style={{ marginBottom: "1rem" }}>
              We couldn't verify your email right now. Please try again later.
            </p>
            <label htmlFor="resend-email">Email</label>
            <input
              id="resend-email"
              type="email"
              value={resendEmail}
              onChange={(e) => setResendEmail(e.target.value)}
              placeholder="Enter your email"
              disabled={resending || countdown.active}
            />
            <button
              className="btn btn-primary"
              onClick={handleResend}
              disabled={resending || countdown.active}
            >
              {resending
                ? "Sending..."
                : countdown.active
                  ? `Resend in ${countdown.seconds}s`
                  : "Resend Verification Email"}
            </button>
          </>
        )}

        <p className="switch">
          <a href="/login">Back to Sign In</a>
        </p>
      </div>
    </div>
  );
}
