import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { useToast } from "../components/Toast";
import {
  getAdminOrganizerRequests,
  approveOrganizerRequest,
  rejectOrganizerRequest,
  ApiError,
} from "../api/client";
import type { AdminOrganizerRequest } from "../types";

type Filter = "All" | "Pending" | "Approved" | "Rejected";

export function AdminOrganizerRequests() {
  const { logout } = useAuth();
  const { addToast } = useToast();
  const navigate = useNavigate();

  const [requests, setRequests] = useState<AdminOrganizerRequest[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState<Filter>("Pending");
  const [selected, setSelected] = useState<AdminOrganizerRequest | null>(null);
  const [actionBusy, setActionBusy] = useState<"approve" | "reject" | null>(null);
  const [rejectReason, setRejectReason] = useState("");
  const [showRejectDialog, setShowRejectDialog] = useState(false);

  useEffect(() => {
    void loadRequests();
  }, [filter]);

  const loadRequests = async () => {
    setLoading(true);
    try {
      const statusParam = filter === "All" ? undefined : filter;
      const r = await getAdminOrganizerRequests(statusParam);
      setRequests(r);
    } catch {
      addToast("error", "Unable to load organizer requests.");
    } finally {
      setLoading(false);
    }
  };

  const handleApprove = async (req: AdminOrganizerRequest) => {
    if (actionBusy) return;
    setActionBusy("approve");
    try {
      await approveOrganizerRequest(req.id);
      addToast("success", "Organizer approved successfully.");
      setSelected(null);
      void loadRequests();
    } catch (err) {
      if (err instanceof ApiError) {
        addToast("error", err.message);
      } else {
        addToast("error", "Unable to approve organizer request.");
      }
    } finally {
      setActionBusy(null);
    }
  };

  const handleReject = async (req: AdminOrganizerRequest) => {
    if (actionBusy) return;
    setActionBusy("reject");
    try {
      await rejectOrganizerRequest(req.id, rejectReason.trim() || undefined);
      addToast("success", "Organizer application rejected.");
      setSelected(null);
      setShowRejectDialog(false);
      setRejectReason("");
      void loadRequests();
    } catch (err) {
      if (err instanceof ApiError) {
        addToast("error", err.message);
      } else {
        addToast("error", "Unable to reject organizer request.");
      }
    } finally {
      setActionBusy(null);
    }
  };

  const onLogout = async () => {
    await logout();
    addToast("info", "You have been signed out.");
    navigate("/");
  };

  const filteredRequests = requests;

  return (
    <div className="admin-layout">
      <aside className="admin-sidebar">
        <div className="admin-brand">EventSphere</div>
        <nav className="admin-nav">
          <Link to="/admin" className="admin-nav-item">
            Dashboard
          </Link>
          <Link to="/admin/organizer-requests" className="admin-nav-item active">
            Organizer Requests
          </Link>
          <Link to="/dashboard" className="admin-nav-item">
            Main App
          </Link>
        </nav>
        <button className="btn btn-secondary" onClick={onLogout} style={{ marginTop: "auto" }}>
          Logout
        </button>
      </aside>
      <main className="admin-main">
        <header className="admin-header">
          <h1>Organizer Requests</h1>
        </header>

        <div className="filter-bar">
          {(["All", "Pending", "Approved", "Rejected"] as Filter[]).map((f) => (
            <button
              key={f}
              className={`btn btn-small ${filter === f ? "btn-primary" : "btn-secondary"}`}
              onClick={() => setFilter(f)}
            >
              {f}
            </button>
          ))}
        </div>

        {loading ? (
          <p className="muted">Loading organizer requests...</p>
        ) : filteredRequests.length === 0 ? (
          <p className="muted">No organizer applications found.</p>
        ) : (
          <div className="admin-table-wrapper">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Applicant</th>
                  <th>Organization</th>
                  <th>Submitted</th>
                  <th>Status</th>
                  <th>Action</th>
                </tr>
              </thead>
              <tbody>
                {filteredRequests.map((r) => (
                  <tr key={r.id}>
                    <td>{r.userName}</td>
                    <td>{r.organizationName}</td>
                    <td>{new Date(r.createdAt).toLocaleDateString()}</td>
                    <td>
                      <span className={`status-badge status-${r.status.toLowerCase()}`}>
                        {r.status}
                      </span>
                    </td>
                    <td>
                      <button
                        className="btn btn-small btn-secondary"
                        onClick={() => setSelected(r)}
                      >
                        Review
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        {/* Detail Modal */}
        {selected && (
          <div className="modal-overlay" onClick={() => !actionBusy && setSelected(null)}>
            <div className="modal card" onClick={(e) => e.stopPropagation()}>
              <h3>Organizer Request Details</h3>
              <div className="profile-info">
                <div className="profile-field">
                  <span className="profile-label">Applicant</span>
                  <span>{selected.userName}</span>
                </div>
                <div className="profile-field">
                  <span className="profile-label">Email</span>
                  <span>{selected.userEmail}</span>
                </div>
                <div className="profile-field">
                  <span className="profile-label">Organization</span>
                  <span>{selected.organizationName}</span>
                </div>
                <div className="profile-field">
                  <span className="profile-label">Reason</span>
                  <span>{selected.reason}</span>
                </div>
                {selected.experience && (
                  <div className="profile-field">
                    <span className="profile-label">Experience</span>
                    <span>{selected.experience}</span>
                  </div>
                )}
                <div className="profile-field">
                  <span className="profile-label">Submitted</span>
                  <span>{new Date(selected.createdAt).toLocaleString()}</span>
                </div>
                <div className="profile-field">
                  <span className="profile-label">Status</span>
                  <span className={`status-badge status-${selected.status.toLowerCase()}`}>
                    {selected.status}
                  </span>
                </div>
                {selected.rejectionReason && (
                  <div className="profile-field">
                    <span className="profile-label">Rejection Reason</span>
                    <span>{selected.rejectionReason}</span>
                  </div>
                )}
              </div>

              {selected.status === "Pending" && !showRejectDialog && (
                <div style={{ display: "flex", gap: "0.5rem", marginTop: "1rem" }}>
                  <button
                    className="btn btn-primary"
                    onClick={() => handleApprove(selected)}
                    disabled={actionBusy !== null}
                    style={{
                      flex: 1,
                      background: "var(--ok)",
                    }}
                  >
                    {actionBusy === "approve" ? "Approving..." : "Approve"}
                  </button>
                  <button
                    className="btn btn-secondary"
                    onClick={() => setShowRejectDialog(true)}
                    disabled={actionBusy !== null}
                    style={{
                      color: "var(--danger)",
                      borderColor: "var(--danger)",
                    }}
                  >
                    Reject
                  </button>
                </div>
              )}

              {showRejectDialog && (
                <div style={{ marginTop: "1rem" }}>
                  <label htmlFor="rejectReason">Rejection Reason (optional)</label>
                  <textarea
                    id="rejectReason"
                    value={rejectReason}
                    onChange={(e) => setRejectReason(e.target.value)}
                    rows={3}
                    placeholder="Provide a reason for rejection"
                    style={{ resize: "vertical", width: "100%" }}
                  />
                  <div style={{ display: "flex", gap: "0.5rem", marginTop: "0.5rem" }}>
                    <button
                      className="btn btn-primary"
                      onClick={() => handleReject(selected)}
                      disabled={actionBusy !== null}
                      style={{
                        flex: 1,
                        background: "var(--danger)",
                      }}
                    >
                      {actionBusy === "reject" ? "Rejecting..." : "Confirm Rejection"}
                    </button>
                    <button
                      className="btn btn-secondary"
                      onClick={() => {
                        setShowRejectDialog(false);
                        setRejectReason("");
                      }}
                      disabled={actionBusy !== null}
                    >
                      Cancel
                    </button>
                  </div>
                </div>
              )}

              {selected.status !== "Pending" && (
                <button
                  className="btn btn-secondary"
                  onClick={() => setSelected(null)}
                  style={{ marginTop: "1rem" }}
                >
                  Close
                </button>
              )}
            </div>
          </div>
        )}
      </main>
    </div>
  );
}
