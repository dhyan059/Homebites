using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homebites.Models
{
    public class Payment
    {
        public long Id      { get; set; }
        public long OrderId { get; set; }

        public string? TransactionReference { get; set; }

        [Required, MaxLength(30)]
        public string PaymentMethod { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required, MaxLength(30)]
        public string PaymentStatus { get; set; } = "Pending";

        public DateTime? PaymentDate  { get; set; }
        public string?   FailureReason { get; set; }

        public DateTime? RefundedAt      { get; set; }
        public string?   RefundReference { get; set; }
        public string?   RefundReason    { get; set; }

        public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public Order? Order { get; set; }
    }
}