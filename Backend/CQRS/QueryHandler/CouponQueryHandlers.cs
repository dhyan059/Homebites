using Homebites.CQRS.Common;
using Homebites.CQRS.Query;
using Homebites.Data;
using Homebites.Models;
using Microsoft.EntityFrameworkCore;

namespace Homebites.CQRS.QueryHandler
{
    public class GetAllCouponsQueryHandler : IQueryHandler<GetAllCouponsQuery, List<Coupon>>
    {
        private readonly HomebitesDbContext _context;
        public GetAllCouponsQueryHandler(HomebitesDbContext context) { _context = context; }

        public async Task<List<Coupon>> HandleAsync(GetAllCouponsQuery query, CancellationToken ct = default)
        {
            return await _context.Coupons.OrderByDescending(c => c.CreatedAt).ToListAsync(ct);
        }
    }
}
