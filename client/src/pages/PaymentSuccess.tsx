import { useEffect, useState } from "react";
import { useSearchParams, Link } from "react-router-dom";
import { getPaymentStatus } from "../api/client";
import type { PaymentStatus } from "../types";

export function PaymentSuccess() {
  const [searchParams] = useSearchParams();
  const eventId = Number(searchParams.get("eventId"));
  const [payment, setPayment] = useState<PaymentStatus | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!eventId) return;
    const poll = async () => {
      for (let i = 0; i < 10; i++) {
        try {
          const status = await getPaymentStatus(eventId);
          if (status?.status === "Succeeded") {
            setPayment(status);
            setLoading(false);
            return;
          }
        } catch {
          // ignore
        }
        await new Promise((r) => setTimeout(r, 1500));
      }
      setLoading(false);
    };
    poll();
  }, [eventId]);

  return (
    <div className="payment-result-page">
      <div className="card" style={{ maxWidth: 500, margin: "2rem auto", textAlign: "center" }}>
        {loading ? (
          <>
            <div className="loading-state">Confirming payment...</div>
          </>
        ) : payment ? (
          <>
            <div style={{ fontSize: "3rem", marginBottom: "1rem" }}>&#10003;</div>
            <h2 style={{ margin: "0 0 0.5rem" }}>Payment Successful!</h2>
            <p className="muted" style={{ marginBottom: "1.5rem" }}>
              Your payment of <strong>${payment.amount.toFixed(2)}</strong> has been confirmed.
            </p>
            <div style={{ display: "flex", gap: "0.5rem", justifyContent: "center" }}>
              <Link to="/my-registrations" className="btn btn-primary" style={{ width: "auto", marginTop: 0 }}>
                View My Registrations
              </Link>
              <Link to={`/events/${eventId}`} className="btn btn-secondary" style={{ width: "auto", marginTop: 0 }}>
                Go to Event
              </Link>
            </div>
          </>
        ) : (
          <>
            <h2 style={{ margin: "0 0 0.5rem" }}>Payment Processing</h2>
            <p className="muted" style={{ marginBottom: "1.5rem" }}>
              Your payment is being processed. Please check your registrations.
            </p>
            <Link to="/my-registrations" className="btn btn-primary" style={{ width: "auto", marginTop: 0 }}>
              View My Registrations
            </Link>
          </>
        )}
      </div>
    </div>
  );
}
