using Microsoft.AspNetCore.Mvc;
using Homebites.CQRS.Common;
using Homebites.CQRS.Command;
using Homebites.CQRS.Query;
using Homebites.DTOs;

namespace Homebites.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IDispatcher _dispatcher;

        public OrderController(IDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        // POST: api/order (Command)
        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] OrderRequest request)
        {
            var result = await _dispatcher.SendAsync(new PlaceOrderCommand(request));
            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new
            {
                success = true,
                message = result.Message,
                orderId = result.OrderId,
                orderNumber = result.OrderNumber,
                totalAmount = result.TotalAmount,
                estimatedDeliveryMinutes = result.EstimatedDeliveryMinutes
            });
        }

        // GET: api/order/user/{userId} (Query)
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserOrders(int userId)
        {
            var orders = await _dispatcher.QueryAsync(new GetUserOrdersQuery(userId));
            return Ok(new { success = true, data = orders });
        }

        // POST: api/order/{id}/request-refund (Command)
        [HttpPost("{id}/request-refund")]
        public async Task<IActionResult> RequestRefund(long id, [FromBody] RefundRequestDto request)
        {
            var result = await _dispatcher.SendAsync(new RequestRefundCommand(id, request?.Reason));
            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        // POST: api/order/{id}/rate (Command)
        [HttpPost("{id}/rate")]
        public async Task<IActionResult> RateOrder(long id, [FromBody] OrderRatingDto request)
        {
            var result = await _dispatcher.SendAsync(new RateOrderCommand(id, request.Rating, request.Review));
            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }
    }
}
