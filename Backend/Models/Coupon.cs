using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homebites.Models
{
    public class Coupon
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string DiscountType  { get; set; } = "Percentage"; // Percentage | Fixed

        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountValue  { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal MinOrderAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? MaxDiscount   { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(100)]
        public string? CreatedBy { get; set; } = "Admin";

        [MaxLength(100)]
        public string? UpdatedBy { get; set; }

        public DateTime  CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
