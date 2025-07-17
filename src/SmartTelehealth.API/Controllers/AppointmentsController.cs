using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
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
    public async Task<ActionResult<HomepageDto>> GetHomepageData()
    {
        try
        {
            // Get categories with subscription plans
            var categoriesResponse = await _appointmentService.GetCategoriesWithSubscriptionsAsync();
            if (!categoriesResponse.Success)
                return StatusCode(500, categoriesResponse);

            // Get featured providers
            var providersResponse = await _appointmentService.GetFeaturedProvidersAsync();
            if (!providersResponse.Success)
                return StatusCode(500, providersResponse);

            var homepageData = new HomepageDto
            {
                Categories = categoriesResponse.Data?.ToList() ?? new(),
                FeaturedProviders = providersResponse.Data?.ToList() ?? new(),
                TotalAppointments = 0, // Will be populated from analytics
                TotalPatients = 0, // Will be populated from analytics
                TotalProviders = 0 // Will be populated from analytics
            };

            return Ok(homepageData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting homepage data");
            return StatusCode(500, new { error = "Failed to load homepage data" });
        }
    }

    [HttpGet("categories")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryWithSubscriptionsDto>>>> GetCategories()
    {
        try
        {
            var categories = await _appointmentService.GetCategoriesWithSubscriptionsAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return BadRequest(ApiResponse<IEnumerable<CategoryWithSubscriptionsDto>>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("providers/featured")]
    public async Task<ActionResult<ApiResponse<IEnumerable<FeaturedProviderDto>>>> GetFeaturedProviders()
    {
        try
        {
            var providers = await _appointmentService.GetFeaturedProvidersAsync();
            return Ok(providers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting featured providers");
            return BadRequest(ApiResponse<IEnumerable<FeaturedProviderDto>>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("home-data")]
    public async Task<ActionResult<ApiResponse<object>>> GetHomeData()
    {
        try
        {
            var categories = await _appointmentService.GetCategoriesWithSubscriptionsAsync();
            var providers = await _appointmentService.GetFeaturedProvidersAsync();
            
            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                Categories = categories.Data,
                Providers = providers.Data
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting home data");
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    // Appointment booking flow
    [HttpPost("book")]
    public async Task<ActionResult<AppointmentConfirmationDto>> BookAppointment([FromBody] AppointmentBookingDto bookingDto)
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
                if (!subscriptionValidation.Success)
                {
                    return BadRequest(new { error = "Invalid subscription access" });
                }
            }

            // Calculate appointment fee
            Guid patientGuid2 = Guid.TryParse(bookingDto.PatientId, out var pg2) ? pg2 : Guid.Empty;
            Guid providerGuid = Guid.TryParse(bookingDto.ProviderId, out var prg) ? prg : Guid.Empty;
            Guid categoryGuid2 = Guid.TryParse(bookingDto.CategoryId, out var cg2) ? cg2 : Guid.Empty;
            var feeCalculation = await _appointmentService.CalculateAppointmentFeeAsync(
                patientGuid2, providerGuid, categoryGuid2);
            
            if (!feeCalculation.Success)
            {
                return BadRequest(new { error = "Failed to calculate appointment fee" });
            }

            // Create appointment booking DTO
            var bookDto = new BookAppointmentDto
            {
                PatientId = patientGuid2.ToString(),
                ProviderId = providerGuid.ToString(),
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
            
            if (!appointmentResponse.Success)
            {
                return BadRequest(new { error = appointmentResponse.Message });
            }

            var appointment = appointmentResponse.Data;
            if (appointment == null)
            {
                return BadRequest(new { error = "Failed to create appointment" });
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

            return Ok(confirmation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error booking appointment");
            return StatusCode(500, new { error = "Failed to book appointment" });
        }
    }

    [HttpPost("{appointmentId}/payment")]
    public async Task<ActionResult<AppointmentDto>> ProcessPayment(Guid appointmentId, [FromBody] ProcessPaymentDto request)
    {
        try
        {
            var appointment = await _appointmentService.ProcessPaymentAsync(appointmentId, request);
            return Ok(appointment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for appointment {AppointmentId}", appointmentId);
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }

    // Provider actions
    [HttpPost("{appointmentId}/accept")]
    [Authorize(Roles = "Provider")]
    public async Task<ActionResult<AppointmentDto>> AcceptAppointment(Guid appointmentId, [FromBody] ProviderAcceptDto acceptDto)
    {
        try
        {
            var response = await _appointmentService.ProviderAcceptAppointmentAsync(appointmentId, acceptDto);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting appointment {AppointmentId}", appointmentId);
            return StatusCode(500, new { error = "Failed to accept appointment" });
        }
    }

    [HttpPost("{appointmentId}/reject")]
    [Authorize(Roles = "Provider")]
    public async Task<ActionResult<AppointmentDto>> RejectAppointment(Guid appointmentId, [FromBody] ProviderRejectDto rejectDto)
    {
        try
        {
            var response = await _appointmentService.ProviderRejectAppointmentAsync(appointmentId, rejectDto);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting appointment {AppointmentId}", appointmentId);
            return StatusCode(500, new { error = "Failed to reject appointment" });
        }
    }

    // Meeting management
    [HttpPost("{appointmentId}/start-meeting")]
    public async Task<ActionResult<AppointmentDto>> StartMeeting(Guid appointmentId)
    {
        try
        {
            var response = await _appointmentService.StartMeetingAsync(appointmentId);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting meeting for appointment {AppointmentId}", appointmentId);
            return StatusCode(500, new { error = "Failed to start meeting" });
        }
    }

    [HttpPost("{appointmentId}/end-meeting")]
    public async Task<ActionResult<AppointmentDto>> EndMeeting(Guid appointmentId)
    {
        try
        {
            var response = await _appointmentService.EndMeetingAsync(appointmentId);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending meeting for appointment {AppointmentId}", appointmentId);
            return StatusCode(500, new { error = "Failed to end meeting" });
        }
    }

    [HttpPost("{appointmentId}/complete")]
    [Authorize(Roles = "Provider")]
    public async Task<ActionResult<AppointmentDto>> CompleteAppointment(Guid appointmentId, [FromBody] CompleteAppointmentDto completeDto)
    {
        try
        {
            var response = await _appointmentService.CompleteAppointmentAsync(appointmentId, completeDto);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing appointment {AppointmentId}", appointmentId);
            return StatusCode(500, new { error = "Failed to complete appointment" });
        }
    }

    // Video call integration
    [HttpGet("{appointmentId}/meeting-link")]
    public async Task<ActionResult<string>> GetMeetingLink(Guid appointmentId)
    {
        try
        {
            var response = await _appointmentService.GenerateMeetingLinkAsync(appointmentId);
            if (response.Success)
            {
                return Ok(new { meetingUrl = response.Data });
            }
            return BadRequest(new { error = response.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating meeting link for appointment {AppointmentId}", appointmentId);
            return StatusCode(500, new { error = "Failed to generate meeting link" });
        }
    }

    [HttpGet("{appointmentId}/opentok-token")]
    public async Task<ActionResult<string>> GetOpenTokToken(Guid appointmentId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _appointmentService.GetOpenTokTokenAsync(appointmentId, userId);
            if (response.Success)
            {
                return Ok(new { token = response.Data });
            }
            return BadRequest(new { error = response.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting OpenTok token for appointment {AppointmentId}", appointmentId);
            return StatusCode(500, new { error = "Failed to get OpenTok token" });
        }
    }

    // CRUD operations
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetUserAppointments()
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
            return StatusCode(500, new { error = "Failed to get appointments" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AppointmentDto>> GetAppointment(Guid id)
    {
        try
        {
            var response = await _appointmentService.GetAppointmentByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting appointment {Id}", id);
            return StatusCode(500, new { error = "Failed to get appointment" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AppointmentDto>> UpdateAppointment(Guid id, [FromBody] UpdateAppointmentDto updateDto)
    {
        try
        {
            var response = await _appointmentService.UpdateAppointmentAsync(id, updateDto);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating appointment {Id}", id);
            return StatusCode(500, new { error = "Failed to update appointment" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> CancelAppointment(Guid id, [FromBody] string reason)
    {
        try
        {
            var response = await _appointmentService.CancelAppointmentAsync(id, reason);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling appointment {Id}", id);
            return StatusCode(500, new { error = "Failed to cancel appointment" });
        }
    }

    // Provider availability
    [HttpGet("providers/{providerId}/availability")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ProviderAvailabilityDto>>> GetProviderAvailability(Guid providerId, [FromQuery] DateTime date)
    {
        try
        {
            var response = await _appointmentService.GetProviderAvailabilityAsync(providerId, date);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting provider availability for provider {ProviderId}", providerId);
            return StatusCode(500, new { error = "Failed to get provider availability" });
        }
    }

    // Analytics
    [HttpGet("analytics")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AppointmentAnalyticsDto>> GetAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var response = await _appointmentService.GetAppointmentAnalyticsAsync(startDate, endDate);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting appointment analytics");
            return StatusCode(500, new { error = "Failed to get analytics" });
        }
    }

    // --- PARTICIPANT MANAGEMENT ---
    [HttpPost("{appointmentId}/participants")]
    public async Task<ActionResult<AppointmentParticipantDto>> AddParticipant(Guid appointmentId, [FromBody] AddParticipantDto request)
    {
        try
        {
            Guid? userId = null;
            if (!string.IsNullOrEmpty(request.UserId))
                userId = Guid.TryParse(request.UserId, out var parsedUserId) ? parsedUserId : (Guid?)null;
            Guid participantRoleId = Guid.Empty; // Placeholder, will be replaced with actual role ID
            if (!string.IsNullOrEmpty(request.Role))
                participantRoleId = Guid.TryParse(request.Role, out var parsedRoleId) ? parsedRoleId : Guid.Empty;
            Guid invitedByUserId = Guid.Empty;
            if (!string.IsNullOrEmpty(request.InvitedByUserId))
                invitedByUserId = Guid.TryParse(request.InvitedByUserId, out var parsedInvitedBy) ? parsedInvitedBy : Guid.Empty;
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
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }

    [HttpPost("{appointmentId}/invite-external")]
    public async Task<ActionResult<AppointmentInvitationDto>> InviteExternal(Guid appointmentId, [FromBody] InviteExternalDto request)
    {
        try
        {
            Guid invitedByUserId = Guid.TryParse(request.InvitedByUserId, out var parsedInvitedBy) ? parsedInvitedBy : Guid.Empty;
            var response = await _appointmentService.InviteExternalAsync(appointmentId, request.Email, request.Phone, request.Message, invitedByUserId);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inviting external participant to appointment {AppointmentId}", appointmentId);
            return StatusCode(500, new { error = "Failed to invite external participant" });
        }
    }

    [HttpPost("{appointmentId}/join")]
    public async Task<ActionResult> JoinAppointment(Guid appointmentId, [FromBody] JoinAppointmentDto request)
    {
        try
        {
            Guid? userId = null;
            if (!string.IsNullOrEmpty(request.UserId))
                userId = Guid.TryParse(request.UserId, out var parsedUserId) ? parsedUserId : (Guid?)null;
            var response = await _appointmentService.MarkParticipantJoinedAsync(appointmentId, userId, request.Email);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining appointment {AppointmentId}", appointmentId);
            return StatusCode(500, new { error = "Failed to join appointment" });
        }
    }

    [HttpPost("{appointmentId}/leave")]
    public async Task<ActionResult> LeaveAppointment(Guid appointmentId, [FromBody] LeaveAppointmentDto request)
    {
        try
        {
            Guid? userId = null;
            if (!string.IsNullOrEmpty(request.UserId))
                userId = Guid.TryParse(request.UserId, out var parsedUserId) ? parsedUserId : (Guid?)null;
            var response = await _appointmentService.MarkParticipantLeftAsync(appointmentId, userId, request.Email);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving appointment {AppointmentId}", appointmentId);
            return StatusCode(500, new { error = "Failed to leave appointment" });
        }
    }

    [HttpGet("{appointmentId}/participants")]
    public async Task<ActionResult<IEnumerable<AppointmentParticipantDto>>> GetParticipants(Guid appointmentId)
    {
        try
        {
            var participants = await _appointmentService.GetParticipantsAsync(appointmentId);
            return Ok(participants);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting participants for appointment {AppointmentId}", appointmentId);
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("{appointmentId}/video-token")]
    public async Task<ActionResult<string>> GetVideoToken(Guid appointmentId, [FromQuery] Guid? userId, [FromQuery] string? email, [FromQuery] Guid? role = null)
    {
        try
        {
            var response = await _appointmentService.GenerateVideoTokenAsync(appointmentId, userId, email, role ?? Guid.Empty);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating video token for appointment {AppointmentId}", appointmentId);
            return StatusCode(500, new { error = "Failed to generate video token" });
        }
    }

    // --- PAYMENT MANAGEMENT ---
    [HttpPost("{appointmentId}/confirm-payment")]
    public async Task<ActionResult<AppointmentDto>> ConfirmPayment(Guid appointmentId, [FromBody] ConfirmPaymentDto request)
    {
        try
        {
            var appointment = await _appointmentService.ConfirmPaymentAsync(appointmentId, request.PaymentIntentId);
            return Ok(appointment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming payment for appointment {AppointmentId}", appointmentId);
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }

    [HttpPost("{appointmentId}/refund")]
    public async Task<ActionResult<AppointmentPaymentLogDto>> ProcessRefund(Guid appointmentId, [FromBody] ProcessRefundDto request)
    {
        try
        {
            var response = await _appointmentService.ProcessRefundAsync(appointmentId, request.RefundAmount, request.Reason);
            return StatusCode(response.StatusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for appointment {AppointmentId}", appointmentId);
            return StatusCode(500, new { error = "Failed to process refund" });
        }
    }

    [HttpGet("{appointmentId}/payment-logs")]
    public async Task<ActionResult<IEnumerable<AppointmentPaymentLogDto>>> GetPaymentLogs(Guid appointmentId)
    {
        try
        {
            var paymentLogs = await _appointmentService.GetPaymentLogsAsync(appointmentId);
            return Ok(paymentLogs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment logs for appointment {AppointmentId}", appointmentId);
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }

    // --- PROVIDER ACTIONS ---
    [HttpPost("{appointmentId}/provider-action")]
    public async Task<ActionResult<AppointmentDto>> ProviderAction(Guid appointmentId, [FromBody] ProviderActionDto request)
    {
        try
        {
            var appointment = await _appointmentService.ProviderActionAsync(appointmentId, request.Action, request.Notes);
            return Ok(appointment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing provider action for appointment {AppointmentId}", appointmentId);
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }

    // Health check
    [HttpGet("health")]
    public async Task<ActionResult> HealthCheck()
    {
        try
        {
            var response = await _appointmentService.IsAppointmentServiceHealthyAsync();
            if (response.Success)
            {
                return Ok(new { status = "healthy", message = "Appointment service is operational" });
            }
            return StatusCode(503, new { status = "unhealthy", message = "Appointment service is not operational" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking appointment service health");
            return StatusCode(503, new { status = "error", message = "Failed to check service health" });
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
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