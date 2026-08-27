import type { ApiResponse, AuthData, UserDto, ProfileDto, AdminDashboardStats, AdminOrganizerRequest, EventSummary, EventCategory, EventListResponse, OrganizerEventStats, RegistrationDto, AttendeeDto, ReviewDto, EventReviewSummary, FavoriteDto, NotificationDto, AdminEventDto, PaymentStatus, DigitalPass, AttendanceStats, MessageDto, ConversationDto, ConversationDetailDto, CalendarEvent } from "../types";

const API_BASE = import.meta.env.VITE_API_BASE_URL ?? "";

// The access token lives in memory only (never localStorage) to minimize XSS
// exposure. The refresh token is an HttpOnly cookie the browser manages for us.
let accessToken: string | null = null;
export const setAccessToken = (token: string | null) => {
  accessToken = token;
};

// Callback invoked when a refresh detects the account is suspended.
let onSuspended: ((reason: string | null) => void) | null = null;
export const setOnSuspended = (cb: ((reason: string | null) => void) | null) => {
  onSuspended = cb;
};
/// Read by the SignalR client for hub authentication.
export const getAccessToken = () => accessToken;

// Prevent concurrent refresh requests from revoking the entire token family.
// All callers share the same in-flight promise; only the first 401 triggers a
// real refresh, and subsequent ones wait for the result.
let pendingRefresh: Promise<boolean> | null = null;

export class ApiError extends Error {
  status: number;
  errors?: Record<string, string[]>;
  constructor(status: number, message: string, errors?: Record<string, string[]>) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.errors = errors;
  }
}

export class RateLimitError extends Error {
  retryAfterSeconds: number;
  constructor(retryAfterSeconds: number, message: string) {
    super(message);
    this.name = "RateLimitError";
    this.retryAfterSeconds = retryAfterSeconds;
  }
}

export class NetworkError extends Error {
  constructor() {
    super("Unable to connect to the server.");
    this.name = "NetworkError";
  }
}

export class SuspendedError extends Error {
  reason: string | null;
  constructor(message: string, reason: string | null = null) {
    super(message);
    this.name = "SuspendedError";
    this.reason = reason;
  }
}

interface RequestOptions {
  method?: string;
  body?: unknown;
  auth?: boolean; // attach the bearer token (default true)
  allowRefresh?: boolean; // attempt a silent refresh on 401 (default true)
}

async function request<T>(path: string, opts: RequestOptions = {}): Promise<ApiResponse<T>> {
  const { method = "GET", body, auth = true, allowRefresh = true } = opts;

  const headers: Record<string, string> = {};
  if (body !== undefined) headers["Content-Type"] = "application/json";
  if (auth && accessToken) headers["Authorization"] = `Bearer ${accessToken}`;

  let res: Response;
  try {
    res = await fetch(`${API_BASE}${path}`, {
      method,
      headers,
      credentials: "include", // send/receive the refresh cookie
      body: body !== undefined ? JSON.stringify(body) : undefined,
    });
  } catch {
    throw new NetworkError();
  }

  if (res.status === 429) {
    const raHeader = parseInt(res.headers.get("Retry-After") ?? "", 10);
    let message = "Too many attempts. Please try again shortly.";
    try {
      const parsed = (await res.json()) as ApiResponse;
      if (parsed?.message) message = parsed.message;
    } catch {
      /* no body */
    }
    throw new RateLimitError(Number.isFinite(raHeader) && raHeader > 0 ? raHeader : 30, message);
  }

  if (res.status === 401 && allowRefresh && auth) {
    const refreshed = await tryRefresh();
    if (refreshed) return request<T>(path, { ...opts, allowRefresh: false });
  }

  let payload: ApiResponse<T> | null = null;
  try {
    payload = (await res.json()) as ApiResponse<T>;
  } catch {
    /* no body */
  }

  // Handle suspended account — 403 with accountSuspended error code.
  if (res.status === 403 && payload?.errors?.accountSuspended !== undefined) {
    setAccessToken(null);
    throw new SuspendedError(
      payload.message || "Your account has been suspended.",
      typeof payload.errors.accountSuspended === "string" ? payload.errors.accountSuspended : null,
    );
  }

  if (!res.ok || !payload || payload.success === false) {
    throw new ApiError(res.status, payload?.message ?? "Request failed.", payload?.errors);
  }
  return payload;
}

