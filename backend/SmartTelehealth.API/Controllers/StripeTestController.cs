using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.API.Controllers
{
    [ApiController]
    [Route("api/stripe")]
    public class StripeTestController : ControllerBase
    {
        private readonly ILogger<StripeTestController> _logger;

        public StripeTestController(ILogger<StripeTestController> logger)
        {
            _logger = logger;
        }

        [HttpPost("test-payment")]
        public ActionResult<JsonModel> TestPayment([FromBody] PaymentMethodRequest request)
        {
            _logger.LogInformation("Received Payment Method ID: {PaymentMethodId}", request.PaymentMethodId);

            // Simulate Stripe logic here (replace with your real service call)
            // For now, just log and return a dummy response
            var result = new
            {
                status = "received",
                paymentMethodId = request.PaymentMethodId
            };

            _logger.LogInformation("Stripe operation result: {Result}", result);

            return Ok(new JsonModel 
            { 
                data = result, 
                Message = "Payment method received successfully", 
                StatusCode = 200 
            });
        }
    }

    public class PaymentMethodRequest
    {
        public string PaymentMethodId { get; set; }
    }
} 