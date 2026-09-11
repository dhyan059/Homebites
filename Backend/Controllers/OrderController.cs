using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Homebites.Data;
using Homebites.Models;
using Homebites.DTOs;

namespace Homebites.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly HomebitesDbContext _context;

        public OrderController(HomebitesDbContext context)
        {
            _context = context;
        }

        // POST: api/order
        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] OrderRequest request)
        {
            if (request.Items == null || !request.Items.Any())
            {
                return BadRequest(new { success = false, message = "Cart is empty. Please add items to order." });
            }

            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found." });
            }

            var address = await _context.UserAddresses.FindAsync(request.AddressId);
            if (address == null || address.UserId != request.UserId)
            {
                return BadRequest(new { success = false, message = "Invalid delivery address selected." });
            }

            // Calculate item totals from DB directly to prevent frontend tampering
            var mealIds = request.Items.Select(i => i.MealId).ToList();
            var dbMeals = await _context.Meals.Where(m => mealIds.Contains(m.Id)).ToListAsync();

            decimal subTotal = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in request.Items)
            {
                var meal = dbMeals.FirstOrDefault(m => m.Id == item.MealId);
                if (meal == null) continue;

                decimal effectivePrice = (meal.DiscountPrice.HasValue && meal.DiscountPrice.Value > 0)
                    ? meal.DiscountPrice.Value
                    : meal.Price;

                decimal itemTotal = effectivePrice * item.Quantity;
                subTotal += itemTotal;

                orderItems.Add(new OrderItem
                {
                    MealId = meal.Id,
                    MealName = meal.MealName,
                    IsVeg = meal.IsVeg,
                    Quantity = item.Quantity,
                    UnitPrice = meal.Price,
                    DiscountAmount = meal.DiscountPrice.HasValue ? (meal.Price - meal.DiscountPrice.Value) : 0,
                    ItemTotal = itemTotal,
                    CreatedAt = DateTime.UtcNow
                });
            }

            decimal deliveryCharge = subTotal >= 300 ? 0 : 40.00m;
            decimal taxAmount = Math.Round(subTotal * 0.05m, 2); // 5% GST
            decimal totalAmount = subTotal + deliveryCharge + taxAmount;

            string orderNumber = "HB" + DateTime.UtcNow.ToString("yyMMddHHmmss") + new Random().Next(100, 999);

            var order = new Order
            {
                OrderNumber = orderNumber,
                UserId = user.Id,
                AddressId = address.Id,
                OrderDate = DateTime.UtcNow,
                SubTotal = subTotal,
                DeliveryCharge = deliveryCharge,
                DiscountAmount = 0,
                TaxAmount = taxAmount,
                TotalAmount = totalAmount,
                OrderStatus = "Accepted",
                PaymentStatus = (request.PaymentMethod == "CashOnDelivery") ? "Pending" : "Paid",
                PaymentMethod = request.PaymentMethod,
                CustomerNotes = request.CustomerNotes,
                EstimatedTime = DateTime.UtcNow.AddMinutes(35),
                CreatedAt = DateTime.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Link items with OrderId
            foreach (var oi in orderItems)
            {
                oi.OrderId = order.Id;
                _context.OrderItems.Add(oi);
            }

            // Create Payment record
            var payment = new Payment
            {
                OrderId = order.Id,
                PaymentMethod = request.PaymentMethod,
                Amount = totalAmount,
                PaymentStatus = order.PaymentStatus,
                TransactionReference = "TXN" + Guid.NewGuid().ToString("N")[..12].ToUpper(),
                PaymentDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            _context.Payments.Add(payment);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Order placed successfully!",
                orderId = order.Id,
                orderNumber = order.OrderNumber,
                totalAmount = order.TotalAmount,
                estimatedDeliveryMinutes = 35
            });
        }

        // GET: api/order/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserOrders(int userId)
        {
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new
                {
                    o.Id,
                    o.OrderNumber,
                    o.OrderDate,
                    o.SubTotal,
                    o.DeliveryCharge,
                    o.TaxAmount,
                    o.TotalAmount,
                    o.OrderStatus,
                    o.PaymentStatus,
                    o.PaymentMethod,
                    o.CustomerNotes,
                    o.CancellationReason,
                    o.EstimatedTime,
                    Items = _context.OrderItems
                        .Where(oi => oi.OrderId == o.Id)
                        .Select(oi => new
                        {
                            oi.MealName,
                            oi.IsVeg,
                            oi.Quantity,
                            oi.ItemTotal
                        }).ToList()
                })
                .ToListAsync();

            return Ok(new { success = true, data = orders });
        }

        // POST: api/order/{id}/request-refund
        [HttpPost("{id}/request-refund")]
        public async Task<IActionResult> RequestRefund(long id, [FromBody] RefundRequestDto request)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound(new { success = false, message = "Order not found." });

            if (order.OrderStatus != "Delivered")
                return BadRequest(new { success = false, message = "Refund requests are available only after successful delivery." });

            if (order.OrderStatus == "Refunded")
                return BadRequest(new { success = false, message = "This order has already been refunded." });

            if (order.OrderStatus == "Refund Requested")
                return BadRequest(new { success = false, message = "A refund request is already under review for this order." });

            order.OrderStatus = "Refund Requested";
            order.CancellationReason = string.IsNullOrWhiteSpace(request?.Reason)
                ? "Customer requested refund"
                : request.Reason.Trim();
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Refund request submitted successfully. Our kitchen admin team will review it shortly."
            });
        }

        // POST: api/order/{id}/rate
        [HttpPost("{id}/rate")]
        public async Task<IActionResult> RateOrder(long id, [FromBody] OrderRatingDto request)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound(new { success = false, message = "Order not found." });

            order.CustomerNotes = (order.CustomerNotes ?? "") + $" [Rating: {request.Rating}★ - {request.Review ?? "No review"}]";
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Thank you for rating your order!" });
        }
    }
}
