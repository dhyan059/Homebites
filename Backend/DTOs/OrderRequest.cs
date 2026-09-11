namespace Homebites.DTOs
{
    public class RefundRequestDto
    {
        public string? Reason { get; set; }
    }

    public class OrderRatingDto
    {
        public int Rating { get; set; }
        public string? Review { get; set; }
    }
}
