using Homebites.CQRS.Common;
using Homebites.DTOs;
using Homebites.Models;

namespace Homebites.CQRS.Query
{
    public record GetMenuQuery(int? CategoryId = null, bool? VegOnly = null) : IQuery<List<MealDto>>;
    public record GetAllMealsAdminQuery() : IQuery<List<Meal>>;
}
