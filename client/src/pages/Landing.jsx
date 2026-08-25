import { Link } from 'react-router-dom';
import './Landing.css';

const Landing = () => {
  const features = [
    {
      icon: (
        <svg width="24" height="24" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
        </svg>
      ),
      title: 'Browse Events',
      tag: 'Core',
      description: 'Find events by category, department, or date with real-time availability',
      color: 'var(--chip-lavender)'
    },
    {
      icon: (
        <svg width="24" height="24" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4" />
        </svg>
      ),
      title: 'Easy Registration',
      tag: 'Core',
      description: 'Register for events in seconds with automatic eligibility checks',
      color: 'var(--chip-mint)'
    },
    {
      icon: (
        <svg width="24" height="24" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 4v1m6 11h2m-6 0h-2v4m0-11v3m0 0h.01M12 12h4.01M16 20h4M4 12h4m12 0h.01M5 8h2a1 1 0 001-1V5a1 1 0 00-1-1H5a1 1 0 00-1 1v2a1 1 0 001 1zm12 0h2a1 1 0 001-1V5a1 1 0 00-1-1h-2a1 1 0 00-1 1v2a1 1 0 001 1zM5 20h2a1 1 0 001-1v-2a1 1 0 00-1-1H5a1 1 0 00-1 1v2a1 1 0 001 1z" />
        </svg>
      ),
      title: 'QR Check-In',
      tag: 'Pro',
      description: 'Scan QR codes for instant attendance tracking and verification',
      color: 'var(--chip-ice-blue)'
    },
    {
      icon: (
        <svg width="24" height="24" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
        </svg>
      ),
      title: 'Digital Certificates',
      tag: 'Pro',
      description: 'Download verified certificates instantly after event completion',
      color: 'var(--chip-peach)'
    },
    {
      icon: (
        <svg width="24" height="24" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
        </svg>
      ),
      title: 'Media Gallery',
      tag: 'Feature',
      description: 'View event photos and videos shared by organizers and participants',
      color: 'var(--chip-pink)'
    },
    {
      icon: (
        <svg width="24" height="24" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M11 3.055A9.001 9.001 0 1020.945 13H11V3.055z" />
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M20.488 9H15V3.512A9.025 9.025 0 0120.488 9z" />
        </svg>
      ),
      title: 'Analytics Dashboard',
      tag: 'Admin',
      description: 'Track attendance, engagement, and feedback with detailed reports',
      color: 'var(--chip-lavender)'
    }
  ];

  const stats = [
    { value: '5,000+', label: 'Active Students' },
    { value: '200+', label: 'Events Hosted' },
    { value: '50+', label: 'Organizations' },
    { value: '98%', label: 'Satisfaction Rate' }
  ];

  return (
    <main>
      {/* Hero Section */}
      <section className="hero">
        <div className="container-narrow">
          <div className="hero-content">
            <h1 className="hero-title">
              Discover Campus Events<br />All in One Place
            </h1>
            <p className="hero-subtitle">
              EventSphere brings college events to your fingertips. Register, attend, and get certified—no more missed opportunities.
            </p>
            <div className="hero-actions">
              <Link to="/register" className="button-primary">Get Started</Link>
              <Link to="/events" className="button-link">Browse Events →</Link>
            </div>
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section className="features">
        <div className="container">
          <div className="section-header">
            <h2 className="section-title">Everything You Need</h2>
            <p className="section-description">
              Streamlined event management for students, organizers, and administrators
            </p>
          </div>

          <div className="card-grid">
            {features.map((feature, index) => (
              <div key={index} className="feature-card">
                <div className="feature-icon" style={{ background: feature.color }}>
                  {feature.icon}
                </div>
                <div className="feature-header">
                  <h3 className="feature-title">{feature.title}</h3>
                  <span className="feature-tag">{feature.tag}</span>
                </div>
                <p className="feature-description">{feature.description}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Stats Section */}
      <section className="stats">
        <div className="container">
          <div className="stats-grid">
            {stats.map((stat, index) => (
              <div key={index} className="stat-item">
                <div className="stat-value">{stat.value}</div>
                <div className="stat-label">{stat.label}</div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Footer */}
      <footer className="footer">
        <div className="container">
          <div className="footer-content">
            <div className="footer-section">
              <h4 className="footer-heading">EventSphere</h4>
              <p className="footer-text">College event management made simple</p>
            </div>

            <div className="footer-section">
              <h4 className="footer-heading">Platform</h4>
              <ul className="footer-links">
                <li><Link to="/events">Browse Events</Link></li>
                <li><Link to="/gallery">Gallery</Link></li>
                <li><Link to="/about">About</Link></li>
              </ul>
            </div>

            <div className="footer-section">
              <h4 className="footer-heading">Account</h4>
              <ul className="footer-links">
                <li><Link to="/login">Sign In</Link></li>
                <li><Link to="/register">Register</Link></li>
                <li><Link to="/dashboard">Dashboard</Link></li>
              </ul>
            </div>

            <div className="footer-section">
              <h4 className="footer-heading">Support</h4>
              <ul className="footer-links">
                <li><Link to="/help">Help Center</Link></li>
                <li><Link to="/contact">Contact</Link></li>
                <li><Link to="/privacy">Privacy</Link></li>
              </ul>
            </div>
          </div>

          <div className="footer-bottom">
            <p className="footer-copyright">© 2026 EventSphere. Educational project.</p>
          </div>
        </div>
      </footer>
    </main>
  );
};

export default Landing;
