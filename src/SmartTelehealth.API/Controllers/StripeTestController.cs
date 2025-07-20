using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
        public IActionResult TestPayment([FromBody] PaymentMethodRequest request)
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

            return Ok(result);
        }
    }

    public class PaymentMethodRequest
    {
        public string PaymentMethodId { get; set; }
    }
} 