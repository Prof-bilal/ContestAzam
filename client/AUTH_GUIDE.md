# EventSphere Frontend - Authentication & Dashboard Guide

## 🎨 Authentication Pages

All auth pages follow the UX/UI guide principles with restrained design and minimal color usage.

### Pages Created

1. **Signup Page** (`/register`) - For Participants & Organizers
   - Role selection: Participant or Organizer
   - Beautiful role cards with icons
   - Form validation ready
   - Terms & conditions checkbox

2. **Login Page** (`/login`) - For Participants & Organizers
   - Clean login form
   - Remember me option
   - Forgot password link
   - Visitor note (no account needed to browse)

3. **Admin Auth Page** (`/admin/auth`) - **SECRET ROUTE**
   - Combined login AND registration for admins
   - Hidden shield logo - click 7 times to unlock
   - Admin code field (backend verification required)
   - Not linked anywhere in the UI

## 🔐 Admin Access Method

### Secret URL (Only share with authorized personnel)
Navigate to:
```
http://localhost:5173/admin/auth
```

Then:
1. **Click the shield logo 7 times within 3 seconds**
2. Shield turns purple - admin code field unlocked
3. Enter admin credentials + admin code
4. Choose "Sign In as Admin" or "Register as Admin"

### Why This Works

- **URL is not linked anywhere** - regular users won't find it
- **Shield click sequence** - prevents accidental access
- **Admin code required** - backend must verify this code
- **Dual mode** - both login and registration on same page

## 🚀 How It Works

### For Abdullah (Backend Integration)

#### Regular Signup (`src/pages/Signup.jsx`)
```javascript
// POST /api/auth/register
{
  fullName: string,
  email: string,
  password: string,
  role: 'participant' | 'organizer'
}
```

#### Regular Login (`src/pages/Login.jsx`)
```javascript
// POST /api/auth/login
{
  email: string,
  password: string
}
// Response: JWT token + role
// Redirect to /dashboard/{role}
```

#### Admin Auth (`src/pages/AdminAuth.jsx`)

**Admin Login:**
```javascript
// POST /api/auth/admin/login
{
  email: string,
  password: string,
  adminCode: string  // Backend must verify this!
}
```

**Admin Registration:**
```javascript
// POST /api/auth/admin/register
{
  fullName: string,
  email: string,
  password: string,
  adminCode: string  // Must match backend secret code
}
// Only create admin if adminCode is valid
```

## 👥 User Roles & Routes

### Visitor (No Authentication)
- Can browse events without login
- Navigate to `/events` or `/`
- No registration needed

### Participant
- Signup at `/register` → Select "Participant" role
- Login at `/login`
- Dashboard: `/dashboard/participant`
- Can: Register for events, view certificates, submit feedback

### Organizer
- Signup at `/register` → Select "Organizer" role
- Login at `/login`
- Dashboard: `/dashboard/organizer`
- Can: Create events, manage registrations, issue certificates

### Admin
- Access secret URL: `/admin/auth`
- Click shield logo 7 times to unlock admin code field
- Login or Register with admin code
- Dashboard: `/dashboard/admin`
- Can: Manage users, moderate events, view reports

## 🎯 Dashboard Pages

All dashboard pages are placeholder blanks ready for content:
- `/dashboard/participant` - ParticipantDashboard.jsx
- `/dashboard/organizer` - OrganizerDashboard.jsx
- `/dashboard/admin` - AdminDashboard.jsx

## 🔒 Security Implementation

### Frontend Security (What We Built)

1. **Hidden URL**: `/admin/auth` not linked anywhere
2. **Click sequence**: 7 clicks on shield logo within 3 seconds
3. **Visual feedback**: Shield turns purple when unlocked
4. **Admin code field**: Only appears after unlock sequence
5. **Form disabled**: Can't submit without admin code

### Backend Security (Abdullah's Task)

