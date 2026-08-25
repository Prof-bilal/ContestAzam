import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Navigation from './components/Navigation';
import Landing from './pages/Landing';
import Signup from './pages/Signup';
import Login from './pages/Login';
import AdminLogin from './pages/AdminLogin';
import ParticipantDashboard from './pages/ParticipantDashboard';
import OrganizerDashboard from './pages/OrganizerDashboard';
import AdminDashboard from './pages/AdminDashboard';
import './index.css';

function App() {
  return (
    <Router>
      <Navigation />
      <Routes>
        {/* Public Routes */}
        <Route path="/" element={<Landing />} />

        {/* Auth Routes */}
        <Route path="/register" element={<Signup />} />
        <Route path="/login" element={<Login />} />
        <Route path="/admin/login" element={<AdminLogin />} />

        {/* Dashboard Routes */}
        <Route path="/dashboard/participant" element={<ParticipantDashboard />} />
        <Route path="/dashboard/organizer" element={<OrganizerDashboard />} />
        <Route path="/dashboard/admin" element={<AdminDashboard />} />

        {/* Placeholder routes - to be created later */}
        <Route path="/events" element={<div className="container" style={{ padding: '48px 0', textAlign: 'center' }}>Events page coming soon</div>} />
        <Route path="/gallery" element={<div className="container" style={{ padding: '48px 0', textAlign: 'center' }}>Gallery page coming soon</div>} />
        <Route path="/about" element={<div className="container" style={{ padding: '48px 0', textAlign: 'center' }}>About page coming soon</div>} />
        <Route path="/dashboard" element={<div className="container" style={{ padding: '48px 0', textAlign: 'center' }}>Dashboard coming soon</div>} />
        <Route path="/help" element={<div className="container" style={{ padding: '48px 0', textAlign: 'center' }}>Help page coming soon</div>} />
        <Route path="/contact" element={<div className="container" style={{ padding: '48px 0', textAlign: 'center' }}>Contact page coming soon</div>} />
        <Route path="/privacy" element={<div className="container" style={{ padding: '48px 0', textAlign: 'center' }}>Privacy page coming soon</div>} />
        <Route path="/terms" element={<div className="container" style={{ padding: '48px 0', textAlign: 'center' }}>Terms page coming soon</div>} />
        <Route path="/forgot-password" element={<div className="container" style={{ padding: '48px 0', textAlign: 'center' }}>Forgot password page coming soon</div>} />
      </Routes>
    </Router>
  );
}

export default App;
