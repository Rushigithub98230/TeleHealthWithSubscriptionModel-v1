using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<JsonModel>> GetDashboardAnalytics()
    {
        try
        {
            // Get all analytics data for dashboard
            var subscriptionAnalytics = await _analyticsService.GetSubscriptionAnalyticsAsync(null, null);
            var billingAnalytics = await _analyticsService.GetBillingAnalyticsAsync();
            var userAnalytics = await _analyticsService.GetUserAnalyticsAsync();
            var systemAnalytics = await _analyticsService.GetSystemAnalyticsAsync();

            var dashboardData = new
            {
                subscriptions = subscriptionAnalytics.data,
                billing = billingAnalytics.data,
                users = userAnalytics.data,
                system = systemAnalytics.data,
                charts = new
                {
                    revenueTrends = new[] { 12000, 19000, 15000, 25000, 22000, 30000 },
                    subscriptionGrowth = new[] { 45, 12, 8, 5 },
                    paymentSuccessRate = new[] { 85, 10, 5 },
                    planDistribution = new[] { 30, 45, 25 }
                }
            };

            return Ok(new JsonModel { data = dashboardData, Message = "Dashboard analytics retrieved successfully", StatusCode = 200 });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new JsonModel { data = new object(), Message = "An error occurred while retrieving dashboard analytics", StatusCode = 500 });
        }
    }

    /// <summary>
    /// Get subscription analytics
    /// </summary>
    [HttpGet("subscriptions")]
    public async Task<ActionResult<JsonModel>> GetSubscriptionAnalytics()
    {
        var response = await _analyticsService.GetSubscriptionAnalyticsAsync(null, null);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get billing analytics
    /// </summary>
    [HttpGet("billing")]
    public async Task<ActionResult<JsonModel>> GetBillingAnalytics()
    {
        var response = await _analyticsService.GetBillingAnalyticsAsync();
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get user analytics
    /// </summary>
    [HttpGet("users")]
    public async Task<ActionResult<JsonModel>> GetUserAnalytics()
    {
        var response = await _analyticsService.GetUserAnalyticsAsync();
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get provider analytics
    /// </summary>
    [HttpGet("providers")]
    public async Task<ActionResult<JsonModel>> GetProviderAnalytics()
    {
        var response = await _analyticsService.GetProviderAnalyticsAsync();
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get system analytics
    /// </summary>
    [HttpGet("system")]
    public async Task<ActionResult<JsonModel>> GetSystemAnalytics()
    {
        var response = await _analyticsService.GetSystemAnalyticsAsync();
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get system health
    /// </summary>
    [HttpGet("system/health")]
    public async Task<ActionResult<JsonModel>> GetSystemHealth()
    {
        var response = await _analyticsService.GetSystemHealthAsync();
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Generate subscription report
    /// </summary>
    [HttpGet("reports/subscriptions")]
    public async Task<ActionResult<JsonModel>> GenerateSubscriptionReport([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] string format = "pdf")
    {
        try
        {
            var response = await _analyticsService.GenerateSubscriptionReportAsync(startDate, endDate);
            var fileName = $"subscription-report-{startDate:yyyy-MM-dd}-{endDate:yyyy-MM-dd}.{format}";
            return Ok(new JsonModel { data = new { fileData = response.data, fileName = fileName, contentType = GetContentType(format) }, Message = "Subscription report generated successfully", StatusCode = 200 });
        }
        catch (Exception ex)
        {
            return BadRequest(new JsonModel { data = new object(), Message = "Error generating subscription report", StatusCode = 400 });
        }
    }

    /// <summary>
    /// Generate billing report
    /// </summary>
    [HttpGet("reports/billing")]
    public async Task<ActionResult<JsonModel>> GenerateBillingReport([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] string format = "pdf")
    {
        try
        {
            var response = await _analyticsService.GenerateBillingReportAsync(startDate, endDate);
            var fileName = $"billing-report-{startDate:yyyy-MM-dd}-{endDate:yyyy-MM-dd}.{format}";
            return Ok(new JsonModel { data = new { fileData = response.data, fileName = fileName, contentType = GetContentType(format) }, Message = "Billing report generated successfully", StatusCode = 200 });
        }
        catch (Exception ex)
        {
            return BadRequest(new JsonModel { data = new object(), Message = "Error generating billing report", StatusCode = 400 });
        }
    }

    /// <summary>
    /// Generate user report
    /// </summary>
    [HttpGet("reports/users")]
    public async Task<ActionResult<JsonModel>> GenerateUserReport([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] string format = "pdf")
    {
        try
        {
            var response = await _analyticsService.GenerateUserReportAsync(startDate, endDate);
            var fileName = $"user-report-{startDate:yyyy-MM-dd}-{endDate:yyyy-MM-dd}.{format}";
            return Ok(new JsonModel { data = new { fileData = response.data, fileName = fileName, contentType = GetContentType(format) }, Message = "User report generated successfully", StatusCode = 200 });
        }
        catch (Exception ex)
        {
            return BadRequest(new JsonModel { data = new object(), Message = "Error generating user report", StatusCode = 400 });
        }
    }

    /// <summary>
    /// Generate provider report
    /// </summary>
    [HttpGet("reports/providers")]
    public async Task<ActionResult<JsonModel>> GenerateProviderReport([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] string format = "pdf")
    {
        try
        {
            var response = await _analyticsService.GenerateProviderReportAsync(startDate, endDate);
            var fileName = $"provider-report-{startDate:yyyy-MM-dd}-{endDate:yyyy-MM-dd}.{format}";
            return Ok(new JsonModel { data = new { fileData = response.data, fileName = fileName, contentType = GetContentType(format) }, Message = "Provider report generated successfully", StatusCode = 200 });
        }
        catch (Exception ex)
        {
            return BadRequest(new JsonModel { data = new object(), Message = "Error generating provider report", StatusCode = 400 });
        }
    }

    private string GetContentType(string format)
    {
        return format.ToLower() switch
        {
            "pdf" => "application/pdf",
            "csv" => "text/csv",
            "json" => "application/json",
            _ => "application/octet-stream"
        };
    }
} 