import { useCallback, useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import * as api from "../api/client";
import type { NotificationDto } from "../types";
import { useRealtime } from "../realtime/RealtimeContext";
import { useAuth } from "../auth/AuthContext";
import { NotificationBell } from "../components/NotificationBell";
import { LogoutButton } from "../components/LogoutButton";

const PAGE_SIZE = 20;

export function Notifications() {
  const { user } = useAuth();
  const navigate = useNavigate();
  const { onNotification } = useRealtime();

  const [items, setItems] = useState<NotificationDto[]>([]);
  const [page, setPage] = useState(1);
  const [hasMore, setHasMore] = useState(false);
  const [loading, setLoading] = useState(true);
  const [loadingMore, setLoadingMore] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [busyId, setBusyId] = useState<number | null>(null);
  const [markingAll, setMarkingAll] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const list = await api.getMyNotifications(1, PAGE_SIZE);
      setItems(list);
      setHasMore(list.length === PAGE_SIZE);
      setPage(1);
    } catch {
      setError("Unable to load notifications. Please try again.");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void load();
  }, [load]);

  // Live updates when a new notification arrives.
  useEffect(() => onNotification(() => void load()), [onNotification, load]);

  const loadMore = async () => {
    if (loadingMore || !hasMore) return;
    setLoadingMore(true);
    try {
      const next = page + 1;
      const list = await api.getMyNotifications(next, PAGE_SIZE);
      setItems((prev) => [...prev, ...list]);
      setHasMore(list.length === PAGE_SIZE);
      setPage(next);
    } catch {
      setError("Unable to load more notifications.");
    } finally {
      setLoadingMore(false);
    }
  };

  const markRead = async (n: NotificationDto) => {
    if (busyId === n.id) return; // prevent duplicate requests
    setBusyId(n.id);
    try {
      if (!n.isRead) await api.markNotificationRead(n.id);
      setItems((prev) => prev.map((x) => (x.id === n.id ? { ...x, isRead: true } : x)));
      if (n.actionUrl) navigate(n.actionUrl);
    } catch {
      setError("Could not update the notification.");
    } finally {
      setBusyId(null);
    }
  };

  const toggleUnread = async (n: NotificationDto) => {
    if (busyId === n.id) return;
    setBusyId(n.id);
    try {
      if (n.isRead) await api.markNotificationUnread(n.id);
      else await api.markNotificationRead(n.id);
      setItems((prev) =>
        prev.map((x) => (x.id === n.id ? { ...x, isRead: !n.isRead, readAt: null } : x)),
      );
    } catch {
      setError("Could not update the notification.");
    } finally {
      setBusyId(null);
    }
  };

  const markAll = async () => {
    if (markingAll) return;
    setMarkingAll(true);
    try {
      await api.markAllNotificationsRead();
      setItems((prev) => prev.map((x) => ({ ...x, isRead: true })));
    } catch {
      setError("Could not mark all as read.");
    } finally {
      setMarkingAll(false);
    }
  };

  const unreadCount = items.filter((n) => !n.isRead).length;

  return (
    <div className="admin-layout">
      <aside className="admin-sidebar">
        <div className="admin-brand">EventSphere</div>
        <div className="sidebar-welcome">
          Welcome, <strong>{user?.name}</strong>
        </div>
        <nav className="admin-nav">
          <Link to="/dashboard" className="admin-nav-item">Dashboard</Link>
          <Link to="/events" className="admin-nav-item">Browse Events</Link>
          <Link to="/notifications" className="admin-nav-item active">Notifications</Link>
          <Link to="/messages" className="admin-nav-item">Messages</Link>
          <Link to="/profile" className="admin-nav-item">Profile</Link>
        </nav>
        <LogoutButton style={{ marginTop: "auto" }} />
      </aside>

      <main className="admin-main">
        <header className="admin-header notif-page-header">
          <h1>Notifications</h1>
          <NotificationBell />
        </header>

        {error && (
          <div className="card error-state" role="alert">
            {error}
            <button type="button" className="btn btn-small" onClick={() => void load()}>
              Retry
            </button>
          </div>
        )}

        {loading && <div className="loading-state">Loading notifications…</div>}

        {!loading && !error && items.length === 0 && (
          <div className="empty-state card">
            <h3>No notifications yet</h3>
            <p>You'll see registration confirmations, payment updates and event news here.</p>
            <Link to="/events" className="btn">Browse events</Link>
          </div>
        )}

        {!loading && items.length > 0 && (
          <>
            <div className="notif-toolbar">
              <span>{unreadCount} unread</span>
              <button
                type="button"
                className="btn btn-small btn-secondary"
                onClick={markAll}
                disabled={markingAll || unreadCount === 0}
              >
                {markingAll ? "Marking…" : "Mark all as read"}
              </button>
            </div>

            <ul className="notif-list">
              {items.map((n) => (
                <li key={n.id} className={`notif-row ${n.isRead ? "" : "unread"}`}>
                  <div className="notif-row-main">
                    <span className="notif-type-badge">{n.type.replace(/([A-Z])/g, " $1").trim()}</span>
                    <strong>{n.title}</strong>
                    {n.message && <p>{n.message}</p>}
                    <time dateTime={n.createdAt}>{new Date(n.createdAt).toLocaleString()}</time>
                  </div>
                  <div className="notif-row-actions">
                    <button
                      type="button"
                      className="linklike"
                      onClick={() => void markRead(n)}
                      disabled={busyId === n.id}
                    >
                      {busyId === n.id ? "…" : n.actionUrl ? "Open" : "Mark read"}
                    </button>
                    <button
                      type="button"
                      className="linklike"
                      onClick={() => void toggleUnread(n)}
                      disabled={busyId === n.id}
                    >
                      {n.isRead ? "Mark unread" : "Mark read"}
                    </button>
                  </div>
                </li>
              ))}
            </ul>

            {hasMore && (
              <button type="button" className="btn" onClick={loadMore} disabled={loadingMore}>
                {loadingMore ? "Loading…" : "Load more"}
              </button>
            )}
          </>
        )}
      </main>
    </div>
  );
}