export interface UserDto {
  id: number;
  name: string;
  email: string;
  roles: string[];
  createdAt: string;
  emailConfirmed?: boolean;
}

export interface AuthData {
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  user: UserDto;
}

export interface ApiResponse<T = unknown> {
  success: boolean;
  message: string;
  errors?: Record<string, string[]>;
  data?: T;
}

export interface ProfileDto {
  id: number;
  name: string;
  email: string;
  roles: string[];
  emailConfirmed: boolean;
  createdAt: string;
  fullName: string;
  mobile: string | null;
  department: string | null;
  enrollmentNo: string | null;
  profileImageUrl: string | null;
  organizerRequestStatus: "Pending" | "Approved" | "Rejected" | null;
  organizationName: string | null;
}

export interface AdminDashboardStats {
  totalUsers: number;
  pendingRequests: number;
  approvedOrganizers: number;
  totalEvents: number;
}

export interface AdminOrganizerRequest {
  id: number;
  userId: number;
  userName: string;
  userEmail: string;
  organizationName: string;
  reason: string;
  experience: string | null;
  status: string;
  rejectionReason: string | null;
  reviewedBy: number | null;
  reviewedAt: string | null;
  createdAt: string;
}

// ───────────────────────────── Events ─────────────────────────────

export interface EventSummary {
  id: number;
  title: string;
  description: string | null;
  categoryId: number;
  categoryName: string;
  eventDate: string;
  eventTime: string;
  venue: string | null;
  organizerId: number;
  organizerName: string;
  maxParticipants: number;
  registeredCount: number;
  status: string;
  rejectionReason: string | null;
  imageUrl: string | null;
  registrationDeadline: string | null;
  isPaid: boolean;
  price: number;
  createdAt: string;
  updatedAt: string | null;
  isRegistered: boolean;
}

export interface EventCategory {
  id: number;
  name: string;
  description: string | null;
  eventCount: number;
}

export interface EventListResponse {
  events: EventSummary[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface OrganizerEventStats {
  totalEvents: number;
  draftEvents: number;
  pendingEvents: number;
  approvedEvents: number;
  rejectedEvents: number;
  cancelledEvents: number;
  completedEvents: number;
  totalRegistrations: number;
}

export interface RegistrationDto {
  id: number;
  eventId: number;
  eventTitle: string;
  eventDate: string;
  eventTime: string;
  eventVenue: string | null;
  status: string;
  registeredOn: string;
}

export interface AttendeeDto {
  userId: number;
  fullName: string;
  email: string;
  department: string | null;
  enrollmentNo: string | null;
  registeredOn: string;
  attended: boolean;
  checkedInAt: string | null;
}

export interface ReviewDto {
  id: number;
  eventId: number;
  userId: number;
  userName: string;
  rating: number;
  comment: string | null;
  submittedOn: string;
}

export interface EventReviewSummary {
  averageRating: number;
  totalReviews: number;
  reviews: ReviewDto[];
}

export interface FavoriteDto {
  eventId: number;
  eventTitle: string;
  eventDate: string;
  eventVenue: string | null;
  categoryName: string;
  bookmarkedOn: string;
}

export interface NotificationDto {
  id: number;
  title: string;
  message: string | null;
  type: string;
  relatedEntityId: number | null;
  relatedEntityType: string | null;
  actionUrl: string | null;
  isRead: boolean;
  createdAt: string;
  readAt: string | null;
}

// ───────────────────────────── Messaging ─────────────────────────────

export interface MessageDto {
  id: number;
  conversationId: number;
  senderId: number;
  content: string;
  sentAt: string;
  isRead: boolean;
  readAt: string | null;
}

export interface ConversationDto {
  id: number;
  otherUserId: number | null;
  otherUserName: string;
  createdAt: string;
  updatedAt: string;
  lastMessage: string | null;
  lastMessageAt: string | null;
  unreadCount: number;
}

export interface ConversationDetailDto {
  id: number;
  otherUserId: number | null;
  otherUserName: string;
  messages: MessageDto[];
}

export interface AdminEventDto {
  id: number;
  title: string;
  categoryName: string;
  eventDate: string;
  venue: string | null;
  organizerName: string;
  organizerEmail: string;
  maxParticipants: number;
  registeredCount: number;
  status: string;
  rejectionReason: string | null;
  createdAt: string;
}

// ───────────────────────────── Payment ─────────────────────────────

export interface PaymentStatus {
  id: number;
  amount: number;
  status: string;
  paidAt: string | null;
}

// ───────────────────────────── Digital Pass ─────────────────────────────

export interface DigitalPass {
  registrationId: number;
  eventId: number;
  eventTitle: string;
  eventDate: string;
  eventTime: string;
  venue: string;
  participantName: string;
  qrCodeBase64: string;
  checkInToken: string;
}

// ───────────────────────────── Attendance ─────────────────────────────

export interface AttendanceStats {
  totalRegistered: number;
  totalCheckedIn: number;
  totalPending: number;
  checkInPercentage: number;
}

// ───────────────────────────── Admin ─────────────────────────────

export interface AdminUser {
  id: number;
  email: string;
  fullName: string;
  role: string;
  isActive: boolean;
  suspendReason: string | null;
  createdAt: string;
}

export interface AdminUserDetail {
  id: number;
  email: string;
  fullName: string;
  mobile: string | null;
  department: string | null;
  enrollmentNo: string | null;
  roles: string[];
  isActive: boolean;
  suspendReason: string | null;
  createdAt: string;
}

export interface AdminReview {
  id: number;
  eventId: number;
  eventTitle: string;
  userId: number;
  userName: string;
  rating: number;
  comment: string | null;
  submittedOn: string;
}

export interface MediaItem {
  id: number;
  eventId: number;
  fileType: string;
  fileUrl: string;
  caption: string | null;
  uploadedOn: string;
}

export interface Certificate {
  id: number;
  eventId: number;
  eventTitle: string;
  certificateUrl: string;
  issuedOn: string;
  feePaid: boolean;
}

export interface WaitlistEntry {
  id: number;
  userId: number;
  userName: string;
  eventId: number;
  waitlistTime: string;
  status: string;
}

// ───────────────────────────── Calendar ─────────────────────────────

export interface CalendarEvent {
  id: number;
  title: string;
  eventDate: string;
  eventTime: string;
  venue: string | null;
  categoryName: string;
  imageUrl: string | null;
  status: string;
  registeredCount: number;
  maxParticipants: number;
  isRegistered: boolean;
}
