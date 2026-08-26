import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useToast } from "../components/Toast";
import { getMyRegistrations, cancelMyRegistration } from "../api/client";
import type { RegistrationDto } from "../types";

export function MyRegistrations() {
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

  return (
    <div className="dashboard">
      <div className="dash-header">
        <h1 style={{ margin: 0, fontSize: "1.5rem" }}>My Registrations</h1>
        <Link to="/events" className="btn btn-secondary btn-small">Browse Events</Link>
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
                  <td><Link to={`/events/${reg.eventId}`} style={{ color: "#818cf8" }}>{reg.eventTitle}</Link></td>
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
    </div>
  );
}
