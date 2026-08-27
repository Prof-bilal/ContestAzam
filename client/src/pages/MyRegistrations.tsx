import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { useToast } from "../components/Toast";
import { LogoutButton } from "../components/LogoutButton";
import { getMyRegistrations, cancelMyRegistration } from "../api/client";
import type { RegistrationDto } from "../types";

export function MyRegistrations() {
  const { user } = useAuth();
  const { addToast } = useToast();
  const [registrations, setRegistrations] = useState<RegistrationDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getMyRegistrations()
      .then(setRegistrations)
      .catch(() => setRegistrations([]))
      .finally(() => setLoading(false));
  }, []);

  const handleCancel = async (reg: RegistrationDto) => {
    if (!confirm(`Cancel registration for "${reg.eventTitle}"?`)) return;
    try {
      await cancelMyRegistration(reg.id);
      addToast("success", "Registration cancelled.");
      setRegistrations((prev) => prev.map((r) => r.id === reg.id ? { ...r, status: "Cancelled" } : r));
    } catch {
      addToast("error", "Failed to cancel registration.");
    }
  };

  const highestRole = ["Admin", "Organizer", "Participant"].find((r) =>
    user?.roles.includes(r),
  ) || "Visitor";

  const isOrganizer = user?.roles.includes("Organizer");
  const isAdmin = user?.roles.includes("Admin");

  return (
    <div className="admin-layout">
      <aside className="admin-sidebar">
        <div className="admin-brand">EventSphere</div>
        <div className="sidebar-welcome">
          Welcome, <strong>{user?.name}</strong>
          <span className="role-badge" style={{ marginLeft: "0.5rem", fontSize: "11px" }}>
            {highestRole}
          </span>
        </div>
        <nav className="admin-nav">
          <Link to="/dashboard" className="admin-nav-item">Dashboard</Link>
          <Link to="/events" className="admin-nav-item">Browse Events</Link>
          <Link to="/my-registrations" className="admin-nav-item active">My Registrations</Link>
          {isOrganizer && (
            <>
              <Link to="/organizer/events" className="admin-nav-item">My Events</Link>
              <Link to="/organizer/categories" className="admin-nav-item">Categories</Link>
            </>
          )}
          {isAdmin && (
            <>
              <Link to="/admin/users" className="admin-nav-item">Users</Link>
              <Link to="/admin/events" className="admin-nav-item">Manage Events</Link>
              <Link to="/admin/organizer-requests" className="admin-nav-item">Organizer Requests</Link>
              <Link to="/admin/reviews" className="admin-nav-item">Reviews</Link>
              <Link to="/admin/announcements" className="admin-nav-item">Announcements</Link>
              <Link to="/admin/reports" className="admin-nav-item">Reports</Link>
            </>
          )}
          <Link to="/profile" className="admin-nav-item">Profile</Link>
        </nav>
        <LogoutButton style={{ marginTop: "auto" }} />
      </aside>

      <main className="admin-main">
        <div className="admin-header">
          <h1>My Registrations</h1>
          <Link to="/events" className="btn btn-secondary btn-small" style={{ width: "auto" }}>
            Browse Events
          </Link>
        </div>

        {loading ? (
          <div className="loading-state">Loading...</div>
        ) : registrations.length === 0 ? (
          <div className="empty-state">
            <p>You haven't registered for any events yet.</p>
            <Link to="/events" className="btn btn-small" style={{ width: "auto", marginTop: "0.5rem" }}>Browse Events</Link>
          </div>
        ) : (
          <div className="admin-table-wrapper">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Event</th>
                  <th>Date</th>
                  <th>Venue</th>
                  <th>Status</th>
                  <th>Registered</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {registrations.map((reg) => (
                  <tr key={reg.id}>
                    <td><Link to={`/events/${reg.eventId}`} style={{ color: "var(--deep-teal)" }}>{reg.eventTitle}</Link></td>
                    <td>{new Date(reg.eventDate).toLocaleDateString()}</td>
                    <td>{reg.eventVenue ?? "—"}</td>
                    <td><span className={`status-badge status-${reg.status.toLowerCase()}`}>{reg.status}</span></td>
                    <td>{new Date(reg.registeredOn).toLocaleDateString()}</td>
                    <td>
                      {reg.status === "Confirmed" && (
                        <div style={{ display: "flex", gap: "0.25rem" }}>
                          <Link to={`/my-registrations/${reg.id}/pass`} className="btn btn-primary btn-small" style={{ width: "auto", marginTop: 0 }}>
                            View Pass
                          </Link>
                          <button className="btn btn-danger btn-small" onClick={() => handleCancel(reg)}>Cancel</button>
                        </div>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </main>
    </div>
  );
}
