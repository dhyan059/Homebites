using Homebites.CQRS.Common;

namespace Homebites.CQRS.Command
{
    public record UpdateProfileCommand(int UserId, string FullName) : ICommand<CommandResult>;
    public record ChangePasswordCommand(int UserId, string CurrentPassword, string NewPassword) : ICommand<CommandResult>;
}
