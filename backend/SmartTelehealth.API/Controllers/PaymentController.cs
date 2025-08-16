using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly IStripeService _stripeService;
    private readonly IBillingService _billingService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly ILogger<PaymentController> _logger;
    private readonly IAuditService _auditService;
    private readonly IPaymentSecurityService _paymentSecurityService;

    public PaymentController(
        IStripeService stripeService,
        IBillingService billingService,
        ISubscriptionService subscriptionService,
        ILogger<PaymentController> logger,
        IAuditService auditService,
        IPaymentSecurityService paymentSecurityService)
    {
        _stripeService = stripeService;
        _billingService = billingService;
        _subscriptionService = subscriptionService;
        _logger = logger;
        _auditService = auditService;
        _paymentSecurityService = paymentSecurityService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<JsonModel>> GetAllPayments()
    {
        try
        {
            var result = await GetPaymentHistory();
            return Ok(new JsonModel { data = result.Value, Message = "All payments retrieved successfully", StatusCode = 200 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all payments");
            return StatusCode(500, new JsonModel { data = new List<PaymentHistoryDto>(), Message = "Failed to retrieve payments", StatusCode = 500 });
        }
    }

    /// <summary>
    /// Get all payment methods for the current user
    /// </summary>
    [HttpGet("payment-methods")]
    public async Task<ActionResult<JsonModel>> GetPaymentMethods()
    {
        try
        {
            var userId = GetCurrentUserId();
            var paymentMethods = await _stripeService.GetCustomerPaymentMethodsAsync(userId.ToString());
            return Ok(new JsonModel { data = paymentMethods, Message = "Payment methods retrieved successfully", StatusCode = 200 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment methods for user");
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to retrieve payment methods", StatusCode = 500 });
        }
    }

    /// <summary>
    /// Add a new payment method for the current user
    /// </summary>
    [HttpPost("payment-methods")]
    public async Task<ActionResult<JsonModel>> AddPaymentMethod([FromBody] AddPaymentMethodDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Validate payment method
            var validationResult = await _stripeService.ValidatePaymentMethodAsync(request.PaymentMethodId);
            if (!validationResult)
            {
                return BadRequest(new JsonModel { data = new object(), Message = "Invalid payment method", StatusCode = 400 });
            }

            // Add payment method to customer
            var paymentMethodId = await _stripeService.AddPaymentMethodAsync(userId.ToString(), request.PaymentMethodId);
            
            // Get the payment method details
            var paymentMethods = await _stripeService.GetCustomerPaymentMethodsAsync(userId.ToString());
            var paymentMethod = paymentMethods.FirstOrDefault(pm => pm.Id == paymentMethodId);
            
            if (paymentMethod == null)
            {
                return BadRequest(new JsonModel { data = new object(), Message = "Failed to retrieve payment method details", StatusCode = 400 });
            }
            
            // Log the action
            await _auditService.LogUserActionAsync(userId.ToString(), "AddPaymentMethod", "PaymentMethod", paymentMethod.Id, "Payment method added successfully");
            
            return Ok(new JsonModel { data = paymentMethod, Message = "Payment method added successfully", StatusCode = 200 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding payment method for user");
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to add payment method", StatusCode = 500 });
        }
    }

    /// <summary>
    /// Set a payment method as default for the current user
    /// </summary>
    [HttpPut("payment-methods/{paymentMethodId}/default")]
    public async Task<ActionResult<JsonModel>> SetDefaultPaymentMethod(string paymentMethodId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _stripeService.SetDefaultPaymentMethodAsync(userId.ToString(), paymentMethodId);
            
            if (result)
            {
                await _auditService.LogUserActionAsync(userId.ToString(), "SetDefaultPaymentMethod", "PaymentMethod", paymentMethodId, "Default payment method updated");
                return Ok(new JsonModel { data = true, Message = "Default payment method updated", StatusCode = 200 });
            }
            
            return BadRequest(new JsonModel { data = new object(), Message = "Failed to set default payment method", StatusCode = 400 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default payment method for user");
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to set default payment method", StatusCode = 400 });
        }
    }

    /// <summary>
    /// Remove a payment method for the current user
    /// </summary>
    [HttpDelete("payment-methods/{paymentMethodId}")]
    public async Task<ActionResult<JsonModel>> RemovePaymentMethod(string paymentMethodId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _stripeService.RemovePaymentMethodAsync(userId.ToString(), paymentMethodId);
            
            if (result)
            {
                await _auditService.LogUserActionAsync(userId.ToString(), "RemovePaymentMethod", "PaymentMethod", paymentMethodId, "Payment method removed");
                return Ok(new JsonModel { data = true, Message = "Payment method removed", StatusCode = 200 });
            }
            
            return BadRequest(new JsonModel { data = new object(), Message = "Failed to remove payment method", StatusCode = 400 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing payment method for user");
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to remove payment method", StatusCode = 400 });
        }
    }

    /// <summary>
    /// Process a payment for a billing record
    /// </summary>
    [HttpPost("process-payment")]
    public async Task<ActionResult<JsonModel>> ProcessPayment([FromBody] ProcessPaymentRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var ipAddress = GetClientIpAddress();
            
            // Validate billing record exists and belongs to user
            var billingRecord = await _billingService.GetBillingRecordAsync(request.BillingRecordId);
            if (billingRecord.StatusCode != 200 || billingRecord.data == null)
            {
                return BadRequest(new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 400 });
            }

            if (((BillingRecordDto)billingRecord.data).UserId != userId.ToString())
            {
                await _auditService.LogSecurityEventAsync(userId.ToString(), "PaymentAccessDenied", 
                    $"User {userId} attempted to access billing record {request.BillingRecordId} belonging to {((BillingRecordDto)billingRecord.data).UserId}");
                return Forbid();
            }

            // Security validation
            if (!await _paymentSecurityService.ValidatePaymentRequestAsync(userId.ToString(), ipAddress, ((BillingRecordDto)billingRecord.data).Amount))
            {
                return BadRequest(new JsonModel { data = new object(), Message = "Payment request validation failed", StatusCode = 400 });
            }

            // Process payment
            var result = await _billingService.ProcessPaymentAsync(request.BillingRecordId);
            
            // Log payment attempt
            await _paymentSecurityService.LogPaymentAttemptAsync(
                userId.ToString(), 
                ipAddress, 
                ((BillingRecordDto)billingRecord.data).Amount, 
                result.StatusCode == 200, 
                result.StatusCode == 200 ? null : result.Message);
            
            if (result.StatusCode == 200)
            {
                await _auditService.LogPaymentEventAsync(userId.ToString(), "PaymentProcessed", request.BillingRecordId.ToString(), "Success");
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for billing record {BillingRecordId}", request.BillingRecordId);
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to process payment", StatusCode = 500 });
        }
    }

    /// <summary>
    /// Retry a failed payment
    /// </summary>
    [HttpPost("retry-payment/{billingRecordId}")]
    public async Task<ActionResult<JsonModel>> RetryPayment(Guid billingRecordId)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Validate billing record
            var billingRecord = await _billingService.GetBillingRecordAsync(billingRecordId);
            if (billingRecord.StatusCode != 200 || billingRecord.data == null)
            {
                return BadRequest(new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 400 });
            }

            if (((BillingRecordDto)billingRecord.data).UserId != userId.ToString())
            {
                return Forbid();
            }

            // Retry payment with exponential backoff
            var result = await _billingService.RetryPaymentAsync(billingRecordId);
            
            if (result.StatusCode == 200)
            {
                await _auditService.LogPaymentEventAsync(userId.ToString(), "PaymentRetried", billingRecordId.ToString(), "Success");
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying payment for billing record {BillingRecordId}", billingRecordId);
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to retry payment", StatusCode = 500 });
        }
    }

    /// <summary>
    /// Process a refund for a billing record
    /// </summary>
    [HttpPost("refund/{billingRecordId}")]
    public async Task<ActionResult<JsonModel>> ProcessRefund(Guid billingRecordId, [FromBody] RefundRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Validate billing record
            var billingRecord = await _billingService.GetBillingRecordAsync(billingRecordId);
            if (billingRecord.StatusCode != 200 || billingRecord.data == null)
            {
                return BadRequest(new JsonModel { data = new object(), Message = "Billing record not found", StatusCode = 400 });
            }

            if (((BillingRecordDto)billingRecord.data).UserId != userId.ToString())
            {
                return Forbid();
            }

            // Process refund
            var result = await _billingService.ProcessRefundAsync(billingRecordId, request.Amount, request.Reason);
            
            if (result.StatusCode == 200)
            {
                await _auditService.LogPaymentEventAsync(userId.ToString(), "RefundProcessed", billingRecordId.ToString(), "Success", request.Reason);
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for billing record {BillingRecordId}", billingRecordId);
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to process refund", StatusCode = 500 });
        }
    }

    /// <summary>
    /// Get payment history for the current user
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult<JsonModel>> GetPaymentHistory([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var history = await _billingService.GetPaymentHistoryAsync(userId, startDate, endDate);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment history for user");
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to retrieve payment history", StatusCode = 500 });
        }
    }

    /// <summary>
    /// Validate a payment method
    /// </summary>
    [HttpPost("validate-payment-method")]
    public async Task<ActionResult<JsonModel>> ValidatePaymentMethod([FromBody] ValidatePaymentMethodDto request)
    {
        try
        {
            var validationResult = await _stripeService.ValidatePaymentMethodDetailedAsync(request.PaymentMethodId);
            return Ok(new JsonModel { data = validationResult, Message = "Payment method validation completed", StatusCode = 200 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating payment method");
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to validate payment method", StatusCode = 500 });
        }
    }

    /// <summary>
    /// Get payment analytics for the current user
    /// </summary>
    [HttpGet("analytics")]
    public async Task<ActionResult<JsonModel>> GetPaymentAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var analytics = await _billingService.GetPaymentAnalyticsAsync(userId, startDate, endDate);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment analytics for user");
            return StatusCode(500, new JsonModel { data = new object(), Message = "Failed to retrieve payment analytics", StatusCode = 500 });
        }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID");
        }
        return userId;
    }

    private string GetClientIpAddress()
    {
        var forwarded = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwarded))
        {
            return forwarded.Split(',')[0].Trim();
        }
        
        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        return remoteIp ?? "unknown";
    }
}

// DTOs for Payment Controller
public class AddPaymentMethodDto
{
    public string PaymentMethodId { get; set; } = string.Empty;
}

public class ProcessPaymentRequestDto
{
    public Guid BillingRecordId { get; set; }
    public string? PaymentMethodId { get; set; }
}

public class RefundRequestDto
{
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class ValidatePaymentMethodDto
{
    public string PaymentMethodId { get; set; } = string.Empty;
}

public class PaymentMethodValidationDto
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? CardType { get; set; }
    public string? Last4Digits { get; set; }
}

public class PaymentHistoryDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? PaymentMethodId { get; set; }
}

public class PaymentAnalyticsDto
{
    public decimal TotalSpent { get; set; }
    public int TotalPayments { get; set; }
    public int SuccessfulPayments { get; set; }
    public int FailedPayments { get; set; }
    public decimal AveragePaymentAmount { get; set; }
    public List<MonthlyPaymentDto> MonthlyPayments { get; set; } = new();
}

public class MonthlyPaymentDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Count { get; set; }
} 