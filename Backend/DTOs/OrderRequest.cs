using System.ComponentModel.DataAnnotations;

namespace Homebites.DTOs
{
    public class OrderRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int AddressId { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = string.Empty;

        public string? CustomerNotes { get; set; }

        [Required]
        public List<OrderItemRequest> Items { get; set; } = new();
    }

    public class OrderItemRequest
    {
        [Required]
        public int MealId { get; set; }

        [Range(1, 50)]
        public int Quantity { get; set; }
    }

    public class RefundRequestDto
    {
        public string? Reason { get; set; }
    }

    public class OrderRatingDto
    {
        public int Rating { get; set; }
        public string? Review { get; set; }
    }

    public class OrderHistoryDto
    {
        public long Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string? CustomerNotes { get; set; }
        public string? CancellationReason { get; set; }
        public List<OrderItemHistoryDto> Items { get; set; } = new();
    }

    public class OrderItemHistoryDto
    {
        public string MealName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal ItemTotal { get; set; }
        public bool IsVeg { get; set; }
    }

    public class AdminOrderDto
    {
        public long Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserMobile { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string? CancellationReason { get; set; }
        public DateTime OrderDate { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public string MealName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal ItemTotal { get; set; }
    }

    public class AdminStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingOrders { get; set; }
    }
}
