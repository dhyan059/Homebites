namespace Homebites.DTOs
{
    public class PaymentRequest
    {
        public int OrderId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal Amount { get; set; }

        public string? CardHolderName { get; set; }
        public string? CardNumber { get; set; }
        public string? ExpiryMonth { get; set; }
        public string? ExpiryYear { get; set; }
        public string? Cvv { get; set; }
    }
}
