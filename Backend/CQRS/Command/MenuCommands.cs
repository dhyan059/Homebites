using Homebites.CQRS.Common;

namespace Homebites.CQRS.Command
{
    public record ToggleMealAvailabilityCommand(int MealId) : ICommand<CommandResult>;
    public record DeleteMealCommand(int MealId) : ICommand<CommandResult>;
}
