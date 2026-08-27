import { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { useToast } from "../components/Toast";
import {
  getMyRegistrations,
  getOrganizerStats,
  getOrganizerEvents,
  getAdminDashboard,
  getAdminOrganizerRequests,
} from "../api/client";
import { NotificationBell } from "../components/NotificationBell";
import { LogoutButton } from "../components/LogoutButton";
import type {
  RegistrationDto,
  OrganizerEventStats,
  EventSummary,
  AdminDashboardStats,
  AdminOrganizerRequest,
} from "../types";

export function Dashboard() {
  const { user } = useAuth();
  const { addToast } = useToast();

  const isAdmin = user?.roles.includes("Admin");
  const isOrganizer = user?.roles.includes("Organizer");
  const isParticipant = user?.roles.includes("Participant");

  const [registrations, setRegistrations] = useState<RegistrationDto[]>([]);
  const [orgStats, setOrgStats] = useState<OrganizerEventStats | null>(null);
  const [orgEvents, setOrgEvents] = useState<EventSummary[]>([]);
  const [adminStats, setAdminStats] = useState<AdminDashboardStats | null>(null);
  const [pendingRequests, setPendingRequests] = useState<AdminOrganizerRequest[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const load = async () => {
      try {
        if (isParticipant || isOrganizer || isAdmin) {
          const regs = await getMyRegistrations();
          setRegistrations(regs);
        }
        if (isOrganizer || isAdmin) {
          const stats = await getOrganizerStats();
          setOrgStats(stats);
          const events = await getOrganizerEvents({ pageSize: 5 });
          setOrgEvents(events);
        }
        if (isAdmin) {
          const stats = await getAdminDashboard();
          setAdminStats(stats);
          const reqs = await getAdminOrganizerRequests("Pending");
          setPendingRequests(reqs);
        }
      } catch {
        addToast("error", "Failed to load dashboard data.");
      } finally {
        setLoading(false);
      }
    };
    load();
  }, [isAdmin, isOrganizer, isParticipant, addToast]);

  const upcomingRegistrations = registrations
    .filter((r) => new Date(r.eventDate) >= new Date() && r.status === "Confirmed")
    .slice(0, 5);

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
          <Link to="/dashboard" className="admin-nav-item active">Dashboard</Link>
          <Link to="/events" className="admin-nav-item">Browse Events</Link>
          <Link to="/calendar" className="admin-nav-item">Calendar</Link>
          {isParticipant && !isOrganizer && !isAdmin && (
            <Link to="/my-registrations" className="admin-nav-item">My Registrations</Link>
          )}
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
          <Link to="/notifications" className="admin-nav-item">Notifications</Link>
          <Link to="/messages" className="admin-nav-item">Messages</Link>
        </nav>
        <LogoutButton style={{ marginTop: "auto" }} />
      </aside>

      <main className="admin-main">
        <header className="admin-header notif-page-header">
          <h1>Dashboard</h1>
          <NotificationBell />
        </header>

        {loading && <div className="loading-state">Loading dashboard…</div>}

        {/* ───── Admin Stats ───── */}
        {!loading && isAdmin && adminStats && (
          <div className="admin-stats">
            <div className="stat-card">
              <div className="stat-number">{adminStats.totalUsers}</div>
              <div className="stat-label">Total Users</div>
            </div>
            <div className="stat-card">
              <div className="stat-number" style={{ color: adminStats.pendingRequests > 0 ? "var(--butter-yellow)" : undefined }}>
                {adminStats.pendingRequests}
              </div>
              <div className="stat-label">Pending Requests</div>
            </div>
            <div className="stat-card">
              <div className="stat-number" style={{ color: "var(--deep-teal)" }}>
                {adminStats.approvedOrganizers}
              </div>
              <div className="stat-label">Approved Organizers</div>
            </div>
            <div className="stat-card">
              <div className="stat-number">{adminStats.totalEvents}</div>
              <div className="stat-label">Total Events</div>
            </div>
          </div>
        )}

        {/* ───── Organizer Stats ───── */}
        {!loading && (isOrganizer || isAdmin) && orgStats && (
          <>
            {!isAdmin && (
              <div className="admin-stats">
                <div className="stat-card">
                  <div className="stat-number">{orgStats.totalEvents}</div>
                  <div className="stat-label">Total Events</div>
                </div>
                <div className="stat-card">
                  <div className="stat-number">{orgStats.draftEvents}</div>
                  <div className="stat-label">Drafts</div>
                </div>
                <div className="stat-card">
                  <div className="stat-number">{orgStats.pendingEvents}</div>
                  <div className="stat-label">Pending</div>
                </div>
                <div className="stat-card">
                  <div className="stat-number" style={{ color: "var(--deep-teal)" }}>
                    {orgStats.approvedEvents}
                  </div>
                  <div className="stat-label">Approved</div>
                </div>
                <div className="stat-card">
                  <div className="stat-number">{orgStats.totalRegistrations}</div>
                  <div className="stat-label">Registrations</div>
                </div>
              </div>
            )}
            {isAdmin && (
              <section className="card" style={{ maxWidth: "none" }}>
                <h3>Organizer Stats</h3>
                <div className="admin-stats" style={{ marginBottom: 0 }}>
                  <div className="stat-card">
                    <div className="stat-number">{orgStats.totalEvents}</div>
                    <div className="stat-label">Total Events</div>
                  </div>
                  <div className="stat-card">
                    <div className="stat-number">{orgStats.pendingEvents}</div>
                    <div className="stat-label">Pending</div>
                  </div>
                  <div className="stat-card">
                    <div className="stat-number" style={{ color: "var(--deep-teal)" }}>
                      {orgStats.approvedEvents}
                    </div>
                    <div className="stat-label">Approved</div>
                  </div>
                  <div className="stat-card">
                    <div className="stat-number">{orgStats.totalRegistrations}</div>
                    <div className="stat-label">Registrations</div>
                  </div>
                </div>
              </section>
            )}
          </>
        )}

        {/* ───── Upcoming Registrations ───── */}
        {!loading && (isParticipant || isOrganizer || isAdmin) && (
          <section className="card" style={{ maxWidth: "none" }}>
            <h3>Upcoming Events</h3>
            {upcomingRegistrations.length === 0 ? (
              <p className="muted">No upcoming registrations.</p>
            ) : (
              <div className="admin-table-wrapper">
                <table className="admin-table">
                  <thead>
                    <tr>
                      <th>Event</th>
                      <th>Date</th>
                      <th>Venue</th>
                      <th>Status</th>
                    </tr>
                  </thead>
                  <tbody>
                    {upcomingRegistrations.map((r) => (
                      <tr key={r.id}>
                        <td>
                          <Link to={`/events/${r.eventId}`} style={{ color: "var(--deep-teal)" }}>
                            {r.eventTitle}
                          </Link>
                        </td>
                        <td>{new Date(r.eventDate).toLocaleDateString()}</td>
                        <td>{r.eventVenue || "—"}</td>
                        <td>
                          <span className="status-badge status-approved">{r.status}</span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </section>
        )}

        {/* ───── Admin: Pending Organizer Requests ───── */}
        {!loading && isAdmin && pendingRequests.length > 0 && (
          <section className="card" style={{ maxWidth: "none" }}>
            <h3>Pending Organizer Requests</h3>
            <div className="admin-table-wrapper">
              <table className="admin-table">
                <thead>
                  <tr>
                    <th>User</th>
                    <th>Organization</th>
                    <th>Submitted</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {pendingRequests.map((req) => (
                    <tr key={req.id}>
                      <td>
                        <strong>{req.userName}</strong>
                        <div className="muted">{req.userEmail}</div>
                      </td>
                      <td>{req.organizationName}</td>
                      <td>{new Date(req.createdAt).toLocaleDateString()}</td>
                      <td>
                        <Link
                          to="/admin/organizer-requests"
                          className="btn btn-small"
                          style={{ textDecoration: "none" }}
                        >
                          Review
                        </Link>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </section>
        )}

        {/* ───── Organizer: Recent Events ───── */}
        {!loading && (isOrganizer || isAdmin) && orgEvents.length > 0 && (
          <section className="card" style={{ maxWidth: "none" }}>
            <h3>{isAdmin ? "Recent Events" : "My Recent Events"}</h3>
            <div className="admin-table-wrapper">
              <table className="admin-table">
                <thead>
                  <tr>
                    <th>Event</th>
                    <th>Date</th>
                    <th>Status</th>
                    <th>Registered</th>
                  </tr>
                </thead>
                <tbody>
                  {orgEvents.map((evt) => (
                    <tr key={evt.id}>
                      <td>
                        <Link to={`/events/${evt.id}`} style={{ color: "var(--deep-teal)" }}>
                          {evt.title}
                        </Link>
                        <div className="muted">{evt.categoryName}</div>
                      </td>
                      <td>{new Date(evt.eventDate).toLocaleDateString()}</td>
                      <td>
                        <span className={`status-badge status-${evt.status.toLowerCase()}`}>
                          {evt.status}
                        </span>
                      </td>
                      <td>{evt.registeredCount}/{evt.maxParticipants}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </section>
        )}
      </main>
    </div>
  );
}
