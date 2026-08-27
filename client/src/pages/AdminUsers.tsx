import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { LogoutButton } from "../components/LogoutButton";
import { useToast } from "../components/Toast";
import {
  getAdminUsers,
  getAdminUser,
  toggleUserActive,
  warnUser,
  assignUserRole,
  removeUserRole,
} from "../api/client";
import type { AdminUser, AdminUserDetail } from "../types";

const ALL_ROLES = ["Visitor", "Participant", "Organizer", "Admin"];

export function AdminUsers() {
  const { user } = useAuth();
  const { addToast } = useToast();

  // ── List state ──
  const [users, setUsers] = useState<AdminUser[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [loading, setLoading] = useState(true);
  const [actionId, setActionId] = useState<number | null>(null);

  // ── Detail modal state ──
  const [detailUser, setDetailUser] = useState<AdminUserDetail | null>(null);
  const [detailLoading, setDetailLoading] = useState(false);
  const [addRoleValue, setAddRoleValue] = useState("");

  // ── Suspend reason state ──
  const [suspendReason, setSuspendReason] = useState("");
  const [showSuspendModal, setShowSuspendModal] = useState<number | null>(null);

  // ── Warn state ──
  const [warnMessage, setWarnMessage] = useState("");
  const [warnSendEmail, setWarnSendEmail] = useState(true);
  const [showWarnModal, setShowWarnModal] = useState<number | null>(null);

  // ── Fetch paginated user list ──
  const fetchUsers = (p: number, s: string) => {
    setLoading(true);
    getAdminUsers({ page: p, search: s || undefined })
      .then((res) => {
        setUsers(res.users);
        setTotal(res.total);
      })
      .catch(() => addToast("error", "Failed to load users."))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    fetchUsers(page, search);
  }, [page]);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setPage(1);
    fetchUsers(1, search);
  };

  // ── Warn user ──
  const handleWarn = async (id: number) => {
    setActionId(id);
    try {
      await warnUser(id, warnMessage, warnSendEmail);
      addToast("success", "Warning sent to user.");
    } catch {
      addToast("error", "Failed to send warning.");
    } finally {
      setActionId(null);
      setShowWarnModal(null);
      setWarnMessage("");
      setWarnSendEmail(true);
    }
  };

  // ── Toggle active / suspend ──
  const handleToggle = async (id: number, reason?: string) => {
    setActionId(id);
    try {
      await toggleUserActive(id, reason);
      setUsers((prev) =>
        prev.map((u) => (u.id === id ? { ...u, isActive: !u.isActive } : u))
      );
      addToast("success", "User status updated.");
      // Refresh detail modal if open
      if (detailUser?.id === id) {
        const updated = await getAdminUser(id);
        setDetailUser(updated);
      }
    } catch {
      addToast("error", "Failed to update user.");
    } finally {
      setActionId(null);
      setShowSuspendModal(null);
      setSuspendReason("");
    }
  };

  // ── Assign role ──
  const handleAddRole = async (id: number, role: string) => {
    setActionId(id);
    try {
      await assignUserRole(id, role);
      addToast("success", `Role "${role}" assigned.`);
      fetchUsers(page, search);
      if (detailUser?.id === id) {
        const updated = await getAdminUser(id);
        setDetailUser(updated);
      }
    } catch {
      addToast("error", "Failed to assign role.");
    } finally {
      setActionId(null);
      setAddRoleValue("");
    }
  };

  // ── Remove role ──
  const handleRemoveRole = async (id: number, role: string) => {
    setActionId(id);
    try {
      await removeUserRole(id, role);
      addToast("success", `Role "${role}" removed.`);
      fetchUsers(page, search);
      if (detailUser?.id === id) {
        const updated = await getAdminUser(id);
        setDetailUser(updated);
      }
    } catch {
      addToast("error", "Failed to remove role.");
    } finally {
      setActionId(null);
    }
  };

  // ── Open detail modal ──
  const openDetail = async (id: number) => {
    setDetailLoading(true);
    try {
      const data = await getAdminUser(id);
      setDetailUser(data);
      setAddRoleValue("");
    } catch {
      addToast("error", "Failed to load user details.");
    } finally {
      setDetailLoading(false);
    }
  };

  const closeDetail = () => setDetailUser(null);

  const availableRoles = detailUser
    ? ALL_ROLES.filter((r) => !detailUser.roles.includes(r))
    : [];

  return (
    <div className="admin-layout">
      {/* ── Sidebar ── */}
      <aside className="admin-sidebar">
        <div className="admin-brand">EventSphere</div>
        <div className="sidebar-welcome">
          Welcome, <strong>{user?.name}</strong>
        </div>
        <nav className="admin-nav">
          <Link to="/admin" className="admin-nav-item">
            Dashboard
          </Link>
          <Link to="/admin/users" className="admin-nav-item active">
            Users
          </Link>
          <Link to="/admin/events" className="admin-nav-item">
            Events
          </Link>
          <Link to="/admin/organizer-requests" className="admin-nav-item">
            Organizer Requests
          </Link>
          <Link to="/admin/reviews" className="admin-nav-item">
            Reviews
          </Link>
          <Link to="/admin/announcements" className="admin-nav-item">
            Announcements
          </Link>
          <Link to="/admin/reports" className="admin-nav-item">
            Reports
          </Link>
        </nav>
        <LogoutButton style={{ marginTop: "auto" }} />
      </aside>

      {/* ── Main ── */}
      <main className="admin-main">
        <div className="admin-header">
          <div>
            <h1 style={{ margin: 0, fontSize: "1.5rem" }}>User Management</h1>
            <p className="muted">{total} total users</p>
          </div>
          <form
            onSubmit={handleSearch}
            style={{ display: "flex", gap: "0.5rem" }}
          >
            <input
              type="text"
              placeholder="Search by name or email..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              style={{
                padding: "0.5rem",
                border: "1px solid var(--ink-violet)",
                background: "var(--bg)",
                color: "var(--text)",
              }}
            />
            <button
              className="btn btn-primary btn-small"
              type="submit"
              style={{ width: "auto" }}
            >
              Search
            </button>
          </form>
        </div>

        {/* ── Table ── */}
        {loading ? (
          <div className="loading-state">Loading...</div>
        ) : users.length === 0 ? (
          <div className="empty-state">No users found.</div>
        ) : (
          <div className="admin-table-wrapper">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Email</th>
                  <th>Role</th>
                  <th>Status</th>
                  <th>Joined</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {users.map((u) => (
                  <tr key={u.id}>
                    <td>{u.fullName || "—"}</td>
                    <td>{u.email}</td>
                    <td>
                      <span className="role-badge">{u.role}</span>
                    </td>
                    <td>
                      {u.isActive ? (
                        <span className="status-badge status-approved">
                          Active
                        </span>
                      ) : (
                        <span
                          className="status-badge"
                          style={{ color: "var(--danger)" }}
                        >
                          Suspended
                        </span>
                      )}
                    </td>
                    <td>{new Date(u.createdAt).toLocaleDateString()}</td>
                    <td
                      style={{
                        display: "flex",
                        gap: "0.25rem",
                        flexWrap: "wrap",
                      }}
                    >
                      <button
                        className="btn btn-secondary btn-small"
                        disabled={detailLoading}
                        onClick={() => openDetail(u.id)}
                        style={{ width: "auto", marginTop: 0 }}
                      >
                        View
                      </button>
                      {u.isActive && (
                        <button
                          className="btn btn-small"
                          disabled={actionId === u.id}
                          onClick={() => { setShowWarnModal(u.id); setWarnMessage(""); setWarnSendEmail(true); }}
                          style={{ width: "auto", marginTop: 0, borderColor: "var(--accent-gold)", color: "var(--accent-gold)" }}
                          title="Send warning"
                        >
                          ⚠️ Warn
                        </button>
                      )}
                      {u.isActive ? (
                        <button
                          className="btn btn-small"
                          disabled={actionId === u.id}
                          onClick={() => { setShowSuspendModal(u.id); setSuspendReason(""); }}
                          style={{ width: "auto", marginTop: 0 }}
                        >
                          Suspend
                        </button>
                      ) : (
                        <button
                          className="btn btn-small"
                          disabled={actionId === u.id}
                          onClick={() => handleToggle(u.id)}
                          style={{ width: "auto", marginTop: 0 }}
                        >
                          Reactivate
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        {/* ── Pagination ── */}
        {total > 20 && (
          <div
            style={{
              display: "flex",
              gap: "0.5rem",
              justifyContent: "center",
              marginTop: "1rem",
            }}
          >
            <button
              className="btn btn-secondary btn-small"
              disabled={page <= 1}
              onClick={() => setPage((p) => p - 1)}
              style={{ width: "auto" }}
            >
              Prev
            </button>
            <span className="muted" style={{ lineHeight: "2rem" }}>
              Page {page} of {Math.ceil(total / 20)}
            </span>
            <button
              className="btn btn-secondary btn-small"
              disabled={users.length < 20}
              onClick={() => setPage((p) => p + 1)}
              style={{ width: "auto" }}
            >
              Next
            </button>
          </div>
        )}
      </main>

      {/* ════════════════════════════ Detail Modal ════════════════════════════ */}
      {(detailUser || detailLoading) && (
        <div
          className="modal-overlay"
          onClick={closeDetail}
          style={{
            position: "fixed",
            inset: 0,
            background: "rgba(0,0,0,0.5)",
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            zIndex: 1000,
          }}
        >
          <div
            className="modal-content card"
            onClick={(e) => e.stopPropagation()}
            style={{
              width: "90%",
              maxWidth: 560,
              maxHeight: "85vh",
              overflowY: "auto",
              padding: "1.5rem",
            }}
          >
            {detailLoading && !detailUser ? (
              <div className="loading-state">Loading user details...</div>
            ) : detailUser ? (
              <>
                {/* Header */}
                <div
                  style={{
                    display: "flex",
                    justifyContent: "space-between",
                    alignItems: "flex-start",
                    marginBottom: "1rem",
                  }}
                >
                  <div>
                    <h2 style={{ margin: 0, fontSize: "1.25rem" }}>
                      {detailUser.fullName || detailUser.email}
                    </h2>
                    <p className="muted" style={{ margin: "0.25rem 0 0" }}>
                      {detailUser.email}
                    </p>
                  </div>
                  <button
                    className="btn btn-secondary btn-small"
                    onClick={closeDetail}
                    style={{ width: "auto", marginTop: 0 }}
                  >
                    ✕
                  </button>
                </div>

                {/* Profile info */}
                <div
                  style={{
                    display: "grid",
                    gridTemplateColumns: "1fr 1fr",
                    gap: "0.75rem",
                    marginBottom: "1.25rem",
                  }}
                >
                  <div>
                    <div className="muted" style={{ fontSize: "0.75rem" }}>
                      Mobile
                    </div>
                    <div>{detailUser.mobile || "—"}</div>
                  </div>
                  <div>
                    <div className="muted" style={{ fontSize: "0.75rem" }}>
                      Department
                    </div>
                    <div>{detailUser.department || "—"}</div>
                  </div>
                  <div>
                    <div className="muted" style={{ fontSize: "0.75rem" }}>
                      Enrollment No
                    </div>
                    <div>{detailUser.enrollmentNo || "—"}</div>
                  </div>
                  <div>
                    <div className="muted" style={{ fontSize: "0.75rem" }}>
                      Status
                    </div>
                    <div>
                      {detailUser.isActive ? (
                        <span className="status-badge status-approved">
                          Active
                        </span>
                      ) : (
                        <span
                          className="status-badge"
                          style={{ color: "var(--danger)" }}
                        >
                          Suspended
                        </span>
                      )}
                    </div>
                  </div>
                  <div>
                    <div className="muted" style={{ fontSize: "0.75rem" }}>
                      Joined
                    </div>
                    <div>
                      {new Date(detailUser.createdAt).toLocaleDateString()}
                    </div>
                  </div>
                </div>

                {/* ── Roles section ── */}
                <div style={{ marginBottom: "1.25rem" }}>
                  <div
                    className="muted"
                    style={{ fontSize: "0.75rem", marginBottom: "0.5rem" }}
                  >
                    Roles
                  </div>
                  <div
                    style={{
                      display: "flex",
                      flexWrap: "wrap",
                      gap: "0.4rem",
                      alignItems: "center",
                    }}
                  >
                    {detailUser.roles.map((r) => (
                      <span
                        key={r}
                        className="role-badge"
                        style={{
                          display: "inline-flex",
                          alignItems: "center",
                          gap: "0.35rem",
                        }}
                      >
                        {r}
                        <button
                          onClick={() => handleRemoveRole(detailUser.id, r)}
                          disabled={actionId === detailUser.id}
                          title={`Remove ${r} role`}
                          style={{
                            background: "none",
                            border: "none",
                            color: "var(--danger)",
                            cursor: "pointer",
                            padding: 0,
                            fontSize: "0.85rem",
                            lineHeight: 1,
                          }}
                        >
                          ✕
                        </button>
                      </span>
                    ))}
                  </div>

                  {/* Add role dropdown */}
                  {availableRoles.length > 0 && (
                    <div
                      style={{
                        display: "flex",
                        gap: "0.4rem",
                        marginTop: "0.5rem",
                        alignItems: "center",
                      }}
                    >
                      <select
                        value={addRoleValue}
                        onChange={(e) => setAddRoleValue(e.target.value)}
                        style={{
                          padding: "0.35rem 0.5rem",
                          border: "1px solid var(--ink-violet)",
                          background: "var(--bg)",
                          color: "var(--text)",
                          fontSize: "0.8rem",
                          borderRadius: 4,
                        }}
                      >
                        <option value="">Add a role...</option>
                        {availableRoles.map((r) => (
                          <option key={r} value={r}>
                            {r}
                          </option>
                        ))}
                      </select>
                      <button
                        className="btn btn-primary btn-small"
                        disabled={!addRoleValue || actionId === detailUser.id}
                        onClick={() =>
                          addRoleValue &&
                          handleAddRole(detailUser.id, addRoleValue)
                        }
                        style={{ width: "auto", marginTop: 0 }}
                      >
                        Assign
                      </button>
                    </div>
                  )}
                </div>

                {/* ── Suspension Reason ── */}
                {!detailUser.isActive && detailUser.suspendReason && (
                  <div
                    style={{
                      background: "rgba(220,53,69,0.08)",
                      border: "1px solid rgba(220,53,69,0.2)",
                      borderRadius: 6,
                      padding: "0.6rem 0.8rem",
                      marginBottom: "1rem",
                    }}
                  >
                    <div style={{ fontSize: "0.7rem", textTransform: "uppercase", letterSpacing: "0.05em", color: "var(--danger)", marginBottom: "0.2rem", fontWeight: 600 }}>
                      Suspension Reason
                    </div>
                    <div style={{ fontSize: "0.85rem", lineHeight: 1.5 }}>
                      {detailUser.suspendReason}
                    </div>
                  </div>
                )}

                {!detailUser.isActive && !detailUser.suspendReason && (
                  <div
                    style={{
                      background: "rgba(220,53,69,0.08)",
                      border: "1px solid rgba(220,53,69,0.2)",
                      borderRadius: 6,
                      padding: "0.6rem 0.8rem",
                      marginBottom: "1rem",
                    }}
                  >
                    <div style={{ fontSize: "0.85rem", color: "var(--danger)" }}>
                      ⚠️ This account is currently suspended.
                    </div>
                  </div>
                )}

                {/* ── Actions ── */}
                <div
                  style={{
                    display: "flex",
                    gap: "0.5rem",
                    borderTop: "1px solid var(--border)",
                    paddingTop: "1rem",
                  }}
                >
                  {detailUser.isActive && (
                    <button
                      className="btn btn-small"
                      disabled={actionId === detailUser.id}
                      onClick={() => { setShowWarnModal(detailUser.id); setWarnMessage(""); setWarnSendEmail(true); }}
                      style={{ width: "auto", marginTop: 0, borderColor: "var(--accent-gold)", color: "var(--accent-gold)" }}
                    >
                      ⚠️ Warn User
                    </button>
                  )}
                  {detailUser.isActive ? (
                    <button
                      className="btn btn-small"
                      disabled={actionId === detailUser.id}
                      onClick={() => { setShowSuspendModal(detailUser.id); setSuspendReason(""); }}
                      style={{ width: "auto", marginTop: 0 }}
                    >
                      Suspend User
                    </button>
                  ) : (
                    <button
                      className="btn btn-primary btn-small"
                      disabled={actionId === detailUser.id}
                      onClick={() => handleToggle(detailUser.id)}
                      style={{ width: "auto", marginTop: 0 }}
                    >
                      Reactivate User
                    </button>
                  )}
                </div>
              </>
            ) : null}
          </div>
        </div>
      )}

      {/* ════════════════════════════ Suspend Reason Modal ════════════════════════════ */}
      {showSuspendModal !== null && (
        <div
          className="modal-overlay"
          onClick={() => { setShowSuspendModal(null); setSuspendReason(""); }}
          style={{
            position: "fixed",
            inset: 0,
            background: "rgba(0,0,0,0.5)",
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            zIndex: 1001,
          }}
        >
          <div
            className="modal-content card"
            onClick={(e) => e.stopPropagation()}
            style={{ width: "90%", maxWidth: 440, padding: "1.5rem" }}
          >
            <h2 style={{ margin: "0 0 0.75rem", fontSize: "1.15rem" }}>Suspend User</h2>
            <p className="muted" style={{ margin: "0 0 1rem", fontSize: "0.85rem" }}>
              The user will be immediately logged out and unable to sign in again.
              Provide a reason so they understand why.
            </p>

            <label style={{ fontSize: "0.8rem" }}>Reason (optional)</label>
            <textarea
              value={suspendReason}
              onChange={(e) => setSuspendReason(e.target.value)}
              rows={3}
              maxLength={500}
              placeholder="e.g. Violation of community guidelines..."
              style={{
                padding: "0.5rem",
                border: "1px solid var(--ink-violet)",
                background: "var(--bg)",
                color: "var(--text)",
                width: "100%",
                boxSizing: "border-box",
                resize: "vertical",
                borderRadius: 4,
                marginBottom: "1rem",
              }}
            />

            <div style={{ display: "flex", gap: "0.5rem", justifyContent: "flex-end" }}>
              <button
                className="btn btn-secondary btn-small"
                onClick={() => { setShowSuspendModal(null); setSuspendReason(""); }}
                style={{ width: "auto", marginTop: 0 }}
              >
                Cancel
              </button>
              <button
                className="btn btn-small"
                disabled={actionId === showSuspendModal}
                onClick={() => handleToggle(showSuspendModal, suspendReason || undefined)}
                style={{ width: "auto", marginTop: 0, background: "var(--danger)", color: "#fff", border: "none" }}
              >
                {actionId === showSuspendModal ? "Suspending..." : "Suspend Account"}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ════════════════════════════ Warn User Modal ════════════════════════════ */}
      {showWarnModal !== null && (
        <div
          className="modal-overlay"
          onClick={() => { setShowWarnModal(null); setWarnMessage(""); }}
          style={{
            position: "fixed",
            inset: 0,
            background: "rgba(0,0,0,0.5)",
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            zIndex: 1001,
          }}
        >
          <div
            className="modal-content card"
            onClick={(e) => e.stopPropagation()}
            style={{ width: "90%", maxWidth: 440, padding: "1.5rem" }}
          >
            <h2 style={{ margin: "0 0 0.5rem", fontSize: "1.15rem" }}>⚠️ Warn User</h2>
            <p className="muted" style={{ margin: "0 0 1rem", fontSize: "0.85rem" }}>
              The user will receive an in-app notification. They can still log in.
            </p>

            <label style={{ fontSize: "0.8rem" }}>Warning Message *</label>
            <textarea
              value={warnMessage}
              onChange={(e) => setWarnMessage(e.target.value)}
              rows={3}
              maxLength={1000}
              placeholder="e.g. Please review our community guidelines..."
              style={{
                padding: "0.5rem",
                border: "1px solid var(--ink-violet)",
                background: "var(--bg)",
                color: "var(--text)",
                width: "100%",
                boxSizing: "border-box",
                resize: "vertical",
                borderRadius: 4,
                marginBottom: "0.75rem",
              }}
            />

            <label style={{ display: "flex", alignItems: "center", gap: "0.5rem", fontSize: "0.85rem", cursor: "pointer", marginBottom: "1rem" }}>
              <input
                type="checkbox"
                checked={warnSendEmail}
                onChange={(e) => setWarnSendEmail(e.target.checked)}
              />
              Also send via email
            </label>

            <div style={{ display: "flex", gap: "0.5rem", justifyContent: "flex-end" }}>
              <button
                className="btn btn-secondary btn-small"
                onClick={() => { setShowWarnModal(null); setWarnMessage(""); }}
                style={{ width: "auto", marginTop: 0 }}
              >
                Cancel
              </button>
              <button
                className="btn btn-small"
                disabled={actionId === showWarnModal || !warnMessage.trim()}
                onClick={() => handleWarn(showWarnModal)}
                style={{ width: "auto", marginTop: 0, background: "var(--accent-gold)", color: "#000", border: "none" }}
              >
                {actionId === showWarnModal ? "Sending..." : "Send Warning"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
