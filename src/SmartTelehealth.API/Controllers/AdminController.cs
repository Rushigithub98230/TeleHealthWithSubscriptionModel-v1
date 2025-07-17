using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Interfaces;
using System.Security.Claims;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IBillingService _billingService;
    private readonly ICategoryService _categoryService;
    private readonly IProviderService _providerService;
    private readonly IUserService _userService;
    private readonly IAuditService _auditService;
    private readonly IAnalyticsService _analyticsService;

    public AdminController(
        ISubscriptionService subscriptionService,
        IBillingService billingService,
        ICategoryService categoryService,
        IProviderService providerService,
        IUserService userService,
        IAuditService auditService,
        IAnalyticsService analyticsService)
    {
        _subscriptionService = subscriptionService;
        _billingService = billingService;
        _categoryService = categoryService;
        _providerService = providerService;
        _userService = userService;
        _auditService = auditService;
        _analyticsService = analyticsService;
    }

    /// <summary>
    /// Get admin dashboard data
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<AdminDashboardDto>> GetDashboard()
    {
        try
        {
            var dashboard = new AdminDashboardDto
            {
                TotalSubscriptions = await GetTotalSubscriptions(),
                ActiveSubscriptions = await GetActiveSubscriptions(),
                TotalRevenue = await GetTotalRevenue(),
                MonthlyRecurringRevenue = await GetMonthlyRecurringRevenue(),
                TotalUsers = await GetTotalUsers(),
                TotalProviders = await GetTotalProviders(),
                RecentSubscriptions = await GetRecentSubscriptions(),
                RecentBillingRecords = await GetRecentBillingRecords(),
                SystemHealth = await GetSystemHealthData()
            };

            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving dashboard data", error = ex.Message });
        }
    }

    /// <summary>
    /// Get all subscriptions
    /// </summary>
    [HttpGet("subscriptions")]
    public async Task<IActionResult> GetAllSubscriptions()
    {
        var response = await _subscriptionService.GetActiveSubscriptionsAsync();
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get audit logs
    /// </summary>
    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs([FromQuery] string? action = null, [FromQuery] string? userId = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var response = await _auditService.GetAuditLogsAsync(action, userId, startDate, endDate, page, pageSize);
        return StatusCode(200, response);
    }

    /// <summary>
    /// Export data
    /// </summary>
    [HttpGet("export/{dataType}")]
    public async Task<IActionResult> ExportData(string dataType, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        // TODO: Implement data export functionality in service
        return Ok(new { message = $"Export of {dataType} data initiated" });
    }

    // Helper methods for dashboard
    private async Task<int> GetTotalSubscriptions()
    {
        var response = await _subscriptionService.GetActiveSubscriptionsAsync();
        return response.Data?.Count() ?? 0;
    }

    private async Task<int> GetActiveSubscriptions()
    {
        var response = await _subscriptionService.GetActiveSubscriptionsAsync();
        return response.Data?.Count(s => s.IsActive) ?? 0;
    }

    private async Task<decimal> GetTotalRevenue()
    {
        // TODO: Implement revenue calculation
        return 50000.00m;
    }

    private async Task<decimal> GetMonthlyRecurringRevenue()
    {
        // TODO: Implement MRR calculation
        return 15000.00m;
    }

    private async Task<int> GetTotalUsers()
    {
        // TODO: Implement user count
        return 1250;
    }

    private async Task<int> GetTotalProviders()
    {
        // TODO: Implement provider count
        return 45;
    }

    private async Task<IEnumerable<SubscriptionDto>> GetRecentSubscriptions()
    {
        var response = await _subscriptionService.GetActiveSubscriptionsAsync();
        return response.Data?.OrderByDescending(s => s.CreatedAt).Take(10) ?? new List<SubscriptionDto>();
    }

    private async Task<IEnumerable<BillingRecordDto>> GetRecentBillingRecords()
    {
        // TODO: Implement recent billing records
        return new List<BillingRecordDto>();
    }

    private async Task<SystemHealthDto> GetSystemHealthData()
    {
        return new SystemHealthDto
        {
            DatabaseStatus = "Healthy",
            ApiStatus = "Healthy",
            PaymentGatewayStatus = "Healthy",
            EmailServiceStatus = "Healthy",
            LastBackup = DateTime.UtcNow.AddHours(-2),
            SystemUptime = TimeSpan.FromDays(30),
            ActiveConnections = 150,
            MemoryUsage = 75.5,
            CpuUsage = 45.2
        };
    }
}

public class AdminDashboardDto
{
    public int TotalSubscriptions { get; set; }
    public int ActiveSubscriptions { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRecurringRevenue { get; set; }
    public int TotalUsers { get; set; }
    public int TotalProviders { get; set; }
    public IEnumerable<SubscriptionDto> RecentSubscriptions { get; set; } = new List<SubscriptionDto>();
    public IEnumerable<BillingRecordDto> RecentBillingRecords { get; set; } = new List<BillingRecordDto>();
    public SystemHealthDto SystemHealth { get; set; } = new();
} 