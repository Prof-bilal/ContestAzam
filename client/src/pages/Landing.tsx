import { Link } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

export function Landing() {
  const { user, status } = useAuth();
  const isAuthenticated = status === "authenticated";
  const isAdmin = user?.roles.includes("Admin");
  const isOrganizer = user?.roles.includes("Organizer");

  return (
    <div className="center-screen">
      <div className="landing card">
        <h1 className="brand">EventSphere</h1>
        <p className="tagline">Event management platform</p>

        {isAuthenticated ? (
          <div style={{ textAlign: "center" }}>
            <p style={{ marginBottom: "1rem", fontSize: "1.1rem" }}>
              Welcome back, <strong>{user?.name}</strong>
            </p>
            <div style={{ display: "flex", flexDirection: "column", gap: "0.5rem", alignItems: "center" }}>
              <Link className="btn btn-primary" to="/events" style={{ width: "auto" }}>
                Browse Events
              </Link>
              <Link className="btn btn-secondary" to="/dashboard" style={{ width: "auto" }}>
                Dashboard
              </Link>
              {isOrganizer && (
                <Link className="btn btn-secondary" to="/organizer/events" style={{ width: "auto" }}>
                  My Events
                </Link>
              )}
              {isAdmin && (
                <Link className="btn btn-secondary" to="/admin/events" style={{ width: "auto" }}>
                  Admin Events
                </Link>
              )}
            </div>
          </div>
        ) : (
          <div style={{ display: "flex", flexDirection: "column", gap: "0.75rem", alignItems: "center" }}>
            <Link className="btn btn-primary" to="/events" style={{ width: "auto" }}>
              Browse Events
            </Link>
            <div className="row">
              <Link className="btn btn-secondary" to="/login">
                Login
              </Link>
              <Link className="btn btn-secondary" to="/register">
                Create Account
              </Link>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
