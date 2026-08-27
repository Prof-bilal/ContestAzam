using EventSphere.Api.Common;
using EventSphere.Api.Data;
using EventSphere.Api.DTOs;
using EventSphere.Api.Models;
using EventSphere.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventSphere.Api.Controllers;

[ApiController]
[Route("api/organizer")]
[Authorize(Roles = AppRoles.Organizer)]
public class OrganizerController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly IEngagementService _engagement;
    private readonly IAttendanceService _attendanceService;
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;
    private readonly INotificationService _notifications;
    private readonly ILogger<OrganizerController> _logger;

    public OrganizerController(
        IEventService eventService,
        IEngagementService engagement,
        IAttendanceService attendanceService,
        AppDbContext db,
        UserManager<AppUser> userManager,
        INotificationService notifications,
        ILogger<OrganizerController> logger)
    {
        _eventService = eventService;
        _engagement = engagement;
        _attendanceService = attendanceService;
        _db = db;
        _userManager = userManager;
        _notifications = notifications;
        _logger = logger;
    }

    private int? GetUserId()
    {
        var sub = User.FindFirst("sub")?.Value;
        return int.TryParse(sub, out var id) ? id : null;
    }

    // ───────────────────────────── Event Management ─────────────────────────────

    /// <summary>Get the organizer's events with optional status filter.</summary>
    [HttpGet("events")]
    public async Task<IActionResult> GetMyEvents([FromQuery] EventQueryParams query)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized(ApiResponse.Fail("Invalid session."));

        var events = await _eventService.GetOrganizerEventsAsync(userId.Value, query);
        return Ok(ApiResponse<List<EventSummaryDto>>.Ok(events));
    }

    /// <summary>Get organizer event statistics.</summary>
    [HttpGet("events/stats")]
    public async Task<IActionResult> GetMyStats()
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized(ApiResponse.Fail("Invalid session."));

        var stats = await _eventService.GetOrganizerStatsAsync(userId.Value);
        return Ok(ApiResponse<OrganizerEventStatsDto>.Ok(stats));
    }

    /// <summary>Get organizer's upcoming events for calendar view.</summary>
    [HttpGet("events/calendar")]
    public async Task<IActionResult> GetMyCalendar([FromQuery] CalendarQueryParams query)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized(ApiResponse.Fail("Invalid session."));

        var events = await _eventService.GetCalendarEventsAsync(userId.Value, query);
        return Ok(ApiResponse<List<EventSummaryDto>>.Ok(events));
    }

    // ───────────────────────────── Image Upload ─────────────────────────────

    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png", "image/webp", "image/gif" };
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    /// <summary>Upload an event image. Returns the relative URL.</summary>
    [HttpPost("upload-image")]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse.Fail("No file uploaded."));

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return BadRequest(ApiResponse.Fail("Only JPG, PNG, WebP, and GIF images are allowed."));

        if (!AllowedContentTypes.Contains(file.ContentType))
            return BadRequest(ApiResponse.Fail("Invalid file type."));

        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var relativeUrl = $"/uploads/{fileName}";
        return Ok(ApiResponse<object>.Ok(new { url = relativeUrl }));
    }

    // ───────────────────────────── Category Management ─────────────────────────────

    /// <summary>List all categories with event counts.</summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _eventService.GetAdminCategoriesAsync();
        return Ok(ApiResponse<List<EventCategoryDto>>.Ok(categories));
    }

    /// <summary>Create a new category.</summary>
    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var category = await _eventService.CreateCategoryAsync(request);
        return CreatedAtAction(nameof(GetCategories), ApiResponse<EventCategoryDto>.Ok(category));
    }

    /// <summary>Update an existing category.</summary>
    [HttpPut("categories/{id:int}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
    {
        var updated = await _eventService.UpdateCategoryAsync(id, request);
        if (updated is null)
            return NotFound(ApiResponse.Fail("Category not found."));

        return Ok(ApiResponse<EventCategoryDto>.Ok(updated));
    }

    /// <summary>Delete a category. Fails if category has events.</summary>
    [HttpDelete("categories/{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var deleted = await _eventService.DeleteCategoryAsync(id);
        if (!deleted)
            return BadRequest(ApiResponse.Fail("Cannot delete category. It may not exist or still has events assigned."));

        return Ok(ApiResponse.Ok("Category deleted."));
    }

    // ───────────────────────────── Attendee Management ─────────────────────────────

    /// <summary>List attendees for an event (organizer's own events only).</summary>
    [HttpGet("events/{eventId:int}/attendees")]
    public async Task<IActionResult> GetAttendees(int eventId)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        // Verify ownership
        var evt = await _db.Events.FindAsync(eventId);
        if (evt is null || evt.OrganizerId != userId.Value)
            return NotFound(ApiResponse.Fail("Event not found or you do not have permission."));

        var attendees = await _db.Registrations
            .Include(r => r.Student).ThenInclude(u => u.UserDetails)
            .Include(r => r.Student).ThenInclude(u => u.Attendances.Where(a => a.EventId == eventId))
            .Where(r => r.EventId == eventId && r.Status == RegistrationStatus.Confirmed)
            .OrderBy(r => r.RegisteredOn)
            .Select(r => new AttendeeDto
            {
                UserId = r.StudentId,
                FullName = r.Student.UserDetails != null ? r.Student.UserDetails.FullName : r.Student.Email ?? "",
                Email = r.Student.Email ?? "",
                Department = r.Student.UserDetails != null ? r.Student.UserDetails.Department : null,
                EnrollmentNo = r.Student.UserDetails != null ? r.Student.UserDetails.EnrollmentNo : null,
                RegisteredOn = r.RegisteredOn,
                Attended = r.Student.Attendances.Any(a => a.EventId == eventId && a.Attended),
                CheckedInAt = r.Student.Attendances
                    .Where(a => a.EventId == eventId && a.Attended)
                    .Select(a => (DateTime?)a.MarkedOn)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(ApiResponse<List<AttendeeDto>>.Ok(attendees));
    }    /// <summary>Check in an attendee (mark attendance) for an event.</summary>
    [HttpPost("events/{eventId:int}/attendees/{studentId:int}/check-in")]
    public async Task<IActionResult> CheckInAttendee(int eventId, int studentId)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        // Verify ownership
        var evt = await _db.Events.FindAsync(eventId);
        if (evt is null || evt.OrganizerId != userId.Value)
            return NotFound(ApiResponse.Fail("Event not found or you do not have permission."));

        // Verify the student is registered
        var registration = await _db.Registrations
            .FirstOrDefaultAsync(r => r.EventId == eventId && r.StudentId == studentId && r.Status == RegistrationStatus.Confirmed);
        if (registration is null)
            return NotFound(ApiResponse.Fail("Student is not registered for this event."));

        // P0-5: For paid events, verify payment is complete.
        if (evt.IsPaid)
        {
            var hasPayment = await _db.Payments.AnyAsync(p =>
                p.EventId == eventId &&
                p.UserId == studentId &&
                p.Status == PaymentStatus.Succeeded);
            if (!hasPayment)
                return BadRequest(ApiResponse.Fail("Payment has not been confirmed for this registration."));
        }

        // Check if already checked in
        var existing = await _db.Attendances
            .FirstOrDefaultAsync(a => a.EventId == eventId && a.StudentId == studentId);
        if (existing is not null && existing.Attended)
            return Conflict(ApiResponse.Fail("Student is already checked in."));

        if (existing is not null)
        {
            existing.Attended = true;
            existing.MarkedOn = DateTime.UtcNow;
        }
        else
        {
            _db.Attendances.Add(new Attendance
            {
                EventId = eventId,
                StudentId = studentId,
                Attended = true,
                MarkedOn = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();

        _logger.LogInformation("Organizer {OrganizerId} checked in student {StudentId} for event {EventId}.", userId, studentId, eventId);

        return Ok(ApiResponse.Ok("Attendee checked in."));
    }

    // ───────────────────────────── Attendance (QR + Stats) ─────────────────────────────

    /// <summary>Check in an attendee by QR code token.</summary>
    [HttpPost("events/attendance/check-in")]
    [Authorize(Roles = $"{AppRoles.Organizer},{AppRoles.Admin}")]
    public async Task<IActionResult> CheckInByToken([FromBody] CheckInRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var isAdmin = User.IsInRole(AppRoles.Admin);
        var result = await _attendanceService.CheckInByTokenAsync(request.Token, userId.Value, isAdmin);
        if (!result.Success)
            return BadRequest(ApiResponse.Fail(result.Message));

        return Ok(ApiResponse<CheckInResultDto>.Ok(result));
    }

    /// <summary>Get attendance list for an event.</summary>
    [HttpGet("events/{eventId:int}/attendance")]
    public async Task<IActionResult> GetAttendance(int eventId)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var attendance = await _attendanceService.GetEventAttendanceAsync(eventId, userId.Value);
        return Ok(ApiResponse<List<AttendanceDto>>.Ok(attendance));
    }

    /// <summary>Get attendance statistics for an event.</summary>
    [HttpGet("events/{eventId:int}/attendance/stats")]
    public async Task<IActionResult> GetAttendanceStats(int eventId)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var stats = await _attendanceService.GetAttendanceStatsAsync(eventId, userId.Value);
        return Ok(ApiResponse<AttendanceStatsDto>.Ok(stats));
    }

    // ───────────────────────────── Approve/Reject Registrants ─────────────────────────────

    /// <summary>Approve a pending registration for an event.</summary>
    [HttpPost("events/{eventId:int}/registrations/{studentId:int}/approve")]
    public async Task<IActionResult> ApproveRegistration(int eventId, int studentId)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var evt = await _db.Events.FindAsync(eventId);
        if (evt is null || evt.OrganizerId != userId.Value)
            return NotFound(ApiResponse.Fail("Event not found or you do not have permission."));

        var reg = await _db.Registrations
            .FirstOrDefaultAsync(r => r.EventId == eventId && r.StudentId == studentId);
        if (reg is null) return NotFound(ApiResponse.Fail("Registration not found."));
        if (reg.Status == RegistrationStatus.Confirmed)
            return Conflict(ApiResponse.Fail("Registration is already confirmed."));

        reg.Status = RegistrationStatus.Confirmed;
        reg.CheckInToken = Guid.NewGuid().ToString("N");
        await _db.SaveChangesAsync();

        // Notify the student.
        await _notifications.SendAsync(studentId, NotificationType.RegistrationConfirmed,
            "Registration Approved",
            $"Your registration for \"{evt.Title}\" has been approved.",
            relatedEntityId: evt.Id, relatedEntityType: "Event", actionUrl: $"/events/{evt.Id}");

        return Ok(ApiResponse.Ok("Registration approved."));
    }

    /// <summary>Reject a registration for an event.</summary>
    [HttpPost("events/{eventId:int}/registrations/{studentId:int}/reject")]
    public async Task<IActionResult> RejectRegistration(int eventId, int studentId)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var evt = await _db.Events.FindAsync(eventId);
        if (evt is null || evt.OrganizerId != userId.Value)
            return NotFound(ApiResponse.Fail("Event not found or you do not have permission."));

        var reg = await _db.Registrations
            .FirstOrDefaultAsync(r => r.EventId == eventId && r.StudentId == studentId);
        if (reg is null) return NotFound(ApiResponse.Fail("Registration not found."));
        if (reg.Status == RegistrationStatus.Cancelled)
            return Conflict(ApiResponse.Fail("Registration is already cancelled."));

        reg.Status = RegistrationStatus.Cancelled;
        await _db.SaveChangesAsync();

        await _notifications.SendAsync(studentId, NotificationType.RegistrationCancelled,
            "Registration Rejected",
            $"Your registration for \"{evt.Title}\" was not approved by the organizer.",
            relatedEntityId: evt.Id, relatedEntityType: "Event", actionUrl: $"/events/{evt.Id}");

        return Ok(ApiResponse.Ok("Registration rejected."));
    }

    // ───────────────────────────── Media Gallery ─────────────────────────────

    /// <summary>List media for an event.</summary>
    [HttpGet("events/{eventId:int}/media")]
    public async Task<IActionResult> GetEventMedia(int eventId)
    {
        var media = await _db.MediaGalleries
            .Where(m => m.EventId == eventId)
            .OrderByDescending(m => m.UploadedOn)
            .Select(m => new MediaDto
            {
                Id = m.Id,
                EventId = m.EventId,
                FileType = m.FileType.ToString(),
                FileUrl = m.FileUrl,
                Caption = m.Caption,
                UploadedOn = m.UploadedOn
            })
            .ToListAsync();
        return Ok(ApiResponse<List<MediaDto>>.Ok(media));
    }

    /// <summary>Upload media for an event.</summary>
    [HttpPost("events/{eventId:int}/media")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
    public async Task<IActionResult> UploadMedia(int eventId, IFormFile file, [FromQuery] string? caption)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var evt = await _db.Events.FindAsync(eventId);
        if (evt is null || evt.OrganizerId != userId.Value)
            return NotFound(ApiResponse.Fail("Event not found or you do not have permission."));

        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse.Fail("No file uploaded."));

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var imageExts = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        var videoExts = new[] { ".mp4", ".webm", ".mov" };
        var mediaType = imageExts.Contains(ext) ? MediaType.Image
            : videoExts.Contains(ext) ? MediaType.Video
            : (MediaType?)null;

        if (mediaType is null)
            return BadRequest(ApiResponse.Fail("Only image (JPG, PNG, WebP, GIF) and video (MP4, WebM, MOV) files are allowed."));

        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        Directory.CreateDirectory(uploadsDir);
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
            await file.CopyToAsync(stream);

        var gallery = new MediaGallery
        {
            EventId = eventId,
            FileType = mediaType.Value,
            FileUrl = $"/uploads/{fileName}",
            UploadedBy = userId.Value,
            Caption = caption?.Trim()
        };
        _db.MediaGalleries.Add(gallery);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<MediaDto>.Ok(new MediaDto
        {
            Id = gallery.Id,
            EventId = gallery.EventId,
            FileType = gallery.FileType.ToString(),
            FileUrl = gallery.FileUrl,
            Caption = gallery.Caption,
            UploadedOn = gallery.UploadedOn
        }));
    }

    /// <summary>Delete media.</summary>
    [HttpDelete("media/{id:int}")]
    public async Task<IActionResult> DeleteMedia(int id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var media = await _db.MediaGalleries.FindAsync(id);
        if (media is null) return NotFound(ApiResponse.Fail("Media not found."));
        if (media.UploadedBy != userId.Value)
            return Forbid();

        _db.MediaGalleries.Remove(media);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse.Ok("Media deleted."));
    }

    // ───────────────────────────── Certificates ─────────────────────────────

    /// <summary>Upload a certificate for a participant.</summary>
    [HttpPost("events/{eventId:int}/certificates")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadCertificate(int eventId, [FromBody] UploadCertificateRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var evt = await _db.Events.FindAsync(eventId);
        if (evt is null || evt.OrganizerId != userId.Value)
            return NotFound(ApiResponse.Fail("Event not found or you do not have permission."));

        // Verify the student attended.
        var attended = await _db.Attendances
            .AnyAsync(a => a.EventId == eventId && a.StudentId == request.StudentId && a.Attended);
        if (!attended)
            return BadRequest(ApiResponse.Fail("Student has not been marked as attended for this event."));

        var cert = new Certificate
        {
            EventId = eventId,
            StudentId = request.StudentId,
            CertificateUrl = request.CertificateUrl.Trim(),
            IssuedOn = DateTime.UtcNow,
            FeePaid = request.FeePaid
        };
        _db.Certificates.Add(cert);
        await _db.SaveChangesAsync();

        // Notify the student.
        await _notifications.SendAsync(request.StudentId, NotificationType.CertificateAvailable,
            "Certificate Available",
            $"Your certificate for \"{evt.Title}\" is now available for download.",
            relatedEntityId: evt.Id, relatedEntityType: "Event", actionUrl: $"/my-registrations");

        return Ok(ApiResponse.Ok("Certificate uploaded."));
    }
}
