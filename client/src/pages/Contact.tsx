import { Link } from "react-router-dom";

export function Contact() {
  return (
    <div className="static-page">
      <nav className="static-nav">
        <Link to="/" className="brand-sm">EventSphere</Link>
        <div style={{ display: "flex", gap: "1rem" }}>
          <Link to="/events">Events</Link>
          <Link to="/about">About</Link>
          <Link to="/contact">Contact</Link>
          <Link to="/faq">FAQ</Link>
          <Link to="/login" className="btn btn-primary btn-small" style={{ width: "auto" }}>Login</Link>
        </div>
      </nav>
      <main className="static-content">
        <h1>Contact Us</h1>
        <p>Have questions or need support? Reach out to us.</p>
        <div style={{ marginTop: "2rem", display: "grid", gap: "1.5rem", maxWidth: 500 }}>
          <div className="card" style={{ padding: "1.5rem" }}>
            <h3 style={{ margin: "0 0 0.5rem" }}>Email</h3>
            <p className="muted" style={{ margin: 0 }}>support@eventsphere.app</p>
          </div>
          <div className="card" style={{ padding: "1.5rem" }}>
            <h3 style={{ margin: "0 0 0.5rem" }}>Report an Issue</h3>
            <p className="muted" style={{ margin: 0 }}>
              If you encounter any bugs or security issues, please email us immediately at
              security@eventsphere.app
            </p>
          </div>
          <div className="card" style={{ padding: "1.5rem" }}>
            <h3 style={{ margin: "0 0 0.5rem" }}>Feedback</h3>
            <p className="muted" style={{ margin: 0 }}>
              We value your feedback! If you have suggestions for improving EventSphere,
              let us know.
            </p>
          </div>
        </div>
      </main>
    </div>
  );
}
