import { useCallback, useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import * as api from "../api/client";
import type { NotificationDto } from "../types";
import { useRealtime } from "../realtime/RealtimeContext";

function timeAgo(iso: string): string {
  const seconds = Math.floor((Date.now() - new Date(iso).getTime()) / 1000);
  if (seconds < 60) return "just now";
  const minutes = Math.floor(seconds / 60);
  if (minutes < 60) return `${minutes}m ago`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours}h ago`;
  const days = Math.floor(hours / 24);
  return `${days}d ago`;
}

export function NotificationBell() {
  const [open, setOpen] = useState(false);
  const [items, setItems] = useState<NotificationDto[]>([]);
  const [unread, setUnread] = useState(0);
  const [loading, setLoading] = useState(false);
  const [markingAll, setMarkingAll] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);
  const navigate = useNavigate();
  const { onNotification } = useRealtime();

  const refresh = useCallback(async () => {
    setLoading(true);
    try {
      const [list, count] = await Promise.all([
        api.getMyNotifications(1, 8),
        api.getUnreadNotificationCount(),
      ]);
      setItems(list);
      setUnread(count);
    } catch {
      /* bell failures are non-fatal; the notifications page shows full errors */
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void refresh();
  }, [refresh]);

  // Real-time updates via SignalR.
  useEffect(() => onNotification(() => void refresh()), [onNotification, refresh]);

  // Close dropdown on outside click.
  useEffect(() => {
    if (!open) return;
    const onClick = (e: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) setOpen(false);
    };
    document.addEventListener("mousedown", onClick);
    return () => document.removeEventListener("mousedown", onClick);
  }, [open]);

  const markRead = async (n: NotificationDto) => {
    if (n.isRead) {
      if (n.actionUrl) {
        setOpen(false);
        navigate(n.actionUrl);
      }
      return;
    }
    try {
      await api.markNotificationRead(n.id);
      setItems((prev) => prev.map((x) => (x.id === n.id ? { ...x, isRead: true } : x)));
      setUnread((c) => Math.max(0, c - 1));
      if (n.actionUrl) {
        setOpen(false);
        navigate(n.actionUrl);
      }
    } catch {
      /* non-fatal */
    }
  };

  const markAll = async () => {
    if (markingAll || unread === 0) return;
    setMarkingAll(true);
    try {
      await api.markAllNotificationsRead();
      setItems((prev) => prev.map((x) => ({ ...x, isRead: true })));
      setUnread(0);
    } catch {
      /* non-fatal */
    } finally {
      setMarkingAll(false);
    }
  };

  return (
    <div className="notif-bell" ref={containerRef}>
      <button
        type="button"
        className="notif-bell-btn"
        aria-label={`Notifications (${unread} unread)`}
        onClick={() => setOpen((o) => !o)}
      >
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
          <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9" />
          <path d="M13.73 21a2 2 0 0 1-3.46 0" />
        </svg>
        {unread > 0 && <span className="notif-badge">{unread > 99 ? "99+" : unread}</span>}
      </button>

      {open && (
        <div className="notif-dropdown" role="dialog" aria-label="Notifications">
          <div className="notif-dropdown-header">
            <strong>Notifications</strong>
            <div style={{ display: "flex", gap: "0.5rem" }}>
              <button
                type="button"
                className="linklike"
                onClick={() => {
                  setOpen(false);
                  navigate("/notifications");
                }}
              >
                View all
              </button>
              {unread > 0 && (
                <button type="button" className="linklike" onClick={markAll} disabled={markingAll}>
                  {markingAll ? "Marking…" : "Mark all read"}
                </button>
              )}
            </div>
          </div>

          <div className="notif-dropdown-body">
            {loading && items.length === 0 && <div className="notif-empty">Loading…</div>}
            {!loading && items.length === 0 && (
              <div className="notif-empty">You're all caught up.</div>
            )}
            {items.map((n) => (
              <button
                key={n.id}
                type="button"
                className={`notif-item ${n.isRead ? "" : "unread"}`}
                onClick={() => void markRead(n)}
              >
                <span className="notif-item-title">
                  {!n.isRead && <span className="dot" aria-hidden="true" />}
                  {n.title}
                </span>
                {n.message && <span className="notif-item-msg">{n.message}</span>}
                <span className="notif-item-time">{timeAgo(n.createdAt)}</span>
              </button>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}