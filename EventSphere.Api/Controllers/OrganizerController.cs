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
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<OrganizerController> _logger;

    public OrganizerController(
        IEventService eventService,
        IEngagementService engagement,
        AppDbContext db,
        UserManager<AppUser> userManager,
        ILogger<OrganizerController> logger)
    {
        _eventService = eventService;
        _engagement = engagement;
        _db = db;
        _userManager = userManager;
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
    }

    /// <summary>Check in an attendee (mark attendance) for an event.</summary>
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

        _logger.LogInformation("Organizer {OrganizerId} checked in student {StudentId} for event {EventId}.",
            userId, studentId, eventId);

        return Ok(ApiResponse.Ok("Attendee checked in."));
    }
}
