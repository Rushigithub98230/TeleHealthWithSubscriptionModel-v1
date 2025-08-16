using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Application.DTOs;
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

        [HttpGet("test-connection")]
        public async Task<ActionResult<JsonModel>> TestConnection()
        {
            try
            {
                // Test Stripe connection by attempting to list customers
                var customers = await _stripeService.ListCustomersAsync();
                return Ok(new JsonModel 
                { 
                    data = new { customerCount = customers.Count() }, 
                    Message = "Stripe connection successful", 
                    StatusCode = 200 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new JsonModel 
                { 
                    data = new object(), 
                    Message = "Stripe connection failed", 
                    StatusCode = 400 
                });
            }
        }

        [HttpPost("create-checkout-session")]
        public async Task<ActionResult<JsonModel>> CreateCheckoutSession([FromBody] CheckoutSessionRequest request)
        {
            // Use your actual Stripe test price ID here:
            var priceId = "price_12345"; // <-- Replace with your Stripe test price ID
            var successUrl = request.SuccessUrl;
            var cancelUrl = request.CancelUrl;
            var sessionUrl = await _stripeService.CreateCheckoutSessionAsync(priceId, successUrl, cancelUrl);
            return Ok(new JsonModel 
            { 
                data = new { url = sessionUrl }, 
                Message = "Checkout session created successfully", 
                StatusCode = 200 
            });
        }
    }

    public class CheckoutSessionRequest
    {
        public string SuccessUrl { get; set; }
        public string CancelUrl { get; set; }
    }
} 