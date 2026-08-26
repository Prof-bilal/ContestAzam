import { useSearchParams, Link } from "react-router-dom";

export function PaymentCancel() {
  const [searchParams] = useSearchParams();
  const eventId = searchParams.get("eventId");

  return (
    <div className="payment-result-page">
      <div className="card" style={{ maxWidth: 500, margin: "2rem auto", textAlign: "center" }}>
        <div style={{ fontSize: "3rem", marginBottom: "1rem" }}>&#10007;</div>
        <h2 style={{ margin: "0 0 0.5rem" }}>Payment Cancelled</h2>
        <p className="muted" style={{ marginBottom: "1.5rem" }}>
          Your payment was not completed. You have not been charged.
        </p>
        <div style={{ display: "flex", gap: "0.5rem", justifyContent: "center" }}>
          {eventId && (
            <Link to={`/events/${eventId}`} className="btn btn-primary" style={{ width: "auto", marginTop: 0 }}>
              Try Again
            </Link>
          )}
          <Link to="/events" className="btn btn-secondary" style={{ width: "auto", marginTop: 0 }}>
            Browse Events
          </Link>
        </div>
      </div>
    </div>
  );
}
