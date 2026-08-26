import { useEffect, useState, useRef } from "react";
import { useParams, Link } from "react-router-dom";
import { useToast } from "../components/Toast";
import { checkInByToken, getAttendanceStats } from "../api/client";
import type { AttendanceStats } from "../types";

export function QrCheckIn() {
  const { eventId } = useParams<{ eventId: string }>();
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

  return (
    <div className="qr-checkin-page">
      <div className="dash-header">
        <div>
          <h1 style={{ margin: 0, fontSize: "1.5rem" }}>QR Check-In</h1>
          <Link to={`/organizer/events/${eventId}/attendees`} className="muted" style={{ fontSize: "0.85rem" }}>
            &larr; Back to Attendees
          </Link>
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
            <input
              type="text"
              value={manualToken}
              onChange={(e) => setManualToken(e.target.value)}
              placeholder="Enter check-in token"
              disabled={checking}
            />
            <button type="submit" className="btn btn-primary btn-small" disabled={checking || !manualToken.trim()} style={{ width: "auto", marginTop: "0.5rem" }}>
              {checking ? "Checking..." : "Check In"}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}
