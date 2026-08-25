# PHASES.md — Development Phases & Team Assignments

> EventSphere development broken into 4 phases with clear ownership per team member.

---

## Team

| Member | Role | Module | Focus |
|---|---|---|---|
| **Abdullah** | Backend | Module 1 | Backend Core & Architecture |
| **Jibran** | Backend | Module 2 | Database + Data-Heavy Backend |
| **Ramsha** | Frontend | Module 3 | Frontend Core + Shared UI |
| **Marukh** | Frontend | Module 4 | Frontend Features + Dashboards |

---

## Phase 1 — Foundation (Weeks 1–2)

> **Goal:** Core architecture, database, authentication, shared UI shell.

### Abdullah — Backend Foundation

| Task | Description |
|---|---|
| Project setup | ASP.NET Core solution structure, `Program.cs`, DI |
| Identity setup | ASP.NET Core Identity, user/role seeding |
| JWT authentication | Token generation, validation, middleware |
| Auth API endpoints | Login, register, logout, token refresh |
| Core middleware | Error handling, logging, CORS |
| API conventions | Response format, status codes, validation |
| SignalR setup | Hub architecture for notifications |

### Jibran — Database Foundation

| Task | Description |
|---|---|
| DbContext | `ApplicationDbContext` with all DbSets |
| Core entities | `AppUser`, `UserDetails`, `Event`, `EventCategory` |
| Entity configurations | Keys, relationships, indexes, constraints |
| Migrations | Initial migration + seed data |
| Seed data | Roles, admin user, categories |
| Database documentation | Schema diagram, ER model |

### Ramsha — Frontend Foundation

| Task | Description |
|---|---|
| Layout | `_Layout.cshtml` — navbar, footer, sidebar |
| Shared partials | `_LoginPartial`, `_EventCard`, alerts |
| CSS architecture | Bootstrap setup, custom variables, responsive base |
| Home page | Landing page with hero, stats, upcoming events |
| About/Contact pages | Static informational pages |
| Authentication UI | Login, register views |
| Shared forms | Form styles, validation states, buttons |
| Shared components | Cards, tables, modals, loading states |

### Marukh — Frontend Support

| Task | Description |
|---|---|
| Help Ramsha with layout | Assist with shared components |
| Sitemap page | Create sitemap view for navigation |
| Static pages | FAQ, Contact Us forms |
| CSS polish | Responsive testing, mobile fixes |

### Phase 1 Milestone

- [ ] API runs with JWT auth
- [ ] Database has core entities + seed data
- [ ] Login/Register UI works end-to-end
- [ ] Layout renders on all pages
- [ ] Home page displays upcoming events

---

## Phase 2 — Core Features (Weeks 3–4)

> **Goal:** Event management, registration, categories, search.

### Abdullah — Event API & Services

| Task | Description |
|---|---|
| Event service | CRUD operations, business logic |
| Event API endpoints | GET/POST/PUT/DELETE `/api/events` |
| Category service | Category management |
| Search/filter API | Query by category, date, keyword |
| Registration API | Register/cancel for events |
| Validation | Model validation on all endpoints |
| Error handling | Consistent error responses |

### Jibran — Database & Data Services

| Task | Description |
|---|---|
| Registration entities | `Registration`, `Attendance` |
| Seating entities | `EventSeating`, `EventWaitlist` |
| Waitlist logic | Auto-promotion on cancellation |
| Capacity enforcement | Prevent overbooking |
| Query optimization | Indexes for search/filter queries |
| Attendance tracking | QR code generation, check-in logic |
| Data services | Registration, attendance services |

### Ramsha — Event UI Core

| Task | Description |
|---|---|
| Event listing page | Paginated list with filters |
| Event detail page | Full event info, register button |
| Category navigation | Sidebar/tabs for categories |
| Search interface | Search bar, filter panel |
| Registration UI | Register/cancel buttons, confirmation |
| Pagination | Page controls for event lists |
| Empty states | "No events found" UI |
| Loading states | Skeleton screens |

### Marukh — Event Features UI

| Task | Description |
|---|---|
| Event creation form | Create event view (organizer) |
| Event editing form | Edit event view |
| My Events page | User's registered events |
| My Registrations | Registration history |
| QR code display | Show QR code for check-in |
| Ticket/certificate view | Display ticket info |
| Notification badges | Unread count indicator |

### Phase 2 Milestone

- [ ] Events CRUD works end-to-end
- [ ] Users can register/cancel for events
- [ ] Search and filtering works
- [ ] Event creation form works
- [ ] Capacity limits enforced

---

## Phase 3 — Advanced Features (Weeks 5–6)

> **Goal:** Dashboards, feedback, media, notifications, admin panel.

### Abdullah — Admin & Notification API

| Task | Description |
|---|---|
| Admin dashboard API | Analytics, user stats, event stats |
| User management API | List users, assign roles, suspend |
| Notification API | Create, list, mark read |
| SignalR notifications | Real-time push to connected users |
| Announcement API | System-wide announcements |
| Report generation API | Participation, feedback reports |
| Content moderation API | Approve/reject events, feedback |

### Jibran — Advanced Data

| Task | Description |
|---|---|
| Feedback entity | `Feedback` with ratings |
| Certificate entity | `Certificate` with URLs |
| Media entity | `MediaGallery` with file types |
| Calendar sync entity | `CalendarSync` |
| Share log entity | `EventShareLog` |
| Venue entity | `Venue` with capacity |
| Dashboard queries | Aggregation queries for analytics |
| Report data | Data queries for PDF/Excel export |

### Ramsha — Dashboard & Admin UI

