import { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { useToast } from "../components/Toast";
import { getEvent, updateEvent, publishEvent, getCategories, uploadImage } from "../api/client";
import type { EventCategory } from "../types";

export function EditEvent() {
  const { id } = useParams<{ id: string }>();
  const { addToast } = useToast();
  const navigate = useNavigate();
  const [categories, setCategories] = useState<EventCategory[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [eventDate, setEventDate] = useState("");
  const [eventTime, setEventTime] = useState("");
  const [venue, setVenue] = useState("");
  const [maxParticipants, setMaxParticipants] = useState("");
  const [imageUrl, setImageUrl] = useState("");
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [imagePreview, setImagePreview] = useState<string | null>(null);
  const [uploading, setUploading] = useState(false);
  const [registrationDeadline, setRegistrationDeadline] = useState("");
  const [status, setStatus] = useState("");
  const [isPaid, setIsPaid] = useState(false);
  const [price, setPrice] = useState("");

  const eventId = Number(id);

  useEffect(() => {
    Promise.all([getEvent(eventId).catch(() => null), getCategories().catch(() => [])])
      .then(([evt, cats]) => {
        if (!evt) { addToast("error", "Event not found."); navigate("/organizer/events"); return; }
        setCategories(cats);
        setTitle(evt.title);
        setDescription(evt.description ?? "");
        setCategoryId(evt.categoryId.toString());
        setEventDate(evt.eventDate.split("T")[0]);
        setEventTime(evt.eventTime);
        setVenue(evt.venue ?? "");
        setMaxParticipants(evt.maxParticipants.toString());
        setImageUrl(evt.imageUrl ?? "");
        setRegistrationDeadline(evt.registrationDeadline ? evt.registrationDeadline.slice(0, 16) : "");
        setStatus(evt.status);
        setIsPaid(evt.isPaid);
        setPrice(evt.isPaid ? evt.price.toString() : "");
      })
      .finally(() => setLoading(false));
  }, [eventId]);

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    try {
      let finalImageUrl = imageUrl.trim() || undefined;
      if (imageFile) {
        setUploading(true);
        finalImageUrl = await uploadImage(imageFile);
        setUploading(false);
      }
      await updateEvent(eventId, {
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
      });
      addToast("success", "Event updated.");
    } catch (e: unknown) {
      const msg = e instanceof Error ? e.message : "Failed to update event.";
      addToast("error", msg);
    } finally {
      setSaving(false);
    }
  };

  const handlePublish = async () => {
    setSaving(true);
    try {
      await publishEvent(eventId);
      addToast("success", "Event submitted for approval.");
      setStatus("PendingApproval");
    } catch (e: unknown) {
      const msg = e instanceof Error ? e.message : "Failed to publish.";
      addToast("error", msg);
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <div className="loading-state">Loading event...</div>;

  return (
    <div className="event-form-page">
      <div className="dash-header">
        <h1 style={{ margin: 0, fontSize: "1.5rem" }}>Edit Event</h1>
        <div style={{ display: "flex", gap: "0.5rem" }}>
          <span className={`status-badge status-${status.toLowerCase()}`}>{status}</span>
          <Link to="/organizer/events" className="btn btn-secondary btn-small">Back</Link>
        </div>
      </div>

      <form className="card event-form" onSubmit={handleSave}>
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
        {imageUrl && !imagePreview && (
          <div className="image-current">
            <img src={imageUrl} alt="Current" className="image-preview" />
            <span className="muted">Current image</span>
          </div>
        )}
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
          <button type="submit" className="btn btn-primary" disabled={saving} style={{ width: "auto", marginTop: 0 }}>
            {saving ? (uploading ? "Uploading..." : "Saving...") : "Save Changes"}
          </button>
          {status === "Draft" && (
            <button type="button" className="btn btn-small" disabled={saving} onClick={handlePublish} style={{ width: "auto", marginTop: 0 }}>
              {saving ? "Publishing..." : "Submit for Approval"}
            </button>
          )}
        </div>
      </form>
    </div>
  );
}