async function tryRefresh(): Promise<boolean> {
  // If a refresh is already in-flight, wait for it instead of firing another
  // one (which would revoke the freshly-rotated token).
  if (pendingRefresh) return pendingRefresh;

  pendingRefresh = (async () => {
    try {
      const res = await fetch(`${API_BASE}/api/auth/refresh`, {
        method: "POST",
        credentials: "include",
      });

      // Detect account suspension during refresh.
      if (res.status === 403) {
        let reason: string | null = null;
        try {
          const body = (await res.json()) as ApiResponse;
          if (body.errors?.accountSuspended !== undefined) {
            reason = typeof body.errors.accountSuspended === "string" ? body.errors.accountSuspended : null;
          }
        } catch { /* no body */ }
        setAccessToken(null);
        onSuspended?.(reason);
        return false;
      }

      if (!res.ok) {
        setAccessToken(null);
        return false;
      }
      const body = (await res.json()) as ApiResponse<AuthData>;
      if (body.success && body.data) {
        setAccessToken(body.data.accessToken);
        return true;
      }
    } catch {
      /* fall through */
    }
    setAccessToken(null);
    return false;
  })();

  try {
    return await pendingRefresh;
  } finally {
    pendingRefresh = null;
  }
}

export async function login(email: string, password: string): Promise<UserDto> {
  const res = await request<AuthData>("/api/auth/login", {
    method: "POST",
    body: { email, password },
    auth: false,
  });
  setAccessToken(res.data!.accessToken);
  return res.data!.user;
}

export async function register(
  name: string,
  email: string,
  password: string,
  confirmPassword: string,
  accountType: "Visitor" | "Organizer" = "Visitor",
  organizationName?: string,
  organizationReason?: string,
  organizationExperience?: string,
  department?: string,
  enrollmentNo?: string,
): Promise<UserDto> {
  const body: Record<string, unknown> = { name, email, password, confirmPassword, accountType };
  if (accountType === "Organizer") {
    body.organizationName = organizationName;
    body.organizationReason = organizationReason;
    body.organizationExperience = organizationExperience;
  }
  if (department) body.department = department;
  if (enrollmentNo) body.enrollmentNo = enrollmentNo;
  const res = await request<AuthData>("/api/auth/register", {
    method: "POST",
    body,
    auth: false,
  });
  setAccessToken(res.data!.accessToken);
  return res.data!.user;
}

/// Restores a session from the refresh cookie on app load or after OAuth.
export async function bootstrap(): Promise<UserDto | null> {
  if (!(await tryRefresh())) return null;
  try {
    const res = await request<UserDto>("/api/auth/me", { allowRefresh: false });
    return res.data ?? null;
  } catch {
    return null;
  }
}

export async function logout(): Promise<void> {
  try {
    await fetch(`${API_BASE}/api/auth/logout`, { method: "POST", credentials: "include" });
  } catch {
    /* best-effort */
  }
  setAccessToken(null);
}

export interface DemoResult {
  ok: boolean;
  status: number;
  message: string;
}

export async function demo(area: string): Promise<DemoResult> {
  try {
    const res = await request<unknown>(`/api/demo/${area}`);
    return { ok: true, status: 200, message: res.message };
  } catch (e) {
    if (e instanceof ApiError) return { ok: false, status: e.status, message: e.message };
    if (e instanceof NetworkError) return { ok: false, status: 0, message: e.message };
    throw e;
  }
}

export const oauthUrl = (provider: "google" | "github") =>
  `${API_BASE}/api/auth/external/${provider}`;

export async function verifyEmail(email: string, token: string): Promise<void> {
  await request<unknown>("/api/auth/verify-email", {
    method: "POST",
    body: { email, token },
    auth: false,
  });
}

