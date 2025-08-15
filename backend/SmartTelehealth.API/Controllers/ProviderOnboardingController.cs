using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProviderOnboardingController : ControllerBase
{
    private readonly IProviderOnboardingService _onboardingService;
    private readonly ILogger<ProviderOnboardingController> _logger;

    public ProviderOnboardingController(
        IProviderOnboardingService onboardingService,
        ILogger<ProviderOnboardingController> logger)
    {
        _onboardingService = onboardingService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new provider onboarding application
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<ProviderOnboardingDto>>> CreateOnboarding([FromBody] CreateProviderOnboardingDto createDto)
    {
        var result = await _onboardingService.CreateOnboardingAsync(createDto);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get onboarding by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProviderOnboardingDto>>> GetOnboarding(Guid id)
    {
        var result = await _onboardingService.GetOnboardingAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get onboarding by user ID
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<ApiResponse<ProviderOnboardingDto>>> GetOnboardingByUser(int userId)
    {
        var result = await _onboardingService.GetOnboardingByUserIdAsync(userId);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Update onboarding application
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ProviderOnboardingDto>>> UpdateOnboarding(Guid id, [FromBody] UpdateProviderOnboardingDto updateDto)
    {
        var result = await _onboardingService.UpdateOnboardingAsync(id, updateDto);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Submit onboarding application for review
    /// </summary>
    [HttpPost("{id}/submit")]
    public async Task<ActionResult<ApiResponse<ProviderOnboardingDto>>> SubmitOnboarding(Guid id)
    {
        var result = await _onboardingService.SubmitOnboardingAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Review onboarding application (Admin only)
    /// </summary>
    [HttpPost("{id}/review")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<ApiResponse<ProviderOnboardingDto>>> ReviewOnboarding(Guid id, [FromBody] ReviewProviderOnboardingDto reviewDto)
    {
        var result = await _onboardingService.ReviewOnboardingAsync(id, reviewDto);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get all onboarding applications with optional filtering
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProviderOnboardingDto>>>> GetAllOnboardings(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _onboardingService.GetAllOnboardingsAsync(status, page, pageSize);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get pending onboarding applications
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProviderOnboardingDto>>>> GetPendingOnboardings()
    {
        var result = await _onboardingService.GetPendingOnboardingsAsync();
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get onboarding applications by status
    /// </summary>
    [HttpGet("status/{status}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProviderOnboardingDto>>>> GetOnboardingsByStatus(string status)
    {
        var result = await _onboardingService.GetOnboardingsByStatusAsync(status);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Delete onboarding application
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteOnboarding(Guid id)
    {
        var result = await _onboardingService.DeleteOnboardingAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get onboarding statistics
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<ApiResponse<OnboardingStatisticsDto>>> GetOnboardingStatistics()
    {
        var result = await _onboardingService.GetOnboardingStatisticsAsync();
        return StatusCode(result.StatusCode, result);
    }
} 