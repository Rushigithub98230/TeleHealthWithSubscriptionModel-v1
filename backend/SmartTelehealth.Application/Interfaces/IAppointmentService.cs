using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IAppointmentService
{
    // --- APPOINTMENT MANAGEMENT ---
    Task<JsonModel> CreateAppointmentAsync(CreateAppointmentDto createDto);
    Task<JsonModel> GetAppointmentByIdAsync(Guid id);
    Task<JsonModel> GetPatientAppointmentsAsync(int patientId);
    Task<JsonModel> GetProviderAppointmentsAsync(int providerId);
    Task<JsonModel> GetPendingAppointmentsAsync();
    Task<JsonModel> UpdateAppointmentAsync(Guid id, UpdateAppointmentDto updateDto);
    Task<JsonModel> DeleteAppointmentAsync(Guid id);
    Task<JsonModel> BookAppointmentAsync(BookAppointmentDto bookDto);
    Task<JsonModel> ProcessPaymentAsync(Guid appointmentId, ProcessPaymentDto paymentDto);
    Task<JsonModel> ConfirmPaymentAsync(Guid appointmentId, string paymentIntentId);
    Task<JsonModel> ProviderActionAsync(Guid appointmentId, string action, string? notes = null);

    // --- DOCUMENT MANAGEMENT (Updated to use centralized DocumentService) ---
    Task<JsonModel> UploadDocumentAsync(Guid appointmentId, UploadDocumentDto uploadDto);
    Task<JsonModel> GetAppointmentDocumentsAsync(Guid appointmentId);
    Task<JsonModel> DeleteDocumentAsync(Guid documentId);

    // --- PARTICIPANT MANAGEMENT ---
    Task<JsonModel> AddParticipantAsync(Guid appointmentId, int? userId, string? email, string? phone, Guid participantRoleId, int invitedByUserId);
    Task<JsonModel> InviteExternalAsync(Guid appointmentId, string email, string? phone, string? message, int invitedByUserId);
    Task<JsonModel> MarkParticipantJoinedAsync(Guid appointmentId, int? userId, string? email);
    Task<JsonModel> MarkParticipantLeftAsync(Guid appointmentId, int? userId, string? email);
    Task<JsonModel> GetParticipantsAsync(Guid appointmentId);

    // --- VIDEO CALL MANAGEMENT ---
    Task<string> GetOrCreateVideoSessionAsync(Guid appointmentId);
    Task<string> GenerateVideoTokenAsync(Guid appointmentId, int? userId, string? email, Guid participantRoleId);
    Task<JsonModel> GetOpenTokTokenAsync(Guid appointmentId, int userId);
    Task<JsonModel> GenerateMeetingLinkAsync(Guid appointmentId);

    // --- PAYMENT MANAGEMENT ---
            Task<JsonModel> CreatePaymentLogAsync(Guid appointmentId, int userId, decimal amount, string paymentMethod, string? paymentIntentId = null, string? sessionId = null);
    Task<JsonModel> UpdatePaymentStatusAsync(Guid paymentLogId, Guid paymentStatusId, string? failureReason = null);
    Task<JsonModel> ProcessRefundAsync(Guid appointmentId, decimal refundAmount, string reason);
    Task<JsonModel> GetPaymentLogsAsync(Guid appointmentId);
    Task<JsonModel> GetPaymentStatusAsync(Guid appointmentId);
    Task<JsonModel> CapturePaymentAsync(Guid appointmentId);
    Task<JsonModel> RefundPaymentAsync(Guid appointmentId, decimal? amount = null);

    // --- PROVIDER ACTIONS ---
    Task<JsonModel> ProviderAcceptAppointmentAsync(Guid appointmentId, ProviderAcceptDto acceptDto);
    Task<JsonModel> ProviderRejectAppointmentAsync(Guid appointmentId, ProviderRejectDto rejectDto);
    Task<JsonModel> StartMeetingAsync(Guid appointmentId);
    Task<JsonModel> EndMeetingAsync(Guid appointmentId);
    Task<JsonModel> CompleteAppointmentAsync(Guid appointmentId, CompleteAppointmentDto completeDto);
    Task<JsonModel> CancelAppointmentAsync(Guid appointmentId, string reason);

    // --- RECORDING MANAGEMENT ---
    Task<JsonModel> StartRecordingAsync(Guid appointmentId);
    Task<JsonModel> StopRecordingAsync(Guid appointmentId);
    Task<JsonModel> GetRecordingUrlAsync(Guid appointmentId);

    // --- REMINDER MANAGEMENT ---
    Task<JsonModel> ScheduleReminderAsync(Guid appointmentId, ScheduleReminderDto reminderDto);
    Task<JsonModel> GetAppointmentRemindersAsync(Guid appointmentId);
    Task<JsonModel> SendReminderAsync(Guid reminderId);

    // --- EVENT LOGGING ---
    Task<JsonModel> LogAppointmentEventAsync(Guid appointmentId, LogAppointmentEventDto eventDto);
    Task<JsonModel> GetAppointmentEventsAsync(Guid appointmentId);

    // --- AVAILABILITY MANAGEMENT ---
    Task<JsonModel> GetProviderAvailabilityAsync(Guid providerId, DateTime date);
    Task<JsonModel> CheckProviderAvailabilityAsync(Guid providerId, DateTime startTime, DateTime endTime);

    // --- SUBSCRIPTION & BILLING ---
    Task<JsonModel> ValidateSubscriptionAccessAsync(Guid patientId, Guid categoryId);
            Task<JsonModel> CalculateAppointmentFeeAsync(int patientId, int providerId, Guid categoryId);
    Task<JsonModel> ApplySubscriptionDiscountAsync(Guid appointmentId);

    // --- SYSTEM OPERATIONS ---
    Task<JsonModel> ProcessExpiredAppointmentsAsync();
    Task<JsonModel> AutoCancelAppointmentAsync(Guid appointmentId);
    Task<JsonModel> GetAppointmentAnalyticsAsync(DateTime startDate, DateTime endDate);
    Task<JsonModel> IsAppointmentServiceHealthyAsync();

    // --- ADDITIONAL QUERIES ---
    Task<JsonModel> GetAppointmentsByStatusAsync(Guid appointmentStatusId);
    Task<JsonModel> GetUpcomingAppointmentsAsync();
    Task<JsonModel> GetCategoriesWithSubscriptionsAsync();
    Task<JsonModel> GetFeaturedProvidersAsync();

    // --- CHAT MANAGEMENT ---
    Task SendAppointmentChatMessageAsync(Guid appointmentId, MessageDto message);
} 