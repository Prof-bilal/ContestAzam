import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { useToast } from "../components/Toast";
import { getAdminDashboard } from "../api/client";
import type { AdminDashboardStats } from "../types";

export function AdminDashboard() {
  const { user, logout } = useAuth();
  const { addToast } = useToast();
  const navigate = useNavigate();
  const [stats, setStats] = useState<AdminDashboardStats | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    void loadStats();
  }, []);

  const loadStats = async () => {
    try {
      const s = await getAdminDashboard();
      setStats(s);
    } catch {
      addToast("error", "Unable to load admin dashboard.");
    } finally {
      setLoading(false);
    }
  };

  const onLogout = async () => {
    await logout();
    addToast("info", "You have been signed out.");
    navigate("/");
  };

  if (loading) {
    return <div className="center-screen">Loading dashboard...</div>;
  }

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
          <Link to="/admin" className="admin-nav-item active">Dashboard</Link>
          <Link to="/admin/users" className="admin-nav-item">Users</Link>
          <Link to="/admin/events" className="admin-nav-item">Events</Link>
          <Link to="/admin/organizer-requests" className="admin-nav-item">Organizer Requests</Link>
          <Link to="/admin/reviews" className="admin-nav-item">Reviews</Link>
          <Link to="/admin/announcements" className="admin-nav-item">Announcements</Link>
          <Link to="/admin/reports" className="admin-nav-item">Reports</Link>
          <Link to="/dashboard" className="admin-nav-item">Main App</Link>
        </nav>
        <button className="btn btn-secondary" onClick={onLogout} style={{ marginTop: "auto" }}>
          Logout
        </button>
      </aside>
      <main className="admin-main">
        <header className="admin-header">
          <h1>Admin Dashboard</h1>
          <p className="muted">Welcome, {user?.name}</p>
        </header>

        {stats && (
          <div className="admin-stats">
            <div className="stat-card">
              <div className="stat-number">{stats.totalUsers}</div>
              <div className="stat-label">Total Users</div>
            </div>
            <div className="stat-card">
              <div className="stat-number" style={{ color: stats.pendingRequests > 0 ? "var(--butter-yellow)" : undefined }}>
                {stats.pendingRequests}
              </div>
              <div className="stat-label">Pending Requests</div>
            </div>
            <div className="stat-card">
              <div className="stat-number" style={{ color: "var(--deep-teal)" }}>
                {stats.approvedOrganizers}
              </div>
              <div className="stat-label">Approved Organizers</div>
            </div>
            <div className="stat-card">
              <div className="stat-number">{stats.totalEvents}</div>
              <div className="stat-label">Total Events</div>
            </div>
          </div>
        )}

        <section className="card" style={{ maxWidth: "none" }}>
          <h3>Quick Actions</h3>
          <Link
            to="/admin/organizer-requests"
            className="btn btn-primary"
            style={{ textDecoration: "none", display: "inline-flex", marginTop: "0.5rem" }}
          >
            Review Organizer Requests
          </Link>
        </section>
      </main>
    </div>
  );
}
