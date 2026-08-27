import { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { getDigitalPass } from "../api/client";
import { LogoutButton } from "../components/LogoutButton";
import type { DigitalPass as DigitalPassType } from "../types";

export function DigitalPass() {
  const { id } = useParams<{ id: string }>();
  const [pass, setPass] = useState<DigitalPassType | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const registrationId = Number(id);

  useEffect(() => {
    if (!registrationId) return;
    getDigitalPass(registrationId)
      .then(setPass)
      .catch((e) => setError(e instanceof Error ? e.message : "Failed to load pass."))
      .finally(() => setLoading(false));
  }, [registrationId]);

  if (loading) return <div className="loading-state">Loading your pass...</div>;
  if (error) return <div className="empty-state">{error}</div>;
  if (!pass) return <div className="empty-state">Pass not found.</div>;

  return (
    <div className="digital-pass-page">
      <div className="no-print" style={{ marginBottom: "1rem", display: "flex", gap: "0.5rem", justifyContent: "space-between" }}>
        <Link to="/my-registrations" className="btn btn-secondary btn-small">&larr; Back to Registrations</Link>
        <LogoutButton className="btn btn-secondary btn-small" />
      </div>

      <div className="digital-pass">
        <div className="digital-pass-header">
          <h2>{pass.eventTitle}</h2>
          <span className="digital-pass-subtitle">Event Registration Pass</span>
        </div>

        <div className="digital-pass-qr">
          <img
            src={`data:image/png;base64,${pass.qrCodeBase64}`}
            alt="Check-in QR Code"
            style={{ width: 200, height: 200 }}
          />
          <p className="muted" style={{ margin: "0.5rem 0 0", fontSize: "0.8rem" }}>
            Show this QR code at the event entrance
          </p>
        </div>

        <div className="digital-pass-details">
          <div className="digital-pass-row">
            <span className="digital-pass-label">Participant</span>
            <span className="digital-pass-value">{pass.participantName}</span>
          </div>
          <div className="digital-pass-row">
            <span className="digital-pass-label">Date</span>
            <span className="digital-pass-value">{new Date(pass.eventDate).toLocaleDateString()}</span>
          </div>
          <div className="digital-pass-row">
            <span className="digital-pass-label">Time</span>
            <span className="digital-pass-value">{pass.eventTime}</span>
          </div>
          <div className="digital-pass-row">
            <span className="digital-pass-label">Venue</span>
            <span className="digital-pass-value">{pass.venue}</span>
          </div>
          <div className="digital-pass-row">
            <span className="digital-pass-label">Registration ID</span>
            <span className="digital-pass-value">#{pass.registrationId}</span>
          </div>
        </div>

        <div className="digital-pass-footer">
          Present this pass at the event entrance for check-in
        </div>
      </div>

      <div className="no-print" style={{ marginTop: "1rem", textAlign: "center" }}>
        <button className="btn btn-primary btn-small" onClick={() => window.print()} style={{ width: "auto", marginTop: 0 }}>
          Print Pass
        </button>
      </div>
    </div>
  );
}