export async function resendVerification(email: string): Promise<void> {
  await request<unknown>("/api/auth/resend-verification", {
    method: "POST",
    body: { email },
    auth: false,
  });
}

export async function forgotPassword(email: string): Promise<void> {
  await request<unknown>("/api/auth/forgot-password", {
    method: "POST",
    body: { email },
    auth: false,
  });
}

export async function resetPassword(
  email: string,
  token: string,
  newPassword: string,
  confirmPassword: string,
): Promise<void> {
  await request<unknown>("/api/auth/reset-password", {
    method: "POST",
    body: { email, token, newPassword, confirmPassword },
    auth: false,
  });
}

// --------------------------------------------------------- OAuth Complete Registration

export async function completeOAuthRegistration(
  pendingToken: string,
  accountType: "Visitor" | "Organizer",
  organizationName?: string,
  organizationReason?: string,
  organizationExperience?: string,
  profileImageUrl?: string,
): Promise<UserDto> {
  const body: Record<string, unknown> = { pendingToken, accountType };
  if (accountType === "Organizer") {
    body.organizationName = organizationName;
    body.organizationReason = organizationReason;
    body.organizationExperience = organizationExperience;
  }
  if (profileImageUrl) body.profileImageUrl = profileImageUrl;
  const res = await request<AuthData>("/api/auth/external/complete", {
    method: "POST",
    body,
    auth: false,
  });
  setAccessToken(res.data!.accessToken);
  return res.data!.user;
}

// --------------------------------------------------------- Profile

export async function getProfile(): Promise<ProfileDto> {
  const res = await request<ProfileDto>("/api/profile");
  return res.data!;
}

export async function updateProfile(
  fullName: string,
  mobile?: string,
  department?: string,
  profileImageUrl?: string,
  enrollmentNo?: string,
): Promise<void> {
  await request<unknown>("/api/profile", {
    method: "PUT",
    body: { fullName, mobile, department, profileImageUrl, enrollmentNo },
  });
}

export async function deleteAccount(): Promise<void> {
  await request<unknown>("/api/profile", {
    method: "DELETE",
    body: { confirmation: "DELETE" },
  });
}

export async function changePassword(currentPassword: string, newPassword: string): Promise<void> {
  await request<unknown>("/api/profile/change-password", {
    method: "POST",
    body: { currentPassword, newPassword },
  });
}

export async function uploadProfileImage(file: File): Promise<string> {
  const formData = new FormData();
  formData.append("file", file);

  const headers: Record<string, string> = {};
  if (accessToken) headers["Authorization"] = `Bearer ${accessToken}`;

  const res = await fetch(`${API_BASE}/api/profile/image`, {
    method: "POST",
    headers,
    credentials: "include",
    body: formData,
  });

  const payload = (await res.json()) as ApiResponse<{ url: string }>;
  if (!res.ok || !payload.success) {
    throw new ApiError(res.status, payload.message || "Upload failed.");
  }
  return payload.data!.url;
}

// --------------------------------------------------------- Organizer Requests (user)

export async function submitOrganizerRequest(
  organizationName: string,
  reason: string,
  experience?: string,
): Promise<void> {
  await request<unknown>("/api/auth/organizer-requests", {
    method: "POST",
    body: { organizationName, reason, experience },
  });
}

export async function getMyOrganizerRequest(): Promise<{
  id: number;
  organizationName: string;
  reason: string;
  experience: string | null;
  status: string;
  rejectionReason: string | null;
  reviewedAt: string | null;
  createdAt: string;
} | null> {
  try {
    const res = await request<{
      id: number;
      organizationName: string;
      reason: string;
      experience: string | null;
      status: string;
      rejectionReason: string | null;
      reviewedAt: string | null;
      createdAt: string;
    }>("/api/auth/organizer-requests/me");
    return res.data ?? null;
  } catch {
    return null;
  }
}

// --------------------------------------------------------- Admin

export async function getAdminDashboard(): Promise<AdminDashboardStats> {
  const res = await request<AdminDashboardStats>("/api/admin/dashboard");
  return res.data!;
}

