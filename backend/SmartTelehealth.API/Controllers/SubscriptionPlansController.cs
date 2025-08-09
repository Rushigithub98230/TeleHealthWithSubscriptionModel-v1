using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionPlansController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionPlansController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    /// <summary>
    /// Get all subscription plans (admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllPlans()
    {
        var response = await _subscriptionService.GetAllSubscriptionPlansAsync();
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get active subscription plans (public)
    /// </summary>
    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActivePlans()
    {
        var response = await _subscriptionService.GetActiveSubscriptionPlansAsync();
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get subscription plans by category (public)
    /// </summary>
    [HttpGet("category/{categoryId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPlansByCategory(string categoryId)
    {
        var response = await _subscriptionService.GetSubscriptionPlansByCategoryAsync(categoryId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get a specific subscription plan by ID (public)
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPlan(string id)
    {
        var response = await _subscriptionService.GetSubscriptionPlanAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Create a new subscription plan (admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreatePlan([FromBody] CreateSubscriptionPlanDto createDto)
    {
        var response = await _subscriptionService.CreateSubscriptionPlanAsync(createDto);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Update a subscription plan (admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdatePlan(string id, [FromBody] UpdateSubscriptionPlanDto updateDto)
    {
        if (id != updateDto.Id)
            return BadRequest("ID mismatch");
        updateDto.Id = id;
        var response = await _subscriptionService.UpdateSubscriptionPlanAsync(id, updateDto);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Delete a subscription plan (admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeletePlan(string id)
    {
        var response = await _subscriptionService.DeleteSubscriptionPlanAsync(id);
        return StatusCode(response.StatusCode, response);
    }
} 