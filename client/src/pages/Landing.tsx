import { useEffect, useRef } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import gsap from "gsap";
import { ScrollTrigger } from "gsap/ScrollTrigger";

gsap.registerPlugin(ScrollTrigger);

const FEATURES = [
  { icon: "🔍", title: "Discover Events", desc: "Browse upcoming competitions, workshops, cultural fests, and more. Filter by category, date, or department." },
  { icon: "📝", title: "Easy Registration", desc: "Register for events with a single click. Get real-time slot availability and instant confirmation." },
  { icon: "📱", title: "Digital Check-in", desc: "QR code-based check-in for seamless event attendance. No paper lists, no queues." },
  { icon: "🎓", title: "Certificates", desc: "Receive digital certificates after event completion. Download and share your achievements." },
  { icon: "⭐", title: "Reviews & Feedback", desc: "Rate events and share your experience. Help organizers improve and help students choose." },
  { icon: "📊", title: "Analytics Dashboard", desc: "Organizers and admins get powerful dashboards with registration stats, attendance, and insights." },
];

const STEPS = [
  { num: "01", title: "Browse & Discover", desc: "Explore upcoming events across categories — technical, cultural, sports, and workshops." },
  { num: "02", title: "Register Instantly", desc: "Sign up for events in one click. Get real-time slot updates and confirmation." },
  { num: "03", title: "Attend & Earn", desc: "Check in with QR codes, earn certificates, and leave feedback for future attendees." },
];

const TESTIMONIALS = [
  { name: "Ahmed R.", role: "Computer Science Student", text: "EventSphere made it so easy to find and register for hackathons. I never miss a competition now." },
  { name: "Sara K.", role: "Event Organizer", text: "Managing 200+ registrations used to be chaos. Now everything is automated and organized." },
  { name: "Dr. Hassan", role: "Faculty Advisor", text: "The analytics dashboard gives me visibility into student engagement across all department events." },
];

const STATS = [
  { number: "500+", label: "Events Hosted" },
  { number: "10K+", label: "Students Registered" },
  { number: "50+", label: "Organizers Active" },
  { number: "98%", label: "Satisfaction Rate" },
];

