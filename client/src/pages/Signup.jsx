import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import './Auth.css';
import '../components/FormComponents.css';

const Signup = () => {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    fullName: '',
    email: '',
    password: '',
    confirmPassword: '',
    role: 'participant',
    agreeToTerms: false
  });

  const handleInputChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));
  };

  const handleRoleSelect = (role) => {
    setFormData(prev => ({ ...prev, role }));
  };

  const handleSubmit = (e) => {
    e.preventDefault();

    // TODO: Abdullah will connect this to backend API
    console.log('Signup data:', formData);

    // For now, redirect to respective dashboard based on role
    if (formData.role === 'participant') {
      navigate('/dashboard/participant');
    } else if (formData.role === 'organizer') {
      navigate('/dashboard/organizer');
    }
  };

  return (
    <div className="auth-page">
      <div className="auth-container">
        <div className="auth-card">
          <div className="auth-header">
            <h1 className="auth-title">Create Your Account</h1>
            <p className="auth-subtitle">Join EventSphere to discover and manage campus events</p>
          </div>

          <form className="auth-form" onSubmit={handleSubmit}>
            {/* Full Name */}
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
              <span className="field-hint">Use your college email address</span>
            </label>

            {/* Password */}
            <label className="field-label">
              <span className="label-text">Password</span>
              <input
                type="password"
                name="password"
                className="input-field"
                placeholder="Create a strong password"
                value={formData.password}
                onChange={handleInputChange}
                required
                minLength={8}
              />
              <span className="field-hint">At least 8 characters</span>
            </label>

            {/* Confirm Password */}
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
              />
            </label>

            {/* Role Selection */}
            <div className="role-selection">
              <div className="role-selection-title">I want to join as</div>
              <div className="role-grid">
                {/* Participant Role */}
                <label className={`role-card ${formData.role === 'participant' ? 'selected' : ''}`}>
                  <input
                    type="radio"
                    name="role"
                    value="participant"
                    checked={formData.role === 'participant'}
                    onChange={() => handleRoleSelect('participant')}
                  />
                  <div className="role-card-header">
                    <div className="role-icon" style={{ background: 'var(--chip-mint)' }}>
                      <svg width="24" height="24" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                      </svg>
                    </div>
                    <div className="role-title">Participant</div>
                  </div>
                  <div className="role-description">
                    Discover events, register, attend, and earn certificates
                  </div>
                </label>

                {/* Organizer Role */}
                <label className={`role-card ${formData.role === 'organizer' ? 'selected' : ''}`}>
                  <input
                    type="radio"
                    name="role"
                    value="organizer"
                    checked={formData.role === 'organizer'}
                    onChange={() => handleRoleSelect('organizer')}
                  />
                  <div className="role-card-header">
                    <div className="role-icon" style={{ background: 'var(--chip-lavender)' }}>
                      <svg width="24" height="24" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M21 13.255A23.931 23.931 0 0112 15c-3.183 0-6.22-.62-9-1.745M16 6V4a2 2 0 00-2-2h-4a2 2 0 00-2 2v2m4 6h.01M5 20h14a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                      </svg>
                    </div>
                    <div className="role-title">Organizer</div>
                  </div>
                  <div className="role-description">
                    Create and manage events, track attendance, and issue certificates
                  </div>
                </label>
              </div>
            </div>

            {/* Terms and Conditions */}
            <label className="checkbox-label">
              <input
                type="checkbox"
                name="agreeToTerms"
                checked={formData.agreeToTerms}
                onChange={handleInputChange}
                required
              />
              <span>
                I agree to the <Link to="/terms">Terms of Service</Link> and <Link to="/privacy">Privacy Policy</Link>
              </span>
            </label>

            {/* Submit Button */}
            <button type="submit" className="button button-primary button-full">
              Create Account
            </button>
          </form>

          {/* Footer */}
          <div className="auth-footer">
            <p className="auth-footer-text">
              Already have an account? <Link to="/login" className="auth-footer-link">Sign in</Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Signup;
