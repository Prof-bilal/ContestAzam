import { useEffect } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

export function Suspended() {
  const { suspendReason: ctxReason, logout } = useAuth();
  const [params] = useSearchParams();
  // Fall back to URL param (used when arriving from OAuth without auth context).
  const suspendReason = ctxReason || params.get("reason") || null;

  useEffect(() => {
    // Clear the refresh cookie on the server if still present.
    void logout();
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  return (
    <div
      style={{
        minHeight: "100vh",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        background: "var(--bg)",
        padding: "1rem",
      }}
    >
      <div
        className="card"
        style={{
          maxWidth: 460,
          width: "100%",
          textAlign: "center",
          padding: "2.5rem 2rem",
        }}
      >
        {/* Icon */}
        <div
          style={{
            width: 64,
            height: 64,
            borderRadius: "50%",
            background: "rgba(220,53,69,0.12)",
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            margin: "0 auto 1.25rem",
            fontSize: "1.75rem",
          }}
        >
          🚫
        </div>

        <h1 style={{ margin: "0 0 0.5rem", fontSize: "1.5rem", color: "var(--danger)" }}>
          Account Suspended
        </h1>

        <p style={{ margin: "0 0 1rem", color: "var(--text-secondary)", lineHeight: 1.6 }}>
          Your account has been suspended by an administrator. You can no longer
          access EventSphere.
        </p>

        {suspendReason && (
          <div
            style={{
              background: "rgba(220,53,69,0.08)",
              border: "1px solid rgba(220,53,69,0.2)",
              borderRadius: 8,
              padding: "0.75rem 1rem",
              marginBottom: "1.25rem",
              textAlign: "left",
            }}
          >
            <div
              style={{
                fontSize: "0.7rem",
                textTransform: "uppercase",
                letterSpacing: "0.05em",
                color: "var(--danger)",
                marginBottom: "0.25rem",
                fontWeight: 600,
              }}
            >
              Reason
            </div>
            <div style={{ fontSize: "0.9rem", lineHeight: 1.5 }}>
              {suspendReason}
            </div>
          </div>
        )}

        <p style={{ fontSize: "0.85rem", color: "var(--text-secondary)", marginBottom: "1.5rem" }}>
          If you believe this is a mistake, please contact the site administrator.
        </p>

        <Link
          to="/"
          className="btn btn-primary"
          style={{ width: "auto", marginTop: 0 }}
        >
          Return to Home
        </Link>
      </div>
    </div>
  );
}
