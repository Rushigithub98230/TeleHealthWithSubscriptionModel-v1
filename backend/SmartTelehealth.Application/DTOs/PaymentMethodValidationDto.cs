namespace SmartTelehealth.Application.DTOs
{
    public class PaymentMethodValidationDto
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? CardType { get; set; }
        public string? Last4Digits { get; set; }
    }
} 