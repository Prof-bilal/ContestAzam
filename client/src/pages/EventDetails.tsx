import { useEffect, useState } from "react";
import { useParams, Link, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { useToast } from "../components/Toast";
import {
  getEvent, getEventReviews, registerForEvent,
  submitReview, createCheckoutSession,
} from "../api/client";
import type { EventSummary, EventReviewSummary } from "../types";

export function EventDetails() {
  const { id } = useParams<{ id: string }>();
  const { user } = useAuth();
  const { addToast } = useToast();
  const navigate = useNavigate();

  const [event, setEvent] = useState<EventSummary | null>(null);
  const [reviews, setReviews] = useState<EventReviewSummary | null>(null);
  const [loading, setLoading] = useState(true);
  const [registering, setRegistering] = useState(false);
  const [showRegModal, setShowRegModal] = useState(false);
  const [reviewRating, setReviewRating] = useState(5);
  const [reviewComment, setReviewComment] = useState("");
  const [submittingReview, setSubmittingReview] = useState(false);

  const eventId = Number(id);

  const fetchEvent = () => {
    return getEvent(eventId).then((evt) => {
      setEvent(evt);
      return evt;
    });
  };

  useEffect(() => {
    if (!eventId) return;
    setLoading(true);
    Promise.all([
      fetchEvent().catch(() => null),
      getEventReviews(eventId).catch(() => null),
    ])
      .then(([, rev]) => { setReviews(rev); })
      .finally(() => setLoading(false));
  }, [eventId]);

  const handleRegister = async () => {
    if (!event) return;
    setRegistering(true);
    try {
      if (event.isPaid) {
        const { url } = await createCheckoutSession(eventId);
        window.location.href = url;
        return;
      }
      await registerForEvent(eventId);
      addToast("success", "Successfully registered for the event!");
      setShowRegModal(false);
      const updated = await fetchEvent();
      setEvent(updated);
    } catch (e: unknown) {
      const msg = e instanceof Error ? e.message : "Registration failed.";
      addToast("error", msg);
    } finally {
      setRegistering(false);
    }
  };

  const handleSubmitReview = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmittingReview(true);
    try {
      await submitReview(eventId, reviewRating, reviewComment || undefined);
      addToast("success", "Review submitted!");
      setReviewComment("");
      setReviewRating(5);
      const rev = await getEventReviews(eventId);
      setReviews(rev);
    } catch (e: unknown) {
      const msg = e instanceof Error ? e.message : "Failed to submit review.";
      addToast("error", msg);
    } finally {
      setSubmittingReview(false);
    }
  };

  if (loading) return <div className="loading-state">Loading event...</div>;
  if (!event) return <div className="empty-state">Event not found.</div>;

  const isOrganizer = user?.id === event.organizerId;
  const isPast = new Date(event.eventDate) < new Date();
  const isDeadlinePassed = event.registrationDeadline
    ? new Date(event.registrationDeadline) < new Date()
    : false;
  const spotsLeft = event.maxParticipants - event.registeredCount;
  const isFull = spotsLeft <= 0;
  const isRegistered = event.isRegistered;
  const canRegister = user && !isOrganizer && !isRegistered && event.status === "Approved" && !isPast && !isFull && !isDeadlinePassed;

  return (
    <div className="event-details-page">
      <button onClick={() => navigate(-1)} className="btn btn-secondary btn-small" style={{ marginBottom: "1rem" }}>
        &larr; Back
      </button>

      <div className="event-detail-card">
        {event.imageUrl && (
          <img src={event.imageUrl} alt={event.title} className="event-detail-image" />
        )}

        <div className="event-detail-header">
          <div>
            <span className="event-card-category">{event.categoryName}</span>
            <h1 style={{ margin: "0.5rem 0 0" }}>{event.title}</h1>
            <p className="muted">by {event.organizerName}</p>
          </div>
          <span className={`status-badge status-${event.status.toLowerCase()}`}>{event.status}</span>
        </div>

        <div className="event-detail-info">
          <div className="event-info-item">
            <span className="event-info-label">Date</span>
            <span>{new Date(event.eventDate).toLocaleDateString()}</span>
          </div>
          <div className="event-info-item">
            <span className="event-info-label">Time</span>
            <span>{event.eventTime}</span>
          </div>
          {event.venue && (
            <div className="event-info-item">
              <span className="event-info-label">Venue</span>
              <span>{event.venue}</span>
            </div>
          )}
          <div className="event-info-item">
            <span className="event-info-label">Slots</span>
            <span>{event.registeredCount}/{event.maxParticipants} ({spotsLeft} left)</span>
          </div>
          {event.registrationDeadline && (
            <div className="event-info-item">
              <span className="event-info-label">Registration Deadline</span>
              <span>{new Date(event.registrationDeadline).toLocaleString()}</span>
            </div>
          )}
          <div className="event-info-item">
            <span className="event-info-label">Price</span>
            <span className={event.isPaid ? "event-price-paid" : "event-price-free"}>
              {event.isPaid ? `$${event.price.toFixed(2)}` : "Free"}
            </span>
          </div>
        </div>

        {event.description && (
          <div className="event-detail-description">
            <h3>About this event</h3>
            <p>{event.description}</p>
          </div>
        )}

        <div className="event-detail-actions">
          {isOrganizer && (
            <Link to={`/organizer/events/${eventId}/edit`} className="btn btn-secondary btn-small">
              Edit Event
            </Link>
          )}

          {isRegistered && !isOrganizer && (
            <span className="role-badge" style={{ background: "var(--ok)", color: "#fff" }}>Registered</span>
          )}

          {!user && event.status === "Approved" && (
            <Link to="/login" className="btn btn-primary" style={{ width: "auto", marginTop: 0 }}>
              Login to Register
            </Link>
          )}

          {user && !isOrganizer && !isRegistered && event.status === "Approved" && isPast && (
            <span className="muted">Event has ended — registration closed</span>
          )}

          {user && !isOrganizer && !isRegistered && event.status === "Approved" && !isPast && isFull && (
            <span className="muted">Event is full</span>
          )}

          {user && !isOrganizer && !isRegistered && event.status === "Approved" && !isPast && isDeadlinePassed && (
            <span className="muted">Registration deadline has passed</span>
          )}

          {canRegister && (
            <button className="btn btn-primary" onClick={() => setShowRegModal(true)} style={{ width: "auto", marginTop: 0 }}>
              {event.isPaid ? `Pay $${event.price.toFixed(2)} to Register` : "Register for Event"}
            </button>
          )}

          {user && !isOrganizer && (
            <Link to="/my-registrations" className="btn btn-secondary btn-small">
              My Registrations
            </Link>
          )}
        </div>
      </div>

      {/* Reviews Section */}
      <div className="event-reviews-section">
        <h2>Reviews {reviews ? `(${reviews.totalReviews})` : ""}</h2>
        {reviews && reviews.averageRating > 0 && (
          <div className="review-summary">
            <span className="review-average">{reviews.averageRating}</span>
            <span className="muted">/ 5 average</span>
          </div>
        )}

        {/* Review form — only when registered AND event past */}
        {user && !isOrganizer && (
          <>
            {isRegistered && isPast && (
              <form onSubmit={handleSubmitReview} className="review-form">
                <label className="muted">Your Rating</label>
                <select value={reviewRating} onChange={(e) => setReviewRating(Number(e.target.value))}>
                  {[5, 4, 3, 2, 1].map((r) => (
                    <option key={r} value={r}>{r} star{r !== 1 ? "s" : ""}</option>
                  ))}
                </select>
                <textarea
                  placeholder="Leave a comment (optional)"
                  value={reviewComment}
                  onChange={(e) => setReviewComment(e.target.value)}
                  rows={3}
                />
                <button className="btn btn-small" type="submit" disabled={submittingReview}>
                  {submittingReview ? "Submitting..." : "Submit Review"}
                </button>
              </form>
            )}

            {!isRegistered && !isPast && (
              <div className="review-locked-msg muted">
                Register for this event to leave a review after it ends.
              </div>
            )}

            {!isRegistered && isPast && (
              <div className="review-locked-msg muted">
                Only registered attendees can leave reviews.
              </div>
            )}

            {isRegistered && !isPast && (
              <div className="review-locked-msg muted">
                Reviews will open after the event ends.
              </div>
            )}
          </>
        )}

        <div className="reviews-list">
          {reviews?.reviews.map((r) => (
            <div key={r.id} className="review-item">
              <div className="review-header">
                <strong>{r.userName}</strong>
                <span className="muted">{new Date(r.submittedOn).toLocaleDateString()}</span>
              </div>
              <div className="review-rating">{"★".repeat(r.rating)}{"☆".repeat(5 - r.rating)}</div>
              {r.comment && <p className="review-comment">{r.comment}</p>}
            </div>
          ))}
          {reviews && reviews.reviews.length === 0 && (
            <p className="muted">No reviews yet.</p>
          )}
        </div>
      </div>

      {/* Registration Confirmation Modal */}
      {showRegModal && (
        <div className="modal-overlay" onClick={() => setShowRegModal(false)}>
          <div className="modal card" onClick={(e) => e.stopPropagation()}>
            <h3>Register for Event</h3>
            <p>Are you sure you want to register for <strong>{event.title}</strong>?</p>
            <div style={{ fontSize: "0.9rem", color: "var(--muted)", marginBottom: "1rem" }}>
              {event.venue && <div>Venue: {event.venue}</div>}
              <div>Date: {new Date(event.eventDate).toLocaleDateString()}</div>
              <div>Time: {event.eventTime}</div>
              {event.isPaid && <div style={{ fontWeight: 600, color: "var(--primary)" }}>Price: ${event.price.toFixed(2)}</div>}
              {spotsLeft <= 5 && <div style={{ color: "var(--danger)" }}>Only {spotsLeft} spot(s) left!</div>}
            </div>
            <div style={{ display: "flex", gap: "0.5rem" }}>
              <button className="btn btn-primary btn-small" onClick={handleRegister} disabled={registering} style={{ width: "auto", marginTop: 0 }}>
                {registering ? "Processing..." : event.isPaid ? `Pay $${event.price.toFixed(2)}` : "Confirm Registration"}
              </button>
              <button className="btn btn-secondary btn-small" onClick={() => setShowRegModal(false)}>
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
