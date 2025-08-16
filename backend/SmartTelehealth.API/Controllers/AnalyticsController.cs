using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AnalyticsController : BaseController
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<JsonModel>> GetDashboardAnalytics()
    {
        return await _analyticsService.GetSubscriptionAnalyticsAsync(null, null, GetToken(HttpContext));
    }

    /// <summary>
    /// Get subscription analytics
    /// </summary>
    [HttpGet("subscriptions")]
    public async Task<ActionResult<JsonModel>> GetSubscriptionAnalytics()
    {
        return await _analyticsService.GetSubscriptionAnalyticsAsync(null, null, GetToken(HttpContext));
    }

    /// <summary>
    /// Get billing analytics
    /// </summary>
    [HttpGet("billing")]
    public async Task<ActionResult<JsonModel>> GetBillingAnalytics()
    {
        return await _analyticsService.GetBillingAnalyticsAsync(null, null, GetToken(HttpContext));
    }

    /// <summary>
    /// Get user analytics
    /// </summary>
    [HttpGet("users")]
    public async Task<ActionResult<JsonModel>> GetUserAnalytics()
    {
        return await _analyticsService.GetUserAnalyticsAsync(null, null, GetToken(HttpContext));
    }

    /// <summary>
    /// Get provider analytics
    /// </summary>
    [HttpGet("providers")]
    public async Task<ActionResult<JsonModel>> GetProviderAnalytics()
    {
        return await _analyticsService.GetProviderAnalyticsAsync(null, null, GetToken(HttpContext));
    }

    /// <summary>
    /// Get system analytics
    /// </summary>
    [HttpGet("system")]
    public async Task<ActionResult<JsonModel>> GetSystemAnalytics()
    {
        return await _analyticsService.GetSystemAnalyticsAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Get system health
    /// </summary>
    [HttpGet("system/health")]
    public async Task<ActionResult<JsonModel>> GetSystemHealth()
    {
        return await _analyticsService.GetSystemHealthAsync(GetToken(HttpContext));
    }

    /// <summary>
    /// Get revenue analytics
    /// </summary>
    [HttpGet("revenue")]
    public async Task<ActionResult<JsonModel>> GetRevenueAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.GetRevenueAnalyticsAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Get user activity analytics
    /// </summary>
    [HttpGet("user-activity")]
    public async Task<ActionResult<JsonModel>> GetUserActivityAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.GetUserActivityAnalyticsAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Get appointment analytics
    /// </summary>
    [HttpGet("appointments")]
    public async Task<ActionResult<JsonModel>> GetAppointmentAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.GetAppointmentAnalyticsAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Get subscription analytics with plan filter
    /// </summary>
    [HttpGet("subscriptions/plan/{planId}")]
    public async Task<ActionResult<JsonModel>> GetSubscriptionAnalyticsByPlan(string planId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.GetSubscriptionAnalyticsAsync(startDate, endDate, planId, GetToken(HttpContext));
    }

    /// <summary>
    /// Get subscription dashboard
    /// </summary>
    [HttpGet("subscriptions/dashboard")]
    public async Task<ActionResult<JsonModel>> GetSubscriptionDashboard([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.GetSubscriptionDashboardAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Get churn analytics
    /// </summary>
    [HttpGet("churn")]
    public async Task<ActionResult<JsonModel>> GetChurnAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.GetChurnAnalyticsAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Get plan analytics
    /// </summary>
    [HttpGet("plans")]
    public async Task<ActionResult<JsonModel>> GetPlanAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.GetPlanAnalyticsAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Get usage analytics
    /// </summary>
    [HttpGet("usage")]
    public async Task<ActionResult<JsonModel>> GetUsageAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.GetUsageAnalyticsAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Generate subscription report
    /// </summary>
    [HttpGet("reports/subscriptions")]
    public async Task<ActionResult<JsonModel>> GenerateSubscriptionReport([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.GenerateSubscriptionReportAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Generate billing report
    /// </summary>
    [HttpGet("reports/billing")]
    public async Task<ActionResult<JsonModel>> GenerateBillingReport([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.GenerateBillingReportAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Generate user report
    /// </summary>
    [HttpGet("reports/users")]
    public async Task<ActionResult<JsonModel>> GenerateUserReport([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.GenerateUserReportAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Generate provider report
    /// </summary>
    [HttpGet("reports/providers")]
    public async Task<ActionResult<JsonModel>> GenerateProviderReport([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.GenerateProviderReportAsync(startDate, endDate, GetToken(HttpContext));
    }

    /// <summary>
    /// Export subscription analytics
    /// </summary>
    [HttpGet("export/subscriptions")]
    public async Task<ActionResult<JsonModel>> ExportSubscriptionAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        return await _analyticsService.ExportSubscriptionAnalyticsAsync(startDate, endDate, GetToken(HttpContext));
    }
} 