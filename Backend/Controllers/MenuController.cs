using Microsoft.AspNetCore.Mvc;
using Homebites.CQRS.Common;
using Homebites.CQRS.Command;
using Homebites.CQRS.Query;
using Homebites.DTOs;

namespace Homebites.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : ControllerBase
    {
        private readonly IDispatcher _dispatcher;

        public MenuController(IDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        // GET: api/menu (Query)
        [HttpGet]
        public async Task<IActionResult> GetMenu([FromQuery] int? categoryId, [FromQuery] bool? vegOnly)
        {
            var meals = await _dispatcher.QueryAsync(new GetMenuQuery(categoryId, vegOnly));
            return Ok(new { success = true, TotalMeals = meals.Count, count = meals.Count, data = meals });
        }

        // GET: api/menu/all (Query)
        [HttpGet("all")]
        public async Task<IActionResult> GetAllMealsForAdmin()
        {
            var meals = await _dispatcher.QueryAsync(new GetAllMealsAdminQuery());
            return Ok(new { success = true, count = meals.Count, data = meals });
        }

        // PUT: api/menu/{id}/toggle (Command)
        [HttpPut("{id}/toggle")]
        public async Task<IActionResult> ToggleAvailability(int id)
        {
            var result = await _dispatcher.SendAsync(new ToggleMealAvailabilityCommand(id));
            if (!result.Success)
                return NotFound(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        // DELETE: api/menu/{id} (Command)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMeal(int id)
        {
            var result = await _dispatcher.SendAsync(new DeleteMealCommand(id));
            if (!result.Success)
                return NotFound(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }
    }
}
