import { Link } from 'react-router-dom';
import './Dashboard.css';

const OrganizerDashboard = () => {
  return (
    <div className="dashboard">
      <div className="container">
        <div className="dashboard-header">
          <div>
            <h1 className="dashboard-title">Organizer Dashboard</h1>
            <p className="dashboard-subtitle">Manage your events and track engagement</p>
          </div>
          <Link to="/events/create" className="button button-primary">
            Create Event
          </Link>
        </div>

        <div className="dashboard-placeholder">
          <div className="placeholder-icon" style={{ color: 'var(--chip-lavender)' }}>
            <svg width="64" height="64" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M21 13.255A23.931 23.931 0 0112 15c-3.183 0-6.22-.62-9-1.745M16 6V4a2 2 0 00-2-2h-4a2 2 0 00-2 2v2m4 6h.01M5 20h14a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
            </svg>
          </div>
          <p className="placeholder-text">Organizer Dashboard</p>
          <p className="placeholder-hint">Your events, registrations, attendance, and analytics will appear here</p>
        </div>
      </div>
    </div>
  );
};

export default OrganizerDashboard;
