import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './Auth.css';
import '../components/FormComponents.css';

const AdminLogin = () => {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    email: '',
    password: '',
    secretKey: ''
  });
  const [showSecretKey, setShowSecretKey] = useState(false);

  // Secret key sequence: Press Ctrl+Shift+A three times
  useEffect(() => {
    let keyPressCount = 0;
    let resetTimer;

    const handleKeyPress = (e) => {
      if (e.ctrlKey && e.shiftKey && e.key === 'A') {
        keyPressCount++;

        clearTimeout(resetTimer);
        resetTimer = setTimeout(() => {
          keyPressCount = 0;
        }, 2000);

        if (keyPressCount === 3) {
          setShowSecretKey(true);
          keyPressCount = 0;
        }
      }
    };

    window.addEventListener('keydown', handleKeyPress);
    return () => {
      window.removeEventListener('keydown', handleKeyPress);
      clearTimeout(resetTimer);
    };
  }, []);

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleSubmit = (e) => {
    e.preventDefault();

    // TODO: Abdullah will connect this to backend API
    // Backend should verify admin credentials + secret key
    console.log('Admin login data:', formData);

    // For now, redirect to admin dashboard
    navigate('/dashboard/admin');
  };

  return (
    <div className="auth-page">
      <div className="auth-container">
        <div className="auth-card">
          <div className="auth-header">
            <h1 className="auth-title">Administrator Access</h1>
            <p className="auth-subtitle">Restricted area - authorized personnel only</p>
          </div>

          <form className="auth-form" onSubmit={handleSubmit}>
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
              <span className="label-text">Admin Password</span>
              <input
                type="password"
                name="password"
                className="input-field"
                placeholder="Enter admin password"
                value={formData.password}
                onChange={handleInputChange}
                required
                autoComplete="off"
              />
            </label>

            {/* Secret Key - Only shows after secret sequence */}
            {showSecretKey && (
              <label className="field-label">
                <span className="label-text">Secret Key</span>
                <input
                  type="password"
                  name="secretKey"
                  className="input-field"
                  placeholder="Enter secret key"
                  value={formData.secretKey}
                  onChange={handleInputChange}
                  required
                  autoComplete="off"
                  style={{ borderColor: 'var(--accent-brand)' }}
                />
                <span className="field-hint" style={{ color: 'var(--accent-brand)' }}>
                  Secret key field unlocked
                </span>
              </label>
            )}

            {/* Submit Button */}
            <button
              type="submit"
              className="button button-primary button-full"
              disabled={!showSecretKey}
            >
              Access Admin Panel
            </button>
          </form>

          {/* Hidden Hint */}
          {!showSecretKey && (
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

export default AdminLogin;
