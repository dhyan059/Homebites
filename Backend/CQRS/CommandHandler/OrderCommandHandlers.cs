using Homebites.CQRS.Command;
using Homebites.CQRS.Common;
using Homebites.Data;
using Homebites.Models;
using Microsoft.EntityFrameworkCore;

namespace Homebites.CQRS.CommandHandler
{
    public class PlaceOrderCommandHandler : ICommandHandler<PlaceOrderCommand, PlaceOrderResult>
    {
        private readonly HomebitesDbContext _context;
        public PlaceOrderCommandHandler(HomebitesDbContext context) { _context = context; }

        public async Task<PlaceOrderResult> HandleAsync(PlaceOrderCommand command, CancellationToken cancellationToken = default)
        {
            var request = command.Request;
            if (request.Items == null || !request.Items.Any())
                return new PlaceOrderResult(false, "Cart is empty. Please add items to order.");

            var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
            if (user == null)
                return new PlaceOrderResult(false, "User not found.");

            var address = await _context.UserAddresses.FindAsync(new object[] { request.AddressId }, cancellationToken);
            if (address == null || address.UserId != request.UserId)
                return new PlaceOrderResult(false, "Invalid delivery address selected.");

            var mealIds = request.Items.Select(i => i.MealId).ToList();
            var dbMeals = await _context.Meals.Where(m => mealIds.Contains(m.Id)).ToListAsync(cancellationToken);

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
            decimal taxAmount = Math.Round(subTotal * 0.05m, 2);
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
                CreatedAt = DateTime.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(cancellationToken);

            foreach (var oi in orderItems)
            {
                oi.OrderId = order.Id;
                _context.OrderItems.Add(oi);
            }
            await _context.SaveChangesAsync(cancellationToken);

            return new PlaceOrderResult(true, "Order placed successfully!", order.Id, order.OrderNumber, order.TotalAmount, 35);
        }
    }

    public class UpdateOrderStatusCommandHandler : ICommandHandler<UpdateOrderStatusCommand, CommandResult>
    {
        private readonly HomebitesDbContext _context;
        public UpdateOrderStatusCommandHandler(HomebitesDbContext context) { _context = context; }

        public async Task<CommandResult> HandleAsync(UpdateOrderStatusCommand command, CancellationToken cancellationToken = default)
        {
            var order = await _context.Orders.FindAsync(new object[] { command.OrderId }, cancellationToken);
            if (order == null) return new CommandResult(false, "Order not found.");

            var validStatuses = new[] { "Accepted", "Preparing", "Ready", "PickedUp", "Delivered", "Cancelled", "Refund Requested", "Refunded" };
            if (!validStatuses.Contains(command.Status))
                return new CommandResult(false, "Invalid status value.");

            if (command.Status == "Delivered" && order.PaymentMethod == "CashOnDelivery" && order.PaymentStatus != "Paid")
                return new CommandResult(false, "Collect the full COD amount before marking this order as delivered.");

            order.OrderStatus = command.Status;
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            return new CommandResult(true, $"Order {order.OrderNumber} updated to {command.Status}.");
        }
    }

    public class UpdateOrderPaymentCommandHandler : ICommandHandler<UpdateOrderPaymentCommand, CommandResult>
    {
        private readonly HomebitesDbContext _context;
        public UpdateOrderPaymentCommandHandler(HomebitesDbContext context) { _context = context; }

        public async Task<CommandResult> HandleAsync(UpdateOrderPaymentCommand command, CancellationToken cancellationToken = default)
        {
            var order = await _context.Orders.FindAsync(new object[] { command.OrderId }, cancellationToken);
            if (order == null) return new CommandResult(false, "Order not found.");
            if (order.PaymentMethod != "CashOnDelivery")
                return new CommandResult(false, "Only COD orders can be marked as cash collected.");
            if (command.PaymentStatus != "Paid")
                return new CommandResult(false, "Invalid COD payment status.");
            if (order.PaymentStatus == "Paid")
                return new CommandResult(false, "COD payment is already marked as collected.");

            order.PaymentStatus = "Paid";
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return new CommandResult(true, $"COD payment of ₹{order.TotalAmount:0.00} collected for order {order.OrderNumber}.");
        }
    }

    public class RequestRefundCommandHandler : ICommandHandler<RequestRefundCommand, CommandResult>
    {
        private readonly HomebitesDbContext _context;
        public RequestRefundCommandHandler(HomebitesDbContext context) { _context = context; }

        public async Task<CommandResult> HandleAsync(RequestRefundCommand command, CancellationToken cancellationToken = default)
        {
            var order = await _context.Orders.FindAsync(new object[] { command.OrderId }, cancellationToken);
            if (order == null) return new CommandResult(false, "Order not found.");

            if (order.OrderStatus == "Refunded")
                return new CommandResult(false, "This order has already been refunded.");

            if (order.OrderStatus == "Refund Requested")
                return new CommandResult(false, "A refund request is already under review.");

            order.OrderStatus = "Refund Requested";
            order.CancellationReason = string.IsNullOrWhiteSpace(command.Reason)
                ? "Customer requested refund"
                : command.Reason.Trim();
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            return new CommandResult(true, "Refund request submitted. Admin will review shortly.");
        }
    }

    public class ApproveRefundCommandHandler : ICommandHandler<ApproveRefundCommand, CommandResult>
    {
        private readonly HomebitesDbContext _context;
        public ApproveRefundCommandHandler(HomebitesDbContext context) { _context = context; }

        public async Task<CommandResult> HandleAsync(ApproveRefundCommand command, CancellationToken cancellationToken = default)
        {
            var order = await _context.Orders.FindAsync(new object[] { command.OrderId }, cancellationToken);
            if (order == null) return new CommandResult(false, "Order not found.");

            order.OrderStatus = "Refunded";
            order.PaymentStatus = "Refunded";
            order.CancellationReason = command.Reason ?? "Refund approved by admin";
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            return new CommandResult(true, $"Refund approved for order {order.OrderNumber}.");
        }
    }

    public class RejectRefundCommandHandler : ICommandHandler<RejectRefundCommand, CommandResult>
    {
        private readonly HomebitesDbContext _context;
        public RejectRefundCommandHandler(HomebitesDbContext context) { _context = context; }

        public async Task<CommandResult> HandleAsync(RejectRefundCommand command, CancellationToken cancellationToken = default)
        {
            var order = await _context.Orders.FindAsync(new object[] { command.OrderId }, cancellationToken);
            if (order == null) return new CommandResult(false, "Order not found.");

            order.OrderStatus = "Delivered";
            order.CancellationReason = command.Reason ?? "Refund request declined by Admin";
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            return new CommandResult(true, $"Refund request declined for order {order.OrderNumber}.");
        }
    }

    public class RateOrderCommandHandler : ICommandHandler<RateOrderCommand, CommandResult>
    {
        private readonly HomebitesDbContext _context;
        public RateOrderCommandHandler(HomebitesDbContext context) { _context = context; }

        public async Task<CommandResult> HandleAsync(RateOrderCommand command, CancellationToken cancellationToken = default)
        {
            var order = await _context.Orders.FindAsync(new object[] { command.OrderId }, cancellationToken);
            if (order == null) return new CommandResult(false, "Order not found.");

            order.CustomerNotes = (order.CustomerNotes ?? "") + $" [Rating: {command.Rating}â˜… - {command.Review ?? "No review"}]";
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            return new CommandResult(true, "Thank you for rating your order!");
        }
    }
}
