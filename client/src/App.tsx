import { Navigate, Route, Routes } from "react-router-dom";
import { Landing } from "./pages/Landing";
import { Login } from "./pages/Login";
import { Register } from "./pages/Register";
import { Dashboard } from "./pages/Dashboard";
import { OAuthCallback } from "./pages/OAuthCallback";
import { OAuthComplete } from "./pages/OAuthComplete";
import { VerifyEmail } from "./pages/VerifyEmail";
import { ForgotPassword } from "./pages/ForgotPassword";
import { ResetPassword } from "./pages/ResetPassword";
import { Profile } from "./pages/Profile";
import { AdminDashboard } from "./pages/AdminDashboard";
import { AdminOrganizerRequests } from "./pages/AdminOrganizerRequests";
import { EventDiscovery } from "./pages/EventDiscovery";
import { EventDetails } from "./pages/EventDetails";
import { OrganizerDashboard } from "./pages/OrganizerDashboard";
import { CreateEvent } from "./pages/CreateEvent";
import { EditEvent } from "./pages/EditEvent";
import { MyRegistrations } from "./pages/MyRegistrations";
import { EventAttendees } from "./pages/EventAttendees";
import { AdminEvents } from "./pages/AdminEvents";
import { OrganizerCategories } from "./pages/OrganizerCategories";
import { ProtectedRoute } from "./components/ProtectedRoute";
import { GuestRoute } from "./components/GuestRoute";

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<Landing />} />
      <Route path="/login" element={<GuestRoute><Login /></GuestRoute>} />
      <Route path="/register" element={<GuestRoute><Register /></GuestRoute>} />
      <Route path="/oauth/callback" element={<OAuthCallback />} />
      <Route path="/oauth/complete" element={<GuestRoute><OAuthComplete /></GuestRoute>} />
      <Route path="/verify-email" element={<VerifyEmail />} />
      <Route path="/forgot-password" element={<GuestRoute><ForgotPassword /></GuestRoute>} />
      <Route path="/reset-password" element={<GuestRoute><ResetPassword /></GuestRoute>} />

      {/* Public event pages */}
      <Route path="/events" element={<EventDiscovery />} />
      <Route path="/events/:id" element={<EventDetails />} />

      {/* Authenticated user pages */}
      <Route path="/dashboard" element={<ProtectedRoute><Dashboard /></ProtectedRoute>} />
      <Route path="/profile" element={<ProtectedRoute><Profile /></ProtectedRoute>} />
      <Route path="/my-registrations" element={<ProtectedRoute><MyRegistrations /></ProtectedRoute>} />

      {/* Organizer pages */}
      <Route path="/organizer/events" element={<ProtectedRoute><OrganizerDashboard /></ProtectedRoute>} />
      <Route path="/organizer/events/create" element={<ProtectedRoute><CreateEvent /></ProtectedRoute>} />
      <Route path="/organizer/events/:id/edit" element={<ProtectedRoute><EditEvent /></ProtectedRoute>} />
      <Route path="/organizer/events/:id/attendees" element={<ProtectedRoute><EventAttendees /></ProtectedRoute>} />
      <Route path="/organizer/categories" element={<ProtectedRoute><OrganizerCategories /></ProtectedRoute>} />

      {/* Admin pages */}
      <Route path="/admin" element={<ProtectedRoute><AdminDashboard /></ProtectedRoute>} />
      <Route path="/admin/organizer-requests" element={<ProtectedRoute><AdminOrganizerRequests /></ProtectedRoute>} />
      <Route path="/admin/events" element={<ProtectedRoute><AdminEvents /></ProtectedRoute>} />

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
