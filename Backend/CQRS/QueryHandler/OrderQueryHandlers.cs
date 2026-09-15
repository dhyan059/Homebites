using Homebites.CQRS.Common;
using Homebites.CQRS.Query;
using Homebites.Data;
using Homebites.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Homebites.CQRS.QueryHandler
{
    public class GetUserOrdersQueryHandler : IQueryHandler<GetUserOrdersQuery, List<OrderHistoryDto>>
    {
        private readonly HomebitesDbContext _context;
        public GetUserOrdersQueryHandler(HomebitesDbContext context) { _context = context; }

        public async Task<List<OrderHistoryDto>> HandleAsync(GetUserOrdersQuery query, CancellationToken ct = default)
        {
            var orders = await _context.Orders
                .Where(o => o.UserId == query.UserId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync(ct);

            var orderIds = orders.Select(o => o.Id).ToList();
            var orderItems = await _context.OrderItems
                .Where(oi => orderIds.Contains(oi.OrderId))
                .ToListAsync(ct);

            return orders.Select(o => new OrderHistoryDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                UpdatedAt = o.UpdatedAt,
                OrderStatus = o.OrderStatus,
                PaymentStatus = o.PaymentStatus,
                PaymentMethod = o.PaymentMethod,
                TotalAmount = o.TotalAmount,
                CustomerNotes = o.CustomerNotes,
                CancellationReason = o.CancellationReason,
                Items = orderItems
                    .Where(oi => oi.OrderId == o.Id)
                    .Select(oi => new OrderItemHistoryDto
                    {
                        MealName = oi.MealName,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        ItemTotal = oi.ItemTotal,
                        IsVeg = oi.IsVeg
                    }).ToList()
            }).ToList();
        }
    }

    public class GetAllOrdersAdminQueryHandler : IQueryHandler<GetAllOrdersAdminQuery, List<AdminOrderDto>>
    {
        private readonly HomebitesDbContext _context;
        public GetAllOrdersAdminQueryHandler(HomebitesDbContext context) { _context = context; }

        public async Task<List<AdminOrderDto>> HandleAsync(GetAllOrdersAdminQuery query, CancellationToken ct = default)
        {
            var q = _context.Orders.AsQueryable();
            if (!string.IsNullOrWhiteSpace(query.Status) && query.Status != "all")
                q = q.Where(o => o.OrderStatus == query.Status);

            var orders = await q.OrderByDescending(o => o.OrderDate).ToListAsync(ct);
            var userIds = orders.Select(o => o.UserId).Distinct().ToList();
            var users = await _context.Users.Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, ct);
            var orderIds = orders.Select(o => o.Id).ToList();
            var items = await _context.OrderItems.Where(i => orderIds.Contains(i.OrderId)).ToListAsync(ct);

            return orders.Select(o =>
            {
                users.TryGetValue(o.UserId, out var u);
                return new AdminOrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    UserId = o.UserId,
                    UserName = u?.FullName ?? "Unknown",
                    UserEmail = u?.Email ?? "",
                    UserMobile = u?.Mobile ?? "",
                    TotalAmount = o.TotalAmount,
                    OrderStatus = o.OrderStatus,
                    PaymentStatus = o.PaymentStatus,
                    PaymentMethod = o.PaymentMethod,
                    CancellationReason = o.CancellationReason,
                    OrderDate = o.OrderDate,
                    Items = items.Where(i => i.OrderId == o.Id).Select(i => new OrderItemDto
                    {
                        MealName = i.MealName,
                        Quantity = i.Quantity,
                        ItemTotal = i.ItemTotal
                    }).ToList()
                };
            }).ToList();
        }
    }

    public class GetAdminStatsQueryHandler : IQueryHandler<GetAdminStatsQuery, AdminStatsDto>
    {
        private readonly HomebitesDbContext _context;
        public GetAdminStatsQueryHandler(HomebitesDbContext context) { _context = context; }

        public async Task<AdminStatsDto> HandleAsync(GetAdminStatsQuery query, CancellationToken ct = default)
        {
            var totalUsers = await _context.Users.CountAsync(u => u.Role == "Customer", ct);
            var totalOrders = await _context.Orders.CountAsync(ct);
            var totalRevenue = await _context.Orders.SumAsync(o => (decimal?)o.TotalAmount, ct) ?? 0m;
            var pendingOrders = await _context.Orders.CountAsync(o => o.OrderStatus == "Accepted" || o.OrderStatus == "Preparing", ct);

            return new AdminStatsDto
            {
                TotalUsers = totalUsers,
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                PendingOrders = pendingOrders
            };
        }
    }
}
