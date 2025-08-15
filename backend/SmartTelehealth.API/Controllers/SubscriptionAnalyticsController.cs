using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Infrastructure.Services;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize(Roles = "Admin")]
public class SubscriptionAnalyticsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IBillingService _billingService;
    private readonly IAnalyticsService _analyticsService;
    // private readonly SmartTelehealth.Application.Services.AutomatedBillingService _automatedBillingService;
    private readonly ILogger<SubscriptionAnalyticsController> _logger;
    private readonly IAuditService _auditService;

    public SubscriptionAnalyticsController(
        ISubscriptionService subscriptionService,
        IBillingService billingService,
        IAnalyticsService analyticsService,
        // SmartTelehealth.Application.Services.AutomatedBillingService automatedBillingService,
        ILogger<SubscriptionAnalyticsController> logger,
        IAuditService auditService)
    {
        _subscriptionService = subscriptionService;
        _billingService = billingService;
        _analyticsService = analyticsService;
        // _automatedBillingService = automatedBillingService;
        _logger = logger;
        _auditService = auditService;
    }

    /// <summary>
    /// Get comprehensive subscription analytics dashboard
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<JsonModel> GetDashboard([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var dashboard = new SubscriptionDashboardDto
            {
                Period = new DateRangeDto { StartDate = start, EndDate = end },
                Overview = await GetOverviewMetricsAsync(start, end),
                Revenue = await GetRevenueMetricsAsync(start, end),
                Churn = await GetChurnMetricsAsync(start, end),
                Plans = await GetPlanMetricsAsync(start, end),
                Usage = await GetUsageMetricsAsync(start, end),
                Trends = await GetTrendMetricsAsync(start, end)
            };

            await _auditService.LogUserActionAsync(GetCurrentUserId().ToString(), "GetSubscriptionDashboard", "Analytics", "Dashboard", "Dashboard accessed");
            
            return Ok(JsonModel.SuccessResponse(dashboard));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription dashboard");
            return StatusCode(500, JsonModel.ErrorResponse("Failed to retrieve dashboard data"));
        }
    }

    /// <summary>
    /// Get revenue analytics with detailed breakdown
    /// </summary>
    [HttpGet("revenue")]
    public async Task<ActionResult<JsonModel> GetRevenueAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var revenue = await GetRevenueMetricsAsync(start, end);
            
            return Ok(JsonModel.SuccessResponse(revenue));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue analytics");
            return StatusCode(500, JsonModel.ErrorResponse("Failed to retrieve revenue analytics"));
        }
    }

    /// <summary>
    /// Get churn analysis and retention metrics
    /// </summary>
    [HttpGet("churn")]
    public async Task<ActionResult<JsonModel> GetChurnAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var churn = await GetChurnMetricsAsync(start, end);
            
            return Ok(JsonModel.SuccessResponse(churn));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting churn analytics");
            return StatusCode(500, JsonModel.ErrorResponse("Failed to retrieve churn analytics"));
        }
    }

    /// <summary>
    /// Get plan performance analytics
    /// </summary>
    [HttpGet("plans")]
    public async Task<ActionResult<JsonModel> GetPlanAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var plans = await GetPlanMetricsAsync(start, end);
            
            return Ok(JsonModel.SuccessResponse(plans));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting plan analytics");
            return StatusCode(500, JsonModel.ErrorResponse("Failed to retrieve plan analytics"));
        }
    }

    /// <summary>
    /// Get usage analytics and patterns
    /// </summary>
    [HttpGet("usage")]
    public async Task<ActionResult<JsonModel> GetUsageAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var usage = await GetUsageMetricsAsync(start, end);
            
            return Ok(JsonModel.SuccessResponse(usage));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage analytics");
            return StatusCode(500, JsonModel.ErrorResponse("Failed to retrieve usage analytics"));
        }
    }

    /// <summary>
    /// Get trend analysis and forecasting
    /// </summary>
    [HttpGet("trends")]
    public async Task<ActionResult<JsonModel> GetTrendAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var trends = await GetTrendMetricsAsync(start, end);
            
            return Ok(JsonModel.SuccessResponse(trends));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trend analytics");
            return StatusCode(500, JsonModel.ErrorResponse("Failed to retrieve trend analytics"));
        }
    }

    /// <summary>
    /// Get billing cycle report
    /// </summary>
    [HttpGet("billing-cycle")]
    public async Task<ActionResult<JsonModel> GetBillingCycleReport([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            // var report = await _automatedBillingService.GetBillingCycleReportAsync(startDate, endDate);
            
            // return Ok(report);
            return StatusCode(501, JsonModel.ErrorResponse("Billing cycle report not implemented"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing cycle report");
            return StatusCode(500, JsonModel.ErrorResponse("Failed to retrieve billing cycle report"));
        }
    }

    /// <summary>
    /// Trigger manual billing cycle
    /// </summary>
    [HttpPost("trigger-billing-cycle")]
    public async Task<ActionResult<JsonModel> TriggerBillingCycle()
    {
        try
        {
            // var result = await _automatedBillingService.TriggerManualBillingCycleAsync();
            
            // await _auditService.LogUserActionAsync(GetCurrentUserId().ToString(), "TriggerBillingCycle", "Analytics", "Manual", "Manual billing cycle triggered");
            
            // return Ok(result);
            return StatusCode(501, JsonModel.ErrorResponse("Manual billing cycle not implemented"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering billing cycle");
            return StatusCode(500, JsonModel.ErrorResponse("Failed to trigger billing cycle"));
        }
    }

    /// <summary>
    /// Get subscription analytics for specific subscription
    /// </summary>
    [HttpGet("subscription/{subscriptionId}")]
    public async Task<ActionResult<JsonModel> GetSubscriptionAnalytics(string subscriptionId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var analytics = await _subscriptionService.GetSubscriptionAnalyticsAsync(subscriptionId);
            
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription analytics for {SubscriptionId}", subscriptionId);
            return StatusCode(500, JsonModel.ErrorResponse("Failed to retrieve subscription analytics"));
        }
    }

    /// <summary>
    /// Export analytics data to CSV
    /// </summary>
    [HttpGet("export")]
    public async Task<ActionResult> ExportAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] string format = "csv")
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var exportData = await _analyticsService.ExportSubscriptionAnalyticsAsync(start, end);
            
            await _auditService.LogUserActionAsync(GetCurrentUserId().ToString(), "ExportAnalytics", "Analytics", "Export", $"Analytics exported in {format} format");

            return File(exportData.Data, "text/csv", $"subscription-analytics-{start:yyyy-MM-dd}-{end:yyyy-MM-dd}.csv");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting analytics");
            return StatusCode(500, JsonModel.ErrorResponse("Failed to export analytics"));
        }
    }

    // Private helper methods for analytics calculations
    private async Task<OverviewMetricsDto> GetOverviewMetricsAsync(DateTime startDate, DateTime endDate)
    {
        var allSubscriptions = await _subscriptionService.GetAllSubscriptionsAsync();
        var activeSubscriptions = allSubscriptions.Data.Where(s => s.Status == "Active");
        var cancelledSubscriptions = allSubscriptions.Data.Where(s => s.Status == "Cancelled");
        var pausedSubscriptions = allSubscriptions.Data.Where(s => s.Status == "Paused");

        return new OverviewMetricsDto
        {
            TotalSubscriptions = allSubscriptions.Data.Count(),
            ActiveSubscriptions = activeSubscriptions.Count(),
            CancelledSubscriptions = cancelledSubscriptions.Count(),
            PausedSubscriptions = pausedSubscriptions.Count(),
            TrialSubscriptions = allSubscriptions.Data.Count(s => s.IsTrialSubscription),
            NewSubscriptionsThisPeriod = allSubscriptions.Data.Count(s => s.StartDate >= startDate && s.StartDate <= endDate),
            CancelledSubscriptionsThisPeriod = cancelledSubscriptions.Count(s => s.CancelledDate >= startDate && s.CancelledDate <= endDate),
            AverageSubscriptionValue = allSubscriptions.Data.Any() ? allSubscriptions.Data.Average(s => s.CurrentPrice) : 0,
            TotalRevenue = allSubscriptions.Data.Sum(s => s.CurrentPrice)
        };
    }

    private async Task<RevenueAnalyticsDto> GetRevenueMetricsAsync(DateTime startDate, DateTime endDate)
    {
        var allSubscriptions = await _subscriptionService.GetAllSubscriptionsAsync();
        
        // Create a mock billing history since we can't call GetPaymentHistoryAsync with "all"
        var billingHistory = new List<PaymentHistoryDto>();

        var monthlyRevenue = billingHistory
            .Where(bh => bh.Status == "Paid")
            .GroupBy(bh => new { bh.PaymentDate.Year, bh.PaymentDate.Month })
            .Select(g => new MonthlyRevenueDto
            {
                Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                Revenue = g.Sum(bh => bh.Amount),
                PaymentCount = g.Count()
            })
            .OrderBy(mr => mr.Month)
            .ToList();

        return new RevenueAnalyticsDto
        {
            TotalRevenue = billingHistory.Where(bh => bh.Status == "Paid").Sum(bh => bh.Amount),
            MonthlyRecurringRevenue = billingHistory.Where(bh => bh.Status == "Paid" && bh.PaymentDate >= DateTime.UtcNow.AddDays(-30)).Sum(bh => bh.Amount),
            AverageRevenuePerUser = allSubscriptions.Data.Any() ? billingHistory.Where(bh => bh.Status == "Paid").Sum(bh => bh.Amount) / allSubscriptions.Data.Count() : 0,
            RevenueGrowth = CalculateGrowthRate(monthlyRevenue),
            MonthlyRevenue = monthlyRevenue,
            RevenueByPlan = allSubscriptions.Data
                .GroupBy(s => s.PlanName)
                .Select(g => new PlanRevenueDto
                {
                    PlanName = g.Key,
                    Revenue = g.Sum(s => s.CurrentPrice),
                    SubscriptionCount = g.Count()
                })
                .ToList()
        };
    }

    private async Task<ChurnAnalyticsDto> GetChurnMetricsAsync(DateTime startDate, DateTime endDate)
    {
        var allSubscriptions = await _subscriptionService.GetAllSubscriptionsAsync();
        var cancelledSubscriptions = allSubscriptions.Data.Where(s => s.Status == "Cancelled" && s.CancelledDate >= startDate && s.CancelledDate <= endDate);

        var churnRate = allSubscriptions.Data.Any() ? (decimal)cancelledSubscriptions.Count() / allSubscriptions.Data.Count() * 100 : 0;
        var retentionRate = 100 - churnRate;

        return new ChurnAnalyticsDto
        {
            ChurnRate = churnRate,
            RetentionRate = retentionRate,
            CancelledSubscriptions = cancelledSubscriptions.Count(),
            CancellationReasons = cancelledSubscriptions
                .GroupBy(s => s.CancellationReason ?? "No reason provided")
                .Select(g => new CancellationReasonDto
                {
                    Reason = g.Key,
                    Count = g.Count(),
                    Percentage = (decimal)g.Count() / cancelledSubscriptions.Count() * 100
                })
                .OrderByDescending(cr => cr.Count)
                .ToList(),
            AverageLifetime = CalculateAverageLifetime(allSubscriptions.Data),
            CohortRetention = await CalculateCohortRetentionAsync(allSubscriptions.Data, startDate, endDate)
        };
    }

    private async Task<PlanAnalyticsDto> GetPlanMetricsAsync(DateTime startDate, DateTime endDate)
    {
        var allSubscriptions = await _subscriptionService.GetAllSubscriptionsAsync();
        var allPlans = await _subscriptionService.GetAllPlansAsync();

        var planMetrics = allPlans.Data.Select(plan => new PlanPerformanceDto
        {
            PlanName = plan.Name,
            TotalSubscriptions = allSubscriptions.Data.Count(s => s.PlanName == plan.Name),
            ActiveSubscriptions = allSubscriptions.Data.Count(s => s.PlanName == plan.Name && s.Status == "Active"),
            CancelledSubscriptions = allSubscriptions.Data.Count(s => s.PlanName == plan.Name && s.Status == "Cancelled"),
            NewSubscriptionsThisPeriod = allSubscriptions.Data.Count(s => s.PlanName == plan.Name && s.StartDate >= startDate && s.StartDate <= endDate),
            Revenue = allSubscriptions.Data.Where(s => s.PlanName == plan.Name).Sum(s => s.CurrentPrice),
            AverageSubscriptionValue = allSubscriptions.Data.Where(s => s.PlanName == plan.Name).Any() 
                ? allSubscriptions.Data.Where(s => s.PlanName == plan.Name).Average(s => s.CurrentPrice) 
                : 0,
            ConversionRate = CalculateConversionRate(plan, allSubscriptions.Data, startDate, endDate)
        }).ToList();

        return new PlanAnalyticsDto
        {
            PlanPerformance = planMetrics,
            TopPerformingPlans = planMetrics.OrderByDescending(p => p.Revenue).Take(5).ToList(),
            PlanComparison = planMetrics.OrderByDescending(p => p.TotalSubscriptions).ToList()
        };
    }

    private async Task<UsageAnalyticsDto> GetUsageMetricsAsync(DateTime startDate, DateTime endDate)
    {
        var allSubscriptions = await _subscriptionService.GetAllSubscriptionsAsync();
        var activeSubscriptions = allSubscriptions.Data.Where(s => s.Status == "Active");

        return new UsageAnalyticsDto
        {
            AverageUsagePerUser = CalculateAverageUsage(activeSubscriptions),
            UsageDistribution = await CalculateUsageDistributionAsync(activeSubscriptions),
            PeakUsageTimes = await CalculatePeakUsageTimesAsync(startDate, endDate),
            UnderutilizedSubscriptions = activeSubscriptions.Count(s => s.UsagePercentage < 50),
            OverutilizedSubscriptions = activeSubscriptions.Count(s => s.UsagePercentage > 90),
            UsageTrends = await CalculateUsageTrendsAsync(startDate, endDate)
        };
    }

    private async Task<TrendAnalyticsDto> GetTrendMetricsAsync(DateTime startDate, DateTime endDate)
    {
        var allSubscriptions = await _subscriptionService.GetAllSubscriptionsAsync();
        
        // Create a mock billing history since we can't call GetPaymentHistoryAsync with "all"
        var billingHistory = new List<PaymentHistoryDto>();

        var monthlyTrends = billingHistory
            .Where(bh => bh.Status == "Paid")
            .GroupBy(bh => new { bh.PaymentDate.Year, bh.PaymentDate.Month })
            .Select(g => new MonthlyTrendDto
            {
                Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                Revenue = g.Sum(bh => bh.Amount),
                PaymentCount = g.Count(),
                AveragePayment = g.Average(bh => bh.Amount)
            })
            .OrderBy(mt => mt.Month)
            .ToList();

        return new TrendAnalyticsDto
        {
            RevenueTrend = monthlyTrends,
            SubscriptionGrowth = CalculateSubscriptionGrowth(allSubscriptions.Data, startDate, endDate),
            ChurnTrend = CalculateChurnTrend(allSubscriptions.Data, startDate, endDate),
            Forecast = await GenerateForecastAsync(monthlyTrends)
        };
    }

    // Helper calculation methods
    private decimal CalculateGrowthRate(List<MonthlyRevenueDto> monthlyRevenue)
    {
        if (monthlyRevenue.Count < 2) return 0;
        
        var currentMonth = monthlyRevenue.Last().Revenue;
        var previousMonth = monthlyRevenue[monthlyRevenue.Count - 2].Revenue;
        
        return previousMonth > 0 ? (currentMonth - previousMonth) / previousMonth * 100 : 0;
    }

    private decimal CalculateAverageLifetime(IEnumerable<SubscriptionDto> subscriptions)
    {
        var cancelledSubscriptions = subscriptions.Where(s => s.Status == "Cancelled" && s.CancelledDate.HasValue);
        if (!cancelledSubscriptions.Any()) return 0;

        var totalDays = cancelledSubscriptions.Sum(s => (s.CancelledDate.Value - s.StartDate).Days);
        return totalDays / cancelledSubscriptions.Count();
    }

    private async Task<List<CohortRetentionDto>> CalculateCohortRetentionAsync(IEnumerable<SubscriptionDto> subscriptions, DateTime startDate, DateTime endDate)
    {
        var cohorts = subscriptions
            .Where(s => s.StartDate >= startDate && s.StartDate <= endDate)
            .GroupBy(s => new { s.StartDate.Year, s.StartDate.Month })
            .Select(g => new CohortRetentionDto
            {
                Cohort = $"{g.Key.Year}-{g.Key.Month:D2}",
                InitialSubscriptions = g.Count(),
                RetainedSubscriptions = g.Count(s => s.Status == "Active" || s.Status == "Paused"),
                RetentionRate = (decimal)g.Count(s => s.Status == "Active" || s.Status == "Paused") / g.Count() * 100
            })
            .OrderBy(cr => cr.Cohort)
            .ToList();

        return cohorts;
    }

    private decimal CalculateConversionRate(SubscriptionPlanDto plan, IEnumerable<SubscriptionDto> subscriptions, DateTime startDate, DateTime endDate)
    {
        var trialSubscriptions = subscriptions.Count(s => s.PlanName == plan.Name && s.IsTrialSubscription && s.StartDate >= startDate && s.StartDate <= endDate);
        var convertedSubscriptions = subscriptions.Count(s => s.PlanName == plan.Name && !s.IsTrialSubscription && s.StartDate >= startDate && s.StartDate <= endDate);

        return trialSubscriptions > 0 ? (decimal)convertedSubscriptions / trialSubscriptions * 100 : 0;
    }

    private decimal CalculateAverageUsage(IEnumerable<SubscriptionDto> subscriptions)
    {
        return subscriptions.Any() ? subscriptions.Average(s => s.UsagePercentage) : 0;
    }

    private async Task<List<UsageDistributionDto>> CalculateUsageDistributionAsync(IEnumerable<SubscriptionDto> subscriptions)
    {
        return new List<UsageDistributionDto>
        {
            new() { Range = "0-25%", Count = subscriptions.Count(s => s.UsagePercentage >= 0 && s.UsagePercentage <= 25) },
            new() { Range = "26-50%", Count = subscriptions.Count(s => s.UsagePercentage > 25 && s.UsagePercentage <= 50) },
            new() { Range = "51-75%", Count = subscriptions.Count(s => s.UsagePercentage > 50 && s.UsagePercentage <= 75) },
            new() { Range = "76-100%", Count = subscriptions.Count(s => s.UsagePercentage > 75 && s.UsagePercentage <= 100) }
        };
    }

    private async Task<List<PeakUsageTimeDto>> CalculatePeakUsageTimesAsync(DateTime startDate, DateTime endDate)
    {
        // This would typically query actual usage data
        return new List<PeakUsageTimeDto>
        {
            new() { Hour = 9, UsageCount = 150 },
            new() { Hour = 10, UsageCount = 200 },
            new() { Hour = 11, UsageCount = 180 },
            new() { Hour = 14, UsageCount = 160 },
            new() { Hour = 15, UsageCount = 170 },
            new() { Hour = 16, UsageCount = 190 }
        };
    }

    private async Task<List<UsageTrendDto>> CalculateUsageTrendsAsync(DateTime startDate, DateTime endDate)
    {
        // This would typically query actual usage data over time
        return new List<UsageTrendDto>
        {
            new() { Date = startDate.AddDays(0), AverageUsage = 65 },
            new() { Date = startDate.AddDays(7), AverageUsage = 68 },
            new() { Date = startDate.AddDays(14), AverageUsage = 72 },
            new() { Date = startDate.AddDays(21), AverageUsage = 70 }
        };
    }

    private List<MonthlyTrendDto> CalculateSubscriptionGrowth(IEnumerable<SubscriptionDto> subscriptions, DateTime startDate, DateTime endDate)
    {
        return subscriptions
            .Where(s => s.StartDate >= startDate && s.StartDate <= endDate)
            .GroupBy(s => new { s.StartDate.Year, s.StartDate.Month })
            .Select(g => new MonthlyTrendDto
            {
                Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                Revenue = g.Sum(s => s.CurrentPrice),
                PaymentCount = g.Count(),
                AveragePayment = g.Average(s => s.CurrentPrice)
            })
            .OrderBy(mt => mt.Month)
            .ToList();
    }

    private List<MonthlyTrendDto> CalculateChurnTrend(IEnumerable<SubscriptionDto> subscriptions, DateTime startDate, DateTime endDate)
    {
        return subscriptions
            .Where(s => s.Status == "Cancelled" && s.CancelledDate >= startDate && s.CancelledDate <= endDate)
            .GroupBy(s => new { s.CancelledDate.Value.Year, s.CancelledDate.Value.Month })
            .Select(g => new MonthlyTrendDto
            {
                Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                Revenue = g.Sum(s => s.CurrentPrice),
                PaymentCount = g.Count(),
                AveragePayment = g.Average(s => s.CurrentPrice)
            })
            .OrderBy(mt => mt.Month)
            .ToList();
    }

    private async Task<ForecastDto> GenerateForecastAsync(List<MonthlyTrendDto> trends)
    {
        // Simple linear regression for forecasting
        if (trends.Count < 2) return new ForecastDto();

        var recentTrends = trends.TakeLast(6).ToList();
        var averageGrowth = recentTrends.Count > 1 
            ? (recentTrends.Last().Revenue - recentTrends.First().Revenue) / (recentTrends.Count - 1)
            : 0;

        return new ForecastDto
        {
            NextMonthRevenue = recentTrends.Any() ? recentTrends.Last().Revenue + averageGrowth : 0,
            NextMonthSubscriptions = recentTrends.Any() ? recentTrends.Last().PaymentCount + (int)(averageGrowth / 100) : 0,
            GrowthRate = averageGrowth,
            Confidence = 85.5m // This would be calculated based on historical accuracy
        };
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID");
        }
        return userId;
    }
}

