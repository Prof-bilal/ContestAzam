# Software Requirements Specification
### Version 1.0
## EventSphere

**Theme:** College Event Information System
**Category:** Full-Stack Application Development

© Aptech Limited

---

## Table of Contents

1. [1.1 Background and Necessity](#11-background-and-necessity-for-the-full-stack-web-application)
2. [1.2 Proposed Solution](#12-proposed-solution)
3. [1.3 Purpose of the Document](#13-purpose-of-the-document)
4. [1.4 Scope of the Project](#14-scope-of-the-project)
5. [1.5 Constraints](#15-constraints)
6. [1.6 Functional Requirements](#16-functional-requirements)
7. [1.7 Non-Functional Requirements](#17-non-functional-requirements)
8. [1.8 Interface Requirements](#18-interface-requirements)
9. [1.9 Project Deliverables](#19-project-deliverables)

---

## 1.1 Background and Necessity for the Full-Stack Web Application

Colleges and universities frequently organize technical competitions, cultural fests, academic events, and more. However, details about such events are often shared through noticeboards or group messages, leading to missed updates and low participation. Additionally, when the staff has to manage these events manually, it often leads to miscommunication, resource mismanagement, scheduling conflicts, and poor student engagement.

For students who are keen to participate in events, searching for accurate information and schedules can also be cumbersome in case of manually managed systems.

There may also be students who just want to view information about past and upcoming events without any intention of participating. In a manually managed system, they find it difficult to locate the information they are seeking.

Few issues and challenges with a traditional/manually managed system are as follows:

- **Ineffective Communication Channels**: Event details are often circulated through noticeboards, word-of-mouth, or scattered group messages leading to missed announcements and confusion among students.
- **Low Participation Rates**: Lack of timely and consistent updates results in low student turnout and missed opportunities for those genuinely interested in participating.
- **Manual Event Management Challenges**:
  - Staff face difficulties coordinating schedules, managing resources, and keeping track of registrations.
  - High potential for miscommunication and scheduling conflicts.
  - Limited visibility into past events or participation analytics.
- **Limited Accessibility for Students**: Students eager to participate must rely on informal sources or manually seek out information from faculty or peers.
- **Passive Participants are Overlooked**: Students who simply wish to browse past or upcoming events (for awareness or interest) find it difficult to locate such information in a manual system.

To overcome all these challenges, a centralized College Event Information System is necessary which provides real-time access to event information.

The system will serve as a digital platform where students, faculty, and other authorities can easily access event details, schedules, register for events, and receive notifications, all in one place.

---

## 1.2 Proposed Solution

The proposed solution is to develop a fully functional Web-based College Event Information System titled **'EventSphere'**. It will display upcoming and past events, allow student registrations for events, and enable administrators to add/edit/delete events. It will also provide features such as event categorization, notifications, image galleries, and so on.

---

## 1.3 Purpose of the Document

The purpose of this document is to outline the requirements for development of the **EventSphere** Full-stack application. This document will serve as a guide for the development team, ensuring that all stakeholders have a clear understanding of the project's objectives and functionalities.

---

## 1.4 Scope of the Project

**EventSphere** will be a responsive and visually appealing Full-stack application to be used by individuals via modern browsers on both desktop and mobile devices.

**EventSphere** enables users to plan, publish, and manage events; handle registrations with customizable forms; manage attendee participation, feedback, and certificate distribution; and track analytics. It supports role-based access for participants, organizers, and admins.

### Architecture Diagram

```
Web Browser (Chrome / Firefox)
        ↕ HTTP
    Web Server
        ↕
 Application Instance
        ↕
 Web Technologies for Processing
        ↕
    Database
```

---

## 1.5 Constraints

- Compatible with major browsers: Chrome, Firefox, Safari, Edge.
- Fully responsive for desktop and mobile.
- Fast load times, handle high traffic, real-time features without lag.

---

## 1.6 Functional Requirements

### User Roles

1. **Normal Student (Visitor)** — unregistered, browses public content only
2. **Participant (Registered Student)** — registers, attends, gets certificates, submits feedback
3. **Organizer (College Staff)** — creates/manages events, manages registrations, uploads media, issues certificates
4. **Admin (System Administrator)** — manages users, approves events, moderates content, sends announcements, generates reports

### Normal Student (Visitor)

- View comprehensive list of events (upcoming, ongoing, past)
- Browse detailed event info (title, description, date, time, venue, category, organizers)
- Filter events by category, department, event type, date range
- Access media gallery (images/videos from past events)
- View announcements, notifications, event banners on home page
- Access About Us, Contact Us, FAQs pages
- Prompted to login/signup when attempting restricted actions

### Participant (Registered Student)

- Register on platform (name, email, department, enrolment number)
- Secure login with personal dashboard
- Browse and register for events (subject to eligibility and slot availability)
- Real-time notifications and reminders
- Cancel registration before cutoff date
- QR code check-in for attendance
- Download e-certificate after attendance (upon payment of certificate fees)
- Submit feedback (star ratings + written comments)
- View past event history and participation status
- Bookmark events for future interest
- Save favorite images/videos to profile
- Option to pay certificate fees (payment processing out of scope)

### Organizer (College Staff)

- Login with institutional credentials or admin-approved account
- Organizer Dashboard (upcoming/ongoing/completed events, metrics)
- Create new events (title, category, description, venue, date/time, max participants, media upload)
- Events enter "Pending Approval" state until admin approves
- Edit, cancel, reschedule events (auto-notify registered participants)
- Monitor registrations in real time, view participant lists, approve/reject registrants
- QR code scanning for attendance, generate attendance reports
- Upload certificates for eligible participants
- Upload photos/videos to gallery, moderate feedback, communicate with participants

### Admin (System Administrator)

- Secure login with elevated credentials, possible 2FA
- Admin Dashboard (analytics: users by role, events by status, top departments, alerts)
- Approve, reject, or request changes to event proposals
- Manage all users (view profiles, assign roles, reset passwords, suspend/delete accounts)
- Moderate content (event descriptions, feedback, media uploads)
- Send system-wide announcements or targeted messages
- Generate and export reports (PDF/Excel): participation, feedback trends, user growth, certificates

### Additional Functionalities

1. **User Registration and Login** — registration with name, email, contact number, username, password; client-side validation
2. **Media Gallery** — categorized by event type, date, department (Cultural, Technical, Sports, Annual Day, Workshops, Competitions)
3. **User Dashboard** — event registration management, activity overview, notifications, saved media, profile settings, search and filter
4. **User Reviews** — user type selection, event feedback, rate components (venue, coordination, technical, hospitality), comment section, view peer reviews
5. **Dynamic Venue Capacity Management** — configurable seating limits, automatic capacity enforcement, live tracking, waitlist auto-adjustment
6. **Real-time Slot Availability** — live slot count, auto-updates, urgency visibility
7. **Calendar Integration** — Add to Calendar button, .ics format, timezone-aware syncing
8. **Social Media Sharing** — share buttons (Facebook, WhatsApp, Twitter, LinkedIn, Instagram, email), auto-filled messages, custom hashtags
9. **Certificate Fee Payment** — accept fee details (payment processing out of scope)
10. **Sitemap** — added to home page for navigation flow

---

## 1.7 Non-Functional Requirements

- **Safe to use** — no malicious downloads
- **Accessibility** — clear fonts, UI elements, navigation
- **User-friendliness** — easy to navigate
- **Operability** — reliable and efficient
- **Performance** — minimal load time, smooth redirection
- **Scalability** — handle increasing traffic and data
- **Security** — authentication for certain features
- **Availability** — 24/7 with minimum downtime
- **Compatibility** — latest browsers and devices

---

## 1.8 Interface Requirements

### 1.8.1 Hardware

- Intel Core i5/i7 or higher
- 8 GB RAM or higher
- Color SVGA monitor
- 500 GB Hard Disk
- Mouse, Keyboard

### 1.8.2 Software

- **IDE:** Appropriate IDE per platform
- **Frontend:** HTML5, CSS3, Bootstrap, ReactJS/AngularJS/Angular/TypeScript, JavaScript, jQuery, XML
- **Backend:** (choose one)
  - Java SDK with Apache NetBeans or Eclipse, Jakarta EE
  - C# with ASP.NET MVC and ASP.NET MVC Core (optional), Visual Studio IDE
  - PHP with Laravel Framework
  - Python with Flask or Django
  - MongoDB, Express.js, Angular, Node.js
  - MongoDB, Express.js, React, Node.js
- **Database:** MySQL/SQL Server

---

## Sample Database Structure

### Users

| Field | Type | Key | Description |
|---|---|---|---|
| user_id | INT | PK | Unique user ID |
| Email | VARCHAR(100) | UNIQUE | Login email |
| password | VARCHAR(255) | | Encrypted password |
| Role | ENUM | | participant, organizer, admin |
| created_at | DATETIME | | Creation timestamp |

### UserDetails

| Field | Type | Key | Description |
|---|---|---|---|
| detail_id | INT | PK | Detail record ID |
| user_id | INT | FK | References Users |
| full_name | VARCHAR(100) | | Full name |
| mobile | VARCHAR(15) | | Mobile number |
| department | VARCHAR(100) | | Academic department |
| enrollment_no | VARCHAR(50) | | Enrollment number |

### Events

| Field | Type | Key | Description |
|---|---|---|---|
| event_id | INT | PK | Event ID |
| Title | VARCHAR(150) | | Event title |
| description | TEXT | | Description |
| category | VARCHAR(50) | | Type: technical, cultural, etc. |
| Date | DATE | | Event date |
| Time | TIME | | Event time |
| Venue | VARCHAR(100) | | Location |
| organizer_id | INT | FK | References Users |

### Registrations

| Field | Type | Key | Description |
|---|---|---|---|
| registration_id | INT | PK | Registration ID |
| event_id | INT | FK | References Events |
| student_id | INT | FK | References Users |
| registered_on | DATETIME | | Registration timestamp |
| Status | ENUM | | confirmed, cancelled, waitlist |

### Attendance

| Field | Type | Key | Description |
|---|---|---|---|
| attendance_id | INT | PK | Attendance ID |
| event_id | INT | FK | References Events |
| student_id | INT | FK | References Users |
| attended | BOOLEAN | | TRUE if attended |
| marked_on | DATETIME | | Timestamp |

### Feedback

| Field | Type | Key | Description |
|---|---|---|---|
| feedback_id | INT | PK | Feedback ID |
| event_id | INT | FK | References Events |
| student_id | INT | FK | References Users |
| Rating | INT | | 1 to 5 |
| comments | TEXT | | Optional feedback |
| submitted_on | DATETIME | | Timestamp |

### Certificates

| Field | Type | Key | Description |
|---|---|---|---|
| certificate_id | INT | PK | Certificate ID |
| event_id | INT | FK | References Events |
| student_id | INT | FK | References Users |
| certificate_url | VARCHAR(255) | | File path/URL |
| issued_on | DATETIME | | Issue date |

### MediaGallery

| Field | Type | Key | Description |
|---|---|---|---|
| media_id | INT | PK | Media file ID |
| event_id | INT | FK | References Events |
| file_type | ENUM | | image, video |
| file_url | VARCHAR(255) | | File path/URL |
| uploaded_by | INT | FK | References Users |
| caption | VARCHAR(150) | | Optional caption |
| uploaded_on | DATETIME | | Upload timestamp |

### Event Seating

| Field | Type | Description |
|---|---|---|
| event_id | INT (PK, FK) | References event |
| venue_id | INT (FK) | References venue |
| total_seats | INT | Total seats |
| seats_booked | INT | Seats booked |
| seats_available | INT | Derived: total - booked |
| waitlist_enabled | BOOLEAN | Enable waitlist |

### Event Waitlist

| Field | Type | Description |
|---|---|---|
| waitlist_id | INT (PK) | Waitlist entry ID |
| user_id | INT (FK) | Waitlisted user |
| event_id | INT (FK) | Event |
| waitlist_time | DATETIME | Timestamp |
| status | ENUM | waiting, confirmed, cancelled |

### Calendar Sync

| Field | Type | Description |
|---|---|---|
| sync_id | INT (PK) | Sync ID |
| user_id | INT (FK) | User |
| event_id | INT (FK) | Event |
| calendar_type | VARCHAR | Google, Outlook, Apple |
| sync_timestamp | DATETIME | Sync time |
| calendar_url | VARCHAR | .ics URL |

### Event Share Log

| Field | Type | Description |
|---|---|---|
| share_id | INT (PK) | Share ID |
| user_id | INT (FK) | User |
| event_id | INT (FK) | Event |
| platform | VARCHAR | Facebook, WhatsApp, etc. |
| share_timestamp | DATETIME | Share time |
| share_message | TEXT | Message content |

---

## 1.9 Project Deliverables

- Problem Definition
- Design Specifications
- Diagrams (Flowcharts, DFDs, etc.)
- Database Design
- Test Data
- Project Installation Instructions
- User Credentials for all user types
- Sitemap on home page
- Video demonstration (.mp4) — MANDATORY
- Hosted working application (preferred)
- SQL scripts (.sql) for database and table definitions

---

*~~~ End of Document ~~~*

© Aptech Limited
