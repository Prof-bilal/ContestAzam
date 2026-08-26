using EventSphere.Api.DTOs;

namespace EventSphere.Api.Services;

public interface IAttendanceService
{
    Task<DigitalPassDto?> GetDigitalPassAsync(int registrationId, int userId);
    Task<CheckInResultDto> CheckInByTokenAsync(string token);
    Task<CheckInResultDto> CheckInManualAsync(int eventId, int studentId, int organizerId);
    Task<List<AttendanceDto>> GetEventAttendanceAsync(int eventId, int organizerId);
    Task<AttendanceStatsDto> GetAttendanceStatsAsync(int eventId, int organizerId);
}
