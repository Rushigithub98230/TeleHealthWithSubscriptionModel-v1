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
    public async Task<ActionResult<ApiResponse<ProviderFeeDto>>> CreateFee([FromBody] CreateProviderFeeDto createDto)
    {
        var result = await _feeService.CreateFeeAsync(createDto);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get fee by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProviderFeeDto>>> GetFee(Guid id)
    {
        var result = await _feeService.GetFeeAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get fee by provider and category
    /// </summary>
    [HttpGet("provider/{providerId}/category/{categoryId}")]
    public async Task<ActionResult<ApiResponse<ProviderFeeDto>>> GetFeeByProviderAndCategory(Guid providerId, Guid categoryId)
    {
        var result = await _feeService.GetFeeByProviderAndCategoryAsync(providerId, categoryId);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Update fee proposal
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ProviderFeeDto>>> UpdateFee(Guid id, [FromBody] UpdateProviderFeeDto updateDto)
    {
        var result = await _feeService.UpdateFeeAsync(id, updateDto);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Submit fee proposal for review
    /// </summary>
    [HttpPost("{id}/propose")]
    public async Task<ActionResult<ApiResponse<ProviderFeeDto>>> ProposeFee(Guid id)
    {
        var result = await _feeService.ProposeFeeAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Review fee proposal (Admin only)
    /// </summary>
    [HttpPost("{id}/review")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<ApiResponse<ProviderFeeDto>>> ReviewFee(Guid id, [FromBody] ReviewProviderFeeDto reviewDto)
    {
        var result = await _feeService.ReviewFeeAsync(id, reviewDto);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get fees by provider
    /// </summary>
    [HttpGet("provider/{providerId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProviderFeeDto>>>> GetFeesByProvider(Guid providerId)
    {
        var result = await _feeService.GetFeesByProviderAsync(providerId);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get fees by category
    /// </summary>
    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProviderFeeDto>>>> GetFeesByCategory(Guid categoryId)
    {
        var result = await _feeService.GetFeesByCategoryAsync(categoryId);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get all fees with optional filtering
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProviderFeeDto>>>> GetAllFees(
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
    public async Task<ActionResult<ApiResponse<IEnumerable<ProviderFeeDto>>>> GetPendingFees()
    {
        var result = await _feeService.GetPendingFeesAsync();
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get fees by status
    /// </summary>
    [HttpGet("status/{status}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProviderFeeDto>>>> GetFeesByStatus(string status)
    {
        var result = await _feeService.GetFeesByStatusAsync(status);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Delete fee
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteFee(Guid id)
    {
        var result = await _feeService.DeleteFeeAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get fee statistics
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<ApiResponse<FeeStatisticsDto>>> GetFeeStatistics()
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
    public async Task<ActionResult<ApiResponse<CategoryFeeRangeDto>>> CreateFeeRange([FromBody] CreateCategoryFeeRangeDto createDto)
    {
        var result = await _feeRangeService.CreateFeeRangeAsync(createDto);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get fee range by ID
    /// </summary>
    [HttpGet("ranges/{id}")]
    public async Task<ActionResult<ApiResponse<CategoryFeeRangeDto>>> GetFeeRange(Guid id)
    {
        var result = await _feeRangeService.GetFeeRangeAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get fee range by category
    /// </summary>
    [HttpGet("ranges/category/{categoryId}")]
    public async Task<ActionResult<ApiResponse<CategoryFeeRangeDto>>> GetFeeRangeByCategory(Guid categoryId)
    {
        var result = await _feeRangeService.GetFeeRangeByCategoryAsync(categoryId);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Update fee range
    /// </summary>
    [HttpPut("ranges/{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<ApiResponse<CategoryFeeRangeDto>>> UpdateFeeRange(Guid id, [FromBody] UpdateCategoryFeeRangeDto updateDto)
    {
        var result = await _feeRangeService.UpdateFeeRangeAsync(id, updateDto);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get all fee ranges
    /// </summary>
    [HttpGet("ranges")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryFeeRangeDto>>>> GetAllFeeRanges()
    {
        var result = await _feeRangeService.GetAllFeeRangesAsync();
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Delete fee range
    /// </summary>
    [HttpDelete("ranges/{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteFeeRange(Guid id)
    {
        var result = await _feeRangeService.DeleteFeeRangeAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get fee range statistics
    /// </summary>
    [HttpGet("ranges/statistics")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<ApiResponse<FeeRangeStatisticsDto>>> GetFeeRangeStatistics()
    {
        var result = await _feeRangeService.GetFeeRangeStatisticsAsync();
        return StatusCode(result.StatusCode, result);
    }
} 