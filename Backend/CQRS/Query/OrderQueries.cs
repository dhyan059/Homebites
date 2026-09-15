using Homebites.CQRS.Common;
using Homebites.DTOs;

namespace Homebites.CQRS.Query
{
    public record GetUserOrdersQuery(int UserId) : IQuery<List<OrderHistoryDto>>;
    public record GetAllOrdersAdminQuery(string? Status = null) : IQuery<List<AdminOrderDto>>;
    public record GetAdminStatsQuery() : IQuery<AdminStatsDto>;
}
