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
    public async Task<IActionResult> GenerateSubscriptionReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string format = "pdf")
    {
        try
        {
            var reportBytes = await _analyticsService.GenerateSubscriptionReportAsync(startDate, endDate, format);
            var fileName = $"subscription-report-{startDate:yyyy-MM-dd}-{endDate:yyyy-MM-dd}.{format}";
            
            return File(reportBytes, GetContentType(format), fileName);
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
    public async Task<IActionResult> GenerateBillingReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string format = "pdf")
    {
        try
        {
            var reportBytes = await _analyticsService.GenerateBillingReportAsync(startDate, endDate, format);
            var fileName = $"billing-report-{startDate:yyyy-MM-dd}-{endDate:yyyy-MM-dd}.{format}";
            
            return File(reportBytes, GetContentType(format), fileName);
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
    public async Task<IActionResult> GenerateUserReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string format = "pdf")
    {
        try
        {
            var reportBytes = await _analyticsService.GenerateUserReportAsync(startDate, endDate, format);
            var fileName = $"user-report-{startDate:yyyy-MM-dd}-{endDate:yyyy-MM-dd}.{format}";
            
            return File(reportBytes, GetContentType(format), fileName);
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
    public async Task<IActionResult> GenerateProviderReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string format = "pdf")
    {
        try
        {
            var reportBytes = await _analyticsService.GenerateProviderReportAsync(startDate, endDate, format);
            var fileName = $"provider-report-{startDate:yyyy-MM-dd}-{endDate:yyyy-MM-dd}.{format}";
            
            return File(reportBytes, GetContentType(format), fileName);
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