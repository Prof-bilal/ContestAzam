import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { LogoutButton } from "../components/LogoutButton";
import { useToast } from "../components/Toast";
import { getAdminEvents, approveEvent, rejectEvent } from "../api/client";
import type { AdminEventDto } from "../types";

export function AdminEvents() {
  const { user } = useAuth();
  const { addToast } = useToast();
  const [events, setEvents] = useState<AdminEventDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [statusFilter, setStatusFilter] = useState("");
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [rejectModal, setRejectModal] = useState<number | null>(null);
  const [rejectReason, setRejectReason] = useState("");

  const fetchEvents = () => {
    setLoading(true);
    getAdminEvents({ status: statusFilter || undefined, page, pageSize: 15 })
      .then((res) => { setEvents(res.events); setTotalPages(res.totalPages); })
      .catch(() => setEvents([]))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchEvents(); }, [statusFilter, page]);

  const handleApprove = async (id: number) => {
    try {
      await approveEvent(id);
      addToast("success", "Event approved.");
      fetchEvents();
    } catch { addToast("error", "Failed to approve event."); }
  };

  const handleReject = async () => {
    if (!rejectModal) return;
    try {
      await rejectEvent(rejectModal, rejectReason || undefined);
      addToast("success", "Event rejected.");
      setRejectModal(null);
      setRejectReason("");
      fetchEvents();
    } catch { addToast("error", "Failed to reject event."); }
  };

  return (
    <div className="admin-layout">
      <aside className="admin-sidebar">
        <div className="admin-brand">EventSphere</div>
        <div className="sidebar-welcome">
          Welcome, <strong>{user?.name}</strong>
          <span className="role-badge" style={{ marginLeft: "0.5rem", fontSize: "11px" }}>Admin</span>
        </div>
        <nav className="admin-nav">
          <Link to="/admin" className="admin-nav-item">Dashboard</Link>
          <Link to="/admin/users" className="admin-nav-item">Users</Link>
          <Link to="/admin/events" className="admin-nav-item active">Events</Link>
          <Link to="/admin/organizer-requests" className="admin-nav-item">Organizer Requests</Link>
          <Link to="/admin/reviews" className="admin-nav-item">Reviews</Link>
          <Link to="/admin/announcements" className="admin-nav-item">Announcements</Link>
          <Link to="/admin/reports" className="admin-nav-item">Reports</Link>
        </nav>
        <LogoutButton style={{ marginTop: "auto" }} />
      </aside>
      <main className="admin-main">
        <div className="admin-header">
          <h1>Event Management</h1>
        </div>

        <div className="filter-bar">
          {["", "PendingApproval", "Approved", "Rejected", "Cancelled"].map((s) => (
            <button
              key={s}
              className={`btn btn-small ${statusFilter === s ? "" : "btn-secondary"}`}
              onClick={() => { setStatusFilter(s); setPage(1); }}
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
          <>
            <div className="admin-table-wrapper">
              <table className="admin-table">
                <thead>
                  <tr>
                    <th>Event</th>
                    <th>Organizer</th>
                    <th>Date</th>
                    <th>Category</th>
                    <th>Slots</th>
                    <th>Status</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {events.map((evt) => (
                    <tr key={evt.id}>
                      <td><Link to={`/events/${evt.id}`} style={{ color: "var(--deep-teal)" }}>{evt.title}</Link></td>
                      <td>
                        <div>{evt.organizerName}</div>
                        <div className="muted">{evt.organizerEmail}</div>
                      </td>
                      <td>{new Date(evt.eventDate).toLocaleDateString()}</td>
                      <td>{evt.categoryName}</td>
                      <td>{evt.registeredCount}/{evt.maxParticipants}</td>
                      <td><span className={`status-badge status-${evt.status.toLowerCase()}`}>{evt.status}</span></td>
                      <td>
                        {evt.status === "PendingApproval" && (
                          <div style={{ display: "flex", gap: "0.35rem" }}>
                            <button className="btn btn-small" onClick={() => handleApprove(evt.id)} style={{ width: "auto", marginTop: 0, background: "var(--deep-teal)" }}>Approve</button>
                            <button className="btn btn-danger btn-small" onClick={() => setRejectModal(evt.id)}>Reject</button>
                          </div>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {totalPages > 1 && (
              <div className="pagination">
                <button className="btn btn-secondary btn-small" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>Previous</button>
                <span className="muted">Page {page} of {totalPages}</span>
                <button className="btn btn-secondary btn-small" disabled={page >= totalPages} onClick={() => setPage((p) => p + 1)}>Next</button>
              </div>
            )}
          </>
        )}
      </main>

      {rejectModal && (
        <div className="modal-overlay" onClick={() => setRejectModal(null)}>
          <div className="modal card" onClick={(e) => e.stopPropagation()}>
            <h3>Reject Event</h3>
            <label className="muted">Reason (optional)</label>
            <textarea
              value={rejectReason}
              onChange={(e) => setRejectReason(e.target.value)}
              rows={3}
              placeholder="Provide a reason for rejection..."
            />
            <div style={{ display: "flex", gap: "0.5rem", marginTop: "1rem" }}>
              <button className="btn btn-danger btn-small" onClick={handleReject}>Reject</button>
              <button className="btn btn-secondary btn-small" onClick={() => setRejectModal(null)}>Cancel</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
