import { Link } from "react-router-dom";

export function About() {
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
        <h1>About EventSphere</h1>
        <p>
          EventSphere is a centralized College Event Information System that provides real-time
          access to event information for students, faculty, and organizers.
        </p>
        <h2>Our Mission</h2>
        <p>
          To eliminate the challenges of manually managed college events — missed announcements,
          low participation, scheduling conflicts, and poor student engagement — by providing a
          digital platform where everyone can easily access event details, register, and stay informed.
        </p>
        <h2>What We Offer</h2>
        <ul>
          <li>Browse and discover upcoming college events</li>
          <li>Register for events with instant confirmation</li>
          <li>QR code check-in for seamless attendance</li>
          <li>Real-time notifications and reminders</li>
          <li>Digital event passes</li>
          <li>Event reviews and feedback</li>
          <li>Organizer tools for event management</li>
        </ul>
        <h2>Who Can Use It</h2>
        <ul>
          <li><strong>Visitors</strong> — Browse events and discover what's happening on campus</li>
          <li><strong>Participants</strong> — Register for events, check in, earn certificates</li>
          <li><strong>Organizers</strong> — Create and manage events, track attendance</li>
          <li><strong>Admins</strong> — Oversee the platform, approve events, manage users</li>
        </ul>
      </main>
    </div>
  );
}
