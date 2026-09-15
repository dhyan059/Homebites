using Microsoft.AspNetCore.Mvc;
using Homebites.CQRS.Common;
using Homebites.CQRS.Command;
using Homebites.CQRS.Query;

namespace Homebites.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IDispatcher _dispatcher;

        public AdminController(IDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
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

        // GET api/admin/stats (Query)
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _dispatcher.QueryAsync(new GetAdminStatsQuery());
            return Ok(new { success = true, data = stats });
        }

        // GET api/admin/orders (Query)
        [HttpGet("orders")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _dispatcher.QueryAsync(new GetAllOrdersAdminQuery());
            return Ok(new { success = true, data = orders });
        }

        // PUT api/admin/orders/{id}/status (Command)
        [HttpPut("orders/{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(long id, [FromBody] OrderStatusDto dto)
        {
            var result = await _dispatcher.SendAsync(new UpdateOrderStatusCommand(id, dto.Status));
            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        // POST api/admin/orders/{id}/refund (Command)
        [HttpPost("orders/{id}/refund")]
        public async Task<IActionResult> ApproveRefund(long id, [FromBody] RefundDto dto)
        {
            var result = await _dispatcher.SendAsync(new ApproveRefundCommand(id, dto.Reason));
            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        // POST api/admin/orders/{id}/reject-refund (Command)
        [HttpPost("orders/{id}/reject-refund")]
        public async Task<IActionResult> RejectRefund(long id, [FromBody] RefundDto dto)
        {
            var result = await _dispatcher.SendAsync(new RejectRefundCommand(id, dto.Reason));
            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        // GET api/admin/users (Query)
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _dispatcher.QueryAsync(new GetAllUsersQuery());
            return Ok(new { success = true, data = users });
        }

        // PUT api/admin/users/{id}/toggle (Command)
        [HttpPut("users/{id}/toggle")]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            var result = await _dispatcher.SendAsync(new ToggleUserStatusCommand(id));
            if (!result.Success)
                return NotFound(new { success = false, message = "User not found." });

            return Ok(new { success = true, isActive = result.IsActive });
        }

        // GET api/admin/coupons (Query)
        [HttpGet("coupons")]
        public async Task<IActionResult> GetAllCoupons()
        {
            var coupons = await _dispatcher.QueryAsync(new GetAllCouponsQuery());
            return Ok(new { success = true, data = coupons });
        }

        // POST api/admin/coupons (Command)
        [HttpPost("coupons")]
        public async Task<IActionResult> CreateCoupon([FromBody] CreateCouponDto dto)
        {
            var result = await _dispatcher.SendAsync(new CreateCouponCommand(
                dto.Code, dto.DiscountType ?? "Percent", dto.DiscountValue, dto.MinOrderAmount, dto.MaxDiscount, dto.CreatedBy ?? "Admin"));

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        // PUT api/admin/coupons/{id}/toggle (Command)
        [HttpPut("coupons/{id}/toggle")]
        public async Task<IActionResult> ToggleCoupon(int id)
        {
            var result = await _dispatcher.SendAsync(new ToggleCouponCommand(id, "Admin"));
            if (!result.Success)
                return NotFound(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        // DELETE api/admin/coupons/{id} (Command)
        [HttpDelete("coupons/{id}")]
        public async Task<IActionResult> DeleteCoupon(int id)
        {
            var result = await _dispatcher.SendAsync(new DeleteCouponCommand(id));
            if (!result.Success)
                return NotFound(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }
    }

    public class AdminLoginDto    { public string? Email { get; set; } public string? Password { get; set; } }
    public class OrderStatusDto   { public string Status { get; set; } = ""; }
    public class RefundDto        { public string? Reason { get; set; } }
    public class CreateCouponDto
    {
        public string Code { get; set; } = "";
        public string? DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal MinOrderAmount { get; set; }
        public decimal? MaxDiscount { get; set; }
        public string? CreatedBy { get; set; }
    }
}
