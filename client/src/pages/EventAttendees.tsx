import { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { LogoutButton } from "../components/LogoutButton";
import { useToast } from "../components/Toast";
import { getEventAttendees, checkInAttendee, getEvent } from "../api/client";
import type { AttendeeDto, EventSummary } from "../types";

export function EventAttendees() {
  const { id } = useParams<{ id: string }>();
  const { user } = useAuth();
  const { addToast } = useToast();
  const [event, setEvent] = useState<EventSummary | null>(null);
  const [attendees, setAttendees] = useState<AttendeeDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [checkingIn, setCheckingIn] = useState<number | null>(null);

  const eventId = Number(id);

  useEffect(() => {
    Promise.all([
      getEvent(eventId).catch(() => null),
      getEventAttendees(eventId).catch(() => []),
    ])
      .then(([evt, att]) => { setEvent(evt); setAttendees(att); })
      .finally(() => setLoading(false));
  }, [eventId]);

  const handleCheckIn = async (studentId: number) => {
    setCheckingIn(studentId);
    try {
      await checkInAttendee(eventId, studentId);
      addToast("success", "Attendee checked in.");
      setAttendees((prev) => prev.map((a) =>
        a.userId === studentId ? { ...a, attended: true, checkedInAt: new Date().toISOString() } : a
      ));
    } catch {
      addToast("error", "Check-in failed.");
    } finally {
      setCheckingIn(null);
    }
  };

  const highestRole = ["Admin", "Organizer", "Participant"].find((r) =>
    user?.roles.includes(r),
  ) || "Visitor";

  if (loading) return <div className="loading-state">Loading attendees...</div>;

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
          <Link to="/events" className="admin-nav-item">Browse Events</Link>
        </nav>
        <LogoutButton style={{ marginTop: "auto" }} />
      </aside>
      <main className="admin-main">
        <div className="admin-header">
          <div style={{ display: "flex", alignItems: "center", gap: "0.75rem" }}>
            <Link to="/organizer/events" className="btn btn-secondary btn-small" style={{ width: "auto" }}>&larr;</Link>
            <div>
              <h1 style={{ margin: 0, fontSize: "1.5rem" }}>Attendees</h1>
              <p className="muted">{event?.title ?? "Event"} — {attendees.length} registered</p>
            </div>
          </div>
          <Link to={`/organizer/events/${eventId}/check-in`} className="btn btn-primary btn-small" style={{ width: "auto" }}>
            QR Check-In
          </Link>
        </div>

        {attendees.length === 0 ? (
          <div className="empty-state"><p>No attendees yet.</p></div>
        ) : (
          <div className="admin-table-wrapper">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Email</th>
                  <th>Department</th>
                  <th>Enrollment</th>
                  <th>Registered</th>
                  <th>Status</th>
                  <th>Action</th>
                </tr>
              </thead>
              <tbody>
                {attendees.map((a) => (
                  <tr key={a.userId}>
                    <td>{a.fullName}</td>
                    <td>{a.email}</td>
                    <td>{a.department ?? "—"}</td>
                    <td>{a.enrollmentNo ?? "—"}</td>
                    <td>{new Date(a.registeredOn).toLocaleDateString()}</td>
                    <td>
                      {a.attended ? (
                        <span className="status-badge status-approved">Checked In</span>
                      ) : (
                        <span className="status-badge status-pending">Registered</span>
                      )}
                    </td>
                    <td>
                      {!a.attended && (
                        <button
                          className="btn btn-small"
                          disabled={checkingIn === a.userId}
                          onClick={() => handleCheckIn(a.userId)}
                          style={{ width: "auto", marginTop: 0 }}
                        >
                          {checkingIn === a.userId ? "..." : "Check In"}
                        </button>
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
