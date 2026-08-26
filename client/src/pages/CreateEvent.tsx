import { useEffect, useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { useToast } from "../components/Toast";
import { getCategories, createEvent, uploadImage } from "../api/client";
import type { EventCategory } from "../types";

export function CreateEvent() {
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

  return (
    <div className="event-form-page">
      <div className="dash-header">
        <h1 style={{ margin: 0, fontSize: "1.5rem" }}>Create Event</h1>
        <Link to="/organizer/events" className="btn btn-secondary btn-small">Back to Events</Link>
      </div>

      <form className="card event-form" onSubmit={(e) => handleSubmit(e, false)}>
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

        <div style={{ display: "flex", gap: "0.5rem", marginTop: "1rem" }}>
          <button type="submit" className="btn btn-primary" disabled={loading} style={{ width: "auto", marginTop: 0 }}>
            {loading ? (uploading ? "Uploading..." : "Creating...") : "Submit for Approval"}
          </button>
          <button type="button" className="btn btn-secondary" disabled={loading} onClick={(e) => handleSubmit(e as unknown as React.FormEvent, true)}>
            Save as Draft
          </button>
        </div>
      </form>
    </div>
  );
}
