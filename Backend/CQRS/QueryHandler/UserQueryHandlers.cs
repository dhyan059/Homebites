using Homebites.CQRS.Common;
using Homebites.CQRS.Query;
using Homebites.Data;
using Homebites.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Homebites.CQRS.QueryHandler
{
    public class GetAllUsersQueryHandler : IQueryHandler<GetAllUsersQuery, List<UserDto>>
    {
        private readonly HomebitesDbContext _context;
        public GetAllUsersQueryHandler(HomebitesDbContext context) { _context = context; }

        public async Task<List<UserDto>> HandleAsync(GetAllUsersQuery query, CancellationToken ct = default)
        {
            return await _context.Users
                .Where(u => u.Role == "Customer")
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Mobile = u.Mobile,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync(ct);
        }
    }
}
