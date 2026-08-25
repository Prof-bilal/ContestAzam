import { useState, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import './Auth.css';
import '../components/FormComponents.css';

const AdminAuth = () => {
  const navigate = useNavigate();
  const [mode, setMode] = useState('login'); // 'login' or 'register'
  const [formData, setFormData] = useState({
    fullName: '',
    email: '',
    password: '',
    confirmPassword: '',
    adminCode: ''
  });

  const clickCountRef = useRef(0);
  const clickTimerRef = useRef(null);

  // Secret click sequence: Click logo 7 times within 3 seconds to reveal admin code field
  const [showAdminCode, setShowAdminCode] = useState(false);

  const handleLogoClick = () => {
    clickCountRef.current += 1;

    if (clickTimerRef.current) {
      clearTimeout(clickTimerRef.current);
    }

    clickTimerRef.current = setTimeout(() => {
      clickCountRef.current = 0;
    }, 3000);

    if (clickCountRef.current >= 7) {
      setShowAdminCode(true);
      clickCountRef.current = 0;
    }
  };

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleSubmit = (e) => {
    e.preventDefault();

    if (mode === 'login') {
      // TODO: Abdullah will connect this to POST /api/auth/admin/login
      // Backend should verify admin credentials + admin code
      console.log('Admin login data:', formData);
      navigate('/dashboard/admin');
    } else {
      // TODO: Abdullah will connect this to POST /api/auth/admin/register
      // Backend should verify admin code before creating admin user
      console.log('Admin register data:', formData);
      navigate('/dashboard/admin');
    }
  };

  return (
    <div className="auth-page">
      <div className="auth-container">
        <div className="auth-card">
          <div className="auth-header">
            <div
              onClick={handleLogoClick}
              style={{
                display: 'inline-flex',
                alignItems: 'center',
                justifyContent: 'center',
                width: '48px',
                height: '48px',
                margin: '0 auto var(--space-4)',
                background: showAdminCode ? 'var(--accent-brand)' : 'var(--bg-muted)',
                borderRadius: 'var(--radius-md)',
                cursor: 'default',
                userSelect: 'none',
                transition: 'background var(--duration-base)'
              }}
            >
              <svg width="24" height="24" fill="none" stroke={showAdminCode ? '#fff' : 'currentColor'} viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
              </svg>
            </div>
            <h1 className="auth-title">
              {mode === 'login' ? 'Admin Access' : 'Admin Registration'}
            </h1>
            <p className="auth-subtitle">
              {showAdminCode
                ? mode === 'login'
                  ? 'Admin code unlocked - enter credentials'
                  : 'Admin code unlocked - create admin account'
                : 'Restricted area - authorized personnel only'}
            </p>
          </div>

          <form className="auth-form" onSubmit={handleSubmit}>
            {mode === 'register' && (
              <label className="field-label">
                <span className="label-text">Full Name</span>
                <input
                  type="text"
                  name="fullName"
                  className="input-field"
                  placeholder="Enter your full name"
                  value={formData.fullName}
                  onChange={handleInputChange}
                  required
                />
              </label>
            )}

            {/* Email */}
            <label className="field-label">
              <span className="label-text">Admin Email</span>
              <input
                type="email"
                name="email"
                className="input-field"
                placeholder="admin@eventsphere.system"
                value={formData.email}
                onChange={handleInputChange}
                required
                autoComplete="off"
              />
            </label>

            {/* Password */}
            <label className="field-label">
              <span className="label-text">Password</span>
              <input
                type="password"
                name="password"
                className="input-field"
                placeholder={mode === 'login' ? 'Enter admin password' : 'Create a strong password'}
                value={formData.password}
                onChange={handleInputChange}
                required
                minLength={8}
                autoComplete="off"
              />
              {mode === 'register' && (
                <span className="field-hint">At least 8 characters</span>
              )}
            </label>

            {mode === 'register' && (
              <label className="field-label">
                <span className="label-text">Confirm Password</span>
                <input
                  type="password"
                  name="confirmPassword"
                  className="input-field"
                  placeholder="Re-enter your password"
                  value={formData.confirmPassword}
                  onChange={handleInputChange}
                  required
                  autoComplete="off"
                />
              </label>
            )}

            {/* Admin Code - Only shows after secret sequence */}
            {showAdminCode && (
              <label className="field-label">
                <span className="label-text">Admin Code</span>
                <input
                  type="password"
                  name="adminCode"
                  className="input-field"
                  placeholder="Enter admin code"
                  value={formData.adminCode}
                  onChange={handleInputChange}
                  required
                  autoComplete="off"
                  style={{ borderColor: 'var(--accent-brand)' }}
                />
                <span className="field-hint" style={{ color: 'var(--accent-brand)' }}>
                  {mode === 'login'
                    ? 'Admin code field unlocked'
                    : 'Required to create admin account'}
                </span>
              </label>
            )}

            {/* Submit Button */}
            <button
              type="submit"
              className="button button-primary button-full"
              disabled={!showAdminCode}
            >
              {mode === 'login' ? 'Access Admin Panel' : 'Create Admin Account'}
            </button>
          </form>

          {/* Mode Toggle */}
          <div className="auth-divider">
            <span className="auth-divider-text">
              {mode === 'login' ? 'Need admin account?' : 'Already have admin account?'}
            </span>
          </div>

          <button
            type="button"
            onClick={() => setMode(mode === 'login' ? 'register' : 'login')}
            className="button button-secondary button-full"
            disabled={!showAdminCode}
          >
            {mode === 'login' ? 'Register as Admin' : 'Sign In as Admin'}
          </button>

          {/* Hidden Hint */}
          {!showAdminCode && (
            <div className="auth-footer">
              <p className="auth-footer-text" style={{ fontSize: 'var(--text-xs)', color: 'var(--text-tertiary)' }}>
                Security protocol active
              </p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default AdminAuth;
