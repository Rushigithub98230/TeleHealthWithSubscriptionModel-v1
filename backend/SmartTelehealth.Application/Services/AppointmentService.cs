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
    
    public async Task<JsonModel> UploadDocumentAsync(Guid appointmentId, UploadDocumentDto uploadDto)
    {
        try
        {
            // Validate appointment exists
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Appointment not found",
                    StatusCode = 404
                };

            // Use the file content directly since it's already a byte array
            var fileBytes = uploadDto.FileContent;

            // Get document type for appointment documents (you can make this configurable)
            var appointmentDocumentTypes = await _documentTypeService.GetAllDocumentTypesAsync(true);
            var appointmentDocType = GetAppointmentDocumentType(appointmentDocumentTypes.data);

            if (appointmentDocType == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "No suitable document type found for appointment documents",
                    StatusCode = 400
                };
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
                CreatedById = null // Will be set by the document service from current user context
            };

            // Upload using centralized document service
            var result = await _documentService.UploadDocumentAsync(uploadRequest);
            
            // Note: Document count is now managed by the centralized document service
            // No need to update appointment entity

            return result;
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to upload document: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetAppointmentDocumentsAsync(Guid appointmentId)
    {
        try
        {
            // Validate appointment exists
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Appointment not found",
                    StatusCode = 404
                };

            // Get documents using centralized document service
            var documentsResult = await _documentService.GetDocumentsByEntityAsync("Appointment", appointmentId);
            
            if (documentsResult.StatusCode == 200)
            {
                return new JsonModel
                {
                    data = documentsResult.data,
                    Message = "Documents retrieved successfully",
                    StatusCode = 200
                };
            }

            return new JsonModel
            {
                data = new object(),
                Message = "Failed to retrieve appointment documents",
                StatusCode = 500
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to get appointment documents: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> DeleteDocumentAsync(Guid documentId)
    {
        try
        {
            // Get current user ID from context (you'll need to implement this based on your authentication)
            var currentUserId = 0; // Replace with actual user ID from context

            // Delete using centralized document service
            var result = await _documentService.DeleteDocumentAsync(documentId, currentUserId);
            
            // Note: Document count is now managed by the centralized document service
            // No need to update appointment entity

            return result;
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to delete document: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    // --- PARTICIPANT MANAGEMENT ---
    public async Task<JsonModel> AddParticipantAsync(Guid appointmentId, int? userId, string? email, string? phone, Guid participantRoleId, int invitedByUserId)
    {
        try
        {
            // Enforce max participants
            var participants = await _participantRepository.GetByAppointmentAsync(appointmentId);
            if (participants.Count() >= DefaultMaxParticipants)
                return new JsonModel
                {
                    data = new object(),
                    Message = $"Max participants ({DefaultMaxParticipants}) reached.",
                    StatusCode = 400
                };

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
                CreatedDate = DateTime.UtcNow
            };
            await _participantRepository.CreateAsync(participant);
            // Optionally send notification
            return new JsonModel
            {
                data = MapToDto(participant),
                Message = "Participant added successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to add participant: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    // --- INVITATION MANAGEMENT ---
    public async Task<JsonModel> InviteExternalAsync(Guid appointmentId, string email, string? phone, string? message, int invitedByUserId)
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
                CreatedDate = DateTime.UtcNow
            };
            await _invitationRepository.CreateAsync(invitation);
            // Send email/SMS with meeting link (stub)
            return new JsonModel
            {
                data = MapToDto(invitation),
                Message = "Invitation sent successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to invite external: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    // --- JOIN TRACKING ---
    public async Task<JsonModel> MarkParticipantJoinedAsync(Guid appointmentId, int? userId, string? email)
    {
        try
        {
            var participant = await _participantRepository.GetByAppointmentAndUserAsync(appointmentId, userId);
            if (participant == null) 
                return new JsonModel
                {
                    data = new object(),
                    Message = "Participant not found",
                    StatusCode = 404
                };
            
            participant.ParticipantStatusId = await _participantRepository.GetStatusIdByNameAsync("Joined"); // Joined status Guid
            participant.JoinedAt = DateTime.UtcNow;
            await _participantRepository.UpdateAsync(participant);
            return new JsonModel
            {
                data = true,
                Message = "Participant marked as joined",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to mark participant joined: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> MarkParticipantLeftAsync(Guid appointmentId, int? userId, string? email)
    {
        try
        {
            var participant = await _participantRepository.GetByAppointmentAndUserAsync(appointmentId, userId);
            if (participant == null) 
                return new JsonModel
                {
                    data = new object(),
                    Message = "Participant not found",
                    StatusCode = 404
                };
            
            participant.ParticipantStatusId = await _participantRepository.GetStatusIdByNameAsync("Left"); // Left status Guid
            participant.LeftAt = DateTime.UtcNow;
            await _participantRepository.UpdateAsync(participant);
            return new JsonModel
            {
                data = true,
                Message = "Participant marked as left",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to mark participant left: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    // --- GROUP VIDEO SESSION ---
    public async Task<string> GetOrCreateVideoSessionAsync(Guid appointmentId)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
        if (string.IsNullOrEmpty(appointment.OpenTokSessionId))
        {
            var sessionResult = await _openTokService.CreateSessionAsync($"Appointment-{appointmentId}");
                            var dynamicData = sessionResult.data as dynamic;
            if (dynamicData != null)
            {
                appointment.OpenTokSessionId = dynamicData.SessionId ?? string.Empty;
            }
            await _appointmentRepository.UpdateAsync(appointment);
        }
        return appointment.OpenTokSessionId;
    }

    public async Task<string> GenerateVideoTokenAsync(Guid appointmentId, int? userId, string? email, Guid participantRoleId)
    {
        var sessionId = await GetOrCreateVideoSessionAsync(appointmentId);
        var participantRole = await _participantRoleRepository.GetByIdAsync(participantRoleId);
        var openTokRole = MapParticipantRoleNameToOpenTokRole(participantRole.Name);
        var tokenResult = await _openTokService.GenerateTokenAsync(sessionId, userId?.ToString() ?? string.Empty, email ?? string.Empty, openTokRole);
                    return tokenResult.data?.ToString() ?? string.Empty;
    }

    // --- PAYMENT MANAGEMENT ---
    public async Task<JsonModel> CreatePaymentLogAsync(Guid appointmentId, int userId, decimal amount, string paymentMethod, string? paymentIntentId = null, string? sessionId = null)
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
                CreatedDate = DateTime.UtcNow
            };
            await _paymentLogRepository.CreateAsync(paymentLog);
            return new JsonModel
            {
                data = MapToDto(paymentLog),
                Message = "Payment log created successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to create payment log: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> UpdatePaymentStatusAsync(Guid paymentLogId, Guid paymentStatusId, string? failureReason = null)
    {
        try
        {
            var paymentLog = await _paymentLogRepository.GetByIdAsync(paymentLogId);
            if (paymentLog == null) 
                return new JsonModel
                {
                    data = new object(),
                    Message = "Payment log not found",
                    StatusCode = 404
                };
            
            paymentLog.PaymentStatusId = paymentStatusId;
            paymentLog.FailureReason = failureReason;
            paymentLog.PaymentDate = paymentStatusId == await _paymentLogRepository.GetStatusIdByNameAsync("Completed") ? DateTime.UtcNow : null; // Completed status Guid
            paymentLog.UpdatedDate = DateTime.UtcNow;
            
            await _paymentLogRepository.UpdateAsync(paymentLog);
            return new JsonModel
            {
                data = MapToDto(paymentLog),
                Message = "Payment status updated successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to update payment status: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> ProcessRefundAsync(Guid appointmentId, decimal refundAmount, string reason)
    {
        try
        {
            var latestPayment = await _paymentLogRepository.GetLatestByAppointmentAsync(appointmentId);
            if (latestPayment == null) 
                return new JsonModel
                {
                    data = new object(),
                    Message = "No payment found for appointment",
                    StatusCode = 404
                };
            
            // Process refund through Stripe
            var refundSuccess = await _stripeService.ProcessRefundAsync(latestPayment.PaymentIntentId, refundAmount);
            var refundId = refundSuccess ? Guid.NewGuid().ToString() : null; // Generate refund ID if successful
            
            latestPayment.RefundStatusId = await _paymentLogRepository.GetStatusIdByNameAsync("Completed"); // Completed status Guid
            latestPayment.RefundedAmount = refundAmount;
            latestPayment.RefundId = refundId;
            latestPayment.RefundReason = reason;
            latestPayment.RefundDate = DateTime.UtcNow;
                            latestPayment.UpdatedDate = DateTime.UtcNow;
            
            if (refundAmount >= latestPayment.Amount)
            {
                latestPayment.PaymentStatusId = await _paymentLogRepository.GetStatusIdByNameAsync("Refunded"); // Refunded status Guid
            }
            else
            {
                latestPayment.PaymentStatusId = await _paymentLogRepository.GetStatusIdByNameAsync("PartiallyRefunded"); // PartiallyRefunded status Guid
            }
            
            await _paymentLogRepository.UpdateAsync(latestPayment);
            return new JsonModel
            {
                data = MapToDto(latestPayment),
                Message = "Refund processed successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to process refund: {ex.Message}",
                StatusCode = 500
            };
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
            CreatedAt = p.CreatedDate ?? DateTime.UtcNow,
            UpdatedAt = p.UpdatedDate ?? DateTime.UtcNow
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
            CreatedAt = i.CreatedDate ?? DateTime.UtcNow,
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
            CreatedAt = p.CreatedDate ?? DateTime.UtcNow,
            UpdatedAt = p.UpdatedDate ?? DateTime.UtcNow
        };
    }

    // --- APPOINTMENT CRUD OPERATIONS ---
    public async Task<JsonModel> CreateAppointmentAsync(CreateAppointmentDto createDto)
    {
        try
        {
            var appointment = new Appointment
            {
                PatientId = int.Parse(createDto.PatientId),
                ProviderId = int.Parse(createDto.ProviderId),
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
                CreatedDate = DateTime.UtcNow
            };

            await _appointmentRepository.CreateAsync(appointment);
            return new JsonModel
            {
                data = MapToDto(appointment),
                Message = "Appointment created successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to create appointment: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetAppointmentByIdAsync(Guid id)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Appointment not found",
                    StatusCode = 404
                };

            return new JsonModel
            {
                data = MapToDto(appointment),
                Message = "Appointment retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to get appointment: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetPatientAppointmentsAsync(int patientId)
    {
        try
        {
            var appointments = await _appointmentRepository.GetByPatientAsync(patientId);
            var appointmentDtos = appointments.Select(MapToDto);
            return new JsonModel
            {
                data = appointmentDtos,
                Message = "Patient appointments retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to get patient appointments: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetProviderAppointmentsAsync(int providerId)
    {
        try
        {
            var appointments = await _appointmentRepository.GetByProviderAsync(providerId);
            var appointmentDtos = appointments.Select(MapToDto);
            return new JsonModel
            {
                data = appointmentDtos,
                Message = "Provider appointments retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to get provider appointments: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetPendingAppointmentsAsync()
    {
        try
        {
            var appointments = await _appointmentRepository.GetByStatusAsync(await _appointmentRepository.GetStatusIdByNameAsync("Pending")); // Pending status Guid
            var appointmentDtos = appointments.Select(MapToDto);
            return new JsonModel
            {
                data = appointmentDtos,
                Message = "Pending appointments retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to get pending appointments: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> UpdateAppointmentAsync(Guid id, UpdateAppointmentDto updateDto)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Appointment not found",
                    StatusCode = 404
                };

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

            appointment.UpdatedDate = DateTime.UtcNow;
            await _appointmentRepository.UpdateAsync(appointment);

            return new JsonModel
            {
                data = MapToDto(appointment),
                Message = "Appointment updated successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to update appointment: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> DeleteAppointmentAsync(Guid id)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Appointment not found",
                    StatusCode = 404
                };

            await _appointmentRepository.DeleteAsync(id);
            return new JsonModel
            {
                data = true,
                Message = "Appointment deleted successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to delete appointment: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    // --- APPOINTMENT FLOW MANAGEMENT ---
    public async Task<JsonModel> BookAppointmentAsync(BookAppointmentDto bookDto)
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
            if (result.StatusCode != 200)
                return result;

            // Calculate fee
            var fee = await CalculateAppointmentFeeAsync(int.Parse(bookDto.PatientId), int.Parse(bookDto.ProviderId), Guid.Parse(bookDto.CategoryId));
            if (fee.StatusCode == 200)
            {
                var appointment = (AppointmentDto)result.data;
                appointment.Fee = (decimal)fee.data;
                // Update appointment with calculated fee
                var updateDto = new UpdateAppointmentDto();
                await UpdateAppointmentAsync(Guid.Parse(appointment.Id), updateDto);
            }

            return result;
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to book appointment: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> ProcessPaymentAsync(Guid appointmentId, ProcessPaymentDto paymentDto)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Appointment not found",
                    StatusCode = 404
                };

            // Create payment log
            var paymentLog = await CreatePaymentLogAsync(appointmentId, appointment.PatientId, paymentDto.Amount, paymentDto.PaymentMethod, paymentDto.PaymentIntentId, paymentDto.SessionId);

            // Update appointment payment info
            appointment.StripePaymentIntentId = paymentDto.PaymentIntentId;
            appointment.StripeSessionId = paymentDto.SessionId;
            appointment.UpdatedDate = DateTime.UtcNow;
            await _appointmentRepository.UpdateAsync(appointment);

            return new JsonModel
            {
                data = MapToDto(appointment),
                Message = "Payment processed successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to process payment: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> ConfirmPaymentAsync(Guid appointmentId, string paymentIntentId)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null) 
                return new JsonModel
                {
                    data = new object(),
                    Message = "Appointment not found",
                    StatusCode = 404
                };

            // Update payment status
            var paymentLog = await _paymentLogRepository.FindByPaymentIntentIdAsync(paymentIntentId);
            if (paymentLog != null)
            {
                await UpdatePaymentStatusAsync(paymentLog.Id, await _paymentLogRepository.GetStatusIdByNameAsync("Completed")); // Completed status Guid
            }

            // Update appointment status
            appointment.AppointmentStatusId = Guid.Parse("Approved"); // Approved status Guid
            appointment.IsPaymentCaptured = true;
            appointment.UpdatedDate = DateTime.UtcNow;
            await _appointmentRepository.UpdateAsync(appointment);

            return new JsonModel
            {
                data = MapToDto(appointment),
                Message = "Payment confirmed successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to confirm payment: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> ProviderActionAsync(Guid appointmentId, string action, string? notes = null)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null) 
                return new JsonModel
                {
                    data = new object(),
                    Message = "Appointment not found",
                    StatusCode = 404
                };

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
                    return new JsonModel
                    {
                        data = new object(),
                        Message = $"Unknown action: {action}",
                        StatusCode = 400
                    };
            }

            if (!string.IsNullOrEmpty(notes))
                appointment.ProviderNotes = notes;

            appointment.UpdatedDate = DateTime.UtcNow;
            await _appointmentRepository.UpdateAsync(appointment);

            return new JsonModel
            {
                data = MapToDto(appointment),
                Message = "Provider action completed successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to perform provider action: {ex.Message}",
                StatusCode = 500
            };
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
            CreatedAt = a.CreatedDate ?? DateTime.UtcNow,
            UpdatedAt = a.UpdatedDate ?? DateTime.UtcNow,
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

    public async Task<JsonModel> GetParticipantsAsync(Guid appointmentId)
    {
        try
        {
            var participants = await _participantRepository.GetByAppointmentAsync(appointmentId);
            return new JsonModel
            {
                data = participants.Select(MapToDto),
                Message = "Participants retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to get participants: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetPaymentLogsAsync(Guid appointmentId)
    {
        try
        {
            var paymentLogs = await _paymentLogRepository.GetByAppointmentAsync(appointmentId);
            return new JsonModel
            {
                data = paymentLogs.Select(MapToDto),
                Message = "Payment logs retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to get payment logs: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    // --- STUB IMPLEMENTATIONS FOR INTERFACE METHODS ---
    public async Task<JsonModel> ProviderAcceptAppointmentAsync(Guid appointmentId, SmartTelehealth.Application.DTOs.ProviderAcceptDto acceptDto)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Appointment not found",
                    StatusCode = 404
                };

            if (appointment.AppointmentStatusId != await _appointmentRepository.GetStatusIdByNameAsync("Pending")) // Pending status Guid
                return new JsonModel
                {
                    data = new object(),
                    Message = "Appointment is not in pending status",
                    StatusCode = 400
                };

            // Update appointment status to accepted
            appointment.AppointmentStatusId = await _appointmentRepository.GetStatusIdByNameAsync("Accepted"); // Accepted status Guid
            appointment.ProviderNotes = acceptDto.ProviderNotes;
            appointment.UpdatedDate = DateTime.UtcNow;
            appointment.AcceptedAt = DateTime.UtcNow;

            await _appointmentRepository.UpdateAsync(appointment);

            // Send notification to patient
            // await _notificationService.SendAppointmentAcceptedNotificationAsync(appointment.PatientId, appointmentId);

            return new JsonModel
            {
                data = MapToDto(appointment),
                Message = "Appointment accepted successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to accept appointment: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> ProviderRejectAppointmentAsync(Guid appointmentId, SmartTelehealth.Application.DTOs.ProviderRejectDto rejectDto)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Appointment not found",
                    StatusCode = 404
                };

            if (appointment.AppointmentStatusId != await _appointmentRepository.GetStatusIdByNameAsync("Pending")) // Pending status Guid
                return new JsonModel
                {
                    data = new object(),
                    Message = "Appointment is not in pending status",
                    StatusCode = 400
                };

            // Update appointment status to rejected
            appointment.AppointmentStatusId = await _appointmentRepository.GetStatusIdByNameAsync("Rejected"); // Rejected status Guid
            appointment.ProviderNotes = rejectDto.Reason;
            appointment.UpdatedDate = DateTime.UtcNow;
            appointment.RejectedAt = DateTime.UtcNow;

            await _appointmentRepository.UpdateAsync(appointment);

            // Send notification to patient
            // await _notificationService.SendAppointmentRejectedNotificationAsync(appointment.PatientId, appointmentId, rejectDto.Reason);

            return new JsonModel
            {
                data = MapToDto(appointment),
                Message = "Appointment rejected successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to reject appointment: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> StartMeetingAsync(Guid appointmentId)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Appointment not found",
                    StatusCode = 404
                };

            if (appointment.AppointmentStatusId != await _appointmentRepository.GetStatusIdByNameAsync("Accepted")) // Accepted status Guid
                return new JsonModel
                {
                    data = new object(),
                    Message = "Appointment must be accepted before starting",
                    StatusCode = 400
                };

            // Update appointment status to in progress
            appointment.AppointmentStatusId = await _appointmentRepository.GetStatusIdByNameAsync("InMeeting"); // In Progress status Guid
            appointment.StartedAt = DateTime.UtcNow;
            appointment.UpdatedDate = DateTime.UtcNow;

            // Create or get video session
            var sessionId = await GetOrCreateVideoSessionAsync(appointmentId);
            appointment.OpenTokSessionId = sessionId;

            await _appointmentRepository.UpdateAsync(appointment);

            // Send notification to participants
            // await _notificationService.SendMeetingStartedNotificationAsync(appointmentId);

            return new JsonModel
            {
                data = MapToDto(appointment),
                Message = "Meeting started successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to start meeting: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> EndMeetingAsync(Guid appointmentId)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Appointment not found",
                    StatusCode = 404
                };

            if (appointment.AppointmentStatusId != await _appointmentRepository.GetStatusIdByNameAsync("InMeeting")) // In Progress status Guid
                return new JsonModel
                {
                    data = new object(),
                    Message = "Appointment is not in progress",
                    StatusCode = 400
                };

            // Update appointment status to ended
            appointment.AppointmentStatusId = await _appointmentRepository.GetStatusIdByNameAsync("Ended"); // Ended status Guid
            appointment.EndedAt = DateTime.UtcNow;
            appointment.UpdatedDate = DateTime.UtcNow;

            await _appointmentRepository.UpdateAsync(appointment);

            // Send notification to participants
            // await _notificationService.SendMeetingEndedNotificationAsync(appointmentId);

            return new JsonModel
            {
                data = MapToDto(appointment),
                Message = "Meeting ended successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to end meeting: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> CompleteAppointmentAsync(Guid appointmentId, SmartTelehealth.Application.DTOs.CompleteAppointmentDto completeDto)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Appointment not found",
                    StatusCode = 404
                };

            if (appointment.AppointmentStatusId != await _appointmentRepository.GetStatusIdByNameAsync("InMeeting") && appointment.AppointmentStatusId != await _appointmentRepository.GetStatusIdByNameAsync("Ended")) // In Progress or Ended status Guid
                return new JsonModel
                {
                    data = new object(),
                    Message = "Appointment must be in progress or ended to complete",
                    StatusCode = 400
                };

            // Update appointment status to completed
            appointment.AppointmentStatusId = await _appointmentRepository.GetStatusIdByNameAsync("Completed"); // Completed status Guid
            appointment.CompletedAt = DateTime.UtcNow;
            appointment.UpdatedDate = DateTime.UtcNow;
            appointment.ProviderNotes = completeDto.ProviderNotes;
            appointment.Diagnosis = completeDto.Diagnosis;
            appointment.Prescription = completeDto.Prescription;
            appointment.FollowUpInstructions = completeDto.FollowUpInstructions;

            await _appointmentRepository.UpdateAsync(appointment);

            // Send notification to patient
            // await _notificationService.SendAppointmentCompletedNotificationAsync(appointment.PatientId, appointmentId);

            return new JsonModel
            {
                data = MapToDto(appointment),
                Message = "Appointment completed successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to complete appointment: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> CancelAppointmentAsync(Guid appointmentId, string reason)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Appointment not found",
                    StatusCode = 404
                };

            if (appointment.AppointmentStatusId == await _appointmentRepository.GetStatusIdByNameAsync("Completed")) // Completed status Guid
                return new JsonModel
                {
                    data = new object(),
                    Message = "Cannot cancel completed appointment",
                    StatusCode = 400
                };

            // Update appointment status to cancelled
            appointment.AppointmentStatusId = await _appointmentRepository.GetStatusIdByNameAsync("Cancelled"); // Cancelled status Guid
            appointment.CancelledAt = DateTime.UtcNow;
            appointment.UpdatedDate = DateTime.UtcNow;
            appointment.CancellationReason = reason;

            await _appointmentRepository.UpdateAsync(appointment);

            // Send notification to participants
            // await _notificationService.SendAppointmentCancelledNotificationAsync(appointmentId, reason);

            return new JsonModel
            {
                data = MapToDto(appointment),
                Message = "Appointment cancelled successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to cancel appointment: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GenerateMeetingLinkAsync(Guid appointmentId)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Appointment not found",
                    StatusCode = 404
                };

            // Create or get video session
            var sessionId = await GetOrCreateVideoSessionAsync(appointmentId);
            
            // Generate meeting URL
            var meetingUrl = $"/meeting/{appointmentId}?session={sessionId}";
            
            return new JsonModel
            {
                data = meetingUrl,
                Message = "Meeting link generated successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to generate meeting link: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetOpenTokTokenAsync(Guid appointmentId, int userId)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "Appointment not found",
                    StatusCode = 404
                };

            // Get user details
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return new JsonModel
                {
                    data = new object(),
                    Message = "User not found",
                    StatusCode = 404
                };

            // Generate token for the user
            var participantRole = await _participantRoleRepository.GetByIdAsync(Guid.Parse("00000000-0000-0000-0000-000000000001")); // Default role
            var openTokRole = MapParticipantRoleNameToOpenTokRole(participantRole.Name);
            var token = await GenerateVideoTokenAsync(appointmentId, userId, user.Email, participantRole.Id);
            
            return new JsonModel
            {
                data = token,
                Message = "OpenTok token generated successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to get OpenTok token: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public Task<JsonModel> StartRecordingAsync(Guid appointmentId)
    {
        // TODO: Implement start recording
        throw new NotImplementedException();
    }

    public Task<JsonModel> StopRecordingAsync(Guid appointmentId)
    {
        // TODO: Implement stop recording
        throw new NotImplementedException();
    }

    public Task<JsonModel> GetRecordingUrlAsync(Guid appointmentId)
    {
        // TODO: Implement get recording URL
        throw new NotImplementedException();
    }

    public Task<JsonModel> CapturePaymentAsync(Guid appointmentId)
    {
        // TODO: Implement capture payment
        throw new NotImplementedException();
    }

    public Task<JsonModel> RefundPaymentAsync(Guid appointmentId, decimal? amount = null)
    {
        // TODO: Implement refund payment
        throw new NotImplementedException();
    }

    public Task<JsonModel> GetPaymentStatusAsync(Guid appointmentId)
    {
        // TODO: Implement get payment status
        throw new NotImplementedException();
    }

    public Task<JsonModel> ScheduleReminderAsync(Guid appointmentId, ScheduleReminderDto reminderDto)
    {
        // TODO: Implement schedule reminder
        throw new NotImplementedException();
    }

    public Task<JsonModel> GetAppointmentRemindersAsync(Guid appointmentId)
    {
        // TODO: Implement get appointment reminders
        throw new NotImplementedException();
    }

    public Task<JsonModel> SendReminderAsync(Guid reminderId)
    {
        // TODO: Implement send reminder
        throw new NotImplementedException();
    }

    public Task<JsonModel> LogAppointmentEventAsync(Guid appointmentId, LogAppointmentEventDto eventDto)
    {
        // TODO: Implement log appointment event
        throw new NotImplementedException();
    }

    public Task<JsonModel> GetAppointmentEventsAsync(Guid appointmentId)
    {
        // TODO: Implement get appointment events
        throw new NotImplementedException();
    }

    public async Task<JsonModel> GetProviderAvailabilityAsync(Guid providerId, DateTime date)
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
            
            return new JsonModel
            {
                data = availability,
                Message = "Provider availability retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to get provider availability: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public Task<JsonModel> CheckProviderAvailabilityAsync(Guid providerId, DateTime startTime, DateTime endTime)
    {
        // TODO: Implement check provider availability
        throw new NotImplementedException();
    }

    public Task<JsonModel> ValidateSubscriptionAccessAsync(Guid patientId, Guid categoryId)
    {
        // TODO: Implement validate subscription access
        throw new NotImplementedException();
    }

    public Task<JsonModel> CalculateAppointmentFeeAsync(int patientId, int providerId, Guid categoryId)
    {
        // TODO: Implement calculate appointment fee
        throw new NotImplementedException();
    }

    public Task<JsonModel> ApplySubscriptionDiscountAsync(Guid appointmentId)
    {
        // TODO: Implement apply subscription discount
        throw new NotImplementedException();
    }

    public Task<JsonModel> ProcessExpiredAppointmentsAsync()
    {
        // TODO: Implement process expired appointments
        throw new NotImplementedException();
    }

    public Task<JsonModel> AutoCancelAppointmentAsync(Guid appointmentId)
    {
        // TODO: Implement auto cancel appointment
        throw new NotImplementedException();
    }

    public Task<JsonModel> GetAppointmentAnalyticsAsync(DateTime startDate, DateTime endDate)
    {
        // TODO: Implement get appointment analytics
        throw new NotImplementedException();
    }

    public async Task<JsonModel> GetAppointmentsByStatusAsync(Guid appointmentStatusId)
    {
        try
        {
            var appointments = await _appointmentRepository.GetByStatusAsync(appointmentStatusId);
            var appointmentDtos = appointments.Select(MapToDto);
            return new JsonModel
            {
                data = appointmentDtos,
                Message = "Appointments by status retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to get appointments by status: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetUpcomingAppointmentsAsync()
    {
        try
        {
            var appointments = await _appointmentRepository.GetUpcomingAsync();
            var appointmentDtos = appointments.Select(MapToDto);
            return new JsonModel
            {
                data = appointmentDtos,
                Message = "Upcoming appointments retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to get upcoming appointments: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetCategoriesWithSubscriptionsAsync()
    {
        try
        {
            // TODO: Implement actual logic
            return new JsonModel
            {
                data = new List<CategoryWithSubscriptionsDto>(),
                Message = "Categories with subscriptions retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to get categories with subscriptions: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetFeaturedProvidersAsync()
    {
        try
        {
            // TODO: Implement actual logic
            return new JsonModel
            {
                data = new List<FeaturedProviderDto>(),
                Message = "Featured providers retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            return new JsonModel
            {
                data = new object(),
                Message = $"Failed to get featured providers: {ex.Message}",
                StatusCode = 500
            };
        }
    }

    public Task<JsonModel> IsAppointmentServiceHealthyAsync()
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

    private dynamic GetAppointmentDocumentType(object documentTypesData)
    {
        var docTypes = documentTypesData as IEnumerable<object>;
        if (docTypes != null)
        {
            foreach (var dt in docTypes)
            {
                var dynamicDt = dt as dynamic;
                if (dynamicDt != null && 
                    (dynamicDt.Name?.ToLower().Contains("appointment") == true || 
                     dynamicDt.Name?.ToLower().Contains("medical") == true ||
                     dynamicDt.Name?.ToLower().Contains("report") == true))
                {
                    return dynamicDt;
                }
            }
        }
        return null;
    }
} 