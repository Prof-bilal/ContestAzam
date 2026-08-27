import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { LogoutButton } from "../components/LogoutButton";
import { getMyCertificates } from "../api/client";
import type { Certificate } from "../types";

export function Certificates() {
  const { user } = useAuth();
  const [certs, setCerts] = useState<Certificate[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getMyCertificates()
      .then(setCerts)
      .catch(() => {})
      .finally(() => setLoading(false));
  }, []);

  const highestRole = ["Admin", "Organizer", "Participant"].find((r) =>
    user?.roles.includes(r),
  ) || "Visitor";

  if (loading) return <div className="loading-state">Loading certificates...</div>;

  return (
    <div className="admin-layout">
      <aside className="admin-sidebar">
        <div className="admin-brand">EventSphere</div>
        <div className="sidebar-welcome">
          Welcome, <strong>{user?.name}</strong>
          <span className="role-badge" style={{ marginLeft: "0.5rem", fontSize: "11px" }}>{highestRole}</span>
        </div>
        <nav className="admin-nav">
          <Link to="/dashboard" className="admin-nav-item">Dashboard</Link>
          <Link to="/my-registrations" className="admin-nav-item">My Registrations</Link>
          <Link to="/certificates" className="admin-nav-item active">Certificates</Link>
        </nav>
        <LogoutButton style={{ marginTop: "auto" }} />
      </aside>
      <main className="admin-main">
        <div className="admin-header">
          <h1 style={{ margin: 0, fontSize: "1.5rem" }}>My Certificates</h1>
          <p className="muted">{certs.length} certificate{certs.length !== 1 ? "s" : ""}</p>
        </div>

        {certs.length === 0 ? (
          <div className="empty-state">
            <p>No certificates yet. Attend events to earn certificates!</p>
            <Link to="/events" className="btn btn-primary btn-small" style={{ width: "auto" }}>Browse Events</Link>
          </div>
        ) : (
          <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(300px, 1fr))", gap: "1rem" }}>
            {certs.map((c) => (
              <div key={c.id} className="card" style={{ padding: "1.25rem" }}>
                <h3 style={{ margin: "0 0 0.5rem", fontSize: "1rem" }}>{c.eventTitle}</h3>
                <p className="muted" style={{ margin: "0 0 0.5rem" }}>Issued: {new Date(c.issuedOn).toLocaleDateString()}</p>
                <p className="muted" style={{ margin: "0 0 1rem" }}>Fee paid: {c.feePaid ? "Yes" : "No"}</p>
                <a href={c.certificateUrl} target="_blank" rel="noreferrer"
                  className="btn btn-primary btn-small" style={{ width: "auto" }}>
                  Download Certificate
                </a>
              </div>
            ))}
          </div>
        )}
      </main>
    </div>
  );
}
