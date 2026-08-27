import { useEffect, useState, useMemo, useCallback } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { getCalendarEvents } from "../api/client";
import type { CalendarEvent } from "../types";

// ── Helpers ──

function startOfMonth(d: Date): Date {
  return new Date(d.getFullYear(), d.getMonth(), 1);
}

function endOfMonth(d: Date): Date {
  return new Date(d.getFullYear(), d.getMonth() + 1, 0);
}

function startOfGrid(d: Date): Date {
  const first = startOfMonth(d);
  const day = first.getDay(); // 0=Sun
  return new Date(first.getFullYear(), first.getMonth(), first.getDate() - day);
}

function isSameDay(a: Date, b: Date): boolean {
  return (
    a.getFullYear() === b.getFullYear() &&
    a.getMonth() === b.getMonth() &&
    a.getDate() === b.getDate()
  );
}

function isToday(d: Date): boolean {
  return isSameDay(d, new Date());
}

function formatTime(timeStr: string): string {
  // timeStr comes as "HH:mm:ss" or "HH:mm" from the backend TimeSpan
  const parts = timeStr.split(":");
  const h = parseInt(parts[0], 10);
  const m = parts[1] ?? "00";
  const ampm = h >= 12 ? "PM" : "AM";
  const h12 = h % 12 || 12;
  return `${h12}:${m} ${ampm}`;
}

function formatDateParam(d: Date): string {
  return d.toISOString().split("T")[0];
}

const WEEKDAYS = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
const MONTH_NAMES = [
  "January", "February", "March", "April", "May", "June",
  "July", "August", "September", "October", "November", "December",
];

// ── Component ──