| Task | Description |
|---|---|
| User dashboard | Activity overview, stats cards |
| Admin dashboard | Analytics charts, user management |
| User management table | List, search, role assignment |
| Notification center | List, mark read, real-time updates |
| Reports page | View/export reports |
| Admin event approval | Approve/reject event proposals |
| Content moderation | Review feedback, media |

### Marukh — Feature UI Completion

| Task | Description |
|---|---|
| Feedback form | Star ratings + comments |
| Review display | Show reviews on event page |
| Media gallery | Image/video grid per event |
| Media upload | Organizer upload interface |
| Calendar integration | Add to Calendar button (.ics) |
| Social sharing | Share buttons (FB, WhatsApp, Twitter) |
| Certificate download | Download e-certificate |
| Organizer dashboard | Event metrics, registration list |

### Phase 3 Milestone

- [ ] Admin dashboard shows analytics
- [ ] Users can submit feedback
- [ ] Media gallery works
- [ ] Notifications push in real-time
- [ ] Certificate download works
- [ ] Calendar export works

---

## Phase 4 — Polish & Delivery (Weeks 7–8)

> **Goal:** Testing, bug fixes, documentation, deployment, presentation.

### Abdullah — Backend Polish

| Task | Description |
|---|---|
| API documentation | Swagger/OpenAPI setup |
| Performance tuning | Query optimization, caching |
| Security audit | JWT, CORS, input validation |
| Error handling review | Global exception handler |
| Logging review | Structured logging |
| Health checks | `/health` endpoint |
| Final bug fixes | Backend issues |

### Jibran — Database Polish

| Task | Description |
|---|---|
| Migration review | Verify all migrations clean |
| Seed data update | Complete test data |
| Database performance | Index review, query plans |
| Backup strategy | Document backup/restore |
| Data integrity | Verify constraints |
| Final schema documentation | Updated ER diagram |

### Ramsha — Frontend Polish

| Task | Description |
|---|---|
| Responsive testing | Mobile, tablet, desktop |
| Accessibility | ARIA labels, keyboard nav |
| CSS cleanup | Consistent styles, remove duplicates |
| Cross-browser testing | Chrome, Firefox, Safari, Edge |
| Performance | Image optimization, bundle size |
| Final UI review | Consistency check |
| Sitemap update | Complete navigation sitemap |

### Marukh — Feature Polish

| Task | Description |
|---|---|
| Bug fixes | Frontend issues |
| Form validation | Client-side + server-side |
| Error pages | 404, 500, access denied |
| Loading improvements | Better UX for async operations |
| Interactive elements | Tooltips, confirmations |
| Video demo | Record project demonstration |
| Presentation prep | Slides, talking points |

### Phase 4 Milestone

- [ ] All features complete
- [ ] All tests pass
- [ ] Responsive on all devices
- [ ] No critical bugs
- [ ] Documentation complete
- [ ] Video demo recorded
- [ ] Ready for submission

---

## Phase Timeline

```
Week  1-2  ████████████████  Phase 1: Foundation
Week  3-4  ████████████████  Phase 2: Core Features
Week  5-6  ████████████████  Phase 3: Advanced Features
Week  7-8  ████████████████  Phase 4: Polish & Delivery
```

---

## Dependency Map

```
Phase 1 (Foundation)
├── Abdullah: Auth + API shell
├── Jibran: Database + entities
├── Ramsha: Layout + auth UI
└── Marukh: Static pages

        ↓

Phase 2 (Core Features)
├── Abdullah: Event API (needs Jibran's entities)
├── Jibran: Registration + capacity (needs Abdullah's services)
├── Ramsha: Event UI (needs Abdullah's API)
└── Marukh: Event forms (needs Ramsha's shared UI)

        ↓

Phase 3 (Advanced Features)
├── Abdullah: Admin + notification API
├── Jibran: Advanced entities + queries
├── Ramsha: Dashboard + admin UI
└── Marukh: Feature-specific UI

        ↓

Phase 4 (Polish & Delivery)
├── All: Testing, bug fixes, documentation
├── Abdullah: API docs, security
├── Jibran: DB performance, seed data
├── Ramsha: Responsive, accessibility
└── Marukh: Demo video, presentation
```

---

## Blocking Rules

| Blocked By | Blocks |
|---|---|
| Abdullah's API not ready | Ramsha & Marukh cannot test UI |
| Jibran's entities not ready | Abdullah cannot build services |
| Ramsha's layout not ready | Marukh cannot build feature pages |
| No API contract agreed | Frontend and backend diverge |

### How to Avoid Blocking

1. **Abdullah** publishes API contracts (routes, DTOs) before implementation.
2. **Jibran** shares entity diagrams before coding.
3. **Ramsha** publishes layout + shared components before feature pages.
4. **Marukh** uses shared components, doesn't create competing ones.

---

## Git Branch Strategy

```
main
├── develop
│   ├── feature/abdullah-auth
│   ├── feature/jibran-database
│   ├── feature/ramsha-layout
│   ├── feature/marukh-sitemap
│   ├── feature/abdullah-events-api
│   ├── feature/jibran-registration
│   ├── feature/ramsha-event-listing
│   ├── feature/marukh-event-forms
│   └── ...
```

---

## Definition of Done

A phase is complete when:

- All tasks in the phase are done
- Backend compiles and tests pass
- Frontend renders and forms work
- API endpoints respond correctly
- Database migrations are clean
- No regression in previous phases
- Documentation updated
- Code reviewed by at least one team member

---

## Daily Standup Format

Each day, each member answers:

1. **What did I complete yesterday?**
2. **What am I working on today?**
3. **Am I blocked by anyone?**

---

## Communication Rules

- Use a shared chat channel (Discord/Slack/WhatsApp).
- Announce API changes before making them.
- Announce schema changes before making them.
- Announce shared component changes before making them.
- Never merge to `main` without review.