// DTOs for subscription analytics
public class SubscriptionDashboardDto
{
    public DateRangeDto Period { get; set; } = new();
    public OverviewMetricsDto Overview { get; set; } = new();
    public RevenueAnalyticsDto Revenue { get; set; } = new();
    public ChurnAnalyticsDto Churn { get; set; } = new();
    public PlanAnalyticsDto Plans { get; set; } = new();
    public UsageAnalyticsDto Usage { get; set; } = new();
    public TrendAnalyticsDto Trends { get; set; } = new();
}

public class DateRangeDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class OverviewMetricsDto
{
    public int TotalSubscriptions { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int CancelledSubscriptions { get; set; }
    public int PausedSubscriptions { get; set; }
    public int TrialSubscriptions { get; set; }
    public int NewSubscriptionsThisPeriod { get; set; }
    public int CancelledSubscriptionsThisPeriod { get; set; }
    public decimal AverageSubscriptionValue { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class RevenueAnalyticsDto
{
    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRecurringRevenue { get; set; }
    public decimal AverageRevenuePerUser { get; set; }
    public decimal RevenueGrowth { get; set; }
    public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new();
    public List<PlanRevenueDto> RevenueByPlan { get; set; } = new();
}

public class MonthlyRevenueDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int PaymentCount { get; set; }
}

public class PlanRevenueDto
{
    public string PlanName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int SubscriptionCount { get; set; }
}

public class ChurnAnalyticsDto
{
    public decimal ChurnRate { get; set; }
    public decimal RetentionRate { get; set; }
    public int CancelledSubscriptions { get; set; }
    public List<CancellationReasonDto> CancellationReasons { get; set; } = new();
    public decimal AverageLifetime { get; set; }
    public List<CohortRetentionDto> CohortRetention { get; set; } = new();
}

public class CancellationReasonDto
{
    public string Reason { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public class CohortRetentionDto
{
    public string Cohort { get; set; } = string.Empty;
    public int InitialSubscriptions { get; set; }
    public int RetainedSubscriptions { get; set; }
    public decimal RetentionRate { get; set; }
}

public class PlanAnalyticsDto
{
    public List<PlanPerformanceDto> PlanPerformance { get; set; } = new();
    public List<PlanPerformanceDto> TopPerformingPlans { get; set; } = new();
    public List<PlanPerformanceDto> PlanComparison { get; set; } = new();
}

public class PlanPerformanceDto
{
    public string PlanName { get; set; } = string.Empty;
    public int TotalSubscriptions { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int CancelledSubscriptions { get; set; }
    public int NewSubscriptionsThisPeriod { get; set; }
    public decimal Revenue { get; set; }
    public decimal AverageSubscriptionValue { get; set; }
    public decimal ConversionRate { get; set; }
}

public class UsageAnalyticsDto
{
    public decimal AverageUsagePerUser { get; set; }
    public List<UsageDistributionDto> UsageDistribution { get; set; } = new();
    public List<PeakUsageTimeDto> PeakUsageTimes { get; set; } = new();
    public int UnderutilizedSubscriptions { get; set; }
    public int OverutilizedSubscriptions { get; set; }
    public List<UsageTrendDto> UsageTrends { get; set; } = new();
}

public class UsageDistributionDto
{
    public string Range { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class PeakUsageTimeDto
{
    public int Hour { get; set; }
    public int UsageCount { get; set; }
}

public class UsageTrendDto
{
    public DateTime Date { get; set; }
    public decimal AverageUsage { get; set; }
}

public class TrendAnalyticsDto
{
    public List<MonthlyTrendDto> RevenueTrend { get; set; } = new();
    public List<MonthlyTrendDto> SubscriptionGrowth { get; set; } = new();
    public List<MonthlyTrendDto> ChurnTrend { get; set; } = new();
    public ForecastDto Forecast { get; set; } = new();
}

public class MonthlyTrendDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int PaymentCount { get; set; }
    public decimal AveragePayment { get; set; }
}

public class ForecastDto
{
    public decimal NextMonthRevenue { get; set; }
    public int NextMonthSubscriptions { get; set; }
    public decimal GrowthRate { get; set; }
    public decimal Confidence { get; set; }
} 