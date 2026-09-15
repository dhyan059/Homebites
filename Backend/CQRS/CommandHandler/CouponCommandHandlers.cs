using Homebites.CQRS.Command;
using Homebites.CQRS.Common;
using Homebites.Data;
using Homebites.Models;
using Microsoft.EntityFrameworkCore;

namespace Homebites.CQRS.CommandHandler
{
    public class CreateCouponCommandHandler : ICommandHandler<CreateCouponCommand, CommandResult>
    {
        private readonly HomebitesDbContext _context;
        public CreateCouponCommandHandler(HomebitesDbContext context) { _context = context; }

        public async Task<CommandResult> HandleAsync(CreateCouponCommand cmd, CancellationToken ct = default)
        {
            var code = cmd.Code.Trim().ToUpper();
            if (await _context.Coupons.AnyAsync(c => c.Code == code, ct))
                return new CommandResult(false, $"Coupon code '{code}' already exists.");

            _context.Coupons.Add(new Coupon
            {
                Code = code,
                DiscountType = cmd.DiscountType == "Flat" ? "Flat" : "Percent",
                DiscountValue = cmd.DiscountValue,
                MinOrderAmount = cmd.MinOrderAmount,
                MaxDiscount = cmd.MaxDiscount,
                IsActive = true,
                CreatedBy = cmd.CreatedBy ?? "Admin",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync(ct);
            return new CommandResult(true, $"Coupon '{code}' created successfully.");
        }
    }

    public class ToggleCouponCommandHandler : ICommandHandler<ToggleCouponCommand, CommandResult>
    {
        private readonly HomebitesDbContext _context;
        public ToggleCouponCommandHandler(HomebitesDbContext context) { _context = context; }

        public async Task<CommandResult> HandleAsync(ToggleCouponCommand cmd, CancellationToken ct = default)
        {
            var coupon = await _context.Coupons.FindAsync(new object[] { cmd.CouponId }, ct);
            if (coupon == null) return new CommandResult(false, "Coupon not found.");

            coupon.IsActive = !coupon.IsActive;
            coupon.UpdatedBy = cmd.UpdatedBy ?? "Admin";
            coupon.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            return new CommandResult(true, $"Coupon '{coupon.Code}' is now {(coupon.IsActive ? "Active" : "Disabled")}.");
        }
    }

    public class DeleteCouponCommandHandler : ICommandHandler<DeleteCouponCommand, CommandResult>
    {
        private readonly HomebitesDbContext _context;
        public DeleteCouponCommandHandler(HomebitesDbContext context) { _context = context; }

        public async Task<CommandResult> HandleAsync(DeleteCouponCommand cmd, CancellationToken ct = default)
        {
            var coupon = await _context.Coupons.FindAsync(new object[] { cmd.CouponId }, ct);
            if (coupon == null) return new CommandResult(false, "Coupon not found.");

            _context.Coupons.Remove(coupon);
            await _context.SaveChangesAsync(ct);
            return new CommandResult(true, $"Coupon '{coupon.Code}' deleted.");
        }
    }
}
