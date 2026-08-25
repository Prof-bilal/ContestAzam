# EventSphere Frontend - Authentication & Dashboard Guide

## 🎨 Authentication Pages

All auth pages follow the UX/UI guide principles with restrained design and minimal color usage.

### Pages Created

1. **Signup Page** (`/register`)
   - Role selection: Participant or Organizer
   - Beautiful role cards with icons
   - Form validation ready
   - Terms & conditions checkbox

2. **Login Page** (`/login`)
   - Clean login form
   - Remember me option
   - Forgot password link
   - Visitor note (no account needed to browse)

3. **Admin Login** (`/admin/login`) - **HIDDEN**
   - Secret access: Press `Ctrl+Shift+A` three times to unlock secret key field
   - Only you and Abdullah know this route
   - Extra security layer with secret key

## 🚀 How It Works

### For Abdullah (Backend Integration)

All forms are ready for API connection. Just update these functions:

#### Signup (`src/pages/Signup.jsx`)
```javascript
const handleSubmit = (e) => {
  e.preventDefault();
  
  // TODO: Abdullah - Connect to POST /api/auth/register
  // Send: formData (email, password, fullName, role)
  // Receive: JWT token + user role
  // Store JWT in localStorage
  // Redirect based on role
};
```

#### Login (`src/pages/Login.jsx`)
```javascript
const handleSubmit = (e) => {
  e.preventDefault();
  
  // TODO: Abdullah - Connect to POST /api/auth/login
  // Send: formData (email, password)
  // Receive: JWT token + user role
  // Store JWT in localStorage
  // Redirect to /dashboard/{role}
};
```

#### Admin Login (`src/pages/AdminLogin.jsx`)
```javascript
const handleSubmit = (e) => {
  e.preventDefault();
  
  // TODO: Abdullah - Connect to POST /api/auth/admin/login
  // Send: formData (email, password, secretKey)
  // Verify secret key on backend
  // Receive: JWT token with admin role
  // Redirect to /dashboard/admin
};
```

## 👥 User Roles & Routes

### Visitor (No Authentication)
- Can browse events without login
- Just navigate to `/events` or `/`
- No registration needed

### Participant
- Signup: Select "Participant" role
- Dashboard: `/dashboard/participant`
- Can: Register for events, view certificates, submit feedback

### Organizer
- Signup: Select "Organizer" role
- Dashboard: `/dashboard/organizer`
- Can: Create events, manage registrations, issue certificates

### Admin
- Login: Navigate to `/admin/login` (secret route)
- Press `Ctrl+Shift+A` three times to unlock secret key field
- Dashboard: `/dashboard/admin`
- Can: Manage users, moderate events, view reports

## 🎯 Dashboard Pages

All dashboard pages are placeholder blanks ready for content:
- `/dashboard/participant` - ParticipantDashboard.jsx
- `/dashboard/organizer` - OrganizerDashboard.jsx
- `/dashboard/admin` - AdminDashboard.jsx

## 🔐 Security Notes

1. **Admin Access**: 
   - Route `/admin/login` is not linked anywhere in UI
   - Secret key field requires `Ctrl+Shift+A` × 3
   - Backend must verify both admin credentials + secret key

2. **JWT Storage**:
   - Abdullah should store JWT in `localStorage` after login
   - Include role in JWT payload for route protection
   - Add protected route wrapper later

3. **Password Requirements**:
   - Minimum 8 characters (enforced in form)
   - Backend should enforce stronger rules

## 📝 Form Data Structure

### Signup Form
```javascript
{
  fullName: string,
  email: string,
  password: string,
  confirmPassword: string,
  role: 'participant' | 'organizer',
  agreeToTerms: boolean
}
```

### Login Form
```javascript
{
  email: string,
  password: string,
  rememberMe: boolean
}
```

### Admin Login Form
```javascript
{
  email: string,
  password: string,
  secretKey: string  // Only appears after secret sequence
}
```

## 🎨 Design Compliance

✅ Minimal color (only role icons with category chip colors)
✅ Hierarchy through spacing & typography
✅ Soft shadows on cards
✅ Clean form inputs with subtle hover/focus states
✅ Keyboard accessible
✅ Mobile responsive
✅ Consistent with landing page design system

## 🧪 Testing Routes

- Landing: http://localhost:5173/
- Signup: http://localhost:5173/register
- Login: http://localhost:5173/login
- Admin (secret): http://localhost:5173/admin/login
- Participant Dashboard: http://localhost:5173/dashboard/participant
- Organizer Dashboard: http://localhost:5173/dashboard/organizer
- Admin Dashboard: http://localhost:5173/dashboard/admin

---

**Ready for Abdullah to connect backend API!** 🚀
