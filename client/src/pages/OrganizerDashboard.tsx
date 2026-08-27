import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { LogoutButton } from "../components/LogoutButton";
import { useToast } from "../components/Toast";
import { getOrganizerStats, getOrganizerEvents, cancelEvent, deleteEvent } from "../api/client";
import type { OrganizerEventStats, EventSummary } from "../types";

export function OrganizerDashboard() {
  const { user } = useAuth();
  const { addToast } = useToast();
  const [stats, setStats] = useState<OrganizerEventStats | null>(null);
  const [events, setEvents] = useState<EventSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [statusFilter, setStatusFilter] = useState("");

  useEffect(() => {
    setLoading(true);
    Promise.all([
      getOrganizerStats().catch(() => null),
      getOrganizerEvents({ status: statusFilter || undefined, pageSize: 50 }).catch(() => []),
    ])
      .then(([s, e]) => { setStats(s); setEvents(e); })
      .finally(() => setLoading(false));
  }, [statusFilter]);

  const handleCancel = async (id: number) => {
    if (!confirm("Cancel this event?")) return;
    try {
      await cancelEvent(id);
      addToast("success", "Event cancelled.");
      setEvents((prev) => prev.map((e) => e.id === id ? { ...e, status: "Cancelled" } : e));
    } catch { addToast("error", "Failed to cancel event."); }
  };

  const handleDelete = async (id: number) => {
    if (!confirm("Delete this draft event? This cannot be undone.")) return;
    try {
      await deleteEvent(id);
      addToast("success", "Event deleted.");
      setEvents((prev) => prev.filter((e) => e.id !== id));
    } catch { addToast("error", "Failed to delete event."); }
  };

  const highestRole = ["Admin", "Organizer", "Participant"].find((r) =>
    user?.roles.includes(r),
  ) || "Visitor";

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
          <Link to="/organizer/events" className="admin-nav-item active">My Events</Link>
          <Link to="/organizer/categories" className="admin-nav-item">Categories</Link>
          <Link to="/events" className="admin-nav-item">Browse Events</Link>
          <Link to="/profile" className="admin-nav-item">Profile</Link>
        </nav>
        <LogoutButton style={{ marginTop: "auto" }} />
      </aside>
      <main className="admin-main">
        <div className="admin-header">
          <h1>Organizer Dashboard</h1>
          <Link to="/organizer/events/create" className="btn btn-small" style={{ width: "auto", marginTop: "0.5rem" }}>
            + Create Event
          </Link>
        </div>

        {stats && (
          <div className="admin-stats">
            <div className="stat-card"><div className="stat-number">{stats.totalEvents}</div><div className="stat-label">Total Events</div></div>
            <div className="stat-card"><div className="stat-number">{stats.draftEvents}</div><div className="stat-label">Drafts</div></div>
            <div className="stat-card"><div className="stat-number">{stats.pendingEvents}</div><div className="stat-label">Pending</div></div>
            <div className="stat-card"><div className="stat-number">{stats.approvedEvents}</div><div className="stat-label">Approved</div></div>
            <div className="stat-card"><div className="stat-number">{stats.totalRegistrations}</div><div className="stat-label">Registrations</div></div>
          </div>
        )}

        <div className="filter-bar">
          {["", "Draft", "PendingApproval", "Approved", "Rejected", "Cancelled"].map((s) => (
            <button
              key={s}
              className={`btn btn-small ${statusFilter === s ? "" : "btn-secondary"}`}
              onClick={() => setStatusFilter(s)}
            >
              {s || "All"}
            </button>
          ))}
        </div>

        {loading ? (
          <div className="loading-state">Loading...</div>
        ) : events.length === 0 ? (
          <div className="empty-state"><p>No events found.</p></div>
        ) : (
          <div className="admin-table-wrapper">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Event</th>
                  <th>Date</th>
                  <th>Status</th>
                  <th>Registered</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {events.map((evt) => (
                  <tr key={evt.id}>
                    <td>
                      <Link to={`/events/${evt.id}`} style={{ color: "var(--deep-teal)" }}>{evt.title}</Link>
                      <div className="muted">{evt.categoryName}</div>
                    </td>
                    <td>{new Date(evt.eventDate).toLocaleDateString()}</td>
                    <td><span className={`status-badge status-${evt.status.toLowerCase()}`}>{evt.status}</span></td>
                    <td>{evt.registeredCount}/{evt.maxParticipants}</td>
                    <td>
                      <div style={{ display: "flex", gap: "0.35rem", flexWrap: "wrap" }}>
                        {(evt.status === "Draft" || evt.status === "PendingApproval" || evt.status === "Rejected") && (
                          <Link to={`/organizer/events/${evt.id}/edit`} className="btn btn-secondary btn-small">{evt.status === "Rejected" ? "Fix & Resubmit" : "Edit"}</Link>
                        )}
                        {evt.status === "Draft" && (
                          <Link to={`/organizer/events/${evt.id}/attendees`} className="btn btn-secondary btn-small">Attendees</Link>
                        )}
                        {evt.status === "Approved" && (
                          <Link to={`/organizer/events/${evt.id}/attendees`} className="btn btn-secondary btn-small">Attendees</Link>
                        )}
                        {evt.status !== "Cancelled" && evt.status !== "Completed" && evt.status !== "Rejected" && (
                          <button className="btn btn-secondary btn-small" onClick={() => handleCancel(evt.id)}>Cancel</button>
                        )}
                        {evt.status === "Draft" && (
                          <button className="btn btn-danger btn-small" onClick={() => handleDelete(evt.id)}>Delete</button>
                        )}
                      </div>
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
