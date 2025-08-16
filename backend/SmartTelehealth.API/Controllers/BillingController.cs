using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BillingController : ControllerBase
{
    private readonly IBillingService _billingService;
    private readonly IPdfService _pdfService;
    private readonly IUserService _userService;
    private readonly ISubscriptionService _subscriptionService;

    public BillingController(
        IBillingService billingService, 
        IPdfService pdfService,
        IUserService userService,
        ISubscriptionService subscriptionService)
    {
        _billingService = billingService;
        _pdfService = pdfService;
        _userService = userService;
        _subscriptionService = subscriptionService;
    }

    [HttpGet("records")]
    [AllowAnonymous]
    public async Task<ActionResult<JsonModel>> GetAllBillingRecords()
    {
        try
        {
            // For admin users, get all records; for regular users, get only their records
            if (User.IsInRole("Admin") || User.IsInRole("Superadmin"))
            {
                var allRecords = await _billingService.GetAllBillingRecordsAsync();
                return Ok(new JsonModel { data = allRecords.data, Message = "All billing records retrieved successfully", StatusCode = 200 });
            }
            else
            {
                var userId = GetCurrentUserId();
                var userRecords = await _billingService.GetUserBillingHistoryAsync(userId);
                return Ok(new JsonModel { data = userRecords.data, Message = "User billing records retrieved successfully", StatusCode = 200 });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new JsonModel { data = new List<BillingRecordDto>(), Message = "Failed to retrieve billing records", StatusCode = 500 });
        }
    }

    /// <summary>
    /// Download invoice PDF for a billing record
    /// </summary>
    [HttpGet("{id}/invoice-pdf")]
    public async Task<ActionResult<JsonModel>> DownloadInvoicePdf(Guid id)
    {
        try
        {
            var billingRecordResponse = await _billingService.GetBillingRecordAsync(id);
            var billingRecord = (BillingRecordDto)billingRecordResponse.data;
            if (billingRecord == null)
                return NotFound(billingRecordResponse);
            var userId = GetCurrentUserId();
            if (billingRecord.UserId != userId.ToString() && !User.IsInRole("Admin"))
            {
                return Forbid();
            }
            var userResponse = await _userService.GetUserAsync(userId);
            if (userResponse.StatusCode != 200 || userResponse.data == null)
            {
                return NotFound(new JsonModel { data = new object(), Message = "User not found", StatusCode = 404 });
            }
            SubscriptionDto? subscription = null;
            if (!string.IsNullOrEmpty(billingRecord.SubscriptionId))
            {
                try
                {
                    var subscriptionResponse = await _subscriptionService.GetSubscriptionAsync(billingRecord.SubscriptionId);
                    subscription = (SubscriptionDto)subscriptionResponse.data;
                }
                catch
                {
                    // Subscription not found, continue without it
                }
            }
            var pdfBytes = await _pdfService.GenerateInvoicePdfAsync(billingRecord, (UserDto)userResponse.data, subscription);
            return Ok(new JsonModel { data = new { fileData = pdfBytes, fileName = $"invoice-{billingRecord.Id}.pdf", contentType = "application/pdf" }, Message = "Invoice PDF generated successfully", StatusCode = 200 });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new JsonModel
            {
                data = new object(),
                Message = ex.Message,
                StatusCode = 404
            });
        }
    }

    /// <summary>
    /// Download billing history PDF
    /// </summary>
    [HttpGet("history-pdf")]
    public async Task<ActionResult<JsonModel>> DownloadBillingHistoryPdf()
    {
        var userId = GetCurrentUserId();
        var billingHistoryResponse = await _billingService.GetUserBillingHistoryAsync(userId);
        var billingHistory = (IEnumerable<BillingRecordDto>)(billingHistoryResponse.data ?? new List<BillingRecordDto>());
        var userResponse = await _userService.GetUserAsync(userId);
        if (userResponse.StatusCode != 200 || userResponse.data == null)
        {
            return NotFound(new JsonModel { data = new object(), Message = "User not found", StatusCode = 404 });
        }
        var pdfBytes = await _pdfService.GenerateBillingHistoryPdfAsync(billingHistory, (UserDto)userResponse.data);
        return Ok(new JsonModel { data = new { fileData = pdfBytes, fileName = $"billing-history-{userId}-{DateTime.Now:yyyyMMdd}.pdf", contentType = "application/pdf" }, Message = "Billing history PDF generated successfully", StatusCode = 200 });
    }

    /// <summary>
    /// Download subscription summary PDF
    /// </summary>
    [HttpGet("subscription/{subscriptionId}/summary-pdf")]
    public async Task<ActionResult<JsonModel>> DownloadSubscriptionSummaryPdf(Guid subscriptionId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var subscriptionResponse = await _subscriptionService.GetSubscriptionAsync(subscriptionId.ToString());
            var subscription = (SubscriptionDto)subscriptionResponse.data;
            if (subscription == null)
                return NotFound(subscriptionResponse);
            if (subscription.UserId != userId.ToString() && !User.IsInRole("Admin"))
            {
                return Forbid();
            }
            var userResponse = await _userService.GetUserAsync(userId);
            if (userResponse.StatusCode != 200 || userResponse.data == null)
            {
                return NotFound(new JsonModel { data = new object(), Message = "User not found", StatusCode = 404 });
            }
            var pdfBytes = await _pdfService.GenerateSubscriptionSummaryPdfAsync(subscription, (UserDto)userResponse.data);
            return Ok(new JsonModel { data = new { fileData = pdfBytes, fileName = $"subscription-summary-{subscriptionId}-{DateTime.Now:yyyyMMdd}.pdf", contentType = "application/pdf" }, Message = "Subscription summary PDF generated successfully", StatusCode = 200 });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new JsonModel
            {
                data = new object(),
                Message = ex.Message,
                StatusCode = 404
            });
        }
    }

    /// <summary>
    /// Get user's billing history
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult<JsonModel>> GetBillingHistory()
    {
        var userId = GetCurrentUserId();
        var response = await _billingService.GetUserBillingHistoryAsync(userId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get specific billing record
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<JsonModel>> GetBillingRecord(Guid id)
    {
        var response = await _billingService.GetBillingRecordAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Process payment for a billing record
    /// </summary>
    [HttpPost("{id}/process-payment")]
    public async Task<ActionResult<JsonModel>> ProcessPayment(Guid id)
    {
        var response = await _billingService.ProcessPaymentAsync(id);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Process refund for a billing record
    /// </summary>
    [HttpPost("{id}/refund")]
    public async Task<ActionResult<JsonModel>> ProcessRefund(Guid id, [FromBody] RefundRequestDto refundRequest)
    {
        var response = await _billingService.ProcessRefundAsync(id, refundRequest.Amount);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get subscription billing history
    /// </summary>
    [HttpGet("subscription/{subscriptionId}")]
    public async Task<ActionResult<JsonModel>> GetSubscriptionBillingHistory(Guid subscriptionId)
    {
        var response = await _billingService.GetSubscriptionBillingHistoryAsync(subscriptionId);
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Calculate total amount with tax and shipping
    /// </summary>
    [HttpPost("calculate-total")]
    public async Task<ActionResult<JsonModel>> CalculateTotal([FromBody] BillingCalculationRequestDto request)
    {
        var taxAmountResponse = await _billingService.CalculateTaxAmountAsync(request.BaseAmount, request.State);
        var taxAmount = (decimal)taxAmountResponse.data;
        var shippingAmountResponse = await _billingService.CalculateShippingAmountAsync(request.DeliveryAddress, request.IsExpress);
        var shippingAmount = (decimal)shippingAmountResponse.data;
        var totalAmountResponse = await _billingService.CalculateTotalAmountAsync(request.BaseAmount, taxAmount, shippingAmount);
        var totalAmount = (decimal)totalAmountResponse.data;
        var result = new BillingCalculationDto
        {
            BaseAmount = request.BaseAmount,
            TaxAmount = taxAmount,
            ShippingAmount = shippingAmount,
            TotalAmount = totalAmount
        };
                    return Ok(new JsonModel
            {
                data = result,
                Message = "Total calculated successfully",
                StatusCode = 200
            });
    }

    /// <summary>
    /// Check if a billing record is overdue
    /// </summary>
    [HttpGet("{id}/overdue-status")]
    public async Task<ActionResult<JsonModel>> CheckOverdueStatus(Guid id)
    {
        try
        {
            var isOverdueResponse = await _billingService.IsPaymentOverdueAsync(id);
            var isOverdue = (bool)isOverdueResponse.data;
            var billingRecordResponse = await _billingService.GetBillingRecordAsync(id);
            var billingRecord = (BillingRecordDto)billingRecordResponse.data;
            if (billingRecord == null)
                return NotFound(billingRecordResponse);
            var overdueStatus = new OverdueStatusDto
            {
                BillingRecordId = id,
                IsOverdue = isOverdue,
                DueDate = billingRecord.DueDate,
                DaysOverdue = Math.Max(0, (int)((billingRecord.DueDate.HasValue ? (DateTime.UtcNow - billingRecord.DueDate.Value) : TimeSpan.Zero).TotalDays))
            };
            return Ok(new JsonModel
            {
                data = overdueStatus,
                Message = "Overdue status checked successfully",
                StatusCode = 200
            });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new JsonModel
            {
                data = new object(),
                Message = ex.Message,
                StatusCode = 404
            });
        }
    }

    /// <summary>
    /// Get all pending payments (Admin only)
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<JsonModel>> GetPendingPayments()
    {
        var response = await _billingService.GetPendingPaymentsAsync();
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get all overdue billing records (Admin only)
    /// </summary>
    [HttpGet("overdue")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<JsonModel>> GetOverdueBillingRecords()
    {
        var response = await _billingService.GetOverdueBillingRecordsAsync();
        return StatusCode(response.StatusCode, response);
    }

    /// <summary>
    /// Get revenue summary for admin reporting (accrual and cash)
    /// </summary>
    [HttpGet("revenue-summary")]
    [Authorize(Roles = "Admin,Superadmin")]
    public async Task<ActionResult<JsonModel>> GetRevenueSummary([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] string? planId = null)
    {
        var result = await _billingService.GetRevenueSummaryAsync(from, to, planId);
        return Ok(result);
    }

    /// <summary>
    /// Export revenue/financial data for admin (CSV/Excel)
    /// </summary>
    [HttpGet("export-revenue")]
    [Authorize(Roles = "Admin,Superadmin")]
    public async Task<ActionResult<JsonModel>> ExportRevenue([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] string? planId = null, [FromQuery] string format = "csv")
    {
        var result = await _billingService.ExportRevenueAsync(from, to, planId, format);
        var fileName = $"revenue-export-{DateTime.UtcNow:yyyyMMddHHmmss}.{format}";
        return Ok(new JsonModel { data = new { fileData = result.data, fileName = fileName, contentType = "text/csv" }, Message = "Revenue export generated successfully", StatusCode = 200 });
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            throw new InvalidOperationException("User ID not found in claims");
        }
        return userId;
    }
}

public class BillingCalculationRequestDto
{
    public decimal BaseAmount { get; set; }
    public string State { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public bool IsExpress { get; set; }
}

public class BillingCalculationDto
{
    public decimal BaseAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TotalAmount { get; set; }
}

public class OverdueStatusDto
{
    public Guid BillingRecordId { get; set; }
    public bool IsOverdue { get; set; }
    public DateTime? DueDate { get; set; }
    public int DaysOverdue { get; set; }
} 