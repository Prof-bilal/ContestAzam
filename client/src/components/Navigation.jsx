import { useState, useEffect } from 'react';
import { Link, useLocation } from 'react-router-dom';
import './Navigation.css';

const Navigation = () => {
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const location = useLocation();

  const toggleMenu = () => {
    setIsMenuOpen(!isMenuOpen);
  };

  const closeMenu = () => {
    setIsMenuOpen(false);
  };

  // Close menu on route change
  useEffect(() => {
    closeMenu();
  }, [location]);

  // Close menu on escape key
  useEffect(() => {
    const handleEscape = (e) => {
      if (e.key === 'Escape' && isMenuOpen) {
        closeMenu();
      }
    };

    document.addEventListener('keydown', handleEscape);
    return () => document.removeEventListener('keydown', handleEscape);
  }, [isMenuOpen]);

  // Close menu on window resize above mobile breakpoint
  useEffect(() => {
    let resizeTimer;
    const handleResize = () => {
      clearTimeout(resizeTimer);
      resizeTimer = setTimeout(() => {
        if (window.innerWidth > 768 && isMenuOpen) {
          closeMenu();
        }
      }, 250);
    };

    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, [isMenuOpen]);

  const isActive = (path) => {
    return location.pathname === path;
  };

  return (
    <nav className="navbar" role="navigation" aria-label="Main navigation">
      <div className="navbar-container">
        <Link to="/" className="navbar-logo">
          <svg className="nav-icon" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
          </svg>
          <span>EventSphere</span>
        </Link>

        <button
          className="navbar-toggle"
          aria-label="Toggle navigation menu"
          aria-expanded={isMenuOpen}
          onClick={toggleMenu}
        >
          <svg width="24" height="24" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M4 6h16M4 12h16M4 18h16" />
          </svg>
        </button>

        <ul className={`navbar-menu ${isMenuOpen ? 'active' : ''}`} role="menubar">
          <li role="none">
            <Link
              to="/"
              className={`nav-item ${isActive('/') ? 'nav-item-active' : ''}`}
              role="menuitem"
            >
              Home
            </Link>
          </li>
          <li role="none">
            <Link
              to="/events"
              className={`nav-item ${isActive('/events') ? 'nav-item-active' : ''}`}
              role="menuitem"
            >
              Events
            </Link>
          </li>
          <li role="none">
            <Link
              to="/gallery"
              className={`nav-item ${isActive('/gallery') ? 'nav-item-active' : ''}`}
              role="menuitem"
            >
              Gallery
            </Link>
          </li>
          <li role="none">
            <Link
              to="/about"
              className={`nav-item ${isActive('/about') ? 'nav-item-active' : ''}`}
              role="menuitem"
            >
              About
            </Link>
          </li>
          <li role="none">
            <Link
              to="/login"
              className="nav-button-secondary"
              role="menuitem"
            >
              Sign In
            </Link>
          </li>
          <li role="none">
            <Link
              to="/register"
              className="nav-button-primary"
              role="menuitem"
            >
              Get Started
            </Link>
          </li>
        </ul>
      </div>
    </nav>
  );
};

export default Navigation;
