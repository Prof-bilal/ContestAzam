import { Link } from "react-router-dom";

const faqs = [
  { q: "How do I register for an event?", a: "Browse events, click on one you like, and click the Register button. You'll need to create an account first if you don't have one." },
  { q: "How do I become an Organizer?", a: "During registration, select the Organizer option and fill in your organization details. An admin will review and approve your request. You can also apply from your profile page after registering as a Visitor." },
  { q: "How does QR check-in work?", a: "After registering, you'll receive a digital pass with a QR code. Show this QR code at the event entrance. The organizer will scan it to confirm your attendance." },
  { q: "Can I cancel my registration?", a: "Yes! Go to My Registrations and click Cancel. Note that cancellation before the event is recommended." },
  { q: "How do paid events work?", a: "For paid events, you'll be redirected to a secure payment page (Stripe). After successful payment, your registration is automatically confirmed." },
  { q: "How do I reset my password?", a: "Click 'Forgot Password?' on the login page, enter your email, and follow the link sent to your inbox." },
  { q: "Can I sign in with Google or GitHub?", a: "Yes! Click the respective button on the login or registration page." },
  { q: "How do I leave a review?", a: "After an event ends, visit the event page. If you were registered, you'll see a review form where you can rate and comment." },
];

export function Faq() {
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
        <h1>Frequently Asked Questions</h1>
        <div style={{ display: "grid", gap: "1rem", marginTop: "1.5rem" }}>
          {faqs.map((faq, i) => (
            <div key={i} className="card" style={{ padding: "1.25rem" }}>
              <h3 style={{ margin: "0 0 0.5rem", fontSize: "1rem" }}>{faq.q}</h3>
              <p className="muted" style={{ margin: 0, lineHeight: 1.6 }}>{faq.a}</p>
            </div>
          ))}
        </div>
      </main>
    </div>
  );
}