export async function getAdminOrganizerRequests(
  status?: string,
): Promise<AdminOrganizerRequest[]> {
  const qs = status ? `?status=${encodeURIComponent(status)}` : "";
  const res = await request<AdminOrganizerRequest[]>(`/api/admin/organizer-requests${qs}`);
  return res.data ?? [];
}

export async function getAdminOrganizerRequest(
  id: number,
): Promise<AdminOrganizerRequest> {
  const res = await request<AdminOrganizerRequest>(`/api/admin/organizer-requests/${id}`);
  return res.data!;
}

export async function approveOrganizerRequest(id: number): Promise<void> {
  await request<unknown>(`/api/admin/organizer-requests/${id}/approve`, {
    method: "POST",
    body: {},
  });
}

export async function rejectOrganizerRequest(
  id: number,
  rejectionReason?: string,
): Promise<void> {
  await request<unknown>(`/api/admin/organizer-requests/${id}/reject`, {
    method: "POST",
    body: { rejectionReason: rejectionReason || null },
  });
}

// ───────────────────────────── Events (Public) ─────────────────────────────

export async function getEvents(params?: {
  search?: string;
  categoryId?: number;
  fromDate?: string;
  toDate?: string;
  location?: string;
  sortBy?: string;
  sortOrder?: string;
  page?: number;
  pageSize?: number;
}): Promise<EventListResponse> {
  const qs = new URLSearchParams();
  if (params?.search) qs.set("search", params.search);
  if (params?.categoryId) qs.set("categoryId", params.categoryId.toString());
  if (params?.fromDate) qs.set("fromDate", params.fromDate);
  if (params?.toDate) qs.set("toDate", params.toDate);
  if (params?.location) qs.set("location", params.location);
  if (params?.sortBy) qs.set("sortBy", params.sortBy);
  if (params?.sortOrder) qs.set("sortOrder", params.sortOrder);
  if (params?.page) qs.set("page", params.page.toString());
  if (params?.pageSize) qs.set("pageSize", params.pageSize.toString());
  const query = qs.toString();
  const res = await request<EventListResponse>(`/api/events${query ? `?${query}` : ""}`);
  return res.data!;
}

export async function getEvent(id: number): Promise<EventSummary> {
  const res = await request<EventSummary>(`/api/events/${id}`);
  return res.data!;
}

export async function getCategories(): Promise<EventCategory[]> {
  const res = await request<EventCategory[]>("/api/events/categories", { auth: false });
  return res.data ?? [];
}

// ───────────────────────────── Categories (Organizer CRUD) ─────────────────────────────

export async function getOrganizerCategories(): Promise<EventCategory[]> {
  const res = await request<EventCategory[]>("/api/organizer/categories");
  return res.data ?? [];
}

export async function createCategory(data: { name: string; description?: string }): Promise<EventCategory> {
  const res = await request<EventCategory>("/api/organizer/categories", {
    method: "POST",
    body: data,
  });
  return res.data!;
}

export async function updateCategory(id: number, data: { name: string; description?: string }): Promise<EventCategory> {
  const res = await request<EventCategory>(`/api/organizer/categories/${id}`, {
    method: "PUT",
    body: data,
  });
  return res.data!;
}

export async function deleteCategory(id: number): Promise<void> {
  await request<void>(`/api/organizer/categories/${id}`, { method: "DELETE" });
}

// ───────────────────────────── Image Upload ─────────────────────────────

export async function uploadImage(file: File): Promise<string> {
  const formData = new FormData();
  formData.append("file", file);

  const headers: Record<string, string> = {};
  if (accessToken) headers["Authorization"] = `Bearer ${accessToken}`;

  const res = await fetch(`${API_BASE}/api/organizer/upload-image`, {
    method: "POST",
    headers,
    credentials: "include",
    body: formData,
  });

  const payload = (await res.json()) as ApiResponse<{ url: string }>;
  if (!res.ok || !payload.success) {
    throw new ApiError(res.status, payload.message || "Upload failed.");
  }
  return payload.data!.url;
}