**Critical**: Backend must verify admin code on EVERY admin auth request:

```javascript
// Backend validation logic (pseudo-code)
const ADMIN_CODE = process.env.ADMIN_SECRET_CODE; // e.g., "ES-ADMIN-2026"

// Admin Login
if (req.body.adminCode !== ADMIN_CODE) {
  return res.status(403).json({ error: 'Invalid admin code' });
}
// Verify credentials
// Check if user exists with role 'admin' in database
// Return JWT with admin role

// Admin Registration
if (req.body.adminCode !== ADMIN_CODE) {
  return res.status(403).json({ error: 'Invalid admin code' });
}
// Create user with role: 'admin'
// Return JWT with admin role
```

### Why This Is Secure

- **Frontend is just UI** - doesn't grant real access
- **Backend controls everything** - validates admin code + credentials
- **Can't bypass** - even if someone finds URL, they need admin code
- **Hidden from discovery** - no links, no hints in public UI
- **Easy to change** - update admin code in backend env variable

## 📝 Form Data Structures

### Signup Form (Participant/Organizer)
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

### Login Form (Participant/Organizer)
```javascript
{
  email: string,
  password: string,
  rememberMe: boolean
}
```

### Admin Auth Form
```javascript
{
  fullName: string,        // Only for registration
  email: string,
  password: string,
  confirmPassword: string, // Only for registration
  adminCode: string        // Required for both login & register
}
```

## 🎨 Design Compliance

✅ Minimal color (role icons, purple admin shield when active)
✅ Hierarchy through spacing & typography
✅ Soft shadows on cards
✅ Clean form inputs with subtle hover/focus states
✅ Admin shield icon with unlock animation
✅ Keyboard accessible
✅ Mobile responsive
✅ Consistent with landing page design system

## 🧪 Testing Routes

### Public Access
- Landing: http://localhost:5173/
- Signup: http://localhost:5173/register (Participant/Organizer)
- Login: http://localhost:5173/login (Participant/Organizer)
- Events (Visitors): http://localhost:5173/events

### Admin Access (Secret)
1. Navigate to: http://localhost:5173/admin/auth
2. Click shield logo 7 times quickly (within 3 seconds)
3. Shield turns purple, admin code field appears
4. Enter credentials + admin code
5. Choose login or register

### Dashboards
- Participant: http://localhost:5173/dashboard/participant
- Organizer: http://localhost:5173/dashboard/organizer
- Admin: http://localhost:5173/dashboard/admin

## 📋 Admin Access Instructions (Share Only With Authorized Team)

**For You & Abdullah:**

### To Access Admin Panel:
1. Open browser and go to: `http://localhost:5173/admin/auth`
2. Click the shield icon at the top **7 times within 3 seconds**
3. Shield will turn purple - admin code field unlocked
4. Enter your admin email, password, and admin code
5. Click "Access Admin Panel" to log in

### To Register New Admin:
1. Follow steps 1-3 above to unlock admin code field
2. Enter full name, email, password, confirm password, and admin code
3. Click "Register as Admin" to create account
4. Admin code must match the secret code in backend environment

### What is Admin Code?
- A secret password shared only among admins
- Stored in backend environment variable
- Backend verifies it on every admin auth request
- Different from login password (additional layer)

**Example Admin Code**: `ES-ADMIN-2026` (Abdullah will set the real one)

---

**Ready for Abdullah to connect backend API!** 🚀

## 🔑 Quick Reference

| Page | URL | Users |
|------|-----|-------|
| Landing | `/` | Everyone (visitors) |
| Signup | `/register` | Participants & Organizers |
| Login | `/login` | Participants & Organizers |
| Admin Auth | `/admin/auth` | **Admins only (secret)** |
| Participant Dashboard | `/dashboard/participant` | Participants |
| Organizer Dashboard | `/dashboard/organizer` | Organizers |
| Admin Dashboard | `/dashboard/admin` | Admins |
