import { useEffect, useRef } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { useToast } from "../components/Toast";

const ERROR_MESSAGES: Record<string, string> = {
  oauth_failed: "External authentication failed. Please try again.",
  account_exists:
    "An account with this email already exists. Sign in with your password, then link the provider.",
  email_required: "Your provider did not share an email address. Unable to continue.",
  email_unverified: "Your provider email is not verified. Unable to continue.",
  provider_unavailable: "That sign-in provider is not available.",
  account_disabled: "This account is disabled.",
  account_suspended: "This account has been suspended by an administrator.",
};

/// Where the backend redirects after OAuth. On success the refresh cookie is
/// already set; we complete the login by restoring the session, then route on.
/// If the backend redirects to /oauth/complete with a pending token, the user
/// is new and needs to choose an account type.
export function OAuthCallback() {
  const [params] = useSearchParams();
  const { restoreSession } = useAuth();
  const { addToast } = useToast();
  const navigate = useNavigate();
  const handled = useRef(false);

  useEffect(() => {
    if (handled.current) return;
    handled.current = true;

    const error = params.get("error");
    if (error) {
      addToast("error", ERROR_MESSAGES[error] ?? "External authentication failed.");
      if (error === "account_suspended") {
        const reason = params.get("reason") || "";
        navigate(`/suspended${reason ? `?reason=${encodeURIComponent(reason)}` : ""}`, { replace: true });
      } else {
        navigate("/login", { replace: true });
      }
      return;
    }

    // If there's a pending token, redirect to the OAuth complete page.
    const pending = params.get("pending");
    if (pending) {
      navigate(`/oauth/complete?pending=${encodeURIComponent(pending)}`, { replace: true });
      return;
    }

    void (async () => {
      const ok = await restoreSession();
      if (ok) {
        addToast("success", "Signed in.");
        navigate("/dashboard", { replace: true });
      } else {
        addToast("error", "Could not complete sign-in. Please try again.");
        navigate("/login", { replace: true });
      }
    })();
  }, [params, restoreSession, addToast, navigate]);

  return <div className="center-screen">Completing sign-in…</div>;
}
