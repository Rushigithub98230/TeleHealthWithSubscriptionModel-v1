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
    private readonly IProviderPayoutService _payoutService;
    private readonly IPayoutPeriodService _periodService;
    private readonly ILogger<ProviderPayoutController> _logger;

    public ProviderPayoutController(
        IProviderPayoutService payoutService,
        IPayoutPeriodService periodService,
        ILogger<ProviderPayoutController> logger)
    {
        _payoutService = payoutService;
        _periodService = periodService;
        _logger = logger;
    }

    /// <summary>
    /// Get payout by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<JsonModel> GetPayout(Guid id)
    {
        var result = await _payoutService.GetPayoutAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Process payout (Admin only)
    /// </summary>
    [HttpPost("{id}/process")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<JsonModel> ProcessPayout(Guid id, [FromBody] ProcessPayoutDto processDto)
    {
        var result = await _payoutService.ProcessPayoutAsync(id, processDto);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get payouts by provider
    /// </summary>
    [HttpGet("provider/{providerId}")]
    public async Task<ActionResult<JsonModel>> GetPayoutsByProvider(int providerId)
    {
        var result = await _payoutService.GetPayoutsByProviderAsync(providerId);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get payouts by period
    /// </summary>
    [HttpGet("period/{periodId}")]
    public async Task<ActionResult<JsonModel>> GetPayoutsByPeriod(Guid periodId)
    {
        var result = await _payoutService.GetPayoutsByPeriodAsync(periodId);
        return StatusCode(result.StatusCode, result);
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
        var result = await _payoutService.GetAllPayoutsAsync(status, page, pageSize);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get pending payouts
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<JsonModel>> GetPendingPayouts()
    {
        var result = await _payoutService.GetPendingPayoutsAsync();
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get payouts by status
    /// </summary>
    [HttpGet("status/{status}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<JsonModel>> GetPayoutsByStatus(string status)
    {
        var result = await _payoutService.GetPayoutsByStatusAsync(status);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get provider earnings
    /// </summary>
    [HttpGet("earnings/{providerId}")]
    public async Task<ActionResult<JsonModel> GetProviderEarnings(int providerId)
    {
        var result = await _payoutService.GetProviderEarningsAsync(providerId);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get payout statistics
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<JsonModel> GetPayoutStatistics()
    {
        var result = await _payoutService.GetPayoutStatisticsAsync();
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Generate payouts for period
    /// </summary>
    [HttpPost("period/{periodId}/generate")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<JsonModel> GeneratePayoutsForPeriod(Guid periodId)
    {
        var result = await _payoutService.GeneratePayoutsForPeriodAsync(periodId);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Process all pending payouts
    /// </summary>
    [HttpPost("process-all-pending")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<JsonModel> ProcessAllPendingPayouts()
    {
        var result = await _payoutService.ProcessAllPendingPayoutsAsync();
        return StatusCode(result.StatusCode, result);
    }

    // Payout Period endpoints

    /// <summary>
    /// Create a new payout period
    /// </summary>
    [HttpPost("periods")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<JsonModel> CreatePeriod([FromBody] CreatePayoutPeriodDto createDto)
    {
        var result = await _periodService.CreatePeriodAsync(createDto);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get period by ID
    /// </summary>
    [HttpGet("periods/{id}")]
    public async Task<ActionResult<JsonModel> GetPeriod(Guid id)
    {
        var result = await _periodService.GetPeriodAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Update period
    /// </summary>
    [HttpPut("periods/{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<JsonModel> UpdatePeriod(Guid id, [FromBody] CreatePayoutPeriodDto updateDto)
    {
        var result = await _periodService.UpdatePeriodAsync(id, updateDto);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get all periods
    /// </summary>
    [HttpGet("periods")]
    public async Task<ActionResult<JsonModel>> GetAllPeriods()
    {
        var result = await _periodService.GetAllPeriodsAsync();
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get active periods
    /// </summary>
    [HttpGet("periods/active")]
    public async Task<ActionResult<JsonModel>> GetActivePeriods()
    {
        var result = await _periodService.GetActivePeriodsAsync();
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Delete period
    /// </summary>
    [HttpDelete("periods/{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<JsonModel> DeletePeriod(Guid id)
    {
        var result = await _periodService.DeletePeriodAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Process period
    /// </summary>
    [HttpPost("periods/{id}/process")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<JsonModel> ProcessPeriod(Guid id)
    {
        var result = await _periodService.ProcessPeriodAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get period statistics
    /// </summary>
    [HttpGet("periods/statistics")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<JsonModel> GetPeriodStatistics()
    {
        var result = await _periodService.GetPeriodStatisticsAsync();
        return StatusCode(result.StatusCode, result);
    }
} 