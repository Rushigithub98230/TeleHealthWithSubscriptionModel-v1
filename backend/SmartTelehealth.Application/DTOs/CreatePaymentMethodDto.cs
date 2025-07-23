public class CreatePaymentMethodDto
{
    public string UserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
} 