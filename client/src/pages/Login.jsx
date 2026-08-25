import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import './Auth.css';
import '../components/FormComponents.css';

const Login = () => {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    email: '',
    password: '',
    rememberMe: false
  });

  const handleInputChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));
  };

  const handleSubmit = (e) => {
    e.preventDefault();

    // TODO: Abdullah will connect this to backend API
    // Backend should return user role from JWT token
    console.log('Login data:', formData);

    // For now, simulate role detection (backend will handle this)
    // Redirect based on role returned from API
    // Example: if API returns role = 'participant', navigate to /dashboard/participant

    // Temporary: redirect to participant dashboard
    navigate('/dashboard/participant');
  };

  return (
    <div className="auth-page">
      <div className="auth-container">
        <div className="auth-card">
          <div className="auth-header">
            <h1 className="auth-title">Welcome Back</h1>
            <p className="auth-subtitle">Sign in to access your EventSphere dashboard</p>
          </div>

          <form className="auth-form" onSubmit={handleSubmit}>
            {/* Email */}
            <label className="field-label">
              <span className="label-text">Email Address</span>
              <input
                type="email"
                name="email"
                className="input-field"
                placeholder="your.email@college.edu"
                value={formData.email}
                onChange={handleInputChange}
                required
              />
            </label>

            {/* Password */}
            <label className="field-label">
              <span className="label-text">Password</span>
              <input
                type="password"
                name="password"
                className="input-field"
                placeholder="Enter your password"
                value={formData.password}
                onChange={handleInputChange}
                required
              />
            </label>

            {/* Remember Me & Forgot Password */}
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <label className="checkbox-label">
                <input
                  type="checkbox"
                  name="rememberMe"
                  checked={formData.rememberMe}
                  onChange={handleInputChange}
                />
                <span>Remember me</span>
              </label>
              <Link to="/forgot-password" className="auth-footer-link" style={{ fontSize: 'var(--text-sm)' }}>
                Forgot password?
              </Link>
            </div>

            {/* Submit Button */}
            <button type="submit" className="button button-primary button-full">
              Sign In
            </button>
          </form>

          {/* Divider */}
          <div className="auth-divider">
            <span className="auth-divider-text">Don't have an account?</span>
          </div>

          {/* Sign Up Link */}
          <Link to="/register" className="button button-secondary button-full">
            Create Account
          </Link>

          {/* Footer Note */}
          <div className="auth-footer">
            <p className="auth-footer-text" style={{ fontSize: 'var(--text-xs)', color: 'var(--text-tertiary)' }}>
              Browsing as a visitor? No account needed. Just explore events!
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Login;
