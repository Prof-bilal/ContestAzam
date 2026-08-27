using EventSphere.Api.Data;
using EventSphere.Api.DTOs;
using EventSphere.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EventSphere.Api.Services;

public class AttendanceService : IAttendanceService
{
    private readonly AppDbContext _db;
    private readonly IQrCodeService _qrCodeService;
    private readonly INotificationService _notifications;
    private readonly IEmailNotificationService _emails;
    private readonly ILogger<AttendanceService> _logger;

    public AttendanceService(AppDbContext db, IQrCodeService qrCodeService,
        INotificationService notifications, IEmailNotificationService emails,
        ILogger<AttendanceService> logger)
    {
        _db = db;
        _qrCodeService = qrCodeService;
        _notifications = notifications;
        _emails = emails;
        _logger = logger;
    }

    public async Task<DigitalPassDto?> GetDigitalPassAsync(int registrationId, int userId)
    {
        var registration = await _db.Registrations
            .Include(r => r.Event)
            .Include(r => r.Student)
                .ThenInclude(s => s.UserDetails)
            .FirstOrDefaultAsync(r => r.Id == registrationId && r.StudentId == userId);

        if (registration is null) return null;

        var participantName = registration.Student.UserDetails?.FullName
            ?? registration.Student.Email ?? "Participant";

        // Only encode the random token — never expose sequential IDs.
        var qrContent = registration.CheckInToken;
        var qrCodeBase64 = _qrCodeService.GenerateQrCodeBase64(qrContent);

        return new DigitalPassDto
        {
            RegistrationId = registration.Id,
            EventId = registration.EventId,
            EventTitle = registration.Event.Title,
            EventDate = registration.Event.EventDate,
            EventTime = registration.Event.EventTime,
            Venue = registration.Event.Venue ?? "TBA",
            ParticipantName = participantName,
            QrCodeBase64 = qrCodeBase64,
            CheckInToken = registration.CheckInToken
        };
    }