// ───────────────────────────── Events (Organizer CRUD) ─────────────────────────────

export async function createEvent(data: {
  title: string;
  description?: string;
  categoryId: number;
  eventDate: string;
  eventTime: string;
  venue?: string;
  maxParticipants: number;
  imageUrl?: string;
  registrationDeadline?: string;
  isPaid?: boolean;
  price?: number;
  saveAsDraft?: boolean;
}): Promise<EventSummary> {
  const res = await request<EventSummary>("/api/events", { method: "POST", body: data });
  return res.data!;
}

export async function updateEvent(
  id: number,
  data: {
    title: string;
    description?: string;
    categoryId: number;
    eventDate: string;
    eventTime: string;
    venue?: string;
    maxParticipants: number;
    imageUrl?: string;
    registrationDeadline?: string;
    isPaid?: boolean;
    price?: number;
  },
): Promise<EventSummary> {
  const res = await request<EventSummary>(`/api/events/${id}`, { method: "PUT", body: data });
  return res.data!;
}

export async function deleteEvent(id: number): Promise<void> {
  await request<unknown>(`/api/events/${id}`, { method: "DELETE" });
}

export async function publishEvent(id: number): Promise<void> {
  await request<unknown>(`/api/events/${id}/publish`, { method: "PATCH", body: {} });
}

export async function cancelEvent(id: number): Promise<void> {
  await request<unknown>(`/api/events/${id}/cancel`, { method: "PATCH", body: {} });
}

// ───────────────────────────── Events (Registration) ─────────────────────────────

export async function registerForEvent(id: number): Promise<void> {
  await request<unknown>(`/api/events/${id}/register`, { method: "POST", body: {} });
}

export async function cancelRegistration(id: number): Promise<void> {
  await request<unknown>(`/api/events/${id}/register`, { method: "DELETE" });
}

// ───────────────────────────── Events (Reviews) ─────────────────────────────

export async function getEventReviews(id: number): Promise<EventReviewSummary> {
  const res = await request<EventReviewSummary>(`/api/events/${id}/reviews`, { auth: false });
  return res.data!;
}

export async function submitReview(
  eventId: number,
  rating: number,
  comment?: string,
): Promise<ReviewDto> {
  const res = await request<ReviewDto>(`/api/events/${eventId}/reviews`, {
    method: "POST",
    body: { rating, comment },
  });
  return res.data!;
}

// ───────────────────────────── Participant ─────────────────────────────

export async function getMyRegistrations(): Promise<RegistrationDto[]> {
  const res = await request<RegistrationDto[]>("/api/participant/registrations");
  return res.data ?? [];
}

export async function cancelMyRegistration(id: number): Promise<void> {
  await request<unknown>(`/api/participant/registrations/${id}`, { method: "DELETE" });
}

export async function addFavorite(eventId: number): Promise<void> {
  await request<unknown>(`/api/participant/favorites/${eventId}`, { method: "POST", body: {} });
}

export async function removeFavorite(eventId: number): Promise<void> {
  await request<unknown>(`/api/participant/favorites/${eventId}`, { method: "DELETE" });
}

export async function getMyFavorites(): Promise<FavoriteDto[]> {
  const res = await request<FavoriteDto[]>("/api/participant/favorites");
  return res.data ?? [];
}

export async function deleteMyReview(id: number): Promise<void> {
  await request<unknown>(`/api/participant/reviews/${id}`, { method: "DELETE" });
}

export async function getMyNotifications(page = 1, pageSize = 20): Promise<NotificationDto[]> {
  const res = await request<NotificationDto[]>(`/api/notifications?page=${page}&pageSize=${pageSize}`);
  return res.data ?? [];
}

export async function getUnreadNotificationCount(): Promise<number> {
  const res = await request<{ count: number }>("/api/notifications/unread-count");
  return res.data?.count ?? 0;
}

export async function markNotificationRead(id: number): Promise<void> {
  await request<unknown>(`/api/notifications/${id}/read`, { method: "PATCH", body: {} });
}

