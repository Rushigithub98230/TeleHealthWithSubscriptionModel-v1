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
    public async Task<IActionResult> GetDashboardAnalytics()
    {
        try
        {
            // Get all analytics data for dashboard
            var subscriptionAnalytics = await _analyticsService.GetSubscriptionAnalyticsAsync();
            var billingAnalytics = await _analyticsService.GetBillingAnalyticsAsync();
            var userAnalytics = await _analyticsService.GetUserAnalyticsAsync();
            var systemAnalytics = await _analyticsService.GetSystemAnalyticsAsync();

            var dashboardData = new
            {
                subscriptions = subscriptionAnalytics.Data,
                billing = billingAnalytics.Data,
                users = userAnalytics.Data,
                system = systemAnalytics.Data,
                charts = new
                {
                    revenueTrends = new[] { 12000, 19000, 15000, 25000, 22000, 30000 },
                    subscriptionGrowth = new[] { 45, 12, 8, 5 },
                    paymentSuccessRate = new[] { 85, 10, 5 },
                    planDistribution = new[] { 30, 45, 25 }
                }
            };

            return Ok(new { data = dashboardData });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { data = new { } });
        }
    }

    /// <summary>
    /// Get subscription analytics
    /// </summary>
    [HttpGet("subscriptions")]
    public async Task<IActionResult> GetSubscriptionAnalytics()
    {
        var response = await _analyticsService.GetSubscriptionAnalyticsAsync();
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get billing analytics
    /// </summary>
    [HttpGet("billing")]
    public async Task<IActionResult> GetBillingAnalytics()
    {
        var response = await _analyticsService.GetBillingAnalyticsAsync();
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get user analytics
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetUserAnalytics()
    {
        var response = await _analyticsService.GetUserAnalyticsAsync();
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get provider analytics
    /// </summary>
    [HttpGet("providers")]
    public async Task<IActionResult> GetProviderAnalytics()
    {
        var response = await _analyticsService.GetProviderAnalyticsAsync();
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get system analytics
    /// </summary>
    [HttpGet("system")]
    public async Task<IActionResult> GetSystemAnalytics()
    {
        var response = await _analyticsService.GetSystemAnalyticsAsync();
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get system health
    /// </summary>
    [HttpGet("system/health")]
    public async Task<IActionResult> GetSystemHealth()
    {
        var response = await _analyticsService.GetSystemHealthAsync();
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Generate subscription report
    /// </summary>
    [HttpGet("reports/subscriptions")]
    public async Task<IActionResult> GenerateSubscriptionReport([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] string format = "pdf")
    {
        try
        {
            var response = await _analyticsService.GenerateSubscriptionReportAsync(startDate, endDate);
            var fileName = $"subscription-report-{startDate:yyyy-MM-dd}-{endDate:yyyy-MM-dd}.{format}";
            return File(response.Data, GetContentType(format), fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error generating subscription report", error = ex.Message });
        }
    }

    /// <summary>
    /// Generate billing report
    /// </summary>
    [HttpGet("reports/billing")]
    public async Task<IActionResult> GenerateBillingReport([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] string format = "pdf")
    {
        try
        {
            var response = await _analyticsService.GenerateBillingReportAsync(startDate, endDate);
            var fileName = $"billing-report-{startDate:yyyy-MM-dd}-{endDate:yyyy-MM-dd}.{format}";
            return File(response.Data, GetContentType(format), fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error generating billing report", error = ex.Message });
        }
    }

    /// <summary>
    /// Generate user report
    /// </summary>
    [HttpGet("reports/users")]
    public async Task<IActionResult> GenerateUserReport([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] string format = "pdf")
    {
        try
        {
            var response = await _analyticsService.GenerateUserReportAsync(startDate, endDate);
            var fileName = $"user-report-{startDate:yyyy-MM-dd}-{endDate:yyyy-MM-dd}.{format}";
            return File(response.Data, GetContentType(format), fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error generating user report", error = ex.Message });
        }
    }

    /// <summary>
    /// Generate provider report
    /// </summary>
    [HttpGet("reports/providers")]
    public async Task<IActionResult> GenerateProviderReport([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] string format = "pdf")
    {
        try
        {
            var response = await _analyticsService.GenerateProviderReportAsync(startDate, endDate);
            var fileName = $"provider-report-{startDate:yyyy-MM-dd}-{endDate:yyyy-MM-dd}.{format}";
            return File(response.Data, GetContentType(format), fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error generating provider report", error = ex.Message });
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