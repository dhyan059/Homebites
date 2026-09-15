using Homebites.CQRS.Common;

namespace Homebites.CQRS.Command
{
    public record CreateCouponCommand(string Code, string DiscountType, decimal DiscountValue, decimal MinOrderAmount, decimal? MaxDiscount, string? CreatedBy) : ICommand<CommandResult>;
    public record ToggleCouponCommand(int CouponId, string? UpdatedBy) : ICommand<CommandResult>;
    public record DeleteCouponCommand(int CouponId) : ICommand<CommandResult>;
}
