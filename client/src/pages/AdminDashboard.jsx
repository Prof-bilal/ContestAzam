import { Link } from 'react-router-dom';
import './Dashboard.css';

const AdminDashboard = () => {
  return (
    <div className="dashboard">
      <div className="container">
        <div className="dashboard-header">
          <div>
            <h1 className="dashboard-title">Admin Dashboard</h1>
            <p className="dashboard-subtitle">System administration and moderation</p>
          </div>
          <div style={{ display: 'flex', gap: 'var(--space-3)' }}>
            <Link to="/admin/users" className="button button-secondary">
              Manage Users
            </Link>
            <Link to="/admin/events" className="button button-secondary">
              Moderate Events
            </Link>
          </div>
        </div>

        <div className="dashboard-placeholder">
          <div className="placeholder-icon" style={{ color: 'var(--status-danger)' }}>
            <svg width="64" height="64" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
            </svg>
          </div>
          <p className="placeholder-text">Admin Dashboard</p>
          <p className="placeholder-hint">User management, event moderation, reports, and system settings will appear here</p>
        </div>
      </div>
    </div>
  );
};

export default AdminDashboard;
