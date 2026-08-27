import type { ReactNode } from "react";
import { Navigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

/// Route guard for UX only. The backend independently enforces authorization on
/// every request; hiding a route here is never the security boundary.
export function ProtectedRoute({ children }: { children: ReactNode }) {
  const { status } = useAuth();

  if (status === "loading") {
    return <div className="center-screen">Loading…</div>;
  }
  if (status === "suspended") {
    return <Navigate to="/suspended" replace />;
  }
  if (status !== "authenticated") {
    return <Navigate to="/login" replace />;
  }
  return <>{children}</>;
}
