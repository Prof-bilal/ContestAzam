import { Link } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { LogoutButton } from "../components/LogoutButton";

export function AdminReports() {
  const { user } = useAuth();

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
          <Link to="/admin/announcements" className="admin-nav-item">Announcements</Link>
          <Link to="/admin/reports" className="admin-nav-item active">Reports</Link>
        </nav>
        <LogoutButton style={{ marginTop: "auto" }} />
      </aside>
      <main className="admin-main">
        <div className="admin-header">
          <h1 style={{ margin: 0, fontSize: "1.5rem" }}>Reports & Export</h1>
        </div>
        <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(280px, 1fr))", gap: "1rem" }}>
          <div className="card" style={{ padding: "1.5rem" }}>
            <h3 style={{ margin: "0 0 0.5rem" }}>Participation Report</h3>
            <p className="muted" style={{ margin: "0 0 1rem" }}>Event registrations, capacity, and attendance overview</p>
            <a href="/api/admin/reports/participation" target="_blank" rel="noreferrer"
              className="btn btn-primary btn-small" style={{ width: "auto" }}>
              Download CSV
            </a>
          </div>
          <div className="card" style={{ padding: "1.5rem" }}>
            <h3 style={{ margin: "0 0 0.5rem" }}>User Report</h3>
            <p className="muted" style={{ margin: "0 0 1rem" }}>All active users with roles and join dates</p>
            <a href="/api/admin/reports/users" target="_blank" rel="noreferrer"
              className="btn btn-primary btn-small" style={{ width: "auto" }}>
              Download CSV
            </a>
          </div>
        </div>
      </main>
    </div>
  );
}
