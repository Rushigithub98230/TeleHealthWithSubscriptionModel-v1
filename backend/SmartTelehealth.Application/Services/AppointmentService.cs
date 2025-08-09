using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace SmartTelehealth.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IAppointmentParticipantRepository _participantRepository;
    private readonly IAppointmentInvitationRepository _invitationRepository;
    private readonly IAppointmentPaymentLogRepository _paymentLogRepository;
    private readonly IUserRepository _userRepository;
    private readonly IOpenTokService _openTokService;
    private readonly INotificationService _notificationService;
    private readonly IStripeService _stripeService;
    private readonly IParticipantRoleRepository _participantRoleRepository;
    private readonly IDocumentService _documentService;
    private readonly IDocumentTypeService _documentTypeService;
    // Add other dependencies as needed (e.g., chat, audit, config)

    private const int DefaultMaxParticipants = 8;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        IAppointmentParticipantRepository participantRepository,
        IAppointmentInvitationRepository invitationRepository,
        IAppointmentPaymentLogRepository paymentLogRepository,
        IUserRepository userRepository,
        IOpenTokService openTokService,
        INotificationService notificationService,
        IStripeService stripeService,
        IParticipantRoleRepository participantRoleRepository,
        IDocumentService documentService,
        IDocumentTypeService documentTypeService
    )
    {
        _appointmentRepository = appointmentRepository;
        _participantRepository = participantRepository;
        _invitationRepository = invitationRepository;
        _paymentLogRepository = paymentLogRepository;
        _userRepository = userRepository;
        _openTokService = openTokService;
        _notificationService = notificationService;
        _stripeService = stripeService;
        _participantRoleRepository = participantRoleRepository;
        _documentService = documentService;
        _documentTypeService = documentTypeService;
    }

    // --- DOCUMENT MANAGEMENT (Updated to use centralized DocumentService) ---
    
    public async Task<ApiResponse<DocumentDto>> UploadDocumentAsync(Guid appointmentId, UploadDocumentDto uploadDto)
    {
        try
        {
            // Validate appointment exists
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
                return ApiResponse<DocumentDto>.ErrorResponse("Appointment not found", 404);

            // Use the file content directly since it's already a byte array
            var fileBytes = uploadDto.FileContent;

            // Get document type for appointment documents (you can make this configurable)
            var appointmentDocumentTypes = await _documentTypeService.GetAllDocumentTypesAsync(true);
            var appointmentDocType = appointmentDocumentTypes.Data?.FirstOrDefault(dt => 
                dt.Name.ToLower().Contains("appointment") || 
                dt.Name.ToLower().Contains("medical") ||
                dt.Name.ToLower().Contains("report"));

            if (appointmentDocType == null)
            {
                return ApiResponse<DocumentDto>.ErrorResponse("No suitable document type found for appointment documents", 400);
            }

            // Create upload request for centralized document service
            var uploadRequest = new UploadDocumentRequest
            {
                FileData = fileBytes,
                FileName = uploadDto.FileName,
                ContentType = uploadDto.FileType,
                EntityType = "Appointment",
                EntityId = appointmentId,
                ReferenceType = "appointment_document",
                Description = $"Document uploaded for appointment {appointmentId}",
                IsPublic = false,
                IsEncrypted = false,
                DocumentTypeId = appointmentDocType.DocumentTypeId,
                CreatedById = appointment.PatientId // or get from current user context
            };

            // Upload using centralized document service
            var result = await _documentService.UploadDocumentAsync(uploadRequest);
            
            // Note: Document count is now managed by the centralized document service
            // No need to update appointment entity

            return result;
        }
        catch (Exception ex)
        {
            return ApiResponse<DocumentDto>.ErrorResponse($"Failed to upload document: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<DocumentDto>>> GetAppointmentDocumentsAsync(Guid appointmentId)
    {
        try
        {
            // Validate appointment exists
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
                return ApiResponse<IEnumerable<DocumentDto>>.ErrorResponse("Appointment not found", 404);

            // Get documents using centralized document service
            var documentsResult = await _documentService.GetDocumentsByEntityAsync("Appointment", appointmentId);
            
            if (documentsResult.Success)
            {
                return ApiResponse<IEnumerable<DocumentDto>>.SuccessResponse(documentsResult.Data);
            }

            return ApiResponse<IEnumerable<DocumentDto>>.ErrorResponse("Failed to retrieve appointment documents");
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<DocumentDto>>.ErrorResponse($"Failed to get appointment documents: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteDocumentAsync(Guid documentId)
    {
        try
        {
            // Get current user ID from context (you'll need to implement this based on your authentication)
            var currentUserId = Guid.Empty; // Replace with actual user ID from context

            // Delete using centralized document service
            var result = await _documentService.DeleteDocumentAsync(documentId, currentUserId);
            
            // Note: Document count is now managed by the centralized document service
            // No need to update appointment entity

            return result;
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResponse($"Failed to delete document: {ex.Message}");
        }
    }

    // --- PARTICIPANT MANAGEMENT ---
    public async Task<ApiResponse<AppointmentParticipantDto>> AddParticipantAsync(Guid appointmentId, Guid? userId, string? email, string? phone, Guid participantRoleId, Guid invitedByUserId)
    {
        try
        {
            // Enforce max participants
            var participants = await _participantRepository.GetByAppointmentAsync(appointmentId);
            if (participants.Count() >= DefaultMaxParticipants)
                return ApiResponse<AppointmentParticipantDto>.ErrorResponse($"Max participants ({DefaultMaxParticipants}) reached.");

            var participant = new AppointmentParticipant
            {
                AppointmentId = appointmentId,
                UserId = userId,
                ExternalEmail = email,
                ExternalPhone = phone,
                ParticipantRoleId = participantRoleId,
                ParticipantStatusId = await _participantRepository.GetStatusIdByNameAsync("Invited"), // Invited status Guid
                InvitedAt = DateTime.UtcNow,
                InvitedByUserId = invitedByUserId,
                CreatedAt = DateTime.UtcNow
            };
            await _participantRepository.CreateAsync(participant);
            // Optionally send notification
            return ApiResponse<AppointmentParticipantDto>.SuccessResponse(MapToDto(participant));
        }
        catch (Exception ex)
        {
            return ApiResponse<AppointmentParticipantDto>.ErrorResponse($"Failed to add participant: {ex.Message}");
        }
    }

    // --- INVITATION MANAGEMENT ---
    public async Task<ApiResponse<AppointmentInvitationDto>> InviteExternalAsync(Guid appointmentId, string email, string? phone, string? message, Guid invitedByUserId)
    {
        try
        {
            var invitation = new AppointmentInvitation
            {
                AppointmentId = appointmentId,
                InvitedByUserId = invitedByUserId,
                InvitedEmail = email,
                InvitedPhone = phone,
                Message = message,
                InvitationStatusId = await _invitationRepository.GetStatusIdByNameAsync("Pending"), // Pending status Guid
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                CreatedAt = DateTime.UtcNow
            };
            await _invitationRepository.CreateAsync(invitation);
            // Send email/SMS with meeting link (stub)
            return ApiResponse<AppointmentInvitationDto>.SuccessResponse(MapToDto(invitation));
        }
        catch (Exception ex)
        {
            return ApiResponse<AppointmentInvitationDto>.ErrorResponse($"Failed to invite external: {ex.Message}");
        }
    }

    // --- JOIN TRACKING ---
    public async Task<ApiResponse<bool>> MarkParticipantJoinedAsync(Guid appointmentId, Guid? userId, string? email)
    {
        try
        {
            var participant = await _participantRepository.GetByAppointmentAndUserAsync(appointmentId, userId ?? Guid.Empty);
            if (participant == null) 
                return ApiResponse<bool>.ErrorResponse("Participant not found");
            
            participant.ParticipantStatusId = await _participantRepository.GetStatusIdByNameAsync("Joined"); // Joined status Guid
            participant.JoinedAt = DateTime.UtcNow;
            await _participantRepository.UpdateAsync(participant);
            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResponse($"Failed to mark participant joined: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> MarkParticipantLeftAsync(Guid appointmentId, Guid? userId, string? email)
    {
        try
        {
            var participant = await _participantRepository.GetByAppointmentAndUserAsync(appointmentId, userId ?? Guid.Empty);
            if (participant == null) 
                return ApiResponse<bool>.ErrorResponse("Participant not found");
            
            participant.ParticipantStatusId = await _participantRepository.GetStatusIdByNameAsync("Left"); // Left status Guid
            participant.LeftAt = DateTime.UtcNow;
            await _participantRepository.UpdateAsync(participant);
            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResponse($"Failed to mark participant left: {ex.Message}");
        }
    }

    // --- GROUP VIDEO SESSION ---
    public async Task<string> GetOrCreateVideoSessionAsync(Guid appointmentId)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
        if (string.IsNullOrEmpty(appointment.OpenTokSessionId))
        {
            var sessionResult = await _openTokService.CreateSessionAsync($"Appointment-{appointmentId}");
            appointment.OpenTokSessionId = sessionResult.Data?.SessionId ?? string.Empty;
            await _appointmentRepository.UpdateAsync(appointment);
        }
        return appointment.OpenTokSessionId;
    }

    public async Task<string> GenerateVideoTokenAsync(Guid appointmentId, Guid? userId, string? email, Guid participantRoleId)
    {
        var sessionId = await GetOrCreateVideoSessionAsync(appointmentId);
        var participantRole = await _participantRoleRepository.GetByIdAsync(participantRoleId);
        var openTokRole = MapParticipantRoleNameToOpenTokRole(participantRole.Name);
        var tokenResult = await _openTokService.GenerateTokenAsync(sessionId, userId?.ToString() ?? string.Empty, email ?? string.Empty, openTokRole);
        return tokenResult.Data ?? string.Empty;
    }

    // --- PAYMENT MANAGEMENT ---
    public async Task<ApiResponse<AppointmentPaymentLogDto>> CreatePaymentLogAsync(Guid appointmentId, Guid userId, decimal amount, string paymentMethod, string? paymentIntentId = null, string? sessionId = null)
    {
        try
        {
            var paymentLog = new AppointmentPaymentLog
            {
                AppointmentId = appointmentId,
                UserId = userId,
                Amount = amount,
                PaymentMethod = paymentMethod,
                PaymentIntentId = paymentIntentId,
                SessionId = sessionId,
                PaymentStatusId = await _paymentLogRepository.GetStatusIdByNameAsync("Pending"), // Pending status Guid
                RefundStatusId = await _paymentLogRepository.GetStatusIdByNameAsync("None"), // None status Guid
                Currency = "USD",
                Description = $"Payment for appointment {appointmentId}",
                CreatedAt = DateTime.UtcNow
            };
            await _paymentLogRepository.CreateAsync(paymentLog);
            return ApiResponse<AppointmentPaymentLogDto>.SuccessResponse(MapToDto(paymentLog));
        }
        catch (Exception ex)
        {
            return ApiResponse<AppointmentPaymentLogDto>.ErrorResponse($"Failed to create payment log: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AppointmentPaymentLogDto>> UpdatePaymentStatusAsync(Guid paymentLogId, Guid paymentStatusId, string? failureReason = null)
    {
        try
        {
            var paymentLog = await _paymentLogRepository.GetByIdAsync(paymentLogId);
            if (paymentLog == null) 
                return ApiResponse<AppointmentPaymentLogDto>.ErrorResponse("Payment log not found");
            
            paymentLog.PaymentStatusId = paymentStatusId;
            paymentLog.FailureReason = failureReason;
            paymentLog.PaymentDate = paymentStatusId == await _paymentLogRepository.GetStatusIdByNameAsync("Completed") ? DateTime.UtcNow : null; // Completed status Guid
            paymentLog.UpdatedAt = DateTime.UtcNow;
            
            await _paymentLogRepository.UpdateAsync(paymentLog);
            return ApiResponse<AppointmentPaymentLogDto>.SuccessResponse(MapToDto(paymentLog));
        }
        catch (Exception ex)
        {
            return ApiResponse<AppointmentPaymentLogDto>.ErrorResponse($"Failed to update payment status: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AppointmentPaymentLogDto>> ProcessRefundAsync(Guid appointmentId, decimal refundAmount, string reason)
    {
        try
        {
            var latestPayment = await _paymentLogRepository.GetLatestByAppointmentAsync(appointmentId);
            if (latestPayment == null) 
                return ApiResponse<AppointmentPaymentLogDto>.ErrorResponse("No payment found for appointment");
            
            // Process refund through Stripe
            var refundSuccess = await _stripeService.ProcessRefundAsync(latestPayment.PaymentIntentId, refundAmount);
            var refundId = refundSuccess ? Guid.NewGuid().ToString() : null; // Generate refund ID if successful
            
            latestPayment.RefundStatusId = await _paymentLogRepository.GetStatusIdByNameAsync("Completed"); // Completed status Guid
            latestPayment.RefundedAmount = refundAmount;
            latestPayment.RefundId = refundId;
            latestPayment.RefundReason = reason;
            latestPayment.RefundDate = DateTime.UtcNow;
            latestPayment.UpdatedAt = DateTime.UtcNow;
            
            if (refundAmount >= latestPayment.Amount)
            {
                latestPayment.PaymentStatusId = await _paymentLogRepository.GetStatusIdByNameAsync("Refunded"); // Refunded status Guid
            }
            else
            {
                latestPayment.PaymentStatusId = await _paymentLogRepository.GetStatusIdByNameAsync("PartiallyRefunded"); // PartiallyRefunded status Guid
            }
            
            await _paymentLogRepository.UpdateAsync(latestPayment);
            return ApiResponse<AppointmentPaymentLogDto>.SuccessResponse(MapToDto(latestPayment));
        }
        catch (Exception ex)
        {
            return ApiResponse<AppointmentPaymentLogDto>.ErrorResponse($"Failed to process refund: {ex.Message}");
        }
    }

    // --- CHAT INTEGRATION (STUB) ---
    public Task SendAppointmentChatMessageAsync(Guid appointmentId, MessageDto message)
    {
        // TODO: Implement chat message persistence and SignalR notification
        throw new NotImplementedException();
    }

    // --- MAPPING METHODS ---
    private AppointmentParticipantDto MapToDto(AppointmentParticipant p)
    {
        return new AppointmentParticipantDto
        {
            Id = p.Id.ToString(),
            AppointmentId = p.AppointmentId.ToString(),
            UserId = p.UserId?.ToString(),
            UserName = p.User?.FirstName + " " + p.User?.LastName,
            ExternalEmail = p.ExternalEmail,
            ExternalPhone = p.ExternalPhone,
            ParticipantRoleId = p.ParticipantRoleId,
            ParticipantRoleName = p.ParticipantRole?.Name ?? "",
            ParticipantStatusId = p.ParticipantStatusId,
            ParticipantStatusName = p.ParticipantStatus?.Name ?? "",
            InvitedAt = p.InvitedAt,
            JoinedAt = p.JoinedAt,
            LeftAt = p.LeftAt,
            LastSeenAt = p.LastSeenAt,
            InvitedByUserId = p.InvitedByUserId?.ToString(),
            InvitedByUserName = p.InvitedByUser?.FirstName + " " + p.InvitedByUser?.LastName,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        };
    }

    private AppointmentInvitationDto MapToDto(AppointmentInvitation i)
    {
        return new AppointmentInvitationDto
        {
            Id = i.Id.ToString(),
            AppointmentId = i.AppointmentId.ToString(),
            InvitedByUserId = i.InvitedByUserId.ToString(),
            InvitedByUserName = i.InvitedByUser?.FirstName + " " + i.InvitedByUser?.LastName,
            InvitedUserId = i.InvitedUserId?.ToString(),
            InvitedUserName = i.InvitedUser?.FirstName + " " + i.InvitedUser?.LastName,
            InvitedEmail = i.InvitedEmail,
            InvitedPhone = i.InvitedPhone,
             InvitationStatusId = i.InvitationStatusId,
            InvitationStatusName = i.InvitationStatus?.Name ?? "",
            Message = i.Message,
            ExpiresAt = i.ExpiresAt,
            CreatedAt = i.CreatedAt,
            RespondedAt = i.RespondedAt
        };
    }

    private AppointmentPaymentLogDto MapToDto(AppointmentPaymentLog p)
    {
        return new AppointmentPaymentLogDto
        {
            Id = p.Id.ToString(),
            AppointmentId = p.AppointmentId.ToString(),
            UserId = p.UserId.ToString(),
            UserName = p.User?.FirstName + " " + p.User?.LastName,
            PaymentStatusId = p.PaymentStatusId,
            PaymentStatusName = p.PaymentStatus?.Name ?? "",
            RefundStatusId = p.RefundStatusId,
            RefundStatusName = p.RefundStatus?.Name ?? "",
            PaymentMethod = p.PaymentMethod,
            PaymentIntentId = p.PaymentIntentId,
            SessionId = p.SessionId,
            RefundId = p.RefundId,
            Amount = p.Amount,
            RefundedAmount = p.RefundedAmount,
            Currency = p.Currency,
            Description = p.Description,
            FailureReason = p.FailureReason,
            RefundReason = p.RefundReason,
            PaymentDate = p.PaymentDate,
            RefundDate = p.RefundDate,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        };
    }

    // --- APPOINTMENT CRUD OPERATIONS ---
    public async Task<ApiResponse<AppointmentDto>> CreateAppointmentAsync(CreateAppointmentDto createDto)
    {
        try
        {
            var appointment = new Appointment
            {
                PatientId = Guid.Parse(createDto.PatientId),
                ProviderId = Guid.Parse(createDto.ProviderId),
                CategoryId = Guid.Parse(createDto.CategoryId),
                SubscriptionId = Guid.TryParse(createDto.SubscriptionId, out var subId) ? subId : (Guid?)null,
                ConsultationId = Guid.TryParse(createDto.ConsultationId, out var consId) ? consId : (Guid?)null,
                AppointmentTypeId = createDto.AppointmentTypeId,
                ConsultationModeId = createDto.ConsultationModeId,
                ScheduledAt = DateTime.Parse(createDto.ScheduledAt),
                DurationMinutes = createDto.DurationMinutes,
                ReasonForVisit = createDto.ReasonForVisit,
                Symptoms = createDto.Symptoms,
                PatientNotes = createDto.PatientNotes,
                Fee = createDto.Fee,
                IsRecordingEnabled = createDto.IsRecordingEnabled,
                ExpiresAt = !string.IsNullOrEmpty(createDto.ExpiresAt) ? DateTime.Parse(createDto.ExpiresAt) : DateTime.UtcNow.AddDays(1),
                AppointmentStatusId = await _appointmentRepository.GetStatusIdByNameAsync("Pending"), // Pending status Guid
                CreatedAt = DateTime.UtcNow
            };

            await _appointmentRepository.CreateAsync(appointment);
            return ApiResponse<AppointmentDto>.SuccessResponse(MapToDto(appointment));
        }
        catch (Exception ex)
        {
            return ApiResponse<AppointmentDto>.ErrorResponse($"Failed to create appointment: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AppointmentDto>> GetAppointmentByIdAsync(Guid id)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
                return ApiResponse<AppointmentDto>.ErrorResponse("Appointment not found");

            return ApiResponse<AppointmentDto>.SuccessResponse(MapToDto(appointment));
        }
        catch (Exception ex)
        {
            return ApiResponse<AppointmentDto>.ErrorResponse($"Failed to get appointment: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<AppointmentDto>>> GetPatientAppointmentsAsync(Guid patientId)
    {
        try
        {
            var appointments = await _appointmentRepository.GetByPatientAsync(patientId);
            var appointmentDtos = appointments.Select(MapToDto);
            return ApiResponse<IEnumerable<AppointmentDto>>.SuccessResponse(appointmentDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<AppointmentDto>>.ErrorResponse($"Failed to get patient appointments: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<AppointmentDto>>> GetProviderAppointmentsAsync(Guid providerId)
    {
        try
        {
            var appointments = await _appointmentRepository.GetByProviderAsync(providerId);
            var appointmentDtos = appointments.Select(MapToDto);
            return ApiResponse<IEnumerable<AppointmentDto>>.SuccessResponse(appointmentDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<AppointmentDto>>.ErrorResponse($"Failed to get provider appointments: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<AppointmentDto>>> GetPendingAppointmentsAsync()
    {
        try
        {
            var appointments = await _appointmentRepository.GetByStatusAsync(await _appointmentRepository.GetStatusIdByNameAsync("Pending")); // Pending status Guid
            var appointmentDtos = appointments.Select(MapToDto);
            return ApiResponse<IEnumerable<AppointmentDto>>.SuccessResponse(appointmentDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<AppointmentDto>>.ErrorResponse($"Failed to get pending appointments: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AppointmentDto>> UpdateAppointmentAsync(Guid id, UpdateAppointmentDto updateDto)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
                return ApiResponse<AppointmentDto>.ErrorResponse("Appointment not found");

                                                                  if (updateDto.AppointmentStatusId.HasValue)
                appointment.AppointmentStatusId = updateDto.AppointmentStatusId.Value;
            if (updateDto.StartedAt.HasValue)
                appointment.StartedAt = updateDto.StartedAt.Value;
            if (updateDto.EndedAt.HasValue)
                appointment.EndedAt = updateDto.EndedAt.Value;
            if (updateDto.Diagnosis != null)
                appointment.Diagnosis = updateDto.Diagnosis;
            if (updateDto.Prescription != null)
                appointment.Prescription = updateDto.Prescription;
            if (updateDto.ProviderNotes != null)
                appointment.ProviderNotes = updateDto.ProviderNotes;
            if (updateDto.FollowUpInstructions != null)
                appointment.FollowUpInstructions = updateDto.FollowUpInstructions;
            if (updateDto.OpenTokSessionId != null)
                appointment.OpenTokSessionId = updateDto.OpenTokSessionId;
            if (updateDto.MeetingUrl != null)
                appointment.MeetingUrl = updateDto.MeetingUrl;
            if (updateDto.MeetingId != null)
                appointment.MeetingId = updateDto.MeetingId;
            if (updateDto.IsVideoCallStarted.HasValue)
                appointment.IsVideoCallStarted = updateDto.IsVideoCallStarted.Value;
            if (updateDto.IsVideoCallEnded.HasValue)
                appointment.IsVideoCallEnded = updateDto.IsVideoCallEnded.Value;
            if (updateDto.RecordingId != null)
                appointment.RecordingId = updateDto.RecordingId;
            if (updateDto.RecordingUrl != null)
                appointment.RecordingUrl = updateDto.RecordingUrl;
            if (updateDto.IsPatientNotified.HasValue)
                appointment.IsPatientNotified = updateDto.IsPatientNotified.Value;
            if (updateDto.IsProviderNotified.HasValue)
                appointment.IsProviderNotified = updateDto.IsProviderNotified.Value;
            if (updateDto.LastReminderSent.HasValue)
                appointment.LastReminderSent = updateDto.LastReminderSent.Value;

            appointment.UpdatedAt = DateTime.UtcNow;
            await _appointmentRepository.UpdateAsync(appointment);

            return ApiResponse<AppointmentDto>.SuccessResponse(MapToDto(appointment));
        }
        catch (Exception ex)
        {
            return ApiResponse<AppointmentDto>.ErrorResponse($"Failed to update appointment: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteAppointmentAsync(Guid id)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
                return ApiResponse<bool>.ErrorResponse("Appointment not found");

            await _appointmentRepository.DeleteAsync(id);
            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResponse($"Failed to delete appointment: {ex.Message}");
        }
    }

    // --- APPOINTMENT FLOW MANAGEMENT ---
    public async Task<ApiResponse<AppointmentDto>> BookAppointmentAsync(BookAppointmentDto bookDto)
    {
        try
        {
            var createDto = new CreateAppointmentDto
            {
                PatientId = bookDto.PatientId,
                ProviderId = bookDto.ProviderId,
                CategoryId = bookDto.CategoryId,
                SubscriptionId = bookDto.SubscriptionId,
                AppointmentTypeId = bookDto.AppointmentTypeId,
                ConsultationModeId = bookDto.ConsultationModeId,
                ScheduledAt = bookDto.ScheduledAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                DurationMinutes = bookDto.DurationMinutes,
                ReasonForVisit = bookDto.ReasonForVisit,
                Symptoms = bookDto.Symptoms,
                PatientNotes = bookDto.PatientNotes,
                Fee = 0, // Will be calculated
                IsRecordingEnabled = true
            };

            var result = await CreateAppointmentAsync(createDto);
            if (!result.Success)
                return result;

            // Calculate fee
            var fee = await CalculateAppointmentFeeAsync(Guid.Parse(bookDto.PatientId), Guid.Parse(bookDto.ProviderId), Guid.Parse(bookDto.CategoryId));
            if (fee.Success)
            {
                var appointment = result.Data;
                appointment.Fee = fee.Data;
                // Update appointment with calculated fee
                var updateDto = new UpdateAppointmentDto();
                await UpdateAppointmentAsync(Guid.Parse(appointment.Id), updateDto);
            }

            return result;
        }
        catch (Exception ex)
        {
            return ApiResponse<AppointmentDto>.ErrorResponse($"Failed to book appointment: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AppointmentDto>> ProcessPaymentAsync(Guid appointmentId, ProcessPaymentDto paymentDto)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
                return ApiResponse<AppointmentDto>.ErrorResponse("Appointment not found");

            // Create payment log
            var paymentLog = await CreatePaymentLogAsync(appointmentId, appointment.PatientId, paymentDto.Amount, paymentDto.PaymentMethod, paymentDto.PaymentIntentId, paymentDto.SessionId);

            // Update appointment payment info
            appointment.StripePaymentIntentId = paymentDto.PaymentIntentId;
            appointment.StripeSessionId = paymentDto.SessionId;
            appointment.UpdatedAt = DateTime.UtcNow;
            await _appointmentRepository.UpdateAsync(appointment);

            return ApiResponse<AppointmentDto>.SuccessResponse(MapToDto(appointment));
        }
        catch (Exception ex)
        {
            return ApiResponse<AppointmentDto>.ErrorResponse($"Failed to process payment: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AppointmentDto>> ConfirmPaymentAsync(Guid appointmentId, string paymentIntentId)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null) 
                return ApiResponse<AppointmentDto>.ErrorResponse("Appointment not found");

            // Update payment status
            var paymentLog = await _paymentLogRepository.FindByPaymentIntentIdAsync(paymentIntentId);
            if (paymentLog != null)
            {
                await UpdatePaymentStatusAsync(paymentLog.Id, await _paymentLogRepository.GetStatusIdByNameAsync("Completed")); // Completed status Guid
            }

            // Update appointment status
            appointment.AppointmentStatusId = await _appointmentRepository.GetStatusIdByNameAsync("Approved"); // Approved status Guid
            appointment.IsPaymentCaptured = true;
            appointment.UpdatedAt = DateTime.UtcNow;
            await _appointmentRepository.UpdateAsync(appointment);

            return ApiResponse<AppointmentDto>.SuccessResponse(MapToDto(appointment));
        }
        catch (Exception ex)
        {
            return ApiResponse<AppointmentDto>.ErrorResponse($"Failed to confirm payment: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AppointmentDto>> ProviderActionAsync(Guid appointmentId, string action, string? notes = null)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null) 
                return ApiResponse<AppointmentDto>.ErrorResponse("Appointment not found");

            switch (action.ToLower())
            {
                case "accept":
                    appointment.AppointmentStatusId = await _appointmentRepository.GetStatusIdByNameAsync("Approved"); // Approved status Guid
                    break;
                case "reject":
                    appointment.AppointmentStatusId = await _appointmentRepository.GetStatusIdByNameAsync("Rejected"); // Rejected status Guid
                    break;
                case "start":
                    appointment.AppointmentStatusId = await _appointmentRepository.GetStatusIdByNameAsync("InMeeting"); // In Progress status Guid
                    appointment.StartedAt = DateTime.UtcNow;
                    break;
                case "complete":
                    appointment.AppointmentStatusId = await _appointmentRepository.GetStatusIdByNameAsync("Completed"); // Completed status Guid
                    appointment.EndedAt = DateTime.UtcNow;
                    break;
                case "cancel":
                    appointment.AppointmentStatusId = await _appointmentRepository.GetStatusIdByNameAsync("Cancelled"); // Cancelled status Guid
                    break;
                default:
                    return ApiResponse<AppointmentDto>.ErrorResponse($"Unknown action: {action}");
            }

            if (!string.IsNullOrEmpty(notes))
                appointment.ProviderNotes = notes;

            appointment.UpdatedAt = DateTime.UtcNow;
            await _appointmentRepository.UpdateAsync(appointment);

            return ApiResponse<AppointmentDto>.SuccessResponse(MapToDto(appointment));
        }
        catch (Exception ex)
        {
            return ApiResponse<AppointmentDto>.ErrorResponse($"Failed to perform provider action: {ex.Message}");
        }
    }

    // --- MAPPING METHODS ---
    private AppointmentDto MapToDto(Appointment a)
    {
        return new AppointmentDto
        {
            Id = a.Id.ToString(),
            PatientId = a.PatientId.ToString(),
            PatientName = a.Patient?.FirstName + " " + a.Patient?.LastName,
            PatientEmail = a.Patient?.Email,
            ProviderId = a.ProviderId.ToString(),
            ProviderName = a.Provider?.FirstName + " " + a.Provider?.LastName,
            ProviderEmail = a.Provider?.Email,
            CategoryId = a.CategoryId.ToString(),
            CategoryName = a.Category?.Name,
            SubscriptionId = a.SubscriptionId.ToString(),
            SubscriptionName = a.Subscription?.SubscriptionPlan?.Name ?? "",
            ConsultationId = a.ConsultationId.ToString(),
            AppointmentStatusId = a.AppointmentStatusId,
            AppointmentStatusName = a.AppointmentStatus?.Name ?? "",
            AppointmentTypeId = a.AppointmentTypeId,
            AppointmentTypeName = a.AppointmentType?.Name ?? "",
            ConsultationModeId = a.ConsultationModeId,
            ConsultationModeName = a.ConsultationMode?.Name ?? "",
            ScheduledAt = a.ScheduledAt,
            StartedAt = a.StartedAt,
            EndedAt = a.EndedAt,
            DurationMinutes = a.DurationMinutes,
            ReasonForVisit = a.ReasonForVisit,
            Symptoms = a.Symptoms,
            PatientNotes = a.PatientNotes,
            Diagnosis = a.Diagnosis,
            Prescription = a.Prescription,
            ProviderNotes = a.ProviderNotes,
            FollowUpInstructions = a.FollowUpInstructions,
            Fee = a.Fee,
            StripePaymentIntentId = a.StripePaymentIntentId,
            StripeSessionId = a.StripeSessionId,
            IsPaymentCaptured = a.IsPaymentCaptured,
            IsRefunded = a.IsRefunded,
            RefundAmount = a.RefundAmount,
            OpenTokSessionId = a.OpenTokSessionId,
            MeetingUrl = a.MeetingUrl,
            MeetingId = a.MeetingId,
            IsVideoCallStarted = a.IsVideoCallStarted,
            IsVideoCallEnded = a.IsVideoCallEnded,
            RecordingId = a.RecordingId,
            RecordingUrl = a.RecordingUrl,
            IsRecordingEnabled = a.IsRecordingEnabled,
            IsPatientNotified = a.IsPatientNotified,
            IsProviderNotified = a.IsProviderNotified,
            LastReminderSent = a.LastReminderSent,
            ExpiresAt = a.ExpiresAt,
            AutoCancellationAt = a.AutoCancellationAt,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt,
            IsActive = a.IsActive,
            IsCompleted = a.IsCompleted,
            IsCancelled = a.IsCancelled,
            Duration = a.Duration,
            IsExpired = a.IsExpired,
            DocumentCount = a.Documents?.Count ?? 0,
            ReminderCount = a.Reminders?.Count ?? 0,
            EventCount = a.Events?.Count ?? 0
        };
    }

    public async Task<ApiResponse<IEnumerable<AppointmentParticipantDto>>> GetParticipantsAsync(Guid appointmentId)
    {
        try
        {
            var participants = await _participantRepository.GetByAppointmentAsync(appointmentId);
            return ApiResponse<IEnumerable<AppointmentParticipantDto>>.SuccessResponse(participants.Select(MapToDto));
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<AppointmentParticipantDto>>.ErrorResponse($"Failed to get participants: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<AppointmentPaymentLogDto>>> GetPaymentLogsAsync(Guid appointmentId)
    {
        try
        {
            var paymentLogs = await _paymentLogRepository.GetByAppointmentAsync(appointmentId);
            return ApiResponse<IEnumerable<AppointmentPaymentLogDto>>.SuccessResponse(paymentLogs.Select(MapToDto));
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<AppointmentPaymentLogDto>>.ErrorResponse($"Failed to get payment logs: {ex.Message}");
        }
    }

    // --- STUB IMPLEMENTATIONS FOR INTERFACE METHODS ---
    public async Task<ApiResponse<AppointmentDto>> ProviderAcceptAppointmentAsync(Guid appointmentId, SmartTelehealth.Application.DTOs.ProviderAcceptDto acceptDto)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
                return ApiResponse<AppointmentDto>.ErrorResponse("Appointment not found");

            if (appointment.AppointmentStatusId != await _appointmentRepository.GetStatusIdByNameAsync("Pending")) // Pending status Guid
                return ApiResponse<AppointmentDto>.ErrorResponse("Appointment is not in pending status");

            // Update appointment status to accepted
            appointment.AppointmentStatusId = await _appointmentRepository.GetStatusIdByNameAsync("Accepted"); // Accepted status Guid
            appointment.ProviderNotes = acceptDto.ProviderNotes;
            appointment.UpdatedAt = DateTime.UtcNow;
            appointment.AcceptedAt = DateTime.UtcNow;

            await _appointmentRepository.UpdateAsync(appointment);

            // Send notification to patient
            // await _notificationService.SendAppointmentAcceptedNotificationAsync(appointment.PatientId, appointmentId);

            return ApiResponse<AppointmentDto>.SuccessResponse(MapToDto(appointment));
        }
        catch (Exception ex)
        {
            return ApiResponse<AppointmentDto>.ErrorResponse($"Failed to accept appointment: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AppointmentDto>> ProviderRejectAppointmentAsync(Guid appointmentId, SmartTelehealth.Application.DTOs.ProviderRejectDto rejectDto)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
                return ApiResponse<AppointmentDto>.ErrorResponse("Appointment not found");

            if (appointment.AppointmentStatusId != await _appointmentRepository.GetStatusIdByNameAsync("Pending")) // Pending status Guid
                return ApiResponse<AppointmentDto>.ErrorResponse("Appointment is not in pending status");

            // Update appointment status to rejected
            appointment.AppointmentStatusId = await _appointmentRepository.GetStatusIdByNameAsync("Rejected"); // Rejected status Guid
            appointment.ProviderNotes = rejectDto.Reason;
            appointment.UpdatedAt = DateTime.UtcNow;
            appointment.RejectedAt = DateTime.UtcNow;

            await _appointmentRepository.UpdateAsync(appointment);

            // Send notification to patient
            // await _notificationService.SendAppointmentRejectedNotificationAsync(appointment.PatientId, appointmentId, rejectDto.Reason);

            return ApiResponse<AppointmentDto>.SuccessResponse(MapToDto(appointment));
        }
        catch (Exception ex)
        {
            return ApiResponse<AppointmentDto>.ErrorResponse($"Failed to reject appointment: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AppointmentDto>> StartMeetingAsync(Guid appointmentId)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
                return ApiResponse<AppointmentDto>.ErrorResponse("Appointment not found");

            if (appointment.AppointmentStatusId != await _appointmentRepository.GetStatusIdByNameAsync("Accepted")) // Accepted status Guid
                return ApiResponse<AppointmentDto>.ErrorResponse("Appointment must be accepted before starting");

            // Update appointment status to in progress
            appointment.AppointmentStatusId = await _appointmentRepository.GetStatusIdByNameAsync("InMeeting"); // In Progress status Guid
            appointment.StartedAt = DateTime.UtcNow;
            appointment.UpdatedAt = DateTime.UtcNow;

            // Create or get video session
            var sessionId = await GetOrCreateVideoSessionAsync(appointmentId);
            appointment.OpenTokSessionId = sessionId;

            await _appointmentRepository.UpdateAsync(appointment);

            // Send notification to participants
            // await _notificationService.SendMeetingStartedNotificationAsync(appointmentId);

            return ApiResponse<AppointmentDto>.SuccessResponse(MapToDto(appointment));
        }
        catch (Exception ex)
        {
            return ApiResponse<AppointmentDto>.ErrorResponse($"Failed to start meeting: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AppointmentDto>> EndMeetingAsync(Guid appointmentId)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
                return ApiResponse<AppointmentDto>.ErrorResponse("Appointment not found");

            if (appointment.AppointmentStatusId != await _appointmentRepository.GetStatusIdByNameAsync("InMeeting")) // In Progress status Guid
                return ApiResponse<AppointmentDto>.ErrorResponse("Appointment is not in progress");

            // Update appointment status to ended
            appointment.AppointmentStatusId = await _appointmentRepository.GetStatusIdByNameAsync("Ended"); // Ended status Guid
            appointment.EndedAt = DateTime.UtcNow;
            appointment.UpdatedAt = DateTime.UtcNow;

            await _appointmentRepository.UpdateAsync(appointment);

            // Send notification to participants
            // await _notificationService.SendMeetingEndedNotificationAsync(appointmentId);

            return ApiResponse<AppointmentDto>.SuccessResponse(MapToDto(appointment));
        }
        catch (Exception ex)
        {
            return ApiResponse<AppointmentDto>.ErrorResponse($"Failed to end meeting: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AppointmentDto>> CompleteAppointmentAsync(Guid appointmentId, SmartTelehealth.Application.DTOs.CompleteAppointmentDto completeDto)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
                return ApiResponse<AppointmentDto>.ErrorResponse("Appointment not found");

            if (appointment.AppointmentStatusId != await _appointmentRepository.GetStatusIdByNameAsync("InMeeting") && appointment.AppointmentStatusId != await _appointmentRepository.GetStatusIdByNameAsync("Ended")) // In Progress or Ended status Guid
                return ApiResponse<AppointmentDto>.ErrorResponse("Appointment must be in progress or ended to complete");

            // Update appointment status to completed
            appointment.AppointmentStatusId = await _appointmentRepository.GetStatusIdByNameAsync("Completed"); // Completed status Guid
            appointment.CompletedAt = DateTime.UtcNow;
            appointment.UpdatedAt = DateTime.UtcNow;
            appointment.ProviderNotes = completeDto.ProviderNotes;
            appointment.Diagnosis = completeDto.Diagnosis;
            appointment.Prescription = completeDto.Prescription;
            appointment.FollowUpInstructions = completeDto.FollowUpInstructions;

            await _appointmentRepository.UpdateAsync(appointment);

            // Send notification to patient
            // await _notificationService.SendAppointmentCompletedNotificationAsync(appointment.PatientId, appointmentId);

            return ApiResponse<AppointmentDto>.SuccessResponse(MapToDto(appointment));
        }
        catch (Exception ex)
        {
            return ApiResponse<AppointmentDto>.ErrorResponse($"Failed to complete appointment: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AppointmentDto>> CancelAppointmentAsync(Guid appointmentId, string reason)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
                return ApiResponse<AppointmentDto>.ErrorResponse("Appointment not found");

            if (appointment.AppointmentStatusId == await _appointmentRepository.GetStatusIdByNameAsync("Completed")) // Completed status Guid
                return ApiResponse<AppointmentDto>.ErrorResponse("Cannot cancel completed appointment");

            // Update appointment status to cancelled
            appointment.AppointmentStatusId = await _appointmentRepository.GetStatusIdByNameAsync("Cancelled"); // Cancelled status Guid
            appointment.CancelledAt = DateTime.UtcNow;
            appointment.UpdatedAt = DateTime.UtcNow;
            appointment.CancellationReason = reason;

            await _appointmentRepository.UpdateAsync(appointment);

            // Send notification to participants
            // await _notificationService.SendAppointmentCancelledNotificationAsync(appointmentId, reason);

            return ApiResponse<AppointmentDto>.SuccessResponse(MapToDto(appointment));
        }
        catch (Exception ex)
        {
            return ApiResponse<AppointmentDto>.ErrorResponse($"Failed to cancel appointment: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> GenerateMeetingLinkAsync(Guid appointmentId)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
                return ApiResponse<string>.ErrorResponse("Appointment not found");

            // Create or get video session
            var sessionId = await GetOrCreateVideoSessionAsync(appointmentId);
            
            // Generate meeting URL
            var meetingUrl = $"/meeting/{appointmentId}?session={sessionId}";
            
            return ApiResponse<string>.SuccessResponse(meetingUrl);
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.ErrorResponse($"Failed to generate meeting link: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> GetOpenTokTokenAsync(Guid appointmentId, Guid userId)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
                return ApiResponse<string>.ErrorResponse("Appointment not found");

            // Get user details
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return ApiResponse<string>.ErrorResponse("User not found");

            // Generate token for the user
            var participantRole = await _participantRoleRepository.GetByIdAsync(Guid.Parse("00000000-0000-0000-0000-000000000001")); // Default role
            var openTokRole = MapParticipantRoleNameToOpenTokRole(participantRole.Name);
            var token = await GenerateVideoTokenAsync(appointmentId, userId, user.Email, participantRole.Id);
            
            return ApiResponse<string>.SuccessResponse(token);
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.ErrorResponse($"Failed to get OpenTok token: {ex.Message}");
        }
    }

    public Task<ApiResponse<bool>> StartRecordingAsync(Guid appointmentId)
    {
        // TODO: Implement start recording
        throw new NotImplementedException();
    }

    public Task<ApiResponse<bool>> StopRecordingAsync(Guid appointmentId)
    {
        // TODO: Implement stop recording
        throw new NotImplementedException();
    }

    public Task<ApiResponse<string>> GetRecordingUrlAsync(Guid appointmentId)
    {
        // TODO: Implement get recording URL
        throw new NotImplementedException();
    }

    public Task<ApiResponse<bool>> CapturePaymentAsync(Guid appointmentId)
    {
        // TODO: Implement capture payment
        throw new NotImplementedException();
    }

    public Task<ApiResponse<bool>> RefundPaymentAsync(Guid appointmentId, decimal? amount = null)
    {
        // TODO: Implement refund payment
        throw new NotImplementedException();
    }

    public Task<ApiResponse<PaymentStatusDto>> GetPaymentStatusAsync(Guid appointmentId)
    {
        // TODO: Implement get payment status
        throw new NotImplementedException();
    }

    public Task<ApiResponse<AppointmentReminderDto>> ScheduleReminderAsync(Guid appointmentId, ScheduleReminderDto reminderDto)
    {
        // TODO: Implement schedule reminder
        throw new NotImplementedException();
    }

    public Task<ApiResponse<IEnumerable<AppointmentReminderDto>>> GetAppointmentRemindersAsync(Guid appointmentId)
    {
        // TODO: Implement get appointment reminders
        throw new NotImplementedException();
    }

    public Task<ApiResponse<bool>> SendReminderAsync(Guid reminderId)
    {
        // TODO: Implement send reminder
        throw new NotImplementedException();
    }

    public Task<ApiResponse<bool>> LogAppointmentEventAsync(Guid appointmentId, LogAppointmentEventDto eventDto)
    {
        // TODO: Implement log appointment event
        throw new NotImplementedException();
    }

    public Task<ApiResponse<IEnumerable<AppointmentEventDto>>> GetAppointmentEventsAsync(Guid appointmentId)
    {
        // TODO: Implement get appointment events
        throw new NotImplementedException();
    }

    public async Task<ApiResponse<IEnumerable<ProviderAvailabilityDto>>> GetProviderAvailabilityAsync(Guid providerId, DateTime date)
    {
        try
        {
            // TODO: Implement actual provider availability logic
            var availability = new List<ProviderAvailabilityDto>
            {
                new ProviderAvailabilityDto
                {
                    DayOfWeek = date.DayOfWeek,
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(17, 0, 0),
                    IsAvailable = true,
                    Notes = "Standard business hours"
                }
            };
            
            return ApiResponse<IEnumerable<ProviderAvailabilityDto>>.SuccessResponse(availability);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<ProviderAvailabilityDto>>.ErrorResponse($"Failed to get provider availability: {ex.Message}");
        }
    }

    public Task<ApiResponse<bool>> CheckProviderAvailabilityAsync(Guid providerId, DateTime startTime, DateTime endTime)
    {
        // TODO: Implement check provider availability
        throw new NotImplementedException();
    }

    public Task<ApiResponse<bool>> ValidateSubscriptionAccessAsync(Guid patientId, Guid categoryId)
    {
        // TODO: Implement validate subscription access
        throw new NotImplementedException();
    }

    public Task<ApiResponse<decimal>> CalculateAppointmentFeeAsync(Guid patientId, Guid providerId, Guid categoryId)
    {
        // TODO: Implement calculate appointment fee
        throw new NotImplementedException();
    }

    public Task<ApiResponse<bool>> ApplySubscriptionDiscountAsync(Guid appointmentId)
    {
        // TODO: Implement apply subscription discount
        throw new NotImplementedException();
    }

    public Task<ApiResponse<bool>> ProcessExpiredAppointmentsAsync()
    {
        // TODO: Implement process expired appointments
        throw new NotImplementedException();
    }

    public Task<ApiResponse<bool>> AutoCancelAppointmentAsync(Guid appointmentId)
    {
        // TODO: Implement auto cancel appointment
        throw new NotImplementedException();
    }

    public Task<ApiResponse<AppointmentAnalyticsDto>> GetAppointmentAnalyticsAsync(DateTime startDate, DateTime endDate)
    {
        // TODO: Implement get appointment analytics
        throw new NotImplementedException();
    }

    public async Task<ApiResponse<IEnumerable<AppointmentDto>>> GetAppointmentsByStatusAsync(Guid appointmentStatusId)
    {
        try
        {
            var appointments = await _appointmentRepository.GetByStatusAsync(appointmentStatusId);
            var appointmentDtos = appointments.Select(MapToDto);
            return ApiResponse<IEnumerable<AppointmentDto>>.SuccessResponse(appointmentDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<AppointmentDto>>.ErrorResponse($"Failed to get appointments by status: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<AppointmentDto>>> GetUpcomingAppointmentsAsync()
    {
        try
        {
            var appointments = await _appointmentRepository.GetUpcomingAsync();
            var appointmentDtos = appointments.Select(MapToDto);
            return ApiResponse<IEnumerable<AppointmentDto>>.SuccessResponse(appointmentDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<AppointmentDto>>.ErrorResponse($"Failed to get upcoming appointments: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<CategoryWithSubscriptionsDto>>> GetCategoriesWithSubscriptionsAsync()
    {
        try
        {
            // TODO: Implement actual logic
            return ApiResponse<IEnumerable<CategoryWithSubscriptionsDto>>.SuccessResponse(new List<CategoryWithSubscriptionsDto>());
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<CategoryWithSubscriptionsDto>>.ErrorResponse($"Failed to get categories with subscriptions: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<FeaturedProviderDto>>> GetFeaturedProvidersAsync()
    {
        try
        {
            // TODO: Implement actual logic
            return ApiResponse<IEnumerable<FeaturedProviderDto>>.SuccessResponse(new List<FeaturedProviderDto>());
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<FeaturedProviderDto>>.ErrorResponse($"Failed to get featured providers: {ex.Message}");
        }
    }

    public Task<ApiResponse<bool>> IsAppointmentServiceHealthyAsync()
    {
        // TODO: Implement health check
        throw new NotImplementedException();
    }

    private OpenTokRole MapParticipantRoleNameToOpenTokRole(string roleName)
    {
        return roleName switch
        {
            "Patient" => OpenTokRole.Publisher,
            "Provider" => OpenTokRole.Moderator,
            "External" => OpenTokRole.Subscriber,
            _ => OpenTokRole.Publisher
        };
    }
} 