export async function markNotificationUnread(id: number): Promise<void> {
  await request<unknown>(`/api/notifications/${id}/unread`, { method: "PATCH", body: {} });
}

export async function markAllNotificationsRead(): Promise<void> {
  await request<unknown>("/api/notifications/read-all", { method: "PATCH", body: {} });
}

// ───────────────────────────── Messaging ─────────────────────────────

export async function getConversations(): Promise<ConversationDto[]> {
  const res = await request<ConversationDto[]>("/api/conversations");
  return res.data ?? [];
}

export async function getConversation(id: number): Promise<ConversationDetailDto> {
  const res = await request<ConversationDetailDto>(`/api/conversations/${id}`);
  return res.data!;
}

export async function createConversation(recipientId: number): Promise<ConversationDetailDto> {
  const res = await request<ConversationDetailDto>("/api/conversations", {
    method: "POST",
    body: { recipientId },
  });
  return res.data!;
}

export async function sendMessage(conversationId: number, content: string): Promise<MessageDto> {
  const res = await request<MessageDto>(`/api/conversations/${conversationId}/messages`, {
    method: "POST",
    body: { content },
  });
  return res.data!;
}

export async function markConversationRead(conversationId: number): Promise<void> {
  await request<unknown>(`/api/conversations/${conversationId}/read`, { method: "POST", body: {} });
}

export async function getUnreadMessageCount(): Promise<number> {
  const res = await request<{ count: number }>("/api/conversations/unread-count");
  return res.data?.count ?? 0;
}

// ───────────────────────────── Organizer ─────────────────────────────

export async function getOrganizerEvents(params?: {
  status?: string;
  categoryId?: number;
  search?: string;
  sortBy?: string;
  sortOrder?: string;
  page?: number;
  pageSize?: number;
}): Promise<EventSummary[]> {
  const qs = new URLSearchParams();
  if (params?.status) qs.set("status", params.status);
  if (params?.categoryId) qs.set("categoryId", params.categoryId.toString());
  if (params?.search) qs.set("search", params.search);
  if (params?.sortBy) qs.set("sortBy", params.sortBy);
  if (params?.sortOrder) qs.set("sortOrder", params.sortOrder);
  if (params?.page) qs.set("page", params.page.toString());
  if (params?.pageSize) qs.set("pageSize", params.pageSize.toString());
  const query = qs.toString();
  const res = await request<EventSummary[]>(`/api/organizer/events${query ? `?${query}` : ""}`);
  return res.data ?? [];
}

export async function getOrganizerStats(): Promise<OrganizerEventStats> {
  const res = await request<OrganizerEventStats>("/api/organizer/events/stats");
  return res.data!;
}

export async function getOrganizerCalendar(params?: {
  fromDate?: string;
  toDate?: string;
}): Promise<EventSummary[]> {
  const qs = new URLSearchParams();
  if (params?.fromDate) qs.set("fromDate", params.fromDate);
  if (params?.toDate) qs.set("toDate", params.toDate);
  const query = qs.toString();
  const res = await request<EventSummary[]>(`/api/organizer/events/calendar${query ? `?${query}` : ""}`);
  return res.data ?? [];
}

// ───────────────────────────── Calendar (in-app) ─────────────────────────────

export async function getCalendarEvents(params: {
  fromDate: string;
  toDate: string;
}): Promise<CalendarEvent[]> {
  const qs = new URLSearchParams();
  qs.set("fromDate", params.fromDate);
  qs.set("toDate", params.toDate);
  const res = await request<CalendarEvent[]>(`/api/events/calendar?${qs.toString()}`);
  return res.data ?? [];
}

export async function getEventAttendees(eventId: number): Promise<AttendeeDto[]> {
  const res = await request<AttendeeDto[]>(`/api/organizer/events/${eventId}/attendees`);
  return res.data ?? [];
}

export async function checkInAttendee(eventId: number, studentId: number): Promise<void> {
  await request<unknown>(`/api/organizer/events/${eventId}/attendees/${studentId}/check-in`, {
    method: "POST",
    body: {},
  });
}

