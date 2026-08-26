import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { getEvents, getCategories } from "../api/client";
import type { EventSummary, EventCategory } from "../types";
import { useAuth } from "../auth/AuthContext";

export function EventDiscovery() {
  const { user } = useAuth();
  const [events, setEvents] = useState<EventSummary[]>([]);
  const [categories, setCategories] = useState<EventCategory[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [categoryId, setCategoryId] = useState<number | "">("");
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [total, setTotal] = useState(0);
  const [debouncedSearch, setDebouncedSearch] = useState(search);

  useEffect(() => {
    const t = setTimeout(() => setDebouncedSearch(search), 300);
    return () => clearTimeout(t);
  }, [search]);

  useEffect(() => {
    getCategories().then(setCategories).catch(() => {});
  }, []);

  useEffect(() => {
    setLoading(true);
    getEvents({
      search: debouncedSearch || undefined,
      categoryId: categoryId !== "" ? categoryId : undefined,
      page,
      pageSize: 12,
    })
      .then((res) => {
        setEvents(res.events);
        setTotalPages(res.totalPages);
        setTotal(res.total);
      })
      .catch(() => setEvents([]))
      .finally(() => setLoading(false));
  }, [debouncedSearch, categoryId, page]);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setPage(1);
  };

  return (
    <div className="event-discovery">
      <header className="dash-header">
        <div>
          <h1 style={{ margin: 0, fontSize: "1.5rem" }}>Events</h1>
          <p className="muted">{total} event{total !== 1 ? "s" : ""} found</p>
        </div>
        <div style={{ display: "flex", gap: "0.5rem" }}>
          {user?.roles.includes("Organizer") && (
            <Link to="/organizer/events/create" className="btn btn-small">
              + Create Event
            </Link>
          )}
          <Link to="/dashboard" className="btn btn-secondary btn-small">
            Dashboard
          </Link>
        </div>
      </header>

      <form onSubmit={handleSearch} className="event-filters">
        <input
          type="text"
          placeholder="Search events..."
          value={search}
          onChange={(e) => { setSearch(e.target.value); setPage(1); }}
          className="event-search-input"
        />
        <select
          value={categoryId}
          onChange={(e) => { setCategoryId(e.target.value ? Number(e.target.value) : ""); setPage(1); }}
          className="event-filter-select"
        >
          <option value="">All Categories</option>
          {categories.map((c) => (
            <option key={c.id} value={c.id}>{c.name} ({c.eventCount})</option>
          ))}
        </select>
      </form>

      {loading ? (
        <div className="loading-state">Loading events...</div>
      ) : events.length === 0 ? (
        <div className="empty-state">
          <p>No events found.</p>
        </div>
      ) : (
        <>
          <div className="event-grid">
            {events.map((evt) => (
              <Link to={`/events/${evt.id}`} key={evt.id} className="event-card">
                <div className="event-card-image">
                  {evt.imageUrl ? (
                    <img src={evt.imageUrl} alt={evt.title} />
                  ) : (
                    <div className="event-card-placeholder">{evt.categoryName}</div>
                  )}
                </div>
                <div className="event-card-body">
                  <span className="event-card-category">{evt.categoryName}</span>
                  <h3 className="event-card-title">{evt.title}</h3>
                  <div className="event-card-meta">
                    <span>{new Date(evt.eventDate).toLocaleDateString()}</span>
                    {evt.venue && <span>• {evt.venue}</span>}
                  </div>
                  <div className="event-card-footer">
                    <span className="event-card-slots">
                      {evt.registeredCount}/{evt.maxParticipants} registered
                    </span>
                    <span className={`status-badge status-${evt.status.toLowerCase()}`}>
                      {evt.status}
                    </span>
                  </div>
                </div>
              </Link>
            ))}
          </div>

          {totalPages > 1 && (
            <div className="pagination">
              <button
                className="btn btn-secondary btn-small"
                disabled={page <= 1}
                onClick={() => setPage((p) => p - 1)}
              >
                Previous
              </button>
              <span className="muted">Page {page} of {totalPages}</span>
              <button
                className="btn btn-secondary btn-small"
                disabled={page >= totalPages}
                onClick={() => setPage((p) => p + 1)}
              >
                Next
              </button>
            </div>
          )}
        </>
      )}
    </div>
  );
}
