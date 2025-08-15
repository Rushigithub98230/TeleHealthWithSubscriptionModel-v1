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
    public async Task<ActionResult<ApiResponse<IEnumerable<PaymentHistoryDto>>>> GetAllPayments()
    {
        try
        {
            var result = await GetPaymentHistory();
            return Ok(new { data = result.Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all payments");
            return StatusCode(500, new { data = new List<PaymentHistoryDto>() });
        }
    }

    /// <summary>
    /// Get all payment methods for the current user
    /// </summary>
    [HttpGet("payment-methods")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PaymentMethodDto>>>> GetPaymentMethods()
    {
        try
        {
            var userId = GetCurrentUserId();
            var paymentMethods = await _stripeService.GetCustomerPaymentMethodsAsync(userId.ToString());
            return Ok(ApiResponse<IEnumerable<PaymentMethodDto>>.SuccessResponse(paymentMethods));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment methods for user");
            return StatusCode(500, ApiResponse<IEnumerable<PaymentMethodDto>>.ErrorResponse("Failed to retrieve payment methods"));
        }
    }

    /// <summary>
    /// Add a new payment method for the current user
    /// </summary>
    [HttpPost("payment-methods")]
    public async Task<ActionResult<ApiResponse<PaymentMethodDto>>> AddPaymentMethod([FromBody] AddPaymentMethodDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Validate payment method
            var validationResult = await _stripeService.ValidatePaymentMethodAsync(request.PaymentMethodId);
            if (!validationResult)
            {
                return BadRequest(ApiResponse<PaymentMethodDto>.ErrorResponse("Invalid payment method"));
            }

            // Add payment method to customer
            var paymentMethodId = await _stripeService.AddPaymentMethodAsync(userId.ToString(), request.PaymentMethodId);
            
            // Get the payment method details
            var paymentMethods = await _stripeService.GetCustomerPaymentMethodsAsync(userId.ToString());
            var paymentMethod = paymentMethods.FirstOrDefault(pm => pm.Id == paymentMethodId);
            
            if (paymentMethod == null)
            {
                return BadRequest(ApiResponse<PaymentMethodDto>.ErrorResponse("Failed to retrieve payment method details"));
            }
            
            // Log the action
            await _auditService.LogUserActionAsync(userId.ToString(), "AddPaymentMethod", "PaymentMethod", paymentMethod.Id, "Payment method added successfully");
            
            return Ok(ApiResponse<PaymentMethodDto>.SuccessResponse(paymentMethod, "Payment method added successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding payment method for user");
            return StatusCode(500, ApiResponse<PaymentMethodDto>.ErrorResponse("Failed to add payment method"));
        }
    }

    /// <summary>
    /// Set a payment method as default for the current user
    /// </summary>
    [HttpPut("payment-methods/{paymentMethodId}/default")]
    public async Task<ActionResult<ApiResponse<bool>>> SetDefaultPaymentMethod(string paymentMethodId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _stripeService.SetDefaultPaymentMethodAsync(userId.ToString(), paymentMethodId);
            
            if (result)
            {
                await _auditService.LogUserActionAsync(userId.ToString(), "SetDefaultPaymentMethod", "PaymentMethod", paymentMethodId, "Default payment method updated");
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Default payment method updated"));
            }
            
            return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to set default payment method"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default payment method for user");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to set default payment method"));
        }
    }

    /// <summary>
    /// Remove a payment method for the current user
    /// </summary>
    [HttpDelete("payment-methods/{paymentMethodId}")]
    public async Task<ActionResult<ApiResponse<bool>>> RemovePaymentMethod(string paymentMethodId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _stripeService.RemovePaymentMethodAsync(userId.ToString(), paymentMethodId);
            
            if (result)
            {
                await _auditService.LogUserActionAsync(userId.ToString(), "RemovePaymentMethod", "PaymentMethod", paymentMethodId, "Payment method removed");
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Payment method removed"));
            }
            
            return BadRequest(ApiResponse<bool>.ErrorResponse("Failed to remove payment method"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing payment method for user");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to remove payment method"));
        }
    }

    /// <summary>
    /// Process a payment for a billing record
    /// </summary>
    [HttpPost("process-payment")]
    public async Task<ActionResult<ApiResponse<PaymentResultDto>>> ProcessPayment([FromBody] ProcessPaymentRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var ipAddress = GetClientIpAddress();
            
            // Validate billing record exists and belongs to user
            var billingRecord = await _billingService.GetBillingRecordAsync(request.BillingRecordId);
            if (!billingRecord.Success || billingRecord.Data == null)
            {
                return BadRequest(ApiResponse<PaymentResultDto>.ErrorResponse("Billing record not found"));
            }

            if (billingRecord.Data.UserId != userId.ToString())
            {
                await _auditService.LogSecurityEventAsync(userId.ToString(), "PaymentAccessDenied", 
                    $"User {userId} attempted to access billing record {request.BillingRecordId} belonging to {billingRecord.Data.UserId}");
                return Forbid();
            }

            // Security validation
            if (!await _paymentSecurityService.ValidatePaymentRequestAsync(userId.ToString(), ipAddress, billingRecord.Data.Amount))
            {
                return BadRequest(ApiResponse<PaymentResultDto>.ErrorResponse("Payment request validation failed"));
            }

            // Process payment
            var result = await _billingService.ProcessPaymentAsync(request.BillingRecordId);
            
            // Log payment attempt
            await _paymentSecurityService.LogPaymentAttemptAsync(
                userId.ToString(), 
                ipAddress, 
                billingRecord.Data.Amount, 
                result.Success, 
                result.Success ? null : result.Message);
            
            if (result.Success)
            {
                await _auditService.LogPaymentEventAsync(userId.ToString(), "PaymentProcessed", request.BillingRecordId.ToString(), "Success");
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for billing record {BillingRecordId}", request.BillingRecordId);
            return StatusCode(500, ApiResponse<PaymentResultDto>.ErrorResponse("Failed to process payment"));
        }
    }

    /// <summary>
    /// Retry a failed payment
    /// </summary>
    [HttpPost("retry-payment/{billingRecordId}")]
    public async Task<ActionResult<ApiResponse<PaymentResultDto>>> RetryPayment(Guid billingRecordId)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Validate billing record
            var billingRecord = await _billingService.GetBillingRecordAsync(billingRecordId);
            if (!billingRecord.Success || billingRecord.Data == null)
            {
                return BadRequest(ApiResponse<PaymentResultDto>.ErrorResponse("Billing record not found"));
            }

            if (billingRecord.Data.UserId != userId.ToString())
            {
                return Forbid();
            }

            // Retry payment with exponential backoff
            var result = await _billingService.RetryPaymentAsync(billingRecordId);
            
            if (result.Success)
            {
                await _auditService.LogPaymentEventAsync(userId.ToString(), "PaymentRetried", billingRecordId.ToString(), "Success");
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying payment for billing record {BillingRecordId}", billingRecordId);
            return StatusCode(500, ApiResponse<PaymentResultDto>.ErrorResponse("Failed to retry payment"));
        }
    }

    /// <summary>
    /// Process a refund for a billing record
    /// </summary>
    [HttpPost("refund/{billingRecordId}")]
    public async Task<ActionResult<ApiResponse<RefundResultDto>>> ProcessRefund(Guid billingRecordId, [FromBody] RefundRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Validate billing record
            var billingRecord = await _billingService.GetBillingRecordAsync(billingRecordId);
            if (!billingRecord.Success || billingRecord.Data == null)
            {
                return BadRequest(ApiResponse<RefundResultDto>.ErrorResponse("Billing record not found"));
            }

            if (billingRecord.Data.UserId != userId.ToString())
            {
                return Forbid();
            }

            // Process refund
            var result = await _billingService.ProcessRefundAsync(billingRecordId, request.Amount, request.Reason);
            
            if (result.Success)
            {
                await _auditService.LogPaymentEventAsync(userId.ToString(), "RefundProcessed", billingRecordId.ToString(), "Success", request.Reason);
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for billing record {BillingRecordId}", billingRecordId);
            return StatusCode(500, ApiResponse<RefundResultDto>.ErrorResponse("Failed to process refund"));
        }
    }

    /// <summary>
    /// Get payment history for the current user
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PaymentHistoryDto>>>> GetPaymentHistory([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
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
            return StatusCode(500, ApiResponse<IEnumerable<PaymentHistoryDto>>.ErrorResponse("Failed to retrieve payment history"));
        }
    }

    /// <summary>
    /// Validate a payment method
    /// </summary>
    [HttpPost("validate-payment-method")]
    public async Task<ActionResult<ApiResponse<SmartTelehealth.Application.DTOs.PaymentMethodValidationDto>>> ValidatePaymentMethod([FromBody] ValidatePaymentMethodDto request)
    {
        try
        {
            var validationResult = await _stripeService.ValidatePaymentMethodDetailedAsync(request.PaymentMethodId);
            return Ok(ApiResponse<SmartTelehealth.Application.DTOs.PaymentMethodValidationDto>.SuccessResponse(validationResult));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating payment method");
            return StatusCode(500, ApiResponse<SmartTelehealth.Application.DTOs.PaymentMethodValidationDto>.ErrorResponse("Failed to validate payment method"));
        }
    }

    /// <summary>
    /// Get payment analytics for the current user
    /// </summary>
    [HttpGet("analytics")]
    public async Task<ActionResult<ApiResponse<PaymentAnalyticsDto>>> GetPaymentAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
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
            return StatusCode(500, ApiResponse<PaymentAnalyticsDto>.ErrorResponse("Failed to retrieve payment analytics"));
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