// ───────────────────────────── Admin Events ─────────────────────────────

export async function getAdminEvents(params?: {
  status?: string;
  search?: string;
  categoryId?: number;
  sortBy?: string;
  sortOrder?: string;
  page?: number;
  pageSize?: number;
}): Promise<{ events: AdminEventDto[]; total: number; page: number; pageSize: number; totalPages: number }> {
  const qs = new URLSearchParams();
  if (params?.status) qs.set("status", params.status);
  if (params?.search) qs.set("search", params.search);
  if (params?.categoryId) qs.set("categoryId", params.categoryId.toString());
  if (params?.sortBy) qs.set("sortBy", params.sortBy);
  if (params?.sortOrder) qs.set("sortOrder", params.sortOrder);
  if (params?.page) qs.set("page", params.page.toString());
  if (params?.pageSize) qs.set("pageSize", params.pageSize.toString());
  const query = qs.toString();
  const res = await request<{ events: AdminEventDto[]; total: number; page: number; pageSize: number; totalPages: number }>(
    `/api/admin/events${query ? `?${query}` : ""}`,
  );
  return res.data!;
}

export async function approveEvent(id: number): Promise<void> {
  await request<unknown>(`/api/admin/events/${id}/approve`, { method: "PATCH", body: {} });
}

export async function rejectEvent(id: number, reason?: string): Promise<void> {
  await request<unknown>(`/api/admin/events/${id}/reject`, { method: "PATCH", body: { reason: reason || null } });
}

// ───────────────────────────── Payment ─────────────────────────────

export async function createCheckoutSession(eventId: number): Promise<{ url: string }> {
  const res = await request<{ url: string }>("/api/payment/create-checkout", {
    method: "POST",
    body: { eventId },
  });
  return res.data!;
}

export async function getPaymentStatus(eventId: number): Promise<PaymentStatus | null> {
  const res = await request<PaymentStatus>(`/api/payment/status/${eventId}`);
  return res.data!;
}

export async function getStripePublishableKey(): Promise<string> {
  const res = await request<{ key: string }>("/api/payment/publishable-key", { auth: false });
  return res.data!.key;
}

// ───────────────────────────── Digital Pass ─────────────────────────────

export async function getDigitalPass(registrationId: number): Promise<DigitalPass> {
  const res = await request<DigitalPass>(`/api/participant/registrations/${registrationId}/pass`);
  return res.data!;
}

// ───────────────────────────── Attendance ─────────────────────────────

export async function checkInByToken(token: string): Promise<{ success: boolean; message: string; attendeeName?: string; eventTitle?: string }> {
  const res = await request<{ success: boolean; message: string; attendeeName?: string; eventTitle?: string }>(
    "/api/organizer/events/attendance/check-in",
    { method: "POST", body: { token } },
  );
  return res.data!;
}

export async function getEventAttendance(eventId: number): Promise<AttendeeDto[]> {
  const res = await request<AttendeeDto[]>(`/api/organizer/events/${eventId}/attendance`);
  return res.data!;
}

export async function getAttendanceStats(eventId: number): Promise<AttendanceStats> {
  const res = await request<AttendanceStats>(`/api/organizer/events/${eventId}/attendance/stats`);
  return res.data!;
}

// ───────────────────────────── Admin User Management ─────────────────────────────

export async function getAdminUsers(params?: { search?: string; page?: number; pageSize?: number }) {
  const qs = new URLSearchParams();
  if (params?.search) qs.set("search", params.search);
  if (params?.page) qs.set("page", params.page.toString());
  const query = qs.toString();
  const res = await request<{ users: any[]; total: number; page: number; pageSize: number; totalPages: number }>(
    `/api/admin/users${query ? `?${query}` : ""}`
  );
  return res.data!;
}

export async function getAdminUser(id: number) {
  const res = await request<any>(`/api/admin/users/${id}`);
  return res.data!;
}

