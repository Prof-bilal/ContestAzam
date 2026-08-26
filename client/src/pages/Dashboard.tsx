import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { useToast } from "../components/Toast";
import { demo, type DemoResult } from "../api/client";

const AREAS: { key: string; label: string }[] = [
  { key: "visitor", label: "Visitor Area" },
  { key: "participant", label: "Participant Area" },
  { key: "organizer", label: "Organizer Area" },
  { key: "admin", label: "Admin Area" },
];

export function Dashboard() {
  const { user, logout } = useAuth();
  const { addToast } = useToast();
  const navigate = useNavigate();
  const [results, setResults] = useState<Record<string, DemoResult>>({});
  const [busy, setBusy] = useState<string | null>(null);

  const probe = async (area: string) => {
    setBusy(area);
    try {
      const result = await demo(area);
      setResults((prev) => ({ ...prev, [area]: result }));
    } finally {
      setBusy(null);
    }
  };

  const onLogout = async () => {
    await logout();
    addToast("info", "You have been signed out.");
    navigate("/");
  };

  const isAdmin = user?.roles.includes("Admin");
  const isOrganizer = user?.roles.includes("Organizer");
  const isParticipant = user?.roles.includes("Participant");

  return (
    <div className="dashboard">
      <header className="dash-header">
        <h1 className="brand-sm">EventSphere</h1>
        <div style={{ display: "flex", gap: "0.5rem" }}>
          <Link to="/events" className="btn btn-secondary" style={{ textDecoration: "none" }}>
            Events
          </Link>
          {isAdmin && (
            <Link to="/admin" className="btn btn-secondary" style={{ textDecoration: "none" }}>
              Admin
            </Link>
          )}
          <Link to="/profile" className="btn btn-secondary" style={{ textDecoration: "none" }}>
            Profile
          </Link>
          <button className="btn btn-secondary" onClick={onLogout}>
            Logout
          </button>
        </div>
      </header>

      <section className="card">
        <h2>Welcome, {user?.name}</h2>
        <p>
          Role:{" "}
          {(() => {
            const rolePriority = ["Admin", "Organizer", "Participant", "Visitor"];
            const highest = rolePriority.find((r) => user?.roles.includes(r));
            return highest ? (
              <span className="role-badge">{highest}</span>
            ) : null;
          })()}
        </p>
        <p className="muted">{user?.email}</p>
        {!isOrganizer && !isAdmin && (
          <Link
            to="/profile"
            className="btn btn-primary"
            style={{
              textDecoration: "none",
              display: "inline-flex",
              marginTop: "0.75rem",
              width: "auto",
            }}
          >
            Become an Organizer
          </Link>
        )}
      </section>

      {/* Quick Navigation */}
      <section className="card">
        <h3>Quick Links</h3>
        <div className="area-grid">
          <Link to="/events" className="area" style={{ textDecoration: "none" }}>
            <div className="area-title">Browse Events</div>
            <div className="muted">Discover & register</div>
          </Link>

          {(isParticipant || isOrganizer || isAdmin) && (
            <Link to="/my-registrations" className="area" style={{ textDecoration: "none" }}>
              <div className="area-title">My Registrations</div>
              <div className="muted">View your bookings</div>
            </Link>
          )}

          {isOrganizer && (
            <Link to="/organizer/events" className="area" style={{ textDecoration: "none" }}>
              <div className="area-title">Organizer Dashboard</div>
              <div className="muted">Create & manage events</div>
            </Link>
          )}

          {isAdmin && (
            <Link to="/admin/events" className="area" style={{ textDecoration: "none" }}>
              <div className="area-title">Admin Events</div>
              <div className="muted">Approve & manage</div>
            </Link>
          )}
        </div>
      </section>

      <section className="card">
        <h3>Role-protected areas</h3>
        <p className="muted">
          These call backend endpoints. Access is decided by the server — the
          result below reflects the real authorization outcome, not the UI.
        </p>
        <div className="area-grid">
          {AREAS.map((a) => {
            const r = results[a.key];
            return (
              <div key={a.key} className="area">
                <div className="area-title">{a.label}</div>
                <button
                  className="btn btn-small"
                  onClick={() => probe(a.key)}
                  disabled={busy === a.key}
                >
                  {busy === a.key ? "Checking…" : "Check access"}
                </button>
                {r && (
                  <div className={r.ok ? "area-result ok" : "area-result denied"}>
                    {r.ok ? "Allowed" : r.status === 403 ? "Forbidden (403)" : `Denied (${r.status})`}
                  </div>
                )}
              </div>
            );
          })}
        </div>
      </section>
    </div>
  );
}
