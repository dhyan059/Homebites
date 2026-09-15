using Homebites.CQRS.Common;
using Homebites.Models;

namespace Homebites.CQRS.Query
{
    public record GetAllCouponsQuery() : IQuery<List<Coupon>>;
}
