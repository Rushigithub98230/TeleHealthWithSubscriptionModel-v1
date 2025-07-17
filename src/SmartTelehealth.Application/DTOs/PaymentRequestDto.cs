namespace SmartTelehealth.Application.DTOs
{
    public class PaymentRequestDto
    {
        public string PaymentMethodId { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }
} 