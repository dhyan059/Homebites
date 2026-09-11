using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homebites.Models
{
    public class Order
    {
        public long Id { get; set; }

        [Required, MaxLength(30)]
        public string OrderNumber { get; set; } = string.Empty;

        public int  UserId    { get; set; }
        public int  AddressId { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(10,2)")]
        public decimal SubTotal        { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal DeliveryCharge  { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountAmount  { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TaxAmount       { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount     { get; set; }

        [Required, MaxLength(30)]
        public string OrderStatus   { get; set; } = "Pending";

        [Required, MaxLength(30)]
        public string PaymentStatus { get; set; } = "Pending";

        [Required, MaxLength(30)]
        public string PaymentMethod { get; set; } = string.Empty;

        public string? CustomerNotes       { get; set; }
        public string? CancellationReason  { get; set; }

        public DateTime? EstimatedTime { get; set; }
        public DateTime? CompletedAt   { get; set; }

        public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public User?        User    { get; set; }
        public UserAddress? Address { get; set; }
    }
}