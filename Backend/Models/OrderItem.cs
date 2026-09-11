using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homebites.Models
{
    public class OrderItem
    {
        public long Id      { get; set; }
        public long OrderId { get; set; }
        public int  MealId  { get; set; }

        [Required, MaxLength(150)]
        public string MealName { get; set; } = string.Empty;

        public bool IsVeg    { get; set; } = false;
        public int  Quantity { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice      { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal ItemTotal      { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Order? Order { get; set; }
        public Meal?  Meal  { get; set; }
    }
}