using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Homebites.Data;
using Homebites.Models;

namespace Homebites.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : ControllerBase
    {
        private readonly HomebitesDbContext _context;

        public MenuController(HomebitesDbContext context)
        {
            _context = context;
        }

        // GET: api/menu
        [HttpGet]
        public async Task<IActionResult> GetMenu()
        {
            var meals = await _context.Meals
                .Where(m => m.IsAvailable)
                .OrderBy(m => m.CategoryId)
                .ThenBy(m => m.MealName)
                .Select(m => new
                {
                    m.Id,
                    m.CategoryId,
                    m.MealName,
                    m.Description,
                    m.Price,
                    m.DiscountPrice,
                    m.ImageUrl,
                    m.IsVeg,
                    m.PreparationMinutes
                })
                .ToListAsync();

            return Ok(new { success = true, TotalMeals = meals.Count, count = meals.Count, data = meals });
        }

        // GET: api/menu/all (Includes inactive for admin)
        [HttpGet("all")]
        public async Task<IActionResult> GetAllMealsForAdmin()
        {
            var meals = await _context.Meals
                .OrderBy(m => m.CategoryId)
                .ThenBy(m => m.MealName)
                .Select(m => new
                {
                    m.Id,
                    m.CategoryId,
                    m.MealName,
                    m.Description,
                    m.Price,
                    m.DiscountPrice,
                    m.ImageUrl,
                    m.IsVeg,
                    m.IsAvailable,
                    m.PreparationMinutes
                })
                .ToListAsync();

            return Ok(new { success = true, count = meals.Count, data = meals });
        }

        // PUT: api/menu/{id}/toggle
        [HttpPut("{id}/toggle")]
        public async Task<IActionResult> ToggleAvailability(int id)
        {
            var meal = await _context.Meals.FindAsync(id);
            if (meal == null) return NotFound(new { success = false, message = "Meal not found." });

            meal.IsAvailable = !meal.IsAvailable;
            meal.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, isAvailable = meal.IsAvailable, message = $"Item '{meal.MealName}' is now {(meal.IsAvailable ? "Available" : "Unavailable")}." });
        }

        // DELETE: api/menu/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMeal(int id)
        {
            var meal = await _context.Meals.FindAsync(id);
            if (meal == null) return NotFound(new { success = false, message = "Meal not found." });

            // Check if order items refer to this meal; if so, soft delete
            bool hasOrders = await _context.OrderItems.AnyAsync(oi => oi.MealId == id);
            if (hasOrders)
            {
                meal.IsAvailable = false;
                meal.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = $"Meal '{meal.MealName}' has existing order history, so it was set to Unavailable." });
            }

            _context.Meals.Remove(meal);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = $"Meal '{meal.MealName}' removed completely from menu." });
        }
    }
}
