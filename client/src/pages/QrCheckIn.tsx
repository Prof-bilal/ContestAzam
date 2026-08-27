import { useEffect, useState, useRef } from "react";
import { useParams, Link } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { LogoutButton } from "../components/LogoutButton";
import { useToast } from "../components/Toast";
import { checkInByToken, getAttendanceStats } from "../api/client";
import type { AttendanceStats } from "../types";

export function QrCheckIn() {
  const { eventId } = useParams<{ eventId: string }>();
  const { user } = useAuth();
  const { addToast } = useToast();
  const [manualToken, setManualToken] = useState("");
  const [checking, setChecking] = useState(false);
  const [stats, setStats] = useState<AttendanceStats | null>(null);
  const [lastResult, setLastResult] = useState<{ success: boolean; message: string; name?: string } | null>(null);
  const scannerRef = useRef<HTMLDivElement>(null);
  const scannerInstanceRef = useRef<any>(null);

  const eid = Number(eventId);

  const fetchStats = () => {
    if (eid) {
      getAttendanceStats(eid).then(setStats).catch(() => {});
    }
  };

  useEffect(() => {
    fetchStats();
  }, [eid]);

  useEffect(() => {
    let mounted = true;
    const initScanner = async () => {
      try {
        const { Html5QrcodeScanner } = await import("html5-qrcode");
        if (!mounted || !scannerRef.current) return;

        const scanner = new Html5QrcodeScanner(
          "qr-reader",
          { fps: 10, qrbox: { width: 250, height: 250 } },
          false
        );

        scanner.render(
          async (decodedText: string) => {
            if (checking) return;
            await handleTokenCheckIn(decodedText);
            scanner.clear();
          },
          () => {}
        );

        scannerInstanceRef.current = scanner;
      } catch {
        // Camera not available — user can use manual entry
      }
    };

    initScanner();

    return () => {
      mounted = false;
      scannerInstanceRef.current?.clear?.();
    };
  }, [eid]);

  const handleTokenCheckIn = async (token: string) => {
    if (checking) return;
    setChecking(true);
    setLastResult(null);
    try {
      const result = await checkInByToken(token.trim());
      setLastResult({
        success: result.success,
        message: result.message,
        name: result.attendeeName,
      });
      if (result.success) {
        addToast("success", `${result.attendeeName} checked in!`);
        fetchStats();
      } else {
        addToast("error", result.message);
      }
    } catch (e: unknown) {
      const msg = e instanceof Error ? e.message : "Check-in failed.";
      setLastResult({ success: false, message: msg });
      addToast("error", msg);
    } finally {
      setChecking(false);
      setManualToken("");
    }
  };

  const handleManualSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!manualToken.trim()) return;
    await handleTokenCheckIn(manualToken);
  };

  const highestRole = ["Admin", "Organizer", "Participant"].find((r) =>
    user?.roles.includes(r),
  ) || "Visitor";

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
          <Link to="/events" className="admin-nav-item">Browse Events</Link>
        </nav>
        <LogoutButton style={{ marginTop: "auto" }} />
      </aside>

      <main className="admin-main">
        <div className="admin-header">
          <div>
            <div style={{ display: "flex", alignItems: "center", gap: "0.75rem" }}>
              <Link to={`/organizer/events/${eventId}/attendees`} className="btn btn-secondary btn-small" style={{ width: "auto" }}>
                &larr;
              </Link>
              <h1 style={{ margin: 0, fontSize: "1.5rem" }}>QR Check-In</h1>
            </div>
            <p className="muted" style={{ marginLeft: "2.75rem" }}>
              Scan or enter token to check in attendees
            </p>
          </div>
          {stats && (
            <div className="qr-stats-panel">
              <div className="qr-stat">
                <span className="qr-stat-value">{stats.totalCheckedIn}</span>
                <span className="qr-stat-label">Checked In</span>
              </div>
              <div className="qr-stat">
                <span className="qr-stat-value">{stats.totalRegistered}</span>
                <span className="qr-stat-label">Registered</span>
              </div>
              <div className="qr-stat">
                <span className="qr-stat-value">{stats.checkInPercentage}%</span>
                <span className="qr-stat-label">Rate</span>
              </div>
            </div>
          )}
        </div>

        {lastResult && (
          <div className={`qr-checkin-result ${lastResult.success ? "success" : "error"}`}>
            <strong>{lastResult.success ? "Checked In!" : "Failed"}</strong>
            <span>{lastResult.name ? `${lastResult.name} — ` : ""}{lastResult.message}</span>
          </div>
        )}

        <div className="qr-checkin-content">
          <div className="qr-scanner-container">
            <h3>Scan QR Code</h3>
            <div id="qr-reader" ref={scannerRef} style={{ width: "100%" }}></div>
            <p className="muted" style={{ textAlign: "center", marginTop: "0.5rem" }}>
              Point camera at participant's QR code
            </p>
          </div>

          <div className="qr-manual-entry">
            <h3>Manual Entry</h3>
            <form onSubmit={handleManualSubmit}>
              <label htmlFor="checkin-token">Check-in Token</label>
              <input
                id="checkin-token"
                type="text"
                value={manualToken}
                onChange={(e) => setManualToken(e.target.value)}
                placeholder="Enter check-in token"
                disabled={checking}
              />
              <button
                type="submit"
                className="btn btn-primary btn-small"
                disabled={checking || !manualToken.trim()}
                style={{ width: "auto", marginTop: "0.75rem" }}
              >
                {checking ? "Checking..." : "Check In"}
              </button>
            </form>
          </div>
        </div>
      </main>
    </div>
  );
}
