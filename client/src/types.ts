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
  imageUrl: string | null;
  registrationDeadline: string | null;
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
  isRead: boolean;
  createdAt: string;
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
  createdAt: string;
}
