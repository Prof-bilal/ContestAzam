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
[Route("api/admin")]
[Authorize(Roles = AppRoles.Admin)]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IEventService _eventService;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        AppDbContext db,
        IEventService eventService,
        UserManager<AppUser> userManager,
        ILogger<AdminController> logger)
    {
        _db = db;
        _eventService = eventService;
        _userManager = userManager;
        _logger = logger;
    }

    // --------------------------------------------------------- Dashboard Stats

    /// <summary>Get admin dashboard statistics.</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var totalUsers = await _db.Users.CountAsync(u => u.IsActive);
        var pendingRequests = await _db.OrganizerRequests
            .CountAsync(r => r.Status == OrganizerRequestStatus.Pending);
        var approvedOrganizers = await _db.OrganizerRequests
            .CountAsync(r => r.Status == OrganizerRequestStatus.Approved);
        var totalEvents = await _db.Events.CountAsync();

        return Ok(ApiResponse<object>.Ok(new
        {
            totalUsers,
            pendingRequests,
            approvedOrganizers,
            totalEvents
        }));
    }

    // --------------------------------------------------------- Organizer Requests

    /// <summary>List all organizer requests, optionally filtered by status.</summary>
    [HttpGet("organizer-requests")]
    public async Task<IActionResult> GetOrganizerRequests([FromQuery] string? status)
    {
        var query = _db.OrganizerRequests
            .Include(r => r.User)
            .ThenInclude(u => u.UserDetails)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<OrganizerRequestStatus>(status, true, out var parsed))
        {
            query = query.Where(r => r.Status == parsed);
        }

        var requests = await query
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new AdminOrganizerRequestDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserName = r.User.UserDetails != null ? r.User.UserDetails.FullName : r.User.Email ?? "",
                UserEmail = r.User.Email ?? "",
                OrganizationName = r.OrganizationName,
                Reason = r.Reason,
                Experience = r.Experience,
                Status = r.Status.ToString(),
                RejectionReason = r.RejectionReason,
                ReviewedBy = r.ReviewedBy,
                ReviewedAt = r.ReviewedAt,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<List<AdminOrganizerRequestDto>>.Ok(requests));
    }

    /// <summary>Get a specific organizer request by ID.</summary>
    [HttpGet("organizer-requests/{id:int}")]
    public async Task<IActionResult> GetOrganizerRequest(int id)
    {
        var request = await _db.OrganizerRequests
            .Include(r => r.User)
            .ThenInclude(u => u.UserDetails)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request is null)
            return NotFound(ApiResponse.Fail("Organizer request not found."));

        var dto = new AdminOrganizerRequestDto
        {
            Id = request.Id,
            UserId = request.UserId,
            UserName = request.User.UserDetails?.FullName ?? request.User.Email ?? "",
            UserEmail = request.User.Email ?? "",
            OrganizationName = request.OrganizationName,
            Reason = request.Reason,
            Experience = request.Experience,
            Status = request.Status.ToString(),
            RejectionReason = request.RejectionReason,
            ReviewedBy = request.ReviewedBy,
            ReviewedAt = request.ReviewedAt,
            CreatedAt = request.CreatedAt
        };

        return Ok(ApiResponse<AdminOrganizerRequestDto>.Ok(dto));
    }

    /// <summary>Approve an organizer request — assigns the Organizer role to the user.</summary>
    [HttpPost("organizer-requests/{id:int}/approve")]
    public async Task<IActionResult> ApproveOrganizerRequest(int id)
    {
        var request = await _db.OrganizerRequests
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request is null)
            return NotFound(ApiResponse.Fail("Organizer request not found."));

        if (request.Status != OrganizerRequestStatus.Pending)
            return Conflict(ApiResponse.Fail("This request has already been reviewed."));

        // Prevent self-approval (Admin should not approve their own request, though unlikely).
        var adminIdValue = User.FindFirst("sub")?.Value;
        if (int.TryParse(adminIdValue, out var adminId) && request.UserId == adminId)
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse.Fail("Cannot approve your own organizer request."));

        // Verify the user is not already an organizer.
        if (await _userManager.IsInRoleAsync(request.User, AppRoles.Organizer))
        {
            request.Status = OrganizerRequestStatus.Approved;
            request.ReviewedBy = int.TryParse(adminIdValue, out var aid) ? aid : null;
            request.ReviewedAt = DateTime.UtcNow;
            request.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(ApiResponse.Ok("User is already an organizer. Request marked as approved."));
        }

        // Assign Organizer role.
        var result = await _userManager.AddToRoleAsync(request.User, AppRoles.Organizer);
        if (!result.Succeeded)
        {
            _logger.LogError("Failed to assign Organizer role to user {UserId}: {Errors}",
                request.UserId, string.Join(", ", result.Errors.Select(e => e.Description)));
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse.Fail("Failed to assign Organizer role."));
        }

        // Remove Visitor role — Organizer supersedes it.
        if (await _userManager.IsInRoleAsync(request.User, AppRoles.Visitor))
            await _userManager.RemoveFromRoleAsync(request.User, AppRoles.Visitor);

        // Update the denormalized Role mirror.
        request.User.Role = RoleMapping.PrimaryRole(
            await _userManager.GetRolesAsync(request.User));
        await _userManager.UpdateAsync(request.User);

        // Update request status.
        request.Status = OrganizerRequestStatus.Approved;
        request.ReviewedBy = int.TryParse(adminIdValue, out var approvedBy) ? approvedBy : null;
        request.ReviewedAt = DateTime.UtcNow;
        request.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Admin {AdminId} approved organizer request {RequestId} for user {UserId}.",
            adminIdValue, id, request.UserId);

        return Ok(ApiResponse.Ok("Organizer request approved. User now has the Organizer role."));
    }

    /// <summary>Reject an organizer request.</summary>
    [HttpPost("organizer-requests/{id:int}/reject")]
    public async Task<IActionResult> RejectOrganizerRequest(int id, [FromBody] ReviewOrganizerRequestDto dto)
    {
        var request = await _db.OrganizerRequests.FindAsync(id);
        if (request is null)
            return NotFound(ApiResponse.Fail("Organizer request not found."));

        if (request.Status != OrganizerRequestStatus.Pending)
            return Conflict(ApiResponse.Fail("This request has already been reviewed."));

        request.Status = OrganizerRequestStatus.Rejected;
        request.RejectionReason = dto.RejectionReason?.Trim();
        request.ReviewedBy = int.TryParse(User.FindFirst("sub")?.Value, out var adminId) ? adminId : null;
        request.ReviewedAt = DateTime.UtcNow;
        request.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Admin {AdminId} rejected organizer request {RequestId}.", adminId, id);

        return Ok(ApiResponse.Ok("Organizer request rejected."));
    }

    // --------------------------------------------------------- Event Management

    /// <summary>List all events (admin view) with search, filter, sort, paginate.</summary>
    [HttpGet("events")]
    public async Task<IActionResult> GetEvents([FromQuery] EventQueryParams query)
    {
        var (events, total) = await _eventService.GetAdminEventsAsync(query);

        return Ok(ApiResponse<object>.Ok(new
        {
            events,
            total,
            page = query.Page,
            pageSize = query.EffectivePageSize(),
            totalPages = (int)Math.Ceiling((double)total / query.EffectivePageSize())
        }));
    }

    /// <summary>Approve a pending event.</summary>
    [HttpPatch("events/{id:int}/approve")]
    public async Task<IActionResult> ApproveEvent(int id)
    {
        var approved = await _eventService.ApproveEventAsync(id);
        if (!approved)
            return NotFound(ApiResponse.Fail("Event not found or not pending approval."));

        _logger.LogInformation("Admin approved event {EventId}.", id);
        return Ok(ApiResponse.Ok("Event approved."));
    }

    /// <summary>Reject a pending event.</summary>
    [HttpPatch("events/{id:int}/reject")]
    public async Task<IActionResult> RejectEvent(int id, [FromBody] RejectEventRequest request)
    {
        var rejected = await _eventService.RejectEventAsync(id, request.Reason);
        if (!rejected)
            return NotFound(ApiResponse.Fail("Event not found or not pending approval."));

        _logger.LogInformation("Admin rejected event {EventId}.", id);
        return Ok(ApiResponse.Ok("Event rejected."));
    }
}
