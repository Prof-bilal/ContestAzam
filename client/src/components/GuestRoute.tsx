import type { ReactNode } from "react";
import { Navigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

/// Redirects authenticated users away (e.g. to dashboard). Used for
/// login/register pages that should not be accessible when already signed in.
export function GuestRoute({ children }: { children: ReactNode }) {
  const { status } = useAuth();

  if (status === "loading") {
    return <div className="center-screen">Loading…</div>;
  }
  if (status === "authenticated") {
    return <Navigate to="/dashboard" replace />;
  }
  return <>{children}</>;
}