export function Landing() {
  const { status } = useAuth();
  const isAuthenticated = status === "authenticated";

  const heroRef = useRef<HTMLDivElement>(null);
  const featuresRef = useRef<HTMLDivElement>(null);
  const stepsRef = useRef<HTMLDivElement>(null);
  const statsRef = useRef<HTMLDivElement>(null);
  const tealRef = useRef<HTMLDivElement>(null);
  const testimonialsRef = useRef<HTMLDivElement>(null);
  const ctaRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const ctx = gsap.context(() => {
      // Hero animation
      gsap.from(".landing-hero-text h1", {
        y: 60, opacity: 0, duration: 1, ease: "power3.out",
      });
      gsap.from(".landing-hero-text p", {
        y: 40, opacity: 0, duration: 1, delay: 0.2, ease: "power3.out",
      });
      gsap.from(".landing-hero-actions", {
        y: 30, opacity: 0, duration: 0.8, delay: 0.4, ease: "power3.out",
      });
      gsap.from(".landing-hero-illustration", {
        x: 80, opacity: 0, rotation: 5, duration: 1.2, delay: 0.3, ease: "power3.out",
      });

      // Features - stagger in
      gsap.from(".landing-feature-card", {
        scrollTrigger: {
          trigger: ".landing-features-grid",
          start: "top 80%",
        },
        y: 60,
        opacity: 0,
        duration: 0.8,
        stagger: 0.12,
        ease: "power3.out",
      });

      // How It Works - steps
      gsap.from(".landing-step", {
        scrollTrigger: {
          trigger: ".landing-steps-grid",
          start: "top 80%",
        },
        y: 50,
        opacity: 0,
        duration: 0.8,
        stagger: 0.2,
        ease: "power3.out",
      });

      // Stats counter
      gsap.from(".landing-stat-item", {
        scrollTrigger: {
          trigger: ".landing-stats-section",
          start: "top 80%",
        },
        y: 40,
        opacity: 0,
        duration: 0.6,
        stagger: 0.15,
        ease: "power3.out",
      });

      // Teal section
      gsap.from(".landing-teal-text", {
        scrollTrigger: {
          trigger: ".landing-teal-section",
          start: "top 75%",
        },
        x: -60,
        opacity: 0,
        duration: 1,
        ease: "power3.out",
      });
      gsap.from(".landing-teal-stats", {
        scrollTrigger: {
          trigger: ".landing-teal-section",
          start: "top 75%",
        },
        x: 60,
        opacity: 0,
        duration: 1,
        delay: 0.2,
        ease: "power3.out",
      });

      // Testimonials
      gsap.from(".landing-testimonial-card", {
        scrollTrigger: {
          trigger: ".landing-testimonials-grid",
          start: "top 80%",
        },
        y: 50,
        opacity: 0,
        duration: 0.8,
        stagger: 0.15,
        ease: "power3.out",
      });

      // CTA
      gsap.from(".landing-cta-section .landing-cta-inner", {
        scrollTrigger: {
          trigger: ".landing-cta-section",
          start: "top 80%",
        },
        y: 40,
        opacity: 0,
        duration: 1,
        ease: "power3.out",
      });
    });

    return () => ctx.revert();
  }, []);

  return (
    <>
      {/* 1. Hero Section */}
      <section className="landing-hero" ref={heroRef}>
        <div className="landing-hero-text">
          <h1>College events, beautifully organized.</h1>
          <p>
            EventSphere brings students, organizers, and administrators together
            on one platform. Discover competitions, register for events, and
            never miss what matters.
          </p>
          <div className="landing-hero-actions">
            <Link to="/events" className="btn btn-primary" style={{ width: "auto", marginTop: 0 }}>
              Browse Events
            </Link>
            {isAuthenticated ? (
              <Link to="/dashboard" className="btn btn-secondary" style={{ textDecoration: "none" }}>
                Dashboard
              </Link>
            ) : (
              <>
                <Link to="/login" className="btn btn-secondary" style={{ textDecoration: "none" }}>
                  Sign In
                </Link>
                <Link to="/register" className="btn btn-secondary" style={{ textDecoration: "none" }}>
                  Create Account
                </Link>
              </>
            )}
          </div>
        </div>
        <div className="landing-hero-visual">
          <div className="landing-hero-illustration">
            <div className="landing-illo-icon">📅</div>
            <div className="landing-illo-text">EventSphere</div>
          </div>
        </div>
      </section>

      {/* 2. Features Section */}
      <section className="landing-features" ref={featuresRef}>
        <div className="landing-features-header">
          <h2>Everything you need</h2>
          <p>From discovery to check-in, EventSphere covers the full event lifecycle.</p>
        </div>
        <div className="landing-features-grid">
          {FEATURES.map((f) => (
            <div className="landing-feature-card" key={f.title}>
              <div className="landing-feature-icon">{f.icon}</div>
              <h3>{f.title}</h3>
              <p>{f.desc}</p>
            </div>
          ))}
        </div>
      </section>

      {/* 3. How It Works */}
      <section className="landing-steps" ref={stepsRef}>
        <div className="landing-steps-header">
          <h2>How it works</h2>
          <p>Three simple steps to get started.</p>
        </div>
        <div className="landing-steps-grid">
          {STEPS.map((s) => (
            <div className="landing-step" key={s.num}>
              <div className="landing-step-num">{s.num}</div>
              <h3>{s.title}</h3>
              <p>{s.desc}</p>
            </div>
          ))}
        </div>
      </section>

      {/* 4. Stats Section */}
      <section className="landing-stats-section" ref={statsRef}>
        <div className="landing-stats-inner">
          {STATS.map((s) => (
            <div className="landing-stat-item" key={s.label}>
              <span className="landing-stat-number">{s.number}</span>
              <span className="landing-stat-label">{s.label}</span>
            </div>
          ))}
        </div>
      </section>

      {/* 5. Teal Section */}
      <section className="landing-teal-section" ref={tealRef}>
        <div className="landing-teal-inner">
          <div className="landing-teal-text">
            <h2>Built for college campuses</h2>
            <p>
              EventSphere is designed specifically for educational institutions.
              Manage technical competitions, cultural fests, workshops, and
              annual day celebrations from one place.
            </p>
            <p>
              Role-based access ensures organizers create events, students
              register and participate, and administrators maintain quality and
              oversight.
            </p>
          </div>
          <div className="landing-teal-stats">
            <div className="landing-teal-stat">
              <span className="landing-teal-stat-number">4</span>
              <span className="landing-teal-stat-label">User Roles</span>
            </div>
            <div className="landing-teal-stat">
              <span className="landing-teal-stat-number">6+</span>
              <span className="landing-teal-stat-label">Categories</span>
            </div>
            <div className="landing-teal-stat">
              <span className="landing-teal-stat-number">24/7</span>
              <span className="landing-teal-stat-label">Available</span>
            </div>
          </div>
        </div>
      </section>

      {/* 6. Testimonials */}
      <section className="landing-testimonials" ref={testimonialsRef}>
        <div className="landing-testimonials-header">
          <h2>What people say</h2>
          <p>Trusted by students and organizers across campuses.</p>
        </div>
        <div className="landing-testimonials-grid">
          {TESTIMONIALS.map((t) => (
            <div className="landing-testimonial-card" key={t.name}>
              <div className="landing-testimonial-text">"{t.text}"</div>
              <div className="landing-testimonial-author">
                <div className="landing-testimonial-avatar">{t.name.charAt(0)}</div>
                <div>
                  <div className="landing-testimonial-name">{t.name}</div>
                  <div className="landing-testimonial-role">{t.role}</div>
                </div>
              </div>
            </div>
          ))}
        </div>
      </section>

      {/* 7. CTA Section */}
      <section className="landing-cta-section" ref={ctaRef}>
        <div className="landing-cta-inner">
          <h2>Ready to get started?</h2>
          <p>Join thousands of students and organizers on EventSphere.</p>
          <div className="landing-cta-actions">
            <Link to="/register" className="btn btn-primary" style={{ width: "auto", marginTop: 0 }}>
              Create Free Account
            </Link>
            <Link to="/events" className="btn btn-secondary" style={{ textDecoration: "none" }}>
              Browse Events
            </Link>
          </div>
        </div>
      </section>

      {/* 8. Footer / Sitemap */}
      <footer className="landing-footer">
        <div className="landing-footer-grid">
          <div className="landing-footer-brand">
            <h3>EventSphere</h3>
            <p>
              A centralized platform for managing college events. Discover,
              register, and participate in events that matter to you.
            </p>
          </div>
          <div className="landing-footer-col">
            <h4>Platform</h4>
            <ul>
              <li><Link to="/events">Browse Events</Link></li>
              <li><Link to="/register">Create Account</Link></li>
              <li><Link to="/login">Sign In</Link></li>
            </ul>
          </div>
          <div className="landing-footer-col">
            <h4>Categories</h4>
            <ul>
              <li><Link to="/events">Technical</Link></li>
              <li><Link to="/events">Cultural</Link></li>
              <li><Link to="/events">Sports</Link></li>
              <li><Link to="/events">Workshops</Link></li>
            </ul>
          </div>
          <div className="landing-footer-col">
            <h4>Support</h4>
            <ul>
              <li><Link to="/forgot-password">Reset Password</Link></li>
              <li><Link to="/verify-email">Verify Email</Link></li>
            </ul>
          </div>
          <div className="landing-footer-col">
            <h4>Company</h4>
            <ul>
              <li><Link to="/about">About Us</Link></li>
              <li><Link to="/contact">Contact</Link></li>
              <li><Link to="/faq">FAQ</Link></li>
            </ul>
          </div>
        </div>
        <div className="landing-footer-bottom">
          EventSphere &mdash; College Event Information System
        </div>
      </footer>
    </>
  );
}
