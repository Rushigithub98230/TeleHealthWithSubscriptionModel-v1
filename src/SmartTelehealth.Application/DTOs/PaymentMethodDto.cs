public class AddPaymentMethodDto
{
    public string PaymentMethodId { get; set; } = string.Empty;
    public string PaymentMethodType { get; set; } = "Card";
    public string? CardLast4 { get; set; }
    public string? CardBrand { get; set; }
    public int? ExpiryMonth { get; set; }
    public int? ExpiryYear { get; set; }
    public string? CardholderName { get; set; }
    public bool IsDefault { get; set; } = false;
} 