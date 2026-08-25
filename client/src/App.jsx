import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Navigation from './components/Navigation';
import Landing from './pages/Landing';
import './index.css';

function App() {
  return (
    <Router>
      <Navigation />
      <Routes>
        <Route path="/" element={<Landing />} />
        {/* Placeholder routes - to be created later */}
        <Route path="/events" element={<div className="container" style={{ padding: '48px 0', textAlign: 'center' }}>Events page coming soon</div>} />
        <Route path="/gallery" element={<div className="container" style={{ padding: '48px 0', textAlign: 'center' }}>Gallery page coming soon</div>} />
        <Route path="/about" element={<div className="container" style={{ padding: '48px 0', textAlign: 'center' }}>About page coming soon</div>} />
        <Route path="/login" element={<div className="container" style={{ padding: '48px 0', textAlign: 'center' }}>Login page coming soon</div>} />
        <Route path="/register" element={<div className="container" style={{ padding: '48px 0', textAlign: 'center' }}>Register page coming soon</div>} />
        <Route path="/dashboard" element={<div className="container" style={{ padding: '48px 0', textAlign: 'center' }}>Dashboard coming soon</div>} />
        <Route path="/help" element={<div className="container" style={{ padding: '48px 0', textAlign: 'center' }}>Help page coming soon</div>} />
        <Route path="/contact" element={<div className="container" style={{ padding: '48px 0', textAlign: 'center' }}>Contact page coming soon</div>} />
        <Route path="/privacy" element={<div className="container" style={{ padding: '48px 0', textAlign: 'center' }}>Privacy page coming soon</div>} />
      </Routes>
    </Router>
  );
}

export default App;
