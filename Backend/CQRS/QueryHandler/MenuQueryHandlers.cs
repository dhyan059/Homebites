using Homebites.CQRS.Common;
using Homebites.CQRS.Query;
using Homebites.Data;
using Homebites.DTOs;
using Homebites.Models;
using Microsoft.EntityFrameworkCore;

namespace Homebites.CQRS.QueryHandler
{
    public class GetMenuQueryHandler : IQueryHandler<GetMenuQuery, List<MealDto>>
    {
        private readonly HomebitesDbContext _context;
        public GetMenuQueryHandler(HomebitesDbContext context) { _context = context; }

        public async Task<List<MealDto>> HandleAsync(GetMenuQuery query, CancellationToken ct = default)
        {
            var q = _context.Meals.Where(m => m.IsAvailable);

            if (query.CategoryId.HasValue && query.CategoryId.Value > 0)
                q = q.Where(m => m.CategoryId == query.CategoryId.Value);

            if (query.VegOnly.HasValue && query.VegOnly.Value)
                q = q.Where(m => m.IsVeg);

            return await q
                .OrderBy(m => m.CategoryId)
                .ThenBy(m => m.MealName)
                .Select(m => new MealDto
                {
                    Id = m.Id,
                    CategoryId = m.CategoryId,
                    MealName = m.MealName,
                    Description = m.Description,
                    Price = m.Price,
                    DiscountPrice = m.DiscountPrice,
                    ImageUrl = m.ImageUrl,
                    IsVeg = m.IsVeg,
                    PreparationMinutes = m.PreparationMinutes
                })
                .ToListAsync(ct);
        }
    }

    public class GetAllMealsAdminQueryHandler : IQueryHandler<GetAllMealsAdminQuery, List<Meal>>
    {
        private readonly HomebitesDbContext _context;
        public GetAllMealsAdminQueryHandler(HomebitesDbContext context) { _context = context; }

        public async Task<List<Meal>> HandleAsync(GetAllMealsAdminQuery query, CancellationToken ct = default)
        {
            return await _context.Meals.OrderBy(m => m.MealName).ToListAsync(ct);
        }
    }
}