    public async Task<CheckInResultDto> CheckInByTokenAsync(string token, int callerUserId, bool isAdmin)
    {
        var registration = await _db.Registrations
            .Include(r => r.Event)
            .Include(r => r.Student)
                .ThenInclude(s => s.UserDetails)
            .FirstOrDefaultAsync(r => r.CheckInToken == token && r.Status == RegistrationStatus.Confirmed);

        if (registration is null)
        {
            return new CheckInResultDto
            {
                Success = false,
                Message = "Invalid or expired check-in token."
            };
        }

        // P0-1: Verify the caller owns this event (or is Admin).
        if (!isAdmin && registration.Event.OrganizerId != callerUserId)
        {
            return new CheckInResultDto
            {
                Success = false,
                Message = "You do not have permission to check in attendees for this event."
            };
        }

        // P0-2: Event-day validation — allow check-in only within a window around the event.
        var eventStartUtc = DateTime.SpecifyKind(
            registration.Event.EventDate.Add(registration.Event.EventTime), DateTimeKind.Utc);
        var now = DateTime.UtcNow;
        var windowStart = eventStartUtc.AddHours(-24); // 24 hours before
        var windowEnd = eventStartUtc.AddHours(6);      // 6 hours after start

        if (now < windowStart || now > windowEnd)
        {
            return new CheckInResultDto
            {
                Success = false,
                Message = "Check-in is not available for this event at this time."
            };
        }

        // P0-5: For paid events, verify payment is complete.
        if (registration.Event.IsPaid && registration.PaymentId is null)
        {
            // Check if a successful payment exists even if PaymentId link is missing.
            var hasPayment = await _db.Payments.AnyAsync(p =>
                p.EventId == registration.EventId &&
                p.UserId == registration.StudentId &&
                p.Status == PaymentStatus.Succeeded);
            if (!hasPayment)
            {
                return new CheckInResultDto
                {
                    Success = false,
                    Message = "Payment has not been confirmed for this registration."
                };
            }
        }
        else if (registration.Event.IsPaid && registration.PaymentId.HasValue)
        {
            var payment = await _db.Payments.FindAsync(registration.PaymentId.Value);
            if (payment is null || payment.Status != PaymentStatus.Succeeded)
            {
                return new CheckInResultDto
                {
                    Success = false,
                    Message = "Payment has not been confirmed for this registration."
                };
            }
        }

        var existingAttendance = await _db.Attendances
            .FirstOrDefaultAsync(a => a.EventId == registration.EventId && a.StudentId == registration.StudentId);

        if (existingAttendance?.Attended == true)
        {
            return new CheckInResultDto
            {
                Success = false,
                Message = "Already checked in.",
                AttendeeName = registration.Student.UserDetails?.FullName ?? registration.Student.Email,
                EventTitle = registration.Event.Title
            };
        }

        if (existingAttendance is not null)
        {
            existingAttendance.Attended = true;
            existingAttendance.MarkedOn = DateTime.UtcNow;
        }
        else
        {
            _db.Attendances.Add(new Attendance
            {
                EventId = registration.EventId,
                StudentId = registration.StudentId,
                Attended = true,
                MarkedOn = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();

        var attendeeName = registration.Student.UserDetails?.FullName ?? registration.Student.Email;

        // Notify the attendee their attendance was confirmed.
        var student = registration.Student;
        var studentName = student.UserDetails?.FullName ?? student.UserName ?? "there";
        await _notifications.SendAsync(registration.StudentId, NotificationType.AttendanceConfirmed,
            "Attendance Confirmed",
            $"Your attendance for {registration.Event.Title} was confirmed.",
            relatedEntityId: registration.EventId, relatedEntityType: "Event",
            actionUrl: $"/my-registrations/{registration.Id}/pass");
        await _emails.TrySendAttendanceConfirmedAsync(student.Email ?? string.Empty, studentName, registration.Event.Title);

        _logger.LogInformation("Check-in by token for Event {EventId}, Student {StudentId}", registration.EventId, registration.StudentId);

        return new CheckInResultDto
        {
            Success = true,
            Message = "Check-in successful.",
            AttendeeName = attendeeName,
            EventTitle = registration.Event.Title
        };
    }

    public async Task<CheckInResultDto> CheckInManualAsync(int eventId, int studentId, int organizerId)
    {
        var evt = await _db.Events.FindAsync(eventId);
        if (evt is null)
            return new CheckInResultDto { Success = false, Message = "Event not found." };

        if (evt.OrganizerId != organizerId)
            return new CheckInResultDto { Success = false, Message = "Unauthorized." };

        var registration = await _db.Registrations
            .Include(r => r.Student)
                .ThenInclude(s => s.UserDetails)
            .FirstOrDefaultAsync(r => r.EventId == eventId && r.StudentId == studentId && r.Status == RegistrationStatus.Confirmed);

        if (registration is null)
            return new CheckInResultDto { Success = false, Message = "Student is not registered for this event." };

        var existingAttendance = await _db.Attendances
            .FirstOrDefaultAsync(a => a.EventId == eventId && a.StudentId == studentId);

        if (existingAttendance?.Attended == true)
        {
            return new CheckInResultDto
            {
                Success = false,
                Message = "Already checked in.",
                AttendeeName = registration.Student.UserDetails?.FullName ?? registration.Student.Email,
                EventTitle = evt.Title
            };
        }

        if (existingAttendance is not null)
        {
            existingAttendance.Attended = true;
            existingAttendance.MarkedOn = DateTime.UtcNow;
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

        var attendeeName = registration.Student.UserDetails?.FullName ?? registration.Student.Email;

        // Notify the attendee their attendance was confirmed.
        var student = registration.Student;
        var studentName = student.UserDetails?.FullName ?? student.UserName ?? "there";
        await _notifications.SendAsync(registration.StudentId, NotificationType.AttendanceConfirmed,
            "Attendance Confirmed",
            $"Your attendance for {evt.Title} was confirmed.",
            relatedEntityId: evt.Id, relatedEntityType: "Event",
            actionUrl: $"/my-registrations/{registration.Id}/pass");
        await _emails.TrySendAttendanceConfirmedAsync(student.Email ?? string.Empty, studentName, evt.Title);

        _logger.LogInformation("Manual check-in for Event {EventId}, Student {StudentId}", eventId, studentId);

        return new CheckInResultDto
        {
            Success = true,
            Message = "Check-in successful.",
            AttendeeName = attendeeName,
            EventTitle = evt.Title
        };
    }

    public async Task<List<AttendanceDto>> GetEventAttendanceAsync(int eventId, int organizerId)
    {
        var evt = await _db.Events.FindAsync(eventId);
        if (evt is null || evt.OrganizerId != organizerId)
            return new List<AttendanceDto>();

        var registrations = await _db.Registrations
            .Include(r => r.Student)
                .ThenInclude(s => s.UserDetails)
            .Include(r => r.Student)
                .ThenInclude(s => s.Attendances.Where(a => a.EventId == eventId))
            .Where(r => r.EventId == eventId && r.Status == RegistrationStatus.Confirmed)
            .ToListAsync();

        return registrations.Select(r => new AttendanceDto
        {
            UserId = r.StudentId,
            FullName = r.Student.UserDetails?.FullName ?? r.Student.Email ?? "",
            Email = r.Student.Email ?? "",
            Attended = r.Student.Attendances.Any(a => a.Attended),
            CheckedInAt = r.Student.Attendances.FirstOrDefault(a => a.Attended)?.MarkedOn,
            CheckInMethod = r.Student.Attendances.Any(a => a.Attended) ? "QR" : ""
        }).ToList();
    }

    public async Task<AttendanceStatsDto> GetAttendanceStatsAsync(int eventId, int organizerId)
    {
        var evt = await _db.Events.FindAsync(eventId);
        if (evt is null || evt.OrganizerId != organizerId)
            return new AttendanceStatsDto();

        var totalRegistered = await _db.Registrations
            .CountAsync(r => r.EventId == eventId && r.Status == RegistrationStatus.Confirmed);

        var totalCheckedIn = await _db.Attendances
            .CountAsync(a => a.EventId == eventId && a.Attended);

        return new AttendanceStatsDto
        {
            TotalRegistered = totalRegistered,
            TotalCheckedIn = totalCheckedIn,
            TotalPending = totalRegistered - totalCheckedIn,
            CheckInPercentage = totalRegistered > 0 ? Math.Round((double)totalCheckedIn / totalRegistered * 100, 1) : 0
        };
    }
}
