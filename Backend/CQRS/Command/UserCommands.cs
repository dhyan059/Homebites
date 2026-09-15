using Homebites.CQRS.Common;

namespace Homebites.CQRS.Command
{
    public record ToggleUserStatusCommand(int UserId) : ICommand<ToggleUserStatusResult>;
    public record ToggleUserStatusResult(bool Success, bool IsActive);
}
