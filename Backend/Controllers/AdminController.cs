using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Homebites.Data;
using Homebites.Models;
using System.Security.Cryptography;
using System.Text;

namespace Homebites.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly HomebitesDbContext _context;

        public AdminController(HomebitesDbContext context) { _context = context; }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "_homebites_salt"));
            return Convert.ToBase64String(bytes);
        }

        // POST api/admin/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] AdminLoginDto dto)
        {
            if (dto.Email?.Trim().ToLower() == "admin@bites.in" &&
                dto.Password == "Dhyan@2003")
            {
                return Ok(new {
                    success = true,
                    admin = new { email = "admin@bites.in", name = "HomeBites Admin", role = "Admin" }
                });
            }
            return Unauthorized(new { success = false, message = "Invalid admin credentials." });
        }

        // GET api/admin/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var totalUsers   = await _context.Users.CountAsync(u => u.Role == "Customer");
            var totalOrders  = await _context.Orders.CountAsync();
            var totalRevenue = await _context.Orders.SumAsync(o => (decimal?)o.TotalAmount) ?? 0;
            var pendingOrders = await _context.Orders.CountAsync(o =>
                o.OrderStatus == "Accepted" || o.OrderStatus == "Preparing");

            return Ok(new { success = true, data = new {
                totalUsers, totalOrders,
                totalRevenue = Math.Round(totalRevenue, 2),
                pendingOrders
            }});
        }

        // GET api/admin/orders
        [HttpGet("orders")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.Orders
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new {
                    o.Id, o.OrderNumber, o.OrderDate,
                    o.SubTotal, o.DeliveryCharge, o.TaxAmount, o.TotalAmount,
                    o.OrderStatus, o.PaymentStatus, o.PaymentMethod,
                    o.CustomerNotes, o.CancellationReason,
                    o.EstimatedTime, o.CreatedAt,
                    o.UserId,
                    UserName = _context.Users.Where(u => u.Id == o.UserId).Select(u => u.FullName).FirstOrDefault(),
                    UserEmail = _context.Users.Where(u => u.Id == o.UserId).Select(u => u.Email).FirstOrDefault(),
                    UserMobile = _context.Users.Where(u => u.Id == o.UserId).Select(u => u.Mobile).FirstOrDefault(),
                    Items = _context.OrderItems.Where(oi => oi.OrderId == o.Id)
                        .Select(oi => new { oi.MealName, oi.Quantity, oi.ItemTotal, oi.IsVeg }).ToList()
                })
                .ToListAsync();

            return Ok(new { success = true, data = orders });
        }

        // PUT api/admin/orders/{id}/status
        [HttpPut("orders/{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(long id, [FromBody] OrderStatusDto dto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound(new { success = false, message = "Order not found." });

            if (dto.Status == "Cancelled" && order.OrderStatus == "Delivered")
                return BadRequest(new { success = false, message = "A successfully delivered order cannot be cancelled." });

            var validStatuses = new[] { "Accepted", "Preparing", "Ready", "PickedUp", "Delivered", "Cancelled", "Refund Requested", "Refunded" };
            if (!validStatuses.Contains(dto.Status))
                return BadRequest(new { success = false, message = "Invalid status value." });

            order.OrderStatus = dto.Status;
            order.UpdatedAt = DateTime.UtcNow;
            if (dto.Status == "Delivered") order.CompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = $"Order {order.OrderNumber} updated to {dto.Status}." });
        }

        // POST api/admin/orders/{id}/refund
        [HttpPost("orders/{id}/refund")]
        public async Task<IActionResult> ApproveRefund(long id, [FromBody] RefundDto dto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound(new { success = false, message = "Order not found." });

            if (order.OrderStatus != "Refund Requested")
                return BadRequest(new { success = false, message = "Only a pending refund request can be approved." });

            order.OrderStatus = "Refunded";
            order.PaymentStatus = "Refunded";
            order.CancellationReason = dto.Reason ?? "Refund approved by admin";
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = $"Refund approved for order {order.OrderNumber}." });
        }

        // POST api/admin/orders/{id}/reject-refund
        [HttpPost("orders/{id}/reject-refund")]
        public async Task<IActionResult> RejectRefund(long id, [FromBody] RefundDto dto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound(new { success = false, message = "Order not found." });

            order.OrderStatus = "Delivered";
            order.CancellationReason = dto.Reason ?? "Refund request declined by Admin";
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = $"Refund request for order {order.OrderNumber} was declined." });
        }

        // GET api/admin/users
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new {
                    u.Id, u.FullName, u.Email, u.Mobile, u.Role,
                    u.IsActive, u.IsEmailVerified, u.IsMobileVerified,
                    u.LastLoginAt, u.CreatedAt,
                    OrderCount = _context.Orders.Count(o => o.UserId == u.Id)
                }).ToListAsync();

            return Ok(new { success = true, data = users });
        }

        // PUT api/admin/users/{id}/toggle
        [HttpPut("users/{id}/toggle")]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { success = false, message = "User not found." });
            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { success = true, isActive = user.IsActive });
        }
    }

    public class AdminLoginDto    { public string? Email { get; set; } public string? Password { get; set; } }
    public class OrderStatusDto   { public string Status { get; set; } = ""; }
    public class RefundDto        { public string? Reason { get; set; } }
}
