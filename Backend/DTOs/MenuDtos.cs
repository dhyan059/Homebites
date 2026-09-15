namespace Homebites.DTOs
{
    public class MealDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string MealName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsVeg { get; set; }
        public int PreparationMinutes { get; set; }
    }
}
