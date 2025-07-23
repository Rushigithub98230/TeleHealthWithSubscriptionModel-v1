using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Core.Interfaces;

public interface IAppointmentParticipantRepository
{
    Task<AppointmentParticipant?> GetByIdAsync(Guid id);
    Task<IEnumerable<AppointmentParticipant>> GetAllAsync();
    Task<AppointmentParticipant> CreateAsync(AppointmentParticipant participant);
    Task<AppointmentParticipant> UpdateAsync(AppointmentParticipant participant);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<IEnumerable<AppointmentParticipant>> GetByAppointmentAsync(Guid appointmentId);
    Task<IEnumerable<AppointmentParticipant>> GetByUserAsync(Guid userId);
    Task<AppointmentParticipant?> GetByAppointmentAndUserAsync(Guid appointmentId, Guid userId);
    Task<Guid> GetStatusIdByNameAsync(string name);
} 