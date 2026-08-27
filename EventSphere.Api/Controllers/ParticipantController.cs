using EventSphere.Api.Common;
using EventSphere.Api.DTOs;
using EventSphere.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventSphere.Api.Controllers;

[ApiController]
[Route("api/participant")]
[Authorize]
public class ParticipantController : ControllerBase
{
    private readonly IEngagementService _engagement;
    private readonly IAttendanceService _attendanceService;
    private readonly ILogger<ParticipantController> _logger;

    public ParticipantController(IEngagementService engagement, IAttendanceService attendanceService, ILogger<ParticipantController> logger)
    {
        _engagement = engagement;
        _attendanceService = attendanceService;
        _logger = logger;
    }

    private int? GetUserId()
    {
        var sub = User.FindFirst("sub")?.Value;
        return int.TryParse(sub, out var id) ? id : null;
    }

    // ───────────────────────────── Registrations ─────────────────────────────

    /// <summary>Get the authenticated user's registration history.</summary>
    [HttpGet("registrations")]
    public async Task<IActionResult> GetMyRegistrations()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var registrations = await _engagement.GetMyRegistrationsAsync(userId.Value);
        return Ok(ApiResponse<List<RegistrationDto>>.Ok(registrations));
    }

    /// <summary>Cancel a registration by registration ID.</summary>
    [HttpDelete("registrations/{id:int}")]
    public async Task<IActionResult> CancelRegistration(int id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var cancelled = await _engagement.CancelRegistrationByIdAsync(id, userId.Value);
        if (!cancelled) return NotFound(ApiResponse.Fail("Registration not found or already cancelled."));

        return Ok(ApiResponse.Ok("Registration cancelled."));
    }

    // ───────────────────────────── Digital Pass ─────────────────────────────

    /// <summary>Get digital pass with QR code for a registration.</summary>
    [HttpGet("registrations/{id:int}/pass")]
    public async Task<IActionResult> GetDigitalPass(int id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var pass = await _attendanceService.GetDigitalPassAsync(id, userId.Value);
        if (pass is null) return NotFound(ApiResponse.Fail("Registration not found."));

        return Ok(ApiResponse<DigitalPassDto>.Ok(pass));
    }

    // ───────────────────────────── Favorites ─────────────────────────────

    /// <summary>Bookmark an event.</summary>
    [HttpPost("favorites/{eventId:int}")]
    public async Task<IActionResult> AddFavorite(int eventId)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var added = await _engagement.AddFavoriteAsync(userId.Value, eventId);
        if (!added) return Conflict(ApiResponse.Fail("Event already bookmarked."));

        return Ok(ApiResponse.Ok("Event bookmarked."));
    }

    /// <summary>Remove a bookmark.</summary>
    [HttpDelete("favorites/{eventId:int}")]
    public async Task<IActionResult> RemoveFavorite(int eventId)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var removed = await _engagement.RemoveFavoriteAsync(userId.Value, eventId);
        if (!removed) return NotFound(ApiResponse.Fail("Bookmark not found."));

        return Ok(ApiResponse.Ok("Bookmark removed."));
    }

    /// <summary>List my bookmarked events.</summary>
    [HttpGet("favorites")]
    public async Task<IActionResult> GetMyFavorites()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var favorites = await _engagement.GetMyFavoritesAsync(userId.Value);
        return Ok(ApiResponse<List<FavoriteDto>>.Ok(favorites));
    }

    // ───────────────────────────── Reviews ─────────────────────────────

    /// <summary>Delete my own review.</summary>
    [HttpDelete("reviews/{id:int}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var deleted = await _engagement.DeleteReviewAsync(id, userId.Value);
        if (!deleted) return NotFound(ApiResponse.Fail("Review not found or you do not have permission."));

        return Ok(ApiResponse.Ok("Review deleted."));
    }

    // ───────────────────────────── Certificates ─────────────────────────────

    /// <summary>List my certificates.</summary>
    [HttpGet("certificates")]
    public async Task<IActionResult> GetMyCertificates()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var certs = await _engagement.GetMyCertificatesAsync(userId.Value);
        return Ok(ApiResponse<List<CertificateDto>>.Ok(certs));
    }

    // ───────────────────────────── Waitlist ─────────────────────────────

    /// <summary>Join the waitlist for a full event.</summary>
    [HttpPost("waitlist/{eventId:int}")]
    public async Task<IActionResult> JoinWaitlist(int eventId)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var result = await _engagement.JoinWaitlistAsync(userId.Value, eventId);
        if (!result)
            return Conflict(ApiResponse.Fail("Event is not full, already on waitlist, or not available."));

        return Ok(ApiResponse.Ok("Added to waitlist."));
    }

    /// <summary>Leave the waitlist.</summary>
    [HttpDelete("waitlist/{eventId:int}")]
    public async Task<IActionResult> LeaveWaitlist(int eventId)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        var result = await _engagement.LeaveWaitlistAsync(userId.Value, eventId);
        if (!result) return NotFound(ApiResponse.Fail("Not on the waitlist for this event."));

        return Ok(ApiResponse.Ok("Removed from waitlist."));
    }

    /// <summary>Get .ics calendar file for an event.</summary>
    [HttpGet("events/{eventId:int}/calendar")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEventCalendar(int eventId)
    {
        var evt = await _engagement.GetEventForCalendarAsync(eventId);
        if (evt is null) return NotFound(ApiResponse.Fail("Event not found."));

        var startUtc = DateTime.SpecifyKind(evt.EventDate.Add(evt.EventTime), DateTimeKind.Utc);
        var endUtc = startUtc.AddHours(2);

        var ics = $"""
        BEGIN:VCALENDAR
        VERSION:2.0
        PRODID:-//EventSphere//EN
        BEGIN:VEVENT
        DTSTART:{startUtc:yyyyMMddTHHmmssZ}
        DTEND:{endUtc:yyyyMMddTHHmmssZ}
        SUMMARY:{EscapeIcs(evt.Title)}
        DESCRIPTION:{EscapeIcs(evt.Description ?? "")}
        LOCATION:{EscapeIcs(evt.Venue ?? "")}
        END:VEVENT
        END:VCALENDAR
        """;

        return File(System.Text.Encoding.UTF8.GetBytes(ics), "text/calendar", $"{evt.Title}.ics");
    }

    private static string EscapeIcs(string text) => text.Replace("\\", "\\\\").Replace(";", "\\;").Replace(",", "\\,").Replace("\n", "\\n");
}
