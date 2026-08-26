using EventSphere.Api.Data;
using EventSphere.Api.DTOs;
using EventSphere.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EventSphere.Api.Services;

public class AttendanceService : IAttendanceService
{
    private readonly AppDbContext _db;
    private readonly IQrCodeService _qrCodeService;
    private readonly ILogger<AttendanceService> _logger;

    public AttendanceService(AppDbContext db, IQrCodeService qrCodeService, ILogger<AttendanceService> logger)
    {
        _db = db;
        _qrCodeService = qrCodeService;
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

        var qrContent = $"{registration.Id}:{registration.EventId}:{registration.CheckInToken}";
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

    public async Task<CheckInResultDto> CheckInByTokenAsync(string token)
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