export function Calendar() {
  const { user } = useAuth();
  const [currentDate, setCurrentDate] = useState(() => new Date());
  const [events, setEvents] = useState<CalendarEvent[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filter, setFilter] = useState<"all" | "registered">("all");

  // Calculate date range for the visible grid (includes padding days from adjacent months)
  const gridStart = useMemo(() => startOfGrid(currentDate), [currentDate]);
  const gridEnd = useMemo(() => {
    const end = endOfMonth(currentDate);
    const day = end.getDay();
    return new Date(end.getFullYear(), end.getMonth(), end.getDate() + (6 - day));
  }, [currentDate]);

  // Fetch events for the visible range
  const fetchEvents = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await getCalendarEvents({
        fromDate: formatDateParam(gridStart),
        toDate: formatDateParam(gridEnd),
      });
      setEvents(data);
    } catch {
      setError("Failed to load calendar events.");
    } finally {
      setLoading(false);
    }
  }, [gridStart, gridEnd]);

  useEffect(() => {
    fetchEvents();
  }, [fetchEvents]);

  // Group events by date string for quick lookup
  const eventsByDate = useMemo(() => {
    const map = new Map<string, CalendarEvent[]>();
    for (const evt of events) {
      const dateKey = evt.eventDate.split("T")[0]; // "YYYY-MM-DD"
      if (!map.has(dateKey)) map.set(dateKey, []);
      map.get(dateKey)!.push(evt);
    }
    return map;
  }, [events]);

  // Filter events
  const filteredEventsByDate = useMemo(() => {
    if (filter === "all") return eventsByDate;
    const filtered = new Map<string, CalendarEvent[]>();
    for (const [key, evts] of eventsByDate) {
      const f = evts.filter((e) => e.isRegistered);
      if (f.length > 0) filtered.set(key, f);
    }
    return filtered;
  }, [eventsByDate, filter]);

  // Build grid days
  const gridDays = useMemo(() => {
    const days: Date[] = [];
    const current = new Date(gridStart);
    while (current <= gridEnd) {
      days.push(new Date(current));
      current.setDate(current.getDate() + 1);
    }
    return days;
  }, [gridStart, gridEnd]);

  // Navigation
  const goToPrevMonth = () => {
    setCurrentDate((d) => new Date(d.getFullYear(), d.getMonth() - 1, 1));
  };

  const goToNextMonth = () => {
    setCurrentDate((d) => new Date(d.getFullYear(), d.getMonth() + 1, 1));
  };

  const goToToday = () => {
    setCurrentDate(new Date());
  };

  // Upcoming events (today or later, sorted)
  const upcomingEvents = useMemo(() => {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return events
      .filter((e) => new Date(e.eventDate) >= today)
      .sort((a, b) => new Date(a.eventDate).getTime() - new Date(b.eventDate).getTime())
      .slice(0, 8);
  }, [events]);

  // Past events
  const pastEvents = useMemo(() => {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return events
      .filter((e) => new Date(e.eventDate) < today)
      .sort((a, b) => new Date(b.eventDate).getTime() - new Date(a.eventDate).getTime())
      .slice(0, 5);
  }, [events]);

  return (
    <div className="admin-layout">
      {/* ── Sidebar ── */}
      <aside className="admin-sidebar">
        <div className="admin-brand">EventSphere</div>
        <div className="sidebar-welcome">
          Welcome, <strong>{user?.name}</strong>
        </div>
        <nav className="admin-nav">
          <Link to="/dashboard" className="admin-nav-item">Dashboard</Link>
          <Link to="/events" className="admin-nav-item">Browse Events</Link>
          <Link to="/calendar" className="admin-nav-item active">Calendar</Link>
          <Link to="/my-registrations" className="admin-nav-item">My Registrations</Link>
          <Link to="/favorites" className="admin-nav-item">Favorites</Link>
          <Link to="/notifications" className="admin-nav-item">Notifications</Link>
        </nav>
      </aside>

      {/* ── Main ── */}
      <main className="admin-main">
        <div className="admin-header">
          <div>
            <h1 style={{ margin: 0, fontSize: "1.5rem" }}>📅 Calendar</h1>
            <p className="muted">
              {MONTH_NAMES[currentDate.getMonth()]} {currentDate.getFullYear()}
            </p>
          </div>
          <div style={{ display: "flex", gap: "0.5rem", alignItems: "center" }}>
            <select
              value={filter}
              onChange={(e) => setFilter(e.target.value as "all" | "registered")}
              className="event-filter-select"
              style={{ width: "auto" }}
            >
              <option value="all">All Events</option>
              <option value="registered">My Registrations</option>
            </select>
          </div>
        </div>

        {/* ── Month Navigation ── */}
        <div style={{ display: "flex", alignItems: "center", justifyContent: "center", gap: "1rem", marginBottom: "1.25rem" }}>
          <button className="btn btn-secondary btn-small" onClick={goToPrevMonth}>
            ← Prev
          </button>
          <button className="btn btn-secondary btn-small" onClick={goToToday}>
            Today
          </button>
          <button className="btn btn-secondary btn-small" onClick={goToNextMonth}>
            Next →
          </button>
        </div>

        {error && (
          <div className="error-state" style={{ marginBottom: "1rem" }}>
            <p>{error}</p>
            <button className="btn btn-secondary btn-small" onClick={fetchEvents}>Retry</button>
          </div>
        )}

        {/* ── Calendar Grid ── */}
        <div className="calendar-grid" style={{
          display: "grid",
          gridTemplateColumns: "repeat(7, 1fr)",
          gap: "1px",
          background: "var(--border, #e5e7eb)",
          borderRadius: 8,
          overflow: "hidden",
          marginBottom: "2rem",
        }}>
          {/* Weekday headers */}
          {WEEKDAYS.map((day) => (
            <div key={day} style={{
              background: "var(--sidebar-bg, #0d0129)",
              color: "#fff",
              padding: "0.5rem",
              textAlign: "center",
              fontSize: "0.8rem",
              fontWeight: 600,
              textTransform: "uppercase",
              letterSpacing: "0.05em",
            }}>
              {day}
            </div>
          ))}

          {/* Day cells */}
          {gridDays.map((day, i) => {
            const dateKey = formatDateParam(day);
            const dayEvents = filteredEventsByDate.get(dateKey) ?? [];
            const inMonth = day.getMonth() === currentDate.getMonth();
            const today = isToday(day);

            return (
              <div
                key={i}
                style={{
                  background: inMonth ? "var(--card, #fff)" : "var(--bg, #f9fafb)",
                  minHeight: 100,
                  padding: "0.35rem",
                  opacity: inMonth ? 1 : 0.5,
                  position: "relative",
                }}
              >
                {/* Day number */}
                <div style={{
                  display: "flex",
                  justifyContent: "space-between",
                  alignItems: "center",
                  marginBottom: "0.25rem",
                }}>
                  <span style={{
                    fontSize: "0.8rem",
                    fontWeight: today ? 700 : 500,
                    color: today ? "var(--primary, #6366f1)" : "var(--text, #1f2937)",
                    background: today ? "rgba(99,102,241,0.12)" : "transparent",
                    borderRadius: "50%",
                    width: 24,
                    height: 24,
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                  }}>
                    {day.getDate()}
                  </span>
                </div>

                {/* Event chips */}
                <div style={{ display: "flex", flexDirection: "column", gap: 2 }}>
                  {dayEvents.slice(0, 3).map((evt) => (
                    <Link
                      key={evt.id}
                      to={`/events/${evt.id}`}
                      style={{
                        display: "block",
                        padding: "2px 5px",
                        borderRadius: 4,
                        fontSize: "0.65rem",
                        lineHeight: 1.3,
                        background: evt.isRegistered
                          ? "rgba(34,197,94,0.15)"
                          : "rgba(99,102,241,0.1)",
                        color: evt.isRegistered
                          ? "var(--success, #16a34a)"
                          : "var(--primary, #6366f1)",
                        textDecoration: "none",
                        fontWeight: 500,
                        overflow: "hidden",
                        textOverflow: "ellipsis",
                        whiteSpace: "nowrap",
                        borderLeft: `2px solid ${evt.isRegistered ? "var(--success, #16a34a)" : "var(--primary, #6366f1)"}`,
                      }}
                      title={`${evt.title} — ${formatTime(evt.eventTime)}`}
                    >
                      {formatTime(evt.eventTime)} {evt.title}
                    </Link>
                  ))}
                  {dayEvents.length > 3 && (
                    <span style={{ fontSize: "0.6rem", color: "var(--text-secondary, #6b7280)", paddingLeft: 5 }}>
                      +{dayEvents.length - 3} more
                    </span>
                  )}
                </div>
              </div>
            );
          })}
        </div>

        {/* ── Upcoming Events ── */}
        <div style={{ marginBottom: "2rem" }}>
          <h2 style={{ fontSize: "1.1rem", marginBottom: "0.75rem" }}>Upcoming Events</h2>
          {loading ? (
            <div className="loading-state">Loading events...</div>
          ) : upcomingEvents.length === 0 ? (
            <div className="empty-state">
              <p>No upcoming events this month.</p>
            </div>
          ) : (
            <div style={{ display: "flex", flexDirection: "column", gap: "0.5rem" }}>
              {upcomingEvents.map((evt) => (
                <Link
                  key={evt.id}
                  to={`/events/${evt.id}`}
                  style={{
                    display: "flex",
                    alignItems: "center",
                    gap: "1rem",
                    padding: "0.75rem 1rem",
                    background: "var(--card, #fff)",
                    borderRadius: 8,
                    textDecoration: "none",
                    color: "inherit",
                    border: "1px solid var(--border, #e5e7eb)",
                  }}
                >
                  {evt.imageUrl && (
                    <img
                      src={evt.imageUrl}
                      alt={evt.title}
                      style={{ width: 48, height: 48, borderRadius: 6, objectFit: "cover" }}
                    />
                  )}
                  <div style={{ flex: 1, minWidth: 0 }}>
                    <div style={{ fontWeight: 600, fontSize: "0.9rem", marginBottom: 2 }}>
                      {evt.title}
                    </div>
                    <div style={{ fontSize: "0.78rem", color: "var(--text-secondary, #6b7280)" }}>
                      {new Date(evt.eventDate).toLocaleDateString("en-US", {
                        weekday: "short", month: "short", day: "numeric",
                      })} at {formatTime(evt.eventTime)}
                      {evt.venue && ` · ${evt.venue}`}
                    </div>
                  </div>
                  <div style={{ display: "flex", flexDirection: "column", alignItems: "flex-end", gap: 2 }}>
                    <span style={{
                      fontSize: "0.7rem",
                      padding: "2px 8px",
                      borderRadius: 12,
                      background: "rgba(99,102,241,0.1)",
                      color: "var(--primary, #6366f1)",
                      fontWeight: 500,
                    }}>
                      {evt.categoryName}
                    </span>
                    {evt.isRegistered && (
                      <span style={{
                        fontSize: "0.65rem",
                        padding: "2px 8px",
                        borderRadius: 12,
                        background: "rgba(34,197,94,0.12)",
                        color: "var(--success, #16a34a)",
                        fontWeight: 500,
                      }}>
                        ✓ Registered
                      </span>
                    )}
                  </div>
                </Link>
              ))}
            </div>
          )}
        </div>

        {/* ── Past Events ── */}
        {pastEvents.length > 0 && (
          <div>
            <h2 style={{ fontSize: "1.1rem", marginBottom: "0.75rem", opacity: 0.7 }}>Past Events</h2>
            <div style={{ display: "flex", flexDirection: "column", gap: "0.5rem" }}>
              {pastEvents.map((evt) => (
                <Link
                  key={evt.id}
                  to={`/events/${evt.id}`}
                  style={{
                    display: "flex",
                    alignItems: "center",
                    gap: "1rem",
                    padding: "0.6rem 1rem",
                    background: "var(--card, #fff)",
                    borderRadius: 8,
                    textDecoration: "none",
                    color: "inherit",
                    border: "1px solid var(--border, #e5e7eb)",
                    opacity: 0.7,
                  }}
                >
                  <div style={{ flex: 1, minWidth: 0 }}>
                    <div style={{ fontWeight: 500, fontSize: "0.85rem" }}>{evt.title}</div>
                    <div style={{ fontSize: "0.75rem", color: "var(--text-secondary, #6b7280)" }}>
                      {new Date(evt.eventDate).toLocaleDateString("en-US", {
                        weekday: "short", month: "short", day: "numeric",
                      })} · {evt.venue ?? "No venue"}
                    </div>
                  </div>
                </Link>
              ))}
            </div>
          </div>
        )}
      </main>
    </div>
  );
}
