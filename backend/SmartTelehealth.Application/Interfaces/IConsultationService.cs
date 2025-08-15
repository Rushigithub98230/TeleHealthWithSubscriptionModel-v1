using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IConsultationService
{
    // CRUD Operations
    Task<JsonModel> CreateConsultationAsync(CreateConsultationDto createDto);
    Task<JsonModel> GetConsultationByIdAsync(Guid id);
    Task<JsonModel> GetProviderConsultationsAsync(Guid providerId);
    Task<JsonModel> GetUpcomingConsultationsAsync();
    Task<JsonModel> UpdateConsultationAsync(Guid id, UpdateConsultationDto updateDto);
    Task<JsonModel> DeleteConsultationAsync(Guid id);
    
    // Consultation Management
    Task<JsonModel> CancelConsultationAsync(Guid id, string reason);
    Task<JsonModel> StartConsultationAsync(Guid id);
    Task<JsonModel> CompleteConsultationAsync(Guid id, string notes);
    Task<JsonModel> RescheduleConsultationAsync(Guid id, DateTime newScheduledAt);
    Task<JsonModel> MarkNoShowAsync(Guid id);
    
    // OpenTok Integration
    Task<JsonModel> GenerateMeetingUrlAsync(Guid consultationId);
    Task<JsonModel> JoinMeetingAsync(Guid consultationId, string participantId, string role);
    Task<JsonModel> LeaveMeetingAsync(Guid consultationId, string participantId);
    Task<JsonModel> StartRecordingAsync(Guid consultationId);
    Task<JsonModel> StopRecordingAsync(Guid consultationId);
    Task<JsonModel> GetRecordingsAsync(Guid consultationId);
    
    // Consultation Analytics
    Task<JsonModel> GetConsultationAnalyticsAsync(Guid providerId, DateTime? startDate = null, DateTime? endDate = null);
    Task<JsonModel> GetConsultationsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<JsonModel> CalculateProviderRevenueAsync(Guid providerId, DateTime? startDate = null, DateTime? endDate = null);
    
    // One-Time Consultations
    Task<JsonModel> CreateOneTimeConsultationAsync(CreateOneTimeConsultationDto createDto);
    Task<JsonModel> ProcessOneTimePaymentAsync(Guid consultationId, string paymentMethodId);
    
    // Follow-up Management
    Task<JsonModel> GetFollowUpConsultationsAsync(Guid userId);
    Task<JsonModel> ScheduleFollowUpAsync(Guid consultationId, DateTime followUpDate);
    Task<JsonModel> CancelFollowUpAsync(Guid consultationId);
    Task<JsonModel> GetUserOneTimeConsultationsAsync(int userId);
} 