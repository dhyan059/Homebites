using Homebites.CQRS.Command;
using Homebites.CQRS.Common;
using Homebites.Data;

namespace Homebites.CQRS.CommandHandler
{
    public class ToggleUserStatusCommandHandler : ICommandHandler<ToggleUserStatusCommand, ToggleUserStatusResult>
    {
        private readonly HomebitesDbContext _context;
        public ToggleUserStatusCommandHandler(HomebitesDbContext context) { _context = context; }

        public async Task<ToggleUserStatusResult> HandleAsync(ToggleUserStatusCommand cmd, CancellationToken ct = default)
        {
            var user = await _context.Users.FindAsync(new object[] { cmd.UserId }, ct);
            if (user == null) return new ToggleUserStatusResult(false, false);

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            return new ToggleUserStatusResult(true, user.IsActive);
        }
    }
}
