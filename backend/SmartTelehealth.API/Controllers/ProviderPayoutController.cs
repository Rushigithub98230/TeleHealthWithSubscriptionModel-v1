using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProviderPayoutController : ControllerBase
{
    private readonly IProviderPayoutService _providerPayoutService;
    private readonly IPayoutPeriodService _periodService;
    private readonly ILogger<ProviderPayoutController> _logger;

    public ProviderPayoutController(
        IProviderPayoutService providerPayoutService,
        IPayoutPeriodService periodService,
        ILogger<ProviderPayoutController> logger)
    {
        _providerPayoutService = providerPayoutService;
        _periodService = periodService;
        _logger = logger;
    }

    /// <summary>
    /// Get payout by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<JsonModel>> GetPayout(Guid id)
    {
        var response = await _providerPayoutService.GetPayoutAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Process payout (Admin only)
    /// </summary>
    [HttpPost("{id}/process")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<JsonModel>> ProcessPayout(Guid id, [FromBody] ProcessPayoutDto processDto)
    {
        var response = await _providerPayoutService.ProcessPayoutAsync(id, processDto);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get payouts by provider
    /// </summary>
    [HttpGet("provider/{providerId}")]
    public async Task<ActionResult<JsonModel>> GetPayoutsByProvider(int providerId)
    {
        var response = await _providerPayoutService.GetPayoutsByProviderAsync(providerId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get payouts by period
    /// </summary>
    [HttpGet("period/{periodId}")]
    public async Task<ActionResult<JsonModel>> GetPayoutsByPeriod(Guid periodId)
    {
        var response = await _providerPayoutService.GetPayoutsByPeriodAsync(periodId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get all payouts with optional filtering
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<JsonModel>> GetAllPayouts(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var response = await _providerPayoutService.GetAllPayoutsAsync(status, page, pageSize);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get pending payouts
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<JsonModel>> GetPendingPayouts()
    {
        var response = await _providerPayoutService.GetPendingPayoutsAsync();
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get payouts by status
    /// </summary>
    [HttpGet("status/{status}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<JsonModel>> GetPayoutsByStatus(string status)
    {
        var response = await _providerPayoutService.GetPayoutsByStatusAsync(status);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get provider earnings
    /// </summary>
    [HttpGet("provider/{providerId}/earnings")]
    public async Task<ActionResult<JsonModel>> GetProviderEarnings(int providerId)
    {
        var response = await _providerPayoutService.GetProviderEarningsAsync(providerId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get payout statistics
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<JsonModel>> GetPayoutStatistics()
    {
        var response = await _providerPayoutService.GetPayoutStatisticsAsync();
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Generate payouts for period
    /// </summary>
    [HttpPost("period/{periodId}/generate")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<JsonModel>> GeneratePayoutsForPeriod(Guid periodId)
    {
        var response = await _providerPayoutService.GeneratePayoutsForPeriodAsync(periodId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Process all pending payouts
    /// </summary>
    [HttpPost("process-all-pending")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<JsonModel>> ProcessAllPendingPayouts()
    {
        var response = await _providerPayoutService.ProcessAllPendingPayoutsAsync();
        return StatusCode(response.StatusCode, response);
    }

    // Payout Period endpoints

    /// <summary>
    /// Create a new payout period
    /// </summary>
    [HttpPost("periods")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<JsonModel>> CreatePeriod([FromBody] CreatePayoutPeriodDto createDto)
    {
        var response = await _periodService.CreatePeriodAsync(createDto);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get period by ID
    /// </summary>
    [HttpGet("periods/{id}")]
    public async Task<ActionResult<JsonModel>> GetPeriod(Guid id)
    {
        var response = await _periodService.GetPeriodAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Update period
    /// </summary>
    [HttpPut("periods/{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<JsonModel>> UpdatePeriod(Guid id, [FromBody] CreatePayoutPeriodDto updateDto)
    {
        var response = await _periodService.UpdatePeriodAsync(id, updateDto);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get all periods
    /// </summary>
    [HttpGet("periods")]
    public async Task<ActionResult<JsonModel>> GetAllPeriods()
    {
        var response = await _periodService.GetAllPeriodsAsync();
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get active periods
    /// </summary>
    [HttpGet("periods/active")]
    public async Task<ActionResult<JsonModel>> GetActivePeriods()
    {
        var response = await _periodService.GetActivePeriodsAsync();
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Delete period
    /// </summary>
    [HttpDelete("periods/{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<JsonModel>> DeletePeriod(Guid id)
    {
        var response = await _periodService.DeletePeriodAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Process period
    /// </summary>
    [HttpPost("periods/{id}/process")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<JsonModel>> ProcessPeriod(Guid id)
    {
        var response = await _periodService.ProcessPeriodAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get period statistics
    /// </summary>
    [HttpGet("periods/{id}/statistics")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<JsonModel>> GetPeriodStatistics()
    {
        var response = await _periodService.GetPeriodStatisticsAsync();
        return StatusCode(response.StatusCode, response);
    }
} 