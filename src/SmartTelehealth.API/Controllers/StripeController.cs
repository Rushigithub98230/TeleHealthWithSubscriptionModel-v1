using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.Interfaces;
using System.Threading.Tasks;

namespace SmartTelehealth.API.Controllers
{
    [ApiController]
    [Route("api/stripe")]
    public class StripeController : ControllerBase
    {
        private readonly IStripeService _stripeService;

        public StripeController(IStripeService stripeService)
        {
            _stripeService = stripeService;
        }

        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CheckoutSessionRequest request)
        {
            // Use your actual Stripe test price ID here:
            var priceId = "price_12345"; // <-- Replace with your Stripe test price ID
            var successUrl = request.SuccessUrl;
            var cancelUrl = request.CancelUrl;
            var sessionUrl = await _stripeService.CreateCheckoutSessionAsync(priceId, successUrl, cancelUrl);
            return Ok(new { url = sessionUrl });
        }
    }

    public class CheckoutSessionRequest
    {
        public string SuccessUrl { get; set; }
        public string CancelUrl { get; set; }
    }
} 