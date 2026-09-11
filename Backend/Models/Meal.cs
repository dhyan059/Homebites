using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homebites.Models
{
    public class Meal
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }

        [Required, MaxLength(150)]
        public string MealName { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? DiscountPrice { get; set; }

        public string? ImageUrl { get; set; }
        public bool IsVeg        { get; set; } = false;
        public bool IsAvailable  { get; set; } = true;
        public int  PreparationMinutes { get; set; } = 20;

        public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}