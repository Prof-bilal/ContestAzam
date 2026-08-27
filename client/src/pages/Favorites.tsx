import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { LogoutButton } from "../components/LogoutButton";
import { getMyFavorites, removeFavorite } from "../api/client";
import { useToast } from "../components/Toast";
import type { FavoriteDto } from "../types";

export function Favorites() {
  const { user } = useAuth();
  const { addToast } = useToast();
  const [favorites, setFavorites] = useState<FavoriteDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getMyFavorites()
      .then(setFavorites)
      .catch(() => {})
      .finally(() => setLoading(false));
  }, []);

  const handleRemove = async (eventId: number) => {
    try {
      await removeFavorite(eventId);
      setFavorites((prev) => prev.filter((f) => f.eventId !== eventId));
      addToast("success", "Bookmark removed.");
    } catch {
      addToast("error", "Failed to remove bookmark.");
    }
  };

  const highestRole = ["Admin", "Organizer", "Participant"].find((r) =>
    user?.roles.includes(r),
  ) || "Visitor";

  if (loading) return <div className="loading-state">Loading favorites...</div>;

  return (
    <div className="admin-layout">
      <aside className="admin-sidebar">
        <div className="admin-brand">EventSphere</div>
        <div className="sidebar-welcome">
          Welcome, <strong>{user?.name}</strong>
          <span className="role-badge" style={{ marginLeft: "0.5rem", fontSize: "11px" }}>{highestRole}</span>
        </div>
        <nav className="admin-nav">
          <Link to="/dashboard" className="admin-nav-item">Dashboard</Link>
          <Link to="/events" className="admin-nav-item">Browse Events</Link>
          <Link to="/my-registrations" className="admin-nav-item">My Registrations</Link>
          <Link to="/favorites" className="admin-nav-item active">Favorites</Link>
        </nav>
        <LogoutButton style={{ marginTop: "auto" }} />
      </aside>
      <main className="admin-main">
        <div className="admin-header">
          <h1 style={{ margin: 0, fontSize: "1.5rem" }}>My Favorites</h1>
          <p className="muted">{favorites.length} bookmarked event{favorites.length !== 1 ? "s" : ""}</p>
        </div>

        {favorites.length === 0 ? (
          <div className="empty-state">
            <p>No bookmarked events yet.</p>
            <Link to="/events" className="btn btn-primary btn-small" style={{ width: "auto" }}>Browse Events</Link>
          </div>
        ) : (
          <div className="admin-table-wrapper">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Event</th>
                  <th>Category</th>
                  <th>Date</th>
                  <th>Venue</th>
                  <th>Bookmarked</th>
                  <th>Action</th>
                </tr>
              </thead>
              <tbody>
                {favorites.map((f) => (
                  <tr key={f.eventId}>
                    <td><Link to={`/events/${f.eventId}`}>{f.eventTitle}</Link></td>
                    <td>{f.categoryName}</td>
                    <td>{new Date(f.eventDate).toLocaleDateString()}</td>
                    <td>{f.eventVenue ?? "TBA"}</td>
                    <td>{new Date(f.bookmarkedOn).toLocaleDateString()}</td>
                    <td>
                      <button className="btn btn-small" onClick={() => handleRemove(f.eventId)} style={{ width: "auto", marginTop: 0 }}>
                        Remove
                      </button>
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
