using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProviderFeeController : ControllerBase
{
    private readonly IProviderFeeService _feeService;
    private readonly ICategoryFeeRangeService _feeRangeService;
    private readonly ILogger<ProviderFeeController> _logger;

    public ProviderFeeController(
        IProviderFeeService feeService,
        ICategoryFeeRangeService feeRangeService,
        ILogger<ProviderFeeController> logger)
    {
        _feeService = feeService;
        _feeRangeService = feeRangeService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new provider fee proposal
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<JsonModel>> CreateFee([FromBody] CreateProviderFeeDto createDto)
    {
        var result = await _feeService.CreateFeeAsync(createDto);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get fee by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<JsonModel>> GetFee(Guid id)
    {
        var result = await _feeService.GetFeeAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get fee by provider and category
    /// </summary>
    [HttpGet("provider/{providerId}/category/{categoryId}")]
    public async Task<ActionResult<JsonModel>> GetFeeByProviderAndCategory(int providerId, Guid categoryId)
    {
        var response = await _feeService.GetFeeByProviderAndCategoryAsync(providerId, categoryId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Update fee proposal
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<JsonModel>> UpdateFee(Guid id, [FromBody] UpdateProviderFeeDto updateDto)
    {
        var response = await _feeService.UpdateFeeAsync(id, updateDto);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Submit fee proposal for review
    /// </summary>
    [HttpPost("{id}/propose")]
    public async Task<ActionResult<JsonModel>> ProposeFee(Guid id)
    {
        var response = await _feeService.ProposeFeeAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Review fee proposal (Admin only)
    /// </summary>
    [HttpPost("{id}/review")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<JsonModel>> ReviewFee(Guid id, [FromBody] ReviewProviderFeeDto reviewDto)
    {
        var response = await _feeService.ReviewFeeAsync(id, reviewDto);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get fees by provider
    /// </summary>
    [HttpGet("provider/{providerId}")]
    public async Task<ActionResult<JsonModel>> GetFeesByProvider(int providerId)
    {
        var result = await _feeService.GetFeesByProviderAsync(providerId);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get fees by category
    /// </summary>
    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<JsonModel>> GetFeesByCategory(Guid categoryId)
    {
        var result = await _feeService.GetFeesByCategoryAsync(categoryId);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get all fees with optional filtering
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<JsonModel>> GetAllFees(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _feeService.GetAllFeesAsync(status, page, pageSize);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get pending fees
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<JsonModel>> GetPendingFees()
    {
        var result = await _feeService.GetPendingFeesAsync();
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get fees by status
    /// </summary>
    [HttpGet("status/{status}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<JsonModel>> GetFeesByStatus(string status)
    {
        var result = await _feeService.GetFeesByStatusAsync(status);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Delete fee
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<JsonModel>> DeleteFee(Guid id)
    {
        var result = await _feeService.DeleteFeeAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get fee statistics
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<JsonModel>> GetFeeStatistics()
    {
        var result = await _feeService.GetFeeStatisticsAsync();
        return StatusCode(result.StatusCode, result);
    }

    // Category Fee Range endpoints

    /// <summary>
    /// Create a new category fee range
    /// </summary>
    [HttpPost("ranges")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<JsonModel>> CreateFeeRange([FromBody] CreateCategoryFeeRangeDto createDto)
    {
        var result = await _feeRangeService.CreateFeeRangeAsync(createDto);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get fee range by ID
    /// </summary>
    [HttpGet("ranges/{id}")]
    public async Task<ActionResult<JsonModel>> GetFeeRange(Guid id)
    {
        var result = await _feeRangeService.GetFeeRangeAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get fee range by category
    /// </summary>
    [HttpGet("ranges/category/{categoryId}")]
    public async Task<ActionResult<JsonModel>> GetFeeRangeByCategory(Guid categoryId)
    {
        var result = await _feeRangeService.GetFeeRangeByCategoryAsync(categoryId);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Update fee range
    /// </summary>
    [HttpPut("ranges/{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<JsonModel>> UpdateFeeRange(Guid id, [FromBody] UpdateCategoryFeeRangeDto updateDto)
    {
        var result = await _feeRangeService.UpdateFeeRangeAsync(id, updateDto);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get all fee ranges
    /// </summary>
    [HttpGet("ranges")]
    public async Task<ActionResult<JsonModel>> GetAllFeeRanges()
    {
        var result = await _feeRangeService.GetAllFeeRangesAsync();
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Delete fee range
    /// </summary>
    [HttpDelete("ranges/{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<JsonModel>> DeleteFeeRange(Guid id)
    {
        var result = await _feeRangeService.DeleteFeeRangeAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get fee range statistics
    /// </summary>
    [HttpGet("ranges/statistics")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<JsonModel>> GetFeeRangeStatistics()
    {
        var result = await _feeRangeService.GetFeeRangeStatisticsAsync();
        return StatusCode(result.StatusCode, result);
    }
} 