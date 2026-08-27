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
    private readonly IRefreshTokenService _refreshService;
    private readonly INotificationService _notifications;
    private readonly IEmailNotificationService _emails;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        AppDbContext db,
        IEventService eventService,
        UserManager<AppUser> userManager,
        IRefreshTokenService refreshService,
        INotificationService notifications,
        IEmailNotificationService emails,
        ILogger<AdminController> logger)
    {
        _db = db;
        _eventService = eventService;
        _userManager = userManager;
        _refreshService = refreshService;
        _notifications = notifications;
        _emails = emails;
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

        // Notify the applicant (in-app + email).
        var name = request.User.UserDetails?.FullName ?? request.User.UserName ?? "there";
        await _notifications.SendAsync(request.UserId, NotificationType.OrganizerRequestApproved,
            "Organizer Request Approved",
            "Your organizer request was approved. You can now create and manage events.",
            relatedEntityId: request.Id, relatedEntityType: "OrganizerRequest", actionUrl: "/organizer/events");
        await _emails.TrySendOrganizerApprovedAsync(request.User.Email ?? string.Empty, name);

        _logger.LogInformation("Admin {AdminId} approved organizer request {RequestId} for user {UserId}.",
            adminIdValue, id, request.UserId);

        return Ok(ApiResponse.Ok("Organizer request approved. User now has the Organizer role."));
    }

    /// <summary>Reject an organizer request.</summary>
    [HttpPost("organizer-requests/{id:int}/reject")]
    public async Task<IActionResult> RejectOrganizerRequest(int id, [FromBody] ReviewOrganizerRequestDto dto)
    {
        var request = await _db.OrganizerRequests
            .Include(r => r.User).ThenInclude(u => u.UserDetails)
            .FirstOrDefaultAsync(r => r.Id == id);
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

        // Notify the applicant (in-app + email).
        if (request.User is not null)
        {
            var name = request.User.UserDetails?.FullName ?? request.User.UserName ?? "there";
            await _notifications.SendAsync(request.UserId, NotificationType.OrganizerRequestRejected,
                "Organizer Request Rejected",
                dto.RejectionReason is null or "" ? "Your organizer request was not approved." : $"Your organizer request was not approved: {dto.RejectionReason}",
                relatedEntityId: request.Id, relatedEntityType: "OrganizerRequest", actionUrl: "/");
            await _emails.TrySendOrganizerRejectedAsync(request.User.Email ?? string.Empty, name, dto.RejectionReason);
        }

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

    // --------------------------------------------------------- User Management

    /// <summary>List all users with search and pagination.</summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);
        var q = _db.Users
            .Include(u => u.UserDetails)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            q = q.Where(u => (u.Email != null && u.Email.ToLower().Contains(s)) ||
                             (u.UserDetails != null && u.UserDetails.FullName.ToLower().Contains(s)));
        }

        var total = await q.CountAsync();
        var users = await q
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new AdminUserDto
            {
                Id = u.Id,
                Email = u.Email ?? "",
                FullName = u.UserDetails != null ? u.UserDetails.FullName : "",
                Role = u.Role.ToString(),
                IsActive = u.IsActive,
                SuspendReason = u.SuspendReason,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(new { users, total, page, pageSize, totalPages = (int)Math.Ceiling((double)total / pageSize) }));
    }

    /// <summary>Get a user's full details.</summary>
    [HttpGet("users/{id:int}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _db.Users
            .Include(u => u.UserDetails)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return NotFound(ApiResponse.Fail("User not found."));

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(ApiResponse<AdminUserDetailDto>.Ok(new AdminUserDetailDto
        {
            Id = user.Id,
            Email = user.Email ?? "",
            FullName = user.UserDetails?.FullName ?? "",
            Mobile = user.UserDetails?.Mobile,
            Department = user.UserDetails?.Department,
            EnrollmentNo = user.UserDetails?.EnrollmentNo,
            Roles = roles.ToArray(),
            IsActive = user.IsActive,
            SuspendReason = user.SuspendReason,
            CreatedAt = user.CreatedAt
        }));
    }

    /// <summary>Suspend or reactivate a user.</summary>
    [HttpPatch("users/{id:int}/toggle-active")]
    public async Task<IActionResult> ToggleUserActive(int id, [FromBody] SuspendUserRequest? request)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound(ApiResponse.Fail("User not found."));

        // Prevent admin from deactivating themselves.
        var adminId = User.FindFirst("sub")?.Value;
        if (int.TryParse(adminId, out var aid) && aid == id)
            return BadRequest(ApiResponse.Fail("Cannot deactivate your own account."));

        user.IsActive = !user.IsActive;

        if (!user.IsActive)
        {
            // Store the suspension reason.
            user.SuspendReason = request?.Reason?.Trim();

            // Revoke all refresh tokens so the user is forced to re-authenticate.
            await _refreshService.RevokeAllForUserAsync(user.Id);

            _logger.LogInformation("Admin {AdminId} suspended user {UserId}. Reason: {Reason}",
                adminId, id, user.SuspendReason ?? "(none)");
        }
        else
        {
            // Clear suspension reason on reactivation.
            user.SuspendReason = null;

            _logger.LogInformation("Admin {AdminId} reactivated user {UserId}.", adminId, id);
        }

        // Save IsActive first — this is the critical flag that blocks login.
        // Use UpdateAsync via Identity to ensure it persists even if
        // SuspendReason column is not yet migrated.
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse.Fail("Failed to update user status."));

        // Send notification AFTER successful save so the user doesn't
        // see a "suspended" notification when the save actually failed.
        if (!user.IsActive)
        {
            var suspendMsg = string.IsNullOrWhiteSpace(user.SuspendReason)
                ? "Your account has been suspended by an administrator."
                : $"Your account has been suspended. Reason: {user.SuspendReason}";
            await _notifications.SendAsync(user.Id, NotificationType.EventUpdated,
                "Account Suspended", suspendMsg,
                relatedEntityType: "Account", actionUrl: "/");
        }

        return Ok(ApiResponse.Ok(user.IsActive ? "User reactivated." : "User suspended."));
    }

    /// <summary>Send a warning to a user (in-app notification + optional email).</summary>
    [HttpPost("users/{id:int}/warn")]
    public async Task<IActionResult> WarnUser(int id, [FromBody] WarnUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound(ApiResponse.Fail("User not found."));

        var adminId = User.FindFirst("sub")?.Value;
        var message = request.Message?.Trim();
        if (string.IsNullOrWhiteSpace(message))
            return BadRequest(ApiResponse.Fail("Warning message is required."));

        // Send in-app notification.
        await _notifications.SendAsync(user.Id, NotificationType.EventUpdated,
            "⚠️ Account Warning",
            message,
            relatedEntityType: "Account", actionUrl: "/");

        // Send email if requested.
        if (request.SendEmail)
        {
            var fullName = user.UserDetails?.FullName ?? user.Email ?? "there";
            await _emails.TrySendAccountWarningAsync(user.Email ?? string.Empty, fullName, message);
        }

        _logger.LogInformation("Admin {AdminId} warned user {UserId}: {Message}", adminId, id, message);
        return Ok(ApiResponse.Ok("Warning sent to user."));
    }

    /// <summary>Assign a role to a user.</summary>
    [HttpPost("users/{id:int}/roles")]
    public async Task<IActionResult> AssignRole(int id, [FromBody] AssignRoleRequest request)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound(ApiResponse.Fail("User not found."));

        var role = request.Role?.Trim();
        if (!AppRoles.All.Contains(role ?? ""))
            return BadRequest(ApiResponse.Fail("Invalid role."));

        if (await _userManager.IsInRoleAsync(user, role!))
            return Conflict(ApiResponse.Fail($"User already has the {role} role."));

        var result = await _userManager.AddToRoleAsync(user, role!);
        if (!result.Succeeded)
            return StatusCode(500, ApiResponse.Fail("Failed to assign role."));

        // Update denormalized role.
        user.Role = RoleMapping.PrimaryRole(await _userManager.GetRolesAsync(user));
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Admin assigned role {Role} to user {UserId}.", role, id);
        return Ok(ApiResponse.Ok($"Role {role} assigned."));
    }

    /// <summary>Remove a role from a user.</summary>
    [HttpDelete("users/{id:int}/roles/{role}")]
    public async Task<IActionResult> RemoveRole(int id, string role)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound(ApiResponse.Fail("User not found."));

        if (!await _userManager.IsInRoleAsync(user, role))
            return Conflict(ApiResponse.Fail($"User does not have the {role} role."));

        var result = await _userManager.RemoveFromRoleAsync(user, role);
        if (!result.Succeeded)
            return StatusCode(500, ApiResponse.Fail("Failed to remove role."));

        user.Role = RoleMapping.PrimaryRole(await _userManager.GetRolesAsync(user));
        await _userManager.UpdateAsync(user);

        return Ok(ApiResponse.Ok($"Role {role} removed."));
    }

    // --------------------------------------------------------- Announcements

    /// <summary>Send a system-wide announcement to all active users.</summary>
    [HttpPost("announcements")]
    public async Task<IActionResult> SendAnnouncement([FromBody] SendAnnouncementRequest request)
    {
        var activeUserIds = await _db.Users
            .Where(u => u.IsActive)
            .Select(u => u.Id)
            .ToListAsync();

        var count = 0;
        foreach (var userId in activeUserIds)
        {
            await _notifications.SendAsync(userId, NotificationType.EventUpdated,
                request.Title.Trim(),
                request.Message?.Trim(),
                relatedEntityType: "Announcement",
                actionUrl: "/");
            count++;
        }

        _logger.LogInformation("Admin sent announcement to {Count} users.", count);
        return Ok(ApiResponse.Ok($"Announcement sent to {count} users."));
    }

    // --------------------------------------------------------- Content Moderation

    /// <summary>List all reviews for moderation.</summary>
    [HttpGet("reviews")]
    public async Task<IActionResult> GetReviews([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);
        var q = _db.Feedbacks
            .Include(f => f.Student).ThenInclude(u => u.UserDetails)
            .Include(f => f.Event)
            .AsQueryable();

        var total = await q.CountAsync();
        var reviews = await q
            .OrderByDescending(f => f.SubmittedOn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new AdminReviewDto
            {
                Id = f.Id,
                EventId = f.EventId,
                EventTitle = f.Event.Title,
                UserId = f.StudentId,
                UserName = f.Student.UserDetails != null ? f.Student.UserDetails.FullName : f.Student.Email ?? "",
                Rating = f.Rating,
                Comment = f.Comments,
                SubmittedOn = f.SubmittedOn
            })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(new { reviews, total, page, pageSize }));
    }

    /// <summary>Delete a review (moderation).</summary>
    [HttpDelete("reviews/{id:int}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var review = await _db.Feedbacks.FindAsync(id);
        if (review is null) return NotFound(ApiResponse.Fail("Review not found."));

        _db.Feedbacks.Remove(review);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse.Ok("Review deleted."));
    }

    // --------------------------------------------------------- Report Export

    /// <summary>Export participation report as CSV.</summary>
    [HttpGet("reports/participation")]
    public async Task<IActionResult> ExportParticipationReport()
    {
        var data = await _db.Events
            .Include(e => e.Category)
            .Include(e => e.Registrations)
            .Where(e => e.Status == EventStatus.Approved || e.Status == EventStatus.Completed)
            .Select(e => new
            {
                EventTitle = e.Title,
                Category = e.Category.Name,
                Date = e.EventDate,
                Registered = e.Registrations.Count(r => r.Status == RegistrationStatus.Confirmed),
                MaxCapacity = e.MaxParticipants
            })
            .ToListAsync();

        var csv = "Event Title,Category,Date,Registered,Max Capacity\n";
        foreach (var row in data)
            csv += $"\"{row.EventTitle}\",\"{row.Category}\",{row.Date:yyyy-MM-dd},{row.Registered},{row.MaxCapacity}\n";

        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", $"participation-report-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    /// <summary>Export user growth report as CSV.</summary>
    [HttpGet("reports/users")]
    public async Task<IActionResult> ExportUserReport()
    {
        var users = await _db.Users
            .Include(u => u.UserDetails)
            .Where(u => u.IsActive)
            .Select(u => new
            {
                Email = u.Email ?? "",
                FullName = u.UserDetails != null ? u.UserDetails.FullName : "",
                Role = u.Role.ToString(),
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        var csv = "Email,Full Name,Role,Created At\n";
        foreach (var u in users)
            csv += $"\"{u.Email}\",\"{u.FullName}\",{u.Role},{u.CreatedAt:yyyy-MM-dd}\n";

        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", $"user-report-{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
