import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { LogoutButton } from "../components/LogoutButton";
import { useToast } from "../components/Toast";
import { getAdminReviews, deleteAdminReview } from "../api/client";
import type { AdminReview } from "../types";

export function AdminReviews() {
  const { user } = useAuth();
  const { addToast } = useToast();
  const [reviews, setReviews] = useState<AdminReview[]>([]);
  const [loading, setLoading] = useState(true);
  const [deletingId, setDeletingId] = useState<number | null>(null);

  useEffect(() => {
    getAdminReviews()
      .then((res) => setReviews(res.reviews))
      .catch(() => {})
      .finally(() => setLoading(false));
  }, []);

  const handleDelete = async (id: number) => {
    setDeletingId(id);
    try {
      await deleteAdminReview(id);
      setReviews((prev) => prev.filter((r) => r.id !== id));
      addToast("success", "Review deleted.");
    } catch {
      addToast("error", "Failed to delete review.");
    } finally {
      setDeletingId(null);
    }
  };

  return (
    <div className="admin-layout">
      <aside className="admin-sidebar">
        <div className="admin-brand">EventSphere</div>
        <div className="sidebar-welcome">Welcome, <strong>{user?.name}</strong></div>
        <nav className="admin-nav">
          <Link to="/admin" className="admin-nav-item">Dashboard</Link>
          <Link to="/admin/users" className="admin-nav-item">Users</Link>
          <Link to="/admin/events" className="admin-nav-item">Events</Link>
          <Link to="/admin/organizer-requests" className="admin-nav-item">Organizer Requests</Link>
          <Link to="/admin/reviews" className="admin-nav-item active">Reviews</Link>
          <Link to="/admin/announcements" className="admin-nav-item">Announcements</Link>
          <Link to="/admin/reports" className="admin-nav-item">Reports</Link>
        </nav>
        <LogoutButton style={{ marginTop: "auto" }} />
      </aside>
      <main className="admin-main">
        <div className="admin-header">
          <h1 style={{ margin: 0, fontSize: "1.5rem" }}>Review Moderation</h1>
          <p className="muted">{reviews.length} reviews</p>
        </div>

        {loading ? <div className="loading-state">Loading...</div> : reviews.length === 0 ? (
          <div className="empty-state"><p>No reviews to moderate.</p></div>
        ) : (
          <div className="admin-table-wrapper">
            <table className="admin-table">
              <thead>
                <tr><th>User</th><th>Event</th><th>Rating</th><th>Comment</th><th>Date</th><th>Action</th></tr>
              </thead>
              <tbody>
                {reviews.map((r) => (
                  <tr key={r.id}>
                    <td>{r.userName}</td>
                    <td><Link to={`/events/${r.eventId}`}>{r.eventTitle}</Link></td>
                    <td>{"★".repeat(r.rating)}{"☆".repeat(5 - r.rating)}</td>
                    <td style={{ maxWidth: 300, overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>{r.comment || "—"}</td>
                    <td>{new Date(r.submittedOn).toLocaleDateString()}</td>
                    <td>
                      <button className="btn btn-small" disabled={deletingId === r.id}
                        onClick={() => handleDelete(r.id)} style={{ width: "auto", marginTop: 0 }}>
                        {deletingId === r.id ? "..." : "Delete"}
                      </button>
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
