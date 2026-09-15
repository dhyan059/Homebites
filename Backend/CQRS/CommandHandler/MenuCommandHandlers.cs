using Homebites.CQRS.Command;
using Homebites.CQRS.Common;
using Homebites.Data;

namespace Homebites.CQRS.CommandHandler
{
    public class ToggleMealAvailabilityCommandHandler : ICommandHandler<ToggleMealAvailabilityCommand, CommandResult>
    {
        private readonly HomebitesDbContext _context;
        public ToggleMealAvailabilityCommandHandler(HomebitesDbContext context) { _context = context; }

        public async Task<CommandResult> HandleAsync(ToggleMealAvailabilityCommand cmd, CancellationToken ct = default)
        {
            var meal = await _context.Meals.FindAsync(new object[] { cmd.MealId }, ct);
            if (meal == null) return new CommandResult(false, "Meal not found.");

            meal.IsAvailable = !meal.IsAvailable;
            meal.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            return new CommandResult(true, $"'{meal.MealName}' availability set to {meal.IsAvailable}.");
        }
    }

    public class DeleteMealCommandHandler : ICommandHandler<DeleteMealCommand, CommandResult>
    {
        private readonly HomebitesDbContext _context;
        public DeleteMealCommandHandler(HomebitesDbContext context) { _context = context; }

        public async Task<CommandResult> HandleAsync(DeleteMealCommand cmd, CancellationToken ct = default)
        {
            var meal = await _context.Meals.FindAsync(new object[] { cmd.MealId }, ct);
            if (meal == null) return new CommandResult(false, "Meal not found.");

            _context.Meals.Remove(meal);
            await _context.SaveChangesAsync(ct);

            return new CommandResult(true, $"'{meal.MealName}' deleted successfully.");
        }
    }
}
