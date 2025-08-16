using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly IOpenTokService _openTokService;
    private readonly ILogger<AppointmentsController> _logger;

    public AppointmentsController(
        IAppointmentService appointmentService,
        IOpenTokService openTokService,
        ILogger<AppointmentsController> logger)
    {
        _appointmentService = appointmentService;
        _openTokService = openTokService;
        _logger = logger;
    }

    // Homepage endpoints
    [HttpGet("homepage")]
    [AllowAnonymous]
    public async Task<ActionResult<JsonModel>> GetHomepageData()
    {
        try
        {
            // Get categories with subscription plans
            var categoriesResponse = await _appointmentService.GetCategoriesWithSubscriptionsAsync();
            if (categoriesResponse.StatusCode != 200)
                return StatusCode(categoriesResponse.StatusCode, categoriesResponse);

            // Get featured providers
            var providersResponse = await _appointmentService.GetFeaturedProvidersAsync();
            if (providersResponse.StatusCode != 200)
                return StatusCode(providersResponse.StatusCode, providersResponse);

            var homepageData = new HomepageDto
            {
                Categories = (categoriesResponse.data as IEnumerable<CategoryWithSubscriptionsDto>)?.ToList() ?? new List<CategoryWithSubscriptionsDto>(),
                FeaturedProviders = (providersResponse.data as IEnumerable<FeaturedProviderDto>)?.ToList() ?? new List<FeaturedProviderDto>(),
                TotalAppointments = 0, // Will be populated from analytics
                TotalPatients = 0, // Will be populated from analytics
                TotalProviders = 0 // Will be populated from analytics
            };

            return Ok(new JsonModel { data = homepageData, Message = "Homepage data retrieved successfully", StatusCode = 200 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting homepage data");
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to load homepage data", StatusCode = 500 });
        }
    }

    [HttpGet("categories")]
    public async Task<ActionResult<JsonModel>> GetCategories()
    {
        try
        {
            var categories = await _appointmentService.GetCategoriesWithSubscriptionsAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return BadRequest(new JsonModel
            {
                data = new object(),
                Message = ex.Message,
                StatusCode = 400
            });
        }
    }

    [HttpGet("providers/featured")]
    public async Task<ActionResult<JsonModel>> GetFeaturedProviders()
    {
        try
        {
            var providers = await _appointmentService.GetFeaturedProvidersAsync();
            return Ok(providers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting featured providers");
            return BadRequest(new JsonModel
            {
                data = new object(),
                Message = ex.Message,
                StatusCode = 400
            });
        }
    }

    [HttpGet("home-data")]
    public async Task<ActionResult<JsonModel>> GetHomeData()
    {
        try
        {
            var categories = await _appointmentService.GetCategoriesWithSubscriptionsAsync();
            var providers = await _appointmentService.GetFeaturedProvidersAsync();
            
            return Ok(new JsonModel
            {
                data = new
                {
                    Categories = categories.data,
                    Providers = providers.data
                },
                Message = "Home data retrieved successfully",
                StatusCode = 200
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting home data");
            return BadRequest(new JsonModel
            {
                data = new object(),
                Message = ex.Message,
                StatusCode = 400
            });
        }
    }

    // Appointment booking flow
    [HttpPost("book")]
    public async Task<ActionResult<JsonModel>> BookAppointment([FromBody] AppointmentBookingDto bookingDto)
    {
        try
        {
            // Validate subscription access if subscription is provided
            if (!string.IsNullOrEmpty(bookingDto.SubscriptionId))
            {
                Guid patientGuid = Guid.TryParse(bookingDto.PatientId, out var pg) ? pg : Guid.Empty;
                Guid categoryGuid = Guid.TryParse(bookingDto.CategoryId, out var cg) ? cg : Guid.Empty;
                var subscriptionValidation = await _appointmentService.ValidateSubscriptionAccessAsync(
                    patientGuid, categoryGuid);
                if (subscriptionValidation.StatusCode != 200)
                {
                    return BadRequest(new JsonModel { data = new object(), Message = "Invalid subscription access", StatusCode = 400 });
                }
            }

            // Calculate appointment fee
            if (!int.TryParse(bookingDto.PatientId, out int patientId))
            {
                return BadRequest(new JsonModel { data = new object(), Message = "Invalid patient ID format", StatusCode = 400 });
            }
            if (!int.TryParse(bookingDto.ProviderId, out int providerId))
            {
                return BadRequest(new JsonModel { data = new object(), Message = "Invalid provider ID format", StatusCode = 400 });
            }
            Guid categoryGuid2 = Guid.TryParse(bookingDto.CategoryId, out var cg2) ? cg2 : Guid.Empty;
            var feeCalculation = await _appointmentService.CalculateAppointmentFeeAsync(
                patientId, providerId, categoryGuid2);
            
            if (feeCalculation.StatusCode != 200)
            {
                return BadRequest(new JsonModel { data = new object(), Message = "Failed to calculate appointment fee", StatusCode = 400 });
            }

            // Create appointment booking DTO
            var bookDto = new BookAppointmentDto
            {
                PatientId = patientId.ToString(),
                ProviderId = providerId.ToString(),
                CategoryId = categoryGuid2.ToString(),
                SubscriptionId = bookingDto.SubscriptionId,
                AppointmentTypeId = bookingDto.Type != null ? Guid.Parse(bookingDto.Type) : Guid.Empty,
                ConsultationModeId = bookingDto.Mode != null ? Guid.Parse(bookingDto.Mode) : Guid.Empty,
                ScheduledAt = bookingDto.ScheduledAt,
                DurationMinutes = bookingDto.DurationMinutes,
                ReasonForVisit = bookingDto.ReasonForVisit,
                Symptoms = bookingDto.Symptoms,
                PatientNotes = bookingDto.PatientNotes,
                IsUrgent = bookingDto.IsUrgent
            };

            // Book the appointment
            var appointmentResponse = await _appointmentService.BookAppointmentAsync(bookDto);
            
            if (appointmentResponse.StatusCode != 200)
            {
                return BadRequest(new JsonModel { data = new object(), Message = appointmentResponse.Message, StatusCode = 400 });
            }

            var appointment = appointmentResponse.data as AppointmentDto;
            if (appointment == null)
            {
                return BadRequest(new JsonModel { data = new object(), Message = "Failed to create appointment", StatusCode = 400 });
            }

            // Create confirmation DTO
            var confirmation = new AppointmentConfirmationDto
            {
                AppointmentId = appointment.Id.ToString(),
                AppointmentNumber = $"APT-{appointment.Id.ToString().Substring(0, 8).ToUpper()}",
                PatientName = appointment.PatientName,
                ProviderName = appointment.ProviderName,
                CategoryName = appointment.CategoryName,
                ScheduledAt = appointment.ScheduledAt,
                Fee = appointment.Fee,
                Status = appointment.Status?.ToString(),
                StripePaymentIntentId = appointment.StripePaymentIntentId?.ToString(),
                IsPaymentCaptured = appointment.IsPaymentCaptured,
                CreatedAt = appointment.CreatedAt
            };

            return Ok(new JsonModel { data = confirmation, Message = "Appointment booked successfully", StatusCode = 200 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error booking appointment");
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to book appointment", StatusCode = 500 });
        }
    }

    [HttpPost("{appointmentId}/payment")]
    public async Task<ActionResult<JsonModel>> ProcessPayment(Guid appointmentId, [FromBody] ProcessPaymentDto request)
    {
        try
        {
            var appointment = await _appointmentService.ProcessPaymentAsync(appointmentId, request);
            return Ok(appointment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for appointment {AppointmentId}", appointmentId);
            return BadRequest(new JsonModel
            {
                data = new object(),
                Message = ex.Message,
                StatusCode = 400
            });
        }
    }

    // Provider actions
    [HttpPost("{appointmentId}/accept")]
    [Authorize(Roles = "Provider")]
    public async Task<ActionResult<JsonModel>> AcceptAppointment(Guid appointmentId, [FromBody] ProviderAcceptDto acceptDto)
    {
        try
        {
            var response = await _appointmentService.ProviderAcceptAppointmentAsync(appointmentId, acceptDto);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting appointment {AppointmentId}", appointmentId);
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to accept appointment", StatusCode = 500 });
        }
    }

    [HttpPost("{appointmentId}/reject")]
    [Authorize(Roles = "Provider")]
    public async Task<ActionResult<JsonModel>> RejectAppointment(Guid appointmentId, [FromBody] ProviderRejectDto rejectDto)
    {
        try
        {
            var response = await _appointmentService.ProviderRejectAppointmentAsync(appointmentId, rejectDto);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting appointment {AppointmentId}", appointmentId);
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to reject appointment", StatusCode = 500 });
        }
    }

    // Meeting management
    [HttpPost("{appointmentId}/start-meeting")]
    public async Task<ActionResult<JsonModel>> StartMeeting(Guid appointmentId)
    {
        try
        {
            var response = await _appointmentService.StartMeetingAsync(appointmentId);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting meeting for appointment {AppointmentId}", appointmentId);
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to start meeting", StatusCode = 500 });
        }
    }

    [HttpPost("{appointmentId}/end-meeting")]
    public async Task<ActionResult<JsonModel>> EndMeeting(Guid appointmentId)
    {
        try
        {
            var response = await _appointmentService.EndMeetingAsync(appointmentId);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending meeting for appointment {AppointmentId}", appointmentId);
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to end meeting", StatusCode = 500 });
        }
    }

    [HttpPost("{appointmentId}/complete")]
    [Authorize(Roles = "Provider")]
    public async Task<ActionResult<JsonModel>> CompleteAppointment(Guid appointmentId, [FromBody] CompleteAppointmentDto completeDto)
    {
        try
        {
            var response = await _appointmentService.CompleteAppointmentAsync(appointmentId, completeDto);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing appointment {AppointmentId}", appointmentId);
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to complete appointment", StatusCode = 500 });
        }
    }

    // Video call integration
    [HttpGet("{appointmentId}/meeting-link")]
    public async Task<ActionResult<JsonModel>> GetMeetingLink(Guid appointmentId)
    {
        try
        {
            var response = await _appointmentService.GenerateMeetingLinkAsync(appointmentId);
            if (response.StatusCode == 200)
            {
                return Ok(new JsonModel { data = new { meetingUrl = response.data }, Message = "Meeting link generated successfully", StatusCode = 200 });
            }
            return BadRequest(new JsonModel { data = new object(), Message = response.Message, StatusCode = 400 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating meeting link for appointment {AppointmentId}", appointmentId);
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to generate meeting link", StatusCode = 500 });
        }
    }

    [HttpGet("{appointmentId}/opentok-token")]
    public async Task<ActionResult<JsonModel>> GetOpenTokToken(Guid appointmentId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _appointmentService.GetOpenTokTokenAsync(appointmentId, userId);
            if (response.StatusCode == 200)
            {
                return Ok(new JsonModel { data = new { token = response.data }, Message = "OpenTok token generated successfully", StatusCode = 200 });
            }
            return BadRequest(new JsonModel { data = new object(), Message = response.Message, StatusCode = 400 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting OpenTok token for appointment {AppointmentId}", appointmentId);
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to get OpenTok token", StatusCode = 500 });
        }
    }

    // CRUD operations
    [HttpGet]
    public async Task<ActionResult<JsonModel>> GetUserAppointments()
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _appointmentService.GetPatientAppointmentsAsync(userId);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user appointments");
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to get appointments", StatusCode = 500 });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<JsonModel>> GetAppointment(Guid id)
    {
        try
        {
            var response = await _appointmentService.GetAppointmentByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting appointment {Id}", id);
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to get appointment", StatusCode = 500 });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<JsonModel>> UpdateAppointment(Guid id, [FromBody] UpdateAppointmentDto updateDto)
    {
        try
        {
            var response = await _appointmentService.UpdateAppointmentAsync(id, updateDto);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating appointment {Id}", id);
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to update appointment", StatusCode = 500 });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<JsonModel>> CancelAppointment(Guid id, [FromBody] string reason)
    {
        try
        {
            var response = await _appointmentService.CancelAppointmentAsync(id, reason);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling appointment {Id}", id);
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to cancel appointment", StatusCode = 500 });
        }
    }

    // Provider availability
    [HttpGet("providers/{providerId}/availability")]
    [AllowAnonymous]
    public async Task<ActionResult<JsonModel>> GetProviderAvailability(Guid providerId, [FromQuery] DateTime date)
    {
        try
        {
            var response = await _appointmentService.GetProviderAvailabilityAsync(providerId, date);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting provider availability for provider {ProviderId}", providerId);
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to get provider availability", StatusCode = 500 });
        }
    }

    // Analytics
    [HttpGet("analytics")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<JsonModel>> GetAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var response = await _appointmentService.GetAppointmentAnalyticsAsync(startDate, endDate);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting appointment analytics");
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to get analytics", StatusCode = 500 });
        }
    }

    // --- PARTICIPANT MANAGEMENT ---
    [HttpPost("{appointmentId}/participants")]
    public async Task<ActionResult<JsonModel>> AddParticipant(Guid appointmentId, [FromBody] AddParticipantDto request)
    {
        try
        {
            int? userId = null;
            if (!string.IsNullOrEmpty(request.UserId))
            {
                if (!int.TryParse(request.UserId, out int parsedUserId))
                {
                    return BadRequest(new JsonModel { data = new object(), Message = "Invalid user ID format", StatusCode = 400 });
                }
                userId = parsedUserId;
            }
            Guid participantRoleId = Guid.Empty; // Placeholder, will be replaced with actual role ID
            if (!string.IsNullOrEmpty(request.Role))
                participantRoleId = Guid.TryParse(request.Role, out var parsedRoleId) ? parsedRoleId : Guid.Empty;
            int invitedByUserId = 0;
            if (!string.IsNullOrEmpty(request.InvitedByUserId))
            {
                if (!int.TryParse(request.InvitedByUserId, out int parsedInvitedBy))
                {
                    return BadRequest(new JsonModel { data = new object(), Message = "Invalid invited by user ID format", StatusCode = 400 });
                }
                invitedByUserId = parsedInvitedBy;
            }
            var result = await _appointmentService.AddParticipantAsync(
                appointmentId,
                userId,
                string.IsNullOrEmpty(request.Email) ? null : request.Email,
                string.IsNullOrEmpty(request.Phone) ? null : request.Phone,
                participantRoleId,
                invitedByUserId
            );
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding participant to appointment {AppointmentId}", appointmentId);
            return BadRequest(new JsonModel
            {
                data = new object(),
                Message = ex.Message,
                StatusCode = 400
            });
        }
    }

    [HttpPost("{appointmentId}/invite-external")]
    public async Task<ActionResult<JsonModel>> InviteExternal(Guid appointmentId, [FromBody] InviteExternalDto request)
    {
        try
        {
            int invitedByUserId = 0;
            if (!string.IsNullOrEmpty(request.InvitedByUserId))
            {
                if (!int.TryParse(request.InvitedByUserId, out int parsedInvitedBy))
                {
                    return BadRequest(new JsonModel { data = new object(), Message = "Invalid invited by user ID format", StatusCode = 400 });
                }
                invitedByUserId = parsedInvitedBy;
            }
            var response = await _appointmentService.InviteExternalAsync(appointmentId, request.Email, request.Phone, request.Message, invitedByUserId);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inviting external participant to appointment {AppointmentId}", appointmentId);
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to invite external participant", StatusCode = 500 });
        }
    }

    [HttpPost("{appointmentId}/join")]
    public async Task<ActionResult<JsonModel>> JoinAppointment(Guid appointmentId, [FromBody] JoinAppointmentDto request)
    {
        try
        {
            int? userId = null;
            if (!string.IsNullOrEmpty(request.UserId))
            {
                if (!int.TryParse(request.UserId, out int parsedUserId))
                {
                    return BadRequest(new JsonModel { data = new object(), Message = "Invalid user ID format", StatusCode = 400 });
                }
                userId = parsedUserId;
            }
            var response = await _appointmentService.MarkParticipantJoinedAsync(appointmentId, userId, request.Email);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining appointment {AppointmentId}", appointmentId);
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to join appointment", StatusCode = 500 });
        }
    }

    [HttpPost("{appointmentId}/leave")]
    public async Task<ActionResult<JsonModel>> LeaveAppointment(Guid appointmentId, [FromBody] LeaveAppointmentDto request)
    {
        try
        {
            int? userId = null;
            if (!string.IsNullOrEmpty(request.UserId))
            {
                if (!int.TryParse(request.UserId, out int parsedUserId))
                {
                    return BadRequest(new JsonModel { data = new object(), Message = "Invalid user ID format", StatusCode = 400 });
                }
                userId = parsedUserId;
            }
            var response = await _appointmentService.MarkParticipantLeftAsync(appointmentId, userId, request.Email);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving appointment {AppointmentId}", appointmentId);
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to leave appointment", StatusCode = 500 });
        }
    }

    [HttpGet("{appointmentId}/participants")]
    public async Task<ActionResult<JsonModel>> GetParticipants(Guid appointmentId)
    {
        try
        {
            var participants = await _appointmentService.GetParticipantsAsync(appointmentId);
            return Ok(participants);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting participants for appointment {AppointmentId}", appointmentId);
            return BadRequest(new JsonModel
            {
                data = new object(),
                Message = ex.Message,
                StatusCode = 400
            });
        }
    }

            [HttpGet("{appointmentId}/video-token")]
        public async Task<ActionResult<JsonModel>> GetVideoToken(Guid appointmentId, [FromQuery] string? userId, [FromQuery] string? email, [FromQuery] Guid? role = null)
        {
            try
            {
                int? userIdInt = null;
                if (!string.IsNullOrEmpty(userId))
                {
                    if (!int.TryParse(userId, out int parsedUserId))
                    {
                        return BadRequest(new JsonModel { data = new object(), Message = "Invalid user ID format", StatusCode = 400 });
                    }
                    userIdInt = parsedUserId;
                }
                var response = await _appointmentService.GenerateVideoTokenAsync(appointmentId, userIdInt, email, role ?? Guid.Empty);
                return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating video token for appointment {AppointmentId}", appointmentId);
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to generate video token", StatusCode = 500 });
        }
    }

    // --- PAYMENT MANAGEMENT ---
    [HttpPost("{appointmentId}/confirm-payment")]
    public async Task<ActionResult<JsonModel>> ConfirmPayment(Guid appointmentId, [FromBody] ConfirmPaymentDto request)
    {
        try
        {
            var appointment = await _appointmentService.ConfirmPaymentAsync(appointmentId, request.PaymentIntentId);
            return Ok(appointment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming payment for appointment {AppointmentId}", appointmentId);
            return BadRequest(new JsonModel
            {
                data = new object(),
                Message = ex.Message,
                StatusCode = 400
            });
        }
    }

    [HttpPost("{appointmentId}/refund")]
    public async Task<ActionResult<JsonModel>> ProcessRefund(Guid appointmentId, [FromBody] ProcessRefundDto request)
    {
        try
        {
            var response = await _appointmentService.ProcessRefundAsync(appointmentId, request.RefundAmount, request.Reason);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for appointment {AppointmentId}", appointmentId);
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to process refund", StatusCode = 500 });
        }
    }

    [HttpGet("{appointmentId}/payment-logs")]
    public async Task<ActionResult<JsonModel>> GetPaymentLogs(Guid appointmentId)
    {
        try
        {
            var paymentLogs = await _appointmentService.GetPaymentLogsAsync(appointmentId);
            return Ok(paymentLogs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment logs for appointment {AppointmentId}", appointmentId);
            return BadRequest(new JsonModel
            {
                data = new object(),
                Message = ex.Message,
                StatusCode = 400
            });
        }
    }

    // --- PROVIDER ACTIONS ---
    [HttpPost("{appointmentId}/provider-action")]
    public async Task<ActionResult<JsonModel>> ProviderAction(Guid appointmentId, [FromBody] ProviderActionDto request)
    {
        try
        {
            var appointment = await _appointmentService.ProviderActionAsync(appointmentId, request.Action, request.Notes);
            return Ok(appointment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing provider action for appointment {AppointmentId}", appointmentId);
            return BadRequest(new JsonModel
            {
                data = new object(),
                Message = ex.Message,
                StatusCode = 400
            });
        }
    }

    // Health check
    [HttpGet("health")]
    public async Task<ActionResult<JsonModel>> HealthCheck()
    {
        try
        {
            var response = await _appointmentService.IsAppointmentServiceHealthyAsync();
            if (response.StatusCode == 200)
            {
                return Ok(new JsonModel { data = new { status = "healthy" }, Message = "Appointment service is operational", StatusCode = 200 });
            }
            return StatusCode(503, new JsonModel { data = new { status = "unhealthy" }, Message = "Appointment service is not operational", StatusCode = 503 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking appointment service health");
            return StatusCode(503, new JsonModel { data = new { status = "error" }, Message = "Failed to check service health", StatusCode = 503 });
        }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
} 

public class HomepageDto
{
    public List<CategoryWithSubscriptionsDto> Categories { get; set; } = new();
    public List<FeaturedProviderDto> FeaturedProviders { get; set; } = new();
    public int TotalAppointments { get; set; }
    public int TotalPatients { get; set; }
    public int TotalProviders { get; set; }
} 