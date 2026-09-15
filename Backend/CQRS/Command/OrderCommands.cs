using Homebites.CQRS.Common;
using Homebites.DTOs;

namespace Homebites.CQRS.Command
{
    public record PlaceOrderCommand(OrderRequest Request) : ICommand<PlaceOrderResult>;
    public record PlaceOrderResult(bool Success, string Message, long OrderId = 0, string OrderNumber = "", decimal TotalAmount = 0, int EstimatedDeliveryMinutes = 35);

    public record UpdateOrderStatusCommand(long OrderId, string Status) : ICommand<CommandResult>;
    public record UpdateOrderPaymentCommand(long OrderId, string PaymentStatus) : ICommand<CommandResult>;
    public record RequestRefundCommand(long OrderId, string? Reason) : ICommand<CommandResult>;
    public record ApproveRefundCommand(long OrderId, string? Reason) : ICommand<CommandResult>;
    public record RejectRefundCommand(long OrderId, string? Reason) : ICommand<CommandResult>;
    public record RateOrderCommand(long OrderId, int Rating, string? Review) : ICommand<CommandResult>;
}
