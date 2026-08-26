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
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly IEngagementService _engagement;
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<EventsController> _logger;

    public EventsController(
        IEventService eventService,
        IEngagementService engagement,
        AppDbContext db,
        UserManager<AppUser> userManager,
        ILogger<EventsController> logger)
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

    private bool IsAdmin => User.IsInRole(AppRoles.Admin);

    // ───────────────────────────── Public Endpoints ─────────────────────────────

    /// <summary>List approved events with search, filter, sort, paginate.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ListEvents([FromQuery] EventQueryParams query)
    {
        var (events, total) = await _eventService.GetEventsAsync(query);

        return Ok(ApiResponse<object>.Ok(new
        {
            events,
            total,
            page = query.Page,
            pageSize = query.EffectivePageSize(),
            totalPages = (int)Math.Ceiling((double)total / query.EffectivePageSize())
        }));
    }

    /// <summary>Get a single event by ID (public).</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEvent(int id)
    {
        var currentUserId = GetUserId();
        var evt = await _eventService.GetByIdAsync(id, currentUserId);
        if (evt is null)
            return NotFound(ApiResponse.Fail("Event not found."));

        return Ok(ApiResponse<EventSummaryDto>.Ok(evt));
    }

    /// <summary>List all event categories.</summary>
    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _eventService.GetCategoriesAsync();
        return Ok(ApiResponse<List<EventCategoryDto>>.Ok(categories));
    }

    // ───────────────────────────── Organizer Endpoints ─────────────────────────────

    /// <summary>Create a new event. Saves as Draft or submits for approval.</summary>
    [HttpPost]
    [Authorize(Roles = $"{AppRoles.Organizer},{AppRoles.Admin}")]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized(ApiResponse.Fail("Invalid session."));

        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        if (user is null || !user.IsActive)
            return Unauthorized(ApiResponse.Fail("Invalid session."));

        var evt = await _eventService.CreateAsync(request, userId.Value);
        return CreatedAtAction(nameof(GetEvent), new { id = evt.Id },
            ApiResponse<EventSummaryDto>.Ok(evt, "Event created."));
    }

    /// <summary>Update an event. Organizer can update own Draft/PendingApproval events. Admin can update any.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{AppRoles.Organizer},{AppRoles.Admin}")]
    public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventRequest request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized(ApiResponse.Fail("Invalid session."));

        var evt = await _eventService.UpdateAsync(id, request, userId.Value, IsAdmin);
        if (evt is null)
            return NotFound(ApiResponse.Fail("Event not found or you do not have permission to update it."));

        return Ok(ApiResponse<EventSummaryDto>.Ok(evt, "Event updated."));
    }

    /// <summary>Delete an event. Organizer can delete own Draft events. Admin can delete any.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = $"{AppRoles.Organizer},{AppRoles.Admin}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized(ApiResponse.Fail("Invalid session."));

        var deleted = await _eventService.DeleteAsync(id, userId.Value, IsAdmin);
        if (!deleted)
            return NotFound(ApiResponse.Fail("Event not found, not a draft, or you do not have permission."));

        return Ok(ApiResponse.Ok("Event deleted."));
    }

    /// <summary>Publish a Draft event (submits for admin approval).</summary>
    [HttpPatch("{id:int}/publish")]
    [Authorize(Roles = $"{AppRoles.Organizer},{AppRoles.Admin}")]
    public async Task<IActionResult> PublishEvent(int id)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized(ApiResponse.Fail("Invalid session."));

        var published = await _eventService.PublishAsync(id, userId.Value, IsAdmin);
        if (!published)
            return NotFound(ApiResponse.Fail("Event not found, not a draft, or you do not have permission."));

        return Ok(ApiResponse.Ok("Event submitted for approval."));
    }

    /// <summary>Cancel an event (Draft, PendingApproval, or Approved).</summary>
    [HttpPatch("{id:int}/cancel")]
    [Authorize(Roles = $"{AppRoles.Organizer},{AppRoles.Admin}")]
    public async Task<IActionResult> CancelEvent(int id)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized(ApiResponse.Fail("Invalid session."));

        var cancelled = await _eventService.CancelAsync(id, userId.Value, IsAdmin);
        if (!cancelled)
            return NotFound(ApiResponse.Fail("Event not found, already cancelled/completed, or you do not have permission."));

        return Ok(ApiResponse.Ok("Event cancelled."));
    }

    // ───────────────────────────── Registration (existing — Module 3) ─────────────────────────────

    /// <summary>Register the authenticated user for an event.</summary>
    [HttpPost("{id:int}/register")]
    [Authorize(Roles = $"{AppRoles.Visitor},{AppRoles.Participant},{AppRoles.Organizer},{AppRoles.Admin}")]
    public async Task<IActionResult> RegisterForEvent(int id)
    {
        var userIdValue = User.FindFirst("sub")?.Value;
        if (!int.TryParse(userIdValue, out var userId))
            return Unauthorized(ApiResponse.Fail("Invalid session."));

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null || !user.IsActive)
            return Unauthorized(ApiResponse.Fail("Invalid session."));

        var evt = await _db.Events.FindAsync(id);
        if (evt is null)
            return NotFound(ApiResponse.Fail("Event not found."));

        if (evt.Status != EventStatus.Approved)
            return BadRequest(ApiResponse.Fail("Registration is not open for this event."));

        // Check registration deadline
        if (evt.RegistrationDeadline.HasValue && DateTime.UtcNow > evt.RegistrationDeadline.Value)
            return BadRequest(ApiResponse.Fail("Registration deadline has passed."));

        var currentRegistrations = await _db.Registrations
            .CountAsync(r => r.EventId == id && r.Status == RegistrationStatus.Confirmed);
        if (currentRegistrations >= evt.MaxParticipants)
            return Conflict(ApiResponse.Fail("This event is full."));

        var existingRegistration = await _db.Registrations
            .FirstOrDefaultAsync(r => r.EventId == id && r.StudentId == userId);
        if (existingRegistration is not null)
        {
            if (existingRegistration.Status == RegistrationStatus.Confirmed)
                return Conflict(ApiResponse.Fail("You are already registered for this event."));
            if (existingRegistration.Status == RegistrationStatus.Waitlist)
                return Conflict(ApiResponse.Fail("You are on the waitlist for this event."));

            existingRegistration.Status = RegistrationStatus.Confirmed;
            existingRegistration.RegisteredOn = DateTime.UtcNow;
        }
        else
        {
            _db.Registrations.Add(new Registration
            {
                EventId = id,
                StudentId = userId,
                Status = RegistrationStatus.Confirmed,
                RegisteredOn = DateTime.UtcNow
            });
        }

        if (!await _userManager.IsInRoleAsync(user, AppRoles.Participant))
        {
            var roleResult = await _userManager.AddToRoleAsync(user, AppRoles.Participant);
            if (!roleResult.Succeeded)
            {
                _logger.LogError("Failed to assign Participant role to user {UserId}: {Errors}",
                    userId, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }
            else
            {
                user.Role = RoleMapping.PrimaryRole(await _userManager.GetRolesAsync(user));
                await _userManager.UpdateAsync(user);
            }
        }

        await _db.SaveChangesAsync();
        return Ok(ApiResponse.Ok("Successfully registered for the event."));
    }

    /// <summary>Cancel the authenticated user's registration for an event.</summary>
    [HttpDelete("{id:int}/register")]
    [Authorize]
    public async Task<IActionResult> CancelRegistration(int id)
    {
        var userIdValue = User.FindFirst("sub")?.Value;
        if (!int.TryParse(userIdValue, out var userId))
            return Unauthorized(ApiResponse.Fail("Invalid session."));

        var registration = await _db.Registrations
            .FirstOrDefaultAsync(r => r.EventId == id && r.StudentId == userId);

        if (registration is null)
            return NotFound(ApiResponse.Fail("Registration not found."));

        if (registration.Status == RegistrationStatus.Cancelled)
            return Conflict(ApiResponse.Fail("Registration is already cancelled."));

        registration.Status = RegistrationStatus.Cancelled;
        await _db.SaveChangesAsync();

        return Ok(ApiResponse.Ok("Registration cancelled."));
    }

    // ───────────────────────────── Reviews ─────────────────────────────

    /// <summary>Get reviews for an event (public).</summary>
    [HttpGet("{id:int}/reviews")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEventReviews(int id)
    {
        var userId = GetUserId();
        var reviews = await _engagement.GetEventReviewsAsync(id, userId);
        return Ok(ApiResponse<EventReviewSummaryDto>.Ok(reviews));
    }

    /// <summary>Submit or update a review for an event (authenticated).</summary>
    [HttpPost("{id:int}/reviews")]
    [Authorize(Roles = $"{AppRoles.Participant},{AppRoles.Organizer},{AppRoles.Admin}")]
    public async Task<IActionResult> SubmitReview(int id, [FromBody] SubmitReviewRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized(ApiResponse.Fail("Invalid session."));

        // Verify event exists
        var evt = await _db.Events.FindAsync(id);
        if (evt is null) return NotFound(ApiResponse.Fail("Event not found."));

        // Verify user is registered for this event
        var isRegistered = await _db.Registrations.AnyAsync(r =>
            r.EventId == id && r.StudentId == userId.Value && r.Status == RegistrationStatus.Confirmed);
        if (!isRegistered)
            return BadRequest(ApiResponse.Fail("You must be registered for this event to leave a review."));

        var review = await _engagement.SubmitReviewAsync(userId.Value, id, request);
        if (review is null) return StatusCode(500, ApiResponse.Fail("Failed to submit review."));

        return Ok(ApiResponse<ReviewDto>.Ok(review, "Review submitted."));
    }
}
