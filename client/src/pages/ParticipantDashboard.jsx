import { Link } from 'react-router-dom';
import './Dashboard.css';

const ParticipantDashboard = () => {
  return (
    <div className="dashboard">
      <div className="container">
        <div className="dashboard-header">
          <div>
            <h1 className="dashboard-title">Participant Dashboard</h1>
            <p className="dashboard-subtitle">Welcome back! Discover and manage your events</p>
          </div>
          <Link to="/events" className="button button-primary">
            Browse Events
          </Link>
        </div>

        <div className="dashboard-placeholder">
          <div className="placeholder-icon">
            <svg width="64" height="64" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
            </svg>
          </div>
          <p className="placeholder-text">Participant Dashboard</p>
          <p className="placeholder-hint">Your registered events, certificates, and feedback will appear here</p>
        </div>
      </div>
    </div>
  );
};

export default ParticipantDashboard;