export async function toggleUserActive(id: number, reason?: string) {
  await request<unknown>(`/api/admin/users/${id}/toggle-active`, {
    method: "PATCH",
    body: reason !== undefined ? { reason } : {},
  });
}

export async function warnUser(id: number, message: string, sendEmail: boolean) {
  await request<unknown>(`/api/admin/users/${id}/warn`, {
    method: "POST",
    body: { message, sendEmail },
  });
}

export async function assignUserRole(id: number, role: string) {
  await request<unknown>(`/api/admin/users/${id}/roles`, { method: "POST", body: { role } });
}

export async function removeUserRole(id: number, role: string) {
  await request<unknown>(`/api/admin/users/${id}/roles/${role}`, { method: "DELETE" });
}

// ───────────────────────────── Admin Announcements ─────────────────────────────

export async function sendAnnouncement(title: string, message?: string) {
  await request<unknown>("/api/admin/announcements", { method: "POST", body: { title, message } });
}

// ───────────────────────────── Admin Reviews Moderation ─────────────────────────────

export async function getAdminReviews(page = 1) {
  const res = await request<{ reviews: any[]; total: number }>(`/api/admin/reviews?page=${page}`);
  return res.data!;
}

export async function deleteAdminReview(id: number) {
  await request<unknown>(`/api/admin/reviews/${id}`, { method: "DELETE" });
}

// ───────────────────────────── Admin Reports ─────────────────────────────

export function downloadParticipationReport() {
  window.open(`/api/admin/reports/participation`, "_blank");
}

export function downloadUserReport() {
  window.open(`/api/admin/reports/users`, "_blank");
}

// ───────────────────────────── Media Gallery ─────────────────────────────

export async function getEventMedia(eventId: number) {
  const res = await request<any[]>(`/api/organizer/events/${eventId}/media`);
  return res.data ?? [];
}

export async function uploadEventMedia(eventId: number, file: File, caption?: string) {
  const formData = new FormData();
  formData.append("file", file);
  if (caption) formData.append("caption", caption);
  const headers: Record<string, string> = {};
  if (accessToken) headers["Authorization"] = `Bearer ${accessToken}`;
  const res = await fetch(`${API_BASE}/api/organizer/events/${eventId}/media`, {
    method: "POST", headers, credentials: "include", body: formData,
  });
  const payload = await res.json();
  if (!res.ok || !payload.success) throw new ApiError(res.status, payload.message || "Upload failed.");
  return payload.data;
}

export async function deleteEventMedia(id: number) {
  await request<unknown>(`/api/organizer/media/${id}`, { method: "DELETE" });
}

// ───────────────────────────── Certificates ─────────────────────────────

export async function getMyCertificates() {
  const res = await request<any[]>("/api/participant/certificates");
  return res.data ?? [];
}

export async function uploadCertificate(eventId: number, studentId: number, certificateUrl: string, feePaid: boolean) {
  await request<unknown>(`/api/organizer/events/${eventId}/certificates`, {
    method: "POST", body: { studentId, certificateUrl, feePaid },
  });
}

// ───────────────────────────── Waitlist ─────────────────────────────

export async function joinWaitlist(eventId: number) {
  await request<unknown>(`/api/participant/waitlist/${eventId}`, { method: "POST", body: {} });
}

export async function leaveWaitlist(eventId: number) {
  await request<unknown>(`/api/participant/waitlist/${eventId}`, { method: "DELETE" });
}

// ───────────────────────────── Calendar .ics ─────────────────────────────

export function getCalendarIcsUrl(eventId: number) {
  return `${API_BASE}/api/participant/events/${eventId}/calendar`;
}

// ───────────────────────────── Registrant Management ─────────────────────────────

export async function approveRegistrant(eventId: number, studentId: number) {
  await request<unknown>(`/api/organizer/events/${eventId}/registrations/${studentId}/approve`, { method: "POST", body: {} });
}

export async function rejectRegistrant(eventId: number, studentId: number) {
  await request<unknown>(`/api/organizer/events/${eventId}/registrations/${studentId}/reject`, { method: "POST", body: {} });
}
