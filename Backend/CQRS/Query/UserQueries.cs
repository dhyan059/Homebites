using Homebites.CQRS.Common;
using Homebites.DTOs;

namespace Homebites.CQRS.Query
{
    public record GetAllUsersQuery() : IQuery<List<UserDto>>;
}
