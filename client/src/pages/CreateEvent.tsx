import { useEffect, useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { useToast } from "../components/Toast";
import { getCategories, createEvent, uploadImage } from "../api/client";
import type { EventCategory } from "../types";

export function CreateEvent() {
  const { user, logout } = useAuth();
  const { addToast } = useToast();
  const navigate = useNavigate();
  const [categories, setCategories] = useState<EventCategory[]>([]);
  const [loading, setLoading] = useState(false);

  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [eventDate, setEventDate] = useState("");
  const [eventTime, setEventTime] = useState("");
  const [venue, setVenue] = useState("");
  const [maxParticipants, setMaxParticipants] = useState("");
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [imagePreview, setImagePreview] = useState<string | null>(null);
  const [uploading, setUploading] = useState(false);
  const [registrationDeadline, setRegistrationDeadline] = useState("");
  const [isPaid, setIsPaid] = useState(false);
  const [price, setPrice] = useState("");

  useEffect(() => {
    getCategories().then(setCategories).catch(() => {});
  }, []);

  const handleSubmit = async (e: React.FormEvent, saveAsDraft: boolean) => {
    e.preventDefault();
    if (!title.trim() || !categoryId || !eventDate || !eventTime || !maxParticipants) {
      addToast("error", "Please fill in all required fields.");
      return;
    }
    setLoading(true);
    try {
      let finalImageUrl: string | undefined;
      if (imageFile) {
        setUploading(true);
        finalImageUrl = await uploadImage(imageFile);
        setUploading(false);
      }
      const evt = await createEvent({
        title: title.trim(),
        description: description.trim() || undefined,
        categoryId: Number(categoryId),
        eventDate,
        eventTime,
        venue: venue.trim() || undefined,
        maxParticipants: Number(maxParticipants),
        imageUrl: finalImageUrl,
        registrationDeadline: registrationDeadline || undefined,
        isPaid,
        price: isPaid ? Number(price) : 0,
        saveAsDraft,
      });
      addToast("success", saveAsDraft ? "Event saved as draft." : "Event submitted for approval.");
      navigate(`/events/${evt.id}`);
    } catch (e: unknown) {
      const msg = e instanceof Error ? e.message : "Failed to create event.";
      addToast("error", msg);
    } finally {
      setLoading(false);
    }
  };

  const highestRole = ["Admin", "Organizer", "Participant"].find((r) =>
    user?.roles.includes(r),
  ) || "Visitor";

  const onLogout = async () => {
    await logout();
    addToast("info", "You have been signed out.");
    navigate("/");
  };

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
          <Link to="/organizer/categories" className="admin-nav-item">Categories</Link>
          <Link to="/events" className="admin-nav-item">Browse Events</Link>
          <Link to="/profile" className="admin-nav-item">Profile</Link>
        </nav>
        <button className="btn btn-secondary" onClick={onLogout} style={{ marginTop: "auto" }}>
          Logout
        </button>
      </aside>

      <main className="admin-main">
        <div className="admin-header">
          <div style={{ display: "flex", alignItems: "center", gap: "0.75rem" }}>
            <Link to="/organizer/events" className="btn btn-secondary btn-small" style={{ width: "auto" }}>&larr;</Link>
            <h1 style={{ margin: 0, fontSize: "1.5rem" }}>Create Event</h1>
          </div>
        </div>

        <form className="card event-form" style={{ maxWidth: "none" }} onSubmit={(e) => handleSubmit(e, false)}>
          <label>Title *</label>
          <input value={title} onChange={(e) => setTitle(e.target.value)} required maxLength={150} />

          <label>Description</label>
          <textarea value={description} onChange={(e) => setDescription(e.target.value)} rows={4} maxLength={2000} />

          <label>Category *</label>
          <select value={categoryId} onChange={(e) => setCategoryId(e.target.value)} required>
            <option value="">Select category</option>
            {categories.map((c) => (
              <option key={c.id} value={c.id}>{c.name}</option>
            ))}
          </select>

          <label>Event Date *</label>
          <input type="date" value={eventDate} onChange={(e) => setEventDate(e.target.value)} required />

          <label>Event Time *</label>
          <input type="time" value={eventTime} onChange={(e) => setEventTime(e.target.value)} required />

          <label>Venue</label>
          <input value={venue} onChange={(e) => setVenue(e.target.value)} maxLength={100} />

          <label>Max Participants *</label>
          <input type="number" min={1} value={maxParticipants} onChange={(e) => setMaxParticipants(e.target.value)} required />

          <label>Event Image</label>
          <div className="image-upload-area">
            <input
              type="file"
              accept="image/*"
              onChange={(e) => {
                const file = e.target.files?.[0] ?? null;
                if (imagePreview) URL.revokeObjectURL(imagePreview);
                setImageFile(file);
                if (file) {
                  setImagePreview(URL.createObjectURL(file));
                } else {
                  setImagePreview(null);
                }
              }}
            />
            {imagePreview && (
              <img src={imagePreview} alt="Preview" className="image-preview" />
            )}
          </div>

          <label>Registration Deadline</label>
          <input type="datetime-local" value={registrationDeadline} onChange={(e) => setRegistrationDeadline(e.target.value)} />

          <label>Event Type</label>
          <select value={isPaid ? "paid" : "free"} onChange={(e) => setIsPaid(e.target.value === "paid")}>
            <option value="free">Free</option>
            <option value="paid">Paid</option>
          </select>

          {isPaid && (
            <>
              <label>Price ($) *</label>
              <input
                type="number"
                step="0.01"
                min="0.01"
                value={price}
                onChange={(e) => setPrice(e.target.value)}
                placeholder="0.00"
                required={isPaid}
              />
            </>
          )}

          <div style={{ display: "flex", gap: "0.5rem", marginTop: "1rem" }}>
            <button type="submit" className="btn btn-primary" disabled={loading} style={{ width: "auto", marginTop: 0 }}>
              {loading ? (uploading ? "Uploading..." : "Creating...") : "Submit for Approval"}
            </button>
            <button type="button" className="btn btn-secondary" disabled={loading} onClick={(e) => handleSubmit(e as unknown as React.FormEvent, true)}>
              Save as Draft
            </button>
          </div>
        </form>
      </main>
    </div>
  );
}
