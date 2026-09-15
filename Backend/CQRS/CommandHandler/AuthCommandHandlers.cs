using System.Security.Cryptography;
using System.Text;
using Homebites.CQRS.Command;
using Homebites.CQRS.Common;
using Homebites.Data;

namespace Homebites.CQRS.CommandHandler
{
    public class UpdateProfileCommandHandler : ICommandHandler<UpdateProfileCommand, CommandResult>
    {
        private readonly HomebitesDbContext _context;
        public UpdateProfileCommandHandler(HomebitesDbContext context) { _context = context; }

        public async Task<CommandResult> HandleAsync(UpdateProfileCommand cmd, CancellationToken ct = default)
        {
            var user = await _context.Users.FindAsync(new object[] { cmd.UserId }, ct);
            if (user == null) return new CommandResult(false, "User not found.");

            if (!string.IsNullOrWhiteSpace(cmd.FullName))
                user.FullName = cmd.FullName.Trim();

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
            return new CommandResult(true, "Profile updated successfully.");
        }
    }

    public class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand, CommandResult>
    {
        private readonly HomebitesDbContext _context;
        public ChangePasswordCommandHandler(HomebitesDbContext context) { _context = context; }

        public async Task<CommandResult> HandleAsync(ChangePasswordCommand cmd, CancellationToken ct = default)
        {
            var user = await _context.Users.FindAsync(new object[] { cmd.UserId }, ct);
            if (user == null) return new CommandResult(false, "User not found.");

            string currentHash = HashPassword(cmd.CurrentPassword);
            if (user.PasswordHash != currentHash)
                return new CommandResult(false, "Current password is incorrect.");

            user.PasswordHash = HashPassword(cmd.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
            return new CommandResult(true, "Password changed successfully.");
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "_homebites_salt"));
            return Convert.ToBase64String(bytes);
        }
    }
}
