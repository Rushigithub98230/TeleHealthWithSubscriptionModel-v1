using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IConsultationService
{
    // CRUD Operations
    Task<ApiResponse<ConsultationDto>> CreateConsultationAsync(CreateConsultationDto createDto);
    Task<ApiResponse<ConsultationDto>> GetConsultationByIdAsync(Guid id);
    Task<ApiResponse<IEnumerable<ConsultationDto>>> GetProviderConsultationsAsync(Guid providerId);
    Task<ApiResponse<IEnumerable<ConsultationDto>>> GetUpcomingConsultationsAsync();
    Task<ApiResponse<ConsultationDto>> UpdateConsultationAsync(Guid id, UpdateConsultationDto updateDto);
    Task<ApiResponse<bool>> DeleteConsultationAsync(Guid id);
    
    // Consultation Management
    Task<ApiResponse<bool>> CancelConsultationAsync(Guid id, string reason);
    Task<ApiResponse<bool>> StartConsultationAsync(Guid id);
    Task<ApiResponse<bool>> CompleteConsultationAsync(Guid id, string notes);
    Task<ApiResponse<bool>> RescheduleConsultationAsync(Guid id, DateTime newScheduledAt);
    Task<ApiResponse<bool>> MarkNoShowAsync(Guid id);
    
    // OpenTok Integration
    Task<ApiResponse<OpenTokSessionDto>> GenerateMeetingUrlAsync(Guid consultationId);
    Task<ApiResponse<bool>> JoinMeetingAsync(Guid consultationId, string participantId, string role);
    Task<ApiResponse<bool>> LeaveMeetingAsync(Guid consultationId, string participantId);
    Task<ApiResponse<OpenTokRecordingDto>> StartRecordingAsync(Guid consultationId);
    Task<ApiResponse<bool>> StopRecordingAsync(Guid consultationId);
    Task<ApiResponse<IEnumerable<OpenTokRecordingDto>>> GetRecordingsAsync(Guid consultationId);
    
    // Consultation Analytics
    Task<ApiResponse<ConsultationAnalyticsDto>> GetConsultationAnalyticsAsync(Guid providerId, DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<IEnumerable<ConsultationDto>>> GetConsultationsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<ApiResponse<decimal>> CalculateProviderRevenueAsync(Guid providerId, DateTime? startDate = null, DateTime? endDate = null);
    
    // One-Time Consultations
    Task<ApiResponse<ConsultationDto>> CreateOneTimeConsultationAsync(CreateOneTimeConsultationDto createDto);
    Task<ApiResponse<PaymentResultDto>> ProcessOneTimePaymentAsync(Guid consultationId, string paymentMethodId);
    
    // Follow-up Management
    Task<ApiResponse<IEnumerable<ConsultationDto>>> GetFollowUpConsultationsAsync(Guid userId);
    Task<ApiResponse<bool>> ScheduleFollowUpAsync(Guid consultationId, DateTime followUpDate);
    Task<ApiResponse<bool>> CancelFollowUpAsync(Guid consultationId);
    Task<ApiResponse<IEnumerable<ConsultationDto>>> GetUserOneTimeConsultationsAsync(Guid userId);
} 