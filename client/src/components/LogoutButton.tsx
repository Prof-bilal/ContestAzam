import { useState } from "react";
import type { CSSProperties } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { useToast } from "./Toast";

interface LogoutButtonProps {
  className?: string;
  style?: CSSProperties;
}

/**
 * Shared logout control for authenticated layouts.
 *
 * Behavior:
 *  - calls the auth context logout (revokes the refresh cookie server-side and
 *    clears the in-memory access token)
 *  - RealtimeProvider sees the status change and disconnects SignalR + clears
 *    notification/messaging state automatically
 *  - shows a toast and redirects home
 *  - disabled while the request is in flight ("Logging out…") to prevent
 *    duplicate submissions
 */
export function LogoutButton({ className = "btn btn-secondary", style }: LogoutButtonProps) {
  const { logout } = useAuth();
  const { addToast } = useToast();
  const navigate = useNavigate();
  const [busy, setBusy] = useState(false);

  const onLogout = async () => {
    if (busy) return; // prevent duplicate requests
    setBusy(true);
    try {
      await logout();
      addToast("info", "You have been signed out.");
      navigate("/");
    } catch {
      addToast("error", "Sign out failed. Please try again.");
      setBusy(false);
    }
  };

  return (
    <button
      type="button"
      className={className}
      onClick={onLogout}
      disabled={busy}
      aria-busy={busy}
      style={style}
    >
      {busy ? "Logging out…" : "Logout"}
    </button>
  );
}