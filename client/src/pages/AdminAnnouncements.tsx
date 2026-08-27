import { useState } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { LogoutButton } from "../components/LogoutButton";
import { useToast } from "../components/Toast";
import { sendAnnouncement } from "../api/client";

export function AdminAnnouncements() {
  const { user } = useAuth();
  const { addToast } = useToast();
  const [title, setTitle] = useState("");
  const [message, setMessage] = useState("");
  const [sending, setSending] = useState(false);

  const handleSend = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!title.trim()) return;
    setSending(true);
    try {
      await sendAnnouncement(title.trim(), message.trim() || undefined);
      addToast("success", "Announcement sent to all users!");
      setTitle("");
      setMessage("");
    } catch {
      addToast("error", "Failed to send announcement.");
    } finally {
      setSending(false);
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
          <Link to="/admin/reviews" className="admin-nav-item">Reviews</Link>
          <Link to="/admin/announcements" className="admin-nav-item active">Announcements</Link>
          <Link to="/admin/reports" className="admin-nav-item">Reports</Link>
        </nav>
        <LogoutButton style={{ marginTop: "auto" }} />
      </aside>
      <main className="admin-main">
        <div className="admin-header">
          <h1 style={{ margin: 0, fontSize: "1.5rem" }}>Send Announcement</h1>
          <p className="muted">Broadcast a message to all active users</p>
        </div>
        <form onSubmit={handleSend} className="card" style={{ padding: "1.5rem", maxWidth: 600 }}>
          <label htmlFor="ann-title">Title</label>
          <input id="ann-title" value={title} onChange={(e) => setTitle(e.target.value)}
            placeholder="Announcement title" disabled={sending} />
          <label htmlFor="ann-msg" style={{ marginTop: "1rem" }}>Message (optional)</label>
          <textarea id="ann-msg" value={message} onChange={(e) => setMessage(e.target.value)}
            rows={4} placeholder="Write your announcement..." disabled={sending}
            style={{ resize: "vertical" }} />
          <button className="btn btn-primary btn-small" type="submit" disabled={sending || !title.trim()}
            style={{ width: "auto", marginTop: "1rem" }}>
            {sending ? "Sending..." : "Send to All Users"}
          </button>
        </form>
      </main>
    </div>
  );
